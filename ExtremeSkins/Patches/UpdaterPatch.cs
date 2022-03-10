using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;

using Newtonsoft.Json.Linq;

using HarmonyLib;

using ExtremeRoles.Patches.Manager;

namespace ExtremeSkins.Patches
{

    [HarmonyPatch(
        typeof(MainMenuManagerStartPatch.Updater),
        nameof(MainMenuManagerStartPatch.Updater.ExecuteCheckUpdate))]
    public class UpdaterExecuteCheckUpdatePatch
    {
        private static bool errorFlag = false;
        private static bool skinHasUpdate = false;
        private static string skinUpdateUri = null;
        private static Task skinUpdateTask = null;

        public static bool Prefix()
        {
            errorFlag = false;
            string info = ExtremeRoles.Helper.Translation.GetString("chekUpdateWait");
            MainMenuManagerStartPatch.Updater.InfoPopup.Show(info); // Show originally

            MainMenuManagerStartPatch.Updater.CheckForUpdate().GetAwaiter().GetResult();
            checkSkinUpdate().GetAwaiter().GetResult();

            if (MainMenuManagerStartPatch.Updater.HasUpdate || skinHasUpdate)
            {
                MainMenuManagerStartPatch.Updater.SetPopupText(
                    ExtremeRoles.Helper.Translation.GetString("updateNow"));
                MainMenuManagerStartPatch.Updater.ClearOldVersions();

                if (skinUpdateTask == null)
                {
                    if (skinUpdateUri != null)
                    {
                        skinUpdateTask = skinDownloadUpdate();
                    }
                    else
                    {
                        info = ExtremeRoles.Helper.Translation.GetString(
                            "updateManually");
                    }
                }
                else
                {
                    info = ExtremeRoles.Helper.Translation.GetString("updateInProgress");
                }

                // Extreme Rolesのアップデート
                if (MainMenuManagerStartPatch.Updater.UpdateTask == null)
                {
                    if (MainMenuManagerStartPatch.Updater.UpdateUri != null)
                    {
                        MainMenuManagerStartPatch.Updater.UpdateTask = MainMenuManagerStartPatch.Updater.DownloadUpdate();
                    }
                    else
                    {
                        info = ExtremeRoles.Helper.Translation.GetString(
                            "updateManually");
                    }
                }
                else
                {
                    info = ExtremeRoles.Helper.Translation.GetString("updateInProgress");
                }
                if (errorFlag)
                {
                    info = ExtremeRoles.Helper.Translation.GetString("updateManually");
                }

                MainMenuManagerStartPatch.Updater.InfoPopup.StartCoroutine(
                    Effects.Lerp(0.01f, new Action<float>(
                        (p) => { MainMenuManagerStartPatch.Updater.SetPopupText(info); })));

            }
            else
            {
                MainMenuManagerStartPatch.Updater.SetPopupText(
                    ExtremeRoles.Helper.Translation.GetString("latestNow"));
            }
            return false;
        }

        private static async Task<bool> checkSkinUpdate()
        {
            try
            {
                HttpClient http = new HttpClient();
                http.DefaultRequestHeaders.Add("User-Agent", "ExtremeSkins Updater");
                var response = await http.GetAsync(
                    new Uri(MainMenuManagerStartPatch.Updater.CheckUrl),
                    HttpCompletionOption.ResponseContentRead);
                if (response.StatusCode != HttpStatusCode.OK || response.Content == null)
                {
                    ExtremeSkinsPlugin.Logger.LogError("Server returned no data: " + response.StatusCode.ToString());
                    return false;
                }
                string json = await response.Content.ReadAsStringAsync();
                JObject data = JObject.Parse(json);

                string tagname = data["tag_name"]?.ToString();
                if (tagname == null)
                {
                    return false; // Something went wrong
                }
                // check version
                Version ver = Version.Parse(tagname.Replace("v", ""));
                int diff = Assembly.GetExecutingAssembly().GetName().Version.CompareTo(ver);
                if (diff < 0)
                { // Update required
                    skinHasUpdate = true;
                    JToken assets = data["assets"];
                    if (!assets.HasValues)
                    {
                        return false;
                    }
                    for (JToken current = assets.First; current != null; current = current.Next)
                    {
                        string browser_download_url = current["browser_download_url"]?.ToString();
                        if (browser_download_url != null && current["content_type"] != null)
                        {
                            if (current["content_type"].ToString().Equals("application/x-msdownload") &&
                                browser_download_url.EndsWith(".dll"))
                            {
                                skinUpdateUri = browser_download_url;
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExtremeSkinsPlugin.Logger.LogError(ex.ToString());
            }
            return false;
        }

        public static async Task<bool> skinDownloadUpdate()
        {
            try
            {
                HttpClient http = new HttpClient();
                http.DefaultRequestHeaders.Add("User-Agent", "ExtremeSkins Updater");
                var response = await http.GetAsync(
                    new Uri(skinUpdateUri),
                    HttpCompletionOption.ResponseContentRead);
                if (response.StatusCode != HttpStatusCode.OK || response.Content == null)
                {
                    ExtremeSkinsPlugin.Logger.LogError("Server returned no data: " + response.StatusCode.ToString());
                    return false;
                }
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string fullname = Uri.UnescapeDataString(uri.Path);
                if (File.Exists(fullname + ".old")) // Clear old file in case it wasnt;
                    File.Delete(fullname + ".old");

                File.Move(fullname, fullname + ".old"); // rename current executable to old

                using (var responseStream = await response.Content.ReadAsStreamAsync())
                {
                    using (var fileStream = File.Create(fullname))
                    { // probably want to have proper name here
                        responseStream.CopyTo(fileStream);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                ExtremeSkinsPlugin.Logger.LogError(ex.ToString());
                errorFlag = true;
            }
            return false;
        }

    }
}
