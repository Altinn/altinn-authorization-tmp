using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Models.Legacy;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.Core.Services.Legacy;

public class LegacyDelegationChanges (AppDbContext dbContext) : IDelegationChangesService
{
    public async Task<IEnumerable<DelegationChanges>> GetDelegations()
    {
        return await dbContext.LegacyDelegationChanges.AsNoTracking().ToListAsync();
    }


}

public interface IDelegationChangesService
{
    Task<IEnumerable<DelegationChanges>> GetDelegations();
}
