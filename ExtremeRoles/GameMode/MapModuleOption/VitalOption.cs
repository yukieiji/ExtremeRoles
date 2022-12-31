namespace ExtremeRoles.GameMode.MapModuleOption
{
    public class VitalOption
    {
        public bool IsRemoveSecurity    { get; private set; } = false;
        public bool EnableSecurityLimit { get; private set; } = false;
        public float SecurityLimitTime  { get; private set; } = 0.0f;
    }
}
