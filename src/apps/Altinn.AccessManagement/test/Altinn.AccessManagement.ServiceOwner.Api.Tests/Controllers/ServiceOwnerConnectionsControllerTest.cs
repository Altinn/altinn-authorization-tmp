using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.TestUtils;
using Altinn.AccessManagement.TestUtils.Data;
using Altinn.AccessManagement.TestUtils.Fixtures;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.Api.Contracts.Consent;
using Altinn.Authorization.Api.Contracts.Register;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Altinn.AccessManagement.ServiceOwner.Api.Tests.Controllers;

/// <summary>
/// Tests for <see cref="ConnectionsController"/> in the ServiceOwner API.
/// </summary>
public class ServiceOwnerConnectionsControllerTest
{
    public const string Route = "accessmanagement/api/v1/serviceowner/connections";

    #region POST accessmanagement/api/v1/serviceowner/connections/accesspackages

    /// <summary>
    /// Tests for <see cref="ConnectionsController.AddPackages(ServiceOwnerAccessPackageDelegation, CancellationToken)"/>
    /// </summary>
    public class AddPackages : IClassFixture<ApiFixture>
    {
        public AddPackages(ApiFixture fixture)
        {
            Fixture = fixture;

            // Configure the whitelist for the test service owner
            Fixture.WithInMemoryAppsettings(dict =>
            {
                dict[$"ServiceOwnerDelegation:PackageWhiteList:{TestData.StorMektigTenesteeier.Entity.OrganizationIdentifier}:0"] = "innbygger-skatteforhold-privatpersoner";
                dict[$"ServiceOwnerDelegation:PackageWhiteList:{TestData.StorMektigTenesteeier.Entity.OrganizationIdentifier}:1"] = "another-allowed-package";
                dict[$"ServiceOwnerDelegation:PackageWhiteList:{TestData.StorMektigTenesteeier.Entity.OrganizationIdentifier}:2"] = "jordbruk";
            });

            Fixture.EnsureSeedOnce<AddPackages>(db =>
            {
                // Seed any initial data needed for tests
                db.SaveChanges();
            });
        }

        public ApiFixture Fixture { get; }

        private HttpClient CreateClient()
        {
            var client = Fixture.Server.CreateClient();
            var token = TestTokenGenerator.CreateToken(new ClaimsIdentity("mock"), claims =>
            {
                claims.Add(new Claim(AltinnCoreClaimTypes.Org, "SKD"));
                claims.Add(new Claim("scope", AuthzConstants.SCOPE_SERVICEOWNER_PACKAGE_DELEGATION_WRITE));
                claims.Add(new Claim("consumer", GetConsumerClaimJson(TestData.StorMektigTenesteeier.Entity.OrganizationIdentifier)));
            });
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            return client;
        }

        private static string GetConsumerClaimJson(string orgNumber)
        {
            return $$"""{ "authority":"iso6523-actorid-upis", "ID":"0192:{{orgNumber}}"}""";
        }

        [Fact]
        public async Task AddPackage_WithValidRequest_ReturnsOk()
        {
            // Arrange
            var client = CreateClient();

            ServiceOwnerConnectionPartyUrn.PersonId from = ServiceOwnerConnectionPartyUrn.PersonId.Create(PersonIdentifier.Parse(TestData.VegardSolberg.Entity.PersonIdentifier));
            ServiceOwnerConnectionPartyUrn.PersonId to = ServiceOwnerConnectionPartyUrn.PersonId.Create(PersonIdentifier.Parse(TestData.IngerNygard.Entity.PersonIdentifier));
            AccessPackageUrn.AccessPackage package = AccessPackageUrn.AccessPackage.Create(new AccessPackageIdentifier("innbygger-skatteforhold-privatpersoner"));

            ServiceOwnerAccessPackageDelegation request = new()
            {
                From = from,
                To = to,
                PackageUrn = package
            };

            // Act
            var response = await client.PostAsJsonAsync($"{Route}/accesspackages", request, TestContext.Current.CancellationToken);

            // Assert
            var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Verify the assignment package was created in the database
            await Fixture.QueryDb(async db =>
            {
                var assignmentPackage = await db.AssignmentPackages
                    .Include(ap => ap.Assignment)
                    .Where(ap => ap.Assignment.FromId == TestData.VegardSolberg.Id)
                    .Where(ap => ap.Assignment.ToId == TestData.IngerNygard.Id)
                    .Where(ap => ap.PackageId == PackageConstants.InnbyggerSkatteforholdPrivatpersoner.Id)
                    .FirstOrDefaultAsync(TestContext.Current.CancellationToken);

                Assert.NotNull(assignmentPackage);
            });
        }

