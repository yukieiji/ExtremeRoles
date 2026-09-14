using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using ExtremeRoles.Core.Abstract;
using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.IntroRunner;
using ExtremeRoles.Module;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Interface;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using UnityEngine;
using System.Collections;

using Il2CppIEnumerator = Il2CppSystem.Collections.IEnumerator;
using PlayerIl2CppList = Il2CppSystem.Collections.Generic.List<PlayerControl>;

#nullable enable

namespace ExtremeRoles.Patches;

public class IntroCutScenceBeginPatch(IModLogger logger, IGameRuntime gameRuntime)
{
	public static IntroCutScenceBeginPatch Instance
	{
		get
		{
			if (field is null)
			{
				field = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<IntroCutScenceBeginPatch>();
			}
			return field;
		}
	}

	private readonly IModLogger _logger = logger;
	private readonly IGameRuntime _gameRuntime = gameRuntime;

	public void BeginImpostorPrefix(IntroCutscene __instance, ref PlayerIl2CppList yourTeam)
	{
		commonBeginPrefix(__instance, ref yourTeam);
		addFakeTeam(yourTeam, ExtremeRoleType.Impostor);
	}

	public void BeginImpostorPostfix(IntroCutscene __instance)
	{
		CommonBeginPostfix(__instance);
	}

	public void BeginCrewmatePrefix(IntroCutscene __instance, ref PlayerIl2CppList teamToDisplay)
	{
		commonBeginPrefix(__instance, ref teamToDisplay);
	}
	public void BeginCrewmatePostfix(IntroCutscene __instance)
	{
		CommonBeginPostfix(__instance);
	}

	private void commonBeginPrefix(IntroCutscene instance, ref PlayerIl2CppList yourTeam)
	{
		if (!_gameRuntime.TryGetGameContext(out var ctx))
		{
			return;
		}

		setupIntroTeamIcons(ctx, ref yourTeam);
		setupPlayerPrefab(instance);
	}

	public void CommonBeginPostfix(IntroCutscene instance)
	{
		if (!_gameRuntime.TryGetGameContext(out var ctx))
		{
			return;
		}

		setupIntroTeam(ctx, instance);
		setupRole(ctx);
	}

	private void addFakeTeam(PlayerIl2CppList team, ExtremeRoleType temaId)
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

	private bool tryAddFakeTeam(
		byte playerId,
		SingleRoleBase role,
		PlayerIl2CppList impTeam,
		ExtremeRoleType teamId)
	{
		if (role.Status is IRoleFakeIntro mainFake &&
			mainFake.FakeTeam == teamId)
		{
			var player = Helper.Player.GetPlayerControlById(playerId);
			impTeam.Add(player);
			_logger.LogTrace($"Add fake {teamId}: {playerId}");
			return true;
		}
		return false;
	}

	private static void setupIntroTeam(IGameContext ctx, IntroCutscene instance)
	{
		var role = ctx.Roles.GetLocalPlayerRole();
		var text = instance.TeamTitle;

		if (role.IsNeutral())
		{
			var (main, sub) = ctx.Roles.GetInterfaceCastedLocalRole<IRoleAwake<RoleTypes>>();
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
			int targetIndex = RandomGenerator.Instance.Next(3);
			instance.ImpostorText.text = Tr.GetString($"liberalIntro{targetIndex}");

			text.text = Tr.GetString("Liberal");
			text.color = ColorPalette.LiberalColor;
		}
	}

	private void setupRole(IGameContext ctx)
	{
		var localRole = ctx.Roles.GetLocalPlayerRole();
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
		Prefab.PlayerPrefab = UnityEngine.Object.Instantiate(
			__instance.PlayerPrefab);
		UnityEngine.Object.DontDestroyOnLoad(Prefab.PlayerPrefab);
		Prefab.PlayerPrefab.name = "poolablePlayerPrefab";
		Prefab.PlayerPrefab.gameObject.SetActive(false);
	}

	public	void setupIntroTeamIcons(IGameContext ctx,
		ref PlayerIl2CppList yourTeam)
	{

		var role = ctx.Roles.GetLocalPlayerRole();

		if (role.IsLiberal())
		{
			yourTeam.Clear();
			foreach (var p in PlayerCache.AllPlayerControl)
			{
				if (ctx.Roles.TryGetRole(p.PlayerId, out var r) && r.IsLiberal())
				{
					yourTeam.Add(p);
				}
			}
		}
		// Intro solo teams
		else if (role.IsNeutral() || role.Core.Id is ExtremeRoleId.Xion)
		{
			var (main, sub) = ctx.Roles.GetInterfaceCastedLocalRole<IRoleAwake<RoleTypes>>();
			if ((main is not null && !main.IsAwake) || (sub is not null && !sub.IsAwake))
			{
				return;
			}

			var soloTeam = new PlayerIl2CppList();
			soloTeam.Add(PlayerControl.LocalPlayer);
			yourTeam = soloTeam;
		}
	}
}

