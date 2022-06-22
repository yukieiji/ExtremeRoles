using System;
using System.Collections.Generic;
using System.Reflection;

using HarmonyLib;

using TMPro;
using UnityEngine;
using UnityEngine.Events;

using ExtremeRoles.Performance;

using static ExtremeRoles.OptionHolder;
using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;



namespace ExtremeRoles.Patches.Option
{

    [HarmonyPatch]
    [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
    public static class OptionsMenuBehaviourStartPatch
    {

        private static SelectionBehaviour[] modOption = {
            new SelectionBehaviour(
                "streamerModeButton",
                () => Client.StreamerMode = ConfigParser.StreamerMode.Value = !ConfigParser.StreamerMode.Value,
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
            new SelectionBehaviour(
                "hideNamePlateButton",
                () =>
                {
                    Client.HideNamePlate = ConfigParser.HideNamePlate.Value = !ConfigParser.HideNamePlate.Value;
                    Meeting.NamePlateHelper.NameplateChange = true;
                    return Client.HideNamePlate;
                }, ConfigParser.HideNamePlate.Value)
        };

        private static GameObject popUp;
        private static TextMeshPro moreOptionText;
        private static TextMeshPro creditText;

        private static ToggleButtonBehaviour moreOptionButton;
        private static List<ToggleButtonBehaviour> modOptionButton;

        private static ToggleButtonBehaviour importButton;
        private static ToggleButtonBehaviour exportButton;

        private static ToggleButtonBehaviour buttonPrefab;
        public static void Postfix(OptionsMenuBehaviour __instance)
        {
            if (!__instance.CensorChatButton) { return; }

            if (!moreOptionText && Module.Prefab.Text != null)
            {
                moreOptionText = Object.Instantiate(
                    Module.Prefab.Text);
                Object.DontDestroyOnLoad(moreOptionText);
                moreOptionText.gameObject.SetActive(false);
            }

            if (!popUp)
            {
                createCustom(__instance);
            }

            if (!buttonPrefab)
            {
                buttonPrefab = Object.Instantiate(__instance.CensorChatButton);
                Object.DontDestroyOnLoad(buttonPrefab);
                buttonPrefab.name = "censorChatPrefab";
                buttonPrefab.gameObject.SetActive(false);
            }

            setUpOptions();
            initializeMoreButton(__instance);
            setLeaveGameButtonPostion();
        }

        public static void UpdateMenuTranslation()
        {
            if (moreOptionText)
            {
                moreOptionText.text = Helper.Translation.GetString("moreOptionText");
            }
            if (moreOptionButton)
            {
                moreOptionButton.Text.text = Helper.Translation.GetString("modOptionText");
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
            moreOptionButton.transform.localPosition = 
                __instance.CensorChatButton.transform.localPosition + 
                Vector3.down * 0.5f + new Vector3(0.0f, -1.5f, 0.0f);
            moreOptionButton.name = "modMenuButton";

            moreOptionButton.gameObject.SetActive(true);
            moreOptionButton.Text.text = Helper.Translation.GetString("modOptionText");
            var moreOptionsButton = moreOptionButton.GetComponent<PassiveButton>();
            moreOptionsButton.OnClick = new ButtonClickedEvent();
            moreOptionsButton.OnClick.AddListener((Action)(() =>
            {
                if (!popUp) { return; }

                var hudManager = FastDestroyableSingleton<HudManager>.Instance;

                if (__instance.transform.parent && __instance.transform.parent == hudManager.transform)
                {
                    popUp.transform.SetParent(hudManager.transform);
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
            if (!popUp || popUp.GetComponentInChildren<TextMeshPro>() || !moreOptionText) { return; }

            var title = moreOptionText = Object.Instantiate(moreOptionText, popUp.transform);
            title.GetComponent<RectTransform>().localPosition = Vector3.up * 2.3f;
            title.gameObject.SetActive(true);
            title.text = Helper.Translation.GetString("moreOptionText");
            title.transform.localPosition += new Vector3(0.0f, 0.25f, 0f);
            title.name = "titleText";
        }

        private static void setUpOptions()
        {
            if (popUp.transform.GetComponentInChildren<ToggleButtonBehaviour>()) { return; }

            createModOption();
            createOptionInExButton();

            creditText = Object.Instantiate(
                Module.Prefab.Text, popUp.transform);
            creditText.name = "credit";

            string modCredit = string.Format(
                "<size=175%>Extreme Roles<space=0.9em>{0}{1}</size>",
                Helper.Translation.GetString("version"),
                Assembly.GetExecutingAssembly().GetName().Version);
            string developCredit = $"\n<align=left>{Helper.Translation.GetString("developer")}yukieiji";
            string debugCredit = $"\n<align=left>{Helper.Translation.GetString("debugThunk")}stou59，Tyoubi，mamePi";

            creditText.transform.localPosition = new Vector3(0.0f, -2.0f, -.5f);
            creditText.fontSizeMin = creditText.fontSizeMax = 3.0f;
            creditText.font = Object.Instantiate(moreOptionText.font);
            creditText.GetComponent<RectTransform>().sizeDelta = new Vector2(5.4f, 5.5f);
            creditText.text = string.Concat(
                modCredit, developCredit, debugCredit);
            creditText.gameObject.SetActive(true);

        }

        private static void createModOption()
        {
            modOptionButton = new List<ToggleButtonBehaviour>();

            for (var i = 0; i < modOption.Length; i++)
            {
                var info = modOption[i];

                var button = Object.Instantiate(buttonPrefab, popUp.transform);
                button.transform.localPosition = new Vector3(
                    i % 2 == 0 ? -1.17f : 1.17f,
                    1.75f - i / 2 * 0.8f,
                    -.5f);

                button.onState = info.DefaultValue;
                button.Background.color = button.onState ? Color.green : Palette.ImpostorRed;

                button.Text.text = Helper.Translation.GetString(info.Title);
                button.Text.fontSizeMin = button.Text.fontSizeMax = 2.2f;
                button.Text.font = Object.Instantiate(moreOptionText.font);
                button.Text.GetComponent<RectTransform>().sizeDelta = new Vector2(2, 2);

                button.name = info.Title.Replace(" ", "") + "toggle";
                button.gameObject.SetActive(true);
                button.gameObject.transform.SetAsFirstSibling();

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
        }

        private static void createOptionInExButton()
        {
            importButton = Object.Instantiate(
                buttonPrefab, popUp.transform);
            exportButton = Object.Instantiate(
                buttonPrefab, popUp.transform);

            importButton.transform.localPosition = new Vector3(
                -1.35f, -0.9f, -.5f);
            exportButton.transform.localPosition = new Vector3(
                1.35f, -0.9f, -.5f);

            importButton.Text.text = Helper.Translation.GetString("csvImport");
            importButton.Text.enableWordWrapping = false;
            importButton.Background.color = Color.green;
            importButton.Text.fontSizeMin = importButton.Text.fontSizeMax = 2.2f;

            exportButton.Text.text = Helper.Translation.GetString("csvExport");
            exportButton.Text.enableWordWrapping = false;
            exportButton.Background.color = Palette.ImpostorRed;
            exportButton.Text.fontSizeMin = exportButton.Text.fontSizeMax = 2.2f;

            var passiveImportButton = importButton.GetComponent<PassiveButton>();
            passiveImportButton.OnClick = new ButtonClickedEvent();
            passiveImportButton.OnClick.AddListener(
                (UnityAction)CsvImport.Excute);

            var passiveExportButton = exportButton.GetComponent<PassiveButton>();
            passiveExportButton.OnClick = new ButtonClickedEvent();
            passiveExportButton.OnClick.AddListener(
                (UnityAction)CsvExport.Excute);

            passiveImportButton.gameObject.SetActive(true);
            passiveExportButton.gameObject.SetActive(true);

            CsvImport.InfoPopup = Object.Instantiate(
                Module.Prefab.Prop, passiveImportButton.transform);
            CsvExport.InfoPopup = Object.Instantiate(
                Module.Prefab.Prop, passiveExportButton.transform);

            var pos = Module.Prefab.Prop.transform.position;
            pos.z = -2048f;
            CsvImport.InfoPopup.transform.position = pos;
            CsvExport.InfoPopup.transform.position = pos;

            CsvImport.InfoPopup.TextAreaTMP.fontSize *= 0.75f;
            CsvImport.InfoPopup.TextAreaTMP.enableAutoSizing = false;

            CsvExport.InfoPopup.TextAreaTMP.fontSize *= 0.6f;
            CsvExport.InfoPopup.TextAreaTMP.enableAutoSizing = false;
        }

        private static IEnumerable<GameObject> getAllChilds(this GameObject Go)
        {
            for (var i = 0; i < Go.transform.childCount; ++i)
            {
                yield return Go.transform.GetChild(i).gameObject;
            }
        }

        private static void setLeaveGameButtonPostion()
        {
            var leaveGameButton = GameObject.Find("LeaveGameButton");
            if (leaveGameButton == null) { return; }
            leaveGameButton.transform.localPosition += (Vector3.right * 1.3f);
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
                    info = Helper.Translation.GetString("importSuccess");
                }
                else
                {
                    info = Helper.Translation.GetString("importError");
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
