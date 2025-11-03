using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Il2CppInterop.Runtime;
using HarmonyLib;

using BepInEx;
using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Compat.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;


using UnityObject = UnityEngine.Object;
using OptionFactory = ExtremeRoles.Module.CustomOption.Factory.SequentialOptionCategoryFactory;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.Option.ShipGlobal.Sub;
using ExtremeRoles.Compat.Initializer;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Module.CustomOption.Implemented;

#nullable enable

namespace ExtremeRoles.Compat.ModIntegrator;

public sealed class SubmergedIntegrator : ModIntegratorBase, IMultiFloorModMap, IIntegrateOption
{
	public enum SubmergedOption
	{
		EnableElevator,
		ReplaceDoorMinigame,
		SubmergedSpawnSetting
	}

	public enum SpawnSetting
	{
		DefaultKey,
		LowerCentralOnly,
		UpperCentralOnly
	}

	public enum ElevatorSelection
	{
		AllElevator,
		OnlyCentralElevator,
		OnlyLobbyElevator,
		OnlyServiceElevator,
		CentralAndLobbyElevator,
		CentralAndServiceElevator,
		LobbyAndServiceElevator
	}
	/*
	 Submerged(Clone)/Elevators/
	  Central
	   WestLeftElevator
	   WestRightElevator
	 Lobby
	   EastLeftElevator
	   EastRightElevator
	 ServiceElevator
	*/
	private const string centralLeftElevator = "Elevators/WestLeftElevator";
	private const string centralRightElevator = "Elevators/WestRightElevator";
	private const string lobbyLeftElevator = "Elevators/EastLeftElevator";
	private const string lobbyRightElevator = "Elevators/EastRightElevator";
	private const string serviceElevator = "Elevators/ServiceElevator";

	public const string Guid = "Submerged";

	public byte MapId => 6;
	public ShipStatus.MapType MapType => (ShipStatus.MapType)MapId;
	public bool CanPlaceCamera => false;
	public bool IsCustomCalculateLightRadius => true;
	public SpawnSetting Spawn => (SpawnSetting)this.enableSubMergedRandomSpawn.Value<int>();

	public TaskTypes RetrieveOxygenMask;

	private float crewVision;
	private float impostorVision;

	private readonly Type submarineStatusType;
	private readonly FieldInfo submarineStatusReference;
	private readonly PropertyInfo inTransitionField;

	private readonly MethodInfo calculateLightRadiusMethod;

	private readonly MethodInfo rpcRequestChangeFloorMethod;
	private readonly MethodInfo registerFloorOverrideMethod;
	private readonly FieldInfo onUpperField;
	private readonly MethodInfo getFloorHandlerInfo;

	private readonly PropertyInfo submarineOxygenSystemInstanceGetter;
	private readonly MethodInfo submarineOxygenSystemRepairDamageMethod;

	private readonly Type elevatorMover;

	private MonoBehaviour? submarineStatus;
#pragma warning disable CS8618

	private IOption elevatorOption;
	private IOption replaceDoorMinigameOption;
	private IOption enableSubMergedRandomSpawn;

	public SubmergedIntegrator(SubmergedInitializer init) : base(init)
#pragma warning restore CS8618
	{
		// カスタムサボのタスクタイプ取得
		Type taskType = init.GetClass("CustomTaskTypes");
		var retrieveOxigenMaskField = AccessTools.Field(taskType, "RetrieveOxygenMask");
		object? taskTypeObj = retrieveOxigenMaskField.GetValue(null);
		var retrieveOxigenMaskTaskTypeField = AccessTools.Field(taskTypeObj?.GetType(), "taskType");

		object? oxygenTaskType = retrieveOxigenMaskTaskTypeField.GetValue(taskTypeObj);
		if (oxygenTaskType == null) { return; }
		this.RetrieveOxygenMask = (TaskTypes)oxygenTaskType;

		// サブマージドの酸素妨害の修理用
		this.submarineOxygenSystemInstanceGetter = AccessTools.Property(
			init.SubmarineOxygenSystem, "Instance");
		this.submarineOxygenSystemRepairDamageMethod = AccessTools.Method(
			init.SubmarineOxygenSystem, "RepairDamage");

		// サブマージドのカスタムMonoを取ってくる
		this.elevatorMover = init.GetClass("ElevatorMover");

		// フロアを変える用
		Type floorHandlerType = init.GetClass("FloorHandler");
		this.getFloorHandlerInfo = init.GetMethod(
			floorHandlerType, "GetFloorHandler", [ typeof(PlayerControl) ]);
		this.rpcRequestChangeFloorMethod = init.GetMethod(
			floorHandlerType, "RpcRequestChangeFloor");
		this.registerFloorOverrideMethod = init.GetMethod(
			floorHandlerType, "RegisterFloorOverride");
		this.onUpperField = AccessTools.Field(floorHandlerType, "onUpper");

		this.submarineStatusType = init.GetClass("SubmarineStatus");
		this.calculateLightRadiusMethod = init.GetMethod(
			this.submarineStatusType, "CalculateLightRadius");
		this.submarineStatusReference = AccessTools.Field(
			this.submarineStatusType, "referenceHolder");

		Type ventMoveToVentPatchType = init.GetClass("VentPatchData");
		this.inTransitionField = AccessTools.Property(ventMoveToVentPatchType, "InTransition");
	}
#pragma warning restore CS8618

