namespace ExtremeRoles.GameMode.Option.MapModule
{
    public sealed class VitalOption
    {
        public bool DisableVital     { get; set; } = false;
        public bool EnableVitalLimit { get; set; } = false;
        public float VitalLimitTime { get; set; } = 0.0f;
    }
}
