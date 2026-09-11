using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using Hazel;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.Roles;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class TuckerShadowSystemTests
{
	public TuckerShadowSystemTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();

		var mockMeetingHudHelper = new Mock<IMockMeetingHudget_Instance>();
		mockMeetingHudHelper.Setup(x => x.Invoke()).Returns((MeetingHud)null!);
		IMockMeetingHudget_Instance.Instance = mockMeetingHudHelper.Object;

		var mockExileHelper = new Mock<IMockExileControllerget_Instance>();
		mockExileHelper.Setup(x => x.Invoke()).Returns((ExileController)null!);
		IMockExileControllerget_Instance.Instance = mockExileHelper.Object;
	}

	[Fact]
	public void Enable_Disable_Deteriorate_MarkClean()
	{
		var system = new TuckerShadowSystem(1.0f, 10.0f, 5.0f, true);
		Assert.False(system.IsDirty);

		system.Enable(1);
		system.Deteriorate(1.0f);
		system.Disable(1);

		system.MarkClean();
		system.Reset(ResetTiming.MeetingStart, null);

		var writer = new Mock<MessageWriter>(System.IntPtr.Zero);
		system.Serialize(writer.Object, true);
	}
}