	public void Awake(ShipStatus map)
	{
		var component = map.GetComponent(Il2CppType.From(this.submarineStatusType));
		if (component)
		{
			this.submarineStatus = component.TryCast(
				this.submarineStatusType) as MonoBehaviour;
		}

		// 毎回毎回取得すると重いのでキャッシュ化
		var curOption = GameOptionsManager.Instance.CurrentGameOptions;
		crewVision = curOption.GetFloat(FloatOptionNames.CrewLightMod);
		impostorVision = curOption.GetFloat(FloatOptionNames.ImpostorLightMod);

		// オプション周りの処理
		disableElevator();
		replaceDoorMinigame();
	}

	public void CreateIntegrateOption(OptionFactory factory)
	{
		// どうせ作っても5個程度なので参照を持つようにする 8byte * 5 = 40byte程度
		this.elevatorOption = factory.CreateSelectionOption<ElevatorSelection>(SubmergedOption.EnableElevator);
		this.replaceDoorMinigameOption = factory.CreateBoolOption(SubmergedOption.ReplaceDoorMinigame, false);

		if (!OptionManager.Instance.TryGetCategory(
				OptionTab.GeneralTab,
				(int)ShipGlobalOptionCategory.RandomSpawnOption,
				out var cate))
		{
			return;
		}

		var randomSpawnOpt = cate.Get(RandomSpawnOption.Enable);
		this.enableSubMergedRandomSpawn = factory.CreateSelectionOption<SpawnSetting>(
			SubmergedOption.SubmergedSpawnSetting, new InvertActive(randomSpawnOpt));
	}

	public void Destroy()
	{
		submarineStatus = null;

		// バグってるかもしれないのでもとに戻しとく
		var curOption = GameOptionsManager.Instance.CurrentGameOptions;
		curOption.SetFloat(FloatOptionNames.CrewLightMod, crewVision);
		curOption.SetFloat(FloatOptionNames.ImpostorLightMod, impostorVision);
	}

	public float CalculateLightRadius(NetworkedPlayerInfo player, bool neutral, bool neutralImpostor)
	{
		object? value = calculateLightRadiusMethod.Invoke(
			this.submarineStatus, new object?[] { null, neutral, neutralImpostor });
		return value != null ? (float)value : 1.0f;
	}

	public float CalculateLightRadius(
		NetworkedPlayerInfo player, float visionMod, bool applayVisionEffects = true)
	{
		// サブマージドの視界計算のロジックは「クルーだと停電効果受ける、インポスターだと受けないので」
		// 1. まずはデフォルトの視界をMOD側で用意した視界の広さにリプレイス
		// 2. 視界効果を受けるかをインポスターかどうかで渡して計算
		// 3. 元の視界の広さに戻す

		var curOption = GameOptionsManager.Instance.CurrentGameOptions;
		curOption.SetFloat(FloatOptionNames.CrewLightMod, visionMod);
		curOption.SetFloat(FloatOptionNames.ImpostorLightMod, visionMod);

		float result = CalculateLightRadius(player, true, !applayVisionEffects);

		curOption.SetFloat(FloatOptionNames.CrewLightMod, crewVision);
		curOption.SetFloat(FloatOptionNames.ImpostorLightMod, impostorVision);

		return result;
	}

	public int GetFloor(Vector3 pos) => pos.y > -6.19f ? 1 : 0;

	public int GetFloor(PlayerControl player)
	{
		MonoBehaviour? floorHandler = getFloorHandler(player);
		if (floorHandler == null) { return int.MaxValue; }

		object? valueObj = this.onUpperField.GetValue(floorHandler);
		return valueObj != null && (bool)valueObj ? 1 : 0;
	}

	public void ChangeFloor(PlayerControl player, int floor)
	{
		if (floor > 1) { return; }
		MonoBehaviour? floorHandler = getFloorHandler(player);
		if (floorHandler == null) { return; }

		object[] args = [ floor == 1 ];

		this.rpcRequestChangeFloorMethod.Invoke(floorHandler, args);
		this.registerFloorOverrideMethod.Invoke(floorHandler, args);
	}

