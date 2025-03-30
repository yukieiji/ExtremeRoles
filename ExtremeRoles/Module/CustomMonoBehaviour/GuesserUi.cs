using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

using Il2CppInterop.Runtime.Attributes;

using ExtremeRoles.Extension.UnityEvents;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;
using ExtremeRoles.Module.SystemType;

using ExtremeRoles.Module.Interface;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class GuesserUi : MonoBehaviour
{
#pragma warning disable CS8618
	private ButtonWrapper buttonPrefab;
    private GridLayoutGroup layout;

    private TextMeshProUGUI title;
    private TextMeshProUGUI info;

    private Image backGround;
    private Button closeButton;

    private ConfirmMenu confirmMenu;

    public GuesserUi(IntPtr ptr) : base(ptr) { }
#pragma warning restore CS8618

	private bool isActive = false;
	private List<GuessBehaviour> infos = new List<GuessBehaviour>();
	private List<ButtonWrapper> buttons = new List<ButtonWrapper>();
	private PlayerControl? targetPlayer = null;

	public void Awake()
    {
        Transform trans = base.transform;

        this.buttonPrefab = trans.Find(
            "MenuBody/GuesserButton").gameObject.GetComponent<ButtonWrapper>();
        this.backGround = trans.Find(
            "MenuBody/Background").gameObject.GetComponent<Image>();
        this.layout = trans.Find(
            "MenuBody/ButtonScroll/Viewport/Content").gameObject.GetComponent<GridLayoutGroup>();
        this.title = trans.Find(
            "MenuBody/Title").gameObject.GetComponent<TextMeshProUGUI>();
        this.info = trans.Find(
            "MenuBody/GuesserInfo").gameObject.GetComponent<TextMeshProUGUI>();

        this.closeButton = trans.Find(
            "MenuBody/CloseButton").gameObject.GetComponent<Button>();

        this.confirmMenu = trans.Find(
            "MenuBody/ConfirmMenu").gameObject.GetComponent<ConfirmMenu>();
        this.isActive = true;

        this.confirmMenu.Awake();
        this.buttonPrefab.Awake();

        this.confirmMenu.ResetButtonAction();
        this.confirmMenu.SetCancelButtonClickAction(
            () =>
            {
                this.isActive = true;
                this.confirmMenu.gameObject.SetActive(false);
            });

        this.closeButton.onClick.RemoveAllListeners();
        this.closeButton.onClick.AddListener(
            () =>
            {
                this.isActive = false;
                this.confirmMenu.gameObject.SetActive(false);
                base.gameObject.SetActive(false);
            });
        this.confirmMenu.gameObject.SetActive(false);
    }

    public void OnEnable()
    {
        this.isActive = true;
        setMeetingObjectActive(false);
    }
    public void OnDisable()
    {
        this.confirmMenu.gameObject.SetActive(false);
        setMeetingObjectActive(true);
    }

    public void Update()
    {
        var meetingHud = MeetingHud.Instance;
		PlayerControl localPlayer = PlayerControl.LocalPlayer;

		if (this.isActive &&
            (
                !meetingHud ||
				meetingHud.state == MeetingHud.VoteStates.Results ||
				Input.GetKeyDown(KeyCode.Escape) ||
				this.targetPlayer == null ||
				this.targetPlayer.Data == null ||
				this.targetPlayer.Data.IsDead ||
				this.targetPlayer.Data.Disconnected ||
				localPlayer == null ||
				localPlayer.Data == null ||
				localPlayer.Data.IsDead ||
				localPlayer.Data.Disconnected
            ))
        {
            base.gameObject.SetActive(false);
        }
    }

    public void SetTitle(string title)
    {
        this.title.text = title;
    }

    public void SetInfo(string newInfo)
    {
        this.info.text = newInfo;
    }

    public void SetImage(Sprite sprite)
    {
        this.backGround.sprite = sprite;
    }

    [HideFromIl2Cpp]
    public void InitButton(
        Action<GuessBehaviour.RoleInfo, byte> gussAction,
        IEnumerable<GuessBehaviour.RoleInfo> guessRoleInfos)
    {
        this.infos.Clear();
        this.buttons.Clear();
        foreach (var roleInfo in guessRoleInfos)
        {
            ButtonWrapper button = Instantiate(
                this.buttonPrefab,
                this.layout.transform);
            button.gameObject.SetActive(true);
            GuessBehaviour guess = button.gameObject.AddComponent<GuessBehaviour>();
            guess.Create(roleInfo, gussAction);
            button.SetButtonClickAction(
                () =>
                {
                    this.isActive = false;
                    this.confirmMenu.gameObject.SetActive(true);
                    this.confirmMenu.SetMenuText(
                        Tr.GetString(
							"guesserConfirmMenu",
                            guess.GetPlayerName(),
                            guess.GetRoleName()));

                    // Okボタンの処理は毎回変える
                    this.confirmMenu.ResetOkButtonAction();
                    this.confirmMenu.SetOkButtonClickAction(
                        () =>
                        {
                            this.isActive = false;
                            this.confirmMenu.gameObject.SetActive(false);
                            base.gameObject.SetActive(false);
                        });
                    this.confirmMenu.SetOkButtonClickAction(
                        guess.GetGuessAction());
                });
            button.SetButtonText(guess.GetRoleName());
            this.infos.Add(guess);
            this.buttons.Add(button);
        }
    }

    public void SetTarget(byte targetPlayerId)
    {
		this.targetPlayer = Helper.Player.GetPlayerControlById(targetPlayerId);
        foreach (var info in this.infos)
        {
            info.SetTarget(targetPlayerId);
        }
    }

    private static void setMeetingObjectActive(bool active)
    {
        if (MeetingHud.Instance == null) { return; }

        HudManager.Instance.Chat.gameObject.SetActive(active);
        MeetingHud.Instance.SkipVoteButton.gameObject.SetActive(active);

        foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates)
        {
            pva.gameObject.SetActive(active);
        }

		if (ExtremeSystemTypeManager.Instance.TryGet<IRaiseHandSystem>(
				ExtremeSystemType.RaiseHandSystem, out var system))
		{
			system.RaiseHandButtonSetActive(active);
		}
    }
}
