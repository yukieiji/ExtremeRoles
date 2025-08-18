using System;
using System.Collections;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.IntroRunner;
using ExtremeRoles.Helper;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Extension.State;

using BepInEx.Unity.IL2CPP.Utils.Collections;

using PlayerIl2CppList = Il2CppSystem.Collections.Generic.List<PlayerControl>;
using Il2CppIEnumerator = Il2CppSystem.Collections.IEnumerator;

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class IntroCutsceneModder : MonoBehaviour
{
	public IntroCutsceneModder(IntPtr ptr) : base(ptr) { }

	public void BeginCrewmatePrefix(
		IntroCutscene instance, ref PlayerIl2CppList crewTeam)
	{
		commonBeginPrefix(instance, ref crewTeam);
	}

	public void BeginImpostorPrefix(
		IntroCutscene instance, ref PlayerIl2CppList impTeam)
	{
		commonBeginPrefix(instance, ref impTeam);
		addFakeTeam(impTeam, ExtremeRoleType.Impostor);
	}

	public void BeginCrewmatePostfix(
		IntroCutscene instance)
	{
		CommonBeginPostfix(instance);
	}

	public void BeginImpostorPostfix(
		IntroCutscene instance)
	{
		CommonBeginPostfix(instance);
	}

	public bool CoBeginPrefix(
		IntroCutscene instance, ref Il2CppIEnumerator __result)
	{
		IIntroRunner runnner = ExtremeGameModeManager.Instance.GetIntroRunner();
		if (runnner is null)
		{
			return true;
		}

		__result = runnner.CoRunIntro(instance).WrapToIl2Cpp();
		return false;
	}

	public bool ShowRolePrefix(
		IntroCutscene instance, ref Il2CppIEnumerator __result)
	{
		var role = ExtremeRoleManager.GetLocalPlayerRole();
		if (role.IsVanillaRole() ||
			(role is IRoleAwake<RoleTypes> awakeVanillaRole && !awakeVanillaRole.IsAwake))
		{
			return true;
		}

		__result = showRoleText(role, instance).WrapToIl2Cpp();
		return false;
	}

	private static void commonBeginPrefix(IntroCutscene instance, ref PlayerIl2CppList yourTeam)
	{
		setupIntroTeamIcons(ref yourTeam);
		setupPlayerPrefab(instance);
	}

	public static void CommonBeginPostfix(IntroCutscene instance)
	{
		setupIntroTeam(instance);
		setupRole();
	}

	private static void addFakeTeam(PlayerIl2CppList team, ExtremeRoleType temaId)
	{
		byte localPlayerId = PlayerControl.LocalPlayer.PlayerId;
		foreach (var (playerId, role) in ExtremeRoleManager.GameRole)
		{
			if (playerId == localPlayerId)
			{
				continue;
			}
			if (tryAddFakeTeam(playerId, role, team, temaId))
			{
				continue;
			}

			if (role is MultiAssignRoleBase multiAssignRole &&
			    multiAssignRole.AnotherRole is not null &&
			    tryAddFakeTeam(playerId, multiAssignRole.AnotherRole, team, temaId))
			{
				continue;
			}
		}
	}

	private static bool tryAddFakeTeam(
		byte playerId,
		SingleRoleBase role,
		PlayerIl2CppList impTeam,
		ExtremeRoleType teamId)
	{
		if (role.Status is IRoleFakeIntro mainFake &&
			mainFake.FakeTeam == teamId)
		{
			var player = Player.GetPlayerControlById(playerId);
			impTeam.Add(player);
			Logging.Debug($"Add fake {teamId}: {playerId}");
			return true;
		}
		return false;
	}

	private static void setupIntroTeam(IntroCutscene instance)
	{
		var role = ExtremeRoleManager.GetLocalPlayerRole();
		var text = instance.TeamTitle;

		if (role.IsNeutral())
		{
			var (main, sub) = ExtremeRoleManager.GetInterfaceCastedLocalRole<IRoleAwake<RoleTypes>>();
			if ((main is not null && !main.IsAwake) ||
				(sub is not null && !sub.IsAwake))
			{
				return;
			}

			instance.BackgroundBar.material.color = ColorPalette.NeutralColor;
			instance.ImpostorText.text = Tr.GetString("neutralIntro");

			text.text = Tr.GetString("Neutral");
			text.color = ColorPalette.NeutralColor;
		}
		else if (role.Core.Id is ExtremeRoleId.Xion)
		{
			instance.BackgroundBar.material.color = ColorPalette.XionBlue;
			instance.ImpostorText.text = Tr.GetString("youAreNewRuleEditor");

			text.text = Tr.GetString("yourHost");
			text.color = ColorPalette.XionBlue;
		}
		else if (role.IsLiberal())
		{
			instance.BackgroundBar.material.color = ColorPalette.LiberalColor;
			instance.ImpostorText.text = Tr.GetString("liberalIntro");

			text.text = Tr.GetString("Liberal");
			text.color = ColorPalette.LiberalColor;
		}
	}

	private static void setupRole()
	{
		var localRole = ExtremeRoleManager.GetLocalPlayerRole();
		if (localRole is IRoleSpecialSetUp setUpRole)
		{
			setUpRole.IntroBeginSetUp();
		}

		if (localRole is MultiAssignRoleBase multiAssignRole &&
			multiAssignRole.AnotherRole is IRoleSpecialSetUp multiSetUpRole)
		{
			multiSetUpRole.IntroBeginSetUp();
		}
	}

	public static void setupPlayerPrefab(IntroCutscene __instance)
	{
		Prefab.PlayerPrefab = Instantiate(
			__instance.PlayerPrefab);
		DontDestroyOnLoad(Prefab.PlayerPrefab);
		Prefab.PlayerPrefab.name = "poolablePlayerPrefab";
		Prefab.PlayerPrefab.gameObject.SetActive(false);
	}

	public static void setupIntroTeamIcons(
		ref PlayerIl2CppList yourTeam)
	{

		var role = ExtremeRoleManager.GetLocalPlayerRole();

        if (role.IsLiberal())
        {
            yourTeam.Clear();
            foreach (var p in Player.AllPlayerControls)
            {
                if (ExtremeRoleManager.TryGetRole(p.PlayerId, out var r) && r.IsLiberal())
                {
                    yourTeam.Add(p);
                }
            }
        }
		// Intro solo teams
		else if (role.IsNeutral() || role.Id is ExtremeRoleId.Xion)
		{
			var (main, sub) = ExtremeRoleManager.GetInterfaceCastedLocalRole<IRoleAwake<RoleTypes>>();
			if ((main is not null && !main.IsAwake) || (sub is not null && !sub.IsAwake))
			{
				return;
			}

			var soloTeam = new PlayerIl2CppList();
			soloTeam.Add(PlayerControl.LocalPlayer);
			yourTeam = soloTeam;
		}
	}

	private static IEnumerator showRoleText(
		SingleRoleBase role,
		IntroCutscene instance)
	{
		var text = instance.RoleBlurbText;
		var youAreText = instance.YouAreText;
		var roleText = instance.RoleText;

		youAreText.color = role.GetNameColor();
		roleText.text = role.GetColoredRoleName();
		roleText.color = role.GetNameColor();

		text.color = role.GetNameColor();
		string desc = role.GetIntroDescription();

		if (role.Core.Id is ExtremeRoleId.Lover
			or ExtremeRoleId.Sharer
			or ExtremeRoleId.Buddy)
		{
			if (role is MultiAssignRoleBase multiAssignRole &&
				multiAssignRole.AnotherRole != null)
			{
				text.fontSize *= 0.45f;
			}
			if (role.IsImpostor())
			{
				desc =
					$"{desc}\n{Tr.GetString("impostorIntroText")}";
			}
			else if (role.IsCrewmate() && role.HasTask())
			{
				desc = $"{desc}\n{Tr.GetString("crewIntroText")}";
			}
		}
		text.text = desc;

		SoundManager.Instance.PlaySound(
			PlayerControl.LocalPlayer.Data.Role.IntroSound, false, 1f);

		youAreText.gameObject.SetActive(true);
		roleText.gameObject.SetActive(true);
		text.gameObject.SetActive(true);

		var crewmate = instance.ourCrewmate;
		if (crewmate == null)
		{
			crewmate = instance.CreatePlayer(
				0, 1, PlayerControl.LocalPlayer.Data, false);
			crewmate.gameObject.SetActive(false);
		}
		crewmate.gameObject.SetActive(true);
		crewmate.transform.localPosition = new Vector3(0f, -1.05f, -18f);
		crewmate.transform.localScale = new Vector3(1f, 1f, 1f);

		yield return new WaitForSeconds(2.5f);

		youAreText.gameObject.SetActive(false);
		roleText.gameObject.SetActive(false);
		text.gameObject.SetActive(false);
		crewmate.gameObject.SetActive(false);
	}
}