	public Console? GetConsole(TaskTypes task)
	{
		var console = UnityObject.FindObjectsOfType<Console>();
		switch (task)
		{
			case TaskTypes.FixLights:
				return console.FirstOrDefault(
					x => x.gameObject.name.Contains("LightsConsole"));
			case TaskTypes.StopCharles:
				List<Console> res = new List<Console>(2);
				Console? leftConsole = console.FirstOrDefault(
					x => x.gameObject.name.Contains("BallastConsole_1"));
				if (leftConsole != null)
				{
					res.Add(leftConsole);
				}
				Console? rightConsole = console.FirstOrDefault(
					x => x.gameObject.name.Contains("BallastConsole_2"));
				if (rightConsole != null)
				{
					res.Add(rightConsole);
				}
				return res[RandomGenerator.Instance.Next(res.Count)];
			default:
				return null;
		}
	}

	public List<Vector2> GetSpawnPos(byte playerId)
	{
		ShipStatus ship = ShipStatus.Instance;
		Vector2 baseVec = Vector2.up;
		baseVec = baseVec.Rotate(
			(float)(playerId - 1) * (360f / (float)GameData.Instance.PlayerCount));
		Vector2 defaultSpawn = ship.InitialSpawnCenter +
			baseVec * ship.SpawnRadius + new Vector2(0f, 0.3636f);

		List<Vector2> spawnPos = new List<Vector2>()
		{
			defaultSpawn, defaultSpawn + new Vector2(0.0f, 48.119f)
		};

		return spawnPos;
	}

	public HashSet<string> GetSystemObjectName(SystemConsoleType sysConsole)
	{
		switch (sysConsole)
		{
			case SystemConsoleType.AdminModule:
				return new HashSet<string>()
				{
					"Submerged(Clone)/TopFloor/Adm-Obsv-Loun-MR/TaskConsoles/console-adm-admintable",
					"Submerged(Clone)/TopFloor/Adm-Obsv-Loun-MR/TaskConsoles/console-adm-admintable (1)",
				};
			case SystemConsoleType.VitalsLabel:
				return new HashSet<string>()
				{
					"Submerged(Clone)/panel_vitals(Clone)",
				};
			case SystemConsoleType.SecurityCamera:
				return new HashSet<string>()
				{
					"Submerged(Clone)/BottomFloor/Engines-Security/TaskConsoles/SecurityConsole",
				};
			default:
				return new HashSet<string>();
		}
	}

	public SystemConsole? GetSystemConsole(SystemConsoleType sysConsole)
	{
		var systemConsoleArray = UnityObject.FindObjectsOfType<SystemConsole>();
		switch (sysConsole)
		{
			case SystemConsoleType.SecurityCamera:
				return systemConsoleArray.FirstOrDefault(
					x => x.gameObject.name.Contains("SecurityConsole"));
			case SystemConsoleType.VitalsLabel:
				return systemConsoleArray.FirstOrDefault(
					x => x.gameObject.name.Contains("panel_vitals(Clone)"));
			case SystemConsoleType.EmergencyButton:
				return systemConsoleArray.FirstOrDefault(
					x => x.gameObject.name.Contains("console-mr-callmeeting"));
			default:
				return null;
		}
	}

