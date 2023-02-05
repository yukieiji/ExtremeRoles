using System;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Module.InfoOverlay.FullDec;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Helper;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;

namespace ExtremeRoles.Module.InfoOverlay
{
    public sealed class InfoOverlay
    {
        public enum ShowType : byte
        {
            LocalPlayerRole,
            LocalPlayerGhostRole,
            VanilaOption,
            AllRole,
            AllGhostRole,
        }

        public bool IsBlock => this.isBlock;
        public bool OverlayShown => this.overlayShown;
        public ShowType CurShowInfo => this.curShow;
        
        private Sprite colorBackGround;
        private SpriteRenderer meetingUnderlay;
        private SpriteRenderer infoUnderlay;

        private ScrollableText ruleInfo = new ScrollableText("ruleInfo", 50, 0.0001f, 0.1f);
        private ScrollableText roleInfo = new ScrollableText("roleInfo", 50, 0.0001f, 0.1f);
        private ScrollableText anotherRoleInfo = new ScrollableText("anotherRoleInfo", 50, 0.0001f, 0.1f);

        private bool overlayShown = false;
        private bool isBlock = false;

        private Dictionary<ShowType, IShowTextBuilder> showText = new Dictionary<ShowType, IShowTextBuilder>
        {
            { ShowType.LocalPlayerRole     , new LocalPlayerRoleShowTextBuilder()      },
            { ShowType.LocalPlayerGhostRole, new LocalPlayerGhostRoleShowTextBuilder() },
            { ShowType.VanilaOption        , new VanillaOptionBuillder()               },
            { ShowType.AllRole             , new AllRoleShowTextBuilder()              },
            { ShowType.AllGhostRole        , new AllGhostRoleShowTextBuilder()         },
        };
        private ShowType curShow;

        private static readonly Vector3 infoAnchorFirstPos = new Vector3(-4.0f, 1.6f, -910f);

        public InfoOverlay()
        {
            pageClear();
            this.overlayShown = false;
            this.isBlock = false;
        }

