using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Repositories;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using System;

namespace Altinn.AccessMgmt.Persistence.Data.Mock;

/// <summary>
/// Mockdata
/// </summary>
public class MockDataService
    (
    IEntityTypeRepository entityTypeRepository,
    IEntityVariantRepository entityVariantRepository,
    IEntityRepository entityRepository,
    IRoleRepository roleRepository,
    IAssignmentRepository assignmentRepository,
    IPackageRepository packageRepository,
    IPackageResourceRepository packageResourceRepository,
    IResourceRepository resourceRepository,
    IResourceTypeRepository resourceTypeRepository,
    IProviderRepository providerRepository
    )
{
    private readonly IEntityTypeRepository entityTypeRepository = entityTypeRepository;
    private readonly IEntityVariantRepository entityVariantRepository = entityVariantRepository;
    private readonly IEntityRepository entityRepository = entityRepository;
    private readonly IRoleRepository roleRepository = roleRepository;
    private readonly IAssignmentRepository assignmentRepository = assignmentRepository;
    private readonly IPackageRepository packageRepository = packageRepository;
    private readonly IPackageResourceRepository packageResourceRepository = packageResourceRepository;
    private readonly IResourceRepository resourceRepository = resourceRepository;
    private readonly IResourceTypeRepository resourceTypeRepository = resourceTypeRepository;
    private readonly IProviderRepository providerRepository = providerRepository;

    public async Task GenerateBasicData()
    {
        var orgType = (await entityTypeRepository.Get(t => t.Name, "Organisasjon")).FirstOrDefault() ?? throw new Exception("Could not find type 'Organisasjon'");
        var persType = (await entityTypeRepository.Get(t => t.Name, "Person")).FirstOrDefault() ?? throw new Exception("Could not find type 'Person'");
        var variants = await entityVariantRepository.Get();
        var variantAS = variants.First(t => t.TypeId == orgType.Id && t.Name == "AS");
        var variantPers = variants.First(t => t.TypeId == persType.Id && t.Name == "Person");
        var roles = await roleRepository.Get();
        var roleDagligLeder = roles.FirstOrDefault(t => t.Code == "DAGL");
        var roleStyreLeder = roles.FirstOrDefault(t => t.Code == "LEDE");
        var roleStyreMedlem = roles.FirstOrDefault(t => t.Code == "MEDL");
        var roleAgent = roles.FirstOrDefault(t => t.Code == "AGENT");

        var spirhAS = new Entity() { Id = Guid.Parse("B2432FB4-744C-404B-9298-03FC282D5B4A"), Name = "Spirh AS", RefId = "ORG-000", TypeId = orgType.Id, VariantId = variantAS.Id };
        var bakerHansenAS = new Entity() { Id = Guid.Parse("212B4355-CE4D-4672-93BB-073AEC2BFC1E"), Name = "Baker Hansen", RefId = "ORG-001", TypeId = orgType.Id, VariantId = variantAS.Id };
        var regnskapsfolkAS = new Entity() { Id = Guid.Parse("02B0602E-9991-4E2F-9667-10B8F9D0C5A4"), Name = "Regnskapsfolk AS", RefId = "ORG-002", TypeId = orgType.Id, VariantId = variantAS.Id };
        var revisjonstroll = new Entity() { Id = Guid.Parse("2571B708-561B-4A33-92A9-1C53B439DE5B"), Name = "Revisjonstroll", RefId = "ORG-003", TypeId = orgType.Id, VariantId = variantAS.Id };
        var bakerNordbyAS = new Entity() { Id = Guid.Parse("E9191151-25C8-4D4D-807E-1F3C930AEB60"), Name = "Baker Nordby", RefId = "ORG-004", TypeId = orgType.Id, VariantId = variantAS.Id };

        await entityRepository.Upsert(spirhAS);
        await entityRepository.Upsert(bakerHansenAS);
        await entityRepository.Upsert(regnskapsfolkAS);
        await entityRepository.Upsert(revisjonstroll);
        await entityRepository.Upsert(bakerNordbyAS);

        var mariusThuen = new Entity() { Id = Guid.Parse("3ECA9413-F58C-4205-8ED4-2322E1C5E5C0"), Name = "Marius Thuen", RefId = "PERS-000", TypeId = persType.Id, VariantId = variantPers.Id };
        var fredrikJohnsen = new Entity() { Id = Guid.Parse("B238C6ED-D186-410D-983F-2B4AA887F376"), Name = "Fredrik Johnsen", RefId = "PERS-001", TypeId = persType.Id, VariantId = variantPers.Id };
        var annaLindeberg = new Entity() { Id = Guid.Parse("A4464915-41F1-4EAA-8B06-2ED8AE00CDD4"), Name = "Anna Lindeberg", RefId = "PERS-005", TypeId = persType.Id, VariantId = variantPers.Id };
        var sivertMoestue = new Entity() { Id = Guid.Parse("4EE4F732-430A-4502-A67D-47139087C7FD"), Name = "Sivert Moestue", RefId = "PERS-002", TypeId = persType.Id, VariantId = variantPers.Id };
        var gunnarHansen = new Entity() { Id = Guid.Parse("26C57500-9F6D-488A-A49C-512307D130FD"), Name = "Gunnar Hansen", RefId = "PERS-003", TypeId = persType.Id, VariantId = variantPers.Id };
        var kjetilNordby = new Entity() { Id = Guid.Parse("2C2D1E7E-5C67-4E0A-A368-537C074CE484"), Name = "Kjetil Nordby", RefId = "PERS-004", TypeId = persType.Id, VariantId = variantPers.Id };
        var nicolineWaltersen = new Entity() { Id = Guid.Parse("36AFE80D-7FB2-4053-9E6C-5BDE5F7D7084"), Name = "Nicoline Waltersen", RefId = "PERS-006", TypeId = persType.Id, VariantId = variantPers.Id };
        var viggoPettersen = new Entity() { Id = Guid.Parse("5E4AB4D0-2C02-491E-9B26-710F7AFABB5A"), Name = "Viggo Pettersen", RefId = "PERS-006", TypeId = persType.Id, VariantId = variantPers.Id };
        var petterStromstad = new Entity() { Id = Guid.Parse("68F9DB99-4CA7-4388-B572-74DE908C2A95"), Name = "Petter Strømstad", RefId = "PERS-006", TypeId = persType.Id, VariantId = variantPers.Id };
        var oleJohnnyMartinsen = new Entity() { Id = Guid.Parse("8F005D82-482B-4B05-8D84-78CBADB125BA"), Name = "Ole-Johnny Martinsen", RefId = "PERS-006", TypeId = persType.Id, VariantId = variantPers.Id };

        await entityRepository.Upsert(mariusThuen);
        await entityRepository.Upsert(fredrikJohnsen);
        await entityRepository.Upsert(annaLindeberg);
        await entityRepository.Upsert(sivertMoestue);
        await entityRepository.Upsert(gunnarHansen);
        await entityRepository.Upsert(kjetilNordby);
        await entityRepository.Upsert(nicolineWaltersen);
        await entityRepository.Upsert(viggoPettersen);
        await entityRepository.Upsert(petterStromstad);
        await entityRepository.Upsert(oleJohnnyMartinsen);

        await assignmentRepository.Upsert(new Assignment() { Id = Guid.Parse("7C044E7B-BC31-4609-9A75-8205B8F020A0"), FromId = spirhAS.Id, ToId = mariusThuen.Id, RoleId = roleDagligLeder.Id });
        await assignmentRepository.Upsert(new Assignment() { Id = Guid.Parse("383BC154-9F3C-40F5-8A67-8ED7218D8119"), FromId = spirhAS.Id, ToId = mariusThuen.Id, RoleId = roleStyreLeder.Id });
        await assignmentRepository.Upsert(new Assignment() { Id = Guid.Parse("930D972E-4E7B-43D7-9B02-913970AAF2D8"), FromId = bakerHansenAS.Id, ToId = fredrikJohnsen.Id, RoleId = roleDagligLeder.Id });
        await assignmentRepository.Upsert(new Assignment() { Id = Guid.Parse("95196B34-D8B6-4EE0-90E9-95DBA7A0D4E1"), FromId = bakerHansenAS.Id, ToId = annaLindeberg.Id, RoleId = roleStyreLeder.Id });
        await assignmentRepository.Upsert(new Assignment() { Id = Guid.Parse("F0E4318C-9642-472B-A64C-982683978648"), FromId = bakerHansenAS.Id, ToId = gunnarHansen.Id, RoleId = roleStyreMedlem.Id });
        await assignmentRepository.Upsert(new Assignment() { Id = Guid.Parse("852A6A26-CD14-459B-B178-A3004C1EEEC6"), FromId = regnskapsfolkAS.Id, ToId = sivertMoestue.Id, RoleId = roleDagligLeder.Id });
        await assignmentRepository.Upsert(new Assignment() { Id = Guid.Parse("527B7EAF-73F7-40DD-BA1A-ACBAE87CB8BE"), FromId = regnskapsfolkAS.Id, ToId = nicolineWaltersen.Id, RoleId = roleStyreLeder.Id });
        await assignmentRepository.Upsert(new Assignment() { Id = Guid.Parse("A763AF6C-F341-48C6-A958-BBD9891310A1"), FromId = revisjonstroll.Id, ToId = viggoPettersen.Id, RoleId = roleDagligLeder.Id });
        await assignmentRepository.Upsert(new Assignment() { Id = Guid.Parse("A8C31715-CE59-411F-8C81-BE69103C5131"), FromId = revisjonstroll.Id, ToId = petterStromstad.Id, RoleId = roleStyreLeder.Id });
        await assignmentRepository.Upsert(new Assignment() { Id = Guid.Parse("FBAB1C24-B4A1-487F-86BA-E02BE9CC4053"), FromId = bakerNordbyAS.Id, ToId = kjetilNordby.Id, RoleId = roleDagligLeder.Id });
        await assignmentRepository.Upsert(new Assignment() { Id = Guid.Parse("CCB8E1F9-DEC4-47E2-9026-E72F03D133ED"), FromId = bakerNordbyAS.Id, ToId = kjetilNordby.Id, RoleId = roleStyreLeder.Id });
    }

    public async Task GeneratePackageResources()
    {
        var packages = await packageRepository.GetExtended();
        var resourceTypes = await resourceTypeRepository.Get();
        var provider = (await providerRepository.Get(t => t.Name, "Digdir")).FirstOrDefault() ?? throw new Exception("Provider not found");

        foreach (var package in packages)
        {
            var resources = await packageResourceRepository.GetB(package.Id);
            if (resources == null || !resources.Any())
            {
                for (int i = 0; i < 5; i++)
                {
                    string title = GetRandomResourceTitle(i);

                    await resourceRepository.Create(new Resource()
                    {
                        Id = Guid.NewGuid(),
                        Name = GetRandomResourceTitle(i),
                        Description = "Somthing generated for the " + package.Name,
                        RefId = title.Replace(' ','_'),
                        TypeId = resourceTypes.OrderBy(t => Guid.NewGuid()).First().Id,
                        ProviderId = provider.Id
                    });
                }
            }
        }

    }

    private static readonly Random _random = new Random();

    private static string GetRandomResourceTitle(int no)
    {
        string[] values1 = { "Skjema", "Rapport", "Søknad" };
        string[] values2 = { "No.00", "#", "TSQ-", "000001" };

        int index1 = _random.Next(values1.Length);
        int index2 = _random.Next(values2.Length);
        return $"{values1[index1]} {values2[index2]}{no}";
    }
}
