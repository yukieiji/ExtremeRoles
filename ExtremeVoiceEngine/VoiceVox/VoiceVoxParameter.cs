using BepInEx.Configuration;

using ExtremeVoiceEngine.Interface;

using ExtremeVoiceEngine.Extension;


namespace ExtremeVoiceEngine.VoiceVox;


// デフォルトで設定している「ずんだもん」は[VoiceVox](https://voicevox.hiroshiba.jp/)によって提供されている合成音声です
public sealed class VoiceVoxParameter : IEngineParameter
{
    public string Speaker { get; set; }     = "ずんだもん";
    public string Style { get; set; }       = "あまあま";
    public float MasterVolume { get; set; } = 2.5f;
    public float Speed { get; set; } = 1.0f;

    private ConfigEntry<string> speakerEntry;
    private ConfigEntry<string> styleEntry;
    private ConfigEntry<float> volumeEntry;
    private ConfigEntry<float> speedEntry;

    public VoiceVoxParameter()
    {
        var config = ExtremeVoiceEnginePlugin.Instance.Config;
        this.speakerEntry = config.Bind(
            "VoiceVoxParameter", "speaker", "ずんだもん");
        this.styleEntry = config.Bind(
            "VoiceVoxParameter", "style", "あまあま");
        this.volumeEntry = config.Bind(
            "VoiceVoxParameter", "volume", 2.5f);
        this.speedEntry = config.Bind(
            "VoiceVoxParameter", "speed", 1.0f);
    }

    public void SaveConfig()
    {
        this.speakerEntry.Value = this.Speaker;
        this.styleEntry.Value = this.Style;
        this.volumeEntry.Value = this.MasterVolume;
        this.speedEntry.Value = this.Speed;
    }

    public void LoadConfig()
    {
        this.Speaker      = this.speakerEntry.Value;
        this.Style        = this.styleEntry.Value;
        this.MasterVolume = this.volumeEntry.Value;
        this.Speed        = this.speedEntry.Value;
    }

    public override string ToString()
        => TranslationControllerExtension.GetString(
            "voicevoxParam", this.Speaker, this.Style, this.MasterVolume, this.Speed);
}
