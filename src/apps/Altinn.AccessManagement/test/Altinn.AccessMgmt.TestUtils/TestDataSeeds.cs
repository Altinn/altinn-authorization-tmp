using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.TestUtils.Data;

namespace Altinn.AccessMgmt.TestUtils;

public static class TestDataSeeds
{
    public static async Task Exec(AppDbContext db)
    {
        #region Entities
        db.Entities.AddRange([
            TestEntities.PersonPaula,
            TestEntities.PersonOrjan,
            TestEntities.OrganizationNordisAS,
            TestEntities.OrganizationVerdiqAS,
        ]);
        #endregion

        await db.SaveChangesAsync();
    }
}
