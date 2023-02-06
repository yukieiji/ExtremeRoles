using AmongUs.GameOptions;

using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Module.RoleAssign
{
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

            foreach (PlayerControl player in
                PlayerControl.AllPlayerControls.GetFastEnumerator())
            {
                switch (player.Data.Role.Role)
                {
                    case RoleTypes.Crewmate:
                        ++crewSingleAssignNum;
                        ++crewMultiAssignNum;
                        break;
                    case RoleTypes.Scientist:
                    case RoleTypes.Engineer:
                        ++crewMultiAssignNum;
                        break;
                    case RoleTypes.Impostor:
                        ++impMultiAssignNum;
                        ++impSingleAssignNum;
                        break;
                    case RoleTypes.Shapeshifter:
                        ++impMultiAssignNum;
                        ++impSingleAssignNum;
                        break;
                    default:
                        break;
                }
            }

            CrewmateSingleAssignPlayerNum = crewSingleAssignNum;
            ImpostorSingleAssignPlayerNum = impSingleAssignNum;

            CrewmateMultiAssignPlayerNum = crewMultiAssignNum;
            ImpostorMultiAssignPlayerNum = impMultiAssignNum;
        }
        public void ReduceImpostorAssignNum(int reduceNum = 1)
        {
            ImpostorMultiAssignPlayerNum  -= reduceNum;
            ImpostorSingleAssignPlayerNum -= reduceNum;
        }
    }
}
