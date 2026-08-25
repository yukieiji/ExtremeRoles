using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using ExtremeRoles.Compat;
using ExtremeRoles.Extension.Manager;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Microsoft.Extensions.DependencyInjection;
using Moq;
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
using xCloud;
using Xunit;

namespace ExtremeRoles.UnitTest.Module;

[Collection("UnityMock")]
public sealed class CustomRegionTests
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

	private readonly List<IRegionInfo> availableRegions = new();

	public CustomRegionTests()
	{
		MockSetupHelper.SetupCommonMocks();
		MockSetupHelper.SetupConstantsHelpers();
		MockSetupHelper.SetupLogger();

		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);
		MockSetupHelper.SetupMockHttps(plugin);

		if (ClientOption.Instance == null)
		{
			ClientOption.Create();
		}

		ResetCustomRegionCache();
		SetupCustomRegionProviderMock();
		SetupServerManagerMock();
	}

	private static void ResetCustomRegionCache()
	{
		var field = typeof(CustomRegion).GetField("curCustomRegion", BindingFlags.NonPublic | BindingFlags.Static);
		if (field?.GetValue(null) is System.Collections.IDictionary dict)
		{
			dict.Clear();
		}
	}

	private void SetupCustomRegionProviderMock()
	{
		var mockProvider = new Mock<ICustomRegionProvider>();
		mockProvider.Setup(p => p.Provide()).Returns(() =>
		{
			var mockTokyoRegion = new Mock<IRegionInfo>(IntPtr.Zero);
			mockTokyoRegion.SetupGet(r => r.Name).Returns(IRegionInfoExtension.ExROfficialServerTokyoManinName);
			mockTokyoRegion.SetupGet(r => r.TranslateName).Returns(StringNames.NoTranslation);

			var mockTokyoServer = new Mock<ServerInfo>(IntPtr.Zero);
			mockTokyoServer.SetupGet(s => s.Name).Returns(IRegionInfoExtension.ExROfficialServerTokyoManinName);
			mockTokyoServer.SetupGet(s => s.Ip).Returns("168.138.196.31");
			mockTokyoServer.SetupGet(s => s.Port).Returns((ushort)22023);
			mockTokyoServer.SetupGet(s => s.UseDtls).Returns(false);

			mockTokyoRegion.SetupGet(r => r.Servers).Returns(new Il2CppReferenceArray<ServerInfo>([mockTokyoServer.Object]));

			var mockCustomRegion = new Mock<IRegionInfo>(IntPtr.Zero);
			mockCustomRegion.SetupGet(r => r.Name).Returns(IRegionInfoExtension.FullCustomServerName);
			mockCustomRegion.SetupGet(r => r.TranslateName).Returns(StringNames.NoTranslation);

			var mockCustomServer = new Mock<ServerInfo>(IntPtr.Zero);
			mockCustomServer.SetupGet(s => s.Name).Returns(IRegionInfoExtension.FullCustomServerName);
			mockCustomServer.SetupGet(s => s.Ip).Returns(ClientOption.Instance?.Ip?.Value ?? "127.0.0.1");
			mockCustomServer.SetupGet(s => s.Port).Returns(ClientOption.Instance?.Port?.Value ?? (ushort)22023);
			mockCustomServer.SetupGet(s => s.UseDtls).Returns(false);

			mockCustomRegion.SetupGet(r => r.Servers).Returns(new Il2CppReferenceArray<ServerInfo>([mockCustomServer.Object]));

			return new List<IRegionInfo> { mockTokyoRegion.Object, mockCustomRegion.Object };
		});

		var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
		services.AddSingleton<ICustomRegionProvider>(mockProvider.Object);
		var serviceProvider = services.BuildServiceProvider();

		var backingField = typeof(ExtremeRolesPlugin).GetField("<Provider>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
		backingField?.SetValue(ExtremeRolesPlugin.Instance, serviceProvider);
	}

	private void SetupServerManagerMock()
	{
		availableRegions.Clear();
		var mockServerManager = MockSetupHelper.SetupDestroyableSingletonMock<ServerManager>();

		mockServerManager.SetupGet(m => m.AvailableRegions)
			.Returns(() => new Il2CppReferenceArray<IRegionInfo>(availableRegions.ToArray()));

		mockServerManager.SetupSet(m => m.AvailableRegions = It.IsAny<Il2CppReferenceArray<IRegionInfo>>())
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

		mockServerManager.Setup(m => m.AddOrUpdateRegion(It.IsAny<IRegionInfo>()))
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

	private static void SetMockHttpClient(Func<HttpRequestMessage, HttpResponseMessage> handler)
	{
		var customClient = new HttpClient(new MockHttpMessageHandler(handler));
		MockSetupHelper.SetupMockHttps(ExtremeRolesPlugin.Instance, customClient);
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

		var mockRegion = new Mock<IRegionInfo>(IntPtr.Zero);
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

		var mockOtherRegion = new Mock<IRegionInfo>(IntPtr.Zero);
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