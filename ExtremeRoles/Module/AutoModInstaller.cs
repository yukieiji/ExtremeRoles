using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;

using UnityEngine;
using Newtonsoft.Json.Linq;

using ExtremeRoles.Helper;
using ExtremeRoles.Extension.Json;
using ExtremeRoles.Extension.Il2Cpp;

namespace ExtremeRoles.Module;

#nullable enable

public sealed class ExRRepositoryInfo : AutoModInstaller.IRepositoryInfo
{
	public const string Endpoint = "https://api.github.com/repos/yukieiji/ExtremeRoles/";

	private static Version? RunningVersion => Assembly.GetExecutingAssembly().GetName().Version;

	public List<string> DllName { private set; get; } = new List<string>()
	{
		"ExtremeRoles.dll"
	};

	public async Task<IReadOnlyList<AutoModInstaller.DownloadData>> GetInstallData(
		HttpClient client,
		AutoModInstaller.InstallType installType)
	{
		var result = new List<AutoModInstaller.DownloadData>(this.DllName.Count);
		var curVersion = RunningVersion;
		if (curVersion is null)
		{
			return result;
		}
		switch (installType)
		{
			case AutoModInstaller.InstallType.Update:
				JObject latestData = await AutoModInstaller.IRepositoryInfo.GetRestApiData(
					client, $"{Endpoint}releases/latest");
				if (getReleaseDiff(latestData, curVersion) < 0)
				{
					ExtremeRolesPlugin.Logger.LogInfo($"Find UpdateData, Create Download Data....");
					convertReleaseToDownloadData(result, latestData);
				}
				break;
			case AutoModInstaller.InstallType.Downgrade:
				int page = 0;
				while (result.Count == 0)
				{
					++page;
					var allRelease = await AutoModInstaller.IRepositoryInfo.GetRestApiData(
						client, $"{Endpoint}releases?page={page}");

					if (!allRelease.IsTryCast<JArray>(out var arr))
					{
						break;
					}

					for (int i = 0; i < arr!.Count; ++i)
					{
						JObject? targetRelease = arr.Get<JObject>(i);
						if (targetRelease == null)
						{
							continue;
						}

						int diff = getReleaseDiff(targetRelease, curVersion);
						if (diff > 1)
						{
							convertReleaseToDownloadData(result, targetRelease);
							break;
						}
					}
				}
				break;
			default:
				break;
		}
		return result;
	}

	private static int getReleaseDiff(in JObject releaseData, in Version curVersion)
	{
		string? tagname = releaseData["tag_name"]?.ToString();

		if (string.IsNullOrEmpty(tagname))
		{
			return 0; // Something went wrong
		}

		Version ver = Version.Parse(tagname.Replace("v", ""));
		int diff = curVersion.CompareTo(ver);

		return diff;
	}

	private void convertReleaseToDownloadData(
		in List<AutoModInstaller.DownloadData> result,
		in JObject releaseData)
	{
		JToken assets = releaseData["assets"];
		if (!assets.HasValues)
		{
			return;
		}

		for (JToken current = assets.First; current != null; current = current.Next)
		{
			string? browser_download_url = current["browser_download_url"]?.ToString();
			if (string.IsNullOrEmpty(browser_download_url) ||
				!current.TryGet(
					AutoModInstaller.IRepositoryInfo.ContentType,
					out var contentResult))
			{
				continue;
			}

			string content = contentResult!.ToString();

			if (content.Equals("application/x-zip-compressed")) { continue; }

			foreach (string dll in this.DllName)
			{
				if (browser_download_url.EndsWith(dll))
				{
					var data = new AutoModInstaller.DownloadData(browser_download_url, dll);
					ExtremeRolesPlugin.Logger.LogInfo($"Create DonwloadData:{data.ToString()}");
					result.Add(
						new(browser_download_url, dll));
				}
			}
		}
	}
}

public sealed class AutoModInstaller
{
	public GenericPopup? InfoPopup { private get; set; }

	public static AutoModInstaller Instance = new AutoModInstaller();

	public bool IsInit => InfoPopup != null;

	public enum InstallType
	{
		Update,
		Downgrade
	}

	public readonly record struct DownloadData(string DownloadUrl, string DllName)
	{
		public override string ToString()
			=> $"DL URL:{this.DownloadUrl}, DllName:{this.DllName}";
	}

	private sealed record TransKey(
		string CheckWait = "",
		string NoOp = "",
		string OpStart = "",
		string OpProgress = "",
		string Restart = "",
		string Fail="");

	private static string pluginFolder
	{
		get
		{
			string? auPath = Path.GetDirectoryName(Application.dataPath);
			if (string.IsNullOrEmpty(auPath)) { return string.Empty; }

			return Path.Combine(auPath, "BepInEx", "plugins");
		}
	}

	private readonly ConcurrentDictionary<Type, IRepositoryInfo> service = new ConcurrentDictionary<Type, IRepositoryInfo>();
	private bool isRunning = false;

	public interface IRepositoryInfo
	{
		protected const string ContentType = "content_type";

		public List<string> DllName { get; }

		public Task<IReadOnlyList<DownloadData>> GetInstallData(
			HttpClient client,
			InstallType installType);

		protected static async Task<JObject> GetRestApiData(HttpClient client, string targetUrl)
		{
			ExtremeRolesPlugin.Logger.LogInfo($"Conecting...:{targetUrl}");

			var response = await client.GetAsync(
				targetUrl, HttpCompletionOption.ResponseContentRead);
			if (response.StatusCode != HttpStatusCode.OK || response.Content == null)
			{
				Logging.Error($"Server returned no data: {response.StatusCode}");
				return null!;
			}

			string json = await response.Content.ReadAsStringAsync();
			JObject data = JObject.Parse(json);

			return data;
		}
	}

	private readonly HttpClient client = new HttpClient();

	public AutoModInstaller()
	{
		this.client = new HttpClient();
		this.client.DefaultRequestHeaders.Add("User-Agent", "ExtremeRoles Updater");

		this.AddRepository<ExRRepositoryInfo>();
	}

	public async Task Update()
	{
		await autoModInstallFromWeb(
			InstallType.Update,
			new("chekUpdateWait",
				"latestNow",
				"updateNow",
				"updateInProgress",
				"updateRestart",
				"updateManually"));
	}

	public void Downgrade()
	{
		var menu = Prefab.CreateConfirmMenu(
			async () =>
			{
				await autoModInstallFromWeb(InstallType.Downgrade, new());
			});

		menu.destroyOnClose = true;
		menu.Show("ダウングレードするがよろしいかね？");
	}

	public void AddRepository<TRepoType>() where TRepoType : class, IRepositoryInfo, new()
	{
		AddRepository(new TRepoType());
	}

	public void AddRepository<TRepoType>(TRepoType repository) where TRepoType : class, IRepositoryInfo, new()
	{
		if (!this.service.TryAdd(typeof(TRepoType), repository))
		{
			ExtremeRolesPlugin.Logger.LogError("This instance already added!!");
		}
	}

	public void AddMod<TRepoType>(string dllName) where TRepoType : class, IRepositoryInfo, new()
	{
		TRepoType? repo;

		if (this.service.TryGetValue(typeof(TRepoType), out var instance) &&
			instance is TRepoType castedInstance)
		{
			repo = castedInstance;
		}
		else
		{
			repo = new TRepoType();
			AddRepository(repo);
		}

		repo.DllName.Add(dllName);
	}

	private async Task autoModInstallFromWeb(InstallType installType, TransKey trans)
	{
		if (this.isRunning || this.InfoPopup == null) { return; }
		this.isRunning = true;

		ExtremeRolesPlugin.Logger.LogInfo($"---- Start Auto install Ops:{installType} ----");

		this.InfoPopup.Show(
			Translation.GetString(trans.CheckWait));

		try
		{
			List<DownloadData> updatingData = new List<DownloadData>(this.service.Count);

			foreach (var repo in this.service.Values)
			{
				var installData = await repo.GetInstallData(this.client, installType);
				if (installData.Count == 0)
				{
					continue;
				}
				updatingData.AddRange(installData);
			}

			if (updatingData.Count == 0)
			{
				ExtremeRolesPlugin.Logger.LogInfo("Install Data nothing!!");
				setPopupText(
					Translation.GetString(trans.NoOp));
				this.isRunning = false;
				return;
			}

			setPopupText(
				Translation.GetString(trans.OpStart));
			clearOldMod();

			this.InfoPopup.StartCoroutine(
				Effects.Lerp(0.01f, new Action<float>(
					(p) => { setPopupText(
						Translation.GetString(trans.OpProgress)); })));

			foreach (DownloadData data in updatingData)
			{
				var result = getStreamFromUrl(data.DownloadUrl).GetAwaiter().GetResult();

				if (!result.HasValue()) { continue; }

				ExtremeRolesPlugin.Logger.LogInfo($"Replacing...");
				using (var stream = result.Value)
				{
					installModFromStream(stream, data.DllName);
				}
			}

			ExtremeRolesPlugin.Logger.LogInfo($"Completed!!!!!!!!!!!!!!");
			this.InfoPopup.StartCoroutine(
				Effects.Lerp(0.01f, new Action<float>(
					(p) => { this.showPopup(
						Translation.GetString(trans.Restart)); })));
		}
		catch (Exception ex)
		{
			Logging.Error(ex.ToString());
			this.showPopup(
				Translation.GetString(trans.Fail));
		}

		this.isRunning = false;
	}

	private void installModFromStream(in Stream stream, in string dllName)
	{
		string installDir = pluginFolder;
		if (string.IsNullOrEmpty(installDir)) { return; }

		string installModPath = Path.Combine(installDir, dllName);
		string oldModPath = $"{installModPath}.old";
		if (File.Exists(oldModPath))
		{
			File.Delete(oldModPath);
		}

		File.Move(installModPath, oldModPath);

		using (var fileStream = File.Create(installModPath))
		{
			stream.CopyTo(fileStream);
		}
	}

	private async Task<Expected<Stream>> getStreamFromUrl(string url)
	{
		ExtremeRolesPlugin.Logger.LogInfo($"Conecting : {url}");
		var response = await this.client.GetAsync(
			new Uri(url),
			HttpCompletionOption.ResponseContentRead);
		if (response.StatusCode != HttpStatusCode.OK || response.Content == null)
		{
			Logging.Error("Server returned no data: " + response.StatusCode.ToString());
			return null!;
		}

		var responseStream = await response.Content.ReadAsStreamAsync();

		return responseStream;
	}

	private void showPopup(string message)
	{
		setPopupText(message);
		if (this.InfoPopup != null)
		{
			this.InfoPopup.gameObject.SetActive(true);
		}
	}

	private void setPopupText(string message)
	{
		if (this.InfoPopup == null)
		{
			return;
		}

		if (this.InfoPopup.TextAreaTMP != null)
		{
			this.InfoPopup.TextAreaTMP.text = message;
		}
	}

	private static void clearOldMod()
	{
		try
		{
			string installDir = pluginFolder;
			if (string.IsNullOrEmpty(installDir)) { return; }

			DirectoryInfo d = new DirectoryInfo(installDir);
			var files = d.GetFiles("*.old").Select(x => x.FullName); // Getting old versions
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
}

