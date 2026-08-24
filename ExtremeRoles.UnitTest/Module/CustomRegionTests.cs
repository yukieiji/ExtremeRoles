using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using Xunit;
using ExtremeRoles.Compat;
using ExtremeRoles.Extension.Manager;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.UnitTest.Module;

[HarmonyPatch(typeof(ServerManager), nameof(ServerManager.Instance), MethodType.Getter)]
public static class ServerManagerInstancePatch
{
    public static bool Prefix(ref ServerManager __result)
    {
        if (CustomRegionTests.GlobalServerManagerMock != null)
        {
            __result = CustomRegionTests.GlobalServerManagerMock.Object;
            return false;
        }
        return true;
    }
}

[Collection("UnityMock")]
public sealed class CustomRegionTests : IDisposable
{
    private sealed class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request));
        }
    }

    public static Mock<ServerManager>? GlobalServerManagerMock { get; private set; }
    private static Harmony? harmonyInstance;
    private static readonly List<IRegionInfo> availableRegions = new();

    public CustomRegionTests()
    {
        MockSetupHelper.SetupCommonMocks();

        var loggerField = typeof(ExtremeRolesPlugin).GetField("Logger", BindingFlags.NonPublic | BindingFlags.Static);
        if (loggerField != null && loggerField.GetValue(null) == null)
        {
            loggerField.SetValue(null, BepInEx.Logging.Logger.CreateLogSource("UnitTest"));
        }

        if (ExtremeRolesPlugin.Instance == null)
        {
            var plugin = (ExtremeRolesPlugin)RuntimeHelpers.GetUninitializedObject(typeof(ExtremeRolesPlugin));
            var config = new ConfigFile("test.cfg", true);
            var configField = typeof(BasePlugin).GetField("<Config>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            configField?.SetValue(plugin, config);

            typeof(ExtremeRolesPlugin).GetField("<Http>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(plugin, new HttpClient());

            var instanceField = typeof(ExtremeRolesPlugin).GetField("<Instance>k__BackingField", BindingFlags.NonPublic | BindingFlags.Static);
            instanceField?.SetValue(null, plugin);
        }

        if (ClientOption.Instance == null)
        {
            ClientOption.Create();
        }

        SetupMockServerManager();
    }

    private static void SetupMockServerManager()
    {
        if (harmonyInstance == null)
        {
            harmonyInstance = new Harmony("test.customregion.patch");
            harmonyInstance.PatchAll(typeof(ServerManagerInstancePatch).Assembly);
        }

        if (GlobalServerManagerMock != null)
        {
            return;
        }

        availableRegions.Clear();
        GlobalServerManagerMock = new Mock<ServerManager>(IntPtr.Zero);
        GC.SuppressFinalize(GlobalServerManagerMock.Object);

        GlobalServerManagerMock.SetupGet(m => m.AvailableRegions)
            .Returns(() => new Il2CppReferenceArray<IRegionInfo>(availableRegions.ToArray()));

        GlobalServerManagerMock.SetupSet(m => m.AvailableRegions = It.IsAny<Il2CppReferenceArray<IRegionInfo>>())
            .Callback<Il2CppReferenceArray<IRegionInfo>>(arr =>
            {
                availableRegions.Clear();
                if (arr != null)
                {
                    foreach (var item in arr)
                    {
                        availableRegions.Add(item);
                    }
                }
            });

        GlobalServerManagerMock.Setup(m => m.AddOrUpdateRegion(It.IsAny<IRegionInfo>()))
            .Callback<IRegionInfo>(r =>
            {
                int existingIndex = availableRegions.FindIndex(x => x.Name == r.Name);
                if (existingIndex >= 0)
                {
                    availableRegions[existingIndex] = r;
                }
                else
                {
                    availableRegions.Add(r);
                }
            });
    }

    public void Dispose()
    {
        harmonyInstance?.UnpatchSelf();
        harmonyInstance = null;
    }

    private static void SetMockHttpClient(Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        var customClient = new HttpClient(new MockHttpMessageHandler(handler));
        typeof(ExtremeRolesPlugin).GetField("<Http>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(ExtremeRolesPlugin.Instance, customClient);
    }

    [Fact]
    public void RegionStatus_IsUpdate_ReturnsTrue_WhenOlderThanOneHour()
    {
        var pastTime = DateTime.UtcNow.AddHours(-2);
        var status = new RegionStatus(RegionStatusEnum.Ok, pastTime);

        Assert.True(status.IsUpdate());
        Assert.Equal(RegionStatusEnum.Ok, status.Status);
        Assert.Equal(pastTime, status.Time);
    }

    [Fact]
    public void RegionStatus_IsUpdate_ReturnsFalse_WhenWithinOneHour()
    {
        var recentTime = DateTime.UtcNow;
        var status = new RegionStatus(RegionStatusEnum.Ok, recentTime);

        Assert.False(status.IsUpdate());
    }

    [Fact]
    public void CustomRegion_TryGetStatus_UnregisteredRegion_ReturnsFalse()
    {
        bool result = CustomRegion.TryGetStatus("NonExistentRegion", out var status);

        Assert.False(result);
        Assert.Equal(RegionStatusEnum.None, status);
    }

    [Fact]
    public void CustomRegion_Add_RegistersRegionsAndGetStatusReturnsOk()
    {
        string json = "{\"status\":\"Ok\",\"version\":\"1.0\",\"post_info\":{\"version\":1,\"at\":\"2025-01-01T00:00:00Z\"}}";
        SetMockHttpClient(req =>
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });

        CustomRegion.Add();

        bool foundTokyo = CustomRegion.TryGetStatus(IRegionInfoExtension.ExROfficialServerTokyoManinName, out var tokyoStatus);
        bool foundCustom = CustomRegion.TryGetStatus(IRegionInfoExtension.FullCustomServerName, out var customStatus);

        Assert.True(foundTokyo);
        Assert.Equal(RegionStatusEnum.Ok, tokyoStatus);
        Assert.True(foundCustom);
        Assert.Equal(RegionStatusEnum.Ok, customStatus);

        Assert.NotNull(CustomRegion.EditableServer);
        Assert.Equal(IRegionInfoExtension.FullCustomServerName, CustomRegion.EditableServer.Name);
    }

    [Fact]
    public void CustomRegion_Add_WhenHttpReturnsInvalidJson_ReturnsDefaultStatusNone()
    {
        SetMockHttpClient(req =>
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Invalid JSON String", Encoding.UTF8, "application/json")
            };
        });

        CustomRegion.Add();

        bool foundTokyo = CustomRegion.TryGetStatus(IRegionInfoExtension.ExROfficialServerTokyoManinName, out var tokyoStatus);
        Assert.True(foundTokyo);
        Assert.Equal(RegionStatusEnum.None, tokyoStatus);
    }

    [Fact]
    public void CustomRegion_Add_WhenHttpThrowsException_ReturnsStatusNg()
    {
        SetMockHttpClient(req =>
        {
            throw new HttpRequestException("Network error");
        });

        CustomRegion.Add();

        bool foundTokyo = CustomRegion.TryGetStatus(IRegionInfoExtension.ExROfficialServerTokyoManinName, out var tokyoStatus);
        Assert.True(foundTokyo);
        Assert.Equal(RegionStatusEnum.Ng, tokyoStatus);
    }

    [Fact]
    public void CustomRegion_ReSelect_UpdatesServerManagerCurrentRegion()
    {
        string json = "{\"status\":\"Ok\",\"version\":\"1.0\",\"post_info\":{\"version\":1,\"at\":\"2025-01-01T00:00:00Z\"}}";
        SetMockHttpClient(req =>
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });

        CustomRegion.Add();

        var mockServerManager = new Mock<ServerManager>(IntPtr.Zero);
        GC.SuppressFinalize(mockServerManager.Object);

        var mockRegion = new Mock<IRegionInfo>();
        mockRegion.SetupGet(r => r.Name).Returns(IRegionInfoExtension.ExROfficialServerTokyoManinName);

        mockServerManager.SetupGet(m => m.CurrentRegion).Returns(mockRegion.Object);
        IRegionInfo? updatedRegion = null;
        mockServerManager.SetupSet(m => m.CurrentRegion = It.IsAny<IRegionInfo>())
            .Callback<IRegionInfo>(r =>
            {
                updatedRegion = r;
            });

        CustomRegion.ReSelect(mockServerManager.Object);

        Assert.NotNull(updatedRegion);
        Assert.Equal(IRegionInfoExtension.ExROfficialServerTokyoManinName, updatedRegion.Name);
    }

    [Fact]
    public void CustomRegion_UpdateEditorableRegion_FiltersAndReAddsRegions()
    {
        string json = "{\"status\":\"Ok\",\"version\":\"1.0\",\"post_info\":{\"version\":1,\"at\":\"2025-01-01T00:00:00Z\"}}";
        SetMockHttpClient(req =>
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });

        var mockOtherRegion = new Mock<IRegionInfo>();
        mockOtherRegion.SetupGet(r => r.Name).Returns("OtherServer");

        ServerManager.Instance.AddOrUpdateRegion(mockOtherRegion.Object);

        CustomRegion.UpdateEditorableRegion();

        Assert.Contains(ServerManager.Instance.AvailableRegions, r =>
        {
            return r.Name == "OtherServer";
        });
        Assert.Contains(ServerManager.Instance.AvailableRegions, r =>
        {
            return r.Name == IRegionInfoExtension.ExROfficialServerTokyoManinName;
        });
        Assert.Contains(ServerManager.Instance.AvailableRegions, r =>
        {
            return r.Name == IRegionInfoExtension.FullCustomServerName;
        });
    }
}
