using ExtremeVoiceEngine.Interface;

namespace ExtremeVoiceEngine.VoiceVox;


// デフォルトで設定している「ずんだもん」は[VoiceVox](https://voicevox.hiroshiba.jp/)によって提供されている合成音声です
public sealed class VoiceVoxParameter : IEngineParameter
{
    public string Speaker { get; set; }     = "ずんだもん";
    public string Style { get; set; }       = "あまあま";
    public float MasterVolume { get; set; } = 10.0f;

    public override string ToString()
        => $"Speaker:{Speaker}   Style:{Style}  Volume:{MasterVolume}";
}
