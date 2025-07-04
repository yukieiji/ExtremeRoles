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
			switch (player.Data.Role.Role)
			{
				case RoleTypes.Crewmate:
					++crewSingleAssignNum;
					++crewMultiAssignNum;
					break;
				case RoleTypes.Scientist:
				case RoleTypes.Engineer:
				case RoleTypes.Noisemaker:
				case RoleTypes.Tracker:
					++crewMultiAssignNum;
					break;
				case RoleTypes.Impostor:
					++impMultiAssignNum;
					++impSingleAssignNum;
					break;
				case RoleTypes.Shapeshifter:
				case RoleTypes.Phantom:
					++impMultiAssignNum;
					break;
				default:
					break;
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
