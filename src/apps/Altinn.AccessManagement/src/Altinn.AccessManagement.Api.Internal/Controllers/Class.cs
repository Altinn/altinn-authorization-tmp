using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Queries.Connection;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Api.Internal.Controllers;

[Route("accessmanagement/api/v1/internal/query")]
[ApiController]
public class QueryController(AppDbContext dbContext) : ControllerBase
{
    [HttpPost("connection")]
    public async Task<ActionResult> GetQuery(ConnectionQueryFilter filter, ConnectionQueryDirection direction, bool useNewQuery)
    {
        var queryService = new ConnectionQuery(dbContext);
        return Ok(await queryService.GenerateDebugQuery(filter, direction, useNewQuery));
    }
}
