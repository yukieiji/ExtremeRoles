using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using Hazel;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.Roles;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class YokoYashiroSystemTests
{
	public YokoYashiroSystemTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
	}

	[Fact]
	public void GetNextStatus_CanSet_IsNearActiveYashiro()
	{
		var system = new YokoYashiroSystem(10.0f, 15.0f, 2.0f, false);
		Assert.False(system.IsDirty);

		Assert.Equal(YokoYashiroSystem.YashiroInfo.StatusType.YashiroActive, YokoYashiroSystem.GetNextStatus(YokoYashiroSystem.YashiroInfo.StatusType.YashiroDeactive));
		Assert.Equal(YokoYashiroSystem.YashiroInfo.StatusType.YashiroSeal, YokoYashiroSystem.GetNextStatus(YokoYashiroSystem.YashiroInfo.StatusType.YashiroActive));
		Assert.Equal(YokoYashiroSystem.YashiroInfo.StatusType.YashiroDeactive, YokoYashiroSystem.GetNextStatus(YokoYashiroSystem.YashiroInfo.StatusType.YashiroSeal));

		Assert.True(system.CanSet(Vector2.zero));
		Assert.False(system.IsNearActiveYashiro(Vector2.zero));

		system.MarkClean();
		system.Reset(ResetTiming.MeetingStart, null);

		var writer = new Mock<MessageWriter>(System.IntPtr.Zero);
		system.Serialize(writer.Object, true);
		writer.Verify(w => w.WritePacked(0), Times.Once);
	}
}
