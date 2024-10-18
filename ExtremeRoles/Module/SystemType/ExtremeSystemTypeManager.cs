using System;
using System.Collections.Generic;
using System.Linq;

using Hazel;
using InnerNet;

using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.Attributes;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance;

using Il2CppObject = Il2CppSystem.Object;
using Il2CppByteArry = Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>;
using System.Diagnostics.CodeAnalysis;


#nullable enable

namespace ExtremeRoles.Module.SystemType;

public enum ExtremeSystemType : byte
{
	RaiseHandSystem,
	GlobalCheckpoint,
	ModdedMeetingTimeSystem,
	ModedMushroom,
	ExtremeConsoleSystem,
	HostUpdateSystem,
	ResetObjectSystem,

	WispTorch,

	BakeryReport,
	DelusionerCounter,

	FakerDummy,
	ThiefMeetingTimeChange,
	TeroristTeroSabotage,
	ScavengerAbility,
	RaiderBomb,

	YokoYashiro,
	TuckerShadow
}

public enum ResetTiming : byte
{
	OnPlayer,
	MeetingStart,
	MeetingEnd,
}

[Il2CppRegister([ typeof(ISystemType) ])]
public sealed class ExtremeSystemTypeManager : Il2CppObject, IAmongUs.ISystemType
{
	public static ExtremeSystemTypeManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new ExtremeSystemTypeManager();
				instance.initialize();
			}
			return instance;
		}
	}

	public const SystemTypes Type = (SystemTypes)60;

	public bool IsDirty { get; private set; } = false;
	public bool IsActiveSpecialSabotage => this.sabotageSystem.Any(s => s.IsBlockOtherSabotage);

	private static ExtremeSystemTypeManager? instance = null;

	private readonly Dictionary<ExtremeSystemType, IExtremeSystemType> allSystems = new();
	private readonly Dictionary<ExtremeSystemType, IDirtableSystemType> dirtableSystems = new ();
	private readonly List<ISabotageExtremeSystemType> sabotageSystem = new();

	private readonly List<ExtremeSystemType> dirtySystem = new List<ExtremeSystemType>();

	public ExtremeSystemTypeManager()
		: base(ClassInjector.DerivedConstructorPointer<ExtremeSystemTypeManager>())
	{
		ClassInjector.DerivedConstructorBody(this);
	}

	public ExtremeSystemTypeManager(IntPtr ptr) : base(ptr)
	{ }

	public static void ModInitialize()
	{
		SystemTypeHelpers.AllTypes = SystemTypeHelpers.AllTypes.Concat([ Type ]).ToArray();
	}

	public static void RpcUpdateSystemOnlyHost(ExtremeSystemType targetSystem, Action<MessageWriter> writeAction)
	{
		MessageWriter writerForReader = createWriter(targetSystem, writeAction);
		var data = writerForReader.ToByteArray(false);

		if (!AmongUsClient.Instance || AmongUsClient.Instance.AmHost)
		{
			Instance.UpdateSystem(data);
			writerForReader.Recycle();
			return;
		}

		callRpc(writerForReader, AmongUsClient.Instance.HostId);

		writerForReader.Recycle();
	}

	public static void UpdateSystem(MessageReader reader)
	{
		Instance.UpdateSystem(
			reader.ReadNetObject<PlayerControl>(),
			reader);
	}

	public static void RpcUpdateSystem(ExtremeSystemType targetSystem, Action<MessageWriter> writeAction)
	{
		MessageWriter writerForReader = createWriter(targetSystem, writeAction);
		var data = writerForReader.ToByteArray(false);
		callRpc(writerForReader, -1);

		Instance.UpdateSystem(data);

		writerForReader.Recycle();
	}

	public bool ExistSystem(ExtremeSystemType type) => this.allSystems.ContainsKey(type);

	public void Deserialize(MessageReader reader, bool initialState)
	{
		int sysmtemNum = reader.ReadPackedInt32();
		for (int i = 0; i < sysmtemNum; ++i)
		{
			ExtremeSystemType systemType = (ExtremeSystemType)reader.ReadByte();
			this.dirtableSystems[systemType].Deserialize(reader, initialState);
		}
	}

	public void Deteriorate(float deltaTime)
	{
		this.dirtySystem.Clear();
		foreach (var (systemTypes, system) in this.dirtableSystems)
		{
			system.Deteriorate(deltaTime);
			if (system.IsDirty)
			{
				this.IsDirty = this.IsDirty || system.IsDirty;
				this.dirtySystem.Add(systemTypes);
			}
		}
	}


	[HideFromIl2Cpp]
	public bool TryGet(ExtremeSystemType systemType, [NotNullWhen(true)] out IExtremeSystemType? system)
		=> this.allSystems.TryGetValue(systemType, out system);

	public T CreateOrGet<T>(ExtremeSystemType systemType) where T : class, IExtremeSystemType, new()
	{
		if (!Instance.TryGet<T>(systemType, out var system))
		{
			system = new T();
			Instance.TryAdd(systemType, system);
		}
		return system;
	}

	[HideFromIl2Cpp]
	public T CreateOrGet<T>(ExtremeSystemType systemType, Func<T> construnctFunc) where T : class, IExtremeSystemType
	{
		if (!Instance.TryGet<T>(systemType, out var system))
		{
			system = construnctFunc.Invoke();
			Instance.TryAdd(systemType, system);
		}
		return system;
	}

	[HideFromIl2Cpp]
	public bool TryGet<T>(ExtremeSystemType systemType, [NotNullWhen(true)] out T? system) where T : class, IExtremeSystemType
	{
		system = default(T);
		if (!this.allSystems.TryGetValue(systemType, out var iSystem))
		{
			return false;
		}
		system = iSystem as T;
		return iSystem is not null;
	}

	[HideFromIl2Cpp]
	public bool TryAdd(ExtremeSystemType systemType, IExtremeSystemType system)
	{
		lock (this.allSystems)
		{
			bool result = this.allSystems.TryAdd(systemType, system);

			if (result &&
				system is IDirtableSystemType dirtableSystem)
			{
				this.dirtableSystems.Add(systemType, dirtableSystem);
			}

			if (result &&
				system is ISabotageExtremeSystemType saboSystem)
			{
				this.sabotageSystem.Add(saboSystem);
			}
			return result;
		}
	}

	public void Reset(PlayerControl? player, byte amount)
	{
		ResetTiming timing = (ResetTiming)amount;
		PlayerControl? resetPlayer = timing == ResetTiming.OnPlayer ? player : null;
		foreach (var system in this.allSystems.Values)
		{
			system.Reset(timing, resetPlayer);
		}
	}

	public void RemoveSystem()
	{
		this.dirtableSystems.Clear();
		this.sabotageSystem.Clear();
		this.dirtySystem.Clear();
		this.allSystems.Clear();

		this.initialize();
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.WritePacked(this.dirtySystem.Count);
		foreach (var systemType in this.dirtySystem)
		{
			writer.Write((byte)systemType);
			this.dirtableSystems[systemType].Serialize(writer, initialState);
		}
		this.IsDirty = initialState;
	}

	public void UpdateSystem(Il2CppByteArry data)
		=> UpdateSystem(
			PlayerControl.LocalPlayer,
			MessageReader.Get(data));

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
	 	ExtremeSystemType systemType = (ExtremeSystemType)msgReader.ReadByte();
		this.allSystems[systemType].UpdateSystem(player, msgReader);
	}

	internal static void AddSystem()
	{
		var system = Instance;
		CachedShipStatus.Instance.Systems.Add(Type, system.Cast<ISystemType>());
	}

	private void initialize()
	{
		add<GlobalCheckpointSystem>(GlobalCheckpointSystem.Type);
	}

	private void add<T>(ExtremeSystemType systemType) where T : class, IExtremeSystemType, new()
	{
		var system = new T();
		Instance.TryAdd(systemType, system);
	}

	private static void callRpc(MessageWriter writer, int target)
	{
		using var caller = new RPCOperator.RpcCaller(
			PlayerControl.LocalPlayer.NetId,
			RPCOperator.Command.UpdateExtremeSystemType,
			target: target);
		caller.WriteNetObject(PlayerControl.LocalPlayer);
		caller.WriteWriter(writer, false);
	}

	private static MessageWriter createWriter(ExtremeSystemType targetSystem, Action<MessageWriter> writeAction)
	{
		MessageWriter writerForReader = MessageWriter.Get(SendOption.Reliable);
		writerForReader.Write((byte)targetSystem);
		writeAction.Invoke(writerForReader);

		return writerForReader;
	}
}
