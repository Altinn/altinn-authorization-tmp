using System.Diagnostics;

namespace Altinn.AccessManagement.Core;

public record PipelineMessage<T>(
    T Data,
    ActivityContext ActivityContext)
{
}
