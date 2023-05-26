using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.CoreScripts;

using HarmonyLib;
using Hazel;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.GameMode;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Patches;

[HarmonyPatch]
public static class CacheLocalPlayerPatch
{
    [HarmonyTargetMethod]
    public static MethodBase TargetMethod()
    {
        var type = typeof(PlayerControl).GetNestedTypes(AccessTools.all).FirstOrDefault(t => t.Name.Contains("Start"));
        return AccessTools.Method(type, nameof(Il2CppSystem.Collections.IEnumerator.MoveNext));
    }

    [HarmonyPostfix]
    public static void SetLocalPlayer()
    {
        PlayerControl localPlayer = PlayerControl.LocalPlayer;
        if (!localPlayer)
        {
            CachedPlayerControl.LocalPlayer = null;
            return;
        }

        CachedPlayerControl cached = CachedPlayerControl.AllPlayerControls.FirstOrDefault(
            p => p.PlayerControl.Pointer == localPlayer.Pointer);
        if (cached != null)
        {
            CachedPlayerControl.LocalPlayer = cached;
            return;
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Awake))]
public static class PlayerControlAwakePatch
{
    public static void Postfix(PlayerControl __instance)
    {
        if (__instance.notRealPlayer) { return; }

        new CachedPlayerControl(__instance);

#if DEBUG
        foreach (var cachedPlayer in CachedPlayerControl.AllPlayerControls)
        {
            if (!cachedPlayer.PlayerControl || 
                !cachedPlayer.PlayerPhysics || 
                !cachedPlayer.NetTransform || 
                !cachedPlayer.transform)
            {
                Logging.Debug($"CachedPlayer {cachedPlayer.PlayerControl.name} has null fields");
            }
        }
#endif
    }
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
public static class PlayerControlExiledPatch
{
    public static void Postfix(
        PlayerControl __instance)
    {
        if (ExtremeRoleManager.GameRole.Count == 0) { return; }

        var role = ExtremeRoleManager.GetLocalPlayerRole();
        var exiledPlayerRole = ExtremeRoleManager.GameRole[__instance.PlayerId];

        if (ExtremeRoleManager.IsDisableWinCheckRole(exiledPlayerRole))
        {
            ExtremeRolesPlugin.ShipState.SetDisableWinCheck(true);
        }

        ExtremeRolesPlugin.ShipState.AddDeadInfo(
            __instance, DeathReason.Exile, null);

        if (role is IRoleExilHook hookRole)
        {
            hookRole.HookExil(__instance);
        }
        if (role is MultiAssignRoleBase multiAssignRole)
        {
            if (multiAssignRole.AnotherRole is IRoleExilHook multiHookRole)
            {
                multiHookRole.HookExil(__instance);
            }
        }

        exiledPlayerRole.ExiledAction(__instance);
        if (exiledPlayerRole is MultiAssignRoleBase multiAssignExiledPlayerRole)
        {
            multiAssignExiledPlayerRole.AnotherRole?.ExiledAction(__instance);
        }

        if (!exiledPlayerRole.HasTask())
        {
            __instance.ClearTasks();
        }

        ExtremeRolesPlugin.ShipState.SetDisableWinCheck(false);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Deserialize))]
public static class PlayerControlDeserializePatch
{
    public static void Postfix(PlayerControl __instance)
    {
        CachedPlayerControl.PlayerPtrs[__instance.Pointer].PlayerId = __instance.PlayerId;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.OnDestroy))]
public static class PlayerControlOnDestroyPatch
{
    public static void Postfix(PlayerControl __instance)
    {
        if (__instance.notRealPlayer) { return; }
        CachedPlayerControl.Remove(__instance);
    }
}


