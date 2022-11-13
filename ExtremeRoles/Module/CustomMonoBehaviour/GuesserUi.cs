using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

using ExtremeRoles.Performance;
using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;

namespace ExtremeRoles.Module.CustomMonoBehaviour
{
    [Il2CppRegister]
    public sealed class GuesserUi : MonoBehaviour
    {
        private ButtonWrapper buttonPrefab;
        private GridLayoutGroup layout;

        private TextMeshProUGUI title;

        private Image backGround;
        private Button closeButton;

        private ConfirmMenu confirmMenu;

        private bool isActive = false;
        private List<GuessBehaviour> infos = new List<GuessBehaviour>();
        private List<ButtonWrapper> buttons = new List<ButtonWrapper>();

        public GuesserUi(IntPtr ptr) : base(ptr) { }

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
            this.closeButton = trans.Find(
                "MenuBody/CloseButton").gameObject.GetComponent<Button>();

            this.confirmMenu = trans.Find(
                "MenuBody/ConfirmMenu").gameObject.GetComponent<ConfirmMenu>();
            this.isActive = true;

            this.confirmMenu.Awake();
            this.buttonPrefab.Awake();
            this.confirmMenu.ResetButtonAction();

            this.confirmMenu.SetCancelButtonClickAction(
                (UnityAction)(() =>
                {
                    this.isActive = true;
                    this.confirmMenu.gameObject.SetActive(false);
                }));
            this.confirmMenu.SetOkButtonClickAction(
                (UnityAction)(() =>
                {
                    this.isActive = false;
                    this.confirmMenu.gameObject.SetActive(false);
                    base.gameObject.SetActive(false);
                }));

            this.closeButton.onClick.RemoveAllListeners();
            this.closeButton.onClick.AddListener(
                (UnityAction)(() =>
                {
                    this.isActive = false;
                    this.confirmMenu.gameObject.SetActive(false);
                    base.gameObject.SetActive(false);
                }));
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

            if (this.isActive &&
                (
                    !meetingHud ||
                    meetingHud.state == MeetingHud.VoteStates.Results ||
                    Input.GetKeyDown(KeyCode.Escape)
                ))
            {
                base.gameObject.SetActive(false);
            }
        }

        public void SetTitle(string title)
        {
            this.title.text = title;
        }

        public void SetImage(Sprite sprite)
        {
            this.backGround.sprite = sprite;
        }

        public void InitButton(
            Action<GuessBehaviour.RoleInfo, byte> gussAction,
            List<GuessBehaviour.RoleInfo> guessRoleInfos)
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
                    (UnityAction)(() =>
                    {
                        this.isActive = false;
                        this.confirmMenu.gameObject.SetActive(true);
                        this.confirmMenu.SetMenuText(guess.GetConfirmText());
                        this.confirmMenu.SetOkButtonClickAction(
                            guess.GetGuessAction());
                    }));
                button.SetButtonText(guess.GetRoleName());
                this.infos.Add(guess);
                this.buttons.Add(button);
            }
        }

        public void SetTarget(byte targetPlayerId)
        {
            foreach (var info in this.infos)
            {
                info.SetTarget(targetPlayerId);
            }
        }

        private static void setMeetingObjectActive(bool active)
        {
            if (MeetingHud.Instance == null) { return; }

            FastDestroyableSingleton<HudManager>.Instance.Chat.gameObject.SetActive(active);

            foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates)
            {
                pva.gameObject.SetActive(active);
            }
        }
    }
}
