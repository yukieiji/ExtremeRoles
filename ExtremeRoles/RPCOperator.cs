using System;
using Hazel;

namespace ExtremeRoles
{
    public static class RPCOperator
    {

        public enum Command
        {

            GameInit = 60,
            ForceEnd,
            SetNormalRole,
            SetCombinationRole,
            ShareOption,
            UncheckedMurderPlayer,

            ReplaceRole,

            AliceAbility
        }

        public static void GameInit()
        {
            OptionsHolder.Load();
            RandomGenerator.Init();
            Roles.ExtremeRoleManager.GameInit();
            Module.GameDataContainer.GameInit();
            Patches.AssassinMeeting.Reset();
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

        public static void SetCombinationRole(
            byte roleId, byte playerId, byte roleIndex, byte id)
        {
            Roles.ExtremeRoleManager.SetPlayerIdToMultiRoleId(
                roleId, playerId, roleIndex, id);
        }

        public static void SetNormalRole(byte roleId, byte playerId)
        {
            Roles.ExtremeRoleManager.SetPlyerIdToSingleRoleId(
                roleId, playerId);
        }

        public static void ShareOption(int numOptions, MessageReader reader)
        {
            OptionsHolder.ShareOption(numOptions, reader);
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
                targetRole.RolePlayerKilledAction(
                    target, source);
                
                if (!targetRole.HasTask)
                {
                    target.ClearTasks();
                }

            }
        }

        public static void ReplaceRole(
            byte callerId, byte targetId, byte operation)
        {
            Roles.ExtremeRoleManager.RoleReplace(
                callerId, targetId,
                (Roles.ExtremeRoleManager.ReplaceOperation)operation);
        }

        public static void AliceAbility(byte callerId)
        {
            Roles.Solo.Neutral.Alice.ShipBroken(callerId);
        }

    }

}
