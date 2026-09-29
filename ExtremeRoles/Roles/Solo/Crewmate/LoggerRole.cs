using System.Collections.Generic;

using Hazel;
using UnityEngine;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Extension.Player;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Crewmate;

#nullable enable

public sealed class LoggerRole : SingleRoleBase, IRoleAutoBuildAbility, IRoleUpdate, IRoleResetMeeting
{
	public enum Option
	{
		ActiveTime,
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

	private readonly List<DetectorData> activeDetectors = new List<DetectorData>();
	private readonly List<DetectorLogGroup> archivedDetectorLogs = new List<DetectorLogGroup>();
	private static readonly Dictionary<int, ExtremeConsole> remoteConsoleMap = new Dictionary<int, ExtremeConsole>();

	private readonly ExtremeConsoleSystem consoleSystem = ExtremeConsoleSystem.Create();

	private bool wasCommsActive = false;
	private bool wasSabotageActive = false;

	public sealed class DetectorLogGroup
	{
		public int IndexNumber { get; }
		public List<string> Logs { get; }

		public DetectorLogGroup(int indexNumber)
		{
			this.IndexNumber = indexNumber;
			this.Logs = new List<string>();
		}

		public DetectorLogGroup(int indexNumber, List<string> logs)
		{
			this.IndexNumber = indexNumber;
			this.Logs = new List<string>(logs);
		}
	}

	public sealed class DetectorData
	{
		public int IndexNumber { get; }
		public Vector2 Position { get; }
		public ExtremeConsole Console { get; }
		public DetectorLogGroup LogGroup { get; }
		public HashSet<byte> PlayersInRange { get; }

		public DetectorData(int indexNumber, Vector2 position, ExtremeConsole console)
		{
			this.IndexNumber = indexNumber;
			this.Position = position;
			this.Console = console;
			this.LogGroup = new DetectorLogGroup(indexNumber);
			this.PlayersInRange = new HashSet<byte>();
		}
	}

	public sealed class DetectorBehavior : ExtremeConsole.IBehavior
	{
		private readonly LoggerRole ownerRole;
		private readonly DetectorData detector;

		public float CoolTime => 0.0f;
		public bool IsCheckWall => false;

		public DetectorBehavior(LoggerRole ownerRole, DetectorData detector)
		{
			this.ownerRole = ownerRole;
			this.detector = detector;
		}

		public bool CanUse(NetworkedPlayerInfo pc)
		{
			if (PlayerControl.LocalPlayer == null || pc.Object == null)
			{
				return false;
			}

			return pc.PlayerId == PlayerControl.LocalPlayer.PlayerId &&
				pc.Object.CanMove &&
				!pc.IsDead;
		}

		public void Use()
		{
			this.ownerRole.RemoveDetector(this.detector);
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
		var fastSettings = HudManager.Instance.UseButton.fastUseSettings;
		Sprite buttonImage = fastSettings.TryGetValue(ImageNames.AdminMapButton, out var value) && value != null
			? value.Image
			: fastSettings[ImageNames.UseButton].Image;

		this.CreateActivatingAbilityCountButton(
			"LoggerDetectorButton",
			buttonImage);

		if (this.Button != null)
		{
			this.Button.SetLabelToCrewmate();
		}
	}

	public bool IsAbilityUse()
	{
		return IRoleAbility.IsCommonUse() && Minigame.Instance == null;
	}

	public bool UseAbility()
	{
		if (PlayerControl.LocalPlayer == null)
		{
			return false;
		}

		Vector2 pos = PlayerControl.LocalPlayer.GetTruePosition();

		this.detectorCounter++;
		int detectorIndex = this.detectorCounter;

		if (this.isVisibleAll)
		{
			using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.LoggerOps))
			{
				caller.WriteByte((byte)RpcType.SetDetector);
				caller.WriteInt(detectorIndex);
				caller.WriteFloat(pos.x);
				caller.WriteFloat(pos.y);
			}
		}

		this.CreateDetector(detectorIndex, pos);
		return true;
	}

	public static void RpcOps(MessageReader reader)
	{
		RpcType type = (RpcType)reader.ReadByte();
		switch (type)
		{
			case RpcType.SetDetector:
				int index = reader.ReadInt32();
				float x = reader.ReadSingle();
				float y = reader.ReadSingle();
				Vector2 pos = new Vector2(x, y);

				var consoleSystem = ExtremeConsoleSystem.Create();
				var console = consoleSystem.CreateConsoleObj(pos, $"LoggerDetector_{index}");
				remoteConsoleMap[index] = console;
				break;

			case RpcType.RemoveDetector:
				int removeIndex = reader.ReadInt32();
				if (remoteConsoleMap.TryGetValue(removeIndex, out var removeConsole))
				{
					if (removeConsole != null && removeConsole.gameObject != null)
					{
						Object.Destroy(removeConsole.gameObject);
					}
					remoteConsoleMap.Remove(removeIndex);
				}
				break;
		}
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
		var objConsole = this.consoleSystem.CreateConsoleObj(pos, $"LoggerDetector_{detectorIndex}");
		var data = new DetectorData(detectorIndex, pos, objConsole);
		var behavior = new DetectorBehavior(this, data);
		objConsole.Behavior = behavior;

		if (!this.isVisibleAll)
		{
			if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.PlayerId != (byte)this.GameControlId)
			{
				objConsole.gameObject.SetActive(false);
			}
		}

