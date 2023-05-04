using System.Collections;

using UnityEngine;

using ExtremeVoiceEngine.Command;

namespace ExtremeVoiceEngine.Interface;

public interface ISpeakEngine
{
    public float Wait { get; set; }
    public AudioSource? Source { get; protected set; }

    public IEnumerator Speek(string text);
    public void Cancel();

    protected static AudioSource? CreateAudioMixer()
    {
        SoundManager soundManager = SoundManager.Instance;

        if (!soundManager) { return null; }

        AudioSource audioSource = soundManager.gameObject.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = soundManager.SfxChannel;
        audioSource.playOnAwake = false;
        audioSource.volume = 1.0f;
        audioSource.loop = false;
        audioSource.clip = null;
        return audioSource;
    }
}
