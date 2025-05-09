using Azure.Messaging.ServiceBus.Administration;

namespace Altinn.Authorization.Cli.ServiceBus.MassTransit;

internal class QueueModel
{
    private readonly string _name;
    private readonly string _errorQueueName;

    private QueueRuntimeProperties? _queueRuntimeProperties;
    private QueueRuntimeProperties? _errorQueueRuntimeProperties;

    public QueueModel(string name)
    {
        _name = name;
        _errorQueueName = $"{name}_error";
    }

    public string Name => _name;

    public string ErrorQueueName => _errorQueueName;

    public long ErrorCount
        => Sum([
            _queueRuntimeProperties?.DeadLetterMessageCount ?? 0,
            _errorQueueRuntimeProperties?.ActiveMessageCount ?? 0,
            _errorQueueRuntimeProperties?.DeadLetterMessageCount ?? 0,
        ]);

    public QueueRuntimeProperties QueueRuntimeProperties
    {
        get => _queueRuntimeProperties;
        internal set => _queueRuntimeProperties = value;
    }

    public QueueRuntimeProperties ErrorQueueRuntimeProperties
    {
        get => _errorQueueRuntimeProperties;
        internal set => _errorQueueRuntimeProperties = value;
    }

    private long Sum(ReadOnlySpan<long> values)
    {
        long sum = 0;
        foreach (long? value in values)
        {
            sum += value.Value;
        }

        return sum;
    }
}
