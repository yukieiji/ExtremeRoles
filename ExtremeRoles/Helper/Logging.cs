using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

using UnityEngine;


namespace ExtremeRoles.Helper;

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
        string logBackupPath = getLogBackupPath();

        if (Directory.Exists(logBackupPath))
        {
            string[] logFile = Directory
                //logのバックアップディレクトリ内の全ファイルを取得
                .GetFiles(logBackupPath)
                //.logだけサーチ
                .Where(filePath => Path.GetExtension(filePath) == ".log")
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

        string movedLog = string.Concat(
            logBackupPath, @$"\ExtremeRolesBackupLog {getTimeStmp()}.log");

        File.Copy(getLogPath(), movedLog);
        replaceLogCustomServerIp(movedLog);
    }

    public static void Dump()
    {

        string dumpFilePath = string.Concat(
            Environment.GetFolderPath(
                Environment.SpecialFolder.DesktopDirectory), @"\",
            $"ExtremeRolesDumpedLogs {getTimeStmp()}.zip");

        string tmpLogFile = string.Concat(
            Path.GetDirectoryName(Application.dataPath), @"\BepInEx/tmp.log");

        File.Copy(getLogPath(), tmpLogFile, true);
        replaceLogCustomServerIp(tmpLogFile);

        using (var dumpedZipFile = ZipFile.Open(
            dumpFilePath, ZipArchiveMode.Update))
        {
            dumpedZipFile.CreateEntryFromFile(
                tmpLogFile, $"ExtremeRolesDumpedLog {getTimeStmp()}.log");

            string logBackupPath = getLogBackupPath();

            if (Directory.Exists(logBackupPath))
            {
                dumpedZipFile.CreateEntry("BackupLog/");
                
                string[] logFile = Directory
                    //logのバックアップディレクトリ内の全ファイルを取得
                    .GetFiles(logBackupPath)
                    //.logだけサーチ
                    .Where(filePath => Path.GetExtension(filePath) == ".log")
                    .ToArray();

                foreach (string logPath in logFile)
                {
                    dumpedZipFile.CreateEntryFromFile(
                        logPath, $"BackupLog/{Path.GetFileName(logPath)}");
                }
            }
        }

        File.Delete(tmpLogFile);

        System.Diagnostics.Process.Start(
            "EXPLORER.EXE", $@"/select, ""{dumpFilePath}""");
    }

    public static void ResetCkpt()
    {
        ckpt = 0;
    }

    private static void replaceLogCustomServerIp(string targetFileName)
    {
        string losStr;
        using (StreamReader prevLog = new StreamReader(targetFileName))
        {
            losStr = prevLog.ReadToEnd();
        }

        losStr = losStr.Replace(
            Module.CustomOption.ClientOption.Instance.Ip.Value,
            "***.***.***.***");

        using StreamWriter newLog = new StreamWriter(targetFileName, true, Encoding.UTF8);
        newLog.Write(losStr);
    }

    private static string getTimeStmp() => DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss");

    private static string getLogPath() => string.Concat(
        Path.GetDirectoryName(Application.dataPath), @"\BepInEx/LogOutput.log");

    private static string getLogBackupPath() => string.Concat(
        Path.GetDirectoryName(Application.dataPath), @"\BepInEx/BackupLog");
}
