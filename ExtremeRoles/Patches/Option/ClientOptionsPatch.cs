using HarmonyLib;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

using static ExtremeRoles.OptionsHolder;
using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;


namespace ExtremeRoles.Patches.Option
{

    [HarmonyPatch]
    public static class ClientOptionsPatch
    {
        private static SelectionBehaviour[] modOption = {
            new SelectionBehaviour(
                "streamerModeButton",
                () => ConfigParser.StreamerMode.Value = !ConfigParser.StreamerMode.Value,
                ConfigParser.StreamerMode.Value),
            new SelectionBehaviour(
                "ghostsSeeTasksButton",
                () => Client.GhostsSeeTask = ConfigParser.GhostsSeeTasks.Value = !ConfigParser.GhostsSeeTasks.Value,
                ConfigParser.GhostsSeeTasks.Value),
            new SelectionBehaviour(
                "ghostsSeeVotesButton",
                () => Client.GhostsSeeVote = ConfigParser.GhostsSeeVotes.Value = !ConfigParser.GhostsSeeVotes.Value,
                ConfigParser.GhostsSeeVotes.Value),
            new SelectionBehaviour("ghostsSeeRolesButton",
                () => Client.GhostsSeeRole = ConfigParser.GhostsSeeRoles.Value = !ConfigParser.GhostsSeeRoles.Value,
                ConfigParser.GhostsSeeRoles.Value),
            new SelectionBehaviour("showRoleSummaryButton",
                () => Client.ShowRoleSummary = ConfigParser.ShowRoleSummary.Value = !ConfigParser.ShowRoleSummary.Value,
                ConfigParser.ShowRoleSummary.Value),
        };

        private static GameObject popUp;
        private static TextMeshPro titleText;

        private static ToggleButtonBehaviour moreOptions;
        private static List<ToggleButtonBehaviour> modButtons;
        private static TextMeshPro titleTextTitle;

