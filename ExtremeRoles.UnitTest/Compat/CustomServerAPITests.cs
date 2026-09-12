using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExtremeRoles.Compat;
using Xunit;

namespace ExtremeRoles.UnitTest.Compat;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class CustomServerAPITests
{
    private sealed class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly string responseJson;

        public MockHttpMessageHandler(string json)
        {
            this.responseJson = json;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(this.responseJson, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }

    public CustomServerAPITests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        MockSetupHelper.SetupLogger();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockHttps(plugin);
    }

    [Fact]
    public void CustomServerPostInfo_ToString_ReturnsFormattedString()
    {
        var info = new CustomServerPostInfo
        {
            Version = 123,
            At = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        Assert.Contains("123", info.ToString());
    }

    [Fact]
    public async Task Post_ReturnsParsedResponse()
    {
        string json = "{\"status\":\"ok\",\"version\":\"1.0.0\",\"post_info\":{\"version\":123,\"at\":\"2025-01-01T00:00:00Z\"}}";
        var customClient = new HttpClient(new MockHttpMessageHandler(json));
        MockSetupHelper.SetupMockHttps(ExtremeRolesPlugin.Instance, customClient);

        var result = await CustomServerAPI.Post("example.com");

        Assert.NotNull(result);
        Assert.Equal("ok", result.Status);
        Assert.Equal("1.0.0", result.Version);
        Assert.NotNull(result.PostInfo);
        Assert.Equal(123, result.PostInfo.Version);
    }
}
