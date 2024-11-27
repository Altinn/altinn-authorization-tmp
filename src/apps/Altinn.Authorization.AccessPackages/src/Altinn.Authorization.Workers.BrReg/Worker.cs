using Altinn.Authorization.Workers.BrReg.Services;

namespace Altinn.Authorization.Workers.BrReg;

/// <summary>
/// BrReg - Worker
/// </summary>
public class Worker : BackgroundService
{
    private readonly Importer importer;
    private readonly Ingestor ingestor;
    private readonly ILogger<Worker> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Worker"/> class.
    /// </summary>
    /// <param name="importer">Importer</param>
    /// <param name="ingestor">Ingestor</param>
    /// <param name="logger">ILogger</param>
    public Worker(Importer importer, Ingestor ingestor, ILogger<Worker> logger)
    {
        this.importer = importer;
        this.ingestor = ingestor;
        _logger = logger;
    }

    /// <summary>
    /// ExecuteAsync
    /// </summary>
    /// <param name="stoppingToken">CancellationToken</param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        /*INGEST*/
        Info("Starting ingest at: {time}", DateTimeOffset.Now);
        await ingestor.IngestAll();
        Info("Ingest completed at: {time}", DateTimeOffset.Now);
       
        while (!stoppingToken.IsCancellationRequested)
        {
            /*IMPORT*/
            Info("Worker awake at: {time}", DateTimeOffset.Now);

            Info("Starting unit import at: {time}", DateTimeOffset.Now);
            await importer.ImportUnit();
            Info("Starting subunit import at: {time}", DateTimeOffset.Now);
            await importer.ImportSubUnit();
            Info("Starting role import at: {time}", DateTimeOffset.Now);
            await importer.ImportRoles();

            importer.WriteChangeRefsToConsole();

            Info("Worker sleeping until: {time}", DateTimeOffset.Now.AddMilliseconds(10000));
            await Task.Delay(10000, stoppingToken);
        }
    }

    private void Info(string? message, params object?[] args)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(message, args);
        }
    }
}
