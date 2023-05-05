using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace ExtremeRoles.Module;

#nullable enable

public sealed class Updater
{
    public record ModUpdateData(string DownloadUrl, string DllName);

    public static Updater Instance = new Updater();

    private HttpClient client = new HttpClient();

    private List<string> updateUrl = new List<string>();
    private List<string> downloadUrls = new List<string>();

    private const string contentType = "content_type";

    private static string pluginFolder
    {
        get
        {
            string? auPath = Path.GetDirectoryName(Application.dataPath);
            if (string.IsNullOrEmpty(auPath)) { return string.Empty; }

            return Path.Combine(auPath, "BepInEx", "plugins");
        }
    }

    public Updater()
    {
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "ExtremeRoles Updater");
    }

    public async void CheckAndUpdate()
    {
        // GetModUpdateDataメソッドで取得する
        List<ModUpdateData> updateData = new List<ModUpdateData>();

        foreach (ModUpdateData data in updateData)
        {
            using (var stream = await getStreamFromUrl(data.DownloadUrl))
            {
                if (stream is null) { continue; }
                installModFromStream(stream, data.DllName);
            }
        }
    }

    private void installModFromStream(Stream stream, string dllName)
    {
        string installDir = pluginFolder;
        if (string.IsNullOrEmpty(installDir)) { return; }

        string installModPath = Path.Combine(installDir, $"{dllName}.dll");
        string oldModPath = $"{installModPath}.old";
        if (File.Exists(oldModPath))
        {
            File.Delete(oldModPath);
        }

        File.Move(installModPath, oldModPath);

        using var fileStream = File.Create(installModPath);
        stream.CopyTo(fileStream);
    }

    private async Task<Stream?> getStreamFromUrl(string url)
    {
        var response = await this.client.GetAsync(
            new Uri(url),
            HttpCompletionOption.ResponseContentRead);
        if (response.StatusCode != HttpStatusCode.OK || response.Content == null)
        {
            ExtremeRolesPlugin.Logger.LogError("Server returned no data: " + response.StatusCode.ToString());
            return null;
        }

        var responseStream = await response.Content.ReadAsStreamAsync();

        return responseStream;
    }

}
