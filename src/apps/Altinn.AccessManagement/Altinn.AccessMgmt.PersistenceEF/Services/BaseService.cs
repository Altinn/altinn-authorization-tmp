using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.PersistenceEF.Services;

public class BaseService<TBasic, TExtended, TAudit>
    where TBasic : class
    where TExtended : class
    where TAudit : class
{
    protected readonly BasicDbContext _basicDb;
    protected readonly ExtendedDbContext _extendedDb;
    protected readonly AuditDbContext _auditDb;

    public BaseService(BasicDbContext basicDb, ExtendedDbContext extendedDb, AuditDbContext auditDb)
    {
        _basicDb = basicDb;
        _extendedDb = extendedDb;
        _auditDb = auditDb;
    }

    public virtual async ValueTask<TBasic?> Get(Guid id) =>
        await _basicDb.Set<TBasic>().SingleOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);

    public virtual async ValueTask<TExtended?> GetExtended(Guid id) =>
        await _extendedDb.Set<TExtended>().SingleOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);

    public virtual async ValueTask<TAudit?> GetAudit(Guid id) =>
        await _auditDb.Set<TAudit>().SingleOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);

    public virtual async ValueTask<TBasic?> Create(TBasic obj)
    {
        await _basicDb.AddAsync(obj);
        await _basicDb.SaveChangesAsync();
        return obj;
    }

    public virtual async Task Create(TBasic obj, AuditValues audit)
    {
        _basicDb.Database.SetAuditSession(audit);
        await _basicDb.AddAsync(obj);
        await _basicDb.SaveChangesAsync();
    }
}
