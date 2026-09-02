using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.GameMode;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class LiberalOptionTests
{
    public LiberalOptionTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        MockSetupHelper.SetupExtremeSystemTypeManagerMock();
        MockSetupHelper.SetupAmongUsClientMock();
        MockSetupHelper.SetupLobbyMock();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupLogger();
        MockSetupHelper.SetupDebugMode();
        MockSetupHelper.SetupMockConfig(plugin);
    }

    [Fact]
    public void Create_RegistersLiberalSettingsInCategory()
    {
        int testCategoryKey = 998811;

        var mockMaxSetting = new Mock<IOption>();
        mockMaxSetting.Setup(m => m.Value<int>()).Returns(5);

        using (var factory = OptionCategoryAssembler.CreateOptionCategory(testCategoryKey, "TestLiberalCategory"))
        {
            LiberalOption.Create(factory, mockMaxSetting.Object);
        }

        bool categoryExists = OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, testCategoryKey, out var cate);
        Assert.True(categoryExists);
        Assert.NotNull(cate);

        Assert.NotNull(cate.Get(LiberalGlobalSetting.WinMoney));
        Assert.NotNull(cate.Get(LiberalGlobalSetting.LiberalMilitantMini));
        Assert.NotNull(cate.Get(LiberalGlobalSetting.LiberalMilitantMax));

        var miniOption = cate.Get(LiberalGlobalSetting.LiberalMilitantMini);
        miniOption.Selection = 1;

        // Trigger OnValueChanged on maxSetting mock if set up
        mockMaxSetting.Raise(m => m.OnValueChanged += null);
    }
}
