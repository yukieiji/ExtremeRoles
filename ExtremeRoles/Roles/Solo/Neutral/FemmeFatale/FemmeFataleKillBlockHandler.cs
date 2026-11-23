using ExtremeRoles.Helper;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Roles.API.Interface.Ability;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral.FemmeFatale;

/// <summary>
/// ホセ・ホセ候補によるファムへのキルブロック（身代わり死）を管理するハンドラ
/// </summary>
public sealed class FemmeFataleKillBlockHandler(
	FemmeFataleStatusModel statusModel,
	bool killDefense,
	bool candidateKillDefense) : IAbility, IKilledFrom
{
    private readonly FemmeFataleStatusModel statusModel = statusModel;
    private readonly bool killDefense = killDefense;
    private readonly bool candidateKillDefense = candidateKillDefense;

    public bool TryKilledFrom(
        PlayerControl rolePlayer, PlayerControl fromPlayer)
	{
		if (!this.killDefense)
		{
			return true;
		}

		byte killerPlayerId = fromPlayer.PlayerId;
		byte femmePlayerId = rolePlayer.PlayerId;

		var guardPlayers = candidateKillDefense ? 
			this.statusModel.GetLivingJosesAndCandidates() : this.statusModel.GetLivingJoses();
		int size = guardPlayers.Count;

		if (size == 0)
		{
			return true;
		}

		int idx = RandomGenerator.Instance.Next(0, size);
		byte chosenJose = guardPlayers[idx];

		using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.FemmeFataleAbility))
		{
			caller.WriteByte((byte)FemmeFataleRole.RpcOps.CoverDead);
			caller.WriteByte(killerPlayerId);
			caller.WriteByte(femmePlayerId);
			caller.WriteByte(chosenJose);
		}
		CoverDead(killerPlayerId, femmePlayerId, chosenJose);
		return false;
	}

    public static void CoverDead(byte killerPlayerId, byte prevTargetPlayerId, byte targetJosePlayerId)
    {
        byte localId = PlayerControl.LocalPlayer.PlayerId;
        if (localId == prevTargetPlayerId)
        {
            Sound.PlaySound(Sound.Type.GuardianAngleGuard, 0.6f);
        }

        RPCOperator.UncheckedMurderPlayer(killerPlayerId, targetJosePlayerId, byte.MinValue);

        if (!Player.TryGetPlayerControl(targetJosePlayerId, out var josePlayer) ||
            josePlayer.Data == null ||
            !josePlayer.Data.IsDead ||
            josePlayer.Data.Disconnected)
        {
            return;
        }

        ExtremeRolesPlugin.ShipState.ReplaceDeadReason(
            targetJosePlayerId, ExtremeShipStatus.PlayerStatus.Martyrdom);
    }
}
