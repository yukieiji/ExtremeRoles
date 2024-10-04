﻿using Hazel;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using UnityEngine;

using ExtremeRoles.Performance.Il2Cpp;
using TMPro;
using ExtremeRoles.Performance;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class VersionChecker : MonoBehaviour
{
    private sealed class PlayerVersion(Version version, string name)
    {
        private readonly Dictionary<int, Version> version = new Dictionary<int, Version>();
        private readonly Version localVersion = version;
        private readonly string modName = name;
        private readonly StringBuilder builder = new StringBuilder();

        public string GetErrorMessage(bool isHost)
            => isHost ? getHostPlayerMessage() : getLocalPlayerMessage();

        public void Serialize(MessageReader reader)
        {
            int id = reader.ReadPackedInt32();
            int major = reader.ReadPackedInt32();
            int minor = reader.ReadPackedInt32();
            int build = reader.ReadPackedInt32();
            int revision = reader.ReadPackedInt32();

            version[id] = new Version(major, minor, build, revision);
        }

        public void Deserialize(RPCOperator.RpcCaller writer)
        {
            int id = AmongUsClient.Instance.ClientId;
            writer.WritePackedInt(id);
            writer.WritePackedInt(localVersion.Major);
            writer.WritePackedInt(localVersion.Minor);
            writer.WritePackedInt(localVersion.Build);
            writer.WritePackedInt(localVersion.Revision);

            version[id] = localVersion;
        }

        private string getHostPlayerMessage()
        {
            this.builder.Clear();

            foreach (InnerNet.ClientData client in
                AmongUsClient.Instance.allClients.GetFastEnumerator())
            {
                if (client.Character == null ||
                    (
                        client.Character.TryGetComponent<DummyBehaviour>(out var dummyComponent) &&
                        dummyComponent.enabled
                    ))
                {
                    continue;
                }

                if (!
                    (
                        this.version.TryGetValue(client.Id, out var clientVer) &&
                        clientVer is not null
                    ))
                {
                    this.builder.AppendLine($"{client.Character.Data.PlayerName}:  {Tr.GetString("errorNotInstalled", modName)}");
                }
                else
                {
                    int diff = localVersion.CompareTo(clientVer);
                    if (diff > 0)
                    {
                        this.builder.AppendLine($"{client.Character.Data.PlayerName}:  {Tr.GetString("errorOldInstalled", modName)}");
                    }
                    else if (diff < 0)
                    {
                        this.builder.AppendLine(
                            $"{client.Character.Data.PlayerName}:  {Tr.GetString("errorNewInstalled", modName)}");
                    }
                }
            }
            return this.builder.ToString();
        }

        private string getLocalPlayerMessage()
        {
            if (version.TryGetValue(AmongUsClient.Instance.HostId, out var hostVer) &&
                hostVer is not null &&
                hostVer.CompareTo(localVersion) == 0)
            {
                return string.Empty;
            }
            return $"{Tr.GetString("errorDiffHostVersion", modName)}\n";
        }
    }

    public bool IsError
    {
        get
        {
            foreach (var version in allModVersion.Values)
            {
                string message = version.GetErrorMessage(true);
                if (string.IsNullOrEmpty(message))
                {
                    return true;
                }
            }
            return false;
        }
    }

    private static Dictionary<uint, PlayerVersion> allModVersion = new Dictionary<uint, PlayerVersion>();

    private bool isSend = false;
    private StringBuilder builder = new StringBuilder();
    private float timer = 0.0f;
    private GameStartManager mng;
    private ActionMapGlyphDisplay display;
    private PassiveButton button;
    private TextMeshPro text;
    private Vector3 defaultPos;
    private const float kickTime = 60.0f;

#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。'required' 修飾子を追加するか、Null 許容として宣言することを検討してください。
	public VersionChecker(IntPtr ptr) : base(ptr) { }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。'required' 修飾子を追加するか、Null 許容として宣言することを検討してください。

	public static void RegisterAssembly(Assembly assm, uint id)
    {
        var name = assm.GetName();
        if (name.Version is null)
        {
            return;
        }
        string? modName = name.Name;
        if (string.IsNullOrEmpty(modName))
        {
            return;
        }
        allModVersion.Add(id, new PlayerVersion(name.Version, modName));
    }

    public void Awake()
    {
        this.mng = base.GetComponent<GameStartManager>();

        this.display = this.mng.StartButtonGlyph;
        this.button = this.mng.StartButton;
        this.text = FastDestroyableSingleton<HudManager>.Instance.transform.Find("AnalyticsRecordingGO").GetComponent<TextMeshPro>();
        this.text.enableWordWrapping = false;
        this.text.alignment = TextAlignmentOptions.Center;
        this.text.GetComponent<RectTransform>().sizeDelta = new Vector2(
            5.0f, 5.5f);
        this.defaultPos = this.text.transform.localPosition;
    }

    public void DeserializeLocalVersion()
    {
        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.ShareVersion))
        {
            caller.WritePackedInt(allModVersion.Count);

            foreach (var (id, version) in allModVersion)
            {
                caller.WritePackedUInt(id);
                version.Deserialize(caller);
            }
        }
        this.timer = 0.0f;
    }

    public static void SerializeLocalVersion(MessageReader reader)
    {
        int num = reader.ReadPackedInt32();
        for (int i = 0; i < num; ++i)
        {
            uint id = reader.ReadPackedUInt32();
            if (allModVersion.TryGetValue(id, out var version) &&
                version is not null)
            {
                version.Serialize(reader);
            }
        }
    }

    public void Update()
    {
        if (PlayerControl.LocalPlayer == null)
        {
            return;
        }

        if (!isSend)
        {
            this.DeserializeLocalVersion();
            this.isSend = true;
        }
        this.builder.Clear();
        bool isHost = AmongUsClient.Instance.AmHost;
        if (isHost)
        {
            this.builder
                .AppendLine(Tr.GetString("errorCannotGameStart"));
        }
        else
        {
            this.builder
                .AppendLine(Tr.GetString("autoDisconnectTo"));
        }

        int curLength = this.builder.Length;

        foreach (var version in allModVersion.Values)
        {
            string message = version.GetErrorMessage(isHost);
            if (string.IsNullOrEmpty(message))
            {
                continue;
            }
            this.builder.Append(message);
        }

        bool isBlock = curLength != this.builder.Length;

        this.text.gameObject.SetActive(false);

        if (isBlock)
        {
            this.text.gameObject.SetActive(true);
            this.text.transform.localPosition =
                mng.StartButton.transform.localPosition + new Vector3(-1.0f, 1.25f);;

            if (isHost)
            {
                if (this.display != null)
                {
                    this.display.SetColor(Palette.DisabledClear);
                }
                this.button.SetButtonEnableState(false);

                this.text.text = this.builder.ToString();
            }
            else
            {
                this.timer += Time.deltaTime;
                if (this.timer > kickTime)
                {
                    this.timer = 0;
                    AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
                    SceneChanger.ChangeScene("MainMenu");
                }
                this.text.text = string.Format(
                    this.builder.ToString(), Mathf.CeilToInt(kickTime - this.timer));
            }
        }
        else
        {
            if (isHost)
            {
                bool isPlayerOk = mng.LastPlayerCount >= mng.MinPlayers;

                if (this.display != null)
                {
                    this.display.SetColor(isPlayerOk ?
                        Palette.EnabledColor : Palette.DisabledClear);
                }

                this.button.SetButtonEnableState(isPlayerOk);
            }
            this.text.transform.localPosition = defaultPos;
            this.text.text = "Analytics Active";
        }
    }
    public void OnDestroy()
    {
        this.text.gameObject.SetActive(false);
    }
}
