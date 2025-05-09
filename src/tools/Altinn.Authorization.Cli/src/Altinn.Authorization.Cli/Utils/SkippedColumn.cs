using Spectre.Console;
using Spectre.Console.Rendering;

namespace Altinn.Authorization.Cli.Utils;

internal sealed class SkippedColumn
    : ProgressColumn
{
    public override IRenderable Render(RenderOptions options, ProgressTask task, TimeSpan deltaTime)
    {
        var skipped = task.State.Get<double>("task.skipped");
        var total = task.Value;
        var notSkipped = total - skipped;

        return Markup.FromInterpolated($"[green]{notSkipped:F0}[/]/[yellow]{skipped:F0}[/]/[blue]{task.MaxValue:F0}[/]");
    }
}
