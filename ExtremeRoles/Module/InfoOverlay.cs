using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using ExtremeRoles.Helper;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module
{
    public class InfoOverlay
    {
        public bool OverlayShown = false;
        public GameObject infoButton = null;
        
        private Sprite colorBackGround;
        private SpriteRenderer meetingUnderlay;
        private SpriteRenderer infoUnderlay;
        private TMPro.TextMeshPro ruleInfoText;
        private TMPro.TextMeshPro roleInfoText;
        private TMPro.TextMeshPro anotherRoleInfoText;

        private int rolePage = 0;
        private List<(string, int)> allRoleText = new List<(string, int)>();

        public InfoOverlay()
        {
            allRoleText.Clear();
            rolePage = 0;
            infoButton = null;
        }

        public void CreateInfoButton()
        {
            var topRight = GameObject.Find("TopRight");

            infoButton = UnityEngine.Object.Instantiate(
                Prefab.HelpButton,
                topRight.transform);
            UnityEngine.Object.DontDestroyOnLoad(infoButton);
            infoButton.name = "infoRoleButton";
            infoButton.gameObject.SetActive(true);
            infoButton.layer = 5;
            SetInfoButtonToGameStartShipPositon();
            var passiveButton = infoButton.GetComponent<PassiveButton>();
            passiveButton.OnClick = new Button.ButtonClickedEvent();
            passiveButton.OnClick.AddListener(
                (UnityAction)ExtremeRolesPlugin.Info.ToggleInfoOverlay);
        }

        public void ChangeRoleInfoPage(int count)
        {
            if (this.allRoleText.Count == 0) { return; }
            this.rolePage = (this.rolePage + count) % this.allRoleText.Count;

            if (this.rolePage < 0)
            {
                this.rolePage = this.allRoleText.Count + this.rolePage;
            }
            updateShowText(getRoleInfoPageText());
        }

        public void HideBlackBG()
        {
            if (meetingUnderlay == null) { return; }
            meetingUnderlay.enabled = false;
        }

        public void HideInfoOverlay()
        {
            if (!OverlayShown) { return; }

            if (MeetingHud.Instance == null) { DestroyableSingleton<HudManager>.Instance.SetHudActive(true); }

            OverlayShown = false;
            var underlayTransparent = new Color(0.1f, 0.1f, 0.1f, 0.0f);
            var underlayOpaque = new Color(0.1f, 0.1f, 0.1f, 0.88f);

            HudManager.Instance.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
            {
                if (infoUnderlay != null)
                {
                    infoUnderlay.color = Color.Lerp(underlayOpaque, underlayTransparent, t);
                    if (t >= 1.0f)
                    {
                        infoUnderlay.enabled = false;
                    }
                }

                if (ruleInfoText != null)
                {
                    ruleInfoText.color = Color.Lerp(Palette.White, Palette.ClearWhite, t);
                    if (t >= 1.0f)
                    {
                        ruleInfoText.enabled = false;
                    }
                }

                if (roleInfoText != null)
                {
                    roleInfoText.color = Color.Lerp(Palette.White, Palette.ClearWhite, t);
                    if (t >= 1.0f)
                    {
                        roleInfoText.enabled = false;
                    }
                }
                if (anotherRoleInfoText != null)
                {
                    anotherRoleInfoText.color = Color.Lerp(Palette.White, Palette.ClearWhite, t);
                    if (t >= 1.0f)
                    {
                        anotherRoleInfoText.enabled = false;
                    }
                }
            })));
        }

        public void MeetingStartRest()
        {
            showBlackBG();
            HideInfoOverlay();
        }

        public void ResetOverlays()
        {
            HideBlackBG();
            HideInfoOverlay();

            UnityEngine.Object.Destroy(meetingUnderlay);
            UnityEngine.Object.Destroy(infoUnderlay);
            UnityEngine.Object.Destroy(ruleInfoText);
            UnityEngine.Object.Destroy(roleInfoText);
            UnityEngine.Object.Destroy(anotherRoleInfoText);

            this.allRoleText.Clear();

            meetingUnderlay = infoUnderlay = null;
            ruleInfoText = roleInfoText = anotherRoleInfoText = null;
            this.OverlayShown = false;
            this.rolePage = 0;
        }

        public void ToggleInfoOverlay()
        {
            if (OverlayShown)
            {
                HideInfoOverlay();
            }
            else
            {
                showInfoOverlay();
            }
        }

        public void SetInfoButtonToGameStartShipPositon()
        {
            infoButton.transform.localPosition = new Vector3(
                4.925f, 2.0f, 0.0f);
        }

        public void SetInfoButtonToInGamePositon()
        {
            infoButton.gameObject.SetActive(true);
            infoButton.transform.localPosition = new Vector3(
                4.925f, 1.3f, 0.0f);
        }

        private string cleanPlaceHolder(string value)
        {
            return Regex.Replace(
                value, "\\{[0-9]+\\}",
                Translation.GetString("gameReplace"));
        }

        private void createAllRoleText()
        {
            int optionId;
            string colorRoleName;
            string roleFullDesc;
            string roleText;

            foreach (var role in Roles.ExtremeRoleManager.NormalRole)
            {
                optionId = role.GetRoleOptionOffset();
                colorRoleName = role.GetColoredRoleName();

                roleFullDesc = Translation.GetString($"{role.Id}FullDescription");
                roleFullDesc = cleanPlaceHolder(roleFullDesc);

                roleText = string.Concat(
                    $"<size=150%>・{colorRoleName}</size>",
                    roleFullDesc != "" ? $"\n{roleFullDesc}\n" : "",
                    $"・{Translation.GetString(colorRoleName)}{Translation.GetString("roleOption")}\n",
                    "{0}");

                this.allRoleText.Add(((string)roleText.Clone(), optionId));
            }

            foreach (var combRole in Roles.ExtremeRoleManager.CombRole)
            {
                if (combRole is ConstCombinationRoleManagerBase)
                {
                    foreach (var role in combRole.Roles)
                    {
                        optionId = role.GetManagerOptionOffset();
                        colorRoleName = role.GetColoredRoleName();

                        roleFullDesc = Translation.GetString($"{role.Id}FullDescription");

                        roleFullDesc = cleanPlaceHolder(roleFullDesc);

                        roleText = string.Concat(
                            $"<size=150%>・{colorRoleName}</size>",
                            roleFullDesc != "" ? $"\n{roleFullDesc}\n" : "",
                            $"・{Translation.GetString(colorRoleName)}{Translation.GetString("roleOption")}\n",
                            "{0}");

                        this.allRoleText.Add(((string)roleText.Clone(), optionId));
                    }
                }
                else if (combRole is FlexibleCombinationRoleManagerBase)
                {

                    var role = ((FlexibleCombinationRoleManagerBase)combRole).BaseRole;

                    optionId = role.GetManagerOptionOffset();
                    colorRoleName = role.GetColoredRoleName();

                    roleFullDesc = Translation.GetString($"{role.Id}FullDescription");
                    roleFullDesc = cleanPlaceHolder(roleFullDesc);

                    roleText = string.Concat(
                        $"<size=150%>・{colorRoleName}</size>",
                        roleFullDesc != "" ? $"\n{roleFullDesc}\n" : "",
                        $"・{Translation.GetString(colorRoleName)}{Translation.GetString("roleOption")}\n",
                        "{0}");

                    this.allRoleText.Add(((string)roleText.Clone(), optionId));
                }
            }
        }

        private string getCommonOptionString()
        {

            var allOption = OptionHolder.AllOption;

            List<string> printOption = new List<string>();

            foreach (OptionHolder.CommonOptionKey key in Enum.GetValues(
                typeof(OptionHolder.CommonOptionKey)))
            {
                if (key == OptionHolder.CommonOptionKey.PresetSelection) { continue; }

                if (key == OptionHolder.CommonOptionKey.NumMeating)
                {
                    printOption.Add("");
                }

                var option = allOption[(int)key];

                if (option == null) { continue; }

                if (!option.IsHidden)
                {
                    printOption.Add(
                        CustomOption.OptionToString(option));
                }
                if (option.Enabled)
                {
                    foreach (CustomOptionBase op in option.Children)
                    {
                        string str = CustomOption.OptionToString(op);
                        if (str != "")
                        {
                            printOption.Add(str);
                        }
                    }
                }

            }

            return string.Join("\n", printOption);

        }

        private Tuple<string, string> getPlayerRoleText()
        {
            string roleText = $"<size=200%>{Translation.GetString("yourRole")}</size>\n";
            string anotherRoleText = "<size=200%> </size>\n";
            var role = Roles.ExtremeRoleManager.GetLocalPlayerRole();
            var allOption = OptionHolder.AllOption;

            string roleOptionString = "";
            string colorRoleName;

            var multiAssignRole = role as MultiAssignRoleBase;
            if (multiAssignRole != null)
            {
                roleOptionString =
                    CustomOption.AllOptionToString(
                        allOption[multiAssignRole.GetManagerOptionId(
                            RoleCommonOption.SpawnRate)]);
                colorRoleName = Design.ColoedString(
                    multiAssignRole.NameColor,
                    Translation.GetString(multiAssignRole.RoleName));
            }
            else if (role.IsVanillaRole())
            {
                colorRoleName = role.GetColoredRoleName();
            }
            else
            {
                roleOptionString =
                    CustomOption.AllOptionToString(
                        allOption[role.GetRoleOptionId(
                            RoleCommonOption.SpawnRate)]);
                colorRoleName = role.GetColoredRoleName();
            }

            string roleFullDesc = role.GetFullDescription();


            roleText += string.Concat(
                $"<size=150%>・{colorRoleName}</size>",
                roleFullDesc != "" ? $"\n{roleFullDesc}\n" : "",
                $"・{Translation.GetString(colorRoleName)}{Translation.GetString("roleOption")}\n",
                roleOptionString != "" ? $"{roleOptionString}" : "");

            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {

                    string anotherRoleOptionString = "";

                    if (!multiAssignRole.IsVanillaRole())
                    {
                        anotherRoleOptionString =
                            CustomOption.AllOptionToString(
                                allOption[multiAssignRole.AnotherRole.GetRoleOptionId(
                                    RoleCommonOption.SpawnRate)]);
                    }
                    string anotherRoleFullDesc = multiAssignRole.AnotherRole.GetFullDescription();

                    anotherRoleText +=
                        $"\n<size=150%>・{multiAssignRole.AnotherRole.GetColoredRoleName()}</size>" +
                        (anotherRoleFullDesc != "" ? $"\n{anotherRoleFullDesc}\n" : "") +
                        $"・{Translation.GetString(multiAssignRole.AnotherRole.GetColoredRoleName())}{Translation.GetString("roleOption")}\n" +
                        (anotherRoleOptionString != "" ? $"{anotherRoleOptionString}" : "");
                }
            }

            return Tuple.Create(roleText, anotherRoleText);

        }

        private Tuple<string, string> getRoleInfoPageText()
        {
            if (this.allRoleText.Count == 0) { createAllRoleText(); }

            var (roleTextBase, optionId) = this.allRoleText[this.rolePage];

            string roleOption = CustomOption.AllOptionToString(
                OptionHolder.AllOption[optionId + (int)RoleCommonOption.SpawnRate]);

            string showRole = string.Concat(
                $"<size=200%>{Translation.GetString("roleDesc")}</size>",
                $"           {Translation.GetString("changeRoleMore")}",
                $"({this.rolePage + 1}/{ this.allRoleText.Count })\n",
                string.Format(roleTextBase, roleOption != "" ? $"{roleOption}" : ""));
            return Tuple.Create(showRole, "");
        }

        private bool initializeOverlays()
        {
            HudManager hudManager = DestroyableSingleton<HudManager>.Instance;
            if (hudManager == null) { return false; }

            if (colorBackGround == null)
            {
                colorBackGround = Loader.CreateSpriteFromResources(
                    Path.BackGround, 100f);
            }


            if (meetingUnderlay == null)
            {
                meetingUnderlay = UnityEngine.Object.Instantiate(hudManager.FullScreen, hudManager.transform);
                meetingUnderlay.transform.localPosition = new Vector3(0f, 0f, 20f);
                meetingUnderlay.gameObject.SetActive(true);
                meetingUnderlay.enabled = false;
            }

            if (infoUnderlay == null)
            {
                infoUnderlay = UnityEngine.Object.Instantiate(meetingUnderlay, hudManager.transform);
                infoUnderlay.transform.localPosition = new Vector3(0f, 0f, -900f);
                infoUnderlay.gameObject.SetActive(true);
                infoUnderlay.enabled = false;
            }

            if (ruleInfoText == null)
            {
                ruleInfoText = UnityEngine.Object.Instantiate(hudManager.TaskText, hudManager.transform);
                ruleInfoText.fontSize = ruleInfoText.fontSizeMin = ruleInfoText.fontSizeMax = 1.15f;
                ruleInfoText.autoSizeTextContainer = false;
                ruleInfoText.enableWordWrapping = false;
                ruleInfoText.alignment = TMPro.TextAlignmentOptions.TopLeft;
                ruleInfoText.transform.position = Vector3.zero;
                ruleInfoText.transform.localPosition = new Vector3(-3.6f, 1.6f, -910f);
                ruleInfoText.transform.localScale = Vector3.one;
                ruleInfoText.color = Palette.White;
                ruleInfoText.enabled = false;
            }

            if (roleInfoText == null)
            {
                roleInfoText = UnityEngine.Object.Instantiate(ruleInfoText, hudManager.transform);
                roleInfoText.maxVisibleLines = 28;
                roleInfoText.fontSize = roleInfoText.fontSizeMin = roleInfoText.fontSizeMax = 1.15f;
                roleInfoText.outlineWidth += 0.02f;
                roleInfoText.autoSizeTextContainer = false;
                roleInfoText.enableWordWrapping = false;
                roleInfoText.alignment = TMPro.TextAlignmentOptions.TopLeft;
                roleInfoText.transform.position = Vector3.zero;
                roleInfoText.transform.localPosition = ruleInfoText.transform.localPosition + new Vector3(3.25f, 0.0f, 0.0f);
                roleInfoText.transform.localScale = Vector3.one;
                roleInfoText.color = Palette.White;
                roleInfoText.enabled = false;
            }
            if (anotherRoleInfoText == null)
            {
                anotherRoleInfoText = UnityEngine.Object.Instantiate(ruleInfoText, hudManager.transform);
                anotherRoleInfoText.maxVisibleLines = 28;
                anotherRoleInfoText.fontSize = anotherRoleInfoText.fontSizeMin = anotherRoleInfoText.fontSizeMax = 1.15f;
                anotherRoleInfoText.outlineWidth += 0.02f;
                anotherRoleInfoText.autoSizeTextContainer = false;
                anotherRoleInfoText.enableWordWrapping = false;
                anotherRoleInfoText.alignment = TMPro.TextAlignmentOptions.TopLeft;
                anotherRoleInfoText.transform.position = Vector3.zero;
                anotherRoleInfoText.transform.localPosition = ruleInfoText.transform.localPosition + new Vector3(6.5f, 0.0f, 0.0f);
                anotherRoleInfoText.transform.localScale = Vector3.one;
                anotherRoleInfoText.color = Palette.White;
                anotherRoleInfoText.enabled = false;
            }

            return true;
        }

        private void showBlackBG()
        {
            if (HudManager.Instance == null) { return; }
            if (!initializeOverlays()) { return; }

            meetingUnderlay.sprite = colorBackGround;
            meetingUnderlay.enabled = true;
            meetingUnderlay.transform.localScale = new Vector3(20f, 20f, 1f);
            var clearBlack = new Color32(0, 0, 0, 0);

            HudManager.Instance.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
            {
                meetingUnderlay.color = Color.Lerp(clearBlack, Palette.Black, t);
            })));
        }



        private void showInfoOverlay()
        {

            if (OverlayShown) { return; }

            HudManager hudManager = DestroyableSingleton<HudManager>.Instance;
            if (PlayerControl.LocalPlayer == null ||
                hudManager == null ||
                HudManager.Instance.isIntroDisplayed ||
                (!PlayerControl.LocalPlayer.CanMove && MeetingHud.Instance == null))
            {
                return;
            }

            if (!initializeOverlays()) { return; }

            if (MapBehaviour.Instance != null)
            {
                MapBehaviour.Instance.Close();
            }

            hudManager.SetHudActive(false);

            OverlayShown = true;

            Transform parent;
            if (MeetingHud.Instance != null)
            {
                parent = MeetingHud.Instance.transform;
            }
            else
            {
                parent = hudManager.transform;
            }
            infoUnderlay.transform.parent = parent;
            ruleInfoText.transform.parent = parent;
            roleInfoText.transform.parent = parent;
            anotherRoleInfoText.transform.parent = parent;

            infoUnderlay.color = new Color(0.1f, 0.1f, 0.1f, 0.88f);
            infoUnderlay.transform.localScale = new Vector3(9.5f, 5.7f, 1f);
            infoUnderlay.enabled = true;

            ruleInfoText.text = $"<size=200%>{Translation.GetString("gameOption")}</size>\n{getCommonOptionString()}";
            ruleInfoText.enabled = true;

            Tuple<string, string> showText;

            if (ExtremeRolesPlugin.GameDataStore.IsRoleSetUpEnd())
            {
                showText = getPlayerRoleText();
            }
            else
            {
                showText = getRoleInfoPageText();
            }

            updateShowText(showText);

            var underlayTransparent = new Color(0.1f, 0.1f, 0.1f, 0.0f);
            var underlayOpaque = new Color(0.1f, 0.1f, 0.1f, 0.88f);
            HudManager.Instance.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
            {
                infoUnderlay.color = Color.Lerp(underlayTransparent, underlayOpaque, t);
                ruleInfoText.color = Color.Lerp(Palette.ClearWhite, Palette.White, t);
                roleInfoText.color = Color.Lerp(Palette.ClearWhite, Palette.White, t);
                anotherRoleInfoText.color = Color.Lerp(Palette.ClearWhite, Palette.White, t);
            })));
        }

        private void updateShowText(Tuple<string, string> text)
        {
            var (roleText, anotherRoleText) = text;

            roleInfoText.text = roleText;
            roleInfoText.enabled = true;

            anotherRoleInfoText.text = anotherRoleText;
            anotherRoleInfoText.enabled = true;
        }
    }
}
