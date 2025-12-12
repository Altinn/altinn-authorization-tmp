using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Altinn.AccessMgmt.Core.HostedServices.Services;

public sealed class DatabaseMaintenanceService(IServiceProvider serviceProvider) : BackgroundService
{
    private readonly List<MaintenanceJob> jobs =
    [
        new MaintenanceJob
        {
            Name = "CleanUpEmptyAssignments",
            Interval = TimeSpan.FromHours(3),
            RunAsync = async (sp, ct) =>
            {
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await CleanUpEmptyAssignments(db, ct);
            }
        }
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTimeOffset.UtcNow;

            var nextJob = jobs.OrderBy(j => j.NextRunUtc).First();
            var delay = nextJob.NextRunUtc - now;

            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, stoppingToken);
            }

            now = DateTimeOffset.UtcNow;

            foreach (var job in jobs.Where(j => j.NextRunUtc <= now).ToList())
            {
                try
                {
                    await job.RunAsync(serviceProvider, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception)
                {
                    // logg/varsling ?
                }
                finally
                {
                    job.NextRunUtc = DateTimeOffset.UtcNow + job.Interval;
                }
            }
        }
    }

    private static async Task CleanUpEmptyAssignments(AppDbContext db, CancellationToken ct)
    {
        List<Guid> roles = [RoleConstants.Agent.Id];

        var orphanAssignments = db.Assignments
            .Where(a =>
                roles.Contains(a.RoleId) &&
                !db.AssignmentPackages.Any(ap => ap.AssignmentId == a.Id) &&
                !db.AssignmentResources.Any(ar => ar.AssignmentId == a.Id) &&
                !db.AssignmentInstances.Any(ai => ai.AssignmentId == a.Id) &&
                !db.Delegations.Any(d => d.FromId == a.Id || d.ToId == a.Id));

        db.Assignments.RemoveRange(orphanAssignments);
        await db.SaveChangesAsync(ct);
    }
}

internal sealed class MaintenanceJob
{
    public required string Name { get; init; }

    public required TimeSpan Interval { get; init; }
    
    public required Func<IServiceProvider, CancellationToken, Task> RunAsync { get; init; }

    public DateTimeOffset NextRunUtc { get; set; } = DateTimeOffset.UtcNow;
}
