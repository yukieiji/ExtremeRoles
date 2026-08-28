using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using ExtremeRoles.Compat;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ExtremeRoles.UnitTest;

public static class MockSetupHelper
{

    public static void SetupExtremeSystemTypeManagerMock()
    {
        var instanceField = typeof(ExtremeSystemTypeManager).GetField("instance", BindingFlags.NonPublic | BindingFlags.Static);
        if (instanceField != null)
        {
            var systemManager = (ExtremeSystemTypeManager)RuntimeHelpers.GetUninitializedObject(typeof(ExtremeSystemTypeManager));
            var allSystemsField = typeof(ExtremeSystemTypeManager).GetField("allSystems", BindingFlags.NonPublic | BindingFlags.Instance);
            if (allSystemsField != null && allSystemsField.GetValue(systemManager) == null)
            {
                allSystemsField.SetValue(systemManager, new System.Collections.Generic.Dictionary<ExtremeSystemType, ExtremeRoles.Module.Interface.IExtremeSystemType>());
            }
            instanceField.SetValue(null, systemManager);
        }
    }

    public static Mock<AmongUsClient> SetupAmongUsClientMock()
    {
        if (MockAmongUsClientget_InstanceHelper.Instance == null)
        {
            var mockClient = new Mock<AmongUsClient>(IntPtr.Zero);
            var mockClientHelper = new Mock<MockAmongUsClientget_InstanceHelper>();
            mockClientHelper.Setup(h => h.Invoke()).Returns(mockClient.Object);
            MockAmongUsClientget_InstanceHelper.Instance = mockClientHelper.Object;
            return mockClient;
        }
        return Mock<AmongUsClient>.Get(AmongUsClient.Instance);
    }

    public static Mock<LobbyBehaviour> SetupLobbyMock()
    {
        if (MockLobbyBehaviourget_InstanceHelper.Instance == null)
        {
            var mockLobby = new Mock<LobbyBehaviour>(IntPtr.Zero);
            var mockLobbyInstance = new Mock<MockLobbyBehaviourget_InstanceHelper>();
            mockLobbyInstance.Setup(x => x.Invoke()).Returns(mockLobby.Object);
            MockLobbyBehaviourget_InstanceHelper.Instance = mockLobbyInstance.Object;
            return mockLobby;
        }
        return Mock<LobbyBehaviour>.Get(LobbyBehaviour.Instance);
    }

    public static Mock<GameData> SetupGameDataMock()
    {
        var mockGameData = new Mock<GameData>(IntPtr.Zero);
        var mockList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>>(IntPtr.Zero);
        mockGameData.SetupGet(g => g.AllPlayers).Returns(mockList.Object);

        var mockGameDataHelper = new Mock<MockGameDataget_InstanceHelper>();
        mockGameDataHelper.Setup(h => h.Invoke()).Returns(mockGameData.Object);
        MockGameDataget_InstanceHelper.Instance = mockGameDataHelper.Object;

        return mockGameData;
    }

