using ExtremeRoles.Roles.API;

namespace ExtremeRoles.GameMode.Logic.Usable
{
    public class HideNSeekModeUsableLogic : ILogicUsable
    {
        public bool CanUseVent(SingleRoleBase role)
        {
            return !role.IsImpostor();
        }
    }
}
