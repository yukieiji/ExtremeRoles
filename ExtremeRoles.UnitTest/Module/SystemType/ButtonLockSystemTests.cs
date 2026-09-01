using System;
using ExtremeRoles.Module.SystemType;
using Hazel;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ButtonLockSystemTests
{
	public ButtonLockSystemTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
	}

	[Fact]
	public void FactoryAndIsLockedMethods_WorkCorrectly()
	{
		Assert.False(ButtonLockSystem.IsAbilityButtonLock());
		Assert.False(ButtonLockSystem.IsReportButtonLock());
		Assert.False(ButtonLockSystem.IsKillButtonLock());

		var abilitySys = ButtonLockSystem.CreateOrGetAbilityButtonLockSystem();
		var reportSys = ButtonLockSystem.CreateOrGetReportButtonLock();
		var killSys = ButtonLockSystem.CreateOrGetKillButtonLockSystem();

		Assert.NotNull(abilitySys);
		Assert.NotNull(reportSys);
		Assert.NotNull(killSys);

		abilitySys.Lock(1);
		Assert.True(ButtonLockSystem.IsAbilityButtonLock());

		reportSys.Lock(1);
		Assert.True(ButtonLockSystem.IsReportButtonLock());

		killSys.Lock(1);
		Assert.True(ButtonLockSystem.IsKillButtonLock());
	}

	[Fact]
	public void Lock_Unlock_ConditionFunc()
	{
		var system = new ButtonLockSystem(ExtremeSystemType.AbilityButtonLockSystem);

		// Default condition func returns true
		system.Lock(10);

		// Custom condition returns false -> lock won't block
		bool allowLock = false;
		system.AddCondition(20, () => allowLock);
		system.Lock(20);

		system.UnLock(10);
		// Since 20 wasn't added because func returned false, unlocking 10 removes all blocked conditions

		system.Lock(10);
		system.UnLock(10);
	}

	[Fact]
	public void Reset_DoesNotThrow()
	{
		var system = new ButtonLockSystem(ExtremeSystemType.AbilityButtonLockSystem);
		system.Reset(ResetTiming.MeetingStart, null);
	}

	[Fact]
	public void UpdateSystem_RpcLock_Unlock_Default()
	{
		var system = new ButtonLockSystem(ExtremeSystemType.AbilityButtonLockSystem);

		// Ops.RpcLock
		var readerLock = new Mock<MessageReader>();
		readerLock.Setup(r => r.ReadByte()).Returns((byte)ButtonLockSystem.Ops.RpcLock);
		readerLock.Setup(r => r.ReadPackedInt32()).Returns(100);

		system.UpdateSystem(null!, readerLock.Object);

		// Ops.Unlock
		var readerUnlock = new Mock<MessageReader>();
		readerUnlock.Setup(r => r.ReadByte()).Returns((byte)ButtonLockSystem.Ops.Unlock);
		readerUnlock.Setup(r => r.ReadPackedInt32()).Returns(100);

		system.UpdateSystem(null!, readerUnlock.Object);

		// Default
		var readerDefault = new Mock<MessageReader>();
		readerDefault.Setup(r => r.ReadByte()).Returns((byte)255);
		readerDefault.Setup(r => r.ReadPackedInt32()).Returns(100);

		system.UpdateSystem(null!, readerDefault.Object);
	}
}
