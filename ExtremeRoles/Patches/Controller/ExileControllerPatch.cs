using System;
using System.Linq;
using HarmonyLib;

using AmongUs.GameOptions;

using ExtremeRoles.Compat;
using ExtremeRoles.Compat.ModIntegrator;
using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.Option.ShipGlobal.Sub;
using ExtremeRoles.Module;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Interface.Ability;
using ExtremeRoles.Roles.API.Interface;


using Il2CppObject = Il2CppSystem.Object;



#nullable enable

namespace ExtremeRoles.Patches.Controller;

[HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
public static class ExileControllerBeginePatch
{
    private const string TransKeyBase = "ExileText";

	public static void SetExiledTarget(
	   ExileController instance)
	{
		if (instance.specialInputHandler != null)
		{
			instance.specialInputHandler.disableVirtualCursor = true;
		}
		ExileController.Instance = instance;
		ControllerManager.Instance.CloseAndResetAll();

		instance.Text.gameObject.SetActive(false);
		instance.Text.text = string.Empty;

		if (HudManager.Instance != null)
		{
			HudManager.Instance.SetMapButtonEnabled(false);
		}
	}

	/* JAJPs
	[Info   :Extreme Roles] TransKey:ExileTextSP    Value:{0}がインポスターだった。
	[Info   :Extreme Roles] TransKey:ExileTextSN    Value:{0}はインポスターではなかった。
	[Info   :Extreme Roles] TransKey:ExileTextPP    Value:{0}はインポスターだった。
	[Info   :Extreme Roles] TransKey:ExileTextPN    Value:{0}はインポスターではなかった。
	[Info   :Extreme Roles] TransKey:NoExileSkip    Value:誰も追放されなかった。（投票スキップ）
	[Info   :Extreme Roles] TransKey:NoExileTie    Value:誰も追放されなかった。（同数投票）
	[Info   :Extreme Roles] TransKey:ExileTextNonConfirm    Value:{0}が追放された。
	[Info   :Extreme Roles] TransKey:ImpostorsRemainS    Value:インポスターが{0}人残っている。
	[Info   :Extreme Roles] TransKey:ImpostorsRemainP    Value:インポスターが{0}人残っている。
	*/

	[HarmonyPrefix, HarmonyPriority(Priority.Last)]
    public static bool Prefix(
        ExileController __instance,
        [HarmonyArgument(0)] ExileController.InitProperties init)
    {
		if (CompatModManager.Instance.IsModMap<SubmergedIntegrator>())
		{
			return true;
		}
		else
		{
			return PrefixRun(__instance, init);
		}
    }

    public static void Postfix(ExileController __instance)
    {
        if (!MeetingReporter.IsExist ||
			OnemanMeetingSystemManager.IsActive)
		{
			return;
		}

		string reports = MeetingReporter.Instance.GetMeetingEndReport();

		if (string.IsNullOrEmpty(reports))
		{
			return;
		}

        TMPro.TextMeshPro infoText = UnityEngine.Object.Instantiate(
            __instance.ImpostorText,
            __instance.Text.transform);

        float textOffset = GameOptionsManager.Instance.CurrentGameOptions.GetBool(
            BoolOptionNames.ConfirmImpostor) ? -0.4f : -0.2f;

        infoText.transform.localPosition += new UnityEngine.Vector3(0f, textOffset, 0f);
        infoText.gameObject.SetActive(true);
        infoText.text = reports;

        __instance.StartCoroutine(
            Effects.Bloop(0.25f, infoText.transform, 1f, 0.5f));
    }

	public static bool PrefixRun(
		ExileController __instance,
		ExileController.InitProperties init)
	{
		if (!GameProgressSystem.IsGameNow)
		{
			return true; 
		}

		__instance.initData = init;
		if (OnemanMeetingSystemManager.TryGetActiveSystem(out var system))
		{
			system.OverrideExileControllerBegin(__instance);
			return false;
		}
		else if (init.confirmImpostor)
		{
			var shipOption = ExtremeGameModeManager.Instance.ShipOption;
			confirmExile(
				__instance, shipOption.Exile);
			return false;
		}
		return true;
	}

    private static void confirmExile(
        ExileController instance,
        in ExileOption option)
    {
        SetExiledTarget(instance);
		var init = instance.initData;
		bool validExiled = init != null && init.outfit != null;

        string completeString = string.Empty;
        if (validExiled)
        {
			completeString = getExiledString(instance, option);
		}
        else if (init != null)
        {
			completeString = getNoExiledString(instance);
        }

        instance.completeString = completeString;
		instance.ImpostorText.text = createImpostorText(init, validExiled, option.Mode);
        instance.StartCoroutine(instance.Animate());
    }

	private static string createImpostorText(ExileController.InitProperties? init, bool isValidExiled, ConfirmExileMode mode)
	{
		var alivePlayers = GameData.Instance.AllPlayers.ToArray().Where(
			x =>
			{
				return
					(
						(isValidExiled && x.PlayerId != init!.networkedPlayer.PlayerId) ||
						!isValidExiled
					) && !x.IsDead && !x.Disconnected;
			});

		int aliveImpNum = Enumerable.Count(
			alivePlayers,
			(NetworkedPlayerInfo p) =>
				ExtremeRoleManager.TryGetRole(p.PlayerId, out var role) && role.IsImpostor());
		int aliveCrewNum = Enumerable.Count(
			alivePlayers,
			(NetworkedPlayerInfo p) =>
				ExtremeRoleManager.TryGetRole(p.PlayerId, out var role) && role.IsCrewmate());
		int aliveNeutNum = Enumerable.Count(
			alivePlayers,
			(NetworkedPlayerInfo p) =>
				ExtremeRoleManager.TryGetRole(p.PlayerId, out var role) && role.IsNeutral());

		return mode switch
		{
			ConfirmExileMode.Impostor => TranslationController.Instance.GetString(
				aliveImpNum == 1 ? StringNames.ImpostorsRemainS : StringNames.ImpostorsRemainP,
				[aliveImpNum]),

			ConfirmExileMode.Crewmate => Tr.GetString(
				aliveCrewNum == 1 ? "CrewmateRemainS" : "CrewmateRemainP", aliveCrewNum),

			ConfirmExileMode.Neutral => Tr.GetString(
				aliveNeutNum == 1 ? "NeutralRemainS" : "NeutralRemainP", aliveNeutNum),

			ConfirmExileMode.AllTeam => Tr.GetString(
				"AllTeamAlive", aliveCrewNum, aliveImpNum, aliveNeutNum),

			_ => string.Empty
		};
	}

	private static string getExiledString(ExileController controller, in ExileOption option)
	{
		var init = controller.initData;
		bool validExiled = init != null && init.outfit != null;

		NetworkedPlayerInfo? targetExiled = init!.networkedPlayer;
		byte exiledPlayerId = targetExiled.PlayerId;
		if (!ExtremeRoleManager.TryGetRole(exiledPlayerId, out var exiledPlayerRole))
		{
			// Critical error: exiled player's role not found. Log and/or return.
			ExtremeRolesPlugin.Logger.LogError($"Exiled player role not found for ID: {exiledPlayerId}");
			// Depending on desired behavior, might throw or simply not populate text, leading to default display.
			// For now, let's return to prevent further processing with a null role.
			return string.Empty;
		}

		string result = string.Empty;
		// 今後複数役職でIExiledAnimationOverrideが出てきた時に考えろ
		if (exiledPlayerRole.AbilityClass is IExiledAnimationOverrideWhenExiled @override)
		{
			result = @override.AnimationText;
			targetExiled = @override.OverideExiledTarget;
		}
		else if (exiledPlayerRole is MultiAssignRoleBase multiAssignRole &&
			exiledPlayerRole.AbilityClass is IExiledAnimationOverrideWhenExiled @multiOverride)
		{
			result = @multiOverride.AnimationText;
			targetExiled = @multiOverride.OverideExiledTarget;
		}

		if (!string.IsNullOrEmpty(result))
		{
			if (targetExiled != null)
			{
				setUpExiledPlayer(controller, targetExiled.DefaultOutfit);
			}
			else
			{
				controller.Player.gameObject.SetActive(false);
			}
			return result;
		}


		string playerName = init!.outfit!.PlayerName;
		string completeString = string.Empty;

		switch (option.Mode)
		{
			case ConfirmExileMode.AllTeam:
				string team = Tr.GetString(exiledPlayerRole.Core.Team.ToString());
				completeString = option.IsConfirmRole ?
					Tr.GetString("ExileTextAllTeamWithRole", playerName, team, exiledPlayerRole.GetColoredRoleName()) :
					Tr.GetString("ExileTextAllTeam", playerName, team);
				break;
			default:
				completeString = getCompleteString(
					playerName, exiledPlayerRole, in option);
				break;
		}

		setUpExiledPlayer(controller, init.outfit);
		return completeString;
	}

	private static void setUpExiledPlayer(ExileController controller, NetworkedPlayerInfo.PlayerOutfit outfit)
	{
		var player = controller.Player;
		player.UpdateFromPlayerOutfit(outfit, PlayerMaterial.MaskType.Exile, false, false, (Il2CppSystem.Action)(() =>
		{
			var cache = ShipStatus.Instance.CosmeticsCache;
			var skinViewData = GameManager.Instance != null ?
				cache.GetSkin(controller.initData.outfit.SkinId) :
				player.GetSkinView();

			if (GameManager.Instance != null &&
				!HatManager.Instance.CheckLongModeValidCosmetic(
				outfit.SkinId, player.GetIgnoreLongMode()))
			{
				skinViewData = cache.GetSkin("skin_None");
			}
			if (controller.useIdleAnim)
			{
				player.FixSkinSprite(skinViewData.IdleFrame);
				return;
			}
			player.FixSkinSprite(skinViewData.EjectFrame);
		}), false);

		player.ToggleName(false);

		if (!controller.useIdleAnim)
		{
			player.SetCustomHatPosition(controller.exileHatPosition);
			player.SetCustomVisorPosition(controller.exileVisorPosition);
		}
	}

	private static string getNoExiledString(ExileController controller)
	{
		string result = TranslationController.Instance.GetString(
			controller.initData.voteTie ? StringNames.NoExileTie : StringNames.NoExileSkip,
			Array.Empty<Il2CppObject>());
		controller.Player.gameObject.SetActive(false);
		
		return result;
	}

    private static string getSuffix(
        bool isExiledSameMode,
        bool isModeTeamContain)
    {
        string modeTeamSuffix;

        if (isExiledSameMode && isModeTeamContain)
        {
            modeTeamSuffix = "PP";
        }
        else if (isExiledSameMode)
        {
            modeTeamSuffix = "SP";
        }
        else if (isModeTeamContain)
        {
            modeTeamSuffix = "PN";
        }
        else
        {
            modeTeamSuffix = "SN";
        }

        return modeTeamSuffix;
    }

    private static string getCompleteString(
        string playerName,
        SingleRoleBase exiledPlayerRole,
		in ExileOption option)
    {
		var mode = option.Mode;

        string teamSuffix = mode switch
        {
            ConfirmExileMode.Crewmate => "Crew",
            ConfirmExileMode.Neutral => "Neut",
            _ => string.Empty,
        };

        var allPlayer = GameData.Instance.AllPlayers.ToArray();
        bool isExiledSameMode = false;
        int modeTeamAlive = 0;

        switch (mode)
        {
            case ConfirmExileMode.Impostor:
                modeTeamAlive = allPlayer.Count(
					(NetworkedPlayerInfo p) =>
						p != null &&
						ExtremeRoleManager.TryGetRole(p.PlayerId, out var role) &&
						role!.IsImpostor());
                isExiledSameMode = exiledPlayerRole.IsImpostor();
                break;
            case ConfirmExileMode.Crewmate:
                modeTeamAlive = allPlayer.Count(
					(NetworkedPlayerInfo p) =>
						p != null &&
						ExtremeRoleManager.TryGetRole(p.PlayerId, out var role) &&
						role!.IsCrewmate());
                isExiledSameMode = exiledPlayerRole.IsCrewmate();
                break;
            case ConfirmExileMode.Neutral:
                modeTeamAlive = allPlayer.Count(
					(NetworkedPlayerInfo p) =>
						p != null &&
						ExtremeRoleManager.TryGetRole(p.PlayerId, out var role) &&
						role!.IsNeutral());
                isExiledSameMode = exiledPlayerRole.IsNeutral();
                break;
            default:
                break;
        };
        string suffix = getSuffix(isExiledSameMode, modeTeamAlive > 1);
        string transKey = $"{TransKeyBase}{suffix}{teamSuffix}";

        if (Enum.TryParse(transKey, out StringNames sn))
        {
            return TranslationController.Instance.GetString(
                sn, [ playerName ]);
        }
        else
        {
            return
                option.IsConfirmRole ?
                Tr.GetString(
					$"{transKey}WithRole",
                    playerName,
                    exiledPlayerRole.GetColoredRoleName()
                ) :
				Tr.GetString(
					transKey, playerName);
        }
    }
}

[HarmonyPatch(typeof(ExileController), nameof(ExileController.ReEnableGameplay))]
public static class ExileControllerReEnableGameplayPatch
{
    public static void Postfix()
    {
        ReEnablePostfix();
    }

    public static void ReEnablePostfix()
    {
		GameProgressSystem.Current = GameProgressSystem.Progress.Task;
		if (!GameProgressSystem.IsGameNow)
		{
			return;
		}

        MeetingReporter.Reset();

        var role = ExtremeRoleManager.GetLocalPlayerRole();

        if (role.TryGetKillCool(out float killCool))
		{
			PlayerControl.LocalPlayer.SetKillTimer(killCool);
		}
    }

}

[HarmonyPatch]
public static class ExileControllerWrapUpPatch
{
	private static bool isPrefixRun = false;

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    public static class BaseExileControllerPatch
    {
        public static void Prefix()
        {
            WrapUpPrefix();
        }
        public static void Postfix(ExileController __instance)
        {
            WrapUpPostfix(__instance.initData.networkedPlayer);
        }
    }

    [HarmonyPatch(typeof(
		AirshipExileController._WrapUpAndSpawn_d__11),
		nameof(AirshipExileController._WrapUpAndSpawn_d__11.MoveNext))]
    public static class AirshipExileControllerPatch
    {
        public static void Postfix(
			AirshipExileController._WrapUpAndSpawn_d__11 __instance,
			ref bool __result)
        {
			WrapUpPrefix();

			if (__result)
			{
				return;
			}

            WrapUpPostfix(__instance.__4__this.initData.networkedPlayer);
        }
    }

    public static void WrapUpPostfix(NetworkedPlayerInfo? exiled)
    {
		InfoOverlay.Instance.IsBlock = false;
        Meeting.Hud.MeetingHudSelectPatch.SetSelectBlock(false);

        if (ExtremeRoleManager.GameRole.Count == 0) { return; }

        var state = ExtremeRolesPlugin.ShipState;

        if (OnemanMeetingSystemManager.TryGetSystem(out var system))
        {
			_ = system.TryStartMeeting();
		}


        var role = ExtremeRoleManager.GetLocalPlayerRole();
        if (role is IRoleAbility abilityRole)
        {
            abilityRole.Button.OnMeetingEnd();
        }
        if (role is IRoleResetMeeting resetRole)
        {
            resetRole.ResetOnMeetingEnd(exiled);
        }
        if (role is MultiAssignRoleBase multiAssignRole)
        {
            if (multiAssignRole.AnotherRole is IRoleAbility abilityMultiAssignRole)
            {
                abilityMultiAssignRole.Button.OnMeetingEnd();
            }
            if (multiAssignRole.AnotherRole is IRoleResetMeeting resetMultiAssignRole)
            {
                resetMultiAssignRole.ResetOnMeetingEnd(exiled);
            }
        }

        var ghostRole = ExtremeGhostRoleManager.GetLocalPlayerGhostRole();
        if (ghostRole != null)
        {
            ghostRole.ResetOnMeetingEnd();
        }
		isPrefixRun = false;
	}

    public static void WrapUpPrefix()
    {
		if (isPrefixRun)
		{
			return;
		}
		ExtremeSystemTypeManager.Instance.Reset(null, (byte)ResetTiming.ExiledEnd);
		isPrefixRun = true;
	}
}
