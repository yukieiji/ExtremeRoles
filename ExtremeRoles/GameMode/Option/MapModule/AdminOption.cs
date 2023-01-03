namespace ExtremeRoles.GameMode.Option.MapModule
{
    public enum AirShipAdminMode
    {
        ModeBoth,
        ModeCockpitOnly,
        ModeArchiveOnly
    }

    public class AdminOption
    {
        public bool DisableAdmin            { get; set; } = false;
        public AirShipAdminMode AirShipEnable { get; set; } = AirShipAdminMode.ModeBoth;
        public bool EnableAdminLimit          { get; set; } = false;
        public float AdminLimitTime           { get; set; } = float.MaxValue;
    }
}
