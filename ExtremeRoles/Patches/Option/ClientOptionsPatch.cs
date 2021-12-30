using System;
using System.Collections.Generic;

using HarmonyLib;

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Twitch;

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
            new SelectionBehaviour(
                "ghostsSeeRolesButton",
                () => Client.GhostsSeeRole = ConfigParser.GhostsSeeRoles.Value = !ConfigParser.GhostsSeeRoles.Value,
                ConfigParser.GhostsSeeRoles.Value),
            new SelectionBehaviour(
                "showRoleSummaryButton",
                () => Client.ShowRoleSummary = ConfigParser.ShowRoleSummary.Value = !ConfigParser.ShowRoleSummary.Value,
                ConfigParser.ShowRoleSummary.Value),
        };

        private static GameObject popUp;
        private static TextMeshPro titleText;

        private static ToggleButtonBehaviour moreOptionButton;
        private static List<ToggleButtonBehaviour> modOptionButton;

        private static ToggleButtonBehaviour importButton;
        private static ToggleButtonBehaviour exportButton;

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
            initializeMoreButton(__instance);
        }

        public static void UpdateMenuTranslation()
        {
            if (titleTextTitle)
            {
                titleTextTitle.text = Helper.Translation.GetString("moreOptionsText");
            }
            if (moreOptionButton)
            {
                moreOptionButton.Text.text = Helper.Translation.GetString("modOptionsText");
            }
            for (int i = 0; i < modOption.Length; i++)
            {
                if (i >= modOptionButton.Count) { break; }
                modOptionButton[i].Text.text = Helper.Translation.GetString(modOption[i].Title);
            }

            importButton.Text.text = Helper.Translation.GetString("csvImport");
            exportButton.Text.text = Helper.Translation.GetString("csvExport");

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
                {
                    Object.Destroy(gObj);
                }
            }

            popUp.SetActive(false);
        }

        private static void initializeMoreButton(OptionsMenuBehaviour __instance)
        {
            moreOptionButton = Object.Instantiate(
                buttonPrefab,
                __instance.CensorChatButton.transform.parent);
            var transform = __instance.CensorChatButton.transform;
            origin ??= transform.localPosition;

            transform.localPosition = origin.Value + Vector3.left * 1.3f;
            moreOptionButton.transform.localPosition = origin.Value + Vector3.right * 1.3f;

            moreOptionButton.gameObject.SetActive(true);
            moreOptionButton.Text.text = Helper.Translation.GetString("modOptionsText");
            var moreOptionsButton = moreOptionButton.GetComponent<PassiveButton>();
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
            if (!popUp || popUp.GetComponentInChildren<TextMeshPro>() || !titleText) { return; }

            var title = titleTextTitle = Object.Instantiate(titleText, popUp.transform);
            title.GetComponent<RectTransform>().localPosition = Vector3.up * 2.3f;
            title.gameObject.SetActive(true);
            title.text = Helper.Translation.GetString("moreOptionsText");
            title.name = "TitleText";
        }

        private static void setUpOptions()
        {
            if (popUp.transform.GetComponentInChildren<ToggleButtonBehaviour>()) { return; }

            modOptionButton = new List<ToggleButtonBehaviour>();

            for (var i = 0; i < modOption.Length; i++)
            {
                var info = modOption[i];

                var button = Object.Instantiate(buttonPrefab, popUp.transform);
                button.transform.localPosition = new Vector3(
                    i % 2 == 0 ? -1.17f : 1.17f,
                    1.3f - i / 2 * 0.8f,
                    -.5f);

                button.onState = info.DefaultValue;
                button.Background.color = button.onState ? Color.green : Palette.ImpostorRed;

                button.Text.text = Helper.Translation.GetString(info.Title);
                button.Text.fontSizeMin = button.Text.fontSizeMax = 2.2f;
                button.Text.font = Object.Instantiate(titleText.font);
                button.Text.GetComponent<RectTransform>().sizeDelta = new Vector2(2, 2);

                button.gameObject.layer = 8;
                button.name = info.Title.Replace(" ", "") + "Toggle";
                button.gameObject.SetActive(true);
                button.gameObject.transform.SetAsFirstSibling();

                var passiveButton = button.GetComponent<PassiveButton>();
                var colliderButton = button.GetComponent<BoxCollider2D>();

                colliderButton.gameObject.layer = 8;
                passiveButton.gameObject.layer = 8;

                colliderButton.size = new Vector2(2.2f, .7f);

                passiveButton.OnClick = new ButtonClickedEvent();
                passiveButton.OnMouseOut = new UnityEvent();
                passiveButton.OnMouseOver = new UnityEvent();

                passiveButton.OnClick.AddListener((Action)(() =>
                {
                    button.onState = info.OnClick();
                    button.Background.color = button.onState ? Color.green : Palette.ImpostorRed;
                }));

                passiveButton.OnMouseOver.AddListener(
                    (Action)(() => button.Background.color = new Color32(34, 139, 34, byte.MaxValue)));
                passiveButton.OnMouseOut.AddListener(
                    (Action)(() => button.Background.color = button.onState ? Color.green : Palette.ImpostorRed));

                foreach (var spr in button.gameObject.GetComponentsInChildren<SpriteRenderer>())
                {
                    spr.size = new Vector2(2.2f, .7f);
                }
                modOptionButton.Add(button);
            }

            importButton = UnityEngine.Object.Instantiate(
                buttonPrefab, popUp.transform);
            exportButton = UnityEngine.Object.Instantiate(
                buttonPrefab, popUp.transform);

            importButton.transform.localPosition = new Vector3(
                -1.35f, -2.0f, -.5f);
            exportButton.transform.localPosition = new Vector3(
                1.35f, -2.0f, -.5f);

            importButton.Text.text = Helper.Translation.GetString("csvImport");
            importButton.Background.color = Color.green;
            importButton.Text.fontSizeMin = importButton.Text.fontSizeMax = 2.2f;
            exportButton.Text.text = Helper.Translation.GetString("csvExport");
            exportButton.Background.color = Palette.ImpostorRed;
            exportButton.Text.fontSizeMin = exportButton.Text.fontSizeMax = 2.2f;

            var passiveImportButton = importButton.GetComponent<PassiveButton>();
            passiveImportButton.OnClick = new ButtonClickedEvent();
            passiveImportButton.OnClick.AddListener(
                (UnityEngine.Events.UnityAction)CsvImport.Excute);

            var passiveExportButton = exportButton.GetComponent<PassiveButton>();
            passiveExportButton.OnClick = new ButtonClickedEvent();
            passiveExportButton.OnClick.AddListener(
                (UnityEngine.Events.UnityAction)CsvExport.Excute);

            passiveImportButton.gameObject.SetActive(true);
            passiveExportButton.gameObject.SetActive(true);

            TwitchManager man = DestroyableSingleton<TwitchManager>.Instance;
            CsvImport.InfoPopup = UnityEngine.Object.Instantiate<GenericPopup>(
                man.TwitchPopup, passiveImportButton.transform);
            CsvExport.InfoPopup = UnityEngine.Object.Instantiate<GenericPopup>(
                man.TwitchPopup, passiveExportButton.transform);

            var pos =  man.TwitchPopup.transform.position;
            pos.z = -2048f;
            CsvImport.InfoPopup.transform.position = pos;
            CsvExport.InfoPopup.transform.position = pos;
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

        private class ClickBehavior
        {
            public string Title;
            public Action OnClick;

            public ClickBehavior(string title, Action onClick)
            {
                Title = title;
                OnClick = onClick;
            }
        }

        private class CsvImport
        {
            public static GenericPopup InfoPopup;

            public static void Excute()
            {
                foreach (var sr in InfoPopup.gameObject.GetComponentsInChildren<SpriteRenderer>())
                {
                    sr.sortingOrder = 8;
                }
                foreach (var mr in InfoPopup.gameObject.GetComponentsInChildren<MeshRenderer>())
                {
                    mr.sortingOrder = 9;
                }

                string info = Helper.Translation.GetString("importPleaseWait");
                InfoPopup.Show(info); // Show originally
                bool result = Module.CustomOptionCsvProcessor.Import();

                if (result)
                {
                    info = Helper.Translation.GetString("ImportSuccess");
                }
                else
                {
                    info = Helper.Translation.GetString("ImportError");
                }
                InfoPopup.StartCoroutine(
                    Effects.Lerp(0.01f, new System.Action<float>((p) => { setPopupText(info); })));
            }
            private static void setPopupText(string message)
            {
                if (InfoPopup == null)
                {
                    return;
                }
                if (InfoPopup.TextAreaTMP != null)
                {
                    InfoPopup.TextAreaTMP.text = message;
                }
            }
        }
        private class CsvExport
        {
            public static GenericPopup InfoPopup;

            public static void Excute()
            {
                foreach (var sr in InfoPopup.gameObject.GetComponentsInChildren<SpriteRenderer>())
                {
                    sr.sortingOrder = 8;
                }
                foreach (var mr in InfoPopup.gameObject.GetComponentsInChildren<MeshRenderer>())
                {
                    mr.sortingOrder = 9;
                }


                InfoPopup.gameObject.transform.SetAsLastSibling();
                string info = Helper.Translation.GetString("exportPleaseWait");
                InfoPopup.Show(info); // Show originally
                bool result = Module.CustomOptionCsvProcessor.Export();

                if (result)
                {
                    info = Helper.Translation.GetString("exportSuccess");
                }
                else
                {
                    info = Helper.Translation.GetString("exportError");
                }
                InfoPopup.StartCoroutine(
                    Effects.Lerp(0.01f, new System.Action<float>((p) => { setPopupText(info); })));
            }
            private static void setPopupText(string message)
            {
                if (InfoPopup == null)
                {
                    return;
                }
                if (InfoPopup.TextAreaTMP != null)
                {
                    InfoPopup.TextAreaTMP.text = message;
                }
            }
        }

    }

}
