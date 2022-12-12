using System;
using System.Collections.Generic;
using UnityEngine;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Host.Button
{
    internal sealed class XionActionToPlayerButton
    {
        private sealed class PlayerActionButton : NoneCoolButtonBase
        {
            public PlayerActionButton(byte playerId, Transform parent)
            {
                var hudManager = FastDestroyableSingleton<HudManager>.Instance;

                this.ButtonAction = Xion.GetPlayerButtonAction(playerId);

                this.Body = UnityEngine.Object.Instantiate(
                    hudManager.KillButton, parent);
                PassiveButton button = Body.GetComponent<PassiveButton>();
                button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                button.OnClick.AddListener((UnityEngine.Events.UnityAction)OnClickEvent);
                SetActive(false);

                var useButton = hudManager.UseButton;

                this.Body.OverrideText("");
                this.Body.graphic.sprite = null;
                ResetCoolTimer();
            }
            public override void Update()
            {
                this.Body?.OverrideText("");
                base.Update();
            }
        }


        private enum PlayerState : byte
        {
            Alive,
            Dead
        }

        private GridArrange iconAnchor;
        private Dictionary<byte, PoolablePlayer> buttonIcons = new Dictionary<byte, PoolablePlayer>();

        private GridArrange buttonAnchor;
        private Dictionary<byte, PlayerActionButton> button = new Dictionary<byte, PlayerActionButton>();

        private Dictionary<byte, PlayerState> playerState = new Dictionary<byte, PlayerState>();

        public XionActionToPlayerButton(byte xionPlayerId)
        {
            this.iconAnchor = createAnchor("ButtonIconAnchor");
            this.buttonIcons = Helper.Player.CreatePlayerIcon(
                this.iconAnchor.gameObject.transform, Vector3.one * 0.275f);

            this.buttonAnchor = createAnchor("buttonAnchor");
            foreach (byte playerId in this.buttonIcons.Keys)
            {

                if (playerId == xionPlayerId) { continue; }
                GameData.PlayerInfo player = GameData.Instance.GetPlayerById(playerId);
                if (player == null || player.Disconnected)
                {
                    continue;
                }

                this.playerState[playerId] = player.IsDead ? PlayerState.Dead : PlayerState.Alive;
                this.button.Add(playerId,
                    new PlayerActionButton(
                        playerId, this.buttonAnchor.gameObject.transform));
            }
            this.updateButtonIcons(false);

            this.buttonAnchor.ArrangeChilds();
            this.iconAnchor.ArrangeChilds();
        }

        public void ResetCoolTime()
        {
            foreach (var button in this.button.Values)
            {
                button.ResetCoolTimer();
            }
        }

        public void SetActive(bool active)
        {
            foreach (var button in this.button.Values)
            {
                button.SetActive(active);
            }
            foreach (var pool in this.buttonIcons.Values)
            {
                pool.gameObject.SetActive(active);
            }
        }

        public void SetActive(byte playerId, bool active)
        {
            this.buttonIcons[playerId].gameObject.SetActive(active);
            this.button[playerId].SetActive(active);
        }

        public void Update(bool isHide)
        {
            foreach (var button in this.button.Values)
            {
                button.Update();
            }
            updateButtonIcons(isHide);
        }

        private void updateButtonIcons(bool isHideGui)
        {
            if (MeetingHud.Instance || ExileController.Instance) { return; }

            List<byte> remove = new List<byte>();
            bool updated = false;
            foreach (var (playerId, pool) in this.buttonIcons)
            {
                GameData.PlayerInfo player = GameData.Instance.GetPlayerById(playerId);
                if (player == null || player.Disconnected)
                {
                    SetActive(playerId, false);
                    remove.Add(playerId);
                    updated = true;
                    continue;
                }

                bool active = !isHideGui;
                SetActive(playerId, active);
                bool isDead = player.IsDead;
                PlayerState curState = isDead ? PlayerState.Dead : PlayerState.Alive;

                if (this.playerState[playerId] != curState)
                {
                    pool.UpdateFromPlayerData(
                        player, PlayerOutfitType.Default,
                        PlayerMaterial.MaskType.None, isDead);
                    updated = true;
                }
            }
            foreach (byte playerId in remove)
            {
                this.buttonIcons.Remove(playerId);
                this.button.Remove(playerId);
            }

            if (updated)
            {
                this.buttonAnchor.ArrangeChilds();
                this.iconAnchor.ArrangeChilds();
            }
        }

        private GridArrange createAnchor(string name)
        {
            GameObject bottomLeft = new GameObject(name);
            bottomLeft.transform.SetParent(
                FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.parent.parent);
            AspectPosition aspectPosition = bottomLeft.AddComponent<AspectPosition>();
            aspectPosition.Alignment = AspectPosition.EdgeAlignments.LeftBottom;
            aspectPosition.anchorPoint = new Vector2(0.5f, 0.5f);
            aspectPosition.DistanceFromEdge = new Vector3(0.375f, 0.35f);
            aspectPosition.AdjustPosition();

            GridArrange grid = bottomLeft.AddComponent<GridArrange>();
            grid.CellSize = new Vector2(0.625f, 0.75f);
            grid.MaxColumns = 10;
            grid.Alignment = GridArrange.StartAlign.Right;
            grid.cells = new();

            return grid;
        }
    }
}
