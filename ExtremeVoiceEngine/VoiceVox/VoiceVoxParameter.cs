using ExtremeVoiceEngine.Interface;

namespace ExtremeVoiceEngine.VoiceVox;

public sealed class VoiceVoxParameter : IEngineParameter
{
    public string Speaker { get; set; }     = "ずんだもん";
    public string Style { get; set; }       = "あまあま";
    public float MasterVolume { get; set; } = 10.0f;
}
