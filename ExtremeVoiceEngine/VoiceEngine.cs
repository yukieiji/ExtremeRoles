using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using BepInEx.Unity.IL2CPP.Utils.Collections;

using ExtremeRoles.Module;

using ExtremeVoiceEngine.Command;
using ExtremeVoiceEngine.Interface;

namespace ExtremeVoiceEngine;

[Il2CppRegister]
public sealed class VoiceEngine : MonoBehaviour
{
    public bool IsWait { get; set; } = false;

    public static VoiceEngine? Instance { get; private set; }

    public ISpeakEngine? Engine { get; set; }

    private bool running = false;
    private Queue<string> textQueue = new Queue<string>();

    public VoiceEngine(IntPtr ptr) : base(ptr) { }

    internal static void CreateCommand()
    {
        CommandManager.Instance.AddCommand(
            "extremevoiceengine",
            new(new Parser(new Option("init", "EngineName", Option.Kind.Need, 'i')), Parse));
        CommandManager.Instance.AddAlias(
            "extremevoiceengine", "eve", "exve");
    }

    public static void Parse(Result? result)
    {
        if (Instance == null) { return; }
    }

    public void Awake()
    {
        Instance = this;
    }

    public void WaitExecute(Action act)
    {
        this.IsWait = true;
        act.Invoke();
        this.IsWait = false;
    }


    public void AddQueue(string text)
    {
        if (this.IsWait) { return; }
        ExtremeVoiceEnginePlugin.Logger.LogInfo($"Add TextToVoice Queue \nText:{text}");
        textQueue.Enqueue(text);
    }

    public void Destroy()
    {
        StopAllCoroutines();
        Engine?.Cancel();
    }

    public void FixedUpdate()
    {
        if (Engine is null ||
            running ||
            textQueue.Count == 0) { return; }

        string text = textQueue.Dequeue();
        StartCoroutine(coSpeek(text).WrapToIl2Cpp());
    }

    private IEnumerator coSpeek(string text)
    {
        if (Engine is null) { yield break; }

        running = true;

        yield return Engine.Speek(text);
        yield return new WaitForSeconds(Engine.Wait);

        running = false;

        yield break;
    }
}
