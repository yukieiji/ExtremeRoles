using HarmonyLib;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;


namespace ExtremeRoles.Patches.Option
{

    [HarmonyPatch]
    public static class ClientOptionsPatch
    {
        private static SelectionBehaviour[] ModOption = {
            new SelectionBehaviour(
                "streamerModeButton",
                () => ExtremeRolesPlugin.StreamerMode.Value = !ExtremeRolesPlugin.StreamerMode.Value,
                ExtremeRolesPlugin.StreamerMode.Value),
            new SelectionBehaviour(
                "ghostsSeeTasksButton",
                () => MapOption.GhostsSeeTasks = ExtremeRolesPlugin.GhostsSeeTasks.Value = !ExtremeRolesPlugin.GhostsSeeTasks.Value,
                ExtremeRolesPlugin.GhostsSeeTasks.Value),
            new SelectionBehaviour(
                "ghostsSeeVotesButton",
                () => MapOption.GhostsSeeVotes = ExtremeRolesPlugin.GhostsSeeVotes.Value = !ExtremeRolesPlugin.GhostsSeeVotes.Value,
                ExtremeRolesPlugin.GhostsSeeVotes.Value),
            new SelectionBehaviour("ghostsSeeRolesButton",
                () => MapOption.GhostsSeeRoles = ExtremeRolesPlugin.GhostsSeeRoles.Value = !ExtremeRolesPlugin.GhostsSeeRoles.Value,
                ExtremeRolesPlugin.GhostsSeeRoles.Value),
            new SelectionBehaviour("showRoleSummaryButton",
                () => MapOption.ShowRoleSummary = ExtremeRolesPlugin.ShowRoleSummary.Value = !ExtremeRolesPlugin.ShowRoleSummary.Value,
                ExtremeRolesPlugin.ShowRoleSummary.Value),
        };

        private static GameObject PopUp;
        private static TextMeshPro TitleText;

        private static ToggleButtonBehaviour MoreOptions;
        private static List<ToggleButtonBehaviour> ModButtons;
        private static TextMeshPro TitleTextTitle;

        private static ToggleButtonBehaviour ButtonPrefab;
        private static Vector3? Origin;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
        public static void MainMenuManagerStartPostfix(MainMenuManager __instance)
        {
            // Prefab for the title
            var tmp = __instance.Announcement.transform.Find("Title_Text").gameObject.GetComponent<TextMeshPro>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.transform.localPosition += Vector3.left * 0.2f;
            TitleText = Object.Instantiate(tmp);
            Object.Destroy(TitleText.GetComponent<TextTranslatorTMP>());
            TitleText.gameObject.SetActive(false);
            Object.DontDestroyOnLoad(TitleText);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
        public static void OptionsMenuBehaviourStartPostfix(OptionsMenuBehaviour __instance)
        {
            if (!__instance.CensorChatButton) { return; }

            if (!PopUp)
            {
                CreateCustom(__instance);
            }

            if (!ButtonPrefab)
            {
                ButtonPrefab = Object.Instantiate(__instance.CensorChatButton);
                Object.DontDestroyOnLoad(ButtonPrefab);
                ButtonPrefab.name = "CensorChatPrefab";
                ButtonPrefab.gameObject.SetActive(false);
            }

            SetUpOptions();
            InitializeMoreButton(__instance);
        }

        private static void CreateCustom(OptionsMenuBehaviour prefab)
        {
            PopUp = Object.Instantiate(prefab.gameObject);
            Object.DontDestroyOnLoad(PopUp);
            var transform = PopUp.transform;
            var pos = transform.localPosition;
            pos.z = -810f;
            transform.localPosition = pos;

            Object.Destroy(PopUp.GetComponent<OptionsMenuBehaviour>());
            foreach (var gObj in PopUp.gameObject.GetAllChilds())
            {
                if (gObj.name != "Background" && gObj.name != "CloseButton")
                    Object.Destroy(gObj);
            }

            PopUp.SetActive(false);
        }

        private static void InitializeMoreButton(OptionsMenuBehaviour __instance)
        {
            MoreOptions = Object.Instantiate(ButtonPrefab, __instance.CensorChatButton.transform.parent);
            var transform = __instance.CensorChatButton.transform;
            Origin ??= transform.localPosition;

            transform.localPosition = Origin.Value + Vector3.left * 1.3f;
            MoreOptions.transform.localPosition = Origin.Value + Vector3.right * 1.3f;

            MoreOptions.gameObject.SetActive(true);
            MoreOptions.Text.text = Helper.Translation.GetString("modOptionsText");
            var moreOptionsButton = MoreOptions.GetComponent<PassiveButton>();
            moreOptionsButton.OnClick = new ButtonClickedEvent();
            moreOptionsButton.OnClick.AddListener((Action)(() =>
            {
                if (!PopUp) { return; }

                if (__instance.transform.parent && __instance.transform.parent == HudManager.Instance.transform)
                {
                    PopUp.transform.SetParent(HudManager.Instance.transform);
                    PopUp.transform.localPosition = new Vector3(0, 0, -800f);
                }
                else
                {
                    PopUp.transform.SetParent(null);
                    Object.DontDestroyOnLoad(PopUp);
                }

                CheckSetTitle();
                RefreshOpen();
            }));
        }

        private static void RefreshOpen()
        {
            PopUp.gameObject.SetActive(false);
            PopUp.gameObject.SetActive(true);
            SetUpOptions();
        }

        private static void CheckSetTitle()
        {
            if (!PopUp || PopUp.GetComponentInChildren<TextMeshPro>() || !TitleText) return;

            var title = TitleTextTitle = Object.Instantiate(TitleText, PopUp.transform);
            title.GetComponent<RectTransform>().localPosition = Vector3.up * 2.3f;
            title.gameObject.SetActive(true);
            title.text = Helper.Translation.GetString("moreOptionsText");
            title.name = "TitleText";
        }

        private static void SetUpOptions()
        {
            if (PopUp.transform.GetComponentInChildren<ToggleButtonBehaviour>()) return;

            ModButtons = new List<ToggleButtonBehaviour>();

            for (var i = 0; i < ModOption.Length; i++)
            {
                var info = ModOption[i];

                var button = Object.Instantiate(ButtonPrefab, PopUp.transform);
                var pos = new Vector3(i % 2 == 0 ? -1.17f : 1.17f, 1.3f - i / 2 * 0.8f, -.5f);

                var transform = button.transform;
                transform.localPosition = pos;

                button.onState = info.DefaultValue;
                button.Background.color = button.onState ? Color.green : Palette.ImpostorRed;

                button.Text.text = Helper.Translation.GetString(info.Title);
                button.Text.fontSizeMin = button.Text.fontSizeMax = 2.2f;
                button.Text.font = Object.Instantiate(TitleText.font);
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
                ModButtons.Add(button);
            }
        }

        private static IEnumerable<GameObject> GetAllChilds(this GameObject Go)
        {
            for (var i = 0; i < Go.transform.childCount; ++i)
            {
                yield return Go.transform.GetChild(i).gameObject;
            }
        }

        public static void UpdateMenuTranslation()
        {
            if (TitleTextTitle)
            {
                TitleTextTitle.text = Helper.Translation.GetString("moreOptionsText");
            }
            if (MoreOptions)
            {
                MoreOptions.Text.text = Helper.Translation.GetString("modOptionsText");
            }
            for (int i = 0; i < ModOption.Length; i++)
            {
                if (i >= ModButtons.Count) { break; }
                ModButtons[i].Text.text = Helper.Translation.GetString(ModOption[i].Title);
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