    public static void SetupDebugMode()
    {
        var debugModeProperty = typeof(ExtremeRolesPlugin).GetProperty("DebugMode", BindingFlags.Public | BindingFlags.Static);
        if (debugModeProperty != null && debugModeProperty.GetValue(null) == null)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var config = new ConfigFile(tempPath, true);
            var entry = config.Bind("DeBug", "DebugMode", false);
            debugModeProperty.SetValue(null, entry);
        }
    }

    public static void SetupConstantsHelpers()
    {
        var mockBroadcastHelper = new Mock<MockConstantsGetBroadcastVersionHelper>();
        mockBroadcastHelper.Setup(h => h.Invoke()).Returns(50000);
        MockConstantsGetBroadcastVersionHelper.Instance = mockBroadcastHelper.Object;
    }

    public static void SetupCompatModManager()
    {
        if (CompatModManager.Instance == null)
        {
            CompatModManager.Initialize();
        }
    }

	public static ExtremeRolesPlugin SetupMockExtremeRolePlugin()
	{
		if (ExtremeRolesPlugin.Instance == null)
		{
			var plugin = (ExtremeRolesPlugin)RuntimeHelpers.GetUninitializedObject(typeof(ExtremeRolesPlugin));
			var instanceField = typeof(ExtremeRolesPlugin).GetField("<Instance>k__BackingField", BindingFlags.NonPublic | BindingFlags.Static);
			instanceField?.SetValue(null, plugin);
		}
		return ExtremeRolesPlugin.Instance!;
	}

	public static void SetupLogger(string loggerName = "UnitTest")
	{
		var loggerField = typeof(ExtremeRolesPlugin).GetField("Logger", BindingFlags.NonPublic | BindingFlags.Static);
		if (loggerField != null && loggerField.GetValue(null) == null)
		{
			loggerField.SetValue(null, BepInEx.Logging.Logger.CreateLogSource("UnitTest"));
		}
	}

	public static void SetupMockConfig(ExtremeRolesPlugin plugin)
	{
		var config = new ConfigFile(Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.cfg"), true);
		var configField = typeof(BasePlugin).GetField("<Config>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
		configField?.SetValue(plugin, config);
	}

	public static void SetupMockHttps(ExtremeRolesPlugin plugin, HttpClient? client = null)
	{
		typeof(ExtremeRolesPlugin).GetField("<Http>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(plugin, client ?? new HttpClient());
	}

	public static Mock<T> SetupDestroyableSingletonMock<T>() where T : DestroyableSingleton<T>
	{
		var mock = new Mock<T>(IntPtr.Zero);
		var mockSingleton = new Mock<MockDestroyableSingletonget_InstanceHelper<T>>();
		MockDestroyableSingletonget_InstanceHelper<T>.Instance = mockSingleton.Object;
		mockSingleton.Setup(x => x.Invoke()).Returns(mock.Object);

		var mockExists = new Mock<MockDestroyableSingletonget_InstanceExistsHelper<T>>();
		MockDestroyableSingletonget_InstanceExistsHelper<T>.Instance = mockExists.Object;
		mockExists.Setup(x => x.Invoke()).Returns(true);

		return mock;
	}

    public static void SetupPaletteHelpers()
    {
        MockPaletteget_CrewmateBlueHelper.Instance = new Mock<MockPaletteget_CrewmateBlueHelper>().Object;
        MockPaletteget_ImpostorRedHelper.Instance = new Mock<MockPaletteget_ImpostorRedHelper>().Object;
        MockPaletteget_WhiteHelper.Instance = new Mock<MockPaletteget_WhiteHelper>().Object;
        MockPaletteget_ClearWhiteHelper.Instance = new Mock<MockPaletteget_ClearWhiteHelper>().Object;
        MockPaletteget_BlackHelper.Instance = new Mock<MockPaletteget_BlackHelper>().Object;

        var mockEnabledColor = new Mock<MockPaletteget_EnabledColorHelper>();
        mockEnabledColor.Setup(x => x.Invoke()).Returns(new Color(1f, 1f, 1f, 1f));
        MockPaletteget_EnabledColorHelper.Instance = mockEnabledColor.Object;

        var mockDisabledClear = new Mock<MockPaletteget_DisabledClearHelper>();
        mockDisabledClear.Setup(x => x.Invoke()).Returns(new Color(0f, 0f, 0f, 0f));
        MockPaletteget_DisabledClearHelper.Instance = mockDisabledClear.Object;

        var mockDisabledGrey = new Mock<MockPaletteget_DisabledGreyHelper>();
        mockDisabledGrey.Setup(x => x.Invoke()).Returns(new Color(0.5f, 0.5f, 0.5f, 1f));
        MockPaletteget_DisabledGreyHelper.Instance = mockDisabledGrey.Object;
    }
}
