using System.Diagnostics;
using System.Runtime.CompilerServices;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Host.Pipeline.Services;
using Altinn.Authorization.Integration.Platform.Register;
using Altinn.Register.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.AccessMgmt.Core.Pipelines;

internal static class RegisterPipelines
{
    internal static class PartyJobs
    {
        internal static async IAsyncEnumerable<(List<T> Items, string NextPage)> Extract<T>(PipelineSourceContext context, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var register = context.Services.ServiceProvider.GetRequiredService<IAltinnRegister>();
            var lease = await context.Lease.Get<Lease>(cancellationToken);
            var result = new List<T>();

            await foreach (var page in await register.StreamParties(AltinnRegisterClient.DefaultFields, lease.NextPage, cancellationToken))
            {
                PipelineUtils.EnsureSuccess(page);
                foreach (var item in page.Content.Data)
                {
                    if (item is T data)
                    {
                        result.Add(data);
                    }
                }

                if (result.Count == 0)
                {
                    continue;
                }

                yield return (
                    result,
                    page.Content?.Links?.Next
                );

                result = [];
            }
        }

        internal static async Task<(List<Entity> Items, string NextPage)> Transform(PipelineSegmentContext<(List<Person> Items, string NextPage)> context)
        {
            var result = new List<Entity>();
            foreach (var person in context.Data.Items)
            {
                var entity = CreateEntity(person, e =>
                {
                    e.DateOfBirth = person.DateOfBirth.HasValue ? person.DateOfBirth.Value : null;
                    e.DateOfDeath = person.DateOfDeath.HasValue ? person.DateOfDeath.Value : null;
                    e.RefId = person.PersonIdentifier.ToString();
                    e.PersonIdentifier = person.PersonIdentifier.ToString();
                    e.TypeId = EntityTypeConstants.Person;
                    e.VariantId = EntityVariantConstants.Person;
                });

                result.Add(entity);
            }

            return (result, context.Data.NextPage);
        }

        internal static async Task<(List<Entity> Items, string NextPage)> Transform(PipelineSegmentContext<(List<Organization> Items, string NextPage)> context)
        {
            var result = new List<Entity>();
            foreach (var organization in context.Data.Items)
            {
                if (!EntityVariantConstants.TryGetByName(organization.UnitType.Value, out var variant))
                {
                    throw new InvalidDataException($"Invalid Unit Type {organization.UnitType}");
                }

                var entity = CreateEntity(organization, o =>
                {
                    o.RefId = organization.OrganizationIdentifier.ToString();
                    o.OrganizationIdentifier = organization.OrganizationIdentifier.ToString();
                    o.VariantId = variant;
                    o.TypeId = EntityTypeConstants.Organisation;
                });

                result.Add(entity);
            }

            return (result, context.Data.NextPage);
        }

        internal static async Task<(List<Entity> Items, string NextPage)> Transform(PipelineSegmentContext<(List<SelfIdentifiedUser> Items, string NextPage)> context)
        {
            var result = new List<Entity>();
            foreach (var si in context.Data.Items)
            {
                var entity = CreateEntity(si, s =>
                {
                    s.RefId = si.User.Value.Username.Value;
                    s.TypeId = EntityTypeConstants.SelfIdentified;
                    s.VariantId = EntityVariantConstants.SI;
                });

                result.Add(entity);
            }

            return (result, context.Data.NextPage);
        }

        internal static async Task<(List<Entity> Items, string NextPage)> Transform(PipelineSegmentContext<(List<SystemUser> Items, string NextPage)> context)
        {
            var result = new List<Entity>();
            foreach (var systemUser in context.Data.Items)
            {
                var systemTypeVariant = systemUser.SystemUserType.Value.Value switch
                {
                    SystemUserType.ClientPartySystemUser => EntityVariantConstants.AgentSystem,
                    SystemUserType.FirstPartySystemUser => EntityVariantConstants.StandardSystem,
                    _ => throw new InvalidDataException($"Missing mapping for system type {systemUser.SystemUserType}")
                };

                var entity = CreateEntity(systemUser, s =>
                {
                    s.RefId = systemUser.Uuid.ToString();
                    s.TypeId = EntityTypeConstants.SystemUser;
                    s.VariantId = systemTypeVariant;
                });

                result.Add(entity);
            }

            return (result, context.Data.NextPage);
        }

        internal static async Task<(List<Entity> Items, string NextPage)> Transform(PipelineSegmentContext<(List<EnterpriseUser> Items, string NextPage)> context)
        {
            var activity = Activity.Current;
            var result = new List<Entity>();
            foreach (var enterpriseUser in context.Data.Items)
            {
                var entity = CreateEntity(enterpriseUser, e =>
                {
                    e.RefId = enterpriseUser.User.Value.Username.Value;
                    e.TypeId = EntityTypeConstants.EnterpriseUser;
                    e.VariantId = EntityVariantConstants.EnterpriseUser;
                });

                result.Add(entity);
            }

            return (result, context.Data.NextPage);
        }

        internal static async Task Load(PipelineSinkContext<(List<Entity> Items, string NextPage)> context)
        {
            var merged = await PipelineUtils.Flush(context.Services, context.Data.Items, ["id"]);
            if (merged > 0)
            {
                await context.Lease.Update(new Lease() { NextPage = context.Data.NextPage });
            }
        }

        private static Entity CreateEntity(Party party, Action<Entity> configureEntity)
        {
            var entity = new Entity()
            {
                Id = party.Uuid,
                Name = party.DisplayName.ToString(),
                DeletedAt = party?.DeletedAt.HasValue == true ? party.DeletedAt.Value : null,
                PartyId = party?.PartyId.HasValue == true ? Convert.ToInt32(party.PartyId.Value) : null,
                UserId = party?.User.Value?.UserId.HasValue == true ? Convert.ToInt32(party.User.Value.UserId.Value) : null,
                Username = party?.User.Value?.Username.HasValue == true ? party.User.Value.Username.ToString() : null,
                IsDeleted = party?.IsDeleted.HasValue == true ? party.IsDeleted.Value : false,
            };

            configureEntity(entity);
            return entity;
        }
    }

    internal static class ExternalRoleAssignmentJobs
    {
        internal static async IAsyncEnumerable<(List<ExternalRoleAssignmentEvent> Items, string NextPage)> Extract(PipelineSourceContext context, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var register = context.Services.ServiceProvider.GetRequiredService<IAltinnRegister>();
            var lease = await context.Lease.Get<Lease>(cancellationToken);

            await foreach (var page in await register.StreamRoles([], lease.NextPage, cancellationToken))
            {
                PipelineUtils.EnsureSuccess(page);
                yield return (
                    page.Content.Data.ToList(),
                    page.Content?.Links?.Next
                );
            }
        }
    }

    internal class Lease
    {
        public string NextPage { get; set; }
    }
}
