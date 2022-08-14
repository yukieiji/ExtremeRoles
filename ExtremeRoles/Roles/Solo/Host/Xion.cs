using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Solo.Host
{
    public sealed partial class Xion : SingleRoleBase
    {
        public byte PlayerId => this.playerId;

        private byte playerId;
        private float defaultCameraZoom;

        public Xion(byte xionPlayerId) : base(
            ExtremeRoleId.Xion,
            ExtremeRoleType.Null,
            ExtremeRoleId.Xion.ToString(),
            ColorPalette.YokoShion,
            false, false, false, true,
            false, false, false, false, false)
        {
            this.MoveSpeed = PlayerControl.GameOptions.PlayerSpeedMod;
            this.defaultCameraZoom = UnityEngine.Camera.main.orthographicSize;
            this.playerId = xionPlayerId;

            Init(xionPlayerId);
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        { }
    }
}
