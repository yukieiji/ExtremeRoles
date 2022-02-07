using System.Collections.Generic;
using Hazel;

namespace ExtremeRoles
{
    public static class RPCOperator
    {

        public enum Command
        {
            // メインコントール
            Initialize = 60,
            RoleSetUpComplete,
            ForceEnd,
            SetNormalRole,
            SetCombinationRole,
            ShareOption,
            UncheckedShapeShift,
            UncheckedMurderPlayer,
            CleanDeadBody,
            FixLightOff,
            ReplaceDeadReason,
            SetRoleWin,
            SetWinGameControlId,
            ShareMapId,
            ShareVersion,

            // 役職関連
            // 役職メインコントール
            ReplaceRole,

            // クルーメイト
            BodyGuardFeatShield,
            BodyGuardResetShield,

            // インポスター
            AssasinAddDead,
            AssasinVoteFor,
            CarrierCarryBody,
            CarrierSetBody,
            PainterPaintBody,
            FakerCreateDummy,
            FakerRemoveAllDummy,
            OverLoaderSwitchAbility,

            // ニュートラル
            AliceShipBroken,
            TaskMasterSetNetTask,
            JesterOutburstKill,
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
            Call(PlayerControl.LocalPlayer.NetId,
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
            ExtremeRolesPlugin.GameDataStore.Initialize();
            ExtremeRolesPlugin.Info.ResetOverlays();
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
            SwitchSystem switchSystem = ShipStatus.Instance.Systems[
                SystemTypes.Electrical].Cast<SwitchSystem>();
            switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
        }

        public static void SetCombinationRole(
            byte roleId, byte playerId, byte id, byte bytedRoleType)
        {
            Roles.ExtremeRoleManager.SetPlayerIdToMultiRoleId(
                roleId, playerId, id, bytedRoleType);
        }

        public static void SetNormalRole(byte roleId, byte playerId)
        {
            Roles.ExtremeRoleManager.SetPlyerIdToSingleRoleId(
                roleId, playerId);
        }

        public static void ShareOption(int numOptions, MessageReader reader)
        {
            OptionHolder.ShareOption(numOptions, reader);
        }

        public static void ReplaceDeadReason(byte playerId, byte reason)
        {
            ExtremeRolesPlugin.GameDataStore.ReplaceDeadReason(
                playerId, (Module.GameDataContainer.PlayerStatus)reason);
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
                    Patches.KillAnimationCoPerformKillPatch.hideNextAnimation = true;
                }
                source.MurderPlayer(target);

                var targetRole = Roles.ExtremeRoleManager.GameRole[targetId];

                if (Roles.ExtremeRoleManager.IsDisableWinCheckRole(targetRole))
                {
                    ExtremeRolesPlugin.GameDataStore.WinCheckDisable = true;
                }

                targetRole.RolePlayerKilledAction(
                    target, source);

                ExtremeRolesPlugin.GameDataStore.WinCheckDisable = false;
            }
        }

        public static void SetWinGameControlId(int id)
        {
            ExtremeRolesPlugin.GameDataStore.WinGameControlId = id;
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
            ExtremeRolesPlugin.GameDataStore.PlayerVersion[
                clientId] = new System.Version(
                    major, minor, build, revision);
        }

        public static void ReplaceRole(
            byte callerId, byte targetId, byte operation)
        {
            Roles.ExtremeRoleManager.RoleReplace(
                callerId, targetId,
                (Roles.ExtremeRoleManager.ReplaceOperation)operation);
        }

        public static void BodyGuardFeatShield(
            byte playerId,
            byte targetPlayer)
        {
            ExtremeRolesPlugin.GameDataStore.ShildPlayer.Add(
                playerId, targetPlayer);
        }

        public static void BodyGuardResetShield(byte playerId)
        {
            ExtremeRolesPlugin.GameDataStore.ShildPlayer.Remove(playerId);
        }

        public static void AssasinAddDead(byte playersId)
        {
            Roles.Combination.Assassin.AddDead(
                playersId);
        }
        public static void AssasinVoteFor(byte targetId)
        {
            Roles.Combination.Assassin.VoteFor(
                targetId);
        }
        public static void CarrierCarryBody(
            byte callerId, byte targetId)
        {
            Roles.Solo.Impostor.Carrier.CarryDeadBody(
                callerId, targetId);
        }
        public static void CarrierSetBody(byte callerId)
        {
            Roles.Solo.Impostor.Carrier.PlaceDeadBody(
                callerId);
        }
        public static void PainterPaintBody(
            byte callerId, byte targetId)
        {
            Roles.Solo.Impostor.Painter.PaintDeadBody(
                callerId, targetId);
        }
        public static void FakerCreateDummy(
            byte callerId, byte targetId)
        {
            Roles.Solo.Impostor.Faker.CreateDummy(
                callerId, targetId);
        }
        public static void FakerRemoveAllDummy(byte callerId)
        {
            Roles.Solo.Impostor.Faker.RemoveAllDummyPlayer(
                callerId);
        }

        public static void OverLoaderSwitchAbility(
            byte callerId, byte activate)
        {

            Roles.Solo.Impostor.OverLoader.SwitchAbility(
                callerId, activate == byte.MaxValue);
        }

        public static void AliceShipBroken(byte callerId)
        {
            Roles.Solo.Neutral.Alice.ShipBroken(callerId);
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

    }

}
