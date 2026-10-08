using ExtremeRoles.Compat;
using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Extension.Player;
using ExtremeRoles.Extension.Vector;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using Hazel;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ExtremeRoles.Roles.Solo.Crewmate;

#nullable enable

public sealed class LoggerRole : SingleRoleBase, IRoleAutoBuildAbility, IRoleUpdate, IRoleResetMeeting
{
	public enum Option
	{
		Range,
		IsVisibleAll,
	}

	public enum RpcType : byte
	{
		SetDetector,
		RemoveDetector,
	}

	public ExtremeAbilityButton? Button { get; set; }

	private float range = 2.5f;
	private bool isVisibleAll = true;
	private int detectorCounter = 0;

	private readonly List<DetectorBehavior> activeDetectors = new List<DetectorBehavior>();
	private readonly List<DetectorData> archivedDetectorLogs = new List<DetectorData>();
	private readonly Dictionary<int, GameObject> remoteDetectorMap = new Dictionary<int, GameObject>();

	private Vector2 playerPos;

	private ExtremeConsoleSystem? consoleSystem;

	private bool wasCommsActive = false;
	private bool wasSabotageActive = false;

	public readonly record struct DetectorData(int IndexNumber, List<string> Logs)
	{
		public readonly List<string> Logs { get; } = [..Logs];

		public DetectorData(int indexNumber) : this(indexNumber, [])
		{
		}
	}

	public sealed class DetectorBehavior(LoggerRole ownerRole, int indexNumber, GameObject detectorObject) : ExtremeConsole.IBehavior
	{
		private readonly LoggerRole ownerRole = ownerRole;
		private readonly GameObject detectorObject = detectorObject;

		public int IndexNumber { get; } = indexNumber;
		public Vector2 Position => this.detectorObject.transform.position;
		public List<string> Logs { get; } = [];
		public HashSet<byte> PlayersInRange { get; } = [];

		public float CoolTime => 0.0f;
		public bool IsCheckWall => true;

		public bool CanUse(NetworkedPlayerInfo pc)
			=> pc.PlayerId == PlayerControl.LocalPlayer.PlayerId &&
				pc.Object.CanMove && !pc.IsDead;

		public void Use()
		{
			var data = new DetectorData(this.IndexNumber, this.Logs);
			this.ownerRole.OnDetectorCollected(this, data);
			DestroyObject();
		}

		public void DestroyObject()
		{
			if (this.detectorObject != null)
			{
				Object.Destroy(this.detectorObject);
			}
		}
	}

	public LoggerRole() : base(
		RoleArgs.BuildCrewmate(
			ExtremeRoleId.Logger,
			ColorPalette.LoggerGreen))
	{
	}

	public void CreateAbility()
	{
		this.CreateActivatingAbilityCountButton(
			"LoggerDetectorButton",
			UnityObjectLoader.LoadFromResources(ExtremeRoleId.Logger),
			IsActivating,
			CleanUp,
			() => { },
			isReduceOnActive: true);
		this.Button?.SetLabelToCrewmate();
	}

	public bool IsAbilityUse()
		=> IRoleAbility.IsCommonUse() && Minigame.Instance == null;

	public bool IsActivating()
	{
		var localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer == null)
		{
			return false;
		}

