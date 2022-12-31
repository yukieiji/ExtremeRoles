namespace ExtremeRoles.GameMode.Option.MapModuleOption
{
    public class SecurityOption
    {
        public bool DisableSecurity     { get; set; } = false;
        public bool EnableSecurityLimit { get; set; } = false;
        public float SecurityLimitTime  { get; set; } = float.MaxValue;
    }
}