	public bool IsCustomSabotageNow()
	{
		foreach (NormalPlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
		{
			if (task != null && IsCustomSabotageTask(task.TaskType))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsCustomSabotageTask(TaskTypes saboTask) => saboTask == this.RetrieveOxygenMask;

	public bool IsCustomVentUse(Vent vent)
	{
		switch (vent.Id)
		{
			case 9:  // Cannot enter vent 9 (Engine Room Exit Only Vent)!
				if (PlayerControl.LocalPlayer.inVent)
				{
					return false;
				}
				return true;
			case 0:
			case 14: // Lower and Upper Central
				return true;
			default:
				return false;
		}
	}

	public (float, bool, bool) IsCustomVentUseResult(
		Vent vent, NetworkedPlayerInfo player, bool isVentUse)
	{
		object? valueObj = inTransitionField.GetValue(null);

		if (valueObj == null || (bool)valueObj)
		{
			return (float.MaxValue, false, false);
		}
		switch (vent.Id)
		{
			case 0:
			case 14: // Lower and Upper Central
				float result = float.MaxValue;
				bool couldUse = isVentUse && !player.IsDead && (player.Object.CanMove || player.Object.inVent);
				bool canUse = couldUse;
				if (canUse)
				{
					Vector3 center = player.Object.Collider.bounds.center;
					Vector3 position = vent.transform.position;
					result = Vector2.Distance(center, position);
					canUse &= result <= vent.UsableDistance;
				}
				return (result, canUse, couldUse);
			default:
				return (float.MaxValue, false, false);
		}
	}

	public void RpcRepairCustomSabotage()
	{
		using (var caller = RPCOperator.CreateCaller(
			RPCOperator.Command.IntegrateModCall))
		{
			caller.WriteByte(IMapMod.RpcCallType);
			caller.WriteByte((byte)MapRpcCall.RepairAllSabo);
		}
		RepairCustomSabotage();
	}

	public void RpcRepairCustomSabotage(TaskTypes saboTask)
	{
		using (var caller = RPCOperator.CreateCaller(
			RPCOperator.Command.IntegrateModCall))
		{
			caller.WriteByte(IMapMod.RpcCallType);
			caller.WriteByte((byte)MapRpcCall.RepairCustomSaboType);
			caller.WriteInt((int)saboTask);
		}
		RepairCustomSabotage(saboTask);
	}

	public void RepairCustomSabotage()
	{
		RepairCustomSabotage(this.RetrieveOxygenMask);
	}

	public void RepairCustomSabotage(TaskTypes saboTask)
	{
		if (saboTask == this.RetrieveOxygenMask)
		{
			ShipStatus.Instance.RpcUpdateSystem((SystemTypes)130, 64);
			this.submarineOxygenSystemRepairDamageMethod.Invoke(
				this.submarineOxygenSystemInstanceGetter.GetValue(null),
				[ PlayerControl.LocalPlayer, (byte)64 ]);
		}
	}
	public void AddCustomComponent(
		GameObject addObject, CustomMonoBehaviourType customMonoType)
	{
		switch (customMonoType)
		{
			case CustomMonoBehaviourType.MovableFloorBehaviour:
				addObject.TryAddComponent(Il2CppType.From(this.elevatorMover));
				break;
			default:
				break;

		}
	}

	public void SetUpNewCamera(SurvCamera camera)
	{
		var fixConsole = camera.transform.FindChild("FixConsole");
		if (fixConsole != null &&
            fixConsole.TryGetComponent<BoxCollider2D>(out var box))
		{
            UnityObject.Destroy(box);
        }
	}

	private MonoBehaviour? getFloorHandler(PlayerControl player)
	{
		object? handlerObj = this.getFloorHandlerInfo.Invoke(null, new object[] { player });

		if (handlerObj == null) { return null; }

		return ((Component)handlerObj).TryCast<MonoBehaviour>();
	}

	private void disableElevator()
	{
		var useElevator = (ElevatorSelection)this.elevatorOption.Value<int>();

		switch (useElevator)
		{
			case ElevatorSelection.OnlyCentralElevator:
				disableSubmergedObj(lobbyRightElevator);
				disableSubmergedObj(lobbyLeftElevator);
				disableSubmergedObj(serviceElevator);
				break;
			case ElevatorSelection.OnlyLobbyElevator:
				disableSubmergedObj(centralRightElevator);
				disableSubmergedObj(centralLeftElevator);
				disableSubmergedObj(serviceElevator);
				break;
			case ElevatorSelection.OnlyServiceElevator:
				disableSubmergedObj(lobbyRightElevator);
				disableSubmergedObj(lobbyLeftElevator);
				disableSubmergedObj(centralRightElevator);
				disableSubmergedObj(centralLeftElevator);
				break;
			case ElevatorSelection.CentralAndLobbyElevator:
				disableSubmergedObj(serviceElevator);
				break;
			case ElevatorSelection.CentralAndServiceElevator:
				disableSubmergedObj(lobbyRightElevator);
				disableSubmergedObj(lobbyLeftElevator);
				break;
			case ElevatorSelection.LobbyAndServiceElevator:
				disableSubmergedObj(centralRightElevator);
				disableSubmergedObj(centralLeftElevator);
				break;
			default:
				break;
		}
	}

	private void replaceDoorMinigame()
	{
		if (!this.replaceDoorMinigameOption.Value<bool>() || ShipStatus.Instance == null)
		{
			return;
		}

		object? transformValue = this.submarineStatusReference.GetValue(this.submarineStatus);
		if (transformValue == null ||
			transformValue is not Transform transform)
		{
			return;
		}

		// AirShip持ってくる
		ShipStatus ship = GameSystem.GetShipObj(4);

		Minigame? doorMinigame = ship.AllDoors
			.Select(x =>
			{
				var door = x.gameObject.GetComponent<DoorConsole>();
				if (door != null && door.MinigamePrefab != null)
				{
					return door.MinigamePrefab;
				}
				else
				{
					return null;
				}
			})
			.FirstOrDefault(x => x != null);

		if (doorMinigame == null) { return; }

		foreach (var doorConsole in ShipStatus.Instance.GetComponentsInChildren<DoorConsole>())
		{
			if (doorConsole == null)
			{
				continue;
			}

			doorConsole.MinigamePrefab = UnityObject.Instantiate(
				doorMinigame, transform);
		}
	}

	private static void disableSubmergedObj(string name)
	{
		GameObject obj = GameObject.Find($"Submerged(Clone)/{name}");
		if (obj != null)
		{
			obj.SetActive(false);
		}
	}
}