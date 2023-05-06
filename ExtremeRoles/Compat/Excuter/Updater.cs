using System;
using System.IO;
using System.Net;
using System.Net.Http;

using Newtonsoft.Json.Linq;

using System.Threading.Tasks;

using ExtremeRoles.Helper;

namespace ExtremeRoles.Compat.Excuter;

internal sealed class Updater : ButtonExcuterBase
{

    private struct ReleaseData
    {
        public JObject Request;
        private string tag;

        public ReleaseData(JObject data)
        {
            Request = data;
            tag = data["tag_name"]?.ToString().TrimStart('v');
        }
        public bool IsNewer(SemanticVersioning.Version version)
        {
            if (!SemanticVersioning.Version.TryParse(tag, out var myVersion)) { return false; }
            
            return myVersion.BaseVersion() > version.BaseVersion();
        }
    }

    private const string agentName = "ExtremeRoles CompatModUpdater";
    private Task updateTask = null;
    private SemanticVersioning.Version installVersion;
    private string dllName;
    private string repoUrl;

    internal Updater(
        CompatModType mod, string dllName, string repoUrl) : base()
    {
        this.dllName = $"{dllName}.dll";
        this.repoUrl = repoUrl;
        this.updateTask = null;
        this.installVersion = ExtremeRolesPlugin.Compat.LoadedMod[mod].Version;
    }

    public override void Excute()
    {
        
        if (!File.Exists(Path.Combine(this.modFolderPath, this.dllName)))
        {
            Popup.Show(Translation.GetString("alreadyUninstallAfterInstall"));
            return;
        }

        string info = Translation.GetString("checkUpdateNow");
        Popup.Show(info);

        ReleaseData? repoData = getGithubUpdate().GetAwaiter().GetResult();

        if (repoData.HasValue)
        {

            ReleaseData release = repoData.Value;

            if (release.IsNewer(this.installVersion))
            {
                info = Translation.GetString("updateNow");

                if (updateTask == null)
                {
                    info = Translation.GetString("updateInProgress");
                    updateTask = downloadAndUpdate(release);
                }
            }
            else
            {
                info = Translation.GetString("latestNow");
            }

            this.Popup.StartCoroutine(
                Effects.Lerp(0.01f, new Action<float>((p) => { SetPopupText(info); })));

        }
        else
        {
            SetPopupText(Translation.GetString("updateManual"));
        }
    }

    private async Task<ReleaseData?> getGithubUpdate()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", agentName);

        var req = await client.GetAsync(
            new Uri(this.repoUrl),
            HttpCompletionOption.ResponseContentRead);
        if (req.StatusCode != HttpStatusCode.OK || req.Content == null)
        {
            Logging.Error($"Server returned no data: {req.StatusCode}");
            return null;
        }

        string dataString = await req.Content.ReadAsStringAsync();
        JObject data = JObject.Parse(dataString);
        return new ReleaseData(data);
    }

    private async Task<bool> downloadAndUpdate(ReleaseData data)
    {
        HttpClient http = new HttpClient();
        http.DefaultRequestHeaders.Add("User-Agent", agentName);
        var response = await http.GetAsync(
            new Uri(this.repoUrl),
            HttpCompletionOption.ResponseContentRead);
        if (response.StatusCode != HttpStatusCode.OK || response.Content == null)
        {
            Logging.Error($"Server returned no data: {response.StatusCode}");
            return false;
        }

        JToken assets = data.Request["assets"];
        string downloadURI = "";

        for (JToken current = assets.First; current != null; current = current.Next)
        {
            string browser_download_url = current["browser_download_url"]?.ToString();
            if (browser_download_url == null ||
                current["content_type"] == null ||
                current["content_type"].ToString().Equals("application/x-zip-compressed") ||
                !browser_download_url.EndsWith(this.dllName))
            {
                continue;
            }

            downloadURI = browser_download_url;
            break;
        }

        if (downloadURI.Length == 0) { return false; }

        var res = await http.GetAsync(
            downloadURI, HttpCompletionOption.ResponseContentRead);
        string filePath = Path.Combine(this.modFolderPath, this.dllName);

        string oldMod = $"{filePath}.old";
        if (File.Exists(oldMod))
        {
            File.Delete(oldMod);
        }
        if (File.Exists(filePath))
        {
            File.Move(filePath, oldMod);
        }
        await using var responseStream = await res.Content.ReadAsStreamAsync();
        await using var fileStream = File.Create(filePath);
        await responseStream.CopyToAsync(fileStream);

        ShowPopup(Translation.GetString("updateRestart"));
        
        return true;
    }

}
