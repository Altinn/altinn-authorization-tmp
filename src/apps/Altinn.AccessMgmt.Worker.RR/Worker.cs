using Altinn.AccessMgmt.Worker.RR.Models;
using Altinn.AccessMgmt.Worker.RR.Services;
using Microsoft.Extensions.Options;

namespace Altinn.AccessMgmt.Worker.RR;

/// <summary>
/// Worker
/// </summary>
public class Worker(ILogger<Worker> logger, IOptions<ResourceRegisterImportConfig> options, Engine engine) : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;
    private readonly ResourceRegisterImportConfig config = options.Value;
    private readonly Engine engine = engine;

    /// <summary>
    /// Execute
    /// </summary>
    /// <param name="stoppingToken">CancellationToken</param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var wrapper = new ResourceRegisterWrapper();

        while (!stoppingToken.IsCancellationRequested && config.IsEnabled)
        {
            Info("Worker running at: {time}", DateTimeOffset.Now);

            var res = await wrapper.GetResources();
            await engine.ImportResource(res);

            Info("Work is done, sleeping: {time}", DateTimeOffset.Now);
            await Task.Delay(config.Interval, stoppingToken);
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
