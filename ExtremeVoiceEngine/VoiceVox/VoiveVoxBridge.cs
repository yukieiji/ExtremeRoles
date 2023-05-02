using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

using ExtremeVoiceEngine.Extension;

namespace ExtremeVoiceEngine.VoiceVox;

public static class VoiveVoxBridge
{
    private const string serverUrl = "http://127.0.0.1:50021/";
    private const string jsonType = "application/json";

    public static async Task<JObject?> GetVoice()
    {
        UnityWebRequest request = UnityWebRequest.Get($"{serverUrl}speakers");

        await request.SendWebRequest();

        if (request.isHttpError ||
            request.isNetworkError ||
            request.responseCode != 200)
        {
            return null;
        }

        string jsonData = request.downloadHandler.text;
        JObject json = JObject.Parse(jsonData);

        return json;
    }
}
