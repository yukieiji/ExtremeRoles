using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AmongUs.Data;
using AmongUs.Data.Settings;
using Assets.InnerNet;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ExtremeRoles.Module;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module;

[Collection("UnityMock")]
public class ModAnnounceTests : IDisposable
{
    private readonly string _cacheDir = "ExtremeRoles/Cache";
    private readonly List<string> _tempFilesCreated = new();

    private sealed class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request));
        }
    }

    public ModAnnounceTests()
    {
        MockSetupHelper.SetupCommonMocks();

        var loggerField = typeof(ExtremeRolesPlugin).GetField("Logger", BindingFlags.NonPublic | BindingFlags.Static);
        if (loggerField != null && loggerField.GetValue(null) == null)
        {
            loggerField.SetValue(null, BepInEx.Logging.Logger.CreateLogSource("UnitTest"));
        }

        if (ExtremeRolesPlugin.Instance == null)
        {
            var plugin = (ExtremeRolesPlugin)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(ExtremeRolesPlugin));
            typeof(ExtremeRolesPlugin).GetField("<Http>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(plugin, new HttpClient());
            typeof(ExtremeRolesPlugin).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.SetValue(null, plugin);
        }

        SetupDataManagerSettings();
    }

    private static void SetupDataManagerSettings()
    {
        try
        {
            var mockLangSettings = new Mock<LanguageSettingsData>(IntPtr.Zero);
            mockLangSettings.SetupGet(l => l.CurrentLanguage).Returns(SupportedLangs.Japanese);

            var mockSettings = new Mock<SettingsData>(IntPtr.Zero);
            mockSettings.SetupGet(s => s.Language).Returns(mockLangSettings.Object);

            var mockSettingsHelper = new Mock<MockDataManagerget_SettingsHelper>();
            mockSettingsHelper.Setup(h => h.Invoke()).Returns(mockSettings.Object);
            MockDataManagerget_SettingsHelper.Instance = mockSettingsHelper.Object;
        }
        catch { }
    }

    public void Dispose()
    {
        foreach (var file in _tempFilesCreated)
        {
            if (File.Exists(file))
            {
                try { File.Delete(file); } catch { }
            }
        }

        if (Directory.Exists(_cacheDir))
        {
            var files = Directory.GetFiles(_cacheDir);
            if (files.Length == 0)
            {
                try { Directory.Delete(_cacheDir); } catch { }
            }
        }
    }

    private void TrackFile(string path)
    {
        _tempFilesCreated.Add(path);
    }

    private static void RunCoroutine(IEnumerator coroutine)
    {
        while (coroutine.MoveNext())
        {
            if (coroutine.Current is IEnumerator sub)
            {
                RunCoroutine(sub);
            }
        }
    }

    [Fact]
    public void WebAnnounce_Convert_ReturnsSavedAnnounceWithGivenIdAndTime()
    {
        var webAnnounce = new ModAnnounce.WebAnnounce("Title", "ShortTitle", "Bio", "Body");
        var time = new DateTime(2025, 1, 1, 12, 0, 0);
        int id = 10001;

        var saved = webAnnounce.Convert(id, time);

        Assert.Equal(id, saved.Id);
        Assert.Equal(time, saved.OpenTime);
        Assert.Equal("Title", saved.Title);
        Assert.Equal("ShortTitle", saved.ShortTitle);
        Assert.Equal("Bio", saved.Bio);
        Assert.Equal("Body", saved.Body);
    }

    [Fact]
    public void SavedAnnounce_Properties_AreInitializedCorrectly()
    {
        var time = new DateTime(2025, 1, 1, 12, 0, 0);
        var saved = new ModAnnounce.SavedAnnounce(10002, time, "Test Title", "Short", "Sub", "Text Body");

        Assert.Equal(10002, saved.Id);
        Assert.Equal(time, saved.OpenTime);
        Assert.Equal("Test Title", saved.Title);
        Assert.Equal("Short", saved.ShortTitle);
        Assert.Equal("Sub", saved.Bio);
        Assert.Equal("Text Body", saved.Body);
    }

    [Fact]
    public void AddModAnnounce_WhenCacheFileDoesNotExist_ReturnsVanillaAnnounce()
    {
        string expectedSaveFile = $"ExtremeRoles/Cache/Announce_{DataManager.Settings.Language.CurrentLanguage}.json";
        if (File.Exists(expectedSaveFile))
        {
            File.Delete(expectedSaveFile);
        }

        var vanilla = new Il2CppReferenceArray<Announcement>(0);

        var result = ModAnnounce.AddModAnnounce(vanilla);

        Assert.Same(vanilla, result);
    }

    [Fact]
    public void AddModAnnounce_WhenCacheFileExists_HandlesModAnnounces()
    {
        if (!Directory.Exists(_cacheDir))
        {
            Directory.CreateDirectory(_cacheDir);
        }

        string saveFile = $"ExtremeRoles/Cache/Announce_{DataManager.Settings.Language.CurrentLanguage}.json";
        TrackFile(saveFile);

        var modAnnounces = new[]
        {
            new ModAnnounce.SavedAnnounce(10001, new DateTime(2025, 1, 10), "Mod 1", "M1", "Bio1", "Body1"),
            new ModAnnounce.SavedAnnounce(10002, new DateTime(2025, 1, 20), "Mod 2", "M2", "Bio2", "Body2"),
        };

        File.WriteAllText(saveFile, JsonSerializer.Serialize(modAnnounces));

        var mockVanillaAnn = new Mock<Announcement>(IntPtr.Zero);
        mockVanillaAnn.SetupGet(a => a.Number).Returns(1);
        mockVanillaAnn.SetupGet(a => a.Date).Returns("2025-01-15T00:00:00");
        mockVanillaAnn.SetupGet(a => a.Title).Returns("Vanilla 1");

        var vanilla = new Il2CppReferenceArray<Announcement>(new[] { mockVanillaAnn.Object });

        var result = ModAnnounce.AddModAnnounce(vanilla);

        Assert.NotNull(result);
    }

    [Fact]
    public void AddModAnnounce_WhenFileCorrupted_ReturnsVanillaAnnounce()
    {
        if (!Directory.Exists(_cacheDir))
        {
            Directory.CreateDirectory(_cacheDir);
        }

        string saveFile = $"ExtremeRoles/Cache/Announce_{DataManager.Settings.Language.CurrentLanguage}.json";
        TrackFile(saveFile);

        File.WriteAllText(saveFile, "Invalid JSON content {{{");

        var vanilla = new Il2CppReferenceArray<Announcement>(0);

        var result = ModAnnounce.AddModAnnounce(vanilla);

        Assert.Same(vanilla, result);
    }

    [Fact]
    public void CoFetchAnnounce_WhenServerReturnsError_TerminatesEarly()
    {
        var customClient = new HttpClient(new MockHttpMessageHandler(req =>
        {
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }));

        typeof(ExtremeRolesPlugin).GetField("<Http>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(ExtremeRolesPlugin.Instance, customClient);

        IEnumerator coroutine = ModAnnounce.CoFetchAnnounce();

        bool hasMore = coroutine.MoveNext();
        Assert.True(hasMore);

        hasMore = coroutine.MoveNext();
        Assert.False(hasMore);
    }

    [Fact]
    public void CoFetchAnnounce_WhenServerReturnsValidDates_DownloadsAndSavesAnnounces()
    {
        string saveFile = $"ExtremeRoles/Cache/Announce_{DataManager.Settings.Language.CurrentLanguage}.json";
        TrackFile(saveFile);
        if (File.Exists(saveFile))
        {
            File.Delete(saveFile);
        }

        var announceTime = new DateTime(2020, 1, 1, 10, 0, 0);
        string datesJson = JsonSerializer.Serialize(new List<DateTime> { announceTime });
        string announceJson = JsonSerializer.Serialize(new ModAnnounce.WebAnnounce("Net Title", "Net Short", "Net Bio", "Net Body"));

        var customClient = new HttpClient(new MockHttpMessageHandler(req =>
        {
            if (req.RequestUri!.AbsoluteUri.EndsWith("allInfo.json"))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(datesJson, Encoding.UTF8, "application/json")
                };
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(announceJson, Encoding.UTF8, "application/json")
                };
            }
        }));

        typeof(ExtremeRolesPlugin).GetField("<Http>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(ExtremeRolesPlugin.Instance, customClient);

        IEnumerator coroutine = ModAnnounce.CoFetchAnnounce();

        Exception? caughtEx = null;
        try
        {
            RunCoroutine(coroutine);
        }
        catch (Exception ex)
        {
            caughtEx = ex;
        }

        if (caughtEx != null)
        {
            Assert.Fail($"Coroutine threw exception: {caughtEx}");
        }

        Assert.True(File.Exists(saveFile), $"File {saveFile} does not exist");
        string content = File.ReadAllText(saveFile);
        Assert.Contains("Net Title", content);
    }
}
