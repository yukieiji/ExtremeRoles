using System;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Module.InfoOverlay.FullDec;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Helper;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.InfoOverlay;

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

    public bool IsBlock { get; private set; }
    public bool OverlayShown { get; private set; }
    public ShowType CurShowInfo { get; private set; }

    private SpriteRenderer infoUnderlay;

    private ScrollableText ruleInfo = new ScrollableText("ruleInfo", 50, 0.0001f, 0.1f);
    private ScrollableText roleInfo = new ScrollableText("roleInfo", 50, 0.0001f, 0.1f);
    private ScrollableText anotherRoleInfo = new ScrollableText("anotherRoleInfo", 50, 0.0001f, 0.1f);


    private Dictionary<ShowType, IShowTextBuilder> showText = new Dictionary<ShowType, IShowTextBuilder>
    {
        { ShowType.LocalPlayerRole     , new LocalPlayerRoleShowTextBuilder()      },
        { ShowType.LocalPlayerGhostRole, new LocalPlayerGhostRoleShowTextBuilder() },
        { ShowType.VanilaOption        , new VanillaOptionBuillder()               },
        { ShowType.AllRole             , new AllRoleShowTextBuilder()              },
        { ShowType.AllGhostRole        , new AllGhostRoleShowTextBuilder()         },
    };

    private static readonly Vector3 infoAnchorFirstPos = new Vector3(-4.0f, 1.6f, -910f);

    public InfoOverlay()
    {
        pageClear();
        this.OverlayShown = false;
        this.IsBlock = false;
    }

    public void BlockShow(bool isBlock)
    {
        this.IsBlock = isBlock;
        if (this.IsBlock)
        {
            HideInfoOverlay();
        }
    }

    public void ChangePage(int add)
    {
        if (!ExtremeRolesPlugin.Info.OverlayShown) { return; }
        var pageBuilder = this.showText[this.CurShowInfo] as PageShowTextBuilderBase;
        
        if (pageBuilder == null) { return; }

        pageBuilder.ChangePage(add);
        this.updateShowText();
    }

    public void HideInfoOverlay()
    {
        if (this.infoUnderlay == null) { return; }

        if (!this.OverlayShown) { return; }

        var hudManager = FastDestroyableSingleton<HudManager>.Instance;

        if (hudManager == null) { return; }
        if (MeetingHud.Instance == null) { hudManager.SetHudActive(true); }

        this.OverlayShown = false;
        var underlayTransparent = new Color(0.1f, 0.1f, 0.1f, 0.0f);
        var underlayOpaque = new Color(0.1f, 0.1f, 0.1f, 0.88f);

        hudManager.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
        {
            this.infoUnderlay.color = Color.Lerp(underlayOpaque, underlayTransparent, t);
            if (t >= 1.0f)
            {
                this.infoUnderlay.enabled = false;
            }
            infoLerp(this.ruleInfo, t, false, Palette.White, Palette.ClearWhite);
            infoLerp(this.roleInfo, t, false, Palette.White, Palette.ClearWhite);
            infoLerp(this.anotherRoleInfo, t, false, Palette.White, Palette.ClearWhite);
        })));

        Transform parent = hudManager.transform;

        this.ruleInfo.AnchorPoint.transform.parent = parent;
        this.roleInfo.AnchorPoint.transform.parent = parent;
        this.anotherRoleInfo.AnchorPoint.transform.parent = parent;
    }

    public void ResetOverlays()
    {
        HideInfoOverlay();

        UnityEngine.Object.Destroy(this.infoUnderlay);

        this.ruleInfo.Clear();
        this.roleInfo.Clear();
        this.anotherRoleInfo.Clear();

        pageClear();

        this.infoUnderlay = null;
        this.OverlayShown = false;
    }

    public void SetShowInfo(ShowType showType)
    {
        this.CurShowInfo = showType;
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


        if (this.OverlayShown)
        {
            if (this.CurShowInfo == showType)
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

        if (this.infoUnderlay == null)
        {
            this.infoUnderlay = UnityEngine.Object.Instantiate(
                hudManager.FullScreen, hudManager.transform);
            this.infoUnderlay.transform.localPosition = new Vector3(0f, 0f, -900f);
            this.infoUnderlay.gameObject.SetActive(true);
            this.infoUnderlay.enabled = false;
            this.infoUnderlay.name = "infoOverlay";
        }

        this.ruleInfo.Initialize(
            new Vector3(-4.0f, 1.6f, -910f),
            new Vector3(0.0f, -0.325f, 0.0f),
            hudManager.TaskPanel.taskText,
            hudManager.transform,
            initInfoText);
        this.roleInfo.Initialize(
            infoAnchorFirstPos + new Vector3(3.5f, 0.0f, 0.0f),
            new Vector3(0.0f, -0.325f, 0.0f),
            hudManager.TaskPanel.taskText,
            hudManager.transform,
            initInfoText);
        this.anotherRoleInfo.Initialize(
            infoAnchorFirstPos + new Vector3(7.0f, 0.0f, 0.0f),
            new Vector3(0.0f, -0.325f, 0.0f),
            hudManager.TaskPanel.taskText,
            hudManager.transform,
            initInfoText);

        return true;
    }

    private void showInfoOverlay(ShowType showType)
    {

        if (this.OverlayShown || this.IsBlock) { return; }

        HudManager hudManager = FastDestroyableSingleton<HudManager>.Instance;
        if (CachedPlayerControl.LocalPlayer == null ||
            hudManager == null ||
            hudManager.IsIntroDisplayed ||
            (
                !CachedPlayerControl.LocalPlayer.PlayerControl.CanMove && 
                MeetingHud.Instance == null
            ) ||
            !this.initializeOverlays())
        {
            return;
        }

        if (MapBehaviour.Instance != null)
        {
            MapBehaviour.Instance.Close();
        }

        hudManager.SetHudActive(false);

        this.OverlayShown = true;

        Transform parent;
        if (MeetingHud.Instance != null)
        {
            parent = MeetingHud.Instance.transform;
        }
        else
        {
            parent = hudManager.transform;
        }

        this.ruleInfo.AnchorPoint.transform.parent = parent;
        this.roleInfo.AnchorPoint.transform.parent = parent;
        this.anotherRoleInfo.AnchorPoint.transform.parent = parent;

        this.infoUnderlay.transform.localScale = new Vector3(10.25f, 5.7f, 1f);
        this.infoUnderlay.color = new Color(0.1f, 0.1f, 0.1f, 0.88f);
        this.infoUnderlay.enabled = true;

        this.ruleInfo.UpdateText(
            $"<size=200%>{Translation.GetString("gameOption")}</size>",
            CommonOption.GetGameOptionString());
        this.ruleInfo.Enable(true);

        SetShowInfo(showType);

        var underlayTransparent = new Color(0.1f, 0.1f, 0.1f, 0.0f);
        var underlayOpaque = new Color(0.1f, 0.1f, 0.1f, 0.88f);
        hudManager.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
        {
            infoLerp(this.ruleInfo, t, true, Palette.ClearWhite, Palette.White);
            infoLerp(this.roleInfo, t, true, Palette.ClearWhite, Palette.White);
            infoLerp(this.anotherRoleInfo, t, true, Palette.ClearWhite, Palette.White);

            this.infoUnderlay.color = Color.Lerp(underlayTransparent, underlayOpaque, t);
        })));
    }
    private void updateShowText()
    {
        var (title, roleText, anotherRoleText) = this.showText[this.CurShowInfo].GetShowText();

        this.roleInfo.UpdateText(title, roleText);
        this.anotherRoleInfo.UpdateText("", anotherRoleText);
    }

    private void pageClear()
    {
        foreach (var builder in this.showText.Values)
        {
            var pageBuilder = builder as PageShowTextBuilderBase;
            pageBuilder?.Clear();
        }
    }

    private static void initInfoText(
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

    private static void infoLerp(
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
}
