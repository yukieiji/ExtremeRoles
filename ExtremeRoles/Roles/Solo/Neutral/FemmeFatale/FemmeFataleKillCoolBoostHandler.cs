using ExtremeRoles.Helper;
using System;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral.FemmeFatale;

/// <summary>
/// ホセ・ホセ候補のキルによるファムのキルクールタイム短縮を管理するハンドラ
/// </summary>
public sealed class FemmeFataleKillCoolBoostHandler(
	FemmeFataleStatusModel statusModel,
	float killKillCoolReduceRate)
{
    private readonly FemmeFataleStatusModel statusModel = statusModel;
    private readonly float killKillCoolReduceRate = killKillCoolReduceRate;

    public void HookMurderPlayer(PlayerControl source, PlayerControl target)
    {
		var local = PlayerControl.LocalPlayer;
        if (source.PlayerId != local.PlayerId ||
			source.PlayerId == target.PlayerId ||
            !statusModel.IsJoseOrCandidate(source.PlayerId) ||
			killKillCoolReduceRate >= 1.0f)
        {
            return;
        }

        float killcool = local.killTimer;
		local.killTimer = Math.Clamp(killcool * killKillCoolReduceRate, 0.05f, 360f);
    }
}
