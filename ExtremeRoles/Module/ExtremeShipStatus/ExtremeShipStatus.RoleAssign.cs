namespace ExtremeRoles.Module.ExtremeShipStatus
{
    public sealed partial class ExtremeShipStatus
    {
        public bool IsRoleSetUpEnd => isRoleSetUpEnd;

        private bool isRoleSetUpEnd;

        public void SwitchRoleAssignToEnd()
        {
            isRoleSetUpEnd = true;
        }

        private void resetRoleAssign()
        {
            isRoleSetUpEnd = false;
        }
    }
}
