using System;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.InfoOverlay.FullDec;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.Interface;

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
     
        private InfoOverlayBehaviour body;

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
        private GameObject prefab;

        private const string prefabName = "ExtremeRoles.Resources.Asset.infooverlay.asset";
        private const string objName = "assets/infooverlay.prefab";

        private static readonly Vector3 infoAnchorFirstPos = new Vector3(-4.0f, 1.6f, -910f);

        public InfoOverlay()
        {
            pageClear();
            this.overlayShown = false;
            this.isBlock = false;
            this.body = null;
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

        public void HideInfoOverlay()
        {
            if (this.body == null) { return; }

            if (!this.OverlayShown) { return; }

            var hudManager = FastDestroyableSingleton<HudManager>.Instance;

            if (hudManager == null) { return; }
            if (MeetingHud.Instance == null) { hudManager.SetHudActive(true); }

            this.overlayShown = false;
            var underlayTransparent = new Color(0.1f, 0.1f, 0.1f, 0.0f);
            var underlayOpaque = new Color(0.1f, 0.1f, 0.1f, 0.88f);

            hudManager.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
            {
                this.body.SetBkColor(
                    Color.Lerp(underlayOpaque, underlayTransparent, t));
                this.body.SetTextColor(Color.Lerp(Palette.White, Palette.ClearWhite, t));
                if (t >= 1.0f)
                {
                    this.body.gameObject.SetActive(false);
                }
            })));

            this.body.gameObject.transform.SetParent(hudManager.transform);
        }

        public void ResetOverlays()
        {
            HideInfoOverlay();
            pageClear();

            UnityEngine.Object.Destroy(this.body);
            this.body = null;
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

            if (!ExtremeRolesPlugin.ShipState.IsRoleSetUpEnd)
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

            if (this.prefab == null)
            {
                this.prefab = Asset.GetGameObjectFromAssetBundle(
                    prefabName, objName);

                if (this.prefab == null) { return false; }

                this.prefab.SetActive(false);
                UnityEngine.Object.DontDestroyOnLoad(this.prefab);
            }
            if (this.body == null)
            {
                GameObject infoObj = UnityEngine.Object.Instantiate(
                    this.prefab, hudManager.transform);
                this.body = infoObj.GetComponent<InfoOverlayBehaviour>();
                this.body.gameObject.SetActive(true);
                this.body.SetTextStyle(hudManager.TaskText);
            }

            // 一応消しておく
            this.body.gameObject.SetActive(false);

            return true;
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

            this.body.transform.SetParent(parent);
            this.body.SetBkColor(new Color(0.1f, 0.1f, 0.1f, 0.88f));

            this.body.SetGameOption(
                $"<size=200%>{Translation.GetString("gameOption")}</size>",
                CommonOption.GetGameOptionString());

            SetShowInfo(showType);

            var underlayTransparent = new Color(0.1f, 0.1f, 0.1f, 0.0f);
            var underlayOpaque = new Color(0.1f, 0.1f, 0.1f, 0.88f);
            hudManager.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
            {
                this.body.SetBkColor(
                    Color.Lerp(underlayTransparent, underlayOpaque, t));
                this.body.SetTextColor(Color.Lerp(Palette.ClearWhite, Palette.White, t));

                if (t >= 1.0f)
                {
                    this.body.gameObject.SetActive(true);
                }

            })));
        }
        private void updateShowText()
        {
            var (title, roleText, anotherRoleText) = this.showText[this.curShow].GetShowText();

            this.body.UpdateBasicInfo(title, roleText);
            this.body.UpdateAditionalInfo("", anotherRoleText);
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