        [Fact]
        public async Task AddPackage_WithExistingAssignment_ReturnsExistingPackage()
        {
            // Arrange
            var client = CreateClient();

            // First, seed an existing assignment with package
            await Fixture.QueryDb(async db =>
            {
                var existingAssignment = new Assignment()
                {
                    FromId = TestData.BjornMoe.Id,
                    ToId = TestData.LarsBakke.Id,
                    RoleId = RoleConstants.Rightholder,
                };
                db.Assignments.Add(existingAssignment);
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);

                var existingPackage = new AssignmentPackage()
                {
                    AssignmentId = existingAssignment.Id,
                    PackageId = PackageConstants.InnbyggerSkatteforholdPrivatpersoner.Id,
                };
                db.AssignmentPackages.Add(existingPackage);
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            });

            ServiceOwnerConnectionPartyUrn.PersonId from = ServiceOwnerConnectionPartyUrn.PersonId.Create(PersonIdentifier.Parse(TestData.BjornMoe.Entity.PersonIdentifier));
            ServiceOwnerConnectionPartyUrn.PersonId to = ServiceOwnerConnectionPartyUrn.PersonId.Create(PersonIdentifier.Parse(TestData.LarsBakke.Entity.PersonIdentifier));
            AccessPackageUrn.AccessPackage package = AccessPackageUrn.AccessPackage.Create(new AccessPackageIdentifier("innbygger-skatteforhold-privatpersoner"));

            ServiceOwnerAccessPackageDelegation request = new()
            {
                From = from,
                To = to,
                PackageUrn = package
            };

