using System.Diagnostics;
using System.IO;

namespace Altinn.Authorization.Testing;

/// <summary>
/// Lightweight, opt-out timing for the integration-test setup path. Added to
/// size where test wall-clock actually goes — per-fixture app-host build vs
/// per-test database provisioning vs the one-time template build — before the
/// fixture-sharing refactor in <c>#3379</c>. Linked into the same assemblies as
/// <see cref="PostgresTestEngine"/>.
/// </summary>
/// <remarks>
/// <para>
/// Recording is a few atomic adds around a <see cref="Stopwatch"/> timestamp, so
/// it is safe to leave on. At process exit one summary line is written to stdout
/// — captured in the CI test-lane log — of the form:
/// </para>
/// <code>
/// ===FIXTURE_TIMING=== host_build_ms=12345 host_build_n=85 db_provision_ms=6789 db_provision_n=85 clone_ms=4200 clone_n=85 template_build_ms=900
/// </code>
/// <para>
/// Disable entirely by setting the <c>FIXTURE_TIMING=off</c> environment
/// variable (then every <see cref="Time{T}(Phase, Func{T})"/> call is a pass-through).
/// </para>
/// </remarks>
internal static class FixtureTiming
{
    private static readonly bool Enabled =
        !string.Equals(Environment.GetEnvironmentVariable("FIXTURE_TIMING"), "off", StringComparison.OrdinalIgnoreCase);

    private static long _hostBuildTicks;
    private static long _hostBuildCount;
    private static long _dbProvisionTicks;
    private static long _dbProvisionCount;
    private static long _cloneTicks;
    private static long _cloneCount;
    private static long _templateBuildTicks;
    private static int _exitHooked;

    /// <summary>Setup phase being measured.</summary>
    internal enum Phase
    {
        /// <summary>Per-fixture <c>WebApplicationFactory</c> host build (DI graph, EF model, middleware).</summary>
        HostBuild,

        /// <summary>Per-fixture database provisioning (factory call, includes the clone).</summary>
        DbProvision,

        /// <summary>Per-test <c>CREATE DATABASE ... WITH TEMPLATE</c> clone inside the engine.</summary>
        Clone,

        /// <summary>One-time container start + role bootstrap + migrate/seed of the template.</summary>
        TemplateBuild,
    }

    /// <summary>Times an async <paramref name="action"/> and attributes it to <paramref name="phase"/>.</summary>
    public static async Task<T> TimeAsync<T>(Phase phase, Func<Task<T>> action)
    {
        if (!Enabled)
        {
            return await action();
        }

        EnsureExitHook();
        var start = Stopwatch.GetTimestamp();
        try
        {
            return await action();
        }
        finally
        {
            Record(phase, Stopwatch.GetElapsedTime(start));
        }
    }

    /// <summary>Times an async <paramref name="action"/> (no return value) and attributes it to <paramref name="phase"/>.</summary>
    public static async Task TimeAsync(Phase phase, Func<Task> action)
    {
        await TimeAsync<object?>(phase, async () =>
        {
            await action();
            return null;
        });
    }

    /// <summary>Times a synchronous <paramref name="action"/> and attributes it to <paramref name="phase"/>.</summary>
    public static T Time<T>(Phase phase, Func<T> action)
    {
        if (!Enabled)
        {
            return action();
        }

        EnsureExitHook();
        var start = Stopwatch.GetTimestamp();
        try
        {
            return action();
        }
        finally
        {
            Record(phase, Stopwatch.GetElapsedTime(start));
        }
    }

    private static void Record(Phase phase, TimeSpan elapsed)
    {
        switch (phase)
        {
            case Phase.HostBuild:
                Interlocked.Add(ref _hostBuildTicks, elapsed.Ticks);
                Interlocked.Increment(ref _hostBuildCount);
                break;
            case Phase.DbProvision:
                Interlocked.Add(ref _dbProvisionTicks, elapsed.Ticks);
                Interlocked.Increment(ref _dbProvisionCount);
                break;
            case Phase.Clone:
                Interlocked.Add(ref _cloneTicks, elapsed.Ticks);
                Interlocked.Increment(ref _cloneCount);
                break;
            case Phase.TemplateBuild:
                Interlocked.Add(ref _templateBuildTicks, elapsed.Ticks);
                break;
        }
    }

    private static void EnsureExitHook()
    {
        if (Interlocked.Exchange(ref _exitHooked, 1) == 0)
        {
            AppDomain.CurrentDomain.ProcessExit += (_, _) => WriteSummary();
        }
    }

    private static void WriteSummary()
    {
        static long Ms(long ticks) => (long)TimeSpan.FromTicks(ticks).TotalMilliseconds;

        var line =
            $"===FIXTURE_TIMING=== " +
            $"host_build_ms={Ms(Interlocked.Read(ref _hostBuildTicks))} " +
            $"host_build_n={Interlocked.Read(ref _hostBuildCount)} " +
            $"db_provision_ms={Ms(Interlocked.Read(ref _dbProvisionTicks))} " +
            $"db_provision_n={Interlocked.Read(ref _dbProvisionCount)} " +
            $"clone_ms={Ms(Interlocked.Read(ref _cloneTicks))} " +
            $"clone_n={Interlocked.Read(ref _cloneCount)} " +
            $"template_build_ms={Ms(Interlocked.Read(ref _templateBuildTicks))}";

        Console.WriteLine(line);

        // MTP / dotnet-coverage can swallow the test host's process-exit stdout
        // (the summary never reached the CI log), so also append to a file when
        // FIXTURE_TIMING_FILE is set — read locally, uploaded as a CI artifact.
        var file = Environment.GetEnvironmentVariable("FIXTURE_TIMING_FILE");
        if (!string.IsNullOrEmpty(file))
        {
            try
            {
                File.AppendAllText(file, line + Environment.NewLine);
            }
            catch
            {
                // Best-effort diagnostic; never fail a test run over timing output.
            }
        }
    }
}
