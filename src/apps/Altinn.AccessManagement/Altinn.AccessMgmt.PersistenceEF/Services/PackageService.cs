using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.PersistenceEF.Services;

public class PackageService(AppDbContext db)
{
    public async Task<ExtPackage> GetPackage(Guid id)
    {
        return await db.ExtendedPackages.SingleOrDefaultAsync(t => t.Id == id);
    }
}
