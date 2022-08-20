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

using BepInEx.IL2CPP.Utils.Collections;

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

        public bool IsRewindTime = false;
        public bool IsShieldOn = false;
        public SpriteRenderer RewindScreen;
        private RoleAbilityButtonBase timeShieldButton;

        private static int skipFrame;
        private static int historyFrame;
        private static int frameCount;
        private static Vector3 prevPos;
        private static Vector3 sefePos;
        private static bool isNotSafePos;
        private static byte rolePlayerId;
        private static TimeMaster rewindingTM;
        private static bool rewindingTrigger;

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
            if (isNotSafePos)
            {
                localPlayer.transform.position = sefePos;
            }

            localPlayer.moveable = true;
            rewindingTM.IsRewindTime = false;
            rewindingTM.RewindScreen.enabled = false;

            ExtremeRolesPlugin.GameDataStore.History.DataClear();
            ExtremeRolesPlugin.GameDataStore.History.BlockAddHistory = false;

            rewindingTM = null;
            skipFrame = 0;
            historyFrame = 0;
            frameCount = 0;
            prevPos = Vector3.zero;
            sefePos = Vector3.zero;
            isNotSafePos = false;
            rolePlayerId = byte.MinValue;
            rewindingTrigger = false;
        }

        public static void RewindPostion((Vector3, bool, bool, bool) hist)
        {
            PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;

            // 梯子とか使っている最中に巻き戻すと色々とおかしくなる
            // => その処理が終わるまで待機、巻き戻しはその後
            //    ただし、処理が終わるまでの間の時間巻き戻し時間は短くなる
            // この時巻き戻しの処理はまだ行ってないのでトリガーはオフっとく
            if (!rewindingTrigger && 
                !localPlayer.inVent && 
                !localPlayer.moveable)
            {
                ++skipFrame;
                
                prevPos = localPlayer.transform.position;
                sefePos = prevPos;
                isNotSafePos = false;

                return;
            }

            // 巻き戻し自体の処理開始 => トリガーをオンにしておく
            rewindingTrigger = true;

            ++frameCount;

            int rewindFrame = historyFrame - skipFrame;

            if (rewindFrame <= frameCount)
            {
                return;
            }

            if (localPlayer.PlayerId == rolePlayerId)
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
                    prevPos.x + offset.x,
                    prevPos.y + offset.y,
                    newPos.z);

                bool isAnythingBetween = PhysicsHelpers.AnythingBetween(
                    prevTruePos, newTruePos,
                    Constants.ShipAndAllObjectsMask, false);

                // (間に何もない and 動ける) or ベント内だったの座標だった場合
                // => 巻き戻しかつ、安全な座標を更新
                if ((!isAnythingBetween && hist.Item2) || hist.Item3)
                {
                    localPlayer.transform.position = newPos;
                    prevPos = newPos;
                    sefePos = newPos;
                    isNotSafePos = false;
                }
                // 何か使っている時の座標(梯子、移動床等)
                // => 巻き戻すが、安全ではない(壁抜けする)座標として記録
                else if (hist.Item4)
                {
                    localPlayer.transform.position = newPos;
                    prevPos = newPos;
                    isNotSafePos = true;
                }
                else
                {
                    localPlayer.transform.position = prevPos;
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

            rolePlayerId = playerId;
            rewindingTM = timeMaster;
            rewindingTM.IsRewindTime = true;

            ExtremeRolesPlugin.GameDataStore.History.BlockAddHistory = true;

            // Screen Initialize
            if (rewindingTM.RewindScreen == null)
            {
                rewindingTM.RewindScreen = UnityEngine.Object.Instantiate(
                     FastDestroyableSingleton<HudManager>.Instance.FullScreen,
                     FastDestroyableSingleton<HudManager>.Instance.transform);
                rewindingTM.RewindScreen.transform.localPosition = new Vector3(0f, 0f, 20f);
                rewindingTM.RewindScreen.gameObject.SetActive(true);
                rewindingTM.RewindScreen.enabled = false;
                rewindingTM.RewindScreen.color = new Color(0f, 0.5f, 0.8f, 0.3f);
            }
            // Screen On
            rewindingTM.RewindScreen.enabled = true;

            // SetUp
            if (MapBehaviour.Instance)
            {
                MapBehaviour.Instance.Close();
            }
            if (Minigame.Instance)
            {
                Minigame.Instance.ForceClose();
            }

            skipFrame = 0;
            historyFrame = ExtremeRolesPlugin.GameDataStore.History.GetSize();
            frameCount = 0;
            prevPos = localPlayer.transform.position;
            sefePos = prevPos;
            isNotSafePos = false;
            rewindingTrigger = false;

            Logging.Debug(
                $"History Size:{ExtremeRolesPlugin.GameDataStore.History.GetSize()}   SkipFrame:{skipFrame}");

            Patches.PlayerControlFixedUpdatePatch.SetNewPosionSetter(
                ExtremeRolesPlugin.GameDataStore.History.GetAllHistory(),
                Patches.PlayerControlFixedUpdatePatch.PostionSetType.TimeMaster);
        }

        private static void shieldOn(byte rolePlayerId)
        {
            TimeMaster timeMaster = ExtremeRoleManager.GetSafeCastedRole<TimeMaster>(rolePlayerId);
            
            if (timeMaster != null)
            {
                timeMaster.IsShieldOn = true; 
            }
        }

        private static void shieldOff(byte rolePlayerId)
        {
            TimeMaster timeMaster = ExtremeRoleManager.GetSafeCastedRole<TimeMaster>(rolePlayerId);

            if (timeMaster != null)
            {
                timeMaster.IsShieldOn = false;
            }
        }
        private static void resetMeeting(byte rolePlayerId)
        {
            TimeMaster timeMaster = ExtremeRoleManager.GetSafeCastedRole<TimeMaster>(rolePlayerId);

            if (timeMaster == null) { return; }
            
            timeMaster.IsShieldOn = false;
            timeMaster.IsRewindTime = false;
            if (timeMaster.RewindScreen != null)
            {
                timeMaster.RewindScreen.enabled = false;
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
            if (this.IsRewindTime) { return false; }

            if (this.IsShieldOn)
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
