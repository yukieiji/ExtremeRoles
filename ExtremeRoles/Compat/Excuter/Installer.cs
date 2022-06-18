using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;

using Newtonsoft.Json.Linq;

using System.Threading.Tasks;

using ExtremeRoles.Helper;

namespace ExtremeRoles.Compat.Excuter
{
    internal class Installer : ButtonExcuterBase
    {

        private struct RepoData
        {
            public JObject Request;

            public RepoData(JObject data)
            {
                Request = data;
            }
        }


        private const string agentName = "ExtremeRoles CompateModInstaller";
        private Task installTask = null;
        private string dllName;
        private string repoUrl;

        internal Installer(string dllName, string repoUrl) : base()
        {
            this.dllName = $"{dllName}.dll";
            this.repoUrl = repoUrl;
            this.installTask = null;
        }

        public override void Excute()
        {
            if (File.Exists(Path.Combine(this.modFolderPath, this.dllName)))
            {
                Popup.Show(Translation.GetString("alreadyInstall"));
                return;
            }

            string info = Translation.GetString("checkInstallNow");
            Popup.Show(info);

            RepoData? repoData = getGithubUpdate().GetAwaiter().GetResult();

            if (repoData.HasValue)
            {
                info = Translation.GetString("installNow");

                if (installTask == null)
                {
                    installTask = downloadAndInstall(repoData.Value);
                }
                else
                {
                    info = Translation.GetString("installInProgress");
                }

                this.Popup.StartCoroutine(
                    Effects.Lerp(0.01f, new Action<float>((p) => { SetPopupText(info); })));

            }
            else
            {
                SetPopupText(Translation.GetString("installManual"));
            }
        }

        private async Task<RepoData?> getGithubUpdate()
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

            var dataString = await req.Content.ReadAsStringAsync();
            JObject data = JObject.Parse(dataString);
            return new RepoData(data);
        }

        private async Task<bool> downloadAndInstall(RepoData data)
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
                if (browser_download_url != null && 
                    current["content_type"] != null)
                {
                    if (current["content_type"].ToString().Equals("application/x-msdownload") &&
                        browser_download_url.EndsWith(".dll"))
                    {
                        downloadURI = browser_download_url;
                        break;
                    }
                }
            }

            if (downloadURI.Length == 0) { return false; }

            var res = await http.GetAsync(
                downloadURI, HttpCompletionOption.ResponseContentRead);
            string filePath = Path.Combine(this.modFolderPath, this.dllName);
            await using var responseStream = await res.Content.ReadAsStreamAsync();
            await using var fileStream = File.Create(filePath);
            await responseStream.CopyToAsync(fileStream);

            ShowPopup(Translation.GetString("installRestart"));
            
            return true;
        }

    }
}
