using System.Collections.Generic;
using System.Linq;
using Hazel;

using ExtremeRoles.Performance;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Extension.Ship;

namespace ExtremeRoles
{
    public static class RPCOperator
    {
        public enum SoundType : byte
        {
            Kill,
        }

        public enum Command : byte
        {
            // メインコントール
            Initialize = 60,
            ForceEnd,
            SetRoleToAllPlayer,
            ShareOption,
            CustomVentUse,
            StartVentAnimation,
            UncheckedSnapTo,
            UncheckedShapeShift,
            UncheckedMurderPlayer,
            UncheckedRevive,
            CleanDeadBody,
            FixLightOff,
            ReplaceDeadReason,
            SetRoleWin,
            SetWinGameControlId,
            SetWinPlayer,
            ShareMapId,
            ShareVersion,
            PlaySound,
            IntegrateModCall,

            // 役職関連
            // 役職メインコントール
            ReplaceRole,

            // コンビロール全般
            HeroHeroAcademia,

            // クルーメイト
            BodyGuardAbility,
            TimeMasterAbility,
            AgencyTakeTask,
            AgencySetNewTask,
            FencerCounterOn,
            FencerCounterOff,
            FencerEnableKillButton,
            CuresMakerCurseKillCool,
            CarpenterUseAbility,
            SurvivorDeadWin,
            CaptainAbility,
            ResurrecterRpc,

            // インポスター
            AssasinVoteFor,
            CarrierAbility,
            PainterPaintBody,
            FakerCreateDummy,
            OverLoaderSwitchAbility,
            CrackerCrackDeadBody,
            MerySetCamp,
            MeryAcivateVent,
            SlaveDriverSetNewTask,
            LastWolfSwitchLight,
            CommanderAttackCommand,
            HypnotistAbility,
            UnderWarperUseVentWithNoAnime,

            // ニュートラル
            AliceShipBroken,
            TaskMasterSetNewTask,
            JesterOutburstKill,
            YandereSetOneSidedLover,
            TotocalcioSetBetPlayer,
            MadmateToFakeImpostor,

            // 幽霊役職
            SetGhostRole,
            UseGhostRoleAbility,

            XionAbility,
        }

        public static void Call(
            uint netId, Command ops)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                netId, (byte)ops,
                Hazel.SendOption.Reliable, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void Call(
            uint netId, Command ops, List<byte> value)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                netId, (byte)ops,
                Hazel.SendOption.Reliable, -1);
            foreach (byte writeVale in value)
            {
                writer.Write(writeVale);
            }
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static void RoleIsWin(byte playerId)
        {
            Call(CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                Command.SetRoleWin, new List<byte>{ playerId });
            SetRoleWin(playerId);
        }

