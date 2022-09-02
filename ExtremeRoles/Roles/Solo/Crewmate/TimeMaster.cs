using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public sealed class TimeMaster : SingleRoleBase, IRoleAbility
    {
        public enum TimeMasterOption
        {
            RewindTime
        }

        public enum TimeMasterOps : byte
        {
            ShieldOff,
            ShieldOn,
            RewindTime,
            ResetMeeting,
        }

        public RoleAbilityButtonBase Button
        { 
            get => this.timeShieldButton;
            set
            {
                this.timeShieldButton = value;
            }
        }

        private bool isRewindTime = false;
        private bool isShieldOn = false;
        private SpriteRenderer rewindScreen;
        private RoleAbilityButtonBase timeShieldButton;
        private static RewindStatus rewindState;

        private sealed class RewindStatus
        {
            public bool IsNotSafePos => this.isNotSafePos;
            public Vector3 SafePos => this.safePos;
            public Vector3 PrevPos => this.prevPos;
            public byte RewindCallPlayerId => this.rolePlayerId;

            public bool Enable => this.rewindingTrigger;

            private int skipFrame;
            private int historyFrame;
            private int frameCount;
            private int rewindFrame;
            private Vector3 prevPos;
            private Vector3 safePos;
            private bool isNotSafePos;
            private byte rolePlayerId;
            private TimeMaster rewindingTM;
            private bool rewindingTrigger;

            public RewindStatus(
                byte rolePlayerId,
                TimeMaster tm,
                Vector3 localPlayerPos)
            {
                this.rewindingTM = tm;
                this.rolePlayerId = rolePlayerId;
                this.skipFrame = 0;
                this.historyFrame = ExtremeRolesPlugin.GameDataStore.History.GetSize();
                this.frameCount = 0;
                this.rewindFrame = 0;
                this.prevPos = localPlayerPos;
                this.safePos = prevPos;
                this.isNotSafePos = false;
                this.rewindingTrigger = false;
            }

            public void UpdatePrevPostion(Vector3 newPos, bool isSafe)
            {
                this.prevPos = newPos;
                this.isNotSafePos = !isSafe;
                if (isSafe)
                {
                    this.safePos = newPos;
                }
            }

            public bool IsEndRewind()
            {
                ++this.frameCount;
                return this.rewindFrame <= this.frameCount;
            }

            public void SkipThisFrame()
            {
                ++this.skipFrame;
                this.prevPos = CachedPlayerControl.LocalPlayer.transform.position;
                this.safePos = prevPos;
                this.isNotSafePos = false;
            }

            public void SetActiveRewind()
            {
                this.rewindingTrigger = true;
                this.rewindFrame = this.historyFrame - this.skipFrame;
            }

            public void Reset()
            {
                this.rewindingTM.isRewindTime = false;
                this.rewindingTM.rewindScreen.enabled = false;
                this.rewindingTM = null;
                this.skipFrame = 0;
                this.historyFrame = 0;
                this.frameCount = 0;
                this.rewindFrame = 0;
                this.prevPos = Vector3.zero;
                this.safePos = Vector3.zero;
                this.isNotSafePos = false;
                this.rolePlayerId = byte.MinValue;
                this.rewindingTrigger = false;
            }
        }

        public TimeMaster() : base(
            ExtremeRoleId.TimeMaster,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.TimeMaster.ToString(),
            ColorPalette.TimeMasterBlue,
            false, true, false, false)
        { }

        public static void Ability(ref MessageReader reader)
        {
            byte tmPlayerId = reader.ReadByte();
            TimeMasterOps ops = (TimeMasterOps)reader.ReadByte();
            switch (ops)
            {
                case TimeMasterOps.ShieldOff:
                    shieldOff(tmPlayerId);
                    break;
                case TimeMasterOps.ShieldOn:
                    shieldOn(tmPlayerId);
                    break;
                case TimeMasterOps.RewindTime:
                    startRewind(tmPlayerId);
                    break;
                case TimeMasterOps.ResetMeeting:
                    resetMeeting(tmPlayerId);
                    break;
                default:
                    break;
            }
        }

        public static void RewindCleanUp()
        {
            PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;

            // 最後の巻き戻しが壁抜けする座標だった場合、壁抜けしない安全な場所に飛ばす
            if (rewindState.IsNotSafePos)
            {
                localPlayer.transform.position = rewindState.SafePos;
            }

            localPlayer.moveable = true;

            ExtremeRolesPlugin.GameDataStore.History.DataClear();
            ExtremeRolesPlugin.GameDataStore.History.BlockAddHistory = false;

            rewindState.Reset();
            rewindState = null;
        }

        public static void RewindPostion((Vector3, bool, bool, bool) hist)
        {
            PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;

            // 巻き戻しを開始する前に色々とチェック
            if (!rewindState.Enable)
            {
                // 梯子とか使っている最中に巻き戻すと色々とおかしくなる
                // => その処理が終わるまで待機、巻き戻しはその後
                //    ただし、処理が終わるまでの間の時間巻き戻し時間は短くなる
                // この時巻き戻しの処理はまだ行ってないのでトリガーはオフっとく
                if (!localPlayer.inVent &&
                    !localPlayer.moveable)
                {
                    rewindState.SkipThisFrame();
                    return;
                }

                // 問題がなければ巻き戻し自体の処理開始 => トリガーをオンにしておく
                rewindState.SetActiveRewind();
            }

            if (rewindState.IsEndRewind())
            {
                return;
            }

            if (localPlayer.PlayerId == rewindState.RewindCallPlayerId)
            {
                return;
            }

            localPlayer.moveable = false;

            if (localPlayer.Data.IsDead)
            {
                localPlayer.transform.position = hist.Item1;
            }
            else
            {
                if (localPlayer.inVent)
                {
                    foreach (Vent vent in CachedShipStatus.Instance.AllVents)
                    {
                        vent.CanUse(localPlayer.Data, out bool canUse, out _);
                        if (canUse)
                        {
                            localPlayer.MyPhysics.RpcExitVent(vent.Id);
                            vent.SetButtons(false);
                        }
                    }
                }

                Vector3 newPos = hist.Item1;
                Vector2 offset = localPlayer.Collider.offset;
                Vector3 newTruePos = new Vector3(
                    newPos.x + offset.x,
                    newPos.y + offset.y,
                    newPos.z);
                Vector3 prevTruePos = new Vector3(
                    rewindState.PrevPos.x + offset.x,
                    rewindState.PrevPos.y + offset.y,
                    newPos.z);

                bool isAnythingBetween = PhysicsHelpers.AnythingBetween(
                    prevTruePos, newTruePos,
                    Constants.ShipAndAllObjectsMask, false);

                // (間に何もない and 動ける) or ベント内だったの座標だった場合
                // => 巻き戻しかつ、安全な座標を更新
                if ((!isAnythingBetween && hist.Item2) || hist.Item3)
                {
                    localPlayer.transform.position = newPos;
                    rewindState.UpdatePrevPostion(newPos, true);
                }
                // 何か使っている時の座標(梯子、移動床等)
                // => 巻き戻すが、安全ではない(壁抜けする)座標として記録
                else if (hist.Item4)
                {
                    localPlayer.transform.position = newPos;
                    rewindState.UpdatePrevPostion(newPos, false);
                }
                else
                {
                    localPlayer.transform.position = rewindState.PrevPos;
                }
            }
        }

        private static void startRewind(byte playerId)
        {
            if (ExtremeRolesPlugin.GameDataStore.History.BlockAddHistory) { return; }

            PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;

            // Enable rewind
            var timeMaster = ExtremeRoleManager.GetSafeCastedRole<TimeMaster>(playerId);
            if (timeMaster == null) { return; }

            timeMaster.isRewindTime = true;

            ExtremeRolesPlugin.GameDataStore.History.BlockAddHistory = true;

            // Screen Initialize
            if (timeMaster.rewindScreen == null)
            {
                timeMaster.rewindScreen = Object.Instantiate(
                     FastDestroyableSingleton<HudManager>.Instance.FullScreen,
                     FastDestroyableSingleton<HudManager>.Instance.transform);
                timeMaster.rewindScreen.transform.localPosition = new Vector3(0f, 0f, 20f);
                timeMaster.rewindScreen.gameObject.SetActive(true);
                timeMaster.rewindScreen.enabled = false;
                timeMaster.rewindScreen.color = new Color(0f, 0.5f, 0.8f, 0.3f);
            }
            // Screen On
            timeMaster.rewindScreen.enabled = true;

            // SetUp
            if (MapBehaviour.Instance)
            {
                MapBehaviour.Instance.Close();
            }
            if (Minigame.Instance)
            {
                Minigame.Instance.ForceClose();
            }

            rewindState = new RewindStatus(
                playerId,
                timeMaster,
                localPlayer.transform.position);

            Logging.Debug(
                $"History Size:{ExtremeRolesPlugin.GameDataStore.History.GetSize()}");

            Patches.PlayerControlFixedUpdatePatch.SetNewPosionSetter(
                ExtremeRolesPlugin.GameDataStore.History.GetAllHistory(),
                Patches.PlayerControlFixedUpdatePatch.PostionSetType.TimeMaster);
        }

        private static void shieldOn(byte playerId)
        {
            TimeMaster timeMaster = ExtremeRoleManager.GetSafeCastedRole<TimeMaster>(playerId);
            
            if (timeMaster != null)
            {
                timeMaster.isShieldOn = true; 
            }
        }

        private static void shieldOff(byte playerId)
        {
            TimeMaster timeMaster = ExtremeRoleManager.GetSafeCastedRole<TimeMaster>(playerId);

            if (timeMaster != null)
            {
                timeMaster.isShieldOn = false;
            }
        }
        private static void resetMeeting(byte playerId)
        {
            TimeMaster timeMaster = ExtremeRoleManager.GetSafeCastedRole<TimeMaster>(playerId);

            if (timeMaster == null) { return; }
            
            timeMaster.isShieldOn = false;
            timeMaster.isRewindTime = false;
            if (timeMaster.rewindScreen != null)
            {
                timeMaster.rewindScreen.enabled = false;
            }

            CachedPlayerControl.LocalPlayer.PlayerControl.moveable = true;

            if (rewindState != null)
            {
                rewindState.Reset();
                rewindState = null;
            }

            ExtremeRolesPlugin.GameDataStore.History.BlockAddHistory = false;
        }

        public void CleanUp()
        {
            RPCOperator.Call(
                CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                RPCOperator.Command.TimeMasterAbility,
                new List<byte>
                {
                   CachedPlayerControl.LocalPlayer.PlayerId,
                   (byte)TimeMasterOps.ShieldOff,
                });
            shieldOff(CachedPlayerControl.LocalPlayer.PlayerId);
        }

        public void CreateAbility()
        {
            this.CreateNormalAbilityButton(
                Translation.GetString("timeShield"),
                Loader.CreateSpriteFromResources(
                   Path.TimeMasterTimeShield),
                abilityCleanUp: this.CleanUp);
            this.Button.SetLabelToCrewmate();
        }

        public bool UseAbility()
        {
            RPCOperator.Call(
                CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                RPCOperator.Command.TimeMasterAbility,
                new List<byte>
                {
                    CachedPlayerControl.LocalPlayer.PlayerId,
                    (byte)TimeMasterOps.ShieldOn,
                });
            shieldOn(CachedPlayerControl.LocalPlayer.PlayerId);

            return true;
        }

        public bool IsAbilityUse() => this.IsCommonUse();

        public void RoleAbilityResetOnMeetingStart()
        {
            RPCOperator.Call(
                CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                RPCOperator.Command.TimeMasterAbility,
                new List<byte>
                {
                    CachedPlayerControl.LocalPlayer.PlayerId,
                    (byte)TimeMasterOps.ResetMeeting,
                });
            resetMeeting(CachedPlayerControl.LocalPlayer.PlayerId);
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public override bool TryRolePlayerKilledFrom(
            PlayerControl rolePlayer, PlayerControl fromPlayer)
        {
            if (this.isRewindTime) { return false; }

            if (this.isShieldOn)
            {
                RPCOperator.Call(
                    CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                    RPCOperator.Command.TimeMasterAbility,
                    new List<byte>
                    {
                        rolePlayer.PlayerId,
                        (byte)TimeMasterOps.RewindTime,
                    });
                startRewind(rolePlayer.PlayerId);

                return false;
            }

            return true;
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            this.CreateCommonAbilityOption(
                parentOps, 3.0f);

            CreateFloatOption(
                TimeMasterOption.RewindTime,
                5.0f, 1.0f, 60.0f, 0.5f,
                parentOps, format: OptionUnit.Second);
        }

        protected override void RoleSpecificInit()
        {
            ExtremeRolesPlugin.GameDataStore.History.Initialize(
                OptionHolder.AllOption[
                    GetRoleOptionId(TimeMasterOption.RewindTime)].GetValue());
            this.RoleAbilityInit();
        }
    }
}
