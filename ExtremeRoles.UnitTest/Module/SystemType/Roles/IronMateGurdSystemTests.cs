using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using Hazel;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType.Roles;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class IronMateGurdSystemTests
{
	public IronMateGurdSystemTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
	}

	[Fact]
	public void SetUp_IsContains_TryGetShield()
	{
		var system = new IronMateGurdSystem(1.5f, 10.0f);
		Assert.False(system.IsContains(1));
		Assert.False(system.TryGetShield(1, out _));

		system.SetUp(1, 2);
		Assert.True(system.IsContains(1));
		bool hasShield = system.TryGetShield(1, out int count);
		Assert.True(hasShield);
		Assert.Equal(2, count);
	}

	[Fact]
	public void UpdateSystem_UpdatesShield()
	{
		var system = new IronMateGurdSystem(1.5f, 10.0f);

		var mockPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockPlayer.SetupGet(p => p.PlayerId).Returns((byte)10); // different from target player id 1

		var reader = new Mock<MessageReader>();
		reader.Setup(r => r.ReadByte()).Returns((byte)1);
		reader.Setup(r => r.ReadPackedInt32()).Returns(3);

		system.UpdateSystem(mockPlayer.Object, reader.Object);
		Assert.True(system.TryGetShield(1, out int count));
		Assert.Equal(3, count);
	}
}
