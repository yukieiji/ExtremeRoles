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

using ExtremeRoles.Module.NewOption.Implemented;
using OptionFactory = ExtremeRoles.Module.NewOption.Factory.SequentialOptionCategoryFactory;

#nullable enable

namespace ExtremeRoles.Compat.ModIntegrator;

public sealed class SubmergedIntegrator : ModIntegratorBase, IMultiFloorModMap
{
	public enum SubmergedOption
	{
		EnableElevator,
		ReplaceDoorMinigame,
		SubmergedSpawnSetting
	}

	public enum SpawnSetting
	{
		DefaultSpawn,
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
	public SpawnSetting Spawn => (SpawnSetting)this.enableSubMergedRandomSpawn.Value;

	public TaskTypes RetrieveOxygenMask;

	private float crewVision;
	private float impostorVision;

	private Type submarineStatusType;
	private FieldInfo submarineStatusReference;
	private PropertyInfo inTransitionField;

	private MethodInfo calculateLightRadiusMethod;

	private MethodInfo rpcRequestChangeFloorMethod;
	private MethodInfo registerFloorOverrideMethod;
	private FieldInfo onUpperField;
	private MethodInfo getFloorHandlerInfo;

	private Type submarineOxygenSystem;
	private PropertyInfo submarineOxygenSystemInstanceGetter;
	private MethodInfo submarineOxygenSystemRepairDamageMethod;

	private Type elevatorMover;

	private MonoBehaviour? submarineStatus;
#pragma warning disable CS8618

	private SelectionCustomOption elevatorOption;
	private BoolCustomOption replaceDoorMinigameOption;
	private SelectionCustomOption enableSubMergedRandomSpawn;

