using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using UnityEngine;

using ExtremeRoles.Extension.Vector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;
using static ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus;
using ExtremeRoles.Module.CustomOption.Factory;


#nullable enable

namespace ExtremeRoles.Roles.Combination.InvestigatorOffice;

public readonly record struct CrimeInfo(
	byte Target,
	byte Killer,
	Vector2 Pos,
	DateTime KilledTime,
	PlayerStatus Reason,
	float ReportTime,
	ExtremeRoleType KillerTeam,
	ExtremeRoleId KillerRole,
	RoleTypes KillerVanillaRole);

public sealed class Investigator : MultiAssignRoleBase, IRoleMurderPlayerHook, IRoleResetMeeting, IRoleReportHook, IRoleUpdate, IRoleSpecialReset
{
	public enum SearchCond : byte
	{
		None,
		FindKillTime,
		FindReportTime,
		FindReson,
		FindTeam,
		FindRole,
		FindName,
	}

	public enum DetectiveOption
	{
		SearchRange,
		SearchTime,
		SearchAssistantTime,
		ContinueSearch,
		SearchCanFindName,
		SearchCanContineMeetingNum,
		ForceMeetingOnSearchEnd,
		TextShowTime,
	}

	public sealed class ProgressCrimeInfo(CrimeInfo crime, byte reporter)
	{
		public CrimeInfo Crime { get; } = crime;
		public byte Reporter { get; } = reporter;

		public int MeetingCount { get; set; } = 0;
		public SearchCond Progress { get; set; } = SearchCond.None;
		public float SearchTime { get; set; } = 0.0f;
	}

	public sealed class ProgressCrimeContainer(int maxMeetingNum)
	{
		private readonly Dictionary<byte, ProgressCrimeInfo> crimeInfo = [];
		private readonly Dictionary<byte, Arrow> arrow = [];

		private readonly int maxMeetingNum = maxMeetingNum;

		public void IncreseMeetingNum()
		{
			var remove = new HashSet<byte>();
			foreach (var (id, info) in this.crimeInfo)
			{
				info.MeetingCount++;
				if (info.MeetingCount > maxMeetingNum)
				{
					remove.Add(id);
				}
			}

			foreach (byte id in remove)
			{
				if (this.arrow.TryGetValue(id, out var arrow))
				{
					arrow.Clear();
					this.arrow.Remove(id);
				}

				if (this.crimeInfo.ContainsKey(id))
				{
					this.crimeInfo.Remove(id);
				}
			}
		}

		public bool TryGetNearCrime(Vector2 pos, float range, [NotNullWhen(true)] out ProgressCrimeInfo? info)
		{
			info = null;

			foreach (var crime in this.crimeInfo.Values)
			{
				Vector2 vector = pos - crime.Crime.Pos;
				float magnitude = vector.magnitude;
				if (magnitude <= range &&
					!PhysicsHelpers.AnyNonTriggersBetween(
						pos, vector.normalized,
						magnitude, Constants.ShipAndObjectsMask))
				{
					// rangeを設定された値から変更していくことで最小の犯罪位置がわかる
					range = magnitude;
					info = crime;
				}
			}
			return info is not null;
		}

		public void HideArrow()
		{
			foreach (var arrow in this.arrow.Values)
			{
				arrow.SetActive(false);
			}
		}

		public void ShowArrow()
		{
			foreach (var arrow in this.arrow.Values)
			{
				arrow.SetActive(true);
			}
		}

		public void Clear()
		{
			this.crimeInfo.Clear();
			foreach (var arrow in this.arrow.Values)
			{
				arrow.SetActive(false);
				arrow.Clear();
			}
			this.arrow.Clear();
		}

		public void Add(CrimeInfo info, byte reporter)
		{
			this.crimeInfo[info.Target] = new ProgressCrimeInfo(info, reporter);

			var arrow = new Arrow(ColorPalette.InvestigatorKokikou);
			arrow.UpdateTarget(info.Pos);
			this.arrow[info.Target] = arrow;
		}