            // Act
            HttpResponseMessage response = await client.PostAsJsonAsync($"{Route}/accesspackages", request, TestContext.Current.CancellationToken);
            string contentText = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task AddPackage_WithInvalidPackageUrn_ReturnsForbidden()
        {
            // Arrange
            var client = CreateClient();

            ServiceOwnerConnectionPartyUrn.PersonId from = ServiceOwnerConnectionPartyUrn.PersonId.Create(PersonIdentifier.Parse(TestData.BjornMoe.Entity.PersonIdentifier));
            ServiceOwnerConnectionPartyUrn.PersonId to = ServiceOwnerConnectionPartyUrn.PersonId.Create(PersonIdentifier.Parse(TestData.LarsBakke.Entity.PersonIdentifier));
            AccessPackageUrn.AccessPackage package = AccessPackageUrn.AccessPackage.Create(new AccessPackageIdentifier("nonexistent-package"));

            ServiceOwnerAccessPackageDelegation request = new()
            {
                From = from,
                To = to,
                PackageUrn = package
            };

            // Act
            var response = await client.PostAsJsonAsync($"{Route}/accesspackages", request, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task AddPackage_WithFromAsOrganizationIdentifiersForPersonPackage_BadRequest()
        {
            // Arrange
            var client = CreateClient();

            ServiceOwnerConnectionPartyUrn.OrganizationId from = ServiceOwnerConnectionPartyUrn.OrganizationId.Create(OrganizationNumber.Parse(TestData.MittRegnskap.Entity.OrganizationIdentifier));
            ServiceOwnerConnectionPartyUrn.OrganizationId to = ServiceOwnerConnectionPartyUrn.OrganizationId.Create(OrganizationNumber.Parse(TestData.RpcAS.Entity.OrganizationIdentifier));
            AccessPackageUrn.AccessPackage package = AccessPackageUrn.AccessPackage.Create(new AccessPackageIdentifier("innbygger-skatteforhold-privatpersoner"));

            ServiceOwnerAccessPackageDelegation request = new()
            {
                From = from,
                To = to,
                PackageUrn = package
            };

            // Act
            var response = await client.PostAsJsonAsync($"{Route}/accesspackages", request, TestContext.Current.CancellationToken);

            // Assert - The current implementation only handles person identifiers, so organization identifiers should return BadRequest
            var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            
            // Update this assertion once organization support is added
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task AddPackage_ToOrganisation_ReturnsOk()
        {
            // Arrange
            var client = CreateClient();

            ServiceOwnerConnectionPartyUrn.PersonId from = ServiceOwnerConnectionPartyUrn.PersonId.Create(PersonIdentifier.Parse(TestData.VegardSolberg.Entity.PersonIdentifier));
            ServiceOwnerConnectionPartyUrn.OrganizationId to = ServiceOwnerConnectionPartyUrn.OrganizationId.Create(OrganizationNumber.Parse(TestData.SvendsenAutomobil.Entity.OrganizationIdentifier));
            AccessPackageUrn.AccessPackage package = AccessPackageUrn.AccessPackage.Create(new AccessPackageIdentifier("innbygger-skatteforhold-privatpersoner"));

            ServiceOwnerAccessPackageDelegation request = new()
            {
                From = from,
                To = to,
                PackageUrn = package
            };

            // Act
            var response = await client.PostAsJsonAsync($"{Route}/accesspackages", request, TestContext.Current.CancellationToken);

            string responsejson = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            await Fixture.QueryDb(async db =>
            {
                var assignmentPackage = await db.AssignmentPackages
                    .Include(ap => ap.Assignment)
                    .Where(ap => ap.Assignment.FromId == TestData.BakerJohnsen.Id)
                    .Where(ap => ap.Assignment.ToId == TestData.SvendsenAutomobil.Id)
                    .Where(ap => ap.PackageId == PackageConstants.Agriculture.Id)
                    .FirstOrDefaultAsync(TestContext.Current.CancellationToken);

                Assert.NotNull(assignmentPackage);
            });
        }

        [Fact]
        public async Task AddPackage_FromOrganisationOnPackageThatSupportsOrganisation_ReturnsOk()
        {
            // Arrange
            var client = CreateClient();

            Fixture.WithInMemoryAppsettings(dict =>
            {
                dict[$"ServiceOwnerDelegation:PackageWhiteList:{TestData.StorMektigTenesteeier.Entity.OrganizationIdentifier}:2"] = "jordbruk";
            });

            ServiceOwnerConnectionPartyUrn.OrganizationId from = ServiceOwnerConnectionPartyUrn.OrganizationId.Create(OrganizationNumber.Parse(TestData.FredriksonsFabrikk.Entity.OrganizationIdentifier));
            ServiceOwnerConnectionPartyUrn.OrganizationId to = ServiceOwnerConnectionPartyUrn.OrganizationId.Create(OrganizationNumber.Parse(TestData.RegnskapNorge.Entity.OrganizationIdentifier));
            AccessPackageUrn.AccessPackage package = AccessPackageUrn.AccessPackage.Create(new AccessPackageIdentifier("jordbruk"));

            ServiceOwnerAccessPackageDelegation request = new()
            {
                From = from,
                To = to,
                PackageUrn = package
            };

            // Act
            var response = await client.PostAsJsonAsync($"{Route}/accesspackages", request, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task RevokePackage_WhereAssignmentIsRevoked_ReturnsNoContent()
        {
            // Arrange - First create a package delegation, then revoke it
            var client = CreateClient();

            ServiceOwnerConnectionPartyUrn.PersonId from = ServiceOwnerConnectionPartyUrn.PersonId.Create(PersonIdentifier.Parse(TestData.SiljeHaugen.Entity.PersonIdentifier));
            ServiceOwnerConnectionPartyUrn.PersonId to = ServiceOwnerConnectionPartyUrn.PersonId.Create(PersonIdentifier.Parse(TestData.EinarBerg.Entity.PersonIdentifier));
            AccessPackageUrn.AccessPackage package = AccessPackageUrn.AccessPackage.Create(new AccessPackageIdentifier("innbygger-skatteforhold-privatpersoner"));

            ServiceOwnerAccessPackageDelegation addRequest = new()
            {
                From = from,
                To = to,
                PackageUrn = package
            };

            // First add the package
            var addResponse = await client.PostAsJsonAsync($"{Route}/accesspackages", addRequest, TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.OK, addResponse.StatusCode);

            // Act - Revoke the package
            var revokeResponse = await client.PostAsJsonAsync($"{Route}/accesspackages/revoke", addRequest, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, revokeResponse.StatusCode);
        }

        [Fact]
        public async Task RevokePackage_WhereAssignmentIsNotRevoked_ReturnsNoContent()
        {
            // Arrange - Create an assignment with two packages via service owner, revoke one
            var client = CreateClient();

            Fixture.WithInMemoryAppsettings(dict =>
            {
                dict[$"ServiceOwnerDelegation:PackageWhiteList:{TestData.StorMektigTenesteeier.Entity.OrganizationIdentifier}:2"] = "another-allowed-package";
            });

            ServiceOwnerConnectionPartyUrn.PersonId from = ServiceOwnerConnectionPartyUrn.PersonId.Create(PersonIdentifier.Parse(TestData.ToneKvam.Entity.PersonIdentifier));
            ServiceOwnerConnectionPartyUrn.PersonId to = ServiceOwnerConnectionPartyUrn.PersonId.Create(PersonIdentifier.Parse(TestData.ArneLund.Entity.PersonIdentifier));

            // Add first package
            ServiceOwnerAccessPackageDelegation addRequest1 = new()
            {
                From = from,
                To = to,
                PackageUrn = AccessPackageUrn.AccessPackage.Create(new AccessPackageIdentifier("innbygger-skatteforhold-privatpersoner"))
            };
            var addResponse1 = await client.PostAsJsonAsync($"{Route}/accesspackages", addRequest1, TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.OK, addResponse1.StatusCode);

            // Act - Revoke the first package (assignment should still exist if there were other packages)
            var revokeResponse = await client.PostAsJsonAsync($"{Route}/accesspackages/revoke", addRequest1, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, revokeResponse.StatusCode);
        }

        [Fact]
        public async Task RevokePackage_WhereAssignmentDoesNotExist_ReturnsBadRequest()
        {
            // Arrange
            var client = CreateClient();

            ServiceOwnerConnectionPartyUrn.PersonId from = ServiceOwnerConnectionPartyUrn.PersonId.Create(PersonIdentifier.Parse(TestData.OddHalvorsen.Entity.PersonIdentifier));
            ServiceOwnerConnectionPartyUrn.PersonId to = ServiceOwnerConnectionPartyUrn.PersonId.Create(PersonIdentifier.Parse(TestData.LivKristiansen.Entity.PersonIdentifier));
            AccessPackageUrn.AccessPackage package = AccessPackageUrn.AccessPackage.Create(new AccessPackageIdentifier("innbygger-skatteforhold-privatpersoner"));

            ServiceOwnerAccessPackageDelegation request = new()
            {
                From = from,
                To = to,
                PackageUrn = package
            };

            // Act
            var response = await client.PostAsJsonAsync($"{Route}/accesspackages/revoke", request, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task RevokePackage_WhereAssignmentPackageDoesNotExist_ReturnsBadRequest()
        {
            // Arrange - Create an assignment with one package, then try to revoke a different package
            var client = CreateClient();

            ServiceOwnerConnectionPartyUrn.PersonId from = ServiceOwnerConnectionPartyUrn.PersonId.Create(PersonIdentifier.Parse(TestData.HelgeNilsen.Entity.PersonIdentifier));
            ServiceOwnerConnectionPartyUrn.PersonId to = ServiceOwnerConnectionPartyUrn.PersonId.Create(PersonIdentifier.Parse(TestData.SteinarAndreassen.Entity.PersonIdentifier));

            // Add a package to create an assignment
            ServiceOwnerAccessPackageDelegation addRequest = new()
            {
                From = from,
                To = to,
                PackageUrn = AccessPackageUrn.AccessPackage.Create(new AccessPackageIdentifier("innbygger-skatteforhold-privatpersoner"))
            };
            var addResponse = await client.PostAsJsonAsync($"{Route}/accesspackages", addRequest, TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.OK, addResponse.StatusCode);

            // Try to revoke a different package that was never added
            Fixture.WithInMemoryAppsettings(dict =>
            {
                dict[$"ServiceOwnerDelegation:PackageWhiteList:{TestData.StorMektigTenesteeier.Entity.OrganizationIdentifier}:2"] = "another-allowed-package";
            });

            ServiceOwnerAccessPackageDelegation revokeRequest = new()
            {
                From = from,
                To = to,
                PackageUrn = AccessPackageUrn.AccessPackage.Create(new AccessPackageIdentifier("another-allowed-package"))
            };

            // Act
            var revokeResponse = await client.PostAsJsonAsync($"{Route}/accesspackages/revoke", revokeRequest, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, revokeResponse.StatusCode);
        }

        [Fact]
        public async Task RevokePackage_WhereAssignmentWasNotPerformedByServiceOwner_ReturnsBadRequest()
        {
            // Arrange - Seed an assignment+package that was NOT created by the service owner
            var client = CreateClient();

            await Fixture.QueryDb(async db =>
            {
                var assignment = new Assignment()
                {
                    FromId = TestData.GeirPedersen.Id,
                    ToId = TestData.MaritEriksen.Id,
                    RoleId = RoleConstants.Rightholder,
                };
                db.Assignments.Add(assignment);
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);

                var pkg = new AssignmentPackage()
                {
                    AssignmentId = assignment.Id,
                    PackageId = PackageConstants.InnbyggerSkatteforholdPrivatpersoner.Id,
                };
                db.AssignmentPackages.Add(pkg);
                await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            });

            ServiceOwnerConnectionPartyUrn.PersonId from = ServiceOwnerConnectionPartyUrn.PersonId.Create(PersonIdentifier.Parse(TestData.GeirPedersen.Entity.PersonIdentifier));
            ServiceOwnerConnectionPartyUrn.PersonId to = ServiceOwnerConnectionPartyUrn.PersonId.Create(PersonIdentifier.Parse(TestData.MaritEriksen.Entity.PersonIdentifier));
            AccessPackageUrn.AccessPackage package = AccessPackageUrn.AccessPackage.Create(new AccessPackageIdentifier("innbygger-skatteforhold-privatpersoner"));

            ServiceOwnerAccessPackageDelegation request = new()
            {
                From = from,
                To = to,
                PackageUrn = package
            };

            // Act
            var response = await client.PostAsJsonAsync($"{Route}/accesspackages/revoke", request, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task AddPackage_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Arrange
            var client = Fixture.Server.CreateClient(); // No auth token

            ServiceOwnerConnectionPartyUrn.PersonId from = ServiceOwnerConnectionPartyUrn.PersonId.Create(PersonIdentifier.Parse(TestData.BjornMoe.Entity.PersonIdentifier));
            ServiceOwnerConnectionPartyUrn.PersonId to = ServiceOwnerConnectionPartyUrn.PersonId.Create(PersonIdentifier.Parse(TestData.LarsBakke.Entity.PersonIdentifier));
            AccessPackageUrn.AccessPackage package = AccessPackageUrn.AccessPackage.Create(new AccessPackageIdentifier("innbygger-skatteforhold-privatpersoner"));

            ServiceOwnerAccessPackageDelegation request = new()
            {
                From = from,
                To = to,
                PackageUrn = package
            };

            // Act
            var response = await client.PostAsJsonAsync($"{Route}/accesspackages", request, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task AddPackage_WithWrongScope_ReturnsForbidden()
        {
            // Arrange
            var client = Fixture.Server.CreateClient();
            var token = TestTokenGenerator.CreateToken(new ClaimsIdentity("mock"), claims =>
            {
                claims.Add(new Claim(AltinnCoreClaimTypes.PartyUuid, TestData.MittRegnskap.Id.ToString()));
                claims.Add(new Claim("scope", "some:other:scope")); // Wrong scope
            });
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            ServiceOwnerConnectionPartyUrn.PersonId from = ServiceOwnerConnectionPartyUrn.PersonId.Create(PersonIdentifier.Parse(TestData.BjornMoe.Entity.PersonIdentifier));
            ServiceOwnerConnectionPartyUrn.PersonId to = ServiceOwnerConnectionPartyUrn.PersonId.Create(PersonIdentifier.Parse(TestData.LarsBakke.Entity.PersonIdentifier));
            AccessPackageUrn.AccessPackage package = AccessPackageUrn.AccessPackage.Create(new AccessPackageIdentifier("innbygger-skatteforhold-privatpersoner"));

            ServiceOwnerAccessPackageDelegation request = new()
            {
                From = from,
                To = to,
                PackageUrn = package
            };

            // Act
            var response = await client.PostAsJsonAsync($"{Route}/accesspackages", request, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task AddPackage_WithPackageNotInWhitelist_ReturnsForbidden()
        {
            // Arrange
            var client = CreateClient();

            ServiceOwnerConnectionPartyUrn.PersonId from = ServiceOwnerConnectionPartyUrn.PersonId.Create(PersonIdentifier.Parse(TestData.BjornMoe.Entity.PersonIdentifier));
            ServiceOwnerConnectionPartyUrn.PersonId to = ServiceOwnerConnectionPartyUrn.PersonId.Create(PersonIdentifier.Parse(TestData.LarsBakke.Entity.PersonIdentifier));
            AccessPackageUrn.AccessPackage package = AccessPackageUrn.AccessPackage.Create(new AccessPackageIdentifier("package-not-in-whitelist"));

            ServiceOwnerAccessPackageDelegation request = new()
            {
                From = from,
                To = to,
                PackageUrn = package
            };

            // Act
            var response = await client.PostAsJsonAsync($"{Route}/accesspackages", request, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task AddPackage_WithServiceOwnerNotInWhitelist_ReturnsForbidden()
        {
            // Arrange - Create client with a different organization that's not in the whitelist
            var client = Fixture.Server.CreateClient();
            var token = TestTokenGenerator.CreateToken(new ClaimsIdentity("mock"), claims =>
            {
                claims.Add(new Claim(AltinnCoreClaimTypes.Org, "OTHER"));
                claims.Add(new Claim("scope", AuthzConstants.SCOPE_SERVICEOWNER_PACKAGE_DELEGATION_WRITE));
                claims.Add(new Claim("consumer", GetConsumerClaimJson(TestData.BakerJohnsen.Entity.OrganizationIdentifier))); // Not in whitelist
            });
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            ServiceOwnerConnectionPartyUrn.PersonId from = ServiceOwnerConnectionPartyUrn.PersonId.Create(PersonIdentifier.Parse(TestData.BjornMoe.Entity.PersonIdentifier));
            ServiceOwnerConnectionPartyUrn.PersonId to = ServiceOwnerConnectionPartyUrn.PersonId.Create(PersonIdentifier.Parse(TestData.LarsBakke.Entity.PersonIdentifier));
            AccessPackageUrn.AccessPackage package = AccessPackageUrn.AccessPackage.Create(new AccessPackageIdentifier("innbygger-skatteforhold-privatpersoner"));

            ServiceOwnerAccessPackageDelegation request = new()
            {
                From = from,
                To = to,
                PackageUrn = package
            };

            // Act
            var response = await client.PostAsJsonAsync($"{Route}/accesspackages", request, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    #endregion
}
