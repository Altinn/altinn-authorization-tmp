using Altinn.AccessManagement.Tests.Fixtures;
using Altinn.AccessMgmt.Core.Services;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Extensions;

namespace AccessMgmt.Tests.Services;

public class RequestServiceTests : IClassFixture<PostgresFixture>
{
    private readonly IEntityService entityService;
    private readonly IAssignmentService assignmentService;
    private readonly IRequestService requestService;

    public RequestServiceTests(PostgresFixture fixture)
    {
        var db = fixture.SharedDbContext;
        entityService = new EntityService(db);
        assignmentService = new AssignmentService(db);
        requestService = new RequestService(db, assignmentService);
    }

    private AuditValues GetAuditValues()
    {
        return new AuditValues(SystemEntityConstants.StaticDataIngest, SystemEntityConstants.StaticDataIngest);
    }

    [Fact]
    public async Task Scenario_Request()
    {
        var spirh = await entityService.GetOrCreateEntity(Guid.CreateVersion7(), "Spirh AS", "12345678", EntityTypeConstants.Organisation, EntityVariantConstants.AS);
        var marius = await entityService.GetOrCreateEntity(Guid.CreateVersion7(), "Marius", "1984-10-23", EntityTypeConstants.Person, EntityVariantConstants.Person);
        var joakim = await entityService.GetOrCreateEntity(Guid.CreateVersion7(), "Joakim", "1984-10-23", EntityTypeConstants.Person, EntityVariantConstants.Person);
        var assMariusSpirh = await assignmentService.GetOrCreateAssignment(spirh.Id, marius.Id, RoleConstants.ManagingDirector, GetAuditValues());

        var bdo = await entityService.GetOrCreateEntity(Guid.CreateVersion7(), "BDO AS", "12345678", EntityTypeConstants.Organisation, EntityVariantConstants.AS);
        var kjetil = await entityService.GetOrCreateEntity(Guid.CreateVersion7(), "Kjetil", "1984-10-23", EntityTypeConstants.Person, EntityVariantConstants.Person);
        var fredrik = await entityService.GetOrCreateEntity(Guid.CreateVersion7(), "Fredrik", "1984-10-23", EntityTypeConstants.Person, EntityVariantConstants.Person);
        var assKjetilBdo = await assignmentService.GetOrCreateAssignment(bdo.Id, kjetil.Id, RoleConstants.ManagingDirector, GetAuditValues());

        // Joakim ber om å bli rettighetshaver i spirh
        var request = await requestService.CreateRequest(joakim.Id, spirh.Id, joakim.Id, RoleConstants.Rightholder, [], [], GetAuditValues());

        Assert.Multiple(() =>
        {
            Assert.NotNull(request);
            Assert.Equal(RequestStatusConstants.Open.Id, request.StatusId);
        });
    }
}