		public void Remove(byte playerId)
		{
			if (this.arrow.TryGetValue(playerId, out var arrow))
			{
				arrow.SetActive(false);
				arrow.Clear();
				this.arrow.Remove(playerId);
			}
			this.crimeInfo.Remove(playerId);
		}
	}

	public sealed class CrimeProgressUpdator(float searchAssistantTime, float searchCrimeInfoTime)
	{
		public ProgressCrimeInfo? Info
		{
			get
			{
				return this.info;
			}
			set
			{
				this.info = value;
				if (this.info is not null &&
					this.info.SearchTime <= 0.0f &&
					this.info.Progress is SearchCond.None)
				{
					resetTimer(this.info);
				}
			}
		}
		private ProgressCrimeInfo? info = null;

		private readonly float searchAssistantTime = searchAssistantTime;
		private readonly float searchCrimeInfoTime = searchCrimeInfoTime;

		public bool TryUpdate(float deltaTime)
		{
			if (this.info is null)
			{
				return false;
			}
			
			this.info.SearchTime -= deltaTime;
			
			if (this.info.SearchTime > 0.0f)
			{
				return false;
			}
			this.info.Progress++;
			return true;
		}

		private void resetTimer(in ProgressCrimeInfo info)
		{
			info.SearchTime = ExtremeRoleManager.TryGetSafeCastedRole<Assistant>(
				info.Reporter, out var _) ?
				this.searchAssistantTime :
				this.searchCrimeInfoTime;
		}
	}

	private sealed record CrimeSearchInfo(ProgressCrimeContainer AllTarget, CrimeProgressUpdator ProgressUpdater);

	public override IStatusModel? Status => this.status;
	private InvestigatorStatus? status;
	private CrimeSearchInfo? searchInfo;

	private float range;

	private TextPopUpper? textPopUp;
	private TMPro.TextMeshPro? searchText;

	private Vector2 prevPlayerPos;
	private static readonly Vector2 defaultPos = new Vector2(100.0f, 100.0f);

	private bool includeName;
	private bool canContinue;
	private bool forceMeetingOnSearchEnd;