	public SubmergedIntegrator(PluginInfo plugin) : base(Guid, plugin)
	{
		// カスタムサボのタスクタイプ取得
		Type taskType = this.ClassType.First(
			t => t.Name == "CustomTaskTypes");
		var retrieveOxigenMaskField = AccessTools.Field(taskType, "RetrieveOxygenMask");
		object? taskTypeObj = retrieveOxigenMaskField.GetValue(null);
		var retrieveOxigenMaskTaskTypeField = AccessTools.Field(taskTypeObj?.GetType(), "taskType");

		object? oxygenTaskType = retrieveOxigenMaskTaskTypeField.GetValue(taskTypeObj);
		if (oxygenTaskType == null) { return; }
		this.RetrieveOxygenMask = (TaskTypes)oxygenTaskType;

		// サブマージドの酸素妨害の修理用
		this.submarineOxygenSystemInstanceGetter = AccessTools.Property(
			this.submarineOxygenSystem, "Instance");
		this.submarineOxygenSystemRepairDamageMethod = AccessTools.Method(
			this.submarineOxygenSystem, "RepairDamage");

		// サブマージドのカスタムMonoを取ってくる
		this.elevatorMover = this.ClassType.First(t => t.Name == "ElevatorMover");

		// フロアを変える用
		Type floorHandlerType = this.ClassType.First(t => t.Name == "FloorHandler");
		this.getFloorHandlerInfo = AccessTools.Method(
			floorHandlerType, "GetFloorHandler", new Type[] { typeof(PlayerControl) });
		this.rpcRequestChangeFloorMethod = AccessTools.Method(
			floorHandlerType, "RpcRequestChangeFloor");
		this.registerFloorOverrideMethod = AccessTools.Method(
			floorHandlerType, "RegisterFloorOverride");

		this.onUpperField = AccessTools.Field(floorHandlerType, "onUpper");

		this.submarineStatusType = ClassType.First(
			t => t.Name == "SubmarineStatus");
		this.calculateLightRadiusMethod = AccessTools.Method(
			this.submarineStatusType, "CalculateLightRadius");
		this.submarineStatusReference = AccessTools.Field(
			this.submarineStatusType, "referenceHolder");

		Type ventMoveToVentPatchType = ClassType.First(t => t.Name == "VentPatchData");
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

	public override void CreateIntegrateOption(OptionFactory factory)
	{
		// どうせ作っても5個程度なので参照を持つようにする 8byte * 5 = 40byte程度
		this.elevatorOption = factory.CreateSelectionOption<ElevatorSelection>(SubmergedOption.EnableElevator);
		this.replaceDoorMinigameOption = factory.CreateBoolOption(SubmergedOption.ReplaceDoorMinigame, false);

		/*
		var randomSpawnOpt = OptionManager.Instance.Get<bool>((int)GlobalOption.EnableSpecialSetting);
		this.enableSubMergedRandomSpawn = factory.CreateSelectionOption<SpawnSetting>(
			SubmergedOption.SubmergedSpawnSetting, randomSpawnOpt, invert: true);
		*/
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
		ShipStatus ship = CachedShipStatus.Instance;
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
			case SystemConsoleType.Admin:
				return new HashSet<string>()
				{
					"Submerged(Clone)/TopFloor/Adm-Obsv-Loun-MR/TaskConsoles/console-adm-admintable",
					"Submerged(Clone)/TopFloor/Adm-Obsv-Loun-MR/TaskConsoles/console-adm-admintable (1)",
				};
			case SystemConsoleType.Vital:
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
			case SystemConsoleType.Vital:
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
				if (CachedPlayerControl.LocalPlayer.PlayerControl.inVent)
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
			CachedShipStatus.Instance.RpcUpdateSystem((SystemTypes)130, 64);
			this.submarineOxygenSystemRepairDamageMethod.Invoke(
				this.submarineOxygenSystemInstanceGetter.GetValue(null),
				new object[] { PlayerControl.LocalPlayer, (byte)64 });
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
		if (fixConsole != null)
		{
			var boxCollider = fixConsole.GetComponent<BoxCollider2D>();
			if (boxCollider != null)
			{
				UnityObject.Destroy(boxCollider);
			}
		}
	}

	protected override void PatchAll(Harmony harmony)
	{
		Type exileCont = ClassType.First(
			t => t.Name == "SubmergedExileController");
		MethodInfo wrapUpAndSpawn = AccessTools.Method(
			exileCont, "WrapUpAndSpawn");
		ExileController? cont = null;
		MethodInfo wrapUpAndSpawnPrefix = SymbolExtensions.GetMethodInfo(
			() => Patches.SubmergedExileControllerWrapUpAndSpawnPatch.Prefix());
#pragma warning disable CS8604
		MethodInfo wrapUpAndSpawnPostfix = SymbolExtensions.GetMethodInfo(
			() => Patches.SubmergedExileControllerWrapUpAndSpawnPatch.Postfix(cont));
#pragma warning restore CS8604

		System.Collections.IEnumerator? enumerator = null;
		Type displayPrespawnStepPatchesType = ClassType.First(
			t => t.Name == "DisplayPrespawnStepPatches");
		MethodInfo displayPrespawnStepPatchesPostfix = AccessTools.Method(
			displayPrespawnStepPatchesType, "CustomPrespawnStep");
#pragma warning disable CS8601
		MethodInfo displayPrespawnStepPatchesPostfixPrefix = SymbolExtensions.GetMethodInfo(
			() => Patches.DisplayPrespawnStepPatchesCustomPrespawnStepPatch.Prefix(ref enumerator));
#pragma warning restore CS8601

		Type submarineSelectSpawn = ClassType.First(
			t => t.Name == "SubmarineSelectSpawn");
		MethodInfo onDestroy = AccessTools.Method(
			submarineSelectSpawn, "OnDestroy");
		MethodInfo onDestroyPrefix = SymbolExtensions.GetMethodInfo(
			() => Patches.SubmarineSelectOnDestroyPatch.Prefix());

		Type hudManagerUpdatePatch = ClassType.First(
			t => t.Name == "ChangeFloorButtonPatches");
		MethodInfo hudManagerUpdatePatchPostfix = AccessTools.Method(
			hudManagerUpdatePatch, "HudUpdatePatch");
		object? hudManagerUpdatePatchInstance = null;
		Patches.HudManagerUpdatePatchPostfixPatch.SetType(
			hudManagerUpdatePatch);
#pragma warning disable CS8604
		MethodInfo hubManagerUpdatePatchPostfixPatch = SymbolExtensions.GetMethodInfo(
			() => Patches.HudManagerUpdatePatchPostfixPatch.Postfix(
				hudManagerUpdatePatchInstance));

		string deteriorateFunction = nameof(ExtremeRoles.Module.Interface.IAmongUs.ISystemType.Deteriorate);
#pragma warning restore CS8604

		this.submarineOxygenSystem = ClassType.First(
			t => t.Name == "SubmarineOxygenSystem");
		MethodInfo submarineOxygenSystemDetoriorate = AccessTools.Method(
			submarineOxygenSystem, deteriorateFunction);
		object? submarineOxygenSystemInstance = null;
		Patches.SubmarineOxygenSystemDetorioratePatch.SetType(this.submarineOxygenSystem);
#pragma warning disable CS8604
		MethodInfo submarineOxygenSystemDetorioratePostfixPatch = SymbolExtensions.GetMethodInfo(
			() => Patches.SubmarineOxygenSystemDetorioratePatch.Postfix(
				submarineOxygenSystemInstance));
#pragma warning restore CS8604

		Type submarineSpawnInSystem = ClassType.First(
			t => t.Name == "SubmarineSpawnInSystem");
		MethodInfo submarineSpawnInSystemDetoriorate = AccessTools.Method(
			submarineSpawnInSystem, deteriorateFunction);
		object? submarineSpawnInSystemInstance = null;
		Patches.SubmarineSpawnInSystemDetorioratePatch.SetType(submarineSpawnInSystem);
#pragma warning disable CS8604
		MethodInfo submarineSpawnInSystemDetorioratePostfixPatch = SymbolExtensions.GetMethodInfo(
			() => Patches.SubmarineSpawnInSystemDetorioratePatch.Postfix(
				submarineSpawnInSystemInstance));
#pragma warning restore CS8604

		Minigame? game = null;

		Type submarineSurvillanceMinigame = ClassType.First(
			t => t.Name == "SubmarineSurvillanceMinigame");
		MethodInfo submarineSurvillanceMinigameSystemUpdate = AccessTools.Method(
			submarineSurvillanceMinigame, "Update");
		Patches.SubmarineSurvillanceMinigamePatch.SetType(submarineSurvillanceMinigame);
#pragma warning disable CS8604
		MethodInfo submarineSurvillanceMinigameSystemUpdatePrefixPatch = SymbolExtensions.GetMethodInfo(
			() => Patches.SubmarineSurvillanceMinigamePatch.Prefix(game));
		MethodInfo submarineSurvillanceMinigameSystemUpdatePostfixPatch = SymbolExtensions.GetMethodInfo(
			() => Patches.SubmarineSurvillanceMinigamePatch.Postfix(game));
#pragma warning restore CS8604

		// このコメントに沿って関数調整：https://github.com/SubmergedAmongUs/Submerged/issues/123#issuecomment-1783889792
		NetworkedPlayerInfo? info = null;
		bool tie = false;
		Type exileControllerPatches = ClassType.First(
			t => t.Name == "ExileControllerPatches");
		MethodInfo exileControllerPatchesExileControllerBegin = AccessTools.Method(
			exileControllerPatches, "ExileController_Begin");
#pragma warning disable CS8604
		MethodInfo exileControllerPatchesExileControllerBeginPatch = SymbolExtensions.GetMethodInfo(
			() => Patches.ExileControllerPatchesPatch.ExileController_BeginPrefix(cont, info, tie));
#pragma warning restore CS8604

		bool upperSelected = false;
		Type submarineSelectSpawnType = ClassType.First(
			t => t.Name == "SubmarineSelectSpawn");
		MethodInfo submarineSelectSpawnCoSelectLevel = AccessTools.Method(
			submarineSelectSpawnType, "CoSelectLevel");
		MethodInfo submarineSelectSpawnCoSelectLevelPatch = SymbolExtensions.GetMethodInfo(
			() => Patches.SubmarineSelectSpawnCoSelectLevelPatch.Prefix(ref upperSelected));


		// 会議終了時のリセット処理を呼び出せるように
		harmony.Patch(wrapUpAndSpawn,
			new HarmonyMethod(wrapUpAndSpawnPrefix),
			new HarmonyMethod(wrapUpAndSpawnPostfix));

		// アサシン会議発動するとスポーン画面が出ないように
		harmony.Patch(displayPrespawnStepPatchesPostfix,
			new HarmonyMethod(displayPrespawnStepPatchesPostfixPrefix));

		// キルクール周りが上書きされているのでそれの調整
		harmony.Patch(onDestroy,
			new HarmonyMethod(onDestroyPrefix));

		// フロアの階層変更ボタンの位置を変えるパッチ
		harmony.Patch(hudManagerUpdatePatchPostfix,
			postfix: new HarmonyMethod(hubManagerUpdatePatchPostfixPatch));

		// 酸素枯渇発動時アサシンは常にマスクを持つパッチ
		harmony.Patch(submarineOxygenSystemDetoriorate,
			postfix: new HarmonyMethod(submarineOxygenSystemDetorioratePostfixPatch));

		// アサシン会議時の暗転を防ぐパッチ
		harmony.Patch(submarineSpawnInSystemDetoriorate,
			postfix: new HarmonyMethod(submarineSpawnInSystemDetorioratePostfixPatch));

		// サブマージドのセキュリティカメラの制限をつけるパッチ
		harmony.Patch(submarineSurvillanceMinigameSystemUpdate,
			new HarmonyMethod(submarineSurvillanceMinigameSystemUpdatePrefixPatch),
			new HarmonyMethod(submarineSurvillanceMinigameSystemUpdatePostfixPatch));

		// 追放が2度発生する不具合の修正
		// このコメントに沿って修正：https://github.com/SubmergedAmongUs/Submerged/issues/123#issuecomment-1783889792
		harmony.Patch(exileControllerPatchesExileControllerBegin,
			new HarmonyMethod(exileControllerPatchesExileControllerBeginPatch));

		// ランダムスポーンを無効化する用
		harmony.Patch(submarineSelectSpawnCoSelectLevel,
			new HarmonyMethod(submarineSelectSpawnCoSelectLevelPatch));
	}

	private MonoBehaviour? getFloorHandler(PlayerControl player)
	{
		object? handlerObj = this.getFloorHandlerInfo.Invoke(null, new object[] { player });

		if (handlerObj == null) { return null; }

		return ((Component)handlerObj).TryCast<MonoBehaviour>();
	}

	private void disableElevator()
	{
		var useElevator = (ElevatorSelection)this.elevatorOption.Value;

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
		if (!this.replaceDoorMinigameOption.Value || CachedShipStatus.Instance == null)
		{ return; }

		object? transformValue = this.submarineStatusReference.GetValue(this.submarineStatus);
		if (transformValue == null ||
			transformValue is not Transform transform) { return; }

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

		foreach (var doorConsole in CachedShipStatus.Instance.GetComponentsInChildren<DoorConsole>())
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