using AmongUs.GameOptions;
using ExtremeRoles.GameMode;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.RoleAssign;

public sealed class NotAssignPlayerData
{
	public int CrewmateSingleAssignPlayerNum { get; private set; }
	public int ImpostorSingleAssignPlayerNum { get; private set; }

	public int CrewmateMultiAssignPlayerNum { get; private set; }
	public int ImpostorMultiAssignPlayerNum { get; private set; }

	public NotAssignPlayerData()
	{
		int crewSingleAssignNum = 0;
		int impSingleAssignNum = 0;

		int crewMultiAssignNum = 0;
		int impMultiAssignNum = 0;

		foreach (PlayerControl player in PlayerCache.AllPlayerControl)
		{
			var role = player.Data.Role.Role;
			if (VanillaRoleProvider.IsDefaultCrewmateRole(role))
			{
				++crewSingleAssignNum;
				++crewMultiAssignNum;
			}
			else if (VanillaRoleProvider.IsCrewmateAdditionalRole(role))
			{
				++crewMultiAssignNum;
			}
			else if (VanillaRoleProvider.IsDefaultImpostorRole(role))
			{
				++impMultiAssignNum;
				++impSingleAssignNum;
			}
			else if (VanillaRoleProvider.IsImpostorAdditionalRole(role))
			{
				++impMultiAssignNum;
			}
		}

		// シオンのプレイヤーはクルーではなく割り当て済みなので
		if (ExtremeGameModeManager.Instance.EnableXion)
		{
			--crewSingleAssignNum;
			--crewMultiAssignNum;
		}

		CrewmateSingleAssignPlayerNum = crewSingleAssignNum;
		ImpostorSingleAssignPlayerNum = impSingleAssignNum;

		CrewmateMultiAssignPlayerNum = crewMultiAssignNum;
		ImpostorMultiAssignPlayerNum = impMultiAssignNum;
	}
	public void ReduceImpostorAssignNum(int reduceNum = 1)
	{
		ImpostorMultiAssignPlayerNum -= reduceNum;
		ImpostorSingleAssignPlayerNum -= reduceNum;
	}
}
