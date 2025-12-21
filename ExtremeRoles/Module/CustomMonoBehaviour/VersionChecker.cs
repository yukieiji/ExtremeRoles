using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using Hazel;
using TMPro;
using UnityEngine;

using ExtremeRoles.Performance.Il2Cpp;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class VersionChecker : MonoBehaviour
{
    private sealed class PlayerVersion(Version version, string name)
    {
        private readonly Dictionary<int, Version> version = new Dictionary<int, Version>();
        private readonly Version localVersion = new Version(version.Major, version.Minor, version.Build);
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

            this.version[id] = new Version(major, minor, build);
        }

        public void Deserialize(RPCOperator.RpcCaller writer)
        {
            int id = AmongUsClient.Instance.ClientId;
            writer.WritePackedInt(id);
            writer.WritePackedInt(this.localVersion.Major);
            writer.WritePackedInt(this.localVersion.Minor);
            writer.WritePackedInt(this.localVersion.Build);

			this.version[id] = this.localVersion;
        }

        private string getHostPlayerMessage()
        {
            this.builder.Clear();

            foreach (var client in
                AmongUsClient.Instance.allClients.GetFastEnumerator())
            {
				var character = client.Character;
                if (character == null ||
                    (
						character.TryGetComponent<DummyBehaviour>(out var dummyComponent) &&
                        dummyComponent.enabled
                    ) ||
					character.Data == null)
                {
                    continue;
                }

				string name = character.Data.PlayerName;
				if (string.IsNullOrEmpty(name))
				{
					continue;
				}

				string key = string.Empty;

				if (!(
                        this.version.TryGetValue(client.Id, out var clientVer) &&
                        clientVer is not null
                    ))
                {
					key = "errorNotInstalled";
                }
				else
				{
					int diff = localVersion.CompareTo(clientVer);
					if (diff > 0)
					{
						key = "errorOldInstalled";
					}
					else if (diff < 0)
					{
						key = "errorNewInstalled";
					}
				}
				if (string.IsNullOrEmpty(key))
				{
					continue;
				}
				this.builder.AppendLine($"{name}:  {Tr.GetString(key, modName)}");
			}
            return this.builder.ToString();
        }

        private string getLocalPlayerMessage()
        {
            if (this.version.TryGetValue(AmongUsClient.Instance.HostId, out var hostVer) &&
                hostVer is not null &&
                hostVer.CompareTo(this.localVersion) == 0)
            {
                return string.Empty;
            }
            return $"{Tr.GetString("errorDiffHostVersion", modName)}\n";
        }
    }

    public static bool IsCheckMiss
    {
        get
        {
            foreach (var version in allModVersion.Values)
            {
                string message = version.GetErrorMessage(true);
                if (!string.IsNullOrEmpty(message))
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
    private GameStartManager? mng;
    private ActionMapGlyphDisplay? display;
    private PassiveButton? button;
    private TextMeshPro? text;
    private Vector3 defaultPos;
    private const float kickTime = 60.0f;

	public VersionChecker(IntPtr ptr) : base(ptr) { }

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
		setUpVariable();
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
        if (LobbyBehaviour.Instance == null ||
			PlayerControl.LocalPlayer == null ||
			AmongUsClient.Instance == null)
        {
            return;
        }

		if (this.text == null ||
			this.button == null ||
			this.mng == null)
		{
			this.setUpVariable();
			return;
		}

        if (!this.isSend)
        {
            this.DeserializeLocalVersion();
            this.isSend = true;
        }
        this.builder.Clear();
        
		bool isHost = AmongUsClient.Instance.AmHost;

		string errorKey = isHost ? "errorCannotGameStart" : "autoDisconnectTo";
		this.builder.AppendLine(Tr.GetString(errorKey));
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
                    this.builder.ToString(),
					Mathf.CeilToInt(kickTime - this.timer));
            }
        }
        else
        {
            if (isHost)
            {
                bool isPlayerOk = this.mng.LastPlayerCount >= this.mng.MinPlayers;

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
		if (this.text != null)
		{
			this.text.gameObject.SetActive(false);
		}
    }

	private void setUpVariable()
	{
		this.mng = base.GetComponent<GameStartManager>();

		if (this.mng == null)
		{
			return;
		}

		this.display = this.mng.StartButtonGlyph;
		this.button = this.mng.StartButton;
		var go = HudManager.Instance.transform.Find("AnalyticsRecordingGO");
		if (go == null)
		{
			return;
		}
		if (!go.TryGetComponent<TextMeshPro>(out this.text))
		{
			return;
		}
		this.text.enableWordWrapping = false;
		this.text.alignment = TextAlignmentOptions.Center;
		this.text.GetComponent<RectTransform>().sizeDelta = new Vector2(
			5.0f, 5.5f);
		this.defaultPos = this.text.transform.localPosition;
	}
}
