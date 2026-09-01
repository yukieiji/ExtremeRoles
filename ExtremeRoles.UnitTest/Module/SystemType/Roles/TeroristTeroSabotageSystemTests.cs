using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.Roles;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class TeroristTeroSabotageSystemTests
{
	public TeroristTeroSabotageSystemTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
	}

	[Fact]
	public void BasicMethods_And_Clear()
	{
		var mockSoundProvider = new Mock<ISoundProvider>();
		mockSoundProvider.Setup(s => s.GetAudio(It.IsAny<ExtremeRoles.Helper.Sound.Type>())).Returns((AudioClip)null!);

		var minigameOpt = new TeroristTeroSabotageSystem.MinigameOption(5.0f, true, 10.0f);
		var option = new TeroristTeroSabotageSystem.Option(60.0f, 3, minigameOpt);

		var system = new TeroristTeroSabotageSystem(option, true, mockSoundProvider.Object);
		Assert.False(system.IsActive);
		Assert.False(system.IsDirty);

		system.MarkClean();
		system.Reset(ResetTiming.MeetingStart, null);
	}
}
