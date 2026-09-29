using System.Collections.Generic;
using System.Text;

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
	private readonly List<DetectorData> archivedDetectors = new List<DetectorData>();
	private readonly Dictionary<int, GameObject> remoteDetectorMap = new Dictionary<int, GameObject>();

	private readonly ExtremeConsoleSystem consoleSystem = ExtremeConsoleSystem.Create();

	private bool wasCommsActive = false;
	private bool wasSabotageActive = false;

	public sealed class DetectorData
	{
		public int IndexNumber { get; }
		public Vector2 Position { get; }
		public List<string> Logs { get; }
		public HashSet<byte> PlayersInRange { get; }

		public DetectorData(int indexNumber, Vector2 position)
		{
			this.IndexNumber = indexNumber;
			this.Position = position;
			this.Logs = new List<string>();
			this.PlayersInRange = new HashSet<byte>();
		}
	}

	public sealed class DetectorBehavior : ExtremeConsole.IBehavior
	{
		private readonly LoggerRole ownerRole;
		private readonly DetectorData detectorData;
		private readonly GameObject gameObject;

		public float CoolTime => 0.0f;
		public bool IsCheckWall => false;

		public DetectorBehavior(LoggerRole ownerRole, DetectorData detectorData, GameObject gameObject)
		{
			this.ownerRole = ownerRole;
			this.detectorData = detectorData;
			this.gameObject = gameObject;
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
			this.ownerRole.RemoveDetector(this.detectorData, this.gameObject);
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
				caller.WriteByte(PlayerControl.LocalPlayer.PlayerId);
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

				ownerLogger.OnRpcSetDetector(index, pos);
				break;

			case RpcType.RemoveDetector:
				int removeIndex = reader.ReadInt32();
				ownerLogger.OnRpcRemoveDetector(removeIndex);
				break;
		}
	}

	public void OnRpcSetDetector(int index, Vector2 pos)
	{
		if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.PlayerId != (byte)this.GameControlId)
		{
			var obj = new GameObject($"LoggerDetectorVisual_{this.GameControlId}_{index}");
			obj.transform.position = new Vector3(pos.x, pos.y, pos.y / 1000.0f);
			var sr = obj.AddComponent<SpriteRenderer>();
			var fastSettings = HudManager.Instance.UseButton.fastUseSettings;
			if (fastSettings.TryGetValue(ImageNames.AdminMapButton, out var val) && val != null)
			{
				sr.sprite = val.Image;
			}
			this.remoteDetectorMap[index] = obj;
		}
	}

	public void OnRpcRemoveDetector(int removeIndex)
	{
		if (this.remoteDetectorMap.TryGetValue(removeIndex, out var removeObj))
		{
			if (removeObj != null)
			{
				Object.Destroy(removeObj);
			}
			this.remoteDetectorMap.Remove(removeIndex);
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
		var data = new DetectorData(detectorIndex, pos);
		var behavior = new DetectorBehavior(this, data, objConsole.gameObject);
		objConsole.Behavior = behavior;

		this.activeDetectors.Add(data);
	}

	public void RemoveDetector(DetectorData detector, GameObject consoleObj)
	{
		if (this.activeDetectors.Remove(detector))
		{
			this.archivedDetectors.Add(detector);

			if (this.isVisibleAll && PlayerControl.LocalPlayer != null)
			{
				using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.LoggerOps))
				{
					caller.WriteByte((byte)RpcType.RemoveDetector);
					caller.WriteByte(PlayerControl.LocalPlayer.PlayerId);
					caller.WriteInt(detector.IndexNumber);
				}
			}

			if (consoleObj != null)
			{
				Object.Destroy(consoleObj);
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
							det.Logs.Add(logEntry);
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
		if (PlayerControl.LocalPlayer != null && PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			return true;
		}

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

		var allDetectors = new List<DetectorData>();
		allDetectors.AddRange(this.activeDetectors);
		allDetectors.AddRange(this.archivedDetectors);

		allDetectors.Sort((a, b) => a.IndexNumber.CompareTo(b.IndexNumber));

		if (allDetectors.Count == 0)
		{
			return;
		}

		var sb = new StringBuilder();
		foreach (var det in allDetectors)
		{
			string headerTemplate = Tr.GetString("LoggerDetectorHeader");
			string header = string.Format(headerTemplate, det.IndexNumber);
			sb.AppendLine(header);

			if (det.Logs.Count == 0)
			{
				string noLog = Tr.GetString("LoggerLogNone");
				sb.AppendLine(noLog);
			}
			else
			{
				foreach (var log in det.Logs)
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
		this.archivedDetectors.Clear();
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
		this.detectorCounter = 0;
		this.activeDetectors.Clear();
		this.archivedDetectors.Clear();
		this.wasCommsActive = false;
		this.wasSabotageActive = false;

		foreach (var remoteObj in this.remoteDetectorMap.Values)
		{
			if (remoteObj != null)
			{
				Object.Destroy(remoteObj);
			}
		}
		this.remoteDetectorMap.Clear();

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
