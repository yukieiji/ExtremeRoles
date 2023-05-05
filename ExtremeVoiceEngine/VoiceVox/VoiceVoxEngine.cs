using System.Collections;

using System.Threading;

using UnityEngine;
using Newtonsoft.Json.Linq;

using ExtremeRoles.Extension.Json;
using ExtremeRoles.Performance;

using ExtremeVoiceEngine.Command;
using ExtremeVoiceEngine.Extension;
using ExtremeVoiceEngine.Interface;
using ExtremeVoiceEngine.Utility;

namespace ExtremeVoiceEngine.VoiceVox;

public sealed class VoiceVoxEngine : IParametableEngine<VoiceVoxParameter>
{
    public float Wait { get; set; }
    public AudioSource? Source { get; set; }
    
    private VoiceVoxParameter param;
    private CancellationTokenSource cts = new CancellationTokenSource();
    private int speakerId
    {
        get => this.speakerIdEntry.Value;
        set
        {
            this.speakerIdEntry.Value = value;
        }
    }

    private BepInEx.Configuration.ConfigEntry<int> speakerIdEntry;
    private static CancellationToken cancellationToken => default(CancellationToken);

    private const string CharacterNameCmd = "char";
    private const string CharacterStyleCmd = "style";
    private const string VolumeCmd = "volume";

    public VoiceVoxEngine()
    {
        this.speakerIdEntry = ExtremeVoiceEnginePlugin.Instance.Config.Bind(
            "VoiceEngine", "speakerId", 0);
        this.param = new VoiceVoxParameter();
        this.param.LoadConfig();
    }

    public bool IsValid()
        => VoiceVoxBridge.IsEstablishServer();

    public void CreateCommand()
    {
        var cmd = CommandManager.Instance;

        cmd.AddSubCommand(
           VoiceEngine.Cmd, "voicevox",
           new(new Parser(
               new Option(CharacterNameCmd , 'c', Option.Kind.Optional),
               new Option(CharacterStyleCmd, 's', Option.Kind.Optional),
               new Option(VolumeCmd        , 'v', Option.Kind.Optional)), Parse));
        cmd.AddAlias("voicevox", "vv");
    }

    public void Parse(Result? result)
    {
        if (result is null) { return; }

        var newParam = new VoiceVoxParameter();
        if (result.TryGetOptionValue(CharacterNameCmd, out string charName))
        {
            newParam.Speaker = charName;
        }
        if (result.TryGetOptionValue(CharacterStyleCmd, out string stypeName))
        {
            newParam.Style = stypeName;
        }
        if (result.TryGetOptionValue(VolumeCmd, out string volume) &&
            float.TryParse(volume, out float value))
        {
            newParam.MasterVolume = value;
        }
        SetParameter(newParam);
    }

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
                this.param.SaveConfig();

                string log = TranslationController.Instance.GetString(
                    "voicevoxParamSetLog", parts: this.param.ToString());
                ExtremeVoiceEnginePlugin.Logger.LogInfo(log);
                if (FastDestroyableSingleton<HudManager>.Instance != null)
                {
                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddLocalChat(log);
                }
                return;
            }
        }
    }

    public override string ToString()
        => TranslationControllerExtension.GetString("voicevoxEngineToString", this.param.ToString());

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

        int speakerId = this.speakerId;

        var jsonQueryTask = VoiceVoxBridge.PostAudioQueryAsync(speakerId, text, linkedToken);
        yield return TaskHelper.CoRunWaitAsync(jsonQueryTask);

        string jsonQuery = jsonQueryTask.Result;
        if (string.IsNullOrEmpty(jsonQuery))
        {
            yield break;
        }

        var streamTask = VoiceVoxBridge.PostSynthesisAsync(speakerId, jsonQuery, linkedToken);
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
