using System.Collections.Generic;
using System.Linq;

using AmongUs.GameOptions;

using ExtremeRoles.Extension.Manager;
using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral.FemmeFatale;

/// <summary>
/// ホセ候補の登録からホセへの昇格、ホセの解除を管理するハンドラ
/// </summary>
public sealed class FemmeFataleJosePromotionHandler(
    FemmeFataleStatusModel statusModel,
    int promoteMeetingCount)
{
    private readonly FemmeFataleStatusModel statusModel = statusModel;
    private readonly int promoteMeetingCount = promoteMeetingCount;
    private readonly Dictionary<byte, int> promoteCounters = [];

    public bool CanPromote(PlayerControl? target)
    {
        return target != null &&
            !statusModel.IsJoseOrCandidate(target.PlayerId);
    }

    public void AddCandidate(byte playerId)
    {
        statusModel.AddCandidate(playerId);
        promoteCounters[playerId] = 0;
    }

    public void UpdateMeetings()
    {
        if (statusModel.JoseCandidatePlayerIds.Count == 0)
        {
            return;
        }

        byte localId = PlayerControl.LocalPlayer.PlayerId;
        foreach (byte candidateId in statusModel.JoseCandidatePlayerIds.ToArray())
        {
            int meetingCount = promoteCounters.TryGetValue(candidateId, out int registeredCount)
                ? registeredCount + 1
                : 1;

            if (meetingCount < promoteMeetingCount)
            {
                promoteCounters[candidateId] = meetingCount;
                continue;
            }

            ExtremeRoleManager.RpcReplaceRole(
                localId, candidateId, ExtremeRoleManager.ReplaceOperation.CreateJose);
            promoteCounters.Remove(candidateId);
            statusModel.RemoveCandidate(candidateId);
        }
    }

    public void RemoveAllJoses()
    {
        byte localId = PlayerControl.LocalPlayer.PlayerId;
        foreach (byte playerId in statusModel.JosePlayerIds.ToArray())
        {
            ExtremeRoleManager.RpcReplaceRole(
                localId, playerId, ExtremeRoleManager.ReplaceOperation.RemoveJose);
        }
        promoteCounters.Clear();
        statusModel.ClearAll();
    }

    public static void TargetToJose(byte rolePlayerId, byte targetPlayerId)
    {
        if (!(
                ExtremeRoleManager.TryGetSafeCastedRole<FemmeFataleRole>(rolePlayerId, out var femme) &&
                Player.TryGetPlayerControl(targetPlayerId, out var targetPlayer)
            ))
        {
            return;
        }

        var targetRole = ExtremeRoleManager.GameRole[targetPlayerId];

        IParentChainStatus.PurgeParent(targetPlayerId);
        resetTargetAnotherRole(targetRole, targetPlayerId, targetPlayer);
        replaceVanilaRole(targetRole, targetPlayer);

        JoseRole jose = new JoseRole(
            targetPlayerId,
            rolePlayerId,
            femme,
            targetRole);

        if (targetRole is VanillaRoleWrapper vanillaRole)
        {
            vanillaRole.AnotherRole = null;
            vanillaRole.CanHasAnotherRole = false;
            vanillaRole.CanCallMeeting = true;
            vanillaRole.CanUseAdmin = true;
            vanillaRole.CanUseSecurity = true;
            vanillaRole.CanUseVital = true;

            jose.CanHasAnotherRole = true;
            jose.SetAnotherRole(vanillaRole);

            ExtremeRoleManager.SetNewRole(targetPlayerId, jose);
        }
        else if (targetRole is MultiAssignRoleBase multiAssignRole)
        {
            multiAssignRole.AnotherRole = null;

            multiAssignRole.CanHasAnotherRole = true;
            jose.CanHasAnotherRole = false;

            ExtremeRoleManager.SetNewAnothorRole(targetPlayerId, jose);
        }
        else
        {
            jose.CanHasAnotherRole = true;
            jose.SetAnotherRole(targetRole);

            ExtremeRoleManager.SetNewRole(targetPlayerId, jose);
        }

        femme.AddJosePlayer(targetPlayerId);

        if (targetPlayerId == PlayerControl.LocalPlayer.PlayerId)
        {
            HudManager.Instance.ReGridButtons();
        }
    }

    private static void resetTargetAnotherRole(
        SingleRoleBase targetRole,
        byte targetPlayerId,
        PlayerControl targetPlayer)
    {
        if (targetRole is not MultiAssignRoleBase multiAssignRole)
        {
            return;
        }

        if (PlayerControl.LocalPlayer.PlayerId == targetPlayerId)
        {
            if (multiAssignRole.AnotherRole is IRoleAbility abilityRole)
            {
                abilityRole.Button.OnMeetingStart();
            }
            if (multiAssignRole.AnotherRole is IRoleResetMeeting meetingResetRole)
            {
                meetingResetRole.ResetOnMeetingStart();
            }
        }

        if (multiAssignRole.AnotherRole is IRoleSpecialReset specialResetRole)
        {
            specialResetRole.AllReset(targetPlayer);
        }
    }

    private static void replaceVanilaRole(
        SingleRoleBase targetRole,
        PlayerControl targetPlayer)
    {
        if (targetRole is MultiAssignRoleBase multiAssignRole &&
            multiAssignRole.AnotherRole is VanillaRoleWrapper)
        {
            RoleManager.Instance.SetRole(
                targetPlayer, RoleTypes.Crewmate);
            return;
        }

        switch (targetPlayer.Data.Role.Role)
        {
            case RoleTypes.Crewmate:
            case RoleTypes.Impostor:
                RoleManager.Instance.SetRole(
                    targetPlayer, RoleTypes.Crewmate);
                break;
        }
    }
}
