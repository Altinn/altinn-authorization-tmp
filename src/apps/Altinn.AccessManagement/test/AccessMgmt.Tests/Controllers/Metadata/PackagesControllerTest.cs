using Altinn.AccessManagement.Api.Metadata.Controllers;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Utils.Models;
using Altinn.AccessMgmt.PersistenceEF.Utils;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AccessMgmt.Tests.Controllers.Metadata;

public class PackagesControllerTest
{
    private static PackagesController CreateController(
        IPackageService packageService,
        ITranslationService translationService,
        string acceptLanguage = "en")
    {
        var controller = new PackagesController(packageService, translationService);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Accept-Language"] = acceptLanguage;
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    private static Mock<ITranslationService> PassThroughTranslation()
    {
        var mock = new Mock<ITranslationService>();
        mock.Setup(x => x.TranslateAsync(It.IsAny<PackageDto>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync((PackageDto dto, string _, bool __) => dto);
        mock.Setup(x => x.TranslateAsync(It.IsAny<AreaGroupDto>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync((AreaGroupDto dto, string _, bool __) => dto);
        mock.Setup(x => x.TranslateAsync(It.IsAny<AreaDto>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync((AreaDto dto, string _, bool __) => dto);
        mock.Setup(x => x.TranslateAsync(It.IsAny<ResourceDto>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync((ResourceDto dto, string _, bool __) => dto);
        mock.Setup(x => x.TranslateAsync(It.IsAny<TypeDto>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync((TypeDto dto, string _, bool __) => dto);
        mock.Setup(x => x.TranslateAsync(It.IsAny<ProviderDto>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync((ProviderDto dto, string _, bool __) => dto);
        return mock;
    }

    // ── Search ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Search_WhenResultsFound_Returns200Ok()
    {
        var serviceMock = new Mock<IPackageService>();
        serviceMock.Setup(s => s.Search(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SearchObject<PackageDto>>
            {
                new() { Object = new PackageDto { Id = Guid.NewGuid(), Name = "Tax" }, Score = 0.9 },
            });

        var controller = CreateController(serviceMock.Object, PassThroughTranslation().Object);

        var result = await controller.Search("Tax");

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var items = Assert.IsAssignableFrom<IEnumerable<SearchObject<PackageDto>>>(ok.Value);
        Assert.NotEmpty(items);
    }

    [Fact]
    public async Task Search_WhenNoResults_Returns204NoContent()
    {
        var serviceMock = new Mock<IPackageService>();
        serviceMock.Setup(s => s.Search(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<SearchObject<PackageDto>>());

        var controller = CreateController(serviceMock.Object, PassThroughTranslation().Object);

        var result = await controller.Search("nothing");

        Assert.IsType<NoContentResult>(result.Result);
    }

    [Fact]
    public async Task Search_WhenServiceReturnsNull_Returns204NoContent()
    {
        var serviceMock = new Mock<IPackageService>();
        serviceMock.Setup(s => s.Search(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<SearchObject<PackageDto>>)null);

        var controller = CreateController(serviceMock.Object, PassThroughTranslation().Object);

        var result = await controller.Search("x");

        Assert.IsType<NoContentResult>(result.Result);
    }

    [Fact]
    public async Task Search_WithInvalidTypeName_ReturnsProblem()
    {
        var serviceMock = new Mock<IPackageService>();
        var controller = CreateController(serviceMock.Object, PassThroughTranslation().Object);

        var result = await controller.Search("x", typeName: "nonexistent_type_that_does_not_exist");

        Assert.IsType<ObjectResult>(result.Result);
        var objectResult = (ObjectResult)result.Result;
        Assert.Equal(500, objectResult.StatusCode); // Problem() defaults to 500
    }

    [Fact]
    public async Task Search_WithValidTypeName_CallsServiceWithTypeId()
    {
        Guid? capturedTypeId = null;
        var serviceMock = new Mock<IPackageService>();
        serviceMock.Setup(s => s.Search(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .Callback<string, List<string>, bool, Guid?, CancellationToken>((_, __, ___, id, ____) => capturedTypeId = id)
            .ReturnsAsync(new List<SearchObject<PackageDto>> { new() { Object = new PackageDto { Id = Guid.NewGuid() }, Score = 1.0 } });

        var controller = CreateController(serviceMock.Object, PassThroughTranslation().Object);

        // "organization" is a known type name in EntityTypeConstants
        await controller.Search("Tax", typeName: "organization");

        Assert.NotNull(capturedTypeId);
    }

    // ── GetHierarchy ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetHierarchy_WhenResultsFound_Returns200Ok()
    {
        var serviceMock = new Mock<IPackageService>();
        serviceMock.Setup(s => s.GetHierarchy(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AreaGroupDto> { new AreaGroupDto { Id = Guid.NewGuid() } });

        var translationMock = PassThroughTranslation();
        translationMock.Setup(x => x.TranslateAsync(It.IsAny<AreaGroupDto>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync((AreaGroupDto dto, string _, bool __) => dto);

        var controller = CreateController(serviceMock.Object, translationMock.Object);

        var result = await controller.GetHierarchy();

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetHierarchy_WhenEmpty_Returns204NoContent()
    {
        var serviceMock = new Mock<IPackageService>();
        serviceMock.Setup(s => s.GetHierarchy(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<AreaGroupDto>());

        var controller = CreateController(serviceMock.Object, PassThroughTranslation().Object);

        var result = await controller.GetHierarchy();

        Assert.IsType<NoContentResult>(result.Result);
    }

    // ── GetGroups ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetGroups_WhenResultsFound_Returns200Ok()
    {
        var serviceMock = new Mock<IPackageService>();
        serviceMock.Setup(s => s.GetAreaGroups(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AreaGroupDto> { new AreaGroupDto { Id = Guid.NewGuid() } });

        var translationMock = PassThroughTranslation();
        translationMock.Setup(x => x.TranslateAsync(It.IsAny<AreaGroupDto>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync((AreaGroupDto dto, string _, bool __) => dto);

        var controller = CreateController(serviceMock.Object, translationMock.Object);

        var result = await controller.GetGroups();

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetGroups_WhenEmpty_Returns204NoContent()
    {
        var serviceMock = new Mock<IPackageService>();
        serviceMock.Setup(s => s.GetAreaGroups(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<AreaGroupDto>());

        var controller = CreateController(serviceMock.Object, PassThroughTranslation().Object);

        var result = await controller.GetGroups();

        Assert.IsType<NoContentResult>(result.Result);
    }

    // ── GetGroup ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetGroup_WhenFound_Returns200Ok()
    {
        var id = Guid.NewGuid();
        var serviceMock = new Mock<IPackageService>();
        serviceMock.Setup(s => s.GetAreaGroup(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AreaGroupDto { Id = id });

        var translationMock = PassThroughTranslation();
        translationMock.Setup(x => x.TranslateAsync(It.IsAny<AreaGroupDto>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync((AreaGroupDto dto, string _, bool __) => dto);

        var controller = CreateController(serviceMock.Object, translationMock.Object);

        var result = await controller.GetGroup(id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetGroup_WhenNotFound_Returns404()
    {
        var serviceMock = new Mock<IPackageService>();
        serviceMock.Setup(s => s.GetAreaGroup(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AreaGroupDto)null);

        var controller = CreateController(serviceMock.Object, PassThroughTranslation().Object);

        var result = await controller.GetGroup(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result.Result);
    }

    // ── GetGroupAreas ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetGroupAreas_WhenAreasFound_Returns200Ok()
    {
        var id = Guid.NewGuid();
        var serviceMock = new Mock<IPackageService>();
        serviceMock.Setup(s => s.GetAreas(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AreaDto> { new AreaDto { Id = Guid.NewGuid() } });

        var controller = CreateController(serviceMock.Object, PassThroughTranslation().Object);

        var result = await controller.GetGroupAreas(id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsAssignableFrom<IEnumerable<AreaDto>>(ok.Value);
        // GetAreaGroup must NOT be hit when areas were found.
        serviceMock.Verify(s => s.GetAreaGroup(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetGroupAreas_WhenNoAreasButGroupExists_Returns204NoContent()
    {
        var id = Guid.NewGuid();
        var serviceMock = new Mock<IPackageService>();
        serviceMock.Setup(s => s.GetAreas(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<AreaDto>());
        serviceMock.Setup(s => s.GetAreaGroup(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AreaGroupDto { Id = id });

        var controller = CreateController(serviceMock.Object, PassThroughTranslation().Object);

        var result = await controller.GetGroupAreas(id);

        Assert.IsType<NoContentResult>(result.Result);
    }

    [Fact]
    public async Task GetGroupAreas_WhenNoAreasAndGroupMissing_Returns404NotFound()
    {
        var id = Guid.NewGuid();
        var serviceMock = new Mock<IPackageService>();
        serviceMock.Setup(s => s.GetAreas(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<AreaDto>)null);
        serviceMock.Setup(s => s.GetAreaGroup(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AreaGroupDto)null);

        var controller = CreateController(serviceMock.Object, PassThroughTranslation().Object);

        var result = await controller.GetGroupAreas(id);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    // ── GetArea ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetArea_WhenFound_Returns200Ok()
    {
        var id = Guid.NewGuid();
        var serviceMock = new Mock<IPackageService>();
        serviceMock.Setup(s => s.GetArea(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AreaDto { Id = id });

        var controller = CreateController(serviceMock.Object, PassThroughTranslation().Object);

        var result = await controller.GetArea(id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetArea_WhenNotFound_Returns404()
    {
        var serviceMock = new Mock<IPackageService>();
        serviceMock.Setup(s => s.GetArea(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AreaDto)null);

        var controller = CreateController(serviceMock.Object, PassThroughTranslation().Object);

        var result = await controller.GetArea(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result.Result);
    }

    // ── GetAreaPackages ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetAreaPackages_WhenPackagesFound_Returns200Ok()
    {
        var id = Guid.NewGuid();
        var serviceMock = new Mock<IPackageService>();
        serviceMock.Setup(s => s.GetPackagesByArea(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PackageDto> { new PackageDto { Id = Guid.NewGuid() } });

        var controller = CreateController(serviceMock.Object, PassThroughTranslation().Object);

        var result = await controller.GetAreaPackages(id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsAssignableFrom<IEnumerable<PackageDto>>(ok.Value);
        // GetArea must NOT be hit when packages were found.
        serviceMock.Verify(s => s.GetArea(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAreaPackages_WhenNoPackagesButAreaExists_Returns204NoContent()
    {
        var id = Guid.NewGuid();
        var serviceMock = new Mock<IPackageService>();
        serviceMock.Setup(s => s.GetPackagesByArea(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<PackageDto>());
        serviceMock.Setup(s => s.GetArea(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AreaDto { Id = id });

        var controller = CreateController(serviceMock.Object, PassThroughTranslation().Object);

        var result = await controller.GetAreaPackages(id);

        Assert.IsType<NoContentResult>(result.Result);
    }

    [Fact]
    public async Task GetAreaPackages_WhenNoPackagesAndAreaMissing_Returns404NotFound()
    {
        var id = Guid.NewGuid();
        var serviceMock = new Mock<IPackageService>();
        serviceMock.Setup(s => s.GetPackagesByArea(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<PackageDto>)null);
        serviceMock.Setup(s => s.GetArea(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AreaDto)null);

        var controller = CreateController(serviceMock.Object, PassThroughTranslation().Object);

        var result = await controller.GetAreaPackages(id);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    // ── GetPackage ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPackage_WhenFound_Returns200Ok()
    {
        var id = Guid.NewGuid();
        var serviceMock = new Mock<IPackageService>();
        serviceMock.Setup(s => s.GetPackage(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PackageDto { Id = id });

        var controller = CreateController(serviceMock.Object, PassThroughTranslation().Object);

        var result = await controller.GetPackage(id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetPackage_WhenNotFound_Returns404()
    {
        var serviceMock = new Mock<IPackageService>();
        serviceMock.Setup(s => s.GetPackage(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PackageDto)null);

        var controller = CreateController(serviceMock.Object, PassThroughTranslation().Object);

        var result = await controller.GetPackage(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result.Result);
    }

    // ── GetPackageByUrn ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetPackageByUrn_WhenFound_Returns200Ok()
    {
        const string urn = "skattnaering";
        var serviceMock = new Mock<IPackageService>();
        serviceMock.Setup(s => s.GetPackageByUrnValue(urn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PackageDto { Id = Guid.NewGuid() });

        var controller = CreateController(serviceMock.Object, PassThroughTranslation().Object);

        var result = await controller.GetPackageByUrn(urn);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetPackageByUrn_WhenNotFound_Returns404()
    {
        var serviceMock = new Mock<IPackageService>();
        serviceMock.Setup(s => s.GetPackageByUrnValue(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PackageDto)null);

        var controller = CreateController(serviceMock.Object, PassThroughTranslation().Object);

        var result = await controller.GetPackageByUrn("missing");

        Assert.IsType<NotFoundResult>(result.Result);
    }

    // ── GetPackageResources ─────────────────────────────────────────────────

    [Fact]
    public async Task GetPackageResources_WhenResourcesFound_Returns200Ok()
    {
        var id = Guid.NewGuid();
        var serviceMock = new Mock<IPackageService>();
        serviceMock.Setup(s => s.GetPackageResources(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ResourceDto> { new ResourceDto { Id = Guid.NewGuid() } });

        var controller = CreateController(serviceMock.Object, PassThroughTranslation().Object);

        var result = await controller.GetPackageResources(id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsAssignableFrom<IEnumerable<ResourceDto>>(ok.Value);
        // GetPackage must NOT be hit when resources were found.
        serviceMock.Verify(s => s.GetPackage(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetPackageResources_WhenNoResourcesButPackageExists_Returns204NoContent()
    {
        var id = Guid.NewGuid();
        var serviceMock = new Mock<IPackageService>();
        serviceMock.Setup(s => s.GetPackageResources(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<ResourceDto>());
        serviceMock.Setup(s => s.GetPackage(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PackageDto { Id = id });

        var controller = CreateController(serviceMock.Object, PassThroughTranslation().Object);

        var result = await controller.GetPackageResources(id);

        Assert.IsType<NoContentResult>(result.Result);
    }

    [Fact]
    public async Task GetPackageResources_WhenNoResourcesAndPackageMissing_Returns404NotFound()
    {
        var id = Guid.NewGuid();
        var serviceMock = new Mock<IPackageService>();
        serviceMock.Setup(s => s.GetPackageResources(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<ResourceDto>)null);
        serviceMock.Setup(s => s.GetPackage(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PackageDto)null);

        var controller = CreateController(serviceMock.Object, PassThroughTranslation().Object);

        var result = await controller.GetPackageResources(id);

        Assert.IsType<NotFoundResult>(result.Result);
    }
}