        public void BlockShow(bool isBlock)
        {
            this.isBlock = isBlock;
            if (this.isBlock)
            {
                HideInfoOverlay();
            }
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

            var hudManager = FastDestroyableSingleton<HudManager>.Instance;

            if (hudManager == null) { return; }
            if (MeetingHud.Instance == null) { hudManager.SetHudActive(true); }

            this.overlayShown = false;
            var underlayTransparent = new Color(0.1f, 0.1f, 0.1f, 0.0f);
            var underlayOpaque = new Color(0.1f, 0.1f, 0.1f, 0.88f);

            hudManager.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
            {
                infoUnderlay.color = Color.Lerp(underlayOpaque, underlayTransparent, t);
                if (t >= 1.0f)
                {
                    infoUnderlay.enabled = false;
                }
                infoLerp(ruleInfo, t, false, Palette.White, Palette.ClearWhite);
                infoLerp(roleInfo, t, false, Palette.White, Palette.ClearWhite);
                infoLerp(anotherRoleInfo, t, false, Palette.White, Palette.ClearWhite);
            })));

            Transform parent = hudManager.transform;

            ruleInfo.AnchorPoint.transform.parent = parent;
            roleInfo.AnchorPoint.transform.parent = parent;
            anotherRoleInfo.AnchorPoint.transform.parent = parent;

        }

        public void ResetOverlays()
        {
            HideBlackBG();
            HideInfoOverlay();

            UnityEngine.Object.Destroy(meetingUnderlay);
            UnityEngine.Object.Destroy(infoUnderlay);
            
            ruleInfo.Clear();
            roleInfo.Clear();
            anotherRoleInfo.Clear();

            pageClear();

            meetingUnderlay = infoUnderlay = null;
            this.overlayShown = false;
        }

        public void SetShowInfo(ShowType showType)
        {
            this.curShow = showType;
            updateShowText();
        }


        public void ToggleInfoOverlay(ShowType showType)
        {
            if (FastDestroyableSingleton<HudManager>.Instance.Chat.IsOpen) { return; }

            if (!RoleAssignState.Instance.IsRoleSetUpEnd)
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
            HudManager hudManager = FastDestroyableSingleton<HudManager>.Instance;
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
                meetingUnderlay.transform.localScale = new Vector3(20f, 20f, 1f);
                meetingUnderlay.gameObject.SetActive(true);
                meetingUnderlay.enabled = false;
                meetingUnderlay.name = "meetingOverlay";
            }
            if (infoUnderlay == null)
            {
                infoUnderlay = UnityEngine.Object.Instantiate(meetingUnderlay, hudManager.transform);
                infoUnderlay.transform.localPosition = new Vector3(0f, 0f, -900f);
                infoUnderlay.transform.localScale = new Vector3(10.25f, 5.7f, 1f);
                infoUnderlay.gameObject.SetActive(true);
                infoUnderlay.enabled = false;
                infoUnderlay.name = "infoOverlay";
            }

            ruleInfo.Initialize(
                new Vector3(-4.0f, 1.6f, -910f),
                new Vector3(0.0f, -0.325f, 0.0f),
                hudManager.TaskPanel.taskText,
                hudManager.transform,
                initInfoText);
            roleInfo.Initialize(
                infoAnchorFirstPos + new Vector3(3.5f, 0.0f, 0.0f),
                new Vector3(0.0f, -0.325f, 0.0f),
                hudManager.TaskPanel.taskText,
                hudManager.transform,
                initInfoText);
            anotherRoleInfo.Initialize(
                infoAnchorFirstPos + new Vector3(7.0f, 0.0f, 0.0f),
                new Vector3(0.0f, -0.325f, 0.0f),
                hudManager.TaskPanel.taskText,
                hudManager.transform,
                initInfoText);

            return true;
        }

        public void ShowBlackBG()
        {

            HudManager hudManager = FastDestroyableSingleton<HudManager>.Instance;

            if (hudManager == null) { return; }
            if (!initializeOverlays()) { return; }

            meetingUnderlay.sprite = colorBackGround;
            meetingUnderlay.enabled = true;
            var clearBlack = new Color32(0, 0, 0, 0);

            hudManager.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
            {
                meetingUnderlay.color = Color.Lerp(clearBlack, Palette.Black, t);
            })));
        }

        private void showInfoOverlay(ShowType showType)
        {

            if (OverlayShown || this.isBlock) { return; }

            HudManager hudManager = FastDestroyableSingleton<HudManager>.Instance;
            if (CachedPlayerControl.LocalPlayer == null ||
                hudManager == null ||
                hudManager.IsIntroDisplayed ||
                (!CachedPlayerControl.LocalPlayer.PlayerControl.CanMove && MeetingHud.Instance == null))
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

            ruleInfo.AnchorPoint.transform.parent = parent;
            roleInfo.AnchorPoint.transform.parent = parent;
            anotherRoleInfo.AnchorPoint.transform.parent = parent;

            infoUnderlay.color = new Color(0.1f, 0.1f, 0.1f, 0.88f);
            infoUnderlay.enabled = true;

            ruleInfo.UpdateText(
                $"<size=200%>{Translation.GetString("gameOption")}</size>",
                CommonOption.GetGameOptionString());
            ruleInfo.Enable(true);

            SetShowInfo(showType);

            var underlayTransparent = new Color(0.1f, 0.1f, 0.1f, 0.0f);
            var underlayOpaque = new Color(0.1f, 0.1f, 0.1f, 0.88f);
            hudManager.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
            {
                infoLerp(ruleInfo, t, true, Palette.ClearWhite, Palette.White);
                infoLerp(roleInfo, t, true, Palette.ClearWhite, Palette.White);
                infoLerp(anotherRoleInfo, t, true, Palette.ClearWhite, Palette.White);

                infoUnderlay.color = Color.Lerp(underlayTransparent, underlayOpaque, t);
            })));
        }
        private void updateShowText()
        {
            var (title, roleText, anotherRoleText) = this.showText[this.curShow].GetShowText();

            roleInfo.UpdateText(title, roleText);
            anotherRoleInfo.UpdateText("", anotherRoleText);
        }

        private void initInfoText(
            TMPro.TextMeshPro text)
        {
            text.fontSize = text.fontSizeMin = text.fontSizeMax = 1.15f;
            text.autoSizeTextContainer = false;
            text.enableWordWrapping = false;
            text.alignment = TMPro.TextAlignmentOptions.TopLeft;
            text.transform.position = Vector3.zero;
            text.transform.localScale = Vector3.one;
            text.color = Palette.White;
            text.enabled = false;
            text.gameObject.layer = 5;
        }

        private void infoLerp(
            ScrollableText text,
            float t, bool isEnable,
            Color32 fromColor, Color32 toColor)
        {
            if (text.BodyText != null)
            {
                text.BodyText.color = Color.Lerp(fromColor, toColor, t);
            }
            if (text.Title != null)
            {
                text.Title.color = Color.Lerp(fromColor, toColor, t);
            }

            if (t >= 1.0f)
            {
                text.Enable(isEnable);
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
