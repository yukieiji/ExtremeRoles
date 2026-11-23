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
        if (source.PlayerId == target.PlayerId ||
            !statusModel.IsJoseOrCandidate(source.PlayerId) ||
			killKillCoolReduceRate >= 1.0f)
        {
            return;
        }

        float killcool = PlayerControl.LocalPlayer.killTimer;
		PlayerControl.LocalPlayer.killTimer = Math.Clamp(0.0f, killcool * killKillCoolReduceRate, 270f);
    }
}