		return this.playerPos.IsCloseTo(localPlayer.GetTruePosition());
	}

	public void CleanUp()
	{
		var localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer == null)
		{
			return;
		}

		this.detectorCounter++;
		int detectorIndex = this.detectorCounter;

		if (this.isVisibleAll)
		{
			using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.LoggerOps))
			{
				caller.WriteByte((byte)RpcType.SetDetector);
				caller.WriteByte(localPlayer.PlayerId);
				caller.WriteInt(detectorIndex);
				caller.WriteFloat(this.playerPos.x);
				caller.WriteFloat(this.playerPos.y);
			}
		}

		this.CreateDetector(detectorIndex, this.playerPos);
	}

	public bool UseAbility()
	{
		var localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer == null)
		{
			return false;
		}

		this.playerPos = localPlayer.GetTruePosition();
		return true;
	}

	public static void RpcOps(MessageReader reader)
	{
		RpcType type = (RpcType)reader.ReadByte();
		byte ownerPlayerId = reader.ReadByte();

		if (!ExtremeRoleManager.TryGetSafeCastedRole<LoggerRole>(ownerPlayerId, out var ownerLogger) || ownerLogger == null)
		{
			return;
		}

		switch (type)
		{
			case RpcType.SetDetector:
				int index = reader.ReadInt32();
				float x = reader.ReadSingle();
				float y = reader.ReadSingle();
				Vector2 pos = new Vector2(x, y);

				ownerLogger.OnRpcSetDetector(index, pos, ownerPlayerId);
				break;

			case RpcType.RemoveDetector:
				int removeIndex = reader.ReadInt32();
				ownerLogger.OnRpcRemoveDetector(removeIndex);
				break;
		}
	}

	public void OnRpcSetDetector(int index, Vector2 pos, byte rolePlayerId)
	{
		var local = PlayerControl.LocalPlayer;
		if (local == null || local.PlayerId == rolePlayerId)
		{
			return;
		}
		var obj = new GameObject($"LoggerDetectorVisual_{this.GameControlId}_{index}");
		obj.transform.position = new Vector3(pos.x, pos.y, pos.y / 1000.0f);
		var sr = obj.AddComponent<SpriteRenderer>();
		setSprite(sr);

		if (CompatModManager.Instance.TryGetModMap(out var modMap))
		{
			modMap.AddCustomComponent(obj, Compat.Interface.CustomMonoBehaviourType.MovableFloorBehaviour);
		}

		this.remoteDetectorMap[index] = obj;
	}

	public void OnRpcRemoveDetector(int removeIndex)
	{
		if (!this.remoteDetectorMap.TryGetValue(removeIndex, out var removeObj))
		{
			return;
		}
		if (removeObj != null)
		{
			Object.Destroy(removeObj);
		}
		this.remoteDetectorMap.Remove(removeIndex);
	}

	public void Update(PlayerControl rolePlayer)
	{
		if (!GameProgressSystem.IsTaskPhase || rolePlayer != PlayerControl.LocalPlayer)
		{
			return;
		}

		this.checkSabotageState();
		this.checkPlayerPositions();
	}

	private void CreateDetector(int detectorIndex, Vector2 pos)
	{
		if (this.consoleSystem is null)
		{
			return;
		}

		var objConsole = this.consoleSystem.CreateConsoleObj(new Vector3(pos.x, pos.y, pos.y / 1000.0f), $"LoggerDetector_{detectorIndex}");
		var behavior = new DetectorBehavior(this, detectorIndex, objConsole.gameObject);
		objConsole.Behavior = behavior;
		setSprite(objConsole.Image!);

		var colider = objConsole.gameObject.AddComponent<CircleCollider2D>();
		colider.isTrigger = true;
		colider.radius = 0.25f;

		if (CompatModManager.Instance.TryGetModMap(out var modMap))
		{
			modMap.AddCustomComponent(objConsole.gameObject, Compat.Interface.CustomMonoBehaviourType.MovableFloorBehaviour);
		}

		this.activeDetectors.Add(behavior);
	}

	private void setSprite(SpriteRenderer sr)
	{
		sr.sprite = UnityObjectLoader.LoadFromResources(ExtremeRoleId.Logger);
	}

	public void OnDetectorCollected(DetectorBehavior detector, DetectorData archivedData)
	{
		if (!this.activeDetectors.Remove(detector))
		{
			return;
		}

		this.archivedDetectorLogs.Add(archivedData);

		if (this.isVisibleAll && PlayerControl.LocalPlayer != null)
		{
			using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.LoggerOps))
			{
				caller.WriteByte((byte)RpcType.RemoveDetector);
				caller.WriteByte(PlayerControl.LocalPlayer.PlayerId);
				caller.WriteInt(detector.IndexNumber);
			}
		}

		if (this.Button?.Behavior is ICountBehavior countBehavior)
		{
			countBehavior.SetAbilityCount(countBehavior.AbilityCount + 1);
		}
	}

	private void checkSabotageState()
	{
		bool isCommsNow = isCommsSabotageActive;
		bool isAnySaboNow = isAnySabotageActive;

		if (!this.wasCommsActive && isCommsNow)
		{
			this.addCommsLogAll();
		}
		else if (!this.wasSabotageActive && isAnySaboNow && !isCommsNow)
		{
			this.addSabotageLogAll();
		}

		this.wasCommsActive = isCommsNow;
		this.wasSabotageActive = isAnySaboNow;
	}

	private void addCommsLogAll()
	{
		string logEntry = Tr.GetString("LoggerLogNoData");
		foreach (var det in this.activeDetectors)
		{
			det.Logs.Add(logEntry);
		}
	}

	private void addSabotageLogAll()
	{
		string logEntry = Tr.GetString("LoggerLogSabotage");
		foreach (var det in this.activeDetectors)
		{
			det.Logs.Add(logEntry);
		}
	}

	private void checkPlayerPositions()
	{
		bool isCommsActive = isCommsSabotageActive;

		foreach (var det in this.activeDetectors)
		{
			foreach (var player in PlayerControl.AllPlayerControls)
			{
				if (player == null)
				{
					continue;
				}

				if (player.IsInValid() || player.inVent)
				{
					det.PlayersInRange.Remove(player.PlayerId);
					continue;
				}

				float dist = Vector2.Distance(player.GetTruePosition(), det.Position);
				if (dist <= this.range)
				{
					if (det.PlayersInRange.Add(player.PlayerId) &&
						!isCommsActive)
					{
						string logEntry = Tr.GetString("LoggerLogPlayerPass", player.Data.DefaultOutfit.PlayerName);
						det.Logs.Add(logEntry);
					}
				}
				else
				{
					det.PlayersInRange.Remove(player.PlayerId);
				}
			}
		}
	}

	private static bool isCommsSabotageActive
		=> PlayerControl.LocalPlayer != null && PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer);

	private static bool isAnySabotageActive
		=> ShipStatus.Instance != null &&
			ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Sabotage, out var system) &&
			system.IsTryCast<SabotageSystemType>(out var saboSystem) ? saboSystem.AnyActive : false;

	public void ResetOnMeetingStart()
	{
		var localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer == null || localPlayer.Data == null || localPlayer.Data.IsDead)
		{
			return;
		}

		var allReports = new List<DetectorData>();
		foreach (var det in this.activeDetectors)
		{
			allReports.Add(new (det.IndexNumber, det.Logs));
		}
		allReports.AddRange(this.archivedDetectorLogs);

		allReports.Sort((a, b) => a.IndexNumber.CompareTo(b.IndexNumber));

		if (allReports.Count == 0)
		{
			return;
		}

		var sb = new StringBuilder();
		foreach (var report in allReports)
		{
			string header = Tr.GetString("LoggerDetectorHeader", report.IndexNumber);
			sb.AppendLine(header);

			if (report.Logs.Count == 0)
			{
				string noLog = Tr.GetString("LoggerLogNone");
				sb.AppendLine(noLog);
			}
			else
			{
				foreach (string log in report.Logs)
				{
					sb.AppendLine(log);
				}
			}
		}

		MeetingReporter.Instance.AddMeetingChatReport(sb.ToString().TrimEnd());

		foreach (var det in this.activeDetectors)
		{
			det.Logs.Clear();
		}
		this.archivedDetectorLogs.Clear();
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		IRoleAbility.CreateAbilityCountOption(factory, 2, 20, 3.0f);
		factory.CreateFloatOption(Option.Range, 2.5f, 0.5f, 10.0f, 0.5f);
		factory.CreateBoolOption(Option.IsVisibleAll, true);
	}

	protected override void RoleSpecificInit()
	{
		this.detectorCounter = 0;

		this.wasCommsActive = false;
		this.wasSabotageActive = false;

		this.consoleSystem = ExtremeConsoleSystem.Create();

		var loader = this.Loader;
		this.range = loader.GetValue<Option, float>(Option.Range);
		this.isVisibleAll = loader.GetValue<Option, bool>(Option.IsVisibleAll);
	}
}
