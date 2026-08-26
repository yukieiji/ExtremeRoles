using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using ExtremeRoles.Compat;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.UI;

namespace ExtremeRoles.UnitTest;

public static class MockSetupHelper
{
    public static void SetupCommonMocks()
    {
        SetupVectorHelpers();
        SetupColorHelpers();
        SetupPaletteHelpers();
        SetupMathfHelpers();
        SetupCompatModManager();
        SetupUnityObjectOperators();
        SetupPlayerTaskHelpers();
    }

    public static void SetupPlayerTaskHelpers()
    {
        var mockHasTask = new Mock<MockPlayerTaskPlayerHasTaskOfTypeHelper>();
        MockPlayerTaskPlayerHasTaskOfTypeHelper.Instance = mockHasTask.Object;
    }

    public static void SetupVectorHelpers()
    {
        var mockZero = new Mock<MockVector3get_zeroHelper>();
        mockZero.Setup(h => h.Invoke()).Returns(new Vector3(0f, 0f, 0f));
        MockVector3get_zeroHelper.Instance = mockZero.Object;

        var mockUp = new Mock<MockVector3get_upHelper>();
        mockUp.Setup(h => h.Invoke()).Returns(new Vector3(0f, 1f, 0f));
        MockVector3get_upHelper.Instance = mockUp.Object;

        var mockOne = new Mock<MockVector3get_oneHelper>();
        mockOne.Setup(h => h.Invoke()).Returns(new Vector3(1f, 1f, 1f));
        MockVector3get_oneHelper.Instance = mockOne.Object;

        var mockAdd = new Mock<MockVector3op_AdditionHelper>();
        mockAdd.Setup(h => h.Invoke(It.IsAny<Vector3>(), It.IsAny<Vector3>()))
            .Returns((Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z));
        MockVector3op_AdditionHelper.Instance = mockAdd.Object;

        var mockMult = new Mock<MockVector3op_MultiplyHelper>();
        mockMult.Setup(h => h.Invoke(It.IsAny<Vector3>(), It.IsAny<float>()))
            .Returns((Vector3 a, float d) => new Vector3(a.x * d, a.y * d, a.z * d));
        MockVector3op_MultiplyHelper.Instance = mockMult.Object;

        var mockMult2 = new Mock<MockVector3op_MultiplyHelper2>();
        mockMult2.Setup(h => h.Invoke(It.IsAny<float>(), It.IsAny<Vector3>()))
            .Returns((float d, Vector3 a) => new Vector3(a.x * d, a.y * d, a.z * d));
        MockVector3op_MultiplyHelper2.Instance = mockMult2.Object;
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

			var shipStateField = typeof(ExtremeRolesPlugin).GetField("<ShipState>k__BackingField", BindingFlags.NonPublic | BindingFlags.Static);
			if (shipStateField != null && shipStateField.GetValue(null) == null)
			{
				var shipState = (ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus)FormatterServices.GetUninitializedObject(typeof(ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus));
				var deadInfoField = typeof(ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus).GetField("deadPlayerInfo", BindingFlags.NonPublic | BindingFlags.Instance);
				deadInfoField?.SetValue(shipState, new Dictionary<byte, ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus.DeadInfo>());
				var deadedAssassinField = typeof(ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus).GetField("deadedAssassin", BindingFlags.NonPublic | BindingFlags.Instance);
				deadedAssassinField?.SetValue(shipState, new Queue<byte>());
				var plusWinnerField = typeof(ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus).GetField("plusWinner", BindingFlags.NonPublic | BindingFlags.Instance);
				plusWinnerField?.SetValue(shipState, new List<NetworkedPlayerInfo>());
				var versionField = typeof(ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus).GetField("playerVersion", BindingFlags.NonPublic | BindingFlags.Instance);
				versionField?.SetValue(shipState, new Dictionary<int, Version>());
				shipStateField.SetValue(null, shipState);
			}
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
		var config = new ConfigFile(Path.Combine(Path.GetTempPath(), "test.cfg"), true);
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
		mockExists.Setup(x => x.Invoke()).Returns(() =>
		{
			var helper = MockDestroyableSingletonget_InstanceHelper<T>.Instance;
			return helper != null && helper.Invoke() != null;
		});
		MockDestroyableSingletonget_InstanceExistsHelper<T>.Instance = mockExists.Object;

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
        MockColor32op_ImplicitHelper.Instance = new Mock<MockColor32op_ImplicitHelper>().Object;
        MockColor32op_ImplicitHelper2.Instance = new Mock<MockColor32op_ImplicitHelper2>().Object;
        MockColorop_ImplicitHelper.Instance = new Mock<MockColorop_ImplicitHelper>().Object;
        MockColorop_ImplicitHelper2.Instance = new Mock<MockColorop_ImplicitHelper2>().Object;

        var mockColorDiv = new Mock<MockColorop_DivisionHelper>();
        mockColorDiv.Setup(h => h.Invoke(It.IsAny<Color>(), It.IsAny<float>()))
            .Returns((Color c, float b) => new Color(c.r / b, c.g / b, c.b / b, c.a / b));
        MockColorop_DivisionHelper.Instance = mockColorDiv.Object;

        var mockColorAdd = new Mock<MockColorop_AdditionHelper>();
        mockColorAdd.Setup(h => h.Invoke(It.IsAny<Color>(), It.IsAny<Color>()))
            .Returns((Color a, Color b) => new Color(a.r + b.r, a.g + b.g, a.b + b.b, a.a + b.a));
        MockColorop_AdditionHelper.Instance = mockColorAdd.Object;

        var mockColorEq = new Mock<MockColorop_EqualityHelper>();
        mockColorEq.Setup(h => h.Invoke(It.IsAny<Color>(), It.IsAny<Color>()))
            .Returns((Color a, Color b) => a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a);
        MockColorop_EqualityHelper.Instance = mockColorEq.Object;

        var mockColorIneq = new Mock<MockColorop_InequalityHelper>();
        mockColorIneq.Setup(h => h.Invoke(It.IsAny<Color>(), It.IsAny<Color>()))
            .Returns((Color a, Color b) => !(a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a));
        MockColorop_InequalityHelper.Instance = mockColorIneq.Object;

        var mockToHtml = new Mock<MockColorUtilityToHtmlStringRGBAHelper>();
        mockToHtml.Setup(h => h.Invoke(It.IsAny<Color>())).Returns("FFFFFFFF");
        MockColorUtilityToHtmlStringRGBAHelper.Instance = mockToHtml.Object;

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
