#nullable enable

using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign;

[Collection("UnityMock")]
public class ExtremeRoleAssigneeTests
{
    public ExtremeRoleAssigneeTests()
    {
        MockSetupHelper.SetupCommonMocks();
        MockSetupHelper.SetupLogger();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);

        if (!OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, ExtremeRoles.Module.CustomOption.Implemented.PresetOption.CategoryId, out _))
        {
            OptionCreator.Create();
        }
    }

    [Fact]
    public void Constructor_SetsBuilder()
    {
        var mockBuilder = new Mock<IRoleAssignDataBuilder>();
        var assignee = new ExtremeRoleAssignee(mockBuilder.Object);

        Assert.NotNull(assignee);
    }
}
