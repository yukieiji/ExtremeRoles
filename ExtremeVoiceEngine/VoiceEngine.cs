using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using BepInEx.Unity.IL2CPP.Utils.Collections;

using ExtremeRoles.Module;

using ExtremeVoiceEngine.Interface;

namespace ExtremeVoiceEngine;

[Il2CppRegister]
public sealed class VoiceEngine : MonoBehaviour
{
    public ISpeaker? Speaker { get; set; }

    private bool running = false;
    private Queue<string> textQueue = new Queue<string>();

    public VoiceEngine(IntPtr ptr) : base(ptr) { }

    public void Destroy()
    {
        StopAllCoroutines();
        this.Speaker?.Cancel();
    }

    public void FixedUpdate()
    {
        if (this.Speaker is null || 
            this.running || 
            this.textQueue.Count == 0) { return; }
        
        string text = this.textQueue.Dequeue();
        StartCoroutine(this.coSpeek(text).WrapToIl2Cpp());
    }

    private IEnumerator coSpeek(string text)
    {
        if (this.Speaker is null) { yield break; }

        this.running = true;

        yield return this.Speaker.Speek(text);
        yield return new WaitForSeconds(this.Speaker.Wait);

        this.running = false;

        yield break;
    }
}
