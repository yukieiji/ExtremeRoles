using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.JsonData;

namespace ExtremeRoles.Compat.Operator;

#nullable enable

internal sealed class Installer : OperatorBase
{
	private const string agentName = "ExtremeRoles CompatModInstaller";

	private Task? installTask = null;
	private string dllName;
	private string repoUrl;
	private bool isRequireReactor = false;

	private HttpClient client;
	private GenericPopup? popup;

	internal Installer(CompatModInfo modInfo) : base()
	{
		this.dllName = $"{modInfo.Name}.dll";
		this.repoUrl = modInfo.RepoUrl;
		this.installTask = null;
		this.isRequireReactor = modInfo.IsRequireReactor;

		this.client = new HttpClient();
		this.client.DefaultRequestHeaders.Add("User-Agent", agentName);
	}

	public override void Excute()
	{
		if (File.Exists(Path.Combine(this.ModFolderPath, this.dllName)))
		{
			Popup.Show(OldTranslation.GetString("alreadyInstall"));
			return;
		}

		if (this.isRequireReactor)
		{
			if (this.popup == null)
			{
				this.popup = Module.Prefab.CreateConfirmMenu(
					excuteInstall);
			}
			this.popup.Show(
				OldTranslation.GetString("isReactorInstall"));
		}
		else
		{
			excuteInstall();
		}
	}

	private void excuteInstall()
	{
		string info = OldTranslation.GetString("checkInstallNow");
		Popup.Show(info);

		List<CompatModRepoData> repoData = getGithubUpdate().GetAwaiter().GetResult();

		if (repoData.Count == 0 ||
			repoData.Count == 1 && this.isRequireReactor)
		{
			SetPopupText(OldTranslation.GetString("installManual"));
		}
		else
		{
			info = OldTranslation.GetString("installNow");

			if (installTask == null)
			{
				info = OldTranslation.GetString("installInProgress");
				installTask = downloadAndInstall(repoData);
			}

			this.Popup.StartCoroutine(
				Effects.Lerp(0.01f, new Action<float>((p) => { SetPopupText(info); })));
		}
	}


	private async Task<List<CompatModRepoData>> getGithubUpdate()
	{
		List<CompatModRepoData> result = new List<CompatModRepoData>();
		if (this.isRequireReactor)
		{
			var reactorData = await JsonParser.GetRestApiAsync<GitHubReleaseData>(this.client, ReactorURL);
			result.Add(new CompatModRepoData(reactorData, ReactorDll));
		}

		var modData = await JsonParser.GetRestApiAsync<GitHubReleaseData>(this.client, this.repoUrl);
		result.Add(new CompatModRepoData(modData, this.dllName));
		return result;
	}

	private async Task<bool> downloadAndInstall(List<CompatModRepoData> data)
	{

		foreach (var repoData in data)
		{
			string downloadUri = repoData.GetDownloadUrl();

			if (string.IsNullOrEmpty(downloadUri)) { return false; }

			var res = await this.client.GetAsync(
				downloadUri, HttpCompletionOption.ResponseContentRead);

			if (res.StatusCode != HttpStatusCode.OK || res.Content == null)
			{
				Logging.Error($"Server returned no data: {res.StatusCode}");
				return false;
			}

			string filePath = Path.Combine(this.ModFolderPath, repoData.DllName);
			await using var responseStream = await res.Content.ReadAsStreamAsync();
			await using var fileStream = File.Create(filePath);
			await responseStream.CopyToAsync(fileStream);
		}

		ShowPopup(OldTranslation.GetString("installRestart"));

		return true;
	}
}
