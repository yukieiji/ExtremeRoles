﻿using System;
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
    public static VoiceEngine? Instance { get; private set; }

    public ISpeaker? Speaker { get; set; }

    private bool running = false;
    private Queue<string> textQueue = new Queue<string>();

    public VoiceEngine(IntPtr ptr) : base(ptr) { }

    public void Awake()
    {
        Instance = this;
    }

    public void AddQueue(string text)
    {
        textQueue.Enqueue(text);
    }

    public void Destroy()
    {
        StopAllCoroutines();
        Speaker?.Cancel();
    }

    public void FixedUpdate()
    {
        if (Speaker is null ||
            running ||
            textQueue.Count == 0) { return; }

        string text = textQueue.Dequeue();
        StartCoroutine(coSpeek(text).WrapToIl2Cpp());
    }

    private IEnumerator coSpeek(string text)
    {
        if (Speaker is null) { yield break; }

        running = true;

        yield return Speaker.Speek(text);
        yield return new WaitForSeconds(Speaker.Wait);

        running = false;

        yield break;
    }
}
