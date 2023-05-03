using System.Collections;

using System.Threading;

using UnityEngine;
using Newtonsoft.Json.Linq;

using ExtremeRoles.Extension.Json;

using ExtremeVoiceEngine.Interface;
using ExtremeVoiceEngine.Utility;

namespace ExtremeVoiceEngine.VoiceVox;

public sealed class VoiceVoxEngine : IParametableEngine<VoiceVoxParameter>
{
    public float Wait { get; set; }
    public AudioSource? Source { get; set; }
    
    private VoiceVoxParameter? param = null;
    private CancellationTokenSource cts = new CancellationTokenSource();
    private int speakerId = 0;

    private static CancellationToken cancellationToken => default(CancellationToken);

    public void Cancel()
    {
        this.cts.Cancel();
        if (Source != null)
        {
            Source.Stop();
            Source.clip = null;
        }
    }

    public void SetParameter(VoiceVoxParameter param)
    {
        var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken).Token;
        
        string jsonStr = VoiceVoxBridge.GetVoice(linkedToken).GetAwaiter().GetResult();

        if (string.IsNullOrEmpty(jsonStr)) { return; }

        string cleanedStr = @"{""Result"":" + jsonStr + @"}";
        
        JObject resultJson = JObject.Parse(cleanedStr);
        JArray? json = resultJson.Get<JArray>("Result");

        ExtremeVoiceEnginePlugin.Instance.Log.LogInfo($"Is Null?:{json == null}");
        if (json == null) { return; }

        for (int i = 0; i < json.Count; ++i)
        {
            JObject? speakerInfo = json.ChildrenTokens[i].TryCast<JObject>();
            if (speakerInfo == null) { continue; }

            JToken? nameToken = speakerInfo["name"];
            if (nameToken == null) { continue; }

            string name = nameToken.ToString();
            ExtremeVoiceEnginePlugin.Instance.Log.LogInfo($"Find Speaker:{name}");
            if (name != param.Speaker) { continue; }

            JArray? styles = speakerInfo.Get<JArray>("styles");
            if (styles == null) { continue; }

            for (int j = 0; j < styles.Count; ++j)
            {
                JObject? styleData = styles.ChildrenTokens[i].TryCast<JObject>();
                if (styleData == null) { continue; }

                JToken styleNameToken = styleData["name"];
                string styleName = styleNameToken.ToString();
                if (styleName != param.Style) { continue; }

                this.speakerId = (int)styleData["id"];
                this.param = param;
                return;
            }
        }

    }

    public IEnumerator Speek(string text)
    {
        if (param is null) { yield break; }
        if (Source == null)
        {
            var source = ISpeakEngine.CreateAudioMixer();
            if (source == null)
            {
                yield break;
            }
            Source = source;
        }

        var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken).Token;

        var jsonQueryTask = VoiceVoxBridge.PostAudioQueryAsync(this.speakerId, text, linkedToken);
        yield return TaskHelper.CoRunWaitAsync(jsonQueryTask);

        string jsonQuery = jsonQueryTask.Result;
        if (string.IsNullOrEmpty(jsonQuery))
        {
            yield break;
        }

        var streamTask = VoiceVoxBridge.PostSynthesisAsync(this.speakerId, jsonQuery, linkedToken);
        yield return TaskHelper.CoRunWaitAsync(streamTask);

        using var stream = streamTask.Result;

        if (stream is null)
        {
            yield break;
        }

        var audioClipTask = AudioClipHelper.CreateFromStreamAsync(stream, linkedToken);
        yield return TaskHelper.CoRunWaitAsync(audioClipTask);

        Source.PlayOneShot(audioClipTask.Result, param.MasterVolume);
        
        while (Source.isPlaying)
        {
            yield return null;
        }
        yield break;
    }

}
