using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Combination.DetectiveOffice;

public sealed class Detective : MultiAssignRoleBase, IRoleMurderPlayerHook, IRoleResetMeeting, IRoleReportHook, IRoleUpdate, IRoleSpecialReset
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
		SearchOnlyOnce,
		SearchCanFindName,
		SearchCanContine,
		TextShowTime,
	}

	private CrimeInfoOld? targetCrime;
	private CrimeInfoContainer info;
	private Arrow crimeArrow;
	private float searchTime;
	private float searchAssistantTime;
	private float timer = 0.0f;
	private float searchCrimeInfoTime;
	private float range;
	private SearchCond cond;
	private string searchStrBase;
	private TMPro.TextMeshPro searchText;
	private TextPopUpper textPopUp;
	private Vector2 prevPlayerPos;
	private static readonly Vector2 defaultPos = new Vector2(100.0f, 100.0f);
	private Dictionary<byte, SearchCond> condition = new Dictionary<byte, SearchCond>();

	private bool includeName;
	private bool onlyOnce;
	private bool canContine;

	public Detective() : base(
		ExtremeRoleId.Detective,
		ExtremeRoleType.Crewmate,
		ExtremeRoleId.Detective.ToString(),
		ColorPalette.DetectiveKokikou,
		false, true, false, false,
		tab: OptionTab.CombinationTab)
	{ }

	public void AllReset(PlayerControl rolePlayer)
	{
		upgradeAssistant();
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
	{
		info.Clear();
	}

	public void ResetOnMeetingStart()
	{
		if (crimeArrow != null)
		{
			crimeArrow.SetActive(false);
		}
		resetSearchCond();
	}

	public void HookReportButton(
		PlayerControl rolePlayer,
		NetworkedPlayerInfo reporter)
	{
		targetCrime = null;
		searchCrimeInfoTime = float.MaxValue;
	}

	public void HookBodyReport(
		PlayerControl rolePlayer,
		NetworkedPlayerInfo reporter,
		NetworkedPlayerInfo reportBody)
	{
		targetCrime = info.GetCrimeInfo(reportBody.PlayerId);
		searchCrimeInfoTime = ExtremeRoleManager.TryGetSafeCastedRole<Assistant>(
			reporter.PlayerId, out var _) ?
				searchAssistantTime : searchTime;
	}

	public void HookMuderPlayer(
		PlayerControl source, PlayerControl target)
	{
		info.AddDeadBody(source, target);
	}

	public void Update(PlayerControl rolePlayer)
	{

		if (prevPlayerPos == defaultPos)
		{
			prevPlayerPos = rolePlayer.GetTruePosition();
		}
		if (info != null)
		{
			info.Update();
		}

		if (targetCrime != null)
		{
			if (crimeArrow == null)
			{
				crimeArrow = new Arrow(
					ColorPalette.DetectiveKokikou);
			}

			var crime = targetCrime.Value;
			Vector2 crimePos = crime.Pos;

			crimeArrow.UpdateTarget(crimePos);
			crimeArrow.Update();
			crimeArrow.SetActive(true);

			Vector2 playerPos = rolePlayer.GetTruePosition();

			if (!PhysicsHelpers.AnythingBetween(
					crimePos, playerPos,
					Constants.ShipAndAllObjectsMask, false) &&
				Vector2.Distance(crimePos, playerPos) < range &&
				prevPlayerPos == rolePlayer.GetTruePosition())
			{

				updateSearchText();

				if (timer > 0.0f)
				{
					timer -= Time.deltaTime;
				}
				else
				{
					timer = searchCrimeInfoTime;
					updateSearchCond(crime);
				}
			}
			else
			{
				timer = searchCrimeInfoTime;
				resetSearchCond();
			}
		}
		else
		{
			if (crimeArrow != null)
			{
				crimeArrow.SetActive(false);
			}
		}
		prevPlayerPos = rolePlayer.GetTruePosition();
	}
	public override void RolePlayerKilledAction(
		PlayerControl rolePlayer, PlayerControl killerPlayer)
	{
		upgradeAssistant();
	}

	public override void ExiledAction(PlayerControl rolePlayer)
	{
		upgradeAssistant();
	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		factory.CreateFloatOption(
			DetectiveOption.SearchRange,
			1.0f, 0.5f, 2.8f, 0.1f);

		factory.CreateFloatOption(
			DetectiveOption.SearchTime,
			6.0f, 3.0f, 10.0f, 0.1f,
			format: OptionUnit.Second);

		factory.CreateFloatOption(
			DetectiveOption.SearchAssistantTime,
			4.0f, 2.0f, 7.5f, 0.1f,
			format: OptionUnit.Second);
		factory.CreateBoolOption(
			DetectiveOption.SearchOnlyOnce,
			true);
		factory.CreateBoolOption(
			DetectiveOption.SearchCanFindName,
			false);
		factory.CreateBoolOption(
			DetectiveOption.SearchCanContine,
			false);
		factory.CreateFloatOption(
			DetectiveOption.TextShowTime,
			60.0f, 5.0f, 120.0f, 0.1f,
			format: OptionUnit.Second);
	}

	protected override void RoleSpecificInit()
	{
		cond = SearchCond.None;
		info = new CrimeInfoContainer();
		info.Clear();

		var loader = Loader;
		range = loader.GetValue<DetectiveOption, float>(
			DetectiveOption.SearchRange);
		searchTime = loader.GetValue<DetectiveOption, float>(
			DetectiveOption.SearchTime);
		searchAssistantTime = loader.GetValue<DetectiveOption, float>(
			DetectiveOption.SearchAssistantTime);

		textPopUp = new TextPopUpper(
			4, loader.GetValue<DetectiveOption, float>(DetectiveOption.TextShowTime),
			new Vector3(-3.75f, -2.5f, -250.0f),
			TMPro.TextAlignmentOptions.BottomLeft);
		searchCrimeInfoTime = float.MaxValue;
		prevPlayerPos = defaultPos;
	}

	private void updateSearchCond(CrimeInfoOld info)
	{
		cond++;
		showSearchResultText(info);
		if (cond == SearchCond.FindRole)
		{
			targetCrime = null;
			if (crimeArrow != null)
			{
				crimeArrow.SetActive(false);
			}
			if (searchText != null)
			{
				searchText.gameObject.SetActive(false);
			}
		}
	}
	private void resetSearchCond()
	{
		cond = SearchCond.None;
		if (searchText != null)
		{
			searchText.gameObject.SetActive(false);
		}
		if (textPopUp != null)
		{
			textPopUp.Clear();
		}
	}
	private void showSearchResultText(in CrimeInfoOld info)
	{
		if (textPopUp == null) { return; }

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
				showStr = Tr.GetString(
					key, roleStr);
				if (!includeName)
				{
					cond++;
				}
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

	private void updateSearchText()
	{
		if (searchText == null)
		{
			searchText = UnityEngine.Object.Instantiate(
				HudManager.Instance.KillButton.cooldownTimerText,
				Camera.main.transform, false);
			searchText.transform.localPosition = new Vector3(0.0f, 0.0f, -250.0f);
			searchText.enableWordWrapping = false;
			searchStrBase = Tr.GetString("searchStrBase");
		}

		searchText.gameObject.SetActive(true);
		searchText.text = string.Format(
			searchStrBase, Mathf.CeilToInt(timer));

	}
	private void upgradeAssistant()
	{
		foreach (var (playerId, role) in ExtremeRoleManager.GameRole)
		{
			if (role.Id != ExtremeRoleId.Assistant) { continue; }
			if (!IsSameControlId(role)) { continue; }

			var playerInfo = GameData.Instance.GetPlayerById(playerId);
			if (!playerInfo.IsDead && !playerInfo.Disconnected)
			{
				DetectiveApprentice.ChangeToDetectiveApprentice(playerId);
				break;
			}
		}
	}
}
