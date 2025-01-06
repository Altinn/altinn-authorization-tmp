using Altinn.Authorization.Workers.BrReg.Services;

namespace Altinn.Authorization.Workers.BrReg;

/// <summary>
/// BrReg - Worker
/// </summary>
public class Worker(Importer importer, Ingestor ingestor, ILogger<Worker> logger) : BackgroundService
{
    private readonly Importer importer = importer;
    private readonly Ingestor ingestor = ingestor;
    private readonly ILogger<Worker> _logger = logger;

    /// <summary>
    /// ExecuteAsync
    /// </summary>
    /// <param name="stoppingToken">CancellationToken</param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        /*INGEST*/
        Info("Starting ingest at: {time}", DateTimeOffset.Now);
        await ingestor.IngestAll(force: false);
        Info("Ingest completed at: {time}", DateTimeOffset.Now);

        if (importer.IsEnabled) 
        { 
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

        Info("Shutting down at: {time}", DateTimeOffset.Now);
        return;
    }

    private void Info(string? message, params object?[] args)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(message, args);
        }
    }
}
