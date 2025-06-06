﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.JsonData;

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
				var latestData = await JsonParser.GetRestApiAsync<GitHubReleaseData>(
					$"{Endpoint}releases/latest");
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
					var allRelease = await JsonParser.GetRestApiAsync<GitHubReleaseData[]>(
						$"{Endpoint}releases?page={page}");

					if (allRelease == null)
					{
						break;
					}

					foreach (var targetRelease in allRelease)
					{
						int diff = getReleaseDiff(targetRelease, curVersion);
						if (diff > 0)
						{
							ExtremeRolesPlugin.Logger.LogInfo($"Find DowngradeData, Create Download Data....");
							convertReleaseToDownloadData(result, targetRelease);
							break;
						}
						else
						{
							// DDOSにならないように3秒程度待つ
							ExtremeRolesPlugin.Logger.LogInfo($"Wait 3s, For preventing DDOS....");
							await Task.Delay(300);
						}
					}
				}
				break;
			default:
				break;
		}
		return result;
	}

	private static int getReleaseDiff(in GitHubReleaseData releaseData, in Version curVersion)
	{
		Version ver = Version.Parse(
			releaseData.tag_name.Replace("v", ""));
		int diff = curVersion.CompareTo(ver);

		return diff;
	}

	private void convertReleaseToDownloadData(
		in List<AutoModInstaller.DownloadData> result,
		in GitHubReleaseData releaseData)
	{
		foreach (var asset in releaseData.assets)
		{
			string? browser_download_url = asset.browser_download_url;
			if (string.IsNullOrEmpty(browser_download_url) ||
				asset.content_type.Equals("application/x-zip-compressed"))
			{
				continue;
			}

			foreach (string dll in this.DllName)
			{
				if (!browser_download_url.EndsWith(dll))
				{
					continue;
				}
				var data = new AutoModInstaller.DownloadData(browser_download_url, dll);
				ExtremeRolesPlugin.Logger.LogInfo($"Create DonwloadData:{data.ToString()}");
				result.Add(
					new(browser_download_url, dll));
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
		string CheckWait,
		string NoOp,
		string OpStart,
		string OpProgress,
		string Restart,
		string Fail);

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
			InstallType installType);
	}

	public AutoModInstaller()
	{
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
				await autoModInstallFromWeb(
					InstallType.Downgrade,
					new("searchDowngradeWait",
						"downgradeNotFound",
						"downgradeStart",
						"downgradeInProgress",
						"downgradeRestart",
						"downgradeManually"));
			});

		menu.destroyOnClose = true;
		menu.Show(
			Tr.GetString("donwgradeConfirm"));
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

	public void AddMod<TRepoType>()
	{
		var assembly = Assembly.GetCallingAssembly();
		if (assembly is null) { return; }
		this.AddMod<ExRRepositoryInfo>($"{assembly.GetName().Name}.dll");
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
			Tr.GetString(trans.CheckWait));

		try
		{
			List<DownloadData> updatingData = new List<DownloadData>(this.service.Count);

			foreach (var repo in this.service.Values)
			{
				var installData = await repo.GetInstallData(installType);
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
					Tr.GetString(trans.NoOp));
				this.isRunning = false;
				ExtremeRolesPlugin.Logger.LogInfo($"---- Nothing Ops:{installType} ----");
				return;
			}

			setPopupText(
				Tr.GetString(trans.OpStart));
			clearOldMod();

			// 起動後すぐに実行すると失敗するんで1s待機
			await Task.Delay(100);

			this.InfoPopup.StartCoroutine(
				Effects.Lerp(0.01f, new Action<float>(
					(p) => { setPopupText(
						Tr.GetString(trans.OpProgress)); })));

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

			ExtremeRolesPlugin.Logger.LogInfo($"---- Complete Auto install Ops:{installType} ----");
			this.InfoPopup.StartCoroutine(
				Effects.Lerp(0.01f, new Action<float>(
					(p) => { this.showPopup(
						Tr.GetString(trans.Restart)); })));
		}
		catch (Exception ex)
		{
			Logging.Error(ex.ToString());
			this.showPopup(
				Tr.GetString(trans.Fail));
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
		var response = await ExtremeRolesPlugin.Instance.Http.GetAsync(
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

