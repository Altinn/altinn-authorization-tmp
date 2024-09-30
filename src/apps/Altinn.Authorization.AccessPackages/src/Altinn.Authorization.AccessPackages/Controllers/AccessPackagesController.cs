using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Services;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Authorization.AccessPackages.Controllers;

/// <summary>
/// Controller responsible for managing access packages within the Altinn authorization service.
/// </summary>
[ApiController]
[Route("[controller]/api/v1")]
public partial class AccessPackagesController : ControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AccessPackagesController"/> class.
    /// </summary>
    /// <param name="logger">An instance of <see cref="ILogger{AccessPackagesController}"/> for logging.</param>
    /// <param name="service">An instance of <see cref="IAccessPackageService"/> for accessing package services.</param>
    public AccessPackagesController(ILogger<AccessPackagesController> logger, IAccessPackageService service)
    {
        Logger = logger;
        Service = service;
    }

    private ILogger<AccessPackagesController> Logger { get; }

    private IAccessPackageService Service { get; }

    /// <summary>
    /// Retrieves metadata for access packages.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation, containing an <see cref="IActionResult"/> with the metadata.</returns>
    [HttpGet("metadata")]
    [ProducesResponseType<IEnumerable<AccessPackageMetadataModel>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetadata(CancellationToken cancellationToken = default)
    {
        Log.ReadMetadata(Logger);
        return Ok(await Service.ListAccessPackagesMetadata(cancellationToken));
    }

    /// <summary>
    /// Provides structured logging methods for the <see cref="AccessPackagesController"/>.
    /// </summary>
    internal partial class Log
    {
        /// <summary>
        /// Logs an informational message when metadata for access packages is being retrieved.
        /// </summary>
        /// <param name="logger">The logger instance used to write the log message.</param>
        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Retrieving access packages.")]
        internal static partial void ReadMetadata(ILogger logger);
    }
}
