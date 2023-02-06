namespace ExtremeRoles.GameMode.Option.MapModule
{
    public sealed class SecurityOption
    {
        public bool DisableSecurity     { get; set; } = false;
        public bool EnableSecurityLimit { get; set; } = false;
        public float SecurityLimitTime  { get; set; } = float.MaxValue;
    }
}
