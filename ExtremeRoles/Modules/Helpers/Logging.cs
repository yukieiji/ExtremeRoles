using System;
using System.Collections.Generic;
using System.Text;

namespace ExtremeRoles.Modules.Helpers
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