public class IntroCutScenceCoBeginPatchBody(IGameProgress progress, IGameRuntimeStarter starter)
{
	private readonly IGameProgress _progress = progress;
	private readonly IGameRuntimeStarter _starter = starter;

	public bool CoBeginPrefix(
		IntroCutscene instance, ref Il2CppIEnumerator __result)
	{
		_starter.Start();
		_progress.Current = GameProgressSystem.Progress.IntroStart;

		IIntroRunner? runnner = ExtremeGameModeManager.Instance.GetIntroRunner();
		if (runnner is null)
		{
			return true;
		}

		__result = runnner.CoRunIntro(instance).WrapToIl2Cpp();
		return false;
	}
}

public class IntroCutScenceShowRolePatchBody(IGameRuntime runtime)
{
	private readonly IGameRuntime _runtime = runtime;

	public bool ShowRolePrefix(
		IntroCutscene instance, ref Il2CppIEnumerator __result)
	{
		if (!_runtime.TryGetGameContext(out var ctx))
		{
			return true;
		}

		var role = ctx.Roles.GetLocalPlayerRole();
		if (role.IsVanillaRole() ||
			(role is IRoleAwake<RoleTypes> awakeVanillaRole && !awakeVanillaRole.IsAwake))
		{
			return true;
		}

		__result = showRoleText(role, instance).WrapToIl2Cpp();
		return false;
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


[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
public static class IntroCutsceneBeginImpostorPatch
{
    public static void Prefix(
        IntroCutscene __instance,
        ref PlayerIl2CppList yourTeam)
    {
		IntroCutScenceBeginPatch.Instance.BeginImpostorPrefix(__instance, ref yourTeam);
	}

	public static void Postfix(
		IntroCutscene __instance)
	{
		IntroCutScenceBeginPatch.Instance.BeginImpostorPostfix(__instance);
	}
}

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
public static class BeginCrewmatePatch
{
    public static void Prefix(
        IntroCutscene __instance,
        ref PlayerIl2CppList teamToDisplay)
    {
		IntroCutScenceBeginPatch.Instance.BeginCrewmatePrefix(__instance, ref teamToDisplay);
	}

    public static void Postfix(
        IntroCutscene __instance)
    {
		IntroCutScenceBeginPatch.Instance.BeginCrewmatePostfix(__instance);
	}
}

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
public static class IntroCutsceneCoBeginPatch
{
	private static IntroCutScenceCoBeginPatchBody? _body;

	public static bool Prefix(
        IntroCutscene __instance, ref Il2CppIEnumerator __result)
    {
		if (_body is null)
		{
			_body = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<IntroCutScenceCoBeginPatchBody>();
		}
		return _body.CoBeginPrefix(__instance, ref __result);
	}
}

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CreatePlayer))]
public static class IntroCutsceneCreatePlayerPatch
{
	public static void Postfix(
		ref PoolablePlayer __result,
		[HarmonyArgument(2)] NetworkedPlayerInfo pData,
		[HarmonyArgument(3)] bool impostorPositioning)
	{
		if (!impostorPositioning ||
			VanillaRoleProvider.IsImpostorRole(pData.Role.Role))
		{
			return;
		}

		// おそらくImpostorRedであると思われるがわからないのでImpostorのデフォルトカラーを取り出す
		// 内部的にはfor分使って呼び出してるけどどうせデフォルト役職なので早めに取り出されると思われる
		__result.SetNameColor(
			RoleManager.Instance.GetRole(RoleTypes.Impostor).NameColor);
	}
}


[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.ShowRole))]
public static class IntroCutsceneSetUpRoleTextPatch
{
	private static IntroCutScenceShowRolePatchBody? _body;

	public static bool Prefix(
        IntroCutscene __instance, ref Il2CppIEnumerator __result)
	{
		if (_body is null)
		{
			_body = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<IntroCutScenceShowRolePatchBody>();
		}
		return _body.ShowRolePrefix(__instance, ref __result);
	}
}
