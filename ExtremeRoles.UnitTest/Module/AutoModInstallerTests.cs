using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExtremeRoles.Module;
using ExtremeRoles.UnitTest.Helper;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module;

public class AutoModInstallerTests
{
    public AutoModInstallerTests()
    {
        MockSetupHelper.SetupCommonMocks();
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
    public void AutoModInstaller_AddRepository_AddsRepositoryInstance()
    {
        var installer = new AutoModInstaller();
        var repo = new DummyRepositoryInfo();

        installer.AddRepository(repo);

        // AddMod for DummyRepositoryInfo should reuse the registered instance and add DllName
        installer.AddMod<DummyRepositoryInfo>("custom.dll");

        Assert.Contains("custom.dll", repo.DllName);
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
}
