using Altinn.AccessManagement.Core.Models.Connection;
using Altinn.AccessManagement.Core.Services.Connection.Interfaces;
using Altinn.AccessManagement.Persistence.Models.Connection;
using Altinn.AccessManagement.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessManagement.Persistence.Repositories.Connection;

/// <summary>
/// Repository for managing connection data access
/// </summary>
public class ConnectionRepository : IConnectionRepository
{
    private readonly AccessManagementDbContext _context;
    private readonly ILogger<ConnectionRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionRepository"/> class
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="logger">The logger</param>
    public ConnectionRepository(AccessManagementDbContext context, ILogger<ConnectionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Connection> CreateConnectionAsync(Connection connection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);

        var entity = connection.ToEntity();
        
        _context.Connections.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Created connection entity with ID {ConnectionId}", entity.Id);

        return entity.ToCore();
    }

    /// <inheritdoc/>
    public async Task<Connection?> GetConnectionAsync(string party, string from, string to, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Connections
            .FirstOrDefaultAsync(c => c.Party == party && c.From == from && c.To == to && c.IsActive, cancellationToken);

        return entity?.ToCore();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Connection>> GetConnectionsForPartyAsync(string party, CancellationToken cancellationToken = default)
    {
        var entities = await _context.Connections
            .Where(c => c.Party == party && c.IsActive)
            .ToListAsync(cancellationToken);

        return entities.Select(e => e.ToCore());
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteConnectionAsync(string party, string from, string to, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Connections
            .FirstOrDefaultAsync(c => c.Party == party && c.From == from && c.To == to && c.IsActive, cancellationToken);

        if (entity == null)
        {
            return false;
        }

        // Soft delete by setting IsActive to false
        entity.IsActive = false;
        entity.LastUpdated = DateTime.UtcNow;
        
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Soft deleted connection entity with ID {ConnectionId}", entity.Id);

        return true;
    }
}