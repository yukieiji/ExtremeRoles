using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;

using System.Text.Json;
using Assets.InnerNet;
using ExtremeRoles.Helper;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

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
		public Announcement Announcement
			=> new Announcement()
			{
				Number = this.Id,
				Title = this.Title,
				ShortTitle = this.ShortTitle,
				SubTitle = this.Bio,
				Text = this.Body,
				Date = this.OpenTime.ToString(),
				Id = "ExtremeRolesAnnounce",
			};
	}

	public const string targetURL = "";
	public const string SaveDirectoryPath = "ExtremeRoles/Cache";
	public const string fileName = "Announce.json";
	public const string SaveFile = $"{SaveDirectoryPath}/{fileName}";

	public static Il2CppReferenceArray<Announcement> AddModAnnounce(
		Il2CppReferenceArray<Announcement> vanillaAnnounce)
	{
		try
		{
			using var stream = new FileStream(SaveFile, FileMode.Open, FileAccess.Read);
			SavedAnnounce[] modAnnounce = JsonSerializer.Deserialize<SavedAnnounce[]>(stream);

			var allAnnounce = modAnnounce
				.Select(x => x.Announcement)
				.Concat(vanillaAnnounce.ToArray())
				.ToArray();

			Array.Sort(allAnnounce,
				(x, y) => DateTime.Compare(
					DateTime.Parse(x.Date),
					DateTime.Parse(y.Date)));
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
			targetURL, HttpCompletionOption.ResponseContentRead);
		yield return task.Wait();

		var response = task.Result;
		if (response.StatusCode != HttpStatusCode.OK || response.Content == null)
		{
			Logging.Error($"Server returned no data: {response.StatusCode}");
			yield break;
		}

		TaskWaiter<Stream> streamReadTask  = response.Content.ReadAsStreamAsync();
		yield return streamReadTask.Wait();

		ValueTaskWaiter<List<DateTime>> jsonReadTask =
			JsonSerializer.DeserializeAsync<List<DateTime>>(streamReadTask.Result);
		yield return jsonReadTask.Wait();

		yield return coSaveToAnnounce(client, jsonReadTask.Result);
	}

	private static IEnumerator coSaveToAnnounce(HttpClient client, List<DateTime> datas)
	{
		var saveAnnounce = new Stack<SavedAnnounce>();
		int id = 10000;

		if (File.Exists(SaveFile))
		{
			using (var stream = new FileStream(SaveFile, FileMode.Open, FileAccess.Read))
			{
				ValueTaskWaiter<Stack<SavedAnnounce>> cacheAnnounce =
					JsonSerializer.DeserializeAsync<Stack<SavedAnnounce>>(stream);

				yield return cacheAnnounce.Wait();

				saveAnnounce = cacheAnnounce.Result;
			}
			id += saveAnnounce.Count;

			var result = saveAnnounce.Select(
				x => x.OpenTime).ToHashSet();

			datas = datas.Where(x => !result.Contains(x)).ToList();

			if (datas.Count == 0)
			{
				yield break;
			}
		}

		var jstTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
		var curJst = TimeZoneInfo.ConvertTime(DateTime.Now, jstTimeZoneInfo);
		// 公開していいやつだけ
		datas = datas.Where(x => x <= curJst).ToList();

		yield return coGetAnnounce(client, datas, id, saveAnnounce);
		yield return coSaveAnnounce(saveAnnounce);
	}

	private static IEnumerator coGetAnnounce(
		HttpClient client,
		IReadOnlyList<DateTime> dlList,
		int id,
		Stack<SavedAnnounce> dlResult)
	{
		foreach (var time in dlList)
		{
			TaskWaiter<HttpResponseMessage> task = client.GetAsync(
				$"{targetURL}/{time}", HttpCompletionOption.ResponseContentRead);
			yield return task.Wait();

			var response = task.Result;

			if (response.StatusCode != HttpStatusCode.OK ||
				response.Content == null)
			{
				Logging.Error($"Server returned no data: {response.StatusCode}");
				continue;
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

	private static IEnumerator coSaveAnnounce(Stack<SavedAnnounce> announce)
	{
		if (!Directory.Exists(SaveDirectoryPath))
		{
			Directory.CreateDirectory(SaveDirectoryPath);
		}
		using var stream = new FileStream(SaveFile, FileMode.OpenOrCreate, FileAccess.Write);

		TaskWaiter serializeWaiter = JsonSerializer.SerializeAsync(stream, announce);
		yield return serializeWaiter.Wait();
	}
}
