using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtremeVoiceEngine.Interface;

public interface ISpeaker
{
    public float Wait { get; set; }

    public IEnumerator Speek(string text);

    public void Cancel();
}
