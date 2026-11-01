using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using Hazel;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.Roles;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class InvisibleSystemTests
{
	public InvisibleSystemTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
	}

	[Fact]
	public void Reset_ClearsActiveInvisibles()
	{
		// Arrange
		var system = new InvisibleSystem();
		var p1 = new Mock<PlayerControl>(System.IntPtr.Zero);
		p1.SetupGet(p => p.PlayerId).Returns((byte)1);

		var reader = new Mock<MessageReader>();
		reader.Setup(r => r.ReadByte()).Returns((byte)1);
		reader.Setup(r => r.ReadBoolean()).Returns(true);

		system.UpdateSystem(p1.Object, reader.Object);
		Assert.True(system.IsInvisible(1));

		// Act
		system.Reset(ResetTiming.MeetingStart, null);

		// Assert
		Assert.False(system.IsInvisible(1));
	}

	[Fact]
	public void Reset_OnPlayer_ClearsActiveInvisibles()
	{
		// Arrange
		var system = new InvisibleSystem();
		var p1 = new Mock<PlayerControl>(System.IntPtr.Zero);
		p1.SetupGet(p => p.PlayerId).Returns((byte)1);

		var reader = new Mock<MessageReader>();
		reader.Setup(r => r.ReadByte()).Returns((byte)1);
		reader.Setup(r => r.ReadBoolean()).Returns(true);

		system.UpdateSystem(p1.Object, reader.Object);
		Assert.True(system.IsInvisible(1));

		// Act
		system.Reset(ResetTiming.OnPlayer, null);

		// Assert
		Assert.False(system.IsInvisible(1));
	}

	[Fact]
	public void Reset_OnOtherTimings_DoesNotClear()
	{
		// Arrange
		var system = new InvisibleSystem();
		var p1 = new Mock<PlayerControl>(System.IntPtr.Zero);
		p1.SetupGet(p => p.PlayerId).Returns((byte)1);

		var reader = new Mock<MessageReader>();
		reader.Setup(r => r.ReadByte()).Returns((byte)1);
		reader.Setup(r => r.ReadBoolean()).Returns(true);

		system.UpdateSystem(p1.Object, reader.Object);

		// Act
		system.Reset(ResetTiming.MeetingEnd, null);

		// Assert
		Assert.True(system.IsInvisible(1));
	}

	[Fact]
	public void MultiplePlayers_CanToggleInvisibilityIndependently()
	{
		// Arrange
		var system = new InvisibleSystem();
		var p1 = new Mock<PlayerControl>(System.IntPtr.Zero);
		p1.SetupGet(p => p.PlayerId).Returns((byte)1);
		var p2 = new Mock<PlayerControl>(System.IntPtr.Zero);
		p2.SetupGet(p => p.PlayerId).Returns((byte)2);

		var startReader1 = new Mock<MessageReader>();
		startReader1.Setup(r => r.ReadByte()).Returns((byte)1);
		startReader1.Setup(r => r.ReadBoolean()).Returns(true);

		var startReader2 = new Mock<MessageReader>();
		startReader2.Setup(r => r.ReadByte()).Returns((byte)2);
		startReader2.Setup(r => r.ReadBoolean()).Returns(true);

		system.UpdateSystem(p1.Object, startReader1.Object);
		system.UpdateSystem(p2.Object, startReader2.Object);

		Assert.True(system.IsInvisible(1));
		Assert.True(system.IsInvisible(2));

		var endReader1 = new Mock<MessageReader>();
		endReader1.Setup(r => r.ReadByte()).Returns((byte)1);
		endReader1.Setup(r => r.ReadBoolean()).Returns(false);

		// Act
		system.UpdateSystem(p1.Object, endReader1.Object);

		// Assert
		Assert.False(system.IsInvisible(1));
		Assert.True(system.IsInvisible(2));
	}

	[Fact]
	public void Update_WhenNoInvisibles_ExecutesWithoutError()
	{
		// Arrange
		var system = new InvisibleSystem();

		// Act
		system.Update();

		// Assert
		Assert.NotNull(system);
	}

	[Fact]
	public void IsInvisible_WhenPlayerNotRegistered_ReturnsFalse()
	{
		// Arrange
		var system = new InvisibleSystem();

		// Act
		bool isInvis = system.IsInvisible(99);

		// Assert
		Assert.False(isInvis);
	}
}
