using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.IL2CPP.Utils;
using UnhollowerBaseLib.Attributes;
using UnityEngine;
using UnityEngine.Networking;
using System.Diagnostics;


namespace ExtremeRoles.Compat
{
    public class BepInExUpdater : MonoBehaviour
    {
        public static bool UpdateRequired => typeof(IL2CPPChainloader).Assembly.GetName().Version < Version.Parse(minimumBepInExVersion);

        private const string minimumBepInExVersion = "6.0.0.565";
        private const string bepInExDownloadURL = "https://builds.bepinex.dev/projects/bepinex_be/565/BepInEx_UnityIL2CPP_x86_265107c_6.0.0-be.565.zip";
        private const string exeFileName = "ExtremeBepInExInstaller.exe";

        public void Awake()
        {
            ExtremeRolesPlugin.Logger.LogInfo("BepInEx Update Required...");
            this.StartCoroutine(Excute());
        }

        [HideFromIl2Cpp]
        public IEnumerator Excute()
        {

            Task.Run(() => MessageBox(
                IntPtr.Zero,
                Helper.Translation.GetString("ReqBepInExUpdate"),
                "Extreme Roles", 0));

            UnityWebRequest www = UnityWebRequest.Get(bepInExDownloadURL);
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                ExtremeRolesPlugin.Logger.LogInfo(www.error);
                yield break;
            }

            string tmpFolder = Path.Combine(Paths.GameRootPath, "tmp");
            string zipPath = Path.Combine(tmpFolder, "BepInEx.zip");
            string extractPath = Path.Combine(tmpFolder, "BepInEx");

            if (Directory.Exists(tmpFolder))
            {
                Directory.Delete(tmpFolder, true);
            }
            Directory.CreateDirectory(tmpFolder);
            
            File.WriteAllBytes(zipPath, www.downloadHandler.data);
            
            ZipFile.ExtractToDirectory(zipPath, extractPath);

            Process.Start(
                Path.Combine(Paths.GameRootPath, "tmp", exeFileName),
                $"{Paths.GameRootPath} {extractPath} {SaveManager.LastLanguage}");

            Application.Quit();
        }

        [DllImport("user32.dll")]
        public static extern int MessageBox(IntPtr hWnd, String text, String caption, int options);

    }
}
