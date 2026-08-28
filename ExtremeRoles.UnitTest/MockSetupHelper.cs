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
    public static void SetupCommonMocks()
    {
        SetupColorHelpers();
        SetupPaletteHelpers();
        SetupMathfHelpers();
        SetupCompatModManager();
        SetupUnityObjectOperators();
        SetupVector2Helpers();

        SetupGameDataMock();
        SetupExtremeSystemTypeManagerMock();
    }

    public static void SetupExtremeSystemTypeManagerMock()
    {
        var instanceField = typeof(ExtremeSystemTypeManager).GetField("instance", BindingFlags.NonPublic | BindingFlags.Static);
        if (instanceField != null && instanceField.GetValue(null) == null)
        {
            var systemManager = (ExtremeSystemTypeManager)RuntimeHelpers.GetUninitializedObject(typeof(ExtremeSystemTypeManager));
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

    public static void SetupVector2Helpers()
    {
        var mockRight = new Mock<MockVector2get_rightHelper>();
        mockRight.Setup(x => x.Invoke()).Returns(new Vector2(1f, 0f));
        MockVector2get_rightHelper.Instance = mockRight.Object;
        var mockRightVec = new Mock<MockVector2get_rightVectorHelper>();
        mockRightVec.Setup(x => x.Invoke()).Returns(new Vector2(1f, 0f));
        MockVector2get_rightVectorHelper.Instance = mockRightVec.Object;

        var mockUp = new Mock<MockVector2get_upHelper>();
        mockUp.Setup(x => x.Invoke()).Returns(new Vector2(0f, 1f));
        MockVector2get_upHelper.Instance = mockUp.Object;
        var mockUpVec = new Mock<MockVector2get_upVectorHelper>();
        mockUpVec.Setup(x => x.Invoke()).Returns(new Vector2(0f, 1f));
        MockVector2get_upVectorHelper.Instance = mockUpVec.Object;

        var mockZero = new Mock<MockVector2get_zeroHelper>();
        mockZero.Setup(x => x.Invoke()).Returns(new Vector2(0f, 0f));
        MockVector2get_zeroHelper.Instance = mockZero.Object;
        var mockZeroVec = new Mock<MockVector2get_zeroVectorHelper>();
        mockZeroVec.Setup(x => x.Invoke()).Returns(new Vector2(0f, 0f));
        MockVector2get_zeroVectorHelper.Instance = mockZeroVec.Object;

        var mockDown = new Mock<MockVector2get_downHelper>();
        mockDown.Setup(x => x.Invoke()).Returns(new Vector2(0f, -1f));
        MockVector2get_downHelper.Instance = mockDown.Object;
        var mockDownVec = new Mock<MockVector2get_downVectorHelper>();
        mockDownVec.Setup(x => x.Invoke()).Returns(new Vector2(0f, -1f));
        MockVector2get_downVectorHelper.Instance = mockDownVec.Object;

        var mockOne = new Mock<MockVector2get_oneHelper>();
        mockOne.Setup(x => x.Invoke()).Returns(new Vector2(1f, 1f));
        MockVector2get_oneHelper.Instance = mockOne.Object;
        var mockOneVec = new Mock<MockVector2get_oneVectorHelper>();
        mockOneVec.Setup(x => x.Invoke()).Returns(new Vector2(1f, 1f));
        MockVector2get_oneVectorHelper.Instance = mockOneVec.Object;

        var mockMultiply = new Mock<MockVector2op_MultiplyHelper>();
        mockMultiply.Setup(x => x.Invoke(It.IsAny<Vector2>(), It.IsAny<Vector2>()))
            .Returns((Vector2 a, Vector2 b) => new Vector2(a.x * b.x, a.y * b.y));
        MockVector2op_MultiplyHelper.Instance = mockMultiply.Object;

        var mockMultiply2 = new Mock<MockVector2op_MultiplyHelper2>();
        mockMultiply2.Setup(x => x.Invoke(It.IsAny<Vector2>(), It.IsAny<float>()))
            .Returns((Vector2 v, float f) => new Vector2(v.x * f, v.y * f));
        MockVector2op_MultiplyHelper2.Instance = mockMultiply2.Object;

        var mockMultiply3 = new Mock<MockVector2op_MultiplyHelper3>();
        mockMultiply3.Setup(x => x.Invoke(It.IsAny<float>(), It.IsAny<Vector2>()))
            .Returns((float f, Vector2 v) => new Vector2(v.x * f, v.y * f));
        MockVector2op_MultiplyHelper3.Instance = mockMultiply3.Object;

        var mockVec2Implicit = new Mock<MockVector2op_ImplicitHelper>();
        mockVec2Implicit.Setup(x => x.Invoke(It.IsAny<Vector3>()))
            .Returns((Vector3 v) => new Vector2(v.x, v.y));
        MockVector2op_ImplicitHelper.Instance = mockVec2Implicit.Object;

        var mockVec2Implicit2 = new Mock<MockVector2op_ImplicitHelper2>();
        mockVec2Implicit2.Setup(x => x.Invoke(It.IsAny<Vector2>()))
            .Returns((Vector2 v) => new Vector3(v.x, v.y, 0f));
        MockVector2op_ImplicitHelper2.Instance = mockVec2Implicit2.Object;
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

	public static void SetupMathfHelpers()
    {
        var mockClamp01 = new Mock<MockMathfClamp01Helper>();
        mockClamp01.Setup(h => h.Invoke(It.IsAny<float>())).Returns((float f) => Math.Clamp(f, 0f, 1f));
        MockMathfClamp01Helper.Instance = mockClamp01.Object;

        var mockClamp = new Mock<MockMathfClampHelper>();
        mockClamp.Setup(h => h.Invoke(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>())).Returns((float v, float min, float max) => Math.Clamp(v, min, max));
        MockMathfClampHelper.Instance = mockClamp.Object;

        var mockClamp2 = new Mock<MockMathfClampHelper2>();
        mockClamp2.Setup(h => h.Invoke(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Returns((int v, int min, int max) => Math.Clamp(v, min, max));
        MockMathfClampHelper2.Instance = mockClamp2.Object;

        var mockMax = new Mock<MockMathfMaxHelper>();
        mockMax.Setup(h => h.Invoke(It.IsAny<float>(), It.IsAny<float>())).Returns((float a, float b) => Math.Max(a, b));
        MockMathfMaxHelper.Instance = mockMax.Object;

        var mockMin = new Mock<MockMathfMinHelper>();
        mockMin.Setup(h => h.Invoke(It.IsAny<float>(), It.IsAny<float>())).Returns((float a, float b) => Math.Min(a, b));
        MockMathfMinHelper.Instance = mockMin.Object;

        var mockAbs = new Mock<MockMathfAbsHelper>();
        mockAbs.Setup(h => h.Invoke(It.IsAny<float>())).Returns((float f) => Math.Abs(f));
        MockMathfAbsHelper.Instance = mockAbs.Object;

        var mockCeilToInt = new Mock<MockMathfCeilToIntHelper>();
        mockCeilToInt.Setup(h => h.Invoke(It.IsAny<float>())).Returns((float f) => (int)Math.Ceiling(f));
        MockMathfCeilToIntHelper.Instance = mockCeilToInt.Object;
    }

    public static void SetupColorHelpers()
    {
        var mockColorEq = new Mock<MockColorop_EqualityHelper>();
        mockColorEq.Setup(x => x.Invoke(It.IsAny<Color>(), It.IsAny<Color>()))
            .Returns((Color a, Color b) => a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a);
        MockColorop_EqualityHelper.Instance = mockColorEq.Object;

        var mockColorIneq = new Mock<MockColorop_InequalityHelper>();
        mockColorIneq.Setup(x => x.Invoke(It.IsAny<Color>(), It.IsAny<Color>()))
            .Returns((Color a, Color b) => a.r != b.r || a.g != b.g || a.b != b.b || a.a != b.a);
        MockColorop_InequalityHelper.Instance = mockColorIneq.Object;

        var mockRandomInitState = new Mock<MockRandomInitStateHelper>();
        MockRandomInitStateHelper.Instance = mockRandomInitState.Object;

        MockColor32op_ImplicitHelper.Instance = new Mock<MockColor32op_ImplicitHelper>().Object;
        MockColor32op_ImplicitHelper2.Instance = new Mock<MockColor32op_ImplicitHelper2>().Object;
        MockColorop_ImplicitHelper.Instance = new Mock<MockColorop_ImplicitHelper>().Object;
        MockColorop_ImplicitHelper2.Instance = new Mock<MockColorop_ImplicitHelper2>().Object;

        MockColorget_blackHelper.Instance = new Mock<MockColorget_blackHelper>().Object;
        MockColorget_blueHelper.Instance = new Mock<MockColorget_blueHelper>().Object;
        MockColorget_clearHelper.Instance = new Mock<MockColorget_clearHelper>().Object;
        MockColorget_cyanHelper.Instance = new Mock<MockColorget_cyanHelper>().Object;
        MockColorget_grayHelper.Instance = new Mock<MockColorget_grayHelper>().Object;
        MockColorget_greenHelper.Instance = new Mock<MockColorget_greenHelper>().Object;
        MockColorget_greyHelper.Instance = new Mock<MockColorget_greyHelper>().Object;
        MockColorget_magentaHelper.Instance = new Mock<MockColorget_magentaHelper>().Object;
        MockColorget_redHelper.Instance = new Mock<MockColorget_redHelper>().Object;
        MockColorget_whiteHelper.Instance = new Mock<MockColorget_whiteHelper>().Object;
        MockColorget_yellowHelper.Instance = new Mock<MockColorget_yellowHelper>().Object;
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

    public static void SetupUnityObjectOperators()
    {
        var mockEq = new Mock<MockObjectop_EqualityHelper>();
        mockEq.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<UnityEngine.Object>()))
            .Returns((UnityEngine.Object x, UnityEngine.Object y) =>
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }
                if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                {
                    return false;
                }
                return ReferenceEquals(x, y);
            });
        MockObjectop_EqualityHelper.Instance = mockEq.Object;

        var mockIneq = new Mock<MockObjectop_InequalityHelper>();
        mockIneq.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<UnityEngine.Object>()))
            .Returns((UnityEngine.Object x, UnityEngine.Object y) =>
            {
                if (ReferenceEquals(x, y))
                {
                    return false;
                }
                if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                {
                    return true;
                }
                return !ReferenceEquals(x, y);
            });
        MockObjectop_InequalityHelper.Instance = mockIneq.Object;

        var mockImplicit = new Mock<MockObjectop_ImplicitHelper>();
        mockImplicit.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>()))
            .Returns((UnityEngine.Object obj) => !ReferenceEquals(obj, null));
        MockObjectop_ImplicitHelper.Instance = mockImplicit.Object;

        var mockUnityActionImplicit = new Mock<MockUnityActionop_ImplicitHelper>();
        mockUnityActionImplicit.Setup(x => x.Invoke(It.IsAny<Action>()))
            .Returns((Action act) => act != null ? new UnityEngine.Events.UnityAction(IntPtr.Zero) : null!);
        MockUnityActionop_ImplicitHelper.Instance = mockUnityActionImplicit.Object;

        var mockDestroy = new Mock<MockObjectDestroyHelper>();
        mockDestroy.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<float>()));
        MockObjectDestroyHelper.Instance = mockDestroy.Object;

        var mockDestroy2 = new Mock<MockObjectDestroyHelper2>();
        mockDestroy2.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>()));
        MockObjectDestroyHelper2.Instance = mockDestroy2.Object;

        var mockMiscDestroy = new Mock<MockMiscDestroyHelper>();
        mockMiscDestroy.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>()));
        MockMiscDestroyHelper.Instance = mockMiscDestroy.Object;

        var mockFindObjects = new Mock<MockObjectFindObjectsOfTypeHelper>();
        mockFindObjects.Setup(x => x.Invoke(It.IsAny<Il2CppSystem.Type>())).Returns(new Il2CppReferenceArray<UnityEngine.Object>(IntPtr.Zero));
        MockObjectFindObjectsOfTypeHelper.Instance = mockFindObjects.Object;

        var mockFindObjects2 = new Mock<MockObjectFindObjectsOfTypeHelper2>();
        mockFindObjects2.Setup(x => x.Invoke(It.IsAny<Il2CppSystem.Type>(), It.IsAny<bool>())).Returns((Il2CppReferenceArray<UnityEngine.Object>)null!);
        MockObjectFindObjectsOfTypeHelper2.Instance = mockFindObjects2.Object;

        MockObjectFindObjectsOfTypeHelper3.Instance = new Mock<MockObjectFindObjectsOfTypeHelper3>().Object;
        MockObjectFindObjectsOfTypeHelper4.Instance = new Mock<MockObjectFindObjectsOfTypeHelper4>().Object;
    }
}
