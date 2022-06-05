using System;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Resources;
using ExtremeRoles.Module.InfoOverlay.FullDec;

namespace ExtremeRoles.Module.InfoOverlay
{
    public class InfoOverlay
    {
        public enum ShowType : byte
        {
            LocalPlayerRole,
            LocalPlayerGhostRole,
            AllRole,
            AllGhostRole,
        }

        public bool OverlayShown => this.overlayShown;
        public ShowType CurShowInfo => this.curShow;
        
        private Sprite colorBackGround;
        private SpriteRenderer meetingUnderlay;
        private SpriteRenderer infoUnderlay;
        private TMPro.TextMeshPro ruleInfoText;
        private TMPro.TextMeshPro roleInfoText;
        private TMPro.TextMeshPro anotherRoleInfoText;

        private bool overlayShown = false;

        private Dictionary<ShowType, IShowTextBuilder> showText = new Dictionary<ShowType, IShowTextBuilder>
        {
            { ShowType.LocalPlayerRole     , new LocalPlayerRoleShowTextBuilder()      },
            { ShowType.LocalPlayerGhostRole, new LocalPlayerGhostRoleShowTextBuilder() },
            { ShowType.AllRole             , new AllRoleShowTextBuilder()              },
            { ShowType.AllGhostRole        , new AllGhostRoleShowTextBuilder()         },
        };
        private ShowType curShow;

        private const int maxLine = 35;
        private const float outlineWidth = 0.02f;

        public InfoOverlay()
        {
            pageClear();
            this.overlayShown = false;
        }

        public void ChangePage(int add)
        {
            if (!ExtremeRolesPlugin.Info.OverlayShown) { return; }
            var pageBuilder = this.showText[this.curShow] as PageShowTextBuilderBase;
            
            if (pageBuilder == null) { return; }

            pageBuilder.ChangePage(add);
            this.updateShowText();
        }

        public void HideBlackBG()
        {
            if (meetingUnderlay == null) { return; }
            meetingUnderlay.enabled = false;
        }

        public void HideInfoOverlay()
        {
            if (infoUnderlay == null) { return; }

            if (!OverlayShown) { return; }

            if (HudManager.Instance == null) { return; }
            if (MeetingHud.Instance == null) { DestroyableSingleton<HudManager>.Instance.SetHudActive(true); }

            this.overlayShown = false;
            var underlayTransparent = new Color(0.1f, 0.1f, 0.1f, 0.0f);
            var underlayOpaque = new Color(0.1f, 0.1f, 0.1f, 0.88f);

            HudManager.Instance.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
            {
                infoUnderlay.color = Color.Lerp(underlayOpaque, underlayTransparent, t);
                if (t >= 1.0f)
                {
                    infoUnderlay.enabled = false;
                }
                textLerp(ref ruleInfoText, t);
                textLerp(ref roleInfoText, t);
                textLerp(ref anotherRoleInfoText, t);
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

            pageClear();

            meetingUnderlay = infoUnderlay = null;
            ruleInfoText = roleInfoText = anotherRoleInfoText = null;
            this.overlayShown = false;
        }

        public void SetShowInfo(ShowType showType)
        {
            this.curShow = showType;
            updateShowText();
        }


        public void ToggleInfoOverlay(ShowType showType)
        {
            if (HudManager.Instance.Chat.IsOpen) { return; }

            if (!ExtremeRolesPlugin.GameDataStore.IsRoleSetUpEnd())
            {
                switch (showType)
                {
                    case ShowType.LocalPlayerRole:
                        showType = ShowType.AllRole;
                        break;
                    case ShowType.LocalPlayerGhostRole:
                        showType = ShowType.AllGhostRole;
                        break;
                    default:
                        break;
                }
            }


            if (OverlayShown)
            {
                if (this.curShow == showType)
                {
                    HideInfoOverlay();
                }
                else
                {
                    SetShowInfo(showType);
                }
            }
            else
            {
                showInfoOverlay(showType);
            }
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
                initInfoText(ref ruleInfoText);
                ruleInfoText.transform.localPosition = new Vector3(-3.6f, 1.6f, -910f);
            }

            if (roleInfoText == null)
            {
                roleInfoText = UnityEngine.Object.Instantiate(ruleInfoText, hudManager.transform);
                initInfoText(ref roleInfoText);
                roleInfoText.outlineWidth += outlineWidth;
                roleInfoText.maxVisibleLines = maxLine;
                roleInfoText.transform.localPosition = ruleInfoText.transform.localPosition + new Vector3(3.25f, 0.0f, 0.0f);
            }
            if (anotherRoleInfoText == null)
            {
                anotherRoleInfoText = UnityEngine.Object.Instantiate(ruleInfoText, hudManager.transform);
                initInfoText(ref anotherRoleInfoText);
                anotherRoleInfoText.outlineWidth += outlineWidth;
                anotherRoleInfoText.maxVisibleLines = maxLine;
                anotherRoleInfoText.transform.localPosition = ruleInfoText.transform.localPosition + new Vector3(6.5f, 0.0f, 0.0f);
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

        private void showInfoOverlay(ShowType showType)
        {

            if (OverlayShown) { return; }

            HudManager hudManager = DestroyableSingleton<HudManager>.Instance;
            if (PlayerControl.LocalPlayer == null ||
                hudManager == null ||
                HudManager.Instance.IsIntroDisplayed ||
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

            this.overlayShown = true;

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

            ruleInfoText.text = $"<size=200%>{Translation.GetString("gameOption")}</size>\n{CommonOption.GetGameOptionString()}";
            ruleInfoText.enabled = true;

            SetShowInfo(showType);

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
        private void updateShowText()
        {
            var (roleText, anotherRoleText) = this.showText[this.curShow].GetShowText();

            roleInfoText.text = roleText;
            roleInfoText.enabled = true;

            anotherRoleInfoText.text = anotherRoleText;
            anotherRoleInfoText.enabled = true;
        }

        private void initInfoText(
            ref TMPro.TextMeshPro text)
        {
            text.fontSize = text.fontSizeMin = text.fontSizeMax = 1.15f;
            text.autoSizeTextContainer = false;
            text.enableWordWrapping = false;
            text.alignment = TMPro.TextAlignmentOptions.TopLeft;
            text.transform.position = Vector3.zero;
            text.transform.localScale = Vector3.one;
            text.color = Palette.White;
            text.enabled = false;
        }

        private void textLerp(
            ref TMPro.TextMeshPro text, float t)
        {
            if (text != null)
            {
                text.color = Color.Lerp(Palette.White, Palette.ClearWhite, t);
                if (t >= 1.0f)
                {
                    text.enabled = false;
                }
            }
        }

        private void pageClear()
        {
            foreach (var builder in this.showText.Values)
            {
                var pageBuilder = builder as PageShowTextBuilderBase;
                pageBuilder?.Clear();
            }
        }
    }
}
