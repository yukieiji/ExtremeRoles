using System.Collections.Generic;
using ExtremeRoles.Module.SystemType;
using Hazel;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class VoteSwapSystemTests
{
	public VoteSwapSystemTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
	}

	[Fact]
	public void CreateOrGet_And_TryGet_ReturnsInstance()
	{
		var system = VoteSwapSystem.CreateOrGet();
		Assert.NotNull(system);

		bool found = VoteSwapSystem.TryGet(out var trySys);
		Assert.True(found);
		Assert.Same(system, trySys);
	}

	[Fact]
	public void Swap_And_TryGetSwapTarget_WhenNoSwapList()
	{
		var voteInfo = new Dictionary<byte, int> { { 1, 2 }, { 2, 1 } };
		var result = VoteSwapSystem.Swap(voteInfo);
		Assert.Equal(voteInfo, result);

		bool found = VoteSwapSystem.TryGetSwapTarget(1, out byte target);
		Assert.False(found);
	}

	[Fact]
	public void RemovePlayerFromSwaps_RemovesPlayer()
	{
		VoteSwapSystem.RemovePlayerFromSwaps(1);
	}

	[Fact]
	public void Reset_ClearsSwapsOnMeetingStart()
	{
		var system = VoteSwapSystem.CreateOrGet();
		system.Reset(ResetTiming.OnPlayer, null);
	}

	[Fact]
	public void UpdateSystem_AddsSwap()
	{
		var system = VoteSwapSystem.CreateOrGet();

		var mockPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockPlayer.SetupGet(p => p.PlayerId).Returns((byte)10);

		var reader = new Mock<MessageReader>();
		reader.SetupSequence(r => r.ReadByte())
			.Returns((byte)1) // source
			.Returns((byte)2) // target
			.Returns((byte)VoteSwapSystem.ShowOps.Hide); // show
		reader.Setup(r => r.ReadPackedUInt32()).Returns(0xFFFFFFFF);

		system.UpdateSystem(mockPlayer.Object, reader.Object);
	}
}
