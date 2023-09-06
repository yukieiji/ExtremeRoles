using System;
using System.Collections.Generic;
using System.Linq;

using Hazel;

using ExtremeRoles.Module.Interface;

using Il2CppObject = Il2CppSystem.Object;
using Il2CppInterop.Runtime.Injection;
using InnerNet;
using ExtremeRoles.Performance;

#nullable enable

namespace ExtremeRoles.Module.SystemType;

public enum ExtremeSystemType : byte
{
	FakerDummy
}

public enum ResetTiming : byte
{
	OnPlayer,
	MeetingStart,
	MeetingEnd,
}

[Il2CppRegister(new Type[] { typeof(ISystemType) })]
public sealed class ExtremeSystemTypeManager : Il2CppObject, IAmongUs.ISystemType
{
	public static ExtremeSystemTypeManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new ExtremeSystemTypeManager();
			}
			return instance;
		}
	}

	public static SystemTypes Type => (SystemTypes)60;

	public bool IsDirty { get; private set; }

	private const byte callId = 35;
	private static ExtremeSystemTypeManager? instance = null;
	private readonly Dictionary<ExtremeSystemType, IExtremeSystemType> systems = new Dictionary<ExtremeSystemType, IExtremeSystemType>();

	public ExtremeSystemTypeManager()
		: base(ClassInjector.DerivedConstructorPointer<ExtremeSystemTypeManager>())
	{
		ClassInjector.DerivedConstructorBody(this);
	}

	public ExtremeSystemTypeManager(IntPtr ptr) : base(ptr)
	{ }

	public static void ModInitialize()
	{
		SystemTypeHelpers.AllTypes = SystemTypeHelpers.AllTypes.Concat(new List<SystemTypes> { Type }).ToArray();
	}

	public static void RpcUpdateSystemOnlyHost(ExtremeSystemType targetSystem, Action<MessageWriter> writeAction)
	{
		MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
		messageWriter.Write((byte)targetSystem);
		writeAction.Invoke(messageWriter);
		CachedShipStatus.Instance.RpcUpdateSystem(Type, messageWriter);
		messageWriter.Recycle();
	}

	public static void RpcUpdateSystem(ExtremeSystemType targetSystem, Action<MessageWriter> writeAction)
	{
		// TODO: ライターが2つと冗長すぎるのでどうにかする
		MessageWriter writerForReader = MessageWriter.Get(SendOption.Reliable);
		writerForReader.Write((byte)targetSystem);
		writeAction.Invoke(writerForReader);
		var data = writerForReader.ToByteArray(false);

		PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(
			CachedShipStatus.Instance.NetId, callId, SendOption.Reliable, -1);
		messageWriter.Write((byte)Type);
		messageWriter.WriteNetObject(localPlayer);
		messageWriter.Write(writerForReader, false);
		AmongUsClient.Instance.FinishRpcImmediately(messageWriter);

		var reader = MessageReader.Get(data);
		CachedShipStatus.Instance.UpdateSystem(Type, localPlayer, reader);
		writerForReader.Recycle();
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		foreach (var system in systems.Values)
		{
			system.Deserialize(reader, initialState);
		}
	}

	public void Detoriorate(float deltaTime)
	{
		foreach (var system in systems.Values)
		{
			system.Detoriorate(deltaTime);
			this.IsDirty = this.IsDirty || system.IsDirty;
		}
	}

	public bool TryGet(ExtremeSystemType systemType, out IExtremeSystemType? system)
		=> this.systems.TryGetValue(systemType, out system);

	public bool TryGet<T>(ExtremeSystemType systemType, out T? system) where T : class, IExtremeSystemType
	{
		system = default(T);
		if (!this.systems.TryGetValue(systemType, out IExtremeSystemType? iSystem))
		{
			return false;
		}
		system = iSystem as T;
		return iSystem is not null;
	}

	public void TryAdd(ExtremeSystemType systemType, IExtremeSystemType system)
		=> this.systems.TryAdd(systemType, system);

	public void Add(ExtremeSystemType systemType, IExtremeSystemType system)
	{
		this.systems.Add(systemType, system);
	}

	public void Reset()
	{
		this.systems.Clear();
	}

	public void RepairDamage(PlayerControl player, byte amount)
	{
		ResetTiming timing = (ResetTiming)amount;
		PlayerControl? resetPlayer = timing == ResetTiming.OnPlayer ? player : null;
		foreach (var system in systems.Values)
		{
			system.Reset(timing, resetPlayer);
		}
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		foreach (var system in systems.Values)
		{
			system.Serialize(writer, initialState);
		}
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
	 	ExtremeSystemType systemType = (ExtremeSystemType)msgReader.ReadByte();
		this.systems[systemType].UpdateSystem(player, msgReader);
	}
}
