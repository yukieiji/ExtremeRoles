namespace ExtremeRoles.GameMode.Vison
{
    public enum VisonType
    {
        None,
        LastWolfLightOff,
        WispLightOff,
    }

    public interface IVisonModifier
    {
        public VisonType Current { get; }

        public void SetModifier(VisonType newVison);

        public void ResetModifier();

        public bool IsModifierResetted();

        public bool TryComputeVison(ShipStatus shipStatus, GameData.PlayerInfo playerInfo, out float vison);
    }
}
