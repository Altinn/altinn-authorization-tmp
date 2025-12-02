using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions.Hint;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessManagement.Api.Internal.Controllers
{
    [ApiController]
    [Route("accessmanagement/api/v1/alpha/testing")]
    public class ValuesController(AppDbContext db, IHintService hintService) : ControllerBase
    {
        [HttpGet]
        public async Task<DbTest> TestReplicaDb()
        {
            using var h = hintService.UseReadOnly("Replica01");



            var result = await db.Database.SqlQueryRaw<DbTest>(
                """
            SELECT 
                inet_server_addr()::text AS "IP",
                current_database()::text AS "DB",
                COALESCE(current_setting('primary_slot_name', true), 'no-slot') AS "Slot"
            """
            ).FirstOrDefaultAsync();

            return result!;
        }
    }

    public class DbTest
    {
        public string IP { get; set; } = string.Empty;

        public string DB { get; set; } = string.Empty;

        public string Slot { get; set; } = string.Empty;
    }
}
