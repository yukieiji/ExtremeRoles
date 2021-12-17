
namespace ExtremeRoles.Helper
{

    public class Logging
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