		this.activeDetectors.Add(data);
	}

	public void RemoveDetector(DetectorData detector)
	{
		if (this.activeDetectors.Remove(detector))
		{
			this.archivedDetectorLogs.Add(detector.LogGroup);

			if (this.isVisibleAll)
			{
				using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.LoggerOps))
				{
					caller.WriteByte((byte)RpcType.RemoveDetector);
					caller.WriteInt(detector.IndexNumber);
				}
			}

			if (detector.Console != null && detector.Console.gameObject != null)
			{
				Object.Destroy(detector.Console.gameObject);
			}

			if (this.Button != null && this.Button.Behavior is ICountBehavior countBehavior)
			{
				countBehavior.SetAbilityCount(countBehavior.AbilityCount + 1);
			}
		}
	}

	private void checkSabotageState()
	{
		bool isCommsNow = isCommsSabotageActive();
		bool isAnySaboNow = isAnySabotageActive();

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
			det.LogGroup.Logs.Add(logEntry);
		}
	}

	private void addSabotageLogAll()
	{
		string logEntry = Tr.GetString("LoggerLogSabotage");
		foreach (var det in this.activeDetectors)
		{
			det.LogGroup.Logs.Add(logEntry);
		}
	}

	private void checkPlayerPositions()
	{
		bool isCommsActive = isCommsSabotageActive();

		foreach (var det in this.activeDetectors)
		{
			foreach (var player in PlayerControl.AllPlayerControls)
			{
				if (player == null || player.Data == null || player.Data.IsDead || player.inVent)
				{
					det.PlayersInRange.Remove(player.PlayerId);
					continue;
				}

				float dist = Vector2.Distance(player.GetTruePosition(), det.Position);
				if (dist <= this.range)
				{
					if (det.PlayersInRange.Add(player.PlayerId))
					{
						if (!isCommsActive)
						{
							string template = Tr.GetString("LoggerLogPlayerPass");
							string logEntry = string.Format(template, player.Data.PlayerName);
							det.LogGroup.Logs.Add(logEntry);
						}
					}
				}
				else
				{
					det.PlayersInRange.Remove(player.PlayerId);
				}
			}
		}
	}

	private static bool isCommsSabotageActive()
	{
		if (ShipStatus.Instance != null && ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Comms, out var system) && system != null)
		{
			if (system.IsTryCast<HudOverrideSystemType>(out var hudSabo) && hudSabo != null)
			{
				return hudSabo.IsActive;
			}

			if (system.IsTryCast<HqHudSystemType>(out var hqSabo) && hqSabo != null)
			{
				return hqSabo.IsActive;
			}
		}
		return false;
	}

	private static bool isAnySabotageActive()
	{
		if (ShipStatus.Instance != null && ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Sabotage, out var system) && system != null)
		{
			if (system.IsTryCast<SabotageSystemType>(out var saboSystem) && saboSystem != null)
			{
				return saboSystem.AnyActive;
			}
		}
		return false;
	}

	public void ResetOnMeetingStart()
	{
		var localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer == null || localPlayer.Data == null || localPlayer.Data.IsDead)
		{
			return;
		}

		var allGroups = new List<DetectorLogGroup>();
		foreach (var det in this.activeDetectors)
		{
			allGroups.Add(det.LogGroup);
		}
		allGroups.AddRange(this.archivedDetectorLogs);

		allGroups.Sort((a, b) => a.IndexNumber.CompareTo(b.IndexNumber));

		foreach (var group in allGroups)
		{
			string headerTemplate = Tr.GetString("LoggerDetectorHeader");
			string header = string.Format(headerTemplate, group.IndexNumber);
			MeetingReporter.Instance.AddMeetingChatReport(header);

			if (group.Logs.Count == 0)
			{
				string noLog = Tr.GetString("LoggerLogNone");
				MeetingReporter.Instance.AddMeetingChatReport(noLog);
			}
			else
			{
				foreach (var log in group.Logs)
				{
					MeetingReporter.Instance.AddMeetingChatReport(log);
				}
			}
		}

		foreach (var det in this.activeDetectors)
		{
			det.LogGroup.Logs.Clear();
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
		factory.CreateFloatOption(Option.ActiveTime, 3.0f, 0.0f, 10.0f, 0.5f, format: OptionUnit.Second);
		factory.CreateFloatOption(Option.Range, 2.5f, 0.5f, 10.0f, 0.5f);
		factory.CreateBoolOption(Option.IsVisibleAll, true);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;
		float activeTime = loader.GetValue<Option, float>(Option.ActiveTime);
		this.range = loader.GetValue<Option, float>(Option.Range);
		this.isVisibleAll = loader.GetValue<Option, bool>(Option.IsVisibleAll);

		if (this.Button != null && this.Button.Behavior is IActivatingBehavior activeBehavior)
		{
			activeBehavior.ActiveTime = activeTime;
		}
	}
}
