using Hazel;

namespace ExtremeRoles
{
    public enum RoleGameOverReason
    {
        AssassinationMarin = 10,
    }

    enum CustomRPC
    {
        // Main Controls

        GameInit = 60,
        ForceEnd,
        SetRole,
        ShareOption,
        VersionHandshake,
        UseUncheckedVent,
        UncheckedMurderPlayer,
        UncheckedCmdReportDeadBody,
        UncheckedExilePlayer,

    }

    public class ExtremeRoleRPC
    {
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
        public static void SetRole(byte roleId, byte playerId, bool combinationRole)
        {
            if (combinationRole)
            {
                Roles.ExtremeRoleManager.SetPlayerIdToMultiRoleId(roleId, playerId);
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
    }

}
