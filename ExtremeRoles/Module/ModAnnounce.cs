using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

using AmongUs.Data;
using Assets.InnerNet;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

using ExtremeRoles.Helper;

#nullable enable

namespace ExtremeRoles.Module;

public static class ModAnnounce
{
	public readonly record struct WebAnnounce(
		string Title,
		string ShortTitle,
		string Bio,
		string Body)
	{
		public SavedAnnounce Convert(int id, DateTime time)
			=> new(id, time, this.Title, this.ShortTitle, this.Bio, this.Body);
	}

	public readonly record struct SavedAnnounce(
		int Id,
		DateTime OpenTime,
		string Title,
		string ShortTitle,
		string Bio,
		string Body)
	{
		public Announcement Convert()
			=> new Announcement()
			{
				Number = this.Id,
				Title = this.Title,
				ShortTitle = this.ShortTitle,
				SubTitle = this.Bio,
				Text = this.Body,
				Date = this.OpenTime.ToString(),
				Id = "ExtremeRolesAnnounce",
				Language = (uint)curLang,
			};
	}

	private const string saveDirectoryPath = "ExtremeRoles/Cache";
	private const string fileName = "Announce_{0}.json";

	private static SupportedLangs curLang => DataManager.Settings.Language.CurrentLanguage;
#if DEBUG
	private const string branch = "develop";
#else
	private const string branch = "main";
#endif
	private const string endPoint = $"https://raw.githubusercontent.com/yukieiji/ExtremeRoles.Announce/{branch}/Announce";
	private const string allannounceData = $"{endPoint}/allInfo.json";

	private static JsonSerializerOptions jsonSerializeOption => new JsonSerializerOptions
	{
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		WriteIndented = true,
	};

	/*
	 *	Announce/
	 *		dateTime.json
	 *		{Lang}
	 *			{dateTime_1}.json
	 *			{dateTime_2}.json
	 */

	private static string saveFile
	{
		get
		{
			string langAnnounce = string.Format(
				fileName, curLang);
			return $"{saveDirectoryPath}/{langAnnounce}";
		}
	}

	public static Il2CppReferenceArray<Announcement> AddModAnnounce(
		Il2CppReferenceArray<Announcement> vanillaAnnounce)
	{
		if (!File.Exists(saveFile))
		{
			return vanillaAnnounce;
		}

		try
		{
			using var stream = new FileStream(saveFile, FileMode.Open, FileAccess.Read);
			SavedAnnounce[]? modAnnounce = JsonSerializer.Deserialize<SavedAnnounce[]>(
				stream, jsonSerializeOption);

			if (modAnnounce is null) { return vanillaAnnounce; }

			var allAnnounce = modAnnounce
				.Select(x => x.Convert())
				.Concat(vanillaAnnounce.ToArray())
				.ToArray();

			Array.Sort(allAnnounce,
				(x, y) => DateTime.Compare(
					DateTime.Parse(y.Date),
					DateTime.Parse(x.Date)));
			return allAnnounce;
		}
		catch (Exception ex)
		{
			Logging.Error($"Can't add ModAnnounce : {ex.Message}");
			return vanillaAnnounce;
		}
	}

	public static IEnumerator CoGetAnnounce()
	{
		var client = new HttpClient();
		client.DefaultRequestHeaders.Add(
			"User-Agent", "ExtremeRoles AnnounceGetter");
		yield return coFetchAnnounce(client);
	}

	private static IEnumerator coFetchAnnounce(HttpClient client)
	{
		TaskWaiter<HttpResponseMessage> task = client.GetAsync(
			allannounceData, HttpCompletionOption.ResponseContentRead);
		yield return task.Wait();

		var response = task.Result;
		if (response.StatusCode != HttpStatusCode.OK || response.Content == null)
		{
			Logging.Error($"Server returned no data: {response.StatusCode}");
			yield break;
		}

		TaskWaiter<Stream> streamReadTask  = response.Content.ReadAsStreamAsync();
		yield return streamReadTask.Wait();

		ValueTaskWaiter<List<DateTime>?> jsonReadTask =
			JsonSerializer.DeserializeAsync<List<DateTime>>(streamReadTask.Result);
		yield return jsonReadTask.Wait();

		var data = jsonReadTask.Result;
		if (data is null) { yield break; }

		yield return coSaveToAnnounce(client, data);
	}

