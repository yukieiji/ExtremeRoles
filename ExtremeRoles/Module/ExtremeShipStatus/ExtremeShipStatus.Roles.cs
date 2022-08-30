using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;

namespace ExtremeRoles.Module.ExtremeShipStatus
{
    public sealed partial class ExtremeShipStatus
    {
        private BakeryUnion union;

        public void AddGlobalActionRole(SingleRoleBase role)
        {
            var allOpt = OptionHolder.AllOption;

            switch (role.Id)
            {
                case ExtremeRoleId.Bakary:
                    if (this.union != null) { return; }
                    this.union = this.status.AddComponent<BakeryUnion>();
                    this.union.SetCookingCondition(
                        allOpt[role.GetRoleOptionId(Bakary.BakaryOption.GoodBakeTime)].GetValue(),
                        allOpt[role.GetRoleOptionId(Bakary.BakaryOption.BadBakeTime)].GetValue(),
                        allOpt[role.GetRoleOptionId(Bakary.BakaryOption.ChangeCooking)].GetValue());
                    break;
                default:
                    break;
            }
        }

        private string getRoleAditionalInfo()
        {
            if (this.union == null) { return string.Empty; }

            return this.union.GetBreadBakingCondition();
        }

        private bool isShowRoleAditionalInfo()
        {
            if (this.union == null) { return false; }

            return this.union.IsEstablish();
        }
        
        private void resetGlobalAction()
        {
            this.union?.ResetTimer();
        }
    }
}
