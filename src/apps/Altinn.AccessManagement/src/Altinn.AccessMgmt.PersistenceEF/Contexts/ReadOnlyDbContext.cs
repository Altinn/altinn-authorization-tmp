using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.PersistenceEF.Contexts;

/// <inheritdoc />
public class ReadOnlyDbContext(DbContextOptions<AppDbContext> options) : AppDbContext(options) { }
