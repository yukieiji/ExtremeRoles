namespace ExtremeRoles.GameMode.MapModuleOption
{
    public class SecurityOption
    {
        public bool IsDisableSecurity   { get; private set; } = false;
        public bool EnableSecurityLimit { get; private set; } = false;
        public float SecurityLimitTime  { get; private set; } = float.MaxValue;
    }
}
