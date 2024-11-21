using Altinn.Authorization.AccessPackages.Repo.Data.Services;

namespace Altinn.Authorization.Importers.ResReg;

/// <summary>
/// Worker
/// </summary>
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly Engine engine;

    /// <summary>
    /// Initializes a new instance of the <see cref="Worker"/> class.
    /// </summary>
    /// <param name="logger">ILogger</param>
    /// <param name="engine">Engine</param>
    public Worker(ILogger<Worker> logger, Engine engine)
    {
        _logger = logger;
        this.engine = engine;
    }

    /// <summary>
    /// Execute
    /// </summary>
    /// <param name="stoppingToken">CancellationToken</param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var wrapper = new ResourceRegisterWrapper();

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            var res = await wrapper.GetResources();

            foreach (var resource in res.Where(t => t.Identifier == "se_4481_2"))
            {
                Console.WriteLine(resource.HasCompetentAuthority.Name);
                Console.WriteLine(resource.HasCompetentAuthority.Orgcode);
                Console.WriteLine(resource.HasCompetentAuthority.Organization);
            }

            await engine.ImportResource(res);

            await Task.Delay(100000, stoppingToken);
        }
    }
}
