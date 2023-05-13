using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text;


namespace ExtremeVoiceEngine.VoiceVox;


public static class VoiceVoxBridge
{
    private static HttpClient client
    {
        get
        {
            if (_clientBody == null)
            {
                _clientBody = new HttpClient();
            }
            return _clientBody;
        }
    }
    private static HttpClient? _clientBody = null;

    private const string serverUrl = "http://127.0.0.1:50021/";
    private const string jsonType = "application/json";

    public static bool IsEstablishServer()
    {
        try
        {
            string json = GetVoice().GetAwaiter().GetResult();
            return !string.IsNullOrEmpty(json);
        }
        catch
        {
            return false;
        }
    }

    public static async Task<string> PostAudioQueryAsync(
        int speaker, string text, CancellationToken cancellationToken = default)
    {
        string url = $"{serverUrl}audio_query?speaker={speaker}&text={text}";

        try
        {
            using var response = await client.PostAsync(url, null, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                string jsonString = await response.Content.ReadAsStringAsync();
                cancellationToken.ThrowIfCancellationRequested();
                return jsonString;
            }
            else
            {
                string message = await response.Content.ReadAsStringAsync();
                cancellationToken.ThrowIfCancellationRequested();
                ExtremeVoiceEnginePlugin.Logger.LogError(
                    $"WebException: AudioQuery request failed. : {
                        (int)response.StatusCode}{ response.StatusCode}\n{message}");
                return string.Empty;
            }
        }
        catch (Exception e)
        {
            ExtremeVoiceEnginePlugin.Logger.LogError(
                $"Exception: AudioQuery request raised error. {e.Message}");
            return string.Empty;
        }
    }

    public static async Task<Stream?> PostSynthesisAsync(
        int speaker, string jsonQuery, CancellationToken cancellationToken = default)
    {
        string url = $"{serverUrl}synthesis?speaker={speaker}";

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(jsonQuery, Encoding.UTF8, jsonType);
        HttpResponseMessage? response = null;

        try
        {
            response = await client.SendAsync(
                request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStreamAsync();
                cancellationToken.ThrowIfCancellationRequested();
                return stream;
            }
            else
            {
                string message = await response.Content.ReadAsStringAsync();
                cancellationToken.ThrowIfCancellationRequested();
                ExtremeVoiceEnginePlugin.Logger.LogError(
                    $"WebException: Synthesis request failed. : {(int)response.StatusCode}{response.StatusCode}\n{message}");
                return null;
            }
        }
        catch (Exception e)
        {
            ExtremeVoiceEnginePlugin.Logger.LogError(
                $"Exception: Synthesis request raised error. {e.Message}");
            return null;
        }
    }


    public static async Task<string> GetVoice(CancellationToken cancellationToken = default)
    {
        string url = $"{serverUrl}speakers";

        try
        {
            using var response = await client.GetAsync(url, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                string jsonString = await response.Content.ReadAsStringAsync();
                cancellationToken.ThrowIfCancellationRequested();
                return jsonString;
            }
            else
            {
                string message = await response.Content.ReadAsStringAsync();
                cancellationToken.ThrowIfCancellationRequested();
                ExtremeVoiceEnginePlugin.Logger.LogError(
                    $"WebException: Get voice request failed. : {(int)response.StatusCode}{response.StatusCode}\n{message}");
                return string.Empty;
            }
        }
        catch (Exception e)
        {
            ExtremeVoiceEnginePlugin.Logger.LogError(
                $"Exception: Get voice request raised error. {e.Message}");
            return string.Empty;
        }
    }
}
