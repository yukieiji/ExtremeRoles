namespace ExtremeRoles.Roles.Solo.Host
{
    public sealed partial class Xion
    {
        public static void Purge()
        {
            PlayerId = byte.MaxValue;
            voted = false;
        }

        protected override void CommonInit()
        {
            return;
        }

        protected override void RoleSpecificInit()
        {
            return;
        }
    }
}
