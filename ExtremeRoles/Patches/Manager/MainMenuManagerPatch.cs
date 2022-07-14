using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;

using Newtonsoft.Json.Linq;

using HarmonyLib;

using TMPro;
using Twitch;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Resources;
using ExtremeRoles.Performance;


namespace ExtremeRoles.Patches.Manager
{
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static class MainMenuManagerStartPatch
    {
        public static void Prefix(MainMenuManager __instance)
        {

            var template = GameObject.Find("ExitGameButton");
            if (template == null) { return; }

            var button = UnityEngine.Object.Instantiate(template, template.transform);
            button.name = "ExtremeRolesUpdateButton";
            UnityEngine.Object.Destroy(button.GetComponent<AspectPosition>());
            UnityEngine.Object.Destroy(button.GetComponent<ConditionalHide>());
            button.transform.localPosition = new Vector3(0.0f, 0.6f, 0.0f);

            PassiveButton passiveButton = button.GetComponent<PassiveButton>();
            passiveButton.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            passiveButton.OnClick.AddListener((UnityEngine.Events.UnityAction)onClick);

            var text = button.transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
            __instance.StartCoroutine(Effects.Lerp(0.1f, new Action<float>((p) => {
                text.SetText(Translation.GetString("UpdateButton"));
            })));
            if (Updater.InfoPopup == null)
            {
                TwitchManager man = FastDestroyableSingleton<TwitchManager>.Instance;
                Updater.InfoPopup = UnityEngine.Object.Instantiate<GenericPopup>(man.TwitchPopup);
                Updater.InfoPopup.TextAreaTMP.fontSize *= 0.7f;
                Updater.InfoPopup.TextAreaTMP.enableAutoSizing = false;
            }

            void onClick()
            {
                Updater.ExecuteCheckUpdate();
            }
        }

        public static void Postfix(MainMenuManager __instance)
        {
            FastDestroyableSingleton<ModManager>.Instance.ShowModStamp();

            var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
            if (amongUsLogo != null)
            {
                amongUsLogo.transform.localScale *= 0.9f;
                amongUsLogo.transform.position += Vector3.up * 0.25f;
            }

            var exrLogo = new GameObject("bannerLogoExtremeRoles");
            exrLogo.transform.position = Vector3.up;
            var renderer = exrLogo.AddComponent<SpriteRenderer>();
            renderer.sprite = Loader.CreateSpriteFromResources(
                Resources.Path.TitleBurner, 300f);

            var tmp = __instance.Announcement.transform.Find(
                "Title_Text").gameObject.GetComponent<TextMeshPro>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.transform.localPosition += Vector3.left * 0.2f;
            Module.Prefab.Text = UnityEngine.Object.Instantiate(tmp);
            UnityEngine.Object.Destroy(Module.Prefab.Text.GetComponent<
                TextTranslatorTMP>());
            Module.Prefab.Text.gameObject.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(Module.Prefab.Text);

            if (Module.Prefab.Prop == null)
            {
                TwitchManager man = DestroyableSingleton<TwitchManager>.Instance;
                Module.Prefab.Prop = UnityEngine.Object.Instantiate(man.TwitchPopup);
                UnityEngine.Object.DontDestroyOnLoad(
                    Module.Prefab.Prop);
                Module.Prefab.Prop.name = "propForInEx";
                Module.Prefab.Prop.gameObject.SetActive(false);
            }
            Compat.CompatModMenu.CreateMenuButton();
        }

        public static class Updater
        {
            public static GenericPopup InfoPopup;
            
            public const string CheckUrl = "https://api.github.com/repos/yukieiji/ExtremeRoles/releases/latest";

            public static string UpdateUri = null;
            public static bool HasUpdate = false;
            public static Task UpdateTask = null;

            public static void ExecuteCheckUpdate()
            {
                string info = Translation.GetString("chekUpdateWait");
                InfoPopup.Show(info); // Show originally
                
                CheckForUpdate().GetAwaiter().GetResult();
                if (HasUpdate)
                {
                    SetPopupText(Translation.GetString("updateNow"));
                    ClearOldVersions();

                    if (UpdateTask == null)
                    {
                        if (UpdateUri != null)
                        {
                            UpdateTask = DownloadUpdate();
                        }
                        else
                        {
                            info = Translation.GetString("updateManually");
                        }
                    }
                    else
                    {
                        info = Translation.GetString("updateInProgress");
                    }

                    InfoPopup.StartCoroutine(
                        Effects.Lerp(0.01f, new Action<float>((p) => { SetPopupText(info); })));

                }
                else
                {
                    SetPopupText(Translation.GetString("latestNow"));
                }

            }
            public static void ClearOldVersions()
            {
                try
                {
                    DirectoryInfo d = new DirectoryInfo(
                        System.IO.Path.GetDirectoryName(
                            Application.dataPath) + @"\BepInEx\plugins");
                    string[] files = d.GetFiles("*.old").Select(x => x.FullName).ToArray(); // Getting old versions
                    foreach (string f in files)
                    {
                        File.Delete(f);
                    }
                }
                catch (Exception e)
                {
                    Logging.Error("Exception occured when clearing old versions:\n" + e);
                }
            }


            public static async Task<bool> CheckForUpdate()
            {
                try
                {
                    HttpClient http = new HttpClient();
                    http.DefaultRequestHeaders.Add("User-Agent", "ExtremeRoles Updater");
                    var response = await http.GetAsync(
                        new Uri(CheckUrl),
                        HttpCompletionOption.ResponseContentRead);
                    if (response.StatusCode != HttpStatusCode.OK || response.Content == null)
                    {
                        Logging.Error("Server returned no data: " + response.StatusCode.ToString());
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
                        HasUpdate = true;
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
                                    browser_download_url.EndsWith("ExtremeRoles.dll"))
                                {
                                    UpdateUri = browser_download_url;
                                    return true;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logging.Error(ex.ToString());
                }
                return false;
            }

            public static async Task<bool> DownloadUpdate()
            {
                try
                {
                    HttpClient http = new HttpClient();
                    http.DefaultRequestHeaders.Add("User-Agent", "ExtremeRoles Updater");
                    var response = await http.GetAsync(
                        new Uri(UpdateUri),
                        HttpCompletionOption.ResponseContentRead);
                    if (response.StatusCode != HttpStatusCode.OK || response.Content == null)
                    {
                        Logging.Error("Server returned no data: " + response.StatusCode.ToString());
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
                    ShowPopup(Translation.GetString("updateRestart"));
                    return true;
                }
                catch (Exception ex)
                {
                    Logging.Error(ex.ToString());
                    ShowPopup(Translation.GetString("updateManually"));
                }
                return false;
            }

            public static void ShowPopup(string message)
            {
                SetPopupText(message);
                InfoPopup.gameObject.SetActive(true);
            }

            public static void SetPopupText(string message)
            {
                if (InfoPopup == null)
                {
                    return;
                }
                
                if (InfoPopup.TextAreaTMP != null)
                {
                    InfoPopup.TextAreaTMP.text = message;
                }
            }
        }

    }
}