[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.StartMeeting))]
public static class PlayerControlCoStartMeetingPatch
{
    public static void Prefix([HarmonyArgument(0)] GameData.PlayerInfo target)
    {
        ExtremeRolesPlugin.Info.BlockShow(true);

        var state = ExtremeRolesPlugin.ShipState;

        if (state.AssassinMeetingTrigger) { return; }

        // Count meetings
        if (target == null)
        {
            state.IncreaseMeetingCount();
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
public static class PlayerControlFixedUpdatePatch
{

    public static void Postfix(PlayerControl __instance)
    {
        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) 
        { return; }
        if (!RoleAssignState.Instance.IsRoleSetUpEnd) { return; }
        if (ExtremeRoleManager.GameRole.Count == 0) { return; }
        if (CachedPlayerControl.LocalPlayer.PlayerId != __instance.PlayerId) { return; }

        SingleRoleBase role = ExtremeRoleManager.GetLocalPlayerRole();
        GhostRoleBase ghostRole = ExtremeGhostRoleManager.GetLocalPlayerGhostRole();

        buttonUpdate(__instance, role, ghostRole);
        refreshRoleDescription(__instance, role, ghostRole);
    }

    private static void refreshRoleDescription(
        PlayerControl player,
        SingleRoleBase playerRole,
        GhostRoleBase playerGhostRole)
    {

        var removedTask = new List<PlayerTask>();
        foreach (PlayerTask task in player.myTasks.GetFastEnumerator())
        {
            if (task == null) { return; }

            var textTask = task.gameObject.GetComponent<ImportantTextTask>();
            if (textTask != null)
            {
                removedTask.Add(task); // TextTask does not have a corresponding RoleInfo and will hence be deleted
            }
        }

        foreach (PlayerTask task in removedTask)
        {
            task.OnRemove();
            player.myTasks.Remove(task);
            UnityEngine.Object.Destroy(task.gameObject);
        }

        var importantTextTask = new GameObject("RoleTask").AddComponent<ImportantTextTask>();
        importantTextTask.transform.SetParent(player.transform, false);

        string addText = playerRole.GetImportantText();
        if (playerGhostRole != null)
        {
            addText =$"{addText}\n{playerGhostRole.GetImportantText()}";
        }
        importantTextTask.Text = addText;
        player.myTasks.Insert(0, importantTextTask);

    }
    private static void buttonUpdate(
        PlayerControl player,
        SingleRoleBase playerRole,
        GhostRoleBase playerGhostRole)
    {
        if (!player.AmOwner) { return; }

        bool enable = 
            (player.IsKillTimerEnabled || player.ForceKillTimerContinue) &&
            (MeetingHud.Instance == null && ExileController.Instance == null);

        killButtonUpdate(player, playerRole, enable);
        ventButtonUpdate(player, playerRole, enable);

        roleAbilityButtonUpdate(playerRole);
        ghostRoleButtonUpdate(playerGhostRole);
    }

    private static void killButtonUpdate(
        PlayerControl player,
        SingleRoleBase role, bool enable)
    {

        bool isImposter = role.IsImpostor();

        HudManager hudManager = FastDestroyableSingleton<HudManager>.Instance;

        if (role.CanKill())
        {
            if (enable && !player.Data.IsDead)
            {
                if (!isImposter)
                {
                    player.SetKillTimer(player.killTimer - Time.fixedDeltaTime);
                }

                PlayerControl target = Player.GetClosestPlayerInKillRange();

                // Logging.Debug($"TargetAlive?:{target}");

                hudManager.KillButton.SetTarget(target);
                Player.SetPlayerOutLine(target, role.GetNameColor());
                hudManager.KillButton.Show();
                hudManager.KillButton.gameObject.SetActive(true);
            }
            else
            {
                hudManager.KillButton.SetDisabled();
            }
        }
        else if (isImposter)
        {
            hudManager.KillButton.SetDisabled();
        }
    }

    private static void roleAbilityButtonUpdate(
        SingleRoleBase role)
    {
        abilityButtonUpdate(role as IRoleAbility);

        var multiAssignRole = role as MultiAssignRoleBase;
        if (multiAssignRole != null)
        {
            abilityButtonUpdate(
                multiAssignRole.AnotherRole as IRoleAbility);
        }
    }

    private static void abilityButtonUpdate(IRoleAbility abilityRole)
    {
        if (abilityRole != null &&
            abilityRole.Button != null)
        {
            abilityRole.Button.Update();
        }
    }

    private static void ventButtonUpdate(
        PlayerControl player, SingleRoleBase role, bool enable)
    {

        HudManager hudManager = FastDestroyableSingleton<HudManager>.Instance;

        if (!role.CanUseVent() || player.Data.IsDead)
        { 
            hudManager.ImpostorVentButton.Hide();
            return;
        }

        bool ventButtonShow = enable || player.inVent;

        if (!role.TryGetVanillaRoleId(out RoleTypes roleId) ||
            roleId is RoleTypes.Shapeshifter or RoleTypes.Impostor)
        {
            if (ventButtonShow &&
                ExtremeGameModeManager.Instance.ShipOption.IsEnableImpostorVent)
            {
                hudManager.ImpostorVentButton.Show();
            }
            else
            {
                hudManager.ImpostorVentButton.SetDisabled();
            }
        }
        else if (
            roleId == RoleTypes.Engineer &&
            player.Data.Role.Role == RoleTypes.Engineer)
        {
            if (ventButtonShow)
            {
                if (!ExtremeGameModeManager.Instance.ShipOption.EngineerUseImpostorVent)
                {
                    hudManager.AbilityButton.Show();
                }
                else
                {
                    hudManager.ImpostorVentButton.Show();
                    hudManager.AbilityButton.gameObject.SetActive(false);
                }
            }
            else
            {
                hudManager.ImpostorVentButton.SetDisabled();
                hudManager.AbilityButton.SetDisabled();
            }
        }
    }

    private static void ghostRoleButtonUpdate(GhostRoleBase playerGhostRole)
    {
        if (playerGhostRole == null) { return; }

        var abilityButton = FastDestroyableSingleton<HudManager>.Instance.AbilityButton;

        switch (CachedPlayerControl.LocalPlayer.Data.Role.Role)
        {
            case RoleTypes.Engineer:
            case RoleTypes.Scientist:
            case RoleTypes.Shapeshifter:
                abilityButton.Hide();
                break;
            case RoleTypes.CrewmateGhost:
            case RoleTypes.ImpostorGhost:
                if (playerGhostRole.IsVanillaRole() &&
                    MeetingHud.Instance == null && 
                    ExileController.Instance == null)
                {
                    abilityButton.Show();
                }
                else
                {
                    abilityButton.Hide();
                }
                break;
            default:
                break;
        }
        playerGhostRole.Button?.Update();
    }
}


[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
public static class PlayerControlHandleRpcPatch
{
    public static void Postfix(
        PlayerControl __instance,
        [HarmonyArgument(0)] byte callId,
        [HarmonyArgument(1)] MessageReader reader)
    {

        if (__instance == null || reader == null) { return; }

        switch ((RPCOperator.Command)callId)
        {
            case RPCOperator.Command.Initialize:
                RPCOperator.Initialize();
                break;
            case RPCOperator.Command.ForceEnd:
                RPCOperator.ForceEnd();
                break;
            case RPCOperator.Command.SetUpReady:
                byte readyPlayerId = reader.ReadByte();
                RPCOperator.SetUpReady(readyPlayerId);
                break;
            case RPCOperator.Command.SetRoleToAllPlayer:
                List<Module.Interface.IPlayerToExRoleAssignData> assignData = 
                    new List<Module.Interface.IPlayerToExRoleAssignData>();
                int assignDataNum = reader.ReadPackedInt32();
                for (int i = 0; i < assignDataNum; ++i)
                {
                    byte assignedPlayerId = reader.ReadByte();
                    byte assignRoleType = reader.ReadByte();
                    int exRoleId = reader.ReadPackedInt32();
                    int controlId = reader.ReadPackedInt32();
                    switch (assignRoleType)
                    {
                        case (byte)Module.IAssignedPlayer.ExRoleType.Single:
                            assignData.Add(new
                                PlayerToSingleRoleAssignData(
                                    assignedPlayerId, exRoleId, controlId));
                            break;
                        case (byte)Module.IAssignedPlayer.ExRoleType.Comb:
                            byte assignCombType = reader.ReadByte(); // combTypeId
                            byte bytedAmongUsVanillaRoleId = reader.ReadByte(); // byted AmongUsVanillaRoleId
                            assignData.Add(new
                                PlayerToCombRoleAssignData(
                                    assignedPlayerId, exRoleId, assignCombType,
                                    controlId, bytedAmongUsVanillaRoleId));
                            break;
                    }
                }
                RPCOperator.SetRoleToAllPlayer(assignData);
                RoleAssignState.Instance.SwitchRoleAssignToEnd();
                if (PlayerRoleAssignData.IsExist)
                {
                    PlayerRoleAssignData.Instance.Destroy();
                }
                break;
            case RPCOperator.Command.ShareOption:
                int numOptions = (int)reader.ReadByte();
                RPCOperator.ShareOption(numOptions, reader);
                break;
            case RPCOperator.Command.CustomVentUse:
                int ventId = reader.ReadPackedInt32();
                byte ventingPlayer = reader.ReadByte();
                byte isEnter = reader.ReadByte();
                RPCOperator.CustomVentUse(ventId, ventingPlayer, isEnter);
                break;
            case RPCOperator.Command.StartVentAnimation:
                int animationVentId = reader.ReadPackedInt32();
                RPCOperator.StartVentAnimation(animationVentId);
                break;
            case RPCOperator.Command.UncheckedSnapTo:
                byte snapPlayerId = reader.ReadByte();
                float snapX = reader.ReadSingle();
                float snapY = reader.ReadSingle();
                RPCOperator.UncheckedSnapTo(
                    snapPlayerId, new Vector2(snapX, snapY));
                break;
            case RPCOperator.Command.UncheckedShapeShift:
                byte shapeShiftPlayerId = reader.ReadByte();
                byte shapeShiftTargetPlayerId = reader.ReadByte();
                byte shapeShiftAnimationTrigger = reader.ReadByte();
                RPCOperator.UncheckedShapeShift(
                    shapeShiftPlayerId,
                    shapeShiftTargetPlayerId,
                    shapeShiftAnimationTrigger);
                break;
            case RPCOperator.Command.UncheckedMurderPlayer:
                byte sourceId = reader.ReadByte();
                byte targetId = reader.ReadByte();
                byte killAnimationTrigger = reader.ReadByte();
                RPCOperator.UncheckedMurderPlayer(
                    sourceId, targetId, killAnimationTrigger);
                break;
            case RPCOperator.Command.UncheckedRevive:
                byte reviveTargetId = reader.ReadByte();
                RPCOperator.UncheckedRevive(reviveTargetId);
                break;
            case RPCOperator.Command.CleanDeadBody:
                byte deadBodyPlayerId = reader.ReadByte();
                RPCOperator.CleanDeadBody(deadBodyPlayerId);
                break;
            case RPCOperator.Command.FixLightOff:
                RPCOperator.FixLightOff();
                break;
            case RPCOperator.Command.ReplaceDeadReason:
                byte changePlayerId = reader.ReadByte();
                byte reason = reader.ReadByte();
                RPCOperator.ReplaceDeadReason(
                    changePlayerId, reason);
                break;
            case RPCOperator.Command.SetRoleWin:
                byte rolePlayerId = reader.ReadByte();
                RPCOperator.SetRoleWin(rolePlayerId);
                break;
            case RPCOperator.Command.SetWinGameControlId:
                int id = reader.ReadInt32();
                RPCOperator.SetWinGameControlId(id);
                break;
            case RPCOperator.Command.SetWinPlayer:
                int playerNum = reader.ReadInt32();
                List<byte> winPlayerId = new List<byte>();
                for (int i= 0; i < playerNum; ++i)
                {
                    winPlayerId.Add(reader.ReadByte());
                }
                RPCOperator.SetWinPlayer(winPlayerId);
                break;
            case RPCOperator.Command.ShareMapId:
                byte mapId = reader.ReadByte();
                RPCOperator.ShareMapId(mapId);
                break;
            case RPCOperator.Command.ShareVersion:
                int major = reader.ReadInt32();
                int minor = reader.ReadInt32();
                int build = reader.ReadInt32();
                int revision = reader.ReadInt32();
                int clientId = reader.ReadPackedInt32();
                RPCOperator.AddVersionData(
                    major, minor, build,
                    revision, clientId);
                break;
            case RPCOperator.Command.PlaySound:
                byte soundType = reader.ReadByte();
                float volume = reader.ReadSingle();
                RPCOperator.PlaySound(soundType, volume);
                break;
            case RPCOperator.Command.ReplaceTask:
                byte replaceTargetPlayerId = reader.ReadByte();
                int taskIndex = reader.ReadInt32();
                int taskId = reader.ReadInt32();
                RPCOperator.ReplaceTask(
                    replaceTargetPlayerId, taskIndex, taskId);
                break;
            case RPCOperator.Command.IntegrateModCall:
                RPCOperator.IntegrateModCall(ref reader);
                break;
            case RPCOperator.Command.CloseMeetingVoteButton:
                RPCOperator.CloseMeetingButton();
                break;
            case RPCOperator.Command.MeetingReporterRpc:
                RPCOperator.MeetingReporterRpcOp(ref reader);
                break;
            case RPCOperator.Command.ReplaceRole:
                byte targetPlayerId = reader.ReadByte();
                byte replaceTarget = reader.ReadByte();
                byte ops = reader.ReadByte();
                RPCOperator.ReplaceRole(
                    targetPlayerId, replaceTarget, ops);
                break;
            case RPCOperator.Command.HeroHeroAcademia:
                RPCOperator.HeroHeroAcademiaCommand(ref reader);
                break;
            case RPCOperator.Command.KidsAbility:
                RPCOperator.KidsAbilityCommand(ref reader);
                break;
            case RPCOperator.Command.MoverAbility:
                RPCOperator.MoverAbility(ref reader);
                break;
            case RPCOperator.Command.BodyGuardAbility:
                RPCOperator.BodyGuardAbility(ref reader);
                break;
            case RPCOperator.Command.TimeMasterAbility:
                RPCOperator.TimeMasterAbility(ref reader);
                break;
            case RPCOperator.Command.AgencyTakeTask:
                byte agencyTargetPlayerId = reader.ReadByte();
                int getTaskNum = reader.ReadInt32();

                List<int> getTaskId = new List<int> ();
                
                for (int i = 0; i < getTaskNum; ++i)
                {
                    getTaskId.Add(reader.ReadInt32());
                }

                RPCOperator.AgencyTakeTask(
                    agencyTargetPlayerId, getTaskId);
                break;
            case RPCOperator.Command.FencerAbility:
                RPCOperator.FencerAbility(ref reader);
                break;
            case RPCOperator.Command.CuresMakerCurseKillCool:
                byte curesMakerPlayerId = reader.ReadByte();
                byte curesPlayerId = reader.ReadByte();
                RPCOperator.CuresMakerCurseKillCool(
                    curesMakerPlayerId, curesPlayerId);
                break;
            case RPCOperator.Command.CarpenterUseAbility:
                RPCOperator.CarpenterUseAbility(ref reader);
                break;
            case RPCOperator.Command.SurvivorDeadWin:
                byte survivorPlayerId = reader.ReadByte();
                RPCOperator.SurvivorDeadWin(survivorPlayerId);
                break;
            case RPCOperator.Command.CaptainAbility:
                RPCOperator.CaptainTargetVote(ref reader);
                break;
            case RPCOperator.Command.ResurrecterRpc:
                RPCOperator.ResurrecterRpc(ref reader);
                break;
            case RPCOperator.Command.TeleporterSetPortal:
                byte teleporterPlayerId = reader.ReadByte();
                float portalX = reader.ReadSingle();
                float portalY = reader.ReadSingle();
                RPCOperator.TeleporterSetPortal(
                    teleporterPlayerId, portalX, portalY);
                break;
            case RPCOperator.Command.AssasinVoteFor:
                byte voteTargetId = reader.ReadByte();
                RPCOperator.AssasinVoteFor(voteTargetId);
                break;
            case RPCOperator.Command.CarrierAbility:
                byte carrierCarryOpCallPlayerId = reader.ReadByte();
                float carrierPlayerPosX = reader.ReadSingle();
                float carrierPlayerPosY = reader.ReadSingle();
                byte carryDeadBodyPlayerId = reader.ReadByte();
                bool isCarryDeadBody = reader.ReadBoolean();
                RPCOperator.CarrierAbility(
                    carrierCarryOpCallPlayerId,
                    carrierPlayerPosX,
                    carrierPlayerPosY,
                    carryDeadBodyPlayerId,
                    isCarryDeadBody);
                break;
            case RPCOperator.Command.PainterPaintBody:
                byte painterPlayerId = reader.ReadByte();
                byte isRandomModeMessage = reader.ReadByte();
                RPCOperator.PainterPaintBody(
                    painterPlayerId,
                    isRandomModeMessage);
                break;
            case RPCOperator.Command.FakerCreateDummy:
                byte fakerPlayerId = reader.ReadByte();
                byte dummyTargetId = reader.ReadByte();
                byte fakerOps = reader.ReadByte();
                RPCOperator.FakerCreateDummy(
                    fakerPlayerId, dummyTargetId, fakerOps);
                break;
            case RPCOperator.Command.OverLoaderSwitchAbility:
                byte overLoaderPlayerId = reader.ReadByte();
                byte activate  = reader.ReadByte();
                RPCOperator.OverLoaderSwitchAbility(
                    overLoaderPlayerId, activate);
                break;
            case RPCOperator.Command.CrackerCrackDeadBody:
                byte crackerId = reader.ReadByte();
                byte crackTarget = reader.ReadByte();
                RPCOperator.CrackerCrackDeadBody(crackerId, crackTarget);
                break;
            case RPCOperator.Command.MeryAbility:
                RPCOperator.MaryAbility(ref reader);
                break;
            case RPCOperator.Command.LastWolfSwitchLight:
                byte swichStatus = reader.ReadByte();
                RPCOperator.LastWolfSwitchLight(swichStatus);
                break;
            case RPCOperator.Command.CommanderAttackCommand:
                byte commanderPlayerId = reader.ReadByte();
                RPCOperator.CommanderAttackCommand(commanderPlayerId);
                break;
            case RPCOperator.Command.HypnotistAbility:
                RPCOperator.HypnotistAbility(ref reader);
                break;
            case RPCOperator.Command.UnderWarperUseVentWithNoAnime:
                byte underWarperPlayerId = reader.ReadByte();
                int targetVentId = reader.ReadPackedInt32();
                bool isVentEnter = reader.ReadBoolean();
                RPCOperator.UnderWarperUseVentWithNoAnime(
                    underWarperPlayerId, targetVentId, isVentEnter);
                break;
            case RPCOperator.Command.SlimeAbility:
                RPCOperator.SlimeAbility(ref reader);
                break;
            case RPCOperator.Command.ZombieRpc:
                RPCOperator.ZombieRpc(ref reader);
                break;
            case RPCOperator.Command.AliceShipBroken:
                byte alicePlayerId = reader.ReadByte();
                byte newTaskSetPlayerId = reader.ReadByte();
                int newTaskNum = reader.ReadInt32();

                List<int> task = new List<int>();

                for (int i = 0; i < newTaskNum; ++i)
                {
                    task.Add(reader.ReadInt32());
                }
                RPCOperator.AliceShipBroken(
                    alicePlayerId, newTaskSetPlayerId, task);
                break;
            case RPCOperator.Command.JesterOutburstKill:
                byte outburstKillerId = reader.ReadByte();
                byte killTargetId = reader.ReadByte();
                RPCOperator.JesterOutburstKill(
                    outburstKillerId, killTargetId);
                break;
            case RPCOperator.Command.YandereSetOneSidedLover:
                byte yanderePlayerId = reader.ReadByte();
                byte loverPlayerId = reader.ReadByte();
                RPCOperator.YandereSetOneSidedLover(
                    yanderePlayerId, loverPlayerId);
                break;
            case RPCOperator.Command.TotocalcioSetBetPlayer:
                byte totocalcioPlayerId = reader.ReadByte();
                byte betPlayerId = reader.ReadByte();
                RPCOperator.TotocalcioSetBetPlayer(
                    totocalcioPlayerId, betPlayerId);
                break;
            case RPCOperator.Command.MadmateToFakeImpostor:
                byte madmatePlayerId = reader.ReadByte();
                RPCOperator.MadmateToFakeImpostor(
                    madmatePlayerId);
                break;
            case RPCOperator.Command.SetGhostRole:
                RPCOperator.SetGhostRole(
                    ref reader);
                break;
            case RPCOperator.Command.UseGhostRoleAbility:
                byte useGhostRoleType = reader.ReadByte();
                bool isReport = reader.ReadBoolean();
                RPCOperator.UseGhostRoleAbility(
                    useGhostRoleType, isReport, ref reader);
                break;
            case RPCOperator.Command.XionAbility:
                RPCOperator.XionAbility(ref reader);
                break;
            default:
                break;
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
public static class PlayerControlMurderPlayerPatch
{
    public static bool Prefix(
        PlayerControl __instance,
        [HarmonyArgument(0)] PlayerControl target)
    {
        if (ExtremeRoleManager.GameRole.Count == 0) { return true; }

        var role = ExtremeRoleManager.GameRole[__instance.PlayerId];

        bool hasOtherKillCool = role.TryGetKillCool(out float killCool);

        if (role.Id == ExtremeRoleId.Villain)
        {
            guardBreakKill(__instance, target, killCool);
            return false; 
        }
        if (!hasOtherKillCool) { return true; }

        __instance.logger.Debug(
            $"{__instance.PlayerId} trying to murder {target.PlayerId}", null);

        if (target.protectedByGuardian)
        {
            target.protectedByGuardianThisRound = true;
            bool flag = CachedPlayerControl.LocalPlayer.Data.Role.Role == RoleTypes.GuardianAngel;
            if (__instance.AmOwner || flag)
            {
                target.ShowFailedMurder();
                __instance.SetKillTimer(killCool / 2f);
            }
            else
            {
                target.RemoveProtection();
            }
            if (flag)
            {
                StatsManager.Instance.IncrementStat(
                    StringNames.StatsGuardianAngelCrewmatesProtected);
            }
            __instance.logger.Debug(
                $"{__instance.PlayerId} failed to murder {target.PlayerId} due to guardian angel protection",
                null);
            return false;
        }

        murderPlayerBody(__instance, target, killCool);
        return false;
    }

    public static void Postfix(
        PlayerControl __instance,
        [HarmonyArgument(0)] PlayerControl target)
    {

        if (ExtremeRoleManager.GameRole.Count == 0) { return; }

        if (!target.Data.IsDead) { return; }

        ExtremeRolesPlugin.ShipState.AddDeadInfo(
            target, DeathReason.Kill, __instance);

        byte targetPlayerId = target.PlayerId;

        var role = ExtremeRoleManager.GameRole[targetPlayerId];

        if (!role.HasTask())
        {
            target.ClearTasks();
        }

        if (ExtremeRoleManager.IsDisableWinCheckRole(role))
        {
            ExtremeRolesPlugin.ShipState.SetDisableWinCheck(true);
        }

        var multiAssignRole = role as MultiAssignRoleBase;

        role.RolePlayerKilledAction(
            target, __instance);
        if (multiAssignRole != null)
        {
            if (multiAssignRole.AnotherRole != null)
            {
                multiAssignRole.AnotherRole.RolePlayerKilledAction(
                    target, __instance);
            }
        }

        ExtremeRolesPlugin.ShipState.SetDisableWinCheck(false);

        var player = CachedPlayerControl.LocalPlayer;

        if (player.PlayerId != targetPlayerId)
        {
            var hookRole = ExtremeRoleManager.GameRole[
                player.PlayerId] as IRoleMurderPlayerHook;
            multiAssignRole = ExtremeRoleManager.GameRole[
                player.PlayerId] as MultiAssignRoleBase;

            if (hookRole != null)
            {
                hookRole.HookMuderPlayer(
                    __instance, target);
            }
            if (multiAssignRole != null)
            {
                hookRole = multiAssignRole.AnotherRole as IRoleMurderPlayerHook;
                if (hookRole != null)
                {
                    hookRole.HookMuderPlayer(
                        __instance, target);
                }
            }
        }
    }
    private static void guardBreakKill(
        PlayerControl instance,
        PlayerControl target,
        float killCool)
    {
        if (target.protectedByGuardian)
        {
            target.RemoveProtection();
        }
        murderPlayerBody(instance, target, killCool);
    }

    private static void murderPlayerBody(
        PlayerControl instance,
        PlayerControl target,
        float killCool)
    {
        if (instance.AmOwner)
        {
            if (GameManager.Instance.IsHideAndSeek())
            {
                StatsManager.Instance.IncrementStat(
                    StringNames.StatsImpostorKills_HideAndSeek);
            }
            else
            {
                StatsManager.Instance.IncrementStat(StringNames.StatsImpostorKills);
            }
            if (instance.CurrentOutfitType == PlayerOutfitType.Shapeshifted)
            {
                StatsManager.Instance.IncrementStat(StringNames.StatsShapeshifterShiftedKills);
            }
            if (Constants.ShouldPlaySfx())
            {
                SoundManager.Instance.PlaySound(
                    instance.KillSfx, false, 0.8f, null);
            }
            instance.SetKillTimer(killCool);
        }
        
        FastDestroyableSingleton<Telemetry>.Instance.WriteMurder();

        target.gameObject.layer = LayerMask.NameToLayer("Ghost");
        if (target.AmOwner)
        {
            StatsManager.Instance.IncrementStat(StringNames.StatsTimesMurdered);
            if (Minigame.Instance)
            {
                try
                {
                    Minigame.Instance.Close();
                    Minigame.Instance.Close();
                }
                catch
                {
                }
            }
            FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(
                instance.Data, target.Data);
            FastDestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.SetActive(false);
            target.cosmetics.SetNameMask(false);
            target.RpcSetScanner(false);
        }
        FastDestroyableSingleton<AchievementManager>.Instance.OnMurder(
            instance.AmOwner, target.AmOwner,
            instance.CurrentOutfitType == PlayerOutfitType.Shapeshifted,
            instance.shapeshiftTargetPlayerId, (int)target.PlayerId);

        var killAnimation = instance.KillAnimations;
        var useKillAnimation = default(KillAnimation);

        if (killAnimation.Count > 0)
        {
            useKillAnimation = killAnimation[UnityEngine.Random.Range(
                0, killAnimation.Count)];
        }

        instance.MyPhysics.StartCoroutine(useKillAnimation.CoPerformKill(instance, target));

        instance.logger.Debug(
            string.Format("{0} succeeded in murdering {1}", instance.PlayerId, target.PlayerId), null);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
public static class PlayerControlSetCoolDownPatch
{
    public static bool Prefix(
        PlayerControl __instance, [HarmonyArgument(0)] float time)
    {
        var roles = ExtremeRoleManager.GameRole;
        if (roles.Count == 0 || !roles.ContainsKey(__instance.PlayerId)) { return true; }

        var role = roles[__instance.PlayerId];

        var killCool = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(
            FloatOptionNames.KillCooldown);
        if (killCool <= 0f) { return false; }
        float maxTime = killCool;

        if (!role.CanKill()) { return false; }

        if (role.TryGetKillCool(out float otherKillCool))
        {
            maxTime = otherKillCool;
        }

        __instance.killTimer = Mathf.Clamp(
            time, 0f, maxTime);
        FastDestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(
            __instance.killTimer, maxTime);

        return false;

    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
public static class PlayerControlShapeshiftPatch
{
    public static bool Prefix(
        PlayerControl __instance,
        [HarmonyArgument(0)] PlayerControl targetPlayer,
        [HarmonyArgument(1)] bool animate)
    {
        var roles = ExtremeRoleManager.GameRole;
        if (roles.Count == 0 || !roles.ContainsKey(__instance.PlayerId)) { return true; }

        var role = roles[__instance.PlayerId];
        if (role.TryGetVanillaRoleId(out RoleTypes roleId) &&
            roleId == RoleTypes.Shapeshifter) { return true; }


        GameData.PlayerInfo targetPlayerInfo = targetPlayer.Data;
        GameData.PlayerOutfit newOutfit;
        if (targetPlayerInfo.PlayerId == __instance.Data.PlayerId)
        {
            newOutfit = __instance.Data.Outfits[PlayerOutfitType.Default];
        }
        else
        {
            newOutfit = targetPlayer.Data.Outfits[PlayerOutfitType.Default];
        }
        Action changeOutfit = delegate ()
        {
            if (targetPlayerInfo.PlayerId == __instance.Data.PlayerId)
            {
                __instance.RawSetOutfit(newOutfit, PlayerOutfitType.Default);
                __instance.logger.Info(
                    string.Format("Player {0} Shapeshift is reverting",
                        __instance.PlayerId), null);
                __instance.shapeshiftTargetPlayerId = -1;
            }
            else
            {
                __instance.RawSetOutfit(newOutfit, PlayerOutfitType.Shapeshifted);
                __instance.logger.Info(
                    string.Format("Player {0} is shapeshifting into {1}",
                        __instance.PlayerId, targetPlayer.PlayerId), null);
                __instance.shapeshiftTargetPlayerId = (int)targetPlayer.PlayerId;
            }
        };
        if (animate)
        {
            __instance.shapeshifting = true;
            if (__instance.AmOwner)
            {
                PlayerControl.HideCursorTemporarily();
            }
            RoleEffectAnimation roleEffectAnimation = UnityEngine.Object.Instantiate<RoleEffectAnimation>(
                FastDestroyableSingleton<RoleManager>.Instance.shapeshiftAnim, __instance.gameObject.transform);
            roleEffectAnimation.SetMaterialColor(
                __instance.Data.Outfits[PlayerOutfitType.Default].ColorId);
            if (__instance.cosmetics.FlipX)
            {
                roleEffectAnimation.transform.position -= new Vector3(0.14f, 0f, 0f);
            }

            Action changeAction = () =>
            {
                changeOutfit.Invoke();
                __instance.cosmetics.SetScale(
                    __instance.MyPhysics.Animations.DefaultPlayerScale,
                    __instance.defaultCosmeticsScale);
            };

            roleEffectAnimation.MidAnimCB = changeAction;

            __instance.StartCoroutine(__instance.ScalePlayer(
                __instance.MyPhysics.Animations.ShapeshiftScale, 0.25f));

            Action roleAnimation = () =>
            {
                __instance.shapeshifting = false;
            };

            roleEffectAnimation.Play(
                __instance, roleAnimation,
                CachedPlayerControl.LocalPlayer.PlayerControl.cosmetics.FlipX,
                RoleEffectAnimation.SoundType.Local, 0f);
            return false;
        }
        changeOutfit.Invoke();
        return false;

    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
public static class PlayerControlRpcSyncSettingsPatch
{
    public static void Postfix()
    {
        Module.CustomOption.OptionManager.Instance.ShareOptionSelections();
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RemoveTask))]
public static class PlayerControlRemoveTaskPatch
{
    public static void Prefix()
    {
        Manager.HudManagerUpdatePatch.SetBlockUpdate(true);
        FastDestroyableSingleton<HudManager>.Instance.taskDirtyTimer = 0.0f;
    }
    public static void Postfix()
    {
        Manager.HudManagerUpdatePatch.SetBlockUpdate(false);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Revive))]
public static class PlayerControlRevivePatch
{
    public static void Postfix(PlayerControl __instance)
    {

        ExtremeRolesPlugin.ShipState.RemoveDeadInfo(__instance.PlayerId);

        if (ExtremeRoleManager.GameRole.Count == 0) { return; }
        if (!RoleAssignState.Instance.IsRoleSetUpEnd) { return; }

        var (onRevive, onReviveOther) = ExtremeRoleManager.GetInterfaceCastedRole<
            IRoleOnRevive>(__instance.PlayerId);

        onRevive?.ReviveAction(__instance);
        onReviveOther?.ReviveAction(__instance);

        SingleRoleBase role = ExtremeRoleManager.GameRole[__instance.PlayerId];

        if (!role.TryGetVanillaRoleId(out RoleTypes roleId) &&
            role.IsImpostor())
        {
            roleId = RoleTypes.Impostor;
        }

        FastDestroyableSingleton<RoleManager>.Instance.SetRole(
            __instance, roleId);

        var ghostRole = ExtremeGhostRoleManager.GetLocalPlayerGhostRole();
        if (ghostRole == null) { return; }

        if (__instance.PlayerId == CachedPlayerControl.LocalPlayer.PlayerId)
        {
            ghostRole.ResetOnMeetingStart();
        }

        lock (ExtremeGhostRoleManager.GameRole)
        {
            ExtremeGhostRoleManager.GameRole.Remove(__instance.PlayerId);
        }
    }
}
