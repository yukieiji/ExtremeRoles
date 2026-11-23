using System;
using System.Collections.Generic;
using System.Linq;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.SystemType;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral.FemmeFatale;

/// <summary>
/// ホセ・ホセ候補のタスク進捗・全タスク完了による
/// ファムのキルクールタイム短縮を管理するハンドラ
/// 死亡ホセ・死亡候補も対象(ゴーストタスク完了も含む)
/// </summary>
public sealed class FemmeFataleTaskCoolBoostHandler(
	FemmeFataleRole role,
	FemmeFataleStatusModel statusModel,
	float taskKillCoolReduceRate,
	float taskCompKillCoolReduceRate)
{
    private readonly FemmeFataleRole role = role;
    private readonly FemmeFataleStatusModel statusModel = statusModel;
    private readonly float taskKillCoolReduceRate = taskKillCoolReduceRate;
    private readonly float taskCompKillCoolReduceRate = taskCompKillCoolReduceRate;

    private readonly Dictionary<byte, float> joseTaskGage = [];
    private readonly HashSet<byte> taskCompJose = [];

    public void Update(PlayerControl rolePlayer)
    {
        if (!GameProgressSystem.IsTaskPhase)
        {
            return;
        }

        foreach (byte playerId in statusModel.JosePlayerIds.Concat(statusModel.JoseCandidatePlayerIds))
        {
            if (!Player.TryGetPlayerControl(playerId, out var player))
            {
                continue;
            }

            float gage = Player.GetPlayerTaskGage(player);
            if (!joseTaskGage.ContainsKey(playerId))
            {
                joseTaskGage.Add(playerId, 0.0f);
            }
            float prevGage = joseTaskGage[playerId];
            joseTaskGage[playerId] = gage;

            float killcool = rolePlayer.killTimer;
            if (gage > prevGage && killcool > 0.0f && taskKillCoolReduceRate < 1.0f)
            {
                rolePlayer.killTimer = Math.Clamp(killcool * taskKillCoolReduceRate, 0.05f, killcool);
            }
            
			if (gage < 1.0f || taskCompJose.Contains(playerId))
            {
				continue;
            }

			taskCompJose.Add(playerId);
			
			if (taskCompKillCoolReduceRate >= 1.0f)
			{
				continue;
			}

			if (!role.HasOtherKillCool)
			{
				role.KillCoolTime = Player.DefaultKillCoolTime;
			}
			role.HasOtherKillCool = true;
			role.KillCoolTime = Math.Clamp(role.KillCoolTime * taskCompKillCoolReduceRate, 0.05f, 360f);
		}
    }

    public void Reset()
    {
        joseTaskGage.Clear();
        taskCompJose.Clear();
    }
}
