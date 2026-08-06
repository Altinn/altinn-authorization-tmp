using System.Text.Json;
using Altinn.Authorization.ABAC.Xacml.JsonProfile;
using Altinn.Platform.Authorization.Controllers;
using Altinn.Platform.Authorization.Models.External;
using Microsoft.AspNetCore.Http;

namespace Altinn.Authorization.Tests.Unit;

[UnitTest]
public class DecisionControllerTest
{
    [Fact]
    public void ApplyXForwardedForHeader_HeaderPresent_SetsExactValue()
    {
        XacmlJsonRequest request = new();
        IHeaderDictionary headers = CreateHeaders("18.203.138.153");

        DecisionController.ApplyXForwardedForHeader(request, headers);

        request.XForwardedForHeader.Should().Be("18.203.138.153");
    }

    [Fact]
    public void ApplyXForwardedForHeader_HeaderAbsent_SetsNull()
    {
        XacmlJsonRequest request = new();
        IHeaderDictionary headers = CreateHeaders(null);

        DecisionController.ApplyXForwardedForHeader(request, headers);

        request.XForwardedForHeader.Should().BeNull();
    }

    [Fact]
    public void ApplyXForwardedForHeader_MultiHopHeaderPresent_SetsValueVerbatim()
    {
        XacmlJsonRequest request = new();
        string chain = "203.0.113.7, 70.41.3.18, 150.172.238.178";
        IHeaderDictionary headers = CreateHeaders(chain);

        DecisionController.ApplyXForwardedForHeader(request, headers);

        request.XForwardedForHeader.Should().Be(chain);
    }

    /// <summary>
    /// Simulates what AuthorizeJsonRequest does: deserialize the internal /decision JSON
    /// body, then stamp the transport header. The body here carries a spoofed
    /// XForwardedForHeader value and no transport header is present, so the result must
    /// stay null end to end.
    /// </summary>
    [Fact]
    public void DeserializeThenApplyXForwardedForHeader_BodySuppliedValueNoTransportHeader_ResultStaysNull()
    {
        string body = """
            {
              "Request": {
                "AccessSubject": [],
                "Action": [],
                "Resource": [],
                "XForwardedForHeader": "203.0.113.99"
              }
            }
            """;

        XacmlJsonRequestRoot deserialized = JsonSerializer.Deserialize<XacmlJsonRequestRoot>(body);
        deserialized.Request.XForwardedForHeader.Should().BeNull("the internal request model ignores this property on deserialize");

        DecisionController.ApplyXForwardedForHeader(deserialized.Request, CreateHeaders(null));

        deserialized.Request.XForwardedForHeader.Should().BeNull();
    }

    /// <summary>
    /// Same simulated flow as above, but with a genuine transport header present that
    /// differs from the spoofed body value, so a bug that read the body value instead of
    /// the transport header would be caught.
    /// </summary>
    [Fact]
    public void DeserializeThenApplyXForwardedForHeader_BodySuppliedValueWithDifferentTransportHeader_ResultUsesTransportValueOnly()
    {
        string body = """
            {
              "Request": {
                "AccessSubject": [],
                "Action": [],
                "Resource": [],
                "XForwardedForHeader": "203.0.113.99"
              }
            }
            """;

        XacmlJsonRequestRoot deserialized = JsonSerializer.Deserialize<XacmlJsonRequestRoot>(body);

        DecisionController.ApplyXForwardedForHeader(deserialized.Request, CreateHeaders("18.203.138.153"));

        deserialized.Request.XForwardedForHeader.Should().Be("18.203.138.153");
    }

    /// <summary>
    /// Simulates what AuthorizeExternal does: map the external DTO, then stamp the
    /// transport header, proving the same stamp applies on the external /authorize path.
    /// </summary>
    [Fact]
    public void ToInternal_ThenApplyXForwardedForHeader_ExternalRequest_SetsValueFromTransportHeader()
    {
        XacmlJsonRequestRootExternal external = new()
        {
            Request = new XacmlJsonRequestExternal
            {
                Resource = [],
            },
        };

        XacmlJsonRequestRoot result = external.ToInternal();
        DecisionController.ApplyXForwardedForHeader(result.Request, CreateHeaders("18.203.138.153"));

        result.Request.XForwardedForHeader.Should().Be("18.203.138.153");
    }

    private static IHeaderDictionary CreateHeaders(string xForwardedForValue)
    {
        HttpContext httpContext = new DefaultHttpContext();
        if (!string.IsNullOrEmpty(xForwardedForValue))
        {
            httpContext.Request.Headers["X-Forwarded-For"] = xForwardedForValue;
        }

        return httpContext.Request.Headers;
    }
}