	private static IEnumerator coSaveToAnnounce(HttpClient client, List<DateTime> datas)
	{
		var saveAnnounce = new Stack<SavedAnnounce>();
		int id = 10000;

		if (File.Exists(saveFile))
		{
			ValueTaskWaiter<Stack<SavedAnnounce>?>? cacheAnnounce = null;
			try
			{
				using var stream = new FileStream(saveFile, FileMode.Open, FileAccess.Read);
				cacheAnnounce = JsonSerializer.DeserializeAsync<Stack<SavedAnnounce>>(stream);
			}
			catch (Exception ex)
			{
				Logging.Error($"Can't serialize announce : {ex.Message}");
			}

			if (cacheAnnounce is not null)
			{
				yield return cacheAnnounce.Wait();

				saveAnnounce = cacheAnnounce.Result;

				if (saveAnnounce is not null)
				{
					id += saveAnnounce.Count;

					var result = saveAnnounce.Select(
						x => x.OpenTime).ToHashSet();

					datas = datas.Where(x => !result.Contains(x)).ToList();

					if (datas.Count == 0)
					{
						yield break;
					}
				}
			}
		}

		var jstTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
		var curJst = TimeZoneInfo.ConvertTime(DateTime.Now, jstTimeZoneInfo);
		// 公開していいやつだけ
		datas = datas.Where(x => x <= curJst).ToList();

		yield return coGetAnnounce(client, datas, id, saveAnnounce);

		yield return coSaveAnnounce(saveAnnounce);

		yield break;
	}

	private static IEnumerator coGetAnnounce(
		HttpClient client,
		IReadOnlyList<DateTime> dlList,
		int id,
		Stack<SavedAnnounce>? dlResult)
	{
		if (dlResult is null) { yield break; }

		foreach (var time in dlList)
		{
			TaskWaiter<HttpResponseMessage> task = client.GetAsync(
				createDLUrl(convertToString(time)),
				HttpCompletionOption.ResponseContentRead);
			yield return task.Wait();

			var response = task.Result;

			if (response.StatusCode != HttpStatusCode.OK ||
				response.Content == null)
			{
				Logging.Error(
					$"Server returned no data: {response.StatusCode}, Maybe can't find currentLanguage announce");
				yield break;
			}

			TaskWaiter<Stream> streamReadTask = response.Content.ReadAsStreamAsync();
			yield return streamReadTask.Wait();

			ValueTaskWaiter<WebAnnounce> jsonReadTask =
			JsonSerializer.DeserializeAsync<WebAnnounce>(streamReadTask.Result);
			yield return jsonReadTask.Wait();

			++id;

			dlResult.Push(
				jsonReadTask.Result.Convert(id, time));
		}
	}

	private static string convertToString(DateTime time) => time.ToString("yyyyMMddTHHmmss");

	private static IEnumerator coSaveAnnounce(Stack<SavedAnnounce>? announce)
	{
		if (announce is null || announce.Count == 0) { yield break; }

		if (!Directory.Exists(saveDirectoryPath))
		{
			Directory.CreateDirectory(saveDirectoryPath);
		}
		using var stream = new FileStream(saveFile, FileMode.OpenOrCreate, FileAccess.Write);
		TaskWaiter waiter = JsonSerializer.SerializeAsync(
			stream, announce.ToArray(),
			jsonSerializeOption);

		yield return waiter.Wait();
		yield break;
	}

	private static string createDLUrl(string dateTime)
		=> $"{endPoint}/{curLang}/{dateTime}.json";
}
