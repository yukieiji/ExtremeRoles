using System.Collections;

namespace ExtremeVoiceEngine.Interface;

public interface ISpeakEngine
{
    public float Wait { get; set; }

    public IEnumerator Speek(string text);

    public void Cancel();
}
