using System;
using System.IO;

using UnityEngine;


namespace ExtremeRoles.Helper
{

    public static class Logging
    {
        private const string LogFileDir = @"\BepInEx/LogOutput.log";
        private const string CopyedLogFileBase = "ExtremeRolesDumpedLog ";
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

        public static void Dump()
        {
            string logPath = 
                string.Concat(
                    Path.GetDirectoryName(Application.dataPath), LogFileDir);

            string copyPath = string.Concat(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.DesktopDirectory), @"\",
                $"{CopyedLogFileBase}{DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss")}.log");
            
            File.Copy(logPath, copyPath);

            System.Diagnostics.Process.Start(
                "EXPLORER.EXE", $@"/select, ""{copyPath}""");
        }
    }
}
