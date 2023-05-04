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

    private const string cmd = "extremevoiceengine";

    public VoiceEngine(IntPtr ptr) : base(ptr) { }

    internal static void CreateCommand()
    {
        CommandManager.Instance.AddCommand(
            cmd,
            new(new Parser(new Option("init", "EngineName", Option.Kind.Need, 'i')), Parse));
        CommandManager.Instance.AddAlias(
            cmd, "eve", "exve");
    }

    public static void Parse(Result? result)
    {
        if (Instance == null || result is null) { return; }

        string value =  result.GetOptionValue("init");
        switch (value)
        {
            case "vv":
            case "VV":
            case "VoiceVox":
            case "voicevox":
                var engine = new VoiceVox.VoiceVoxEngine();
                var parm = new VoiceVox.VoiceVoxParameter();
                engine.SetParameter(parm);
                engine.Wait = 2.0f;
                Instance.Engine = engine;
                break;
            default:
                break;
        }

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
