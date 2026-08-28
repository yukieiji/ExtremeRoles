using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExtremeRoles.Module;
using ExtremeRoles.Module.JsonData;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
[Collection(nameof(MockSetupHelper.SetupLogger))]
public class AutoModInstallerTests
{
    private sealed class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _responseJson;

        public MockHttpMessageHandler(string responseJson)
        {
            _responseJson = responseJson;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_responseJson, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }

    public AutoModInstallerTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();

		MockSetupHelper.SetupLogger();

		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockHttps(plugin);
    }

    private sealed class DummyRepositoryInfo : AutoModInstaller.IRepositoryInfo
    {
        public List<string> DllName { get; } = new List<string>();

        public Task<IReadOnlyList<AutoModInstaller.DownloadData>> GetInstallData(AutoModInstaller.InstallType installType)
        {
            IReadOnlyList<AutoModInstaller.DownloadData> result = new List<AutoModInstaller.DownloadData>
            {
                new AutoModInstaller.DownloadData("https://example.com/mod.dll", "mod.dll")
            };
            return Task.FromResult(result);
        }
    }

    private sealed class AnotherDummyRepositoryInfo : AutoModInstaller.IRepositoryInfo
    {
        public List<string> DllName { get; } = new List<string>();

        public Task<IReadOnlyList<AutoModInstaller.DownloadData>> GetInstallData(AutoModInstaller.InstallType installType)
        {
            return Task.FromResult<IReadOnlyList<AutoModInstaller.DownloadData>>(new List<AutoModInstaller.DownloadData>());
        }
    }

    [Fact]
    public void DownloadData_ToString_ReturnsFormattedString()
    {
        var data = new AutoModInstaller.DownloadData("https://example.com/test.dll", "test.dll");

        Assert.Equal("https://example.com/test.dll", data.DownloadUrl);
        Assert.Equal("test.dll", data.DllName);
        Assert.Equal("DL URL:https://example.com/test.dll, DllName:test.dll", data.ToString());
    }

    [Fact]
    public void AutoModInstaller_InitialState_IsNotInit()
    {
        var installer = new AutoModInstaller();

        Assert.False(installer.IsInit);
    }

    [Fact]
    public void AutoModInstaller_SetInfoPopup_IsInitReturnsTrue()
    {
        var installer = new AutoModInstaller();
        var mockPopup = new Mock<GenericPopup>(IntPtr.Zero);

        installer.InfoPopup = mockPopup.Object;

        Assert.True(installer.IsInit);
    }

    [Fact]
    public void AutoModInstaller_AddMod_CreatesRepositoryIfNotExistsAndReusesIt()
    {
        var installer = new AutoModInstaller();
        var repo = new AnotherDummyRepositoryInfo();

        installer.AddRepository(repo);
        installer.AddMod<AnotherDummyRepositoryInfo>("newmod.dll");
        installer.AddMod<AnotherDummyRepositoryInfo>("another.dll");

        Assert.Equal(2, repo.DllName.Count);
        Assert.Equal("newmod.dll", repo.DllName[0]);
        Assert.Equal("another.dll", repo.DllName[1]);
    }

    [Fact]
    public void ExRRepositoryInfo_DefaultState_ContainsExtremeRolesDll()
    {
        var repo = new ExRRepositoryInfo();

        Assert.Single(repo.DllName);
        Assert.Equal("ExtremeRoles.dll", repo.DllName[0]);
    }

    [Fact]
    public async Task AutoModInstaller_Update_WhenInfoPopupIsNull_ReturnsWithoutError()
    {
        var installer = new AutoModInstaller();
        installer.InfoPopup = null;

        await installer.Update();

        Assert.False(installer.IsInit);
    }

    [Fact]
    public async Task ExRRepositoryInfo_GetInstallData_Update_FetchesLatestReleaseAndParsesData()
    {
        string json = "{\"tag_name\":\"v99999.0.0\",\"assets\":[{\"content_type\":\"application/octet-stream\",\"browser_download_url\":\"https://github.com/yukieiji/ExtremeRoles/releases/download/v99999.0.0/ExtremeRoles.dll\"}]}";
        var customClient = new HttpClient(new MockHttpMessageHandler(json));
		MockSetupHelper.SetupMockHttps(ExtremeRolesPlugin.Instance, customClient);

        var repo = new ExRRepositoryInfo();
        var result = await repo.GetInstallData(AutoModInstaller.InstallType.Update);

        Assert.Single(result);
        Assert.Equal("ExtremeRoles.dll", result[0].DllName);
        Assert.Equal("https://github.com/yukieiji/ExtremeRoles/releases/download/v99999.0.0/ExtremeRoles.dll", result[0].DownloadUrl);
    }

    [Fact]
    public async Task ExRRepositoryInfo_GetInstallData_Downgrade_FetchesReleasesAndParsesData()
    {
        string json = "[{\"tag_name\":\"v0.0.1\",\"assets\":[{\"content_type\":\"application/octet-stream\",\"browser_download_url\":\"https://github.com/yukieiji/ExtremeRoles/releases/download/v0.0.1/ExtremeRoles.dll\"}]}]";
        var customClient = new HttpClient(new MockHttpMessageHandler(json));
		MockSetupHelper.SetupMockHttps(ExtremeRolesPlugin.Instance, customClient);

		var repo = new ExRRepositoryInfo();
        var result = await repo.GetInstallData(AutoModInstaller.InstallType.Downgrade);

        Assert.Single(result);
        Assert.Equal("ExtremeRoles.dll", result[0].DllName);
        Assert.Equal("https://github.com/yukieiji/ExtremeRoles/releases/download/v0.0.1/ExtremeRoles.dll", result[0].DownloadUrl);
    }
}
