using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using BepInEx;
using BepInEx.Unity.IL2CPP.Utils;

using Il2CppInterop.Runtime.Attributes;

using UnityEngine;
using UnityEngine.Networking;

using AmongUs.Data;

using SemanticVersion = SemanticVersioning.Version;


namespace ExtremeRoles.Compat
{
    public sealed class BepInExUpdater : MonoBehaviour
    {
        private const string minimumBepInExVersion = "6.0.0-be.667";
        private const string bepInExDownloadURL = "https://builds.bepinex.dev/projects/bepinex_be/667/BepInEx-Unity.IL2CPP-win-x86-6.0.0-be.667%2B6b500b3.zip";
        private const string exeFileName = "ExtremeBepInExInstaller.exe";

        public static bool IsUpdateRquire()
        {
            string rawBepInExVersionStr = MetadataHelper.GetAttributes<
                AssemblyInformationalVersionAttribute>(typeof(Paths).Assembly)[0].InformationalVersion;
            int suffixIndex = rawBepInExVersionStr.IndexOf('+');
            return 
                SemanticVersion.Parse(rawBepInExVersionStr.Substring(0, suffixIndex)) <
                SemanticVersion.Parse(minimumBepInExVersion);
        }

        public void Awake()
        {
            ExtremeRolesPlugin.Logger.LogInfo("BepInEx Update Required...");
            this.StartCoroutine(Excute());
        }

        [HideFromIl2Cpp]
        public IEnumerator Excute()
        {
            string showStr = Helper.Translation.GetString("ReqBepInExUpdate");

            Task.Run(() => Module.DllApi.MessageBox(
                IntPtr.Zero,
                showStr, "Extreme Roles", 0));

            string tmpFolder = Path.Combine(Paths.GameRootPath, "tmp");
            string zipPath = Path.Combine(tmpFolder, "BepInEx.zip");
            string extractPath = Path.Combine(tmpFolder, "BepInEx");

            if (Directory.Exists(tmpFolder))
            {
                Directory.Delete(tmpFolder, true);
            }
            Directory.CreateDirectory(tmpFolder);

            yield return dlBepInExZip(zipPath);

            ZipFile.ExtractToDirectory(zipPath, extractPath);

            extractExtremeBepInExInstaller(tmpFolder);

            Process.Start(
                Path.Combine(Paths.GameRootPath, "tmp", exeFileName),
                $"{Paths.GameRootPath} {extractPath} {(uint)DataManager.Settings.Language.CurrentLanguage}");

            Application.Quit();
        }

        private static IEnumerator dlBepInExZip(string saveZipPath)
        {

            UnityWebRequest www = UnityWebRequest.Get(bepInExDownloadURL);
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                ExtremeRolesPlugin.Logger.LogInfo(www.error);
                yield break;
            }

            File.WriteAllBytes(saveZipPath, www.downloadHandler.data);
        }

        private static void extractExtremeBepInExInstaller(string extractTmpFolder)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string exePath = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(exeFileName));

            using (var resource = asm.GetManifestResourceStream(exePath))
            {
                using (var file = new FileStream(
                    Path.Combine(extractTmpFolder, exeFileName),
                    FileMode.OpenOrCreate, FileAccess.Write))
                {
                    resource!.CopyTo(file);
                }
            }
        }

        // ここでBepInExのcfgをdlしてコピーする
        private static void replaceConfigValue(string configPath)
        {
            string configStr;
            using (StreamReader prevLog = new StreamReader(configPath))
            {
                configStr = prevLog.ReadToEnd();
            }

            configStr = configStr.Replace(
                "[Logging.Console]\r\n\r\n## Enables showing a console for log output.\r\n# Setting type: Boolean\r\n# Default value: true\r\nEnabled = true",
                "[Logging.Console]\r\n\r\n## Enables showing a console for log output.\r\n# Setting type: Boolean\r\n# Default value: true\r\nEnabled = false");

            using StreamWriter newLog = new StreamWriter(configPath, true, Encoding.UTF8);
            newLog.Write(configStr);
        }
    }
}
