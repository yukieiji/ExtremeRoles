namespace ExtremeRoles.GameMode.MapModuleOption
{
    public enum AirShipAdminMode
    {
        ModeBoth,
        ModeCockpitOnly,
        ModeArchiveOnly
    }

    public class AdminOption
    {
        public bool IsDisableAdmin            { get; private set; } = false;
        public AirShipAdminMode AirShipEnable { get; private set; } = AirShipAdminMode.ModeBoth;
        public bool EnableAdminLimit          { get; private set; } = false;
        public float AdminLimitTime           { get; private set; } = float.MaxValue;
    }
}
