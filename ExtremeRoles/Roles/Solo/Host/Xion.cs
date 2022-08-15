using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Solo.Host
{
    public sealed partial class Xion : SingleRoleBase
    {
        public static byte PlayerId = byte.MaxValue;
        private float defaultCameraZoom;

        public Xion(byte xionPlayerId) : base(
            ExtremeRoleId.Xion,
            ExtremeRoleType.Null,
            ExtremeRoleId.Xion.ToString(),
            ColorPalette.YokoShion,
            false, false, false, true,
            true, true, true, true, true)
        {
            this.MoveSpeed = PlayerControl.GameOptions.PlayerSpeedMod;
            this.defaultCameraZoom = UnityEngine.Camera.main.orthographicSize;
            this.dummyDeadBody.Clear();
            PlayerId = xionPlayerId;
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        { }
    }
}
