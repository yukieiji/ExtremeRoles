using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;
using Hazel;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.SystemType;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ExtremeSystemTypeManagerTests
{
	private sealed class DummySystem : IExtremeSystemType
	{
		public bool ResetCalled { get; private set; }
		public ResetTiming LastResetTiming { get; private set; }
		public PlayerControl? LastResetPlayer { get; private set; }
		public PlayerControl? LastUpdatedPlayer { get; private set; }
		public bool UpdateCalled { get; private set; }

		public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
		{
			ResetCalled = true;
			LastResetTiming = timing;
			LastResetPlayer = resetPlayer;
		}

		public void UpdateSystem(PlayerControl player, MessageReader msgReader)
		{
			UpdateCalled = true;
			LastUpdatedPlayer = player;
		}
	}

	private sealed class DummyDirtableSystem : IDirtableSystemType
	{
		public bool IsDirty { get; set; }
		public bool DeteriorateCalled { get; private set; }
		public bool MarkCleanCalled { get; private set; }
		public bool DeserializeCalled { get; private set; }
		public bool SerializeCalled { get; private set; }

		public void Deteriorate(float deltaTime)
		{
			DeteriorateCalled = true;
		}

		public void MarkClean()
		{
			MarkCleanCalled = true;
			IsDirty = false;
		}

		public void Deserialize(MessageReader reader, bool initialState)
		{
			DeserializeCalled = true;
		}

		public void Serialize(MessageWriter writer, bool initialState)
		{
			SerializeCalled = true;
		}

		public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null) { }
		public void UpdateSystem(PlayerControl player, MessageReader msgReader) { }
	}

	private sealed class DummySabotageSystem : ISabotageExtremeSystemType
	{
		public bool IsBlockOtherSabotage { get; set; }
		public bool IsActive { get; set; }
		public bool IsDirty { get; set; }
		public void Clear() { }
		public void MarkClean() { }
		public void Serialize(MessageWriter writer, bool initialState) { }
		public void Deserialize(MessageReader reader, bool initialState) { }
		public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null) { }
		public void UpdateSystem(PlayerControl player, MessageReader msgReader) { }
	}

	private static ExtremeSystemTypeManager CreateManager()
	{
		var manager = (ExtremeSystemTypeManager)RuntimeHelpers.GetUninitializedObject(typeof(ExtremeSystemTypeManager));
		var allSystemsField = typeof(ExtremeSystemTypeManager).GetField("allSystems", BindingFlags.NonPublic | BindingFlags.Instance);
		allSystemsField?.SetValue(manager, new Dictionary<ExtremeSystemType, IExtremeSystemType>());

		var dirtableSystemsField = typeof(ExtremeSystemTypeManager).GetField("dirtableSystems", BindingFlags.NonPublic | BindingFlags.Instance);
		dirtableSystemsField?.SetValue(manager, new Dictionary<ExtremeSystemType, IDirtableSystemType>());

		var sabotageSystemField = typeof(ExtremeSystemTypeManager).GetField("sabotageSystem", BindingFlags.NonPublic | BindingFlags.Instance);
		sabotageSystemField?.SetValue(manager, new List<ISabotageExtremeSystemType>());

		var dirtySystemField = typeof(ExtremeSystemTypeManager).GetField("dirtySystem", BindingFlags.NonPublic | BindingFlags.Instance);
		dirtySystemField?.SetValue(manager, new List<ExtremeSystemType>());

		return manager;
	}

	[Fact]
	public void Instance_NotNull()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		Assert.NotNull(ExtremeSystemTypeManager.Instance);
	}

	[Fact]
	public void TryAdd_TryGet_ExistSystem_CreateOrGet()
	{
		var manager = CreateManager();

		var dummy = new DummySystem();
		Assert.False(manager.ExistSystem((ExtremeSystemType)200));

		bool added = manager.TryAdd((ExtremeSystemType)200, dummy);
		Assert.True(added);
		Assert.True(manager.ExistSystem((ExtremeSystemType)200));

		// Adding again should return false
		Assert.False(manager.TryAdd((ExtremeSystemType)200, dummy));

		// TryGet non-generic
		Assert.True(manager.TryGet((ExtremeSystemType)200, out var sysOut));
		Assert.Same(dummy, sysOut);

		// TryGet generic
		Assert.True(manager.TryGet<DummySystem>((ExtremeSystemType)200, out var typedSys));
		Assert.Same(dummy, typedSys);

		Assert.False(manager.TryGet<DummySystem>((ExtremeSystemType)201, out var notFoundSys));
		Assert.Null(notFoundSys);

		// CreateOrGet with func
		var createdFunc = manager.CreateOrGet<DummySystem>((ExtremeSystemType)202, () => new DummySystem());
		Assert.NotNull(createdFunc);
	}

	[Fact]
	public void SabotageSystem_And_DirtableSystem_Registered_Correctly()
	{
		var manager = CreateManager();

		var sabo = new DummySabotageSystem { IsBlockOtherSabotage = true };
		manager.TryAdd((ExtremeSystemType)210, sabo);

		Assert.True(manager.IsActiveSpecialSabotage);

		var dirtable = new DummyDirtableSystem { IsDirty = true };
		manager.TryAdd((ExtremeSystemType)211, dirtable);

		manager.Deteriorate(1.0f);
		Assert.True(dirtable.DeteriorateCalled);
		Assert.True(manager.IsDirty);

		manager.MarkClean();
		Assert.True(dirtable.MarkCleanCalled);
	}

	[Fact]
	public void Reset_PropagatesToAllSystems()
	{
		var manager = CreateManager();
		var dummy = new DummySystem();
		manager.TryAdd((ExtremeSystemType)220, dummy);

		manager.Reset(null, (byte)ResetTiming.OnPlayer);
		Assert.True(dummy.ResetCalled);
		Assert.Equal(ResetTiming.OnPlayer, dummy.LastResetTiming);
		Assert.Null(dummy.LastResetPlayer);

		manager.Reset(null, (byte)ResetTiming.MeetingStart);
		Assert.Equal(ResetTiming.MeetingStart, dummy.LastResetTiming);
	}

	[Fact]
	public void RemoveSystem_ClearsAll()
	{
		var manager = CreateManager();
		var dummy = new DummySystem();
		manager.TryAdd((ExtremeSystemType)230, dummy);

		manager.RemoveSystem();
		Assert.False(manager.ExistSystem((ExtremeSystemType)230));
	}

	[Fact]
	public void UpdateSystem_PlayerControlNull_Or_SystemNotFound()
	{
		var manager = CreateManager();
		var reader = new Mock<MessageReader>();
		reader.Setup(r => r.ReadByte()).Returns((byte)240);

		// Player is null -> returns early
		manager.UpdateSystem(null!, reader.Object);

		// System not found -> returns early
		var mockPlayer = MockSetupHelper.SetupPlayerControlMocks();
		manager.UpdateSystem(mockPlayer.Object, reader.Object);
	}

	[Fact]
	public void UpdateSystem_PlayerControlValid_SystemFound()
	{
		var manager = CreateManager();
		var dummy = new DummySystem();
		manager.TryAdd((ExtremeSystemType)240, dummy);

		var reader = new Mock<MessageReader>();
		reader.Setup(r => r.ReadByte()).Returns((byte)240);

		var mockPlayer = MockSetupHelper.SetupPlayerControlMocks();
		manager.UpdateSystem(mockPlayer.Object, reader.Object);

		Assert.True(dummy.UpdateCalled);
		Assert.Same(mockPlayer.Object, dummy.LastUpdatedPlayer);
	}
}
