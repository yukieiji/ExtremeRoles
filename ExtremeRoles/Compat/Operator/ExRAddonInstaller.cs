using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.JsonData;
using Newtonsoft.Json.Linq;

namespace ExtremeRoles.Compat.Operator;

#nullable enable

internal sealed class ExRAddonInstaller : OperatorBase
{
	private string addonDll;
	private string url = "https://api.github.com/repos/yukieiji/ExtremeRoles/releases/latest";
	private const string agentName = "ExtremeRoles CompatModInstaller";

	private HttpClient client;
	private Task? installTask = null;

	internal ExRAddonInstaller(CompatModType addonType) : base()
	{
		this.addonDll = $"{addonType}.dll";

		this.client = new HttpClient();
		this.client.DefaultRequestHeaders.Add("User-Agent", agentName);
	}

	public override void Excute()
	{
		string info = OldTranslation.GetString("checkInstallNow");
		Popup.Show(info);


		var exrRepoData = JsonParser.GetRestApiAsync<GitHubReleaseData>(
			this.client, url).GetAwaiter().GetResult();

		info = OldTranslation.GetString("installNow");

		if (installTask == null)
		{
			info = OldTranslation.GetString("installInProgress");
			installTask = downloadAndInstall(exrRepoData);
		}

		this.Popup.StartCoroutine(
			Effects.Lerp(0.01f, new Action<float>((p) => { SetPopupText(info); })));
	}

	private async Task<bool> downloadAndInstall(GitHubReleaseData data)
	{

		string downloadUri = "";

		foreach (var asset in data.assets)
		{
			string? browser_download_url = asset.browser_download_url;
			if (string.IsNullOrEmpty(browser_download_url) ||
				asset.content_type.Equals("application/x-zip-compressed") ||
				!browser_download_url.EndsWith(this.addonDll))
			{
				continue;
			}

			downloadUri = browser_download_url;
		}

		if (string.IsNullOrEmpty(downloadUri)) { return false; }

		var res = await this.client.GetAsync(
			downloadUri, HttpCompletionOption.ResponseContentRead);
		if (res.StatusCode != HttpStatusCode.OK || res.Content == null)
		{
			Logging.Error($"Server returned no data: {res.StatusCode}");
			return false;
		}

		string filePath = Path.Combine(this.ModFolderPath, this.addonDll);

		await using var responseStream = await res.Content.ReadAsStreamAsync();
		await using var fileStream = File.Create(filePath);
		await responseStream.CopyToAsync(fileStream);

		ShowPopup(OldTranslation.GetString("installRestart"));

		return true;
	}
}
