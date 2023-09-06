using System;
using System.Collections.Generic;
using System.Linq;

using Hazel;

using ExtremeRoles.Module.Interface;

using Il2CppObject = Il2CppSystem.Object;
using Il2CppInterop.Runtime.Injection;


#nullable enable

namespace ExtremeRoles.Module.SystemType;

public enum ExtremeSystemType : byte
{

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
	public static SystemTypes Type => (SystemTypes)60;

	public bool IsDirty { get; private set; }

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

	public bool TryAdd(ExtremeSystemType systemType, IExtremeSystemType system)
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

	public void RpcUpdateSystem(ExtremeSystemType targetSystem, Action<MessageWriter> writeAction)
	{
		MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
		messageWriter.Write((byte)targetSystem);
		writeAction.Invoke(messageWriter);
		ShipStatus.Instance.RpcUpdateSystem(Type, messageWriter);
		messageWriter.Recycle();
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
