using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.TestUtils.Data;

namespace Altinn.AccessMgmt.TestUtils;

public static class TestDataSeeds
{
    public static async Task Exec(AppDbContext db)
    {
        #region Entities
        db.Entities.AddRange([
            Entities.PersonPaula,
            Entities.PersonOrjan,
            Entities.OrganizationNordisAS,
            Entities.OrganizationVerdiqAS,
        ]);
        #endregion

        await db.SaveChangesAsync();
    }
}
