using Altinn.AccessMgmt.FFB.Services;
using Microsoft.AspNetCore.Components;

namespace Altinn.AccessMgmt.FFB.Components.Shared;

/// <summary>
/// Base class for pages that load data per environment. Owns the page-blocking
/// <see cref="Loading"/> flag, the page-level <see cref="Error"/> message, and the
/// <see cref="EnvironmentState"/> subscription. Pages that override
/// <see cref="OnInitialized"/> must call <c>base.OnInitialized()</c>.
/// </summary>
public abstract class EnvironmentPageBase : ComponentBase, IDisposable
{
    [Inject]
    protected EnvironmentState EnvState { get; set; } = default!;

    /// <summary>The single page-blocking busy flag, managed by <see cref="RunAsync"/>.</summary>
    protected bool Loading { get; private set; }

    /// <summary>Page-level error message, set from <see cref="ErrorText.Flatten"/> by <see cref="RunAsync"/>.</summary>
    protected string? Error { get; private set; }

    protected override void OnInitialized()
    {
        EnvState.OnChange += HandleEnvironmentChanged;
    }

    public void Dispose()
    {
        EnvState.OnChange -= HandleEnvironmentChanged;
    }

    /// <summary>
    /// Runs page work with Loading/Error handling and re-renders before and after.
    /// </summary>
    protected async Task RunAsync(Func<Task> work)
    {
        Loading = true;
        Error = null;
        StateHasChanged();

        try
        {
            await work();
        }
        catch (Exception ex)
        {
            Error = ErrorText.Flatten(ex);
        }
        finally
        {
            Loading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Called when the selected environment changes. Details pages reload their data;
    /// tool pages clear stale results.
    /// </summary>
    protected virtual Task OnEnvironmentChangedAsync() => Task.CompletedTask;

    private void HandleEnvironmentChanged() => _ = InvokeAsync(async () =>
    {
        await OnEnvironmentChangedAsync();
        StateHasChanged();
    });
}