        private static ToggleButtonBehaviour buttonPrefab;
        private static Vector3? origin;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
        public static void MainMenuManagerStartPostfix(MainMenuManager __instance)
        {
            // Prefab for the title
            var tmp = __instance.Announcement.transform.Find("Title_Text").gameObject.GetComponent<TextMeshPro>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.transform.localPosition += Vector3.left * 0.2f;
            titleText = Object.Instantiate(tmp);
            Object.Destroy(titleText.GetComponent<TextTranslatorTMP>());
            titleText.gameObject.SetActive(false);
            Object.DontDestroyOnLoad(titleText);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
        public static void OptionsMenuBehaviourStartPostfix(OptionsMenuBehaviour __instance)
        {
            if (!__instance.CensorChatButton) { return; }

            if (!popUp)
            {
                createCustom(__instance);
            }

            if (!buttonPrefab)
            {
                buttonPrefab = Object.Instantiate(__instance.CensorChatButton);
                Object.DontDestroyOnLoad(buttonPrefab);
                buttonPrefab.name = "CensorChatPrefab";
                buttonPrefab.gameObject.SetActive(false);
            }

            setUpOptions();
            InitializeMoreButton(__instance);
        }

        public static void UpdateMenuTranslation()
        {
            if (titleTextTitle)
            {
                titleTextTitle.text = Helper.Translation.GetString("moreOptionsText");
            }
            if (moreOptions)
            {
                moreOptions.Text.text = Helper.Translation.GetString("modOptionsText");
            }
            for (int i = 0; i < modOption.Length; i++)
            {
                if (i >= modButtons.Count) { break; }
                modButtons[i].Text.text = Helper.Translation.GetString(modOption[i].Title);
            }
        }

        private static void createCustom(OptionsMenuBehaviour prefab)
        {
            popUp = Object.Instantiate(prefab.gameObject);
            Object.DontDestroyOnLoad(popUp);
            var transform = popUp.transform;
            var pos = transform.localPosition;
            pos.z = -810f;
            transform.localPosition = pos;

            Object.Destroy(popUp.GetComponent<OptionsMenuBehaviour>());
            foreach (var gObj in popUp.gameObject.getAllChilds())
            {
                if (gObj.name != "Background" && gObj.name != "CloseButton")
                    Object.Destroy(gObj);
            }

            popUp.SetActive(false);
        }

        private static void InitializeMoreButton(OptionsMenuBehaviour __instance)
        {
            moreOptions = Object.Instantiate(
                buttonPrefab,
                __instance.CensorChatButton.transform.parent);
            var transform = __instance.CensorChatButton.transform;
            origin ??= transform.localPosition;

            transform.localPosition = origin.Value + Vector3.left * 1.3f;
            moreOptions.transform.localPosition = origin.Value + Vector3.right * 1.3f;

            moreOptions.gameObject.SetActive(true);
            moreOptions.Text.text = Helper.Translation.GetString("modOptionsText");
            var moreOptionsButton = moreOptions.GetComponent<PassiveButton>();
            moreOptionsButton.OnClick = new ButtonClickedEvent();
            moreOptionsButton.OnClick.AddListener((Action)(() =>
            {
                if (!popUp) { return; }

                if (__instance.transform.parent && __instance.transform.parent == HudManager.Instance.transform)
                {
                    popUp.transform.SetParent(HudManager.Instance.transform);
                    popUp.transform.localPosition = new Vector3(0, 0, -800f);
                }
                else
                {
                    popUp.transform.SetParent(null);
                    Object.DontDestroyOnLoad(popUp);
                }

                checkSetTitle();
                refreshOpen();
            }));
        }

        private static void refreshOpen()
        {
            popUp.gameObject.SetActive(false);
            popUp.gameObject.SetActive(true);
            setUpOptions();
        }

        private static void checkSetTitle()
        {
            if (!popUp || popUp.GetComponentInChildren<TextMeshPro>() || !titleText) return;

            var title = titleTextTitle = Object.Instantiate(titleText, popUp.transform);
            title.GetComponent<RectTransform>().localPosition = Vector3.up * 2.3f;
            title.gameObject.SetActive(true);
            title.text = Helper.Translation.GetString("moreOptionsText");
            title.name = "TitleText";
        }

        private static void setUpOptions()
        {
            if (popUp.transform.GetComponentInChildren<ToggleButtonBehaviour>()) return;

            modButtons = new List<ToggleButtonBehaviour>();

            for (var i = 0; i < modOption.Length; i++)
            {
                var info = modOption[i];

                var button = Object.Instantiate(buttonPrefab, popUp.transform);
                var pos = new Vector3(i % 2 == 0 ? -1.17f : 1.17f, 1.3f - i / 2 * 0.8f, -.5f);

                var transform = button.transform;
                transform.localPosition = pos;

                button.onState = info.DefaultValue;
                button.Background.color = button.onState ? Color.green : Palette.ImpostorRed;

                button.Text.text = Helper.Translation.GetString(info.Title);
                button.Text.fontSizeMin = button.Text.fontSizeMax = 2.2f;
                button.Text.font = Object.Instantiate(titleText.font);
                button.Text.GetComponent<RectTransform>().sizeDelta = new Vector2(2, 2);

                button.name = info.Title.Replace(" ", "") + "Toggle";
                button.gameObject.SetActive(true);

                var passiveButton = button.GetComponent<PassiveButton>();
                var colliderButton = button.GetComponent<BoxCollider2D>();

                colliderButton.size = new Vector2(2.2f, .7f);

                passiveButton.OnClick = new ButtonClickedEvent();
                passiveButton.OnMouseOut = new UnityEvent();
                passiveButton.OnMouseOver = new UnityEvent();

                passiveButton.OnClick.AddListener((Action)(() =>
                {
                    button.onState = info.OnClick();
                    button.Background.color = button.onState ? Color.green : Palette.ImpostorRed;
                }));

                passiveButton.OnMouseOver.AddListener((Action)(() => button.Background.color = new Color32(34, 139, 34, byte.MaxValue)));
                passiveButton.OnMouseOut.AddListener((Action)(() => button.Background.color = button.onState ? Color.green : Palette.ImpostorRed));

                foreach (var spr in button.gameObject.GetComponentsInChildren<SpriteRenderer>())
                {
                    spr.size = new Vector2(2.2f, .7f);
                }
                modButtons.Add(button);
            }
        }

        private static IEnumerable<GameObject> getAllChilds(this GameObject Go)
        {
            for (var i = 0; i < Go.transform.childCount; ++i)
            {
                yield return Go.transform.GetChild(i).gameObject;
            }
        }

        private class SelectionBehaviour
        {
            public string Title;
            public Func<bool> OnClick;
            public bool DefaultValue;

            public SelectionBehaviour(string title, Func<bool> onClick, bool defaultValue)
            {
                Title = title;
                OnClick = onClick;
                DefaultValue = defaultValue;
            }
        }
    }

}
