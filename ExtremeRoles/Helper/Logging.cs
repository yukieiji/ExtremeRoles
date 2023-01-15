using System;
using System.IO;
using System.Linq;

using UnityEngine;


namespace ExtremeRoles.Helper
{

    public static class Logging
    {
        private static int ckpt = 0;
        
        public static void CheckPointDebugLog()
        {
            Debug($"ckpt:{ckpt}");
            ++ckpt;
        }

        public static void CheckPointReleaseLog()
        {
            ExtremeRolesPlugin.Logger.LogInfo($"ckpt:{ckpt}");
            ++ckpt;
        }

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

        public static void BackupCurrentLog()
        {
            string logBackupPath = string.Concat(
                Path.GetDirectoryName(Application.dataPath), @"\BepInEx/BackupLog");

            if (Directory.Exists(logBackupPath))
            {
                string[] logFile = Directory
                    //logのバックアップディレクトリ内の全ファイルを取得
                    .GetFiles(logBackupPath)
                    //.logだけサーチ
                    .Where(filePath => Path.GetFileName(filePath) == ".log")
                    //日付順に降順でソート
                    .OrderBy(filePath => File.GetLastWriteTime(filePath).Date)
                    //同じ日付内で時刻順に降順でソート
                    .ThenBy(filePath => File.GetLastWriteTime(filePath).TimeOfDay)
                    .ToArray();

                if (logFile.Length >= 10)
                {
                    File.Delete(logFile[0]);
                }
            }
            else
            {
                Directory.CreateDirectory(logBackupPath);
            }

            File.Copy(getLogPath(), string.Concat(
                logBackupPath, @$"\ExtremeRolesBackupLog {getTimeStmp()}.log"));
        }

        public static void Dump()
        {
            string copyPath = string.Concat(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.DesktopDirectory), @"\",
                $"ExtremeRolesDumpedLog {getTimeStmp()}.log");
            
            File.Copy(getLogPath(), copyPath);

            System.Diagnostics.Process.Start(
                "EXPLORER.EXE", $@"/select, ""{copyPath}""");
        }

        public static void ResetCkpt()
        {
            ckpt = 0;
        }

        private static string getTimeStmp() => DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss");

        private static string getLogPath() => string.Concat(
            Path.GetDirectoryName(Application.dataPath), @"\BepInEx/LogOutput.log");
    }
}
