﻿using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;

using ExtremeRoles.Module;
using ExtremeRoles.Performance;
using ExtremeRoles.Extension.Controller;

using ExtremeVoiceEngine.Command;
using ExtremeVoiceEngine.Extension;
using ExtremeVoiceEngine.Interface;

namespace ExtremeVoiceEngine;

[Il2CppRegister]
public sealed class VoiceEngine : MonoBehaviour
{
    public bool IsWait { get; set; } = false;

    public static VoiceEngine? Instance { get; private set; }

    [HideFromIl2Cpp]
    public ISpeakEngine? Engine { get; set; }

    public enum EngineType : int
    {
        None = -1,
        VoiceVox
    }

#pragma warning disable CS8618
    private BepInEx.Configuration.ConfigEntry<int> curEngine;
    public VoiceEngine(IntPtr ptr) : base(ptr) { }
#pragma warning restore CS8618


    private bool running = false;
    private Queue<string> textQueue = new Queue<string>();
    private Dictionary<EngineType, ISpeakEngine> engines = new Dictionary<EngineType, ISpeakEngine>();

    public const string Cmd = "extremevoiceengine";

    public static void Parse(Result? result)
    {
        if (Instance == null || result is null) { return; }

        var chat = HudManager.Instance.Chat;
        var trans = TranslationController.Instance;
        string value =  result.GetOptionValue("init");
        EngineType engine;
        switch (value)
        {
            case "vv":
            case "VV":
            case "VOICEVOX":
            case "Voicevox":
            case "VoiceVox":
            case "voiceVox":
            case "voicevox":
                engine = EngineType.VoiceVox;
                break;
            case "none":
            case "NONE":
            case "None":
                Instance.Engine = null;
                Instance.curEngine.Value = (int)EngineType.None;
                chat.AddLocalChat(trans.GetString("disableEngine"));
                return;
            default:
                chat.AddLocalChat(trans.GetString("InvalidedEngine"));
                return;
        }

        Instance.Engine = Instance.engines[engine];
        Instance.curEngine.Value = (int)engine;

        string message = Instance.Engine.IsValid() ?
            trans.GetString("EngineSetTo", parts: engine.ToString()) : trans.GetString("InvalidedEngine");

        chat.AddLocalChat(message);
    }

    public void Awake()
    {
        Instance = this;
        CommandManager.Instance.AddCommand(
            Cmd,
            new(new Parser(new Option("init", 'i', Option.Kind.Need)), Parse));
        CommandManager.Instance.AddAlias(
            Cmd, "eve", "exve");

        this.curEngine = ExtremeVoiceEnginePlugin.Instance.Config.Bind(
            "VoiceEngine", "EngineType", -1);

        setUpEngine<VoiceVox.VoiceVoxEngine, VoiceVox.VoiceVoxParameter>(EngineType.VoiceVox);

        EngineType engineType = (EngineType)this.curEngine.Value;
        if (engineType != EngineType.None)
        {
            this.Engine = this.engines[engineType];
            string engineState = this.Engine.IsValid() ? "active" : "diactive";
            ExtremeVoiceEnginePlugin.Logger.LogInfo($"InitTo:{engineType}, status:{engineState}");
        }

    }

    [HideFromIl2Cpp]
    public void WaitExecute(Action act)
    {
        this.IsWait = true;
        act.Invoke();
        this.IsWait = false;
    }

    public void AddQueue(string text)
    {
        if (Engine is null || this.IsWait) { return; }
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
        this.StartCoroutine(this.coSpeek(text));
    }


    public override string ToString()
    {
        if (this.Engine is null)
        {
            return TranslationControllerExtension.GetString("noEngine");
        }

        string activeStr = this.Engine.IsValid() ?　"engineActive" : "engineDeactive";

        return TranslationControllerExtension.GetString(
            "eveEngineState",
            TranslationControllerExtension.GetString(activeStr),
            this.Engine?.ToString());
    }

    [HideFromIl2Cpp]
    private IEnumerator coSpeek(string text)
    {
        if (Engine is null) { yield break; }

        running = true;

        yield return Engine.Speek(text);
        yield return new WaitForSeconds(Engine.Wait);

        running = false;

        yield break;
    }

    [HideFromIl2Cpp]
    private void setUpEngine<T, W>(EngineType type)
        where W : IEngineParameter
        where T : IParametableEngine<W>, new()
    {
        T engine = new T();
        engine.CreateCommand();
        this.engines.Add(type, engine);
    }
}