	public Investigator() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Investigator,
			ColorPalette.InvestigatorKokikou),
		false, true, false, false,
		tab: OptionTab.CombinationTab)
	{ }

	public void AllReset(PlayerControl rolePlayer)
	{
		upgradeAssistant();
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{

	}

	public void ResetOnMeetingStart()
	{
		hideSearchText();
		this.searchInfo?.AllTarget.HideArrow();
		this.textPopUp?.Clear();
	}

	public void HookReportButton(
		PlayerControl rolePlayer,
		NetworkedPlayerInfo reporter)
	{
		this.searchInfo?.AllTarget.IncreseMeetingNum();
	}

	public void HookBodyReport(
		PlayerControl rolePlayer,
		NetworkedPlayerInfo reporter,
		NetworkedPlayerInfo reportBody)
	{
		if (this.status is null ||
			this.searchInfo is null ||
			!this.status.TryGetCrime(reportBody.PlayerId, out var crime))
		{
			return;
		}
		
		this.searchInfo.AllTarget.IncreseMeetingNum();

		this.status.Clear();
		this.searchInfo.AllTarget.Add(crime, reporter.PlayerId);
	}

	public void HookMuderPlayer(
		PlayerControl source, PlayerControl target)
	{
		this.status?.AddCrime(source, target);
	}

	public void Update(PlayerControl rolePlayer)
	{
		if (this.searchInfo is null ||
			rolePlayer == null ||
			rolePlayer.Data == null ||
			rolePlayer.Data.IsDead ||
			rolePlayer.Data.Disconnected ||
			MeetingHud.Instance != null ||
			ExileController.Instance != null)
		{
			return;
		}

		this.status?.Upate(Time.deltaTime);

		if (this.searchInfo.ProgressUpdater.Info is null)
		{
			updateNoneSearchCrime(this.searchInfo, rolePlayer);
		}
		else
		{
			// 調査中
			searchCrime(this.searchInfo, rolePlayer);
		}
	}

	private void updateNoneSearchCrime(
		CrimeSearchInfo searchInfo,
		PlayerControl rolePlayer)
	{
		var curPos = rolePlayer.GetTruePosition();
		if (this.prevPlayerPos.IsCloseTo(defaultPos))
		{
			this.prevPlayerPos = curPos;
		}

		// 調査開始
		if (this.prevPlayerPos.IsCloseTo(curPos) &&
			searchInfo.AllTarget.TryGetNearCrime(curPos, this.range, out var info))
		{
			searchInfo.AllTarget.HideArrow();
			searchInfo.ProgressUpdater.Info = info;
			return;
		}

		searchInfo.AllTarget.ShowArrow();
		this.prevPlayerPos = rolePlayer.GetTruePosition();
	}

	private void searchCrime(
		CrimeSearchInfo searchInfo,
		PlayerControl rolePlayer)
	{
		if (searchInfo.ProgressUpdater.Info is null)
		{
			return;
		}

		var targetInfo = searchInfo.ProgressUpdater.Info;
		var curPos = rolePlayer.GetTruePosition();
		if (prevPlayerPos.IsNotCloseTo(curPos) ||
			!searchInfo.AllTarget.TryGetNearCrime(curPos, this.range, out var info) ||
			info.Crime != targetInfo.Crime)
		{
			// 調査が外れたとする
			hideSearchText();
			if (this.canContinue)
			{
				targetInfo.Progress = SearchCond.None;
				targetInfo.SearchTime = 0.0f;
			}
			searchInfo.ProgressUpdater.Info = null;
			return;
		}

		searchInfo.AllTarget.HideArrow();

		updateSearchText(targetInfo.SearchTime);

		if (!searchInfo.ProgressUpdater.TryUpdate(Time.deltaTime))
		{
			return;
		}

		showSearchResultText(targetInfo.Progress, targetInfo.Crime);
		if (targetInfo.Progress is SearchCond.FindName ||
			(targetInfo.Progress is SearchCond.FindRole && this.includeName))
		{
			// 調査完了
			searchInfo.AllTarget.Remove(targetInfo.Crime.Target);
			searchInfo.ProgressUpdater.Info = null;
			hideSearchText();
			if (this.forceMeetingOnSearchEnd)
			{
				rolePlayer.StartCoroutine(
					cmdReport(rolePlayer).WrapToIl2Cpp());
			}
		}
	}

	private static IEnumerator cmdReport(PlayerControl rolePlayer)
	{
		yield return new WaitForSeconds(1.0f);
		if (MeetingHud.Instance == null &&
			rolePlayer != null &&
			rolePlayer.Data != null &&
			!rolePlayer.Data.IsDead &&
			!rolePlayer.Data.Disconnected)
		{
			rolePlayer.CmdReportDeadBody(null);
		}
	}

	public override void RolePlayerKilledAction(
		PlayerControl rolePlayer, PlayerControl killerPlayer)
	{
		this.searchInfo?.AllTarget.HideArrow();
		upgradeAssistant();
	}

	public override void ExiledAction(PlayerControl rolePlayer)
	{
		upgradeAssistant();
	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		factory.CreateNewFloatOption(
			DetectiveOption.SearchRange,
			1.0f, 0.5f, 2.8f, 0.1f);

		factory.CreateNewFloatOption(
			DetectiveOption.SearchTime,
			6.0f, 3.0f, 10.0f, 0.1f,
			format: OptionUnit.Second);

		factory.CreateNewFloatOption(
			DetectiveOption.SearchAssistantTime,
			4.0f, 2.0f, 7.5f, 0.1f,
			format: OptionUnit.Second);
		factory.CreateNewBoolOption(
			DetectiveOption.ContinueSearch,
			false);
		factory.CreateNewBoolOption(
			DetectiveOption.SearchCanFindName,
			false);
		factory.CreateNewIntOption(
			DetectiveOption.SearchCanContineMeetingNum,
			1, 1, 10, 1);
		factory.CreateNewBoolOption(
			DetectiveOption.ForceMeetingOnSearchEnd,
			false);
		factory.CreateNewFloatOption(
			DetectiveOption.TextShowTime,
			60.0f, 5.0f, 120.0f, 0.1f,
			format: OptionUnit.Second);
	}

	protected override void RoleSpecificInit()
	{
		var loader = Loader;
		this.range = loader.GetValue<DetectiveOption, float>(
			DetectiveOption.SearchRange);
		this.forceMeetingOnSearchEnd = loader.GetValue<DetectiveOption, bool>(
			DetectiveOption.ForceMeetingOnSearchEnd);

		var container = new ProgressCrimeContainer(
			loader.GetValue<DetectiveOption, int>(
				DetectiveOption.SearchCanContineMeetingNum));
		var updator = new CrimeProgressUpdator(
			loader.GetValue<DetectiveOption, float>(
				DetectiveOption.SearchAssistantTime),
			loader.GetValue<DetectiveOption, float>(
				DetectiveOption.SearchTime));

		this.includeName = loader.GetValue<DetectiveOption, bool>(
			DetectiveOption.SearchCanFindName);
		this.canContinue = loader.GetValue<DetectiveOption, bool>(
			DetectiveOption.ContinueSearch);

		this.searchInfo = new CrimeSearchInfo(container, updator);
		this.status = new InvestigatorStatus();

		textPopUp = new TextPopUpper(
			4, loader.GetValue<DetectiveOption, float>(DetectiveOption.TextShowTime),
			new Vector3(-3.75f, -2.5f, -250.0f),
			TMPro.TextAlignmentOptions.BottomLeft);
		prevPlayerPos = defaultPos;
	}

	private void hideSearchText()
	{
		if (searchText != null)
		{
			searchText.gameObject.SetActive(false);
		}
	}

	private void showSearchResultText(SearchCond cond, in CrimeInfo info)
	{
		if (textPopUp == null)
		{
			return;
		}

		string showStr = "";
		string key = cond.ToString();
		switch (cond)
		{
			case SearchCond.FindKillTime:
				showStr = Tr.GetString(
					key,
					info.KilledTime.ToString());
				break;
			case SearchCond.FindReportTime:
				showStr = Tr.GetString(
					key, Mathf.CeilToInt(info.ReportTime));
				break;
			case SearchCond.FindReson:
				showStr = Tr.GetString(
					key,
					Tr.GetString(info.Reason.ToString()));
				break;
			case SearchCond.FindTeam:
				showStr = Tr.GetString(
					key, Tr.GetString(info.KillerTeam.ToString()));
				break;
			case SearchCond.FindRole:
				var role = info.KillerRole;
				string roleStr = Tr.GetString(info.KillerRole.ToString());
				if (role == ExtremeRoleId.VanillaRole)
				{
					roleStr = Tr.GetString(info.KillerVanillaRole.ToString());
				}
				showStr = Tr.GetString(key, roleStr);
				break;
			case SearchCond.FindName:
				var player = Player.GetPlayerControlById(info.Killer);
				if (player == null ||
					player.Data == null)
				{
					return;
				}
				showStr = Tr.GetString(key, player.Data.DefaultOutfit.PlayerName);
				break;
			default:
				break;
		}

		textPopUp.AddText(showStr);

	}

	private void updateSearchText(float timer)
	{
		if (this.searchText == null)
		{
			this.searchText = UnityEngine.Object.Instantiate(
				HudManager.Instance.KillButton.cooldownTimerText,
				Camera.main.transform, false);
			this.searchText.transform.localPosition = new Vector3(0.0f, 0.0f, -250.0f);
			this.searchText.enableWordWrapping = false;
		}

		this.searchText.gameObject.SetActive(true);
		this.searchText.text = string.Format(
			Tr.GetString("searchStrBase"), Mathf.CeilToInt(timer));

	}
	private void upgradeAssistant()
	{
		foreach (var (playerId, role) in ExtremeRoleManager.GameRole)
		{
			if (role.Core.Id is not ExtremeRoleId.Assistant ||
				!IsSameControlId(role))
			{ 
				continue;
			}

			var playerInfo = GameData.Instance.GetPlayerById(playerId);
			if (!playerInfo.IsDead && !playerInfo.Disconnected)
			{
				InvestigatorApprentice.ChangeToDetectiveApprentice(playerId);
				break;
			}
		}
	}
}
