using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.CoreScripts;

using HarmonyLib;
using Hazel;

using UnityEngine;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Helper;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Patches
{
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

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
    public static class PlayerControlExiledPatch
    {
        public static void Postfix(PlayerControl __instance)
        {

            if (ExtremeRoleManager.GameRole.Count == 0) { return; }

            ExtremeRolesPlugin.ShipState.AddDeadInfo(
                __instance, DeathReason.Exile, null);

            var role = ExtremeRoleManager.GameRole[__instance.PlayerId];

            if (!role.HasTask())
            {
                __instance.ClearTasks();
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
            if (!ExtremeRolesPlugin.ShipState.IsRoleSetUpEnd) { return; }
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
            ventButtonUpdate(playerRole, enable);

            sabotageButtonUpdate(player, playerRole, enable);
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

        private static void sabotageButtonUpdate(
            PlayerControl player,
            SingleRoleBase role, bool enable)
        {
            HudManager hudManager = FastDestroyableSingleton<HudManager>.Instance;

            if (role.CanUseSabotage())
            {
                // インポスターとヴィジランテ、シオンは死んでもサボタージ使える
                if (enable && 
                    (role.IsImpostor() || (
                        role.Id == ExtremeRoleId.Vigilante ||
                        role.Id == ExtremeRoleId.Xion)))
                {
                    hudManager.SabotageButton.Show();
                    hudManager.SabotageButton.gameObject.SetActive(true);
                }
                // それ以外は死んでないときだけサボタージ使える
                else if(enable && !player.Data.IsDead)
                {
                    hudManager.SabotageButton.Show();
                    hudManager.SabotageButton.gameObject.SetActive(true);
                }
                else
                {
                    hudManager.SabotageButton.SetDisabled();
                }
            }
            else
            {
                hudManager.SabotageButton.SetDisabled();
            }
        }

        private static void ventButtonUpdate(
            SingleRoleBase role, bool enable)
        {

            HudManager hudManager = FastDestroyableSingleton<HudManager>.Instance;

            if (role.CanUseVent())
            {
                if (!role.IsVanillaRole())
                {
                    if (enable) { hudManager.ImpostorVentButton.Show(); }
                    else { hudManager.ImpostorVentButton.SetDisabled(); }
                }
                else
                {
                    if (((Roles.Solo.VanillaRoleWrapper)role).VanilaRoleId == RoleTypes.Engineer)
                    {
                        if (enable)
                        {
                            if (!OptionHolder.AllOption[
                                    (int)OptionHolder.CommonOptionKey.EngineerUseImpostorVent].GetValue())
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
            }
            else
            {
                hudManager.ImpostorVentButton.SetDisabled();
            }
        }

        private static void ghostRoleButtonUpdate(GhostRoleBase playerGhostRole)
        {
            if (playerGhostRole != null && playerGhostRole.Button != null)
            {
                switch(CachedPlayerControl.LocalPlayer.Data.Role.Role)
                {
                    case RoleTypes.Engineer:
                    case RoleTypes.Scientist:
                    case RoleTypes.Shapeshifter:
                        FastDestroyableSingleton<HudManager>.Instance.AbilityButton.Hide();
                        break;
                    default:
                        break;
                }
                playerGhostRole.Button.Update();
            }
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
                    List<Module.IAssignedPlayer> assignData = new List<Module.IAssignedPlayer>();
                    int assignDataNum = reader.ReadPackedInt32();
                    for (int i = 0; i < assignDataNum; ++i)
                    {
                        byte assignedPlayerId = reader.ReadByte();
                        byte assignRoleType = reader.ReadByte();
                        int exRoleId = reader.ReadPackedInt32();
                        switch (assignRoleType)
                        {
                            case (byte)Module.IAssignedPlayer.ExRoleType.Single:
                                assignData.Add(new
                                    Module.AssignedPlayerToSingleRoleData(
                                        assignedPlayerId, exRoleId));
                                break;
                            case (byte)Module.IAssignedPlayer.ExRoleType.Comb:
                                byte assignCombType = reader.ReadByte(); // combTypeId
                                byte bytedGameContId = reader.ReadByte(); // byted GameContId
                                byte bytedAmongUsVanillaRoleId = reader.ReadByte(); // byted AmongUsVanillaRoleId
                                assignData.Add(new
                                    Module.AssignedPlayerToCombRoleData(
                                        assignedPlayerId, exRoleId, assignCombType,
                                        bytedGameContId, bytedAmongUsVanillaRoleId));
                                break;
                        }
                    }
                    RPCOperator.SetRoleToAllPlayer(assignData);
                    ExtremeRolesPlugin.ShipState.SwitchRoleAssignToEnd();
                    break;
                case RPCOperator.Command.CleanDeadBody:
                    byte deadBodyPlayerId = reader.ReadByte();
                    RPCOperator.CleanDeadBody(deadBodyPlayerId);
                    break;
                case RPCOperator.Command.FixLightOff:
                    RPCOperator.FixLightOff();
                    break;
                case RPCOperator.Command.ShareOption:
                    int numOptions = (int)reader.ReadByte();
                    RPCOperator.ShareOption(numOptions, reader);
                    break;
                case RPCOperator.Command.ReplaceRole:
                    byte targetPlayerId = reader.ReadByte();
                    byte replaceTarget = reader.ReadByte();
                    byte ops = reader.ReadByte();
                    RPCOperator.ReplaceRole(
                        targetPlayerId, replaceTarget, ops);
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
                case RPCOperator.Command.ReplaceDeadReason:
                    byte changePlayerId = reader.ReadByte();
                    byte reason = reader.ReadByte();
                    RPCOperator.ReplaceDeadReason(
                        changePlayerId, reason);
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
                case RPCOperator.Command.SetRoleWin:
                    byte rolePlayerId = reader.ReadByte();
                    RPCOperator.SetRoleWin(rolePlayerId);
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
                    RPCOperator.PlaySound(soundType);
                    break;
                case RPCOperator.Command.IntegrateModCall:
                    RPCOperator.IntegrateModCall(ref reader);
                    break;
                case RPCOperator.Command.HeroHeroAcademia:
                    RPCOperator.HeroHeroAcademiaCommand(ref reader);
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
                case RPCOperator.Command.AgencySetNewTask:
                    byte agencyCallerId = reader.ReadByte();
                    int taskSetIndex = reader.ReadInt32();
                    int newTaskId = reader.ReadInt32();
                    RPCOperator.AgencySetNewTask(
                        agencyCallerId, taskSetIndex, newTaskId);
                    break;
                case RPCOperator.Command.FencerCounterOn:
                    byte counterOnTimeFencer = reader.ReadByte();
                    RPCOperator.FencerCounterOn(
                        counterOnTimeFencer);
                    break;
                case RPCOperator.Command.FencerCounterOff:
                    byte counterOffTimeFencer = reader.ReadByte();
                    RPCOperator.FencerCounterOff(
                        counterOffTimeFencer);
                    break;
                case RPCOperator.Command.FencerEnableKillButton:
                    byte fencerPlayerId = reader.ReadByte();
                    RPCOperator.FencerEnableKillButton(
                        fencerPlayerId);
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
                case RPCOperator.Command.MerySetCamp:
                    byte maryPlayerId = reader.ReadByte();
                    float maryCampX = reader.ReadSingle();
                    float maryCampY = reader.ReadSingle();
                    RPCOperator.MarySetCamp(
                        maryPlayerId, maryCampX, maryCampY);
                    break;
                case RPCOperator.Command.MeryAcivateVent:
                    int maryCampIndex = reader.ReadInt32();
                    RPCOperator.MaryActiveVent(maryCampIndex);
                    break;
                case RPCOperator.Command.SlaveDriverSetNewTask:
                    byte slaveDriverId = reader.ReadByte();
                    int replaceTaskIndex = reader.ReadInt32();
                    int setTaskId = reader.ReadInt32();
                    RPCOperator.SlaveDriverSetNewTask(
                        slaveDriverId, replaceTaskIndex, setTaskId);
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
                case RPCOperator.Command.TaskMasterSetNewTask:
                    byte taskMasterId = reader.ReadByte();
                    int index = reader.ReadInt32();
                    int taskId = reader.ReadInt32();
                    RPCOperator.TaskMasterSetNewTask(
                        taskMasterId, index, taskId);
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

            GameData.PlayerInfo data = target.Data;
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

            if (__instance.AmOwner)
            {
                StatsManager.Instance.IncrementStat(StringNames.StatsImpostorKills);
                if (__instance.CurrentOutfitType == PlayerOutfitType.Shapeshifted)
                {
                    StatsManager.Instance.IncrementStat(StringNames.StatsShapeshifterShiftedKills);
                }
                if (Constants.ShouldPlaySfx())
                {
                    SoundManager.Instance.PlaySound(
                        __instance.KillSfx, false, 0.8f);
                }
                __instance.SetKillTimer(killCool);
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
			        { }
		        }
                FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(
                    __instance.Data, data);
                FastDestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.SetActive(false);
		        target.cosmetics.SetNameMask(false);
		        target.RpcSetScanner(false);
		        ImportantTextTask importantTextTask = new GameObject("_Player").AddComponent<ImportantTextTask>();
		        importantTextTask.transform.SetParent(
                    __instance.transform, false);
		        if (!PlayerControl.GameOptions.GhostsDoTasks)
		        {
			        target.ClearTasks();
			        importantTextTask.Text = FastDestroyableSingleton<TranslationController>.Instance.GetString(
                        StringNames.GhostIgnoreTasks, Array.Empty<Il2CppSystem.Object>());
		        }
		        else
		        {
			        importantTextTask.Text = FastDestroyableSingleton<TranslationController>.Instance.GetString(
                        StringNames.GhostDoTasks, Array.Empty<Il2CppSystem.Object>());
		        }
		        target.myTasks.Insert(0, importantTextTask);
	        }
            FastDestroyableSingleton<AchievementManager>.Instance.OnMurder(
                __instance.AmOwner, target.AmOwner);
                
            var killAnimation = __instance.KillAnimations.ToList();

            var useKillAnimation = default(KillAnimation);

            if (killAnimation.Count > 0)
            {
                useKillAnimation = killAnimation[UnityEngine.Random.Range(
                    0, killAnimation.Count)];
            }

            __instance.MyPhysics.StartCoroutine(
                useKillAnimation.CoPerformKill(__instance, target));

            __instance.logger.Debug(
                $"{__instance.PlayerId} succeeded in murdering {target.PlayerId}", null);

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

            if (instance.AmOwner)
            {
                StatsManager.Instance.IncrementStat(StringNames.StatsImpostorKills);
                if (instance.CurrentOutfitType == PlayerOutfitType.Shapeshifted)
                {
                    StatsManager.Instance.IncrementStat(StringNames.StatsShapeshifterShiftedKills);
                }
                if (Constants.ShouldPlaySfx())
                {
                    SoundManager.Instance.PlaySound(
                        instance.KillSfx, false, 0.8f);
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
                    { }
                }
                FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(
                    instance.Data, target.Data);
                FastDestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.SetActive(false);
                target.cosmetics.SetNameMask(false);
                target.RpcSetScanner(false);
                ImportantTextTask importantTextTask = new GameObject("_Player").AddComponent<ImportantTextTask>();
                importantTextTask.transform.SetParent(
                    instance.transform, false);
                if (!PlayerControl.GameOptions.GhostsDoTasks)
                {
                    target.ClearTasks();
                    importantTextTask.Text = FastDestroyableSingleton<TranslationController>.Instance.GetString(
                        StringNames.GhostIgnoreTasks, Array.Empty<Il2CppSystem.Object>());
                }
                else
                {
                    importantTextTask.Text = FastDestroyableSingleton<TranslationController>.Instance.GetString(
                        StringNames.GhostDoTasks, Array.Empty<Il2CppSystem.Object>());
                }
                target.myTasks.Insert(0, importantTextTask);
            }
            FastDestroyableSingleton<AchievementManager>.Instance.OnMurder(
                instance.AmOwner, target.AmOwner);

            var killAnimation = instance.KillAnimations.ToList();

            var useKillAnimation = default(KillAnimation);

            if (killAnimation.Count > 0)
            {
                useKillAnimation = killAnimation[UnityEngine.Random.Range(
                    0, killAnimation.Count)];
            }

            instance.MyPhysics.StartCoroutine(
                useKillAnimation.CoPerformKill(instance, target));
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

            var killCool = PlayerControl.GameOptions.KillCooldown;
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
            if (role.IsVanillaRole()) { return true; }


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
                __instance.RawSetName(newOutfit.PlayerName);
                __instance.RawSetColor(newOutfit.ColorId);
                __instance.RawSetHat(newOutfit.HatId, newOutfit.ColorId);
                __instance.RawSetSkin(newOutfit.SkinId, newOutfit.ColorId);
                __instance.RawSetVisor(newOutfit.VisorId, newOutfit.ColorId);
                __instance.RawSetPet(newOutfit.PetId, newOutfit.ColorId);
                __instance.Visible = __instance.Visible;
                if (targetPlayerInfo.PlayerId == __instance.Data.PlayerId)
                {
                    __instance.CurrentOutfitType = PlayerOutfitType.Default;
                    __instance.Data.Outfits.Remove(PlayerOutfitType.Shapeshifted);
                }
                else
                {
                    __instance.CurrentOutfitType = PlayerOutfitType.Shapeshifted;
                    __instance.Data.SetOutfit(__instance.CurrentOutfitType, newOutfit);
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
                    changeOutfit();
                    __instance.cosmetics.SetScale(__instance.defaultPlayerScale);
                };

                roleEffectAnimation.MidAnimCB = changeAction;
                
                if (Constants.ShouldHorseAround())
                {
                    __instance.StartCoroutine(__instance.ScalePlayer(0.3f, 0.25f));
                }
                else
                {
                    __instance.StartCoroutine(__instance.ScalePlayer(0.7f, 0.25f));
                }

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
            changeOutfit();
            return false;

        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
    public static class PlayerControlRpcSyncSettingsPatch
    {
        public static void Postfix()
        {
            OptionHolder.ShareOptionSelections();
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Revive))]
    public static class PlayerControlRevivePatch
    {
        public static void Postfix(PlayerControl __instance)
        {

            ExtremeRolesPlugin.ShipState.RemoveDeadInfo(__instance.PlayerId);

            if (ExtremeRoleManager.GameRole.Count == 0) { return; }
            if (!ExtremeRolesPlugin.ShipState.IsRoleSetUpEnd) { return; }

            var (onRevive, onReviveOther) = ExtremeRoleManager.GetInterfaceCastedRole<
                IRoleOnRevive>(__instance.PlayerId);

            onRevive?.ReviveAction(__instance);
            onReviveOther?.ReviveAction(__instance);

            var ghostRole = ExtremeGhostRoleManager.GetLocalPlayerGhostRole();
            if (ghostRole == null) { return; }

            if (__instance.PlayerId == CachedPlayerControl.LocalPlayer.PlayerId)
            {
                if (ghostRole.Button != null)
                {
                    ghostRole.Button.SetActive(false);
                    ghostRole.Button.ForceAbilityOff();
                }

                ghostRole.ReseOnMeetingStart();
            }

            lock(ExtremeGhostRoleManager.GameRole)
            {
                ExtremeGhostRoleManager.GameRole.Remove(__instance.PlayerId);
            }
        }
    }
}
