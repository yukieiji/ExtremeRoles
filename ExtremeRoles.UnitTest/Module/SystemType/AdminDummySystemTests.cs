using System;
using System.Collections.Generic;
using ExtremeRoles.Module.SystemType;
using Hazel;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class AdminDummySystemTests
{
	public AdminDummySystemTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
	}

	[Fact]
	public void Get_And_TryGet_ReturnsInstance()
	{
		var system = AdminDummySystem.Get();
		Assert.NotNull(system);

		bool success = AdminDummySystem.TryGet(out var trySystem);
		Assert.True(success);
		Assert.Same(system, trySystem);
	}

	[Fact]
	public void Mode_GetSet()
	{
		var system = new AdminDummySystem();
		system.Mode = AdminDummySystem.DummyMode.Override;
		Assert.Equal(AdminDummySystem.DummyMode.Override, system.Mode);
	}

	[Fact]
	public void Add_Remove_TryGet_IsActive()
	{
		var system = new AdminDummySystem();
		Assert.False(system.IsActive);

		// Remove non-existent room
		system.Remove((SystemTypes)1, 1);
		system.Remove((SystemTypes)1, (IReadOnlyList<int>)[1]);

		// Add colors
		system.Add((SystemTypes)1, 10, 20);
		Assert.True(system.IsActive);

		bool found = system.TryGet((SystemTypes)1, out var colors);
		Assert.True(found);
		Assert.NotNull(colors);
		Assert.Equal(new List<int> { 10, 20 }, colors);

		bool notFound = system.TryGet((SystemTypes)2, out var noColors);
		Assert.False(notFound);
		Assert.Null(noColors);

		// Remove with params
		system.Remove((SystemTypes)1, 10);
		system.TryGet((SystemTypes)1, out colors);
		Assert.Equal(new List<int> { 20 }, colors);

		// Add another and remove with list
		system.Add((SystemTypes)1, 30);
		system.Remove((SystemTypes)1, (IReadOnlyList<int>)[20, 30]);
		system.TryGet((SystemTypes)1, out colors);
		Assert.Empty(colors!);
	}

	[Fact]
	public void Reset_ClearsOnPlayerResetTiming()
	{
		var system = new AdminDummySystem();
		system.Add((SystemTypes)1, 5);
		Assert.True(system.IsActive);

		system.Reset(ResetTiming.MeetingStart);
		Assert.True(system.IsActive);

		system.Reset(ResetTiming.OnPlayer);
		Assert.False(system.IsActive);
	}

	[Fact]
	public void UpdateSystem_Add_Remove_Default()
	{
		var system = new AdminDummySystem();

		// Add
		var readerAdd = new Mock<MessageReader>();
		readerAdd.SetupSequence(r => r.ReadByte())
			.Returns((byte)AdminDummySystem.Option.Add)
			.Returns((byte)SystemTypes.Admin);
		readerAdd.Setup(r => r.ReadPackedInt32()).Returns(100);

		system.UpdateSystem(null!, readerAdd.Object);
		Assert.True(system.TryGet(SystemTypes.Admin, out var colors));
		Assert.Contains(100, colors!);

		// Remove
		var readerRemove = new Mock<MessageReader>();
		readerRemove.SetupSequence(r => r.ReadByte())
			.Returns((byte)AdminDummySystem.Option.Remove)
			.Returns((byte)SystemTypes.Admin);

		system.UpdateSystem(null!, readerRemove.Object);

		// Default/Invalid option
		var readerInvalid = new Mock<MessageReader>();
		readerInvalid.SetupSequence(r => r.ReadByte())
			.Returns((byte)255)
			.Returns((byte)SystemTypes.Admin);

		system.UpdateSystem(null!, readerInvalid.Object);
	}
}
