using BepInEx.Configuration;

using ExtremeVoiceEngine.Interface;

using ExtremeVoiceEngine.Extension;


namespace ExtremeVoiceEngine.VoiceVox;


// デフォルトで設定している「ずんだもん」は[VoiceVox](https://voicevox.hiroshiba.jp/)によって提供されている合成音声です
public sealed class VoiceVoxParameter : IEngineParameter
{
    public string Speaker { get; set; }     = "ずんだもん";
    public string Style { get; set; }       = "あまあま";
    public float MasterVolume { get; set; } = 10.0f;

    private ConfigEntry<string> speakerEntry;
    private ConfigEntry<string> styleEntry;
    private ConfigEntry<float> volumeEntry;

    public VoiceVoxParameter()
    {
        var config = ExtremeVoiceEnginePlugin.Instance.Config;
        this.speakerEntry = config.Bind(
            "VoiceVoxParameter", "speaker", "ずんだもん");
        this.styleEntry = config.Bind(
            "VoiceVoxParameter", "style", "あまあま");
        this.volumeEntry = config.Bind(
            "VoiceVoxParameter", "volume", 10.0f);
    }

    public void SaveConfig()
    {
        this.speakerEntry.Value = this.Speaker;
        this.styleEntry.Value = this.Style;
        this.volumeEntry.Value = this.MasterVolume;
    }

    public void LoadConfig()
    {
        this.Speaker      = this.speakerEntry.Value;
        this.Style        = this.styleEntry.Value;
        this.MasterVolume = this.volumeEntry.Value;
    }

    public override string ToString()
        => TranslationController.Instance.GetString(
            "voicevoxParam", Speaker, Style, MasterVolume);
}
