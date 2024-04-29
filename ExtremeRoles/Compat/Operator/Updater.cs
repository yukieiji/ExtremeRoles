using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;

using BepInEx.Unity.IL2CPP;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.JsonData;

namespace ExtremeRoles.Compat.Operator;

#nullable enable

internal sealed class Updater : OperatorBase
{
	private const string agentName = "ExtremeRoles CompatModUpdater";
	private const string reactorGuid = "Reactor";

	private Task? updateTask = null;

	private string dllName;
	private string repoUrl;
	private string guid;
	private bool isRequireReactor;

	private HttpClient client;

	internal Updater(CompatModInfo modInfo) : base()
	{
		this.dllName = $"{modInfo.Name}.dll";
		this.repoUrl = modInfo.RepoUrl;
		this.guid = modInfo.Guid;
		this.isRequireReactor = modInfo.IsRequireReactor;

		this.client = new HttpClient();
		this.client.DefaultRequestHeaders.Add("User-Agent", agentName);
	}

	public override void Excute()
	{

		if (!File.Exists(Path.Combine(this.ModFolderPath, this.dllName)))
		{
			Popup.Show(Translation.GetString("alreadyUninstallAfterInstall"));
			return;
		}

		string info = Translation.GetString("checkUpdateNow");
		Popup.Show(info);

		Dictionary<string, CompatModRepoData> repoData = getGithubUpdate().GetAwaiter().GetResult();

		if (repoData.Count == 0 ||
			repoData.Count == 1 && this.isRequireReactor)
		{
			SetPopupText(Translation.GetString("updateManual"));
		}
		else
		{
			var requireUpdate = repoData.Where(
				x =>
				{
					if (IL2CPPChainloader.Instance.Plugins.TryGetValue(x.Key, out var plugin) &&
						plugin != null)
					{
						return x.Value.IsNewer(plugin.Metadata.Version);
					}
					return false;
				}).Select(x => x.Value);

			if (requireUpdate.Any())
			{
				info = Translation.GetString("updateNow");

				if (updateTask == null)
				{
					info = Translation.GetString("updateInProgress");
					updateTask = downloadAndUpdate(requireUpdate);
				}
			}
			else
			{
				info = Translation.GetString("latestNow");
			}

			this.Popup.StartCoroutine(
				Effects.Lerp(0.01f, new Action<float>((p) => { SetPopupText(info); })));
		}
	}

	private async Task<Dictionary<string, CompatModRepoData>> getGithubUpdate()
	{
		Dictionary<string, CompatModRepoData> result = new Dictionary<string, CompatModRepoData>();
		if (this.isRequireReactor)
		{
			var reactorData = await JsonParser.GetRestApiAsync<GitHubReleaseData>(this.client, ReactorURL);
			result.Add(reactorGuid, new CompatModRepoData(reactorData, ReactorDll));
		}

		var modData = await JsonParser.GetRestApiAsync<GitHubReleaseData>(this.client, this.repoUrl);
		result.Add(this.guid, new CompatModRepoData(modData, this.dllName));
		return result;
	}

	private async Task<bool> downloadAndUpdate(IEnumerable<CompatModRepoData> data)
	{
		foreach (CompatModRepoData repoData in data)
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
		}

		ShowPopup(Translation.GetString("updateRestart"));

		return true;
	}

}
