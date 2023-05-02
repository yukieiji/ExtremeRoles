using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ExtremeVoiceEngine.Interface;

namespace ExtremeVoiceEngine.VoiceVox;

public sealed class VoiceVoxParameter : IEngineParameter
{
    public string Speaker { get; set; }     = string.Empty;
    public string Style { get; set; }       = string.Empty;
    public float MasterVolume { get; set; } = 1.0f;
}
