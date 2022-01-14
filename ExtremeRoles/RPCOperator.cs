using System.Collections.Generic;
using Hazel;

namespace ExtremeRoles
{
    public static class RPCOperator
    {

        public enum Command
        {

            Initialize = 60,
            RoleSetUpComplete,
            ForceEnd,
            SetNormalRole,
            SetCombinationRole,
            ShareOption,
            UncheckedMurderPlayer,
            CleanDeadBody,
            FixLightOff,
            ReplaceDeadReason,

            ReplaceRole,


            AssasinAddDead,
            AssasinVoteFor,
            AliceAbility,
            CarrierCarryBody,
            CarrierSetBody,
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

        public static void ReplaceRole(
            byte callerId, byte targetId, byte operation)
        {
            Roles.ExtremeRoleManager.RoleReplace(
                callerId, targetId,
                (Roles.ExtremeRoleManager.ReplaceOperation)operation);
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

        public static void AliceAbility(byte callerId)
        {
            Roles.Solo.Neutral.Alice.ShipBroken(callerId);
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
    }

}
