﻿using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;

namespace Altinn.Authorization.Cli.Utils;

/// <summary>
/// General extensions for the CLI.
/// </summary>
[ExcludeFromCodeCoverage]
public static class CliExtensions
{
    /// <summary>
    /// Logs a message if the task fails.
    /// </summary>
    /// <typeparam name="T">The task type.</typeparam>
    /// <param name="task">The task.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A wrapped task.</returns>
    public static async Task<T> LogOnFailure<T>(this Task<T> task, string message)
    {
        try
        {
            return await task;
        }
        catch
        {
            AnsiConsole.MarkupLine(message);
            throw;
        }
    }

    /// <summary>
    /// Logs a message if the task fails.
    /// </summary>
    /// <typeparam name="T">The task type.</typeparam>
    /// <param name="task">The task.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A wrapped task.</returns>
    public static async Task<T> LogOnFailure<T>(this ValueTask<T> task, string message)
    {
        try
        {
            return await task;
        }
        catch
        {
            AnsiConsole.MarkupLine(message);
            throw;
        }
    }

    /// <summary>
    /// Logs a message if the task fails.
    /// </summary>
    /// <param name="task">The task.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A wrapped task.</returns>
    public static async Task LogOnFailure(this Task task, string message)
    {
        try
        {
            await task;
        }
        catch
        {
            AnsiConsole.MarkupLine(message);
            throw;
        }
    }

    /// <summary>
    /// Logs a message if the task fails.
    /// </summary>
    /// <param name="task">The task.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A wrapped task.</returns>
    public static async Task LogOnFailure(this ValueTask task, string message)
    {
        try
        {
            await task;
        }
        catch
        {
            AnsiConsole.MarkupLine(message);
            throw;
        }
    }

    /// <summary>
    /// Creates a line progress reporter from a <see cref="ProgressTask"/>.
    /// </summary>
    /// <param name="task">The task.</param>
    /// <returns>A line progress reporter.</returns>
    public static LineProgress AsLineProgress(this ProgressTask task)
        => new(task);

    /// <summary>
    /// A <see cref="IProgress{T}"/> wrapper for <see cref="ProgressTask"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly struct LineProgress(ProgressTask task)
        : IProgress<int>
    {
        /// <inheritdoc/>
        public readonly void Report(int value)
            => task.Increment(value);
    }

    /// <summary>
    /// Starts a <see cref="ProgressTask"/> and stops it when disposed.
    /// </summary>
    /// <param name="task">The task.</param>
    /// <param name="setValueMax">Whether to set the value to the max value when stopping the task.</param>
    /// <returns>A <see cref="IDisposable"/> that stops the <see cref="ProgressTask"/>.</returns>
    public static IDisposable Run(this ProgressTask task, bool setValueMax = false)
    {
        task.StartTask();

        return new Disposable(() =>
        {
            if (setValueMax)
            {
                task.Value = task.MaxValue;
            }

            task.StopTask();
        });
    }

    private sealed class Disposable(Action dispose)
        : IDisposable
    {
        private int _disposed = 0;

        void IDisposable.Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                dispose();
            }
        }
    }
}
