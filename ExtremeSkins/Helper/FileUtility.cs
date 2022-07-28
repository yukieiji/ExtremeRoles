using System.Collections;
using System.IO;
using System.IO.Compression;

using UnityEngine;
using UnityEngine.Networking;

namespace ExtremeSkins.Helper
{
    public static class FileUtility
    {
        public static void DeleteDir(string dir)
        {
            if (!Directory.Exists(dir))
            {
                return;
            }

            //ディレクトリ以外の全ファイルを削除
            string[] filePaths = Directory.GetFiles(dir);
            foreach (string filePath in filePaths)
            {
                File.SetAttributes(filePath, FileAttributes.Normal);
                File.Delete(filePath);
            }

            //ディレクトリの中のディレクトリも再帰的に削除
            string[] directoryPaths = Directory.GetDirectories(dir);
            foreach (string directoryPath in directoryPaths)
            {
                DeleteDir(directoryPath);
            }

            //中が空になったらディレクトリ自身も削除
            Directory.Delete(dir, false);
        }

        public static IEnumerator DlToZip(
            string dlUrl, string zipPath)
        {
            UnityWebRequest www = UnityWebRequest.Get(dlUrl);
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                ExtremeSkinsPlugin.Logger.LogInfo(www.error);
                yield break;
            }
            File.WriteAllBytes(zipPath, www.downloadHandler.data);
        }

    }
}
