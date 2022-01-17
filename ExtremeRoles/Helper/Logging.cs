
namespace ExtremeRoles.Helper
{

    public static class Logging
    {
        public static void Debug(string msg)
        {
#if DEBUG
            if (ExtremeRolesPlugin.DebugMode.Value)
            {
                ExtremeRolesPlugin.Logger.LogInfo(msg);
            }
#endif
        }
        public static void Error(string msg)
        {

            ExtremeRolesPlugin.Logger.LogError(msg);
        }
    }
}
