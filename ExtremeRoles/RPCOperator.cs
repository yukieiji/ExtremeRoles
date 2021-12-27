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
            SetRole,
            ShareOption,
            UncheckedMurderPlayer,

            ReplaceRole,
        }

        public static void GameInit()
        {
            Roles.ExtremeRoleManager.GameInit();
            Module.PlayerDataContainer.GameInit();
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
        public static void SetRole(byte roleId, byte playerId, byte id)
        {
            if (id != Byte.MaxValue)
            {
                Roles.ExtremeRoleManager.SetPlayerIdToMultiRoleId(roleId, playerId, id);
            }
            else
            {
                Roles.ExtremeRoleManager.SetPlyerIdToSingleRoleId(roleId, playerId);
            }
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
                };
                source.MurderPlayer(target);
                Roles.ExtremeRoleManager.GameRole[targetId].RolePlayerKilledAction(
                    target, source);
            }
        }

        public static void ReplaceRole(
            byte callerId, byte targetId, byte operation)
        {

        }

    }

}
