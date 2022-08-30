namespace ExtremeRoles.Module.ExtremeShipStatus
{
    public sealed partial class ExtremeShipStatus
    {
        public enum ForceVisonType
        {
            None,
            LastWolfLightOff
        }

        public ForceVisonType CurVison => this.modVison;
        private ForceVisonType modVison;

        public void SetVison(ForceVisonType newVison)
        {
            this.modVison = newVison;
        }

        public void ResetVison()
        {
            this.modVison = ForceVisonType.None;
        }
        public bool IsCustomVison() => this.modVison != ForceVisonType.None;
    }
}
