using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ExtremeVoiceEngine.Interface;

namespace ExtremeVoiceEngine.VoiceVox;

public sealed class VoiceVoxEngine : IParametableEngine<VoiceVoxParameter>
{
    public float Wait { get; set; }

    public void Cancel()
    {
        throw new NotImplementedException();
    }

    public void SetParameter(VoiceVoxParameter param)
    {
        throw new NotImplementedException();
    }

    public IEnumerator Speek(string text)
    {
        throw new NotImplementedException();
    }
}