        public static void CleanDeadBody(byte targetId)
        {
            DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; ++i)
            {
                if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == targetId)
                {
                    UnityEngine.Object.Destroy(array[i].gameObject);
                    break;
                }
            }
        }

        public static void Initialize()
        {
            OptionHolder.Load();
            RandomGenerator.Initialize();
            Helper.Player.ResetTarget();
            Roles.ExtremeRoleManager.Initialize();
            GhostRoles.ExtremeGhostRoleManager.Initialize();
            ExtremeRolesPlugin.ShipState.Initialize();
            ExtremeRolesPlugin.Info.ResetOverlays();
            
            // チェックポイントリセット
            Helper.Logging.ResetCkpt();

            // キルアニメーションリセット
            Patches.KillAnimationCoPerformKillPatch.HideNextAnimation = false;

            // 各種表示系リセット
            Patches.Manager.HudManagerUpdatePatch.Reset();

            // ミーティング能力リセット
            Patches.Meeting.PlayerVoteAreaSelectPatch.Reset();

            // 各種システムコンソールリセット
            Patches.MiniGame.VitalsMinigameUpdatePatch.Initialize();
            Patches.MiniGame.SecurityHelper.Initialize();
            Patches.MapOverlay.MapCountOverlayUpdatePatch.Initialize();

            // 最終結果リセット
            Module.CustomMonoBehaviour.FinalSummary.Reset();

            VentExtension.ResetCustomVent();
        }

        public static void ForceEnd()
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (!player.Data.Role.IsImpostor)
                {
                    player.RemoveInfected();
                    player.MurderPlayer(player);
                    player.Data.IsDead = true;
                }
            }
        }
        public static void FixLightOff()
        {

            var switchMiniGame = Minigame.Instance as SwitchMinigame;

            if (switchMiniGame != null)
            {
                Minigame.Instance.ForceClose();
            }

            SwitchSystem switchSystem = CachedShipStatus.Systems[
                SystemTypes.Electrical].Cast<SwitchSystem>();
            switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
        }

        public static void SetRoleToAllPlayer(List<Module.IAssignedPlayer> assignData)
        {
            foreach (var data in assignData)
            {
                switch (data.RoleType)
                {
                    case (byte)Module.IAssignedPlayer.ExRoleType.Single:
                        Roles.ExtremeRoleManager.SetPlyerIdToSingleRoleId(
                            data.RoleId, data.PlayerId);
                        break;
                    case (byte)Module.IAssignedPlayer.ExRoleType.Comb:
                        var combData = (Module.AssignedPlayerToCombRoleData)data;
                        Roles.ExtremeRoleManager.SetPlayerIdToMultiRoleId(
                            combData.CombTypeId,
                            combData.RoleId,
                            combData.PlayerId,
                            combData.GameContId,
                            combData.AmongUsRoleId);
                        break;
                }
            }
        }

        public static void ShareOption(int numOptions, MessageReader reader)
        {
            OptionHolder.ShareOption(numOptions, reader);
        }

        public static void ReplaceDeadReason(byte playerId, byte reason)
        {
            ExtremeRolesPlugin.ShipState.ReplaceDeadReason(
                playerId, (ExtremeShipStatus.PlayerStatus)reason);
        }

        public static void CustomVentUse(
            int ventId, byte playerId, byte isEnter)
        {

            HudManager hudManager = FastDestroyableSingleton<HudManager>.Instance;
            ShipStatus ship = CachedShipStatus.Instance;

            if (ship == null || hudManager == null) { return; }

            PlayerControl player = Helper.Player.GetPlayerControlById(playerId);
            if (player == null) { return; }

            MessageReader reader = new MessageReader();
            
            byte[] bytes = System.BitConverter.GetBytes(ventId);
            if (!System.BitConverter.IsLittleEndian)
            {
                System.Array.Reverse(bytes);
            }
            reader.Buffer = bytes;
            reader.Length = bytes.Length;

            Vent vent = ship.AllVents.FirstOrDefault(
                (x) => x.Id == ventId);

            hudManager.StartCoroutine(
                Effects.Lerp(
                    0.6f, new System.Action<float>((p) => {
                        if (vent != null && vent.myRend != null)
                        {
                            vent.myRend.sprite = ship.GetCustomVentSprite(
                                ventId, (int)(p * 17));
                            if (p == 1f)
                            {
                                vent.myRend.sprite = ship.GetCustomVentSprite(
                                    ventId, 0);
                            }
                        }
                    })));

            player.MyPhysics.HandleRpc(isEnter != 0 ? (byte)19 : (byte)20, reader);
        }

        public static void StartVentAnimation(int ventId)
        {

            if (CachedShipStatus.Instance == null) { return; }
            Vent vent = CachedShipStatus.Instance.AllVents.FirstOrDefault(
                (x) => x.Id == ventId);

            HudManager hudManager = FastDestroyableSingleton<HudManager>.Instance;
            ShipStatus ship = CachedShipStatus.Instance;

            if (ship.IsCustomVent(ventId))
            {
                if (hudManager == null) { return; }

                hudManager.StartCoroutine(
                    Effects.Lerp(
                        0.6f, new System.Action<float>((p) => {
                            if (vent != null && vent.myRend != null)
                            {
                                vent.myRend.sprite = ship.GetCustomVentSprite(
                                    ventId, (int)(p * 17));
                                if (p == 1f)
                                {
                                    vent.myRend.sprite = ship.GetCustomVentSprite(
                                        ventId, 0);
                                }
                            }   
                        })
                    )
                );
            }
            else
            {
                vent?.GetComponent<PowerTools.SpriteAnim>()?.Play(
                    vent.ExitVentAnim, 1f);
            }
        }

        public static void UncheckedSnapTo(
            byte teleporterId, UnityEngine.Vector2 pos)
        {
            PlayerControl teleportPlayer = Helper.Player.GetPlayerControlById(teleporterId);
            if (teleportPlayer != null)
            {
                teleportPlayer.NetTransform.SnapTo(pos);
            }
        }

        public static void UncheckedShapeShift(
            byte sourceId, byte targetId, byte useAnimation)
        {
            PlayerControl source = Helper.Player.GetPlayerControlById(sourceId);
            PlayerControl target = Helper.Player.GetPlayerControlById(targetId);

            bool animate = true;

            if (useAnimation != byte.MaxValue)
            {
                animate = false;
            }
            source.Shapeshift(target, animate);
        }

        public static void UncheckedMurderPlayer(
            byte sourceId, byte targetId, byte useAnimation)
        {

            PlayerControl source = Helper.Player.GetPlayerControlById(sourceId);
            PlayerControl target = Helper.Player.GetPlayerControlById(targetId);

            if (source != null && target != null)
            {
                if (useAnimation == 0)
                {
                    Patches.KillAnimationCoPerformKillPatch.HideNextAnimation = true;
                }
                source.MurderPlayer(target);
            }
        }

        public static void UncheckedRevive(byte targetId)
        {
            PlayerControl target = Helper.Player.GetPlayerControlById(targetId);

            if (target != null)
            {
                target.Revive();

                // なんか起きて失敗
                if (target.Data == null || 
                    target.Data.IsDead || 
                    target.Data.Disconnected) { return; }

                // 死体は消しておく
                CleanDeadBody(target.PlayerId);
            }
        }


        public static void SetWinGameControlId(int id)
        {
            ExtremeRolesPlugin.ShipState.SetWinControlId(id);
        }

        public static void SetWinPlayer(List<byte> playerId)
        {
            foreach (byte id in playerId)
            {
                GameData.PlayerInfo player = GameData.Instance.GetPlayerById(id);
                if (player == null) { continue; }
                ExtremeRolesPlugin.ShipState.AddWinner(player);
            }
        }

        public static void SetRoleWin(byte winPlayerId)
        {
            Roles.ExtremeRoleManager.GameRole[winPlayerId].IsWin = true;
        }
        public static void ShareMapId(byte mapId)
        {
            PlayerControl.GameOptions.MapId = mapId;
        }

        public static void AddVersionData(
            int major, int minor,
            int build, int revision, int clientId)
        {
            ExtremeRolesPlugin.ShipState.AddPlayerVersion(
                clientId, major, minor, build, revision);
        }

        public static void PlaySound(byte soundType)
        {
            UnityEngine.AudioClip clip;
            switch ((SoundType)soundType)
            {
                case SoundType.Kill:
                    clip = CachedPlayerControl.LocalPlayer.PlayerControl.KillSfx;
                    break;
                default:
                    return;
            }

            if (Constants.ShouldPlaySfx() && clip != null)
            {
                SoundManager.Instance.PlaySound(clip, false, 0.8f);
            }
        }

        public static void IntegrateModCall(
            ref MessageReader readeer)
        {
            ExtremeRolesPlugin.Compat.IntegrateModCall(ref readeer);
        }

        public static void ReplaceRole(
            byte callerId, byte targetId, byte operation)
        {
            Roles.ExtremeRoleManager.RoleReplace(
                callerId, targetId,
                (Roles.ExtremeRoleManager.ReplaceOperation)operation);
        }

        public static void HeroHeroAcademiaCommand(
            ref MessageReader reader)
        {
            Roles.Combination.HeroAcademia.RpcCommand(
                ref reader);
        }

        public static void BodyGuardAbility(ref MessageReader reader)
        {
            Roles.Solo.Crewmate.BodyGuard.Ability(ref reader);
        }

        public static void TimeMasterAbility(ref MessageReader reader)
        {
            Roles.Solo.Crewmate.TimeMaster.Ability(ref reader);
        }

        public static void AgencyTakeTask(
            byte targetPlayerId, List<int> getTaskId)
        {
            Roles.Solo.Crewmate.Agency.TakeTargetPlayerTask(
                targetPlayerId, getTaskId);
        }
        public static void AgencySetNewTask(
            byte callerId, int index, int taskIndex)
        {
            Roles.Solo.Crewmate.Agency.ReplaceToNewTask(
                callerId, index, taskIndex);
        }
        public static void FencerCounterOn(
            byte playerId)
        {
            Roles.Solo.Crewmate.Fencer.CounterOn(playerId);
        }
        public static void FencerCounterOff(byte playerId)
        {
            Roles.Solo.Crewmate.Fencer.CounterOff(playerId);
        }
        public static void FencerEnableKillButton(byte playerId)
        {
            Roles.Solo.Crewmate.Fencer.EnableKillButton(playerId);
        }

        public static void CuresMakerCurseKillCool(
            byte playerId, byte targetPlayerId)
        {
            Roles.Solo.Crewmate.CurseMaker.CurseKillCool(
                playerId, targetPlayerId);
        }

        public static void CarpenterUseAbility(ref MessageReader reader)
        {
            Roles.Solo.Crewmate.Carpenter.UpdateMapObject(ref reader);
        }

        public static void SurvivorDeadWin(byte playerId)
        {
            Roles.Solo.Crewmate.Survivor.DeadWin(playerId);
        }

        public static void CaptainTargetVote(ref MessageReader reader)
        {
            Roles.Solo.Crewmate.Captain.UseAbility(ref reader);
        }

        public static void ResurrecterRpc(ref MessageReader reader)
        {
            Roles.Solo.Crewmate.Resurrecter.RpcAbility(ref reader);
        }

        public static void AssasinVoteFor(byte targetId)
        {
            Roles.Combination.Assassin.VoteFor(
                targetId);
        }
        public static void CarrierAbility(
            byte callerId, float x, float y,
            byte targetId, bool deadBodyPickUp)
        {
            Roles.Solo.Impostor.Carrier.Ability(
                callerId, x, y, targetId, deadBodyPickUp);
        }

        public static void PainterPaintBody(
            byte targetId, byte isRandomModeMessage)
        {
            Roles.Solo.Impostor.Painter.PaintDeadBody(
                targetId, isRandomModeMessage);
        }
        public static void FakerCreateDummy(
            byte callerId, byte targetId, byte ops)
        {
            Roles.Solo.Impostor.Faker.CreateDummy(
                callerId, targetId, ops);
        }

        public static void OverLoaderSwitchAbility(
            byte callerId, byte activate)
        {

            Roles.Solo.Impostor.OverLoader.SwitchAbility(
                callerId, activate == byte.MaxValue);
        }

        public static void CrackerCrackDeadBody(
            byte callerId, byte targetId)
        {
            Roles.Solo.Impostor.Cracker.CrackDeadBody(
                callerId, targetId);
        }

        public static void MarySetCamp(byte callerId, float x, float y)
        {
            Roles.Solo.Impostor.Mery.SetCamp(
                callerId, new UnityEngine.Vector2(x, y));
        }
        public static void MaryActiveVent(int index)
        {
            Roles.Solo.Impostor.Mery.ActivateVent(index);
        }
        public static void SlaveDriverSetNewTask(
            byte callerId, int index, int taskIndex)
        {
            Roles.Solo.Impostor.SlaveDriver.ReplaceToNewTask(
                callerId, index, taskIndex);
        }
        public static void LastWolfSwitchLight(byte swichStatus)
        {
            Roles.Solo.Impostor.LastWolf.SwitchLight(
                swichStatus == byte.MinValue);
        }
        public static void CommanderAttackCommand(byte rolePlayerId)
        {
            Roles.Solo.Impostor.Commander.AttackCommad(
                rolePlayerId);
        }

        public static void HypnotistAbility(ref MessageReader reader)
        {
            Roles.Solo.Impostor.Hypnotist.Ability(ref reader);
        }

        public static void UnderWarperUseVentWithNoAnime(
            byte playerId, int ventId, bool isEnter)
        {
            Roles.Solo.Impostor.UnderWarper.UseVentWithNoAnimation(
                playerId, ventId, isEnter);
        }

        public static void AliceShipBroken(
            byte callerId, byte targetPlayerId, List<int> taskId)
        {
            Roles.Solo.Neutral.Alice.ShipBroken(
                callerId, targetPlayerId, taskId);
        }

        public static void TaskMasterSetNewTask(
            byte callerId, int index, int taskIndex)
        {
            Roles.Solo.Neutral.TaskMaster.ReplaceToNewTask(
                callerId, index, taskIndex);
        }
        public static void JesterOutburstKill(
            byte killerId, byte targetId)
        {
            Roles.Solo.Neutral.Jester.OutburstKill(
                killerId, targetId);
        }
        public static void YandereSetOneSidedLover(
            byte playerId, byte loverId)
        {
            Roles.Solo.Neutral.Yandere.SetOneSidedLover(
                playerId, loverId);
        }
        public static void TotocalcioSetBetPlayer(
            byte playerId, byte betPlayerId)
        {
            Roles.Solo.Neutral.Totocalcio.SetBetTarget(
                playerId, betPlayerId);
        }
        public static void MadmateToFakeImpostor(byte playerId)
        {
            Roles.Solo.Neutral.Madmate.ToFakeImpostor(playerId);
        }

        public static void SetGhostRole(
            ref MessageReader reader)
        {
            GhostRoles.ExtremeGhostRoleManager.SetGhostRoleToPlayerId(
                ref reader);
        }

        public static void UseGhostRoleAbility(
            byte abilityType, bool isReport, ref MessageReader reader)
        {
            GhostRoles.ExtremeGhostRoleManager.UseAbility(
                abilityType, isReport, ref reader);
        }

        public static void XionAbility(ref MessageReader reader)
        {
            Roles.Solo.Host.Xion.UseAbility(ref reader);
        }

    }

}
