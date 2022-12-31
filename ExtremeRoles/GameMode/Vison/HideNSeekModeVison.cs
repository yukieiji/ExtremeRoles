namespace ExtremeRoles.GameMode.Vison
{
    public class HideNSeekModeVison : IVisonModifier
    {
        public VisonType Current => VisonType.None;

        public void SetModifier(VisonType newVison)
        {
        }
        public void ResetModifier()
        {
        }
        public bool IsModifierResetted() => true;

        public bool TryComputeVison(ShipStatus shipStatus, GameData.PlayerInfo playerInfo, out float vison)
        {
            vison = shipStatus.MaxLightRadius;
            return true;
        }
    }
}
