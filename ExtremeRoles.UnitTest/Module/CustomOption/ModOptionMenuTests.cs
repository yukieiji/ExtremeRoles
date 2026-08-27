using System;
using System.Runtime.CompilerServices;
using ExtremeRoles.Module.CustomOption;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.CustomOption;

[Collection("UnityMock")]
public class ModOptionMenuTests
{
    public ModOptionMenuTests()
    {
        MockSetupHelper.SetupCommonMocks();
        MockSetupHelper.SetupLogger();
        MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(ExtremeRolesPlugin.Instance);
        ClientOption.Create();

        SetupInstantiateMocks();
    }

    private static void SetupInstantiateMocks()
    {
        var mockInst10 = new Mock<MockObjectInstantiateHelper10>();
        mockInst10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
            .Returns((UnityEngine.Object src, Transform parent) =>
            {
                if (src == null) return null!;
                var go = (GameObject)RuntimeHelpers.GetUninitializedObject(typeof(GameObject));
                return src;
            });
        MockObjectInstantiateHelper10.Instance = mockInst10.Object;

        var mockInst5 = new Mock<MockObjectInstantiateHelper5>();
        mockInst5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
            .Returns((UnityEngine.Object src, Transform parent) => src);
        MockObjectInstantiateHelper5.Instance = mockInst5.Object;

        var mockInst = new Mock<MockObjectInstantiateHelper>();
        mockInst.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Vector3>(), It.IsAny<Quaternion>()))
            .Returns((UnityEngine.Object src, Vector3 pos, Quaternion rot) => src);
        MockObjectInstantiateHelper.Instance = mockInst.Object;
    }

    [Fact]
    public void ModOptionMenu_Hide_And_IsReCreate_ShouldWork()
    {
        var optionsMenu = (OptionsMenuBehaviour)RuntimeHelpers.GetUninitializedObject(typeof(OptionsMenuBehaviour));
        var buttonPrefab = (ToggleButtonBehaviour)RuntimeHelpers.GetUninitializedObject(typeof(ToggleButtonBehaviour));

        var transMock = new Mock<Transform>();
        var goMock = new Mock<GameObject>();

        var menu = (ModOptionMenu)RuntimeHelpers.GetUninitializedObject(typeof(ModOptionMenu));

        // Test Hide on null popUp or uninitialized
        menu.Hide();

        Assert.True(menu.IsReCreate);
    }
}
