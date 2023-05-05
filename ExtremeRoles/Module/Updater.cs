using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

namespace ExtremeRoles.Module;

#nullable enable

public sealed class Updater
{
    public record ModUpdateData(string DownloadUrl, string DllName);

    public interface IRepositoryInfo
    {
        protected const string contentType = "content_type";

        public string URL { get; }

        public List<string> DllName { get; }

        public Task<List<ModUpdateData>> GetModUpdateData(HttpClient client);

        public Task<bool> HasUpdate(HttpClient client);
    }

    public static Updater Instance = new Updater();

    private HttpClient client = new HttpClient();
    private ServiceLocator<IRepositoryInfo> repoData = new ServiceLocator<IRepositoryInfo>();

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

    public void AddRepository<T>(T repository) where T : class, IRepositoryInfo, new()
    {
        this.repoData.Register(repository);
    }

    public void AddMod<TRepoType>(string dllName) where TRepoType : class, IRepositoryInfo, new()
    {
        IRepositoryInfo repo = this.repoData.Resolve<TRepoType>();
        repo.DllName.Add(dllName);
    }

    public async void CheckAndUpdate()
    {
        // アプデ確認中
        try
        {
            List<ModUpdateData> updatingData = new List<ModUpdateData>();

            foreach (var repo in this.repoData.GetAllService())
            {
                bool hasUpdate = await repo.HasUpdate(this.client);
                if (!hasUpdate) { continue; }

                List<ModUpdateData> updateData = await repo.GetModUpdateData(this.client);
                updatingData.AddRange(updateData);
            }

            clearOldVersions();

            if (updatingData.Count == 0)
            {
                //アプデなし通知
                return;
            }

            // アプデあり通知、更新処理
            foreach (ModUpdateData data in updatingData)
            {
                using (var stream = await getStreamFromUrl(data.DownloadUrl))
                {
                    if (stream is null) { continue; }
                    installModFromStream(stream, data.DllName);
                }
            }
            // 終了処置
        }
        catch (Exception ex)
        {
            // エラー通知
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

    private static void clearOldVersions()
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
            ExtremeRolesPlugin.Logger.LogError("Exception occured when clearing old versions:\n" + e);
        }
    }
}
