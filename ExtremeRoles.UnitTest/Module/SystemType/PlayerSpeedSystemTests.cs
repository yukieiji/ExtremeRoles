using System;
using Hazel;
using ExtremeRoles.Module.SystemType;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class PlayerSpeedSystemTests : IDisposable
{
	public PlayerSpeedSystemTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupPlayerControlMocks();
	}

	public void Dispose()
	{
		MockSetupHelper.SetupUnityCommonMocks();
	}

	[Fact]
	public void SetSpeed_And_TryGetVelocity_ReturnsCorrectVelocity()
	{
		var system = new PlayerSpeedSystem();
		byte playerId = 1;

		bool hasSpeedInitially = system.TryGetVelocity(playerId, out float initialVel);
		Assert.False(hasSpeedInitially);
		Assert.Equal(1.0f, initialVel);

		system.SetSpeed(playerId, "OverLoader_Speed", 1.5f);
		bool hasSpeedAfterSet = system.TryGetVelocity(playerId, out float velAfterSet);
		Assert.True(hasSpeedAfterSet);
		Assert.Equal(1.5f, velAfterSet);

		system.SetSpeed(playerId, "Extra_Boost", 2.0f);
		system.TryGetVelocity(playerId, out float velCombined);
		Assert.Equal(3.0f, velCombined);

		system.RemoveSpeed(playerId, "OverLoader_Speed");
		system.TryGetVelocity(playerId, out float velAfterRemoveOne);
		Assert.Equal(2.0f, velAfterRemoveOne);

		system.RemoveSpeed(playerId, "Extra_Boost");
		bool hasSpeedAfterRemoveAll = system.TryGetVelocity(playerId, out float velAfterRemoveAll);
		Assert.False(hasSpeedAfterRemoveAll);
		Assert.Equal(1.0f, velAfterRemoveAll);
	}

	[Fact]
	public void ClearPlayerSpeed_RemovesAllPlayerSpeeds()
	{
		var system = new PlayerSpeedSystem();
		byte playerId = 2;

		system.SetSpeed(playerId, "Speed1", 1.2f);
		system.SetSpeed(playerId, "Speed2", 1.3f);

		Assert.True(system.TryGetVelocity(playerId, out _));

		system.ClearPlayerSpeed(playerId);

		Assert.False(system.TryGetVelocity(playerId, out float velocity));
		Assert.Equal(1.0f, velocity);
	}
}
