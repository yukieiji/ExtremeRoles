
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using UnityEngine;

using ExtremeRoles.Module;

namespace ExtremeSkins.SkinLoader;

public interface ISkinLoader
{
	public IReadOnlyDictionary<string, T> Load<T>() where T : class;
	public IEnumerator Fetch();

	protected static async Task getData(string url, string savePath)
	{
		string? auPath = Path.GetDirectoryName(Application.dataPath);

		if (string.IsNullOrEmpty(auPath)) { return; }

		HttpClient http = new HttpClient();
		http.DefaultRequestHeaders.Add("User-Agent", "ExtremeSkins Updater");
		http.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
		{
			NoCache = true
		};

		try
		{
			var response = await http.GetAsync(
				new Uri(url),
				HttpCompletionOption.ResponseContentRead);
			if (response.StatusCode != HttpStatusCode.OK)
			{
				ExtremeSkinsPlugin.Logger.LogInfo($"Can't load json");
			}
			if (response.Content == null)
			{
				ExtremeSkinsPlugin.Logger.LogInfo(
					$"Server returned no data: {response.StatusCode}");
				return;
			}

			using (var responseStream = await response.Content.ReadAsStreamAsync())
			{
				using (var fileStream = File.Create(
					Path.Combine(auPath, savePath)))
				{
					responseStream.CopyTo(fileStream);
				}
			}
		}
		catch (Exception e)
		{
			ExtremeSkinsPlugin.Logger.LogInfo(
				$"Unable to fetch data from {url}\n{e.Message}");
		}
	}
}

public sealed class ExtremeSkinLoader : NullableSingleton<ExtremeSkinLoader>
{
	private readonly Dictionary<Type, ISkinLoader> loader = new Dictionary<Type, ISkinLoader>();

	public void AddLoader<C, T>() where T : ISkinLoader, new()
	{
		this.loader.Add(typeof(C), new T());
	}

	public IEnumerator Fetch()
	{
		foreach (var loader in this.loader.Values)
		{
			yield return loader.Fetch();
		}
	}

	public IReadOnlyDictionary<string, T> Load<T>() where T : class
	{
		return this.loader[typeof(T)].Load<T>();
	}
}
