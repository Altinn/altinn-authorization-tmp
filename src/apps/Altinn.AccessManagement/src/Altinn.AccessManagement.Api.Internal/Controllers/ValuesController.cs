using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Queries.Connection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Api.Internal.Controllers
{
    [ApiController]
    [Route("accessmanagement/api/v1/alpha/testing")]
    public class ValuesController(ConnectionQuery connectionQuery) : ControllerBase
    {

    }
}
