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
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Performance;

using BepInEx.Unity.IL2CPP.Utils.Collections;

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
        private RoleAbilityButtonBase timeShieldButton;

        private bool isRewindTime = false;
        private bool isShieldOn = false;
        private SpriteRenderer rewindScreen;

        private static TimeMasterHistory history;

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

        public static void ResetHistory()
        {
            history = null;
        }

        private static void startRewind(byte playerId)
        {
            if (history.BlockAddHistory) { return; }

            history.StartCoroutine(
                coRewind(playerId, CachedPlayerControl.LocalPlayer).WrapToIl2Cpp());
        }

        private static IEnumerator coRewind(
            byte rolePlayerId, PlayerControl localPlayer)
        {

            // Enable rewind
            var timeMaster = ExtremeRoleManager.GetSafeCastedRole<TimeMaster>(rolePlayerId);
            if (timeMaster == null) { yield break; }
            timeMaster.isRewindTime = true;

            history.SetAddHistoryBlock(true);

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

            float time = Time.fixedDeltaTime;

            // 梯子とか使っている最中に巻き戻すと色々とおかしくなる
            // => その処理が終わるまで待機、巻き戻しはその後
            //    ただし、処理が終わるまでの間の時間巻き戻し時間は短くなる
            int skipFrame = 0;
            if (!localPlayer.inVent && !localPlayer.moveable)
            {
                do
                {
                    yield return new WaitForSeconds(time);
                    ++skipFrame;
                }
                while (!localPlayer.moveable);
            }

            int rewindFrame = history.GetSize() - skipFrame;

            Logging.Debug($"History Size:{history.GetSize()}   SkipFrame:{skipFrame}");

            Vector3 prevPos = localPlayer.transform.position;
            Vector3 sefePos = prevPos;
            bool isNotSafePos = false;
            int frameCount = 0;

            // Rewind Main Process
            foreach ((Vector3, bool, bool, bool) hist in history.GetAllHistory())
            {
                if (rewindFrame == frameCount) { break; }

                yield return new WaitForSeconds(time);

                if (localPlayer.PlayerId == rolePlayerId) { continue; }

                ++frameCount;

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
                            bool canUse;
                            bool couldUse;
                            vent.CanUse(
                                localPlayer.Data,
                                out canUse, out couldUse);
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

            // 最後の巻き戻しが壁抜けする座標だった場合、壁抜けしない安全な場所に飛ばす
            if (isNotSafePos)
            {
                localPlayer.transform.position = sefePos;
            }

            localPlayer.moveable = true;
            timeMaster.isRewindTime = false;
            timeMaster.rewindScreen.enabled = false;

            history.ResetAfterRewind();
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

            // ヒストリーのコルーチン処理を止める
            history.StopAllCoroutines();

            timeMaster.isShieldOn = false;
            timeMaster.isRewindTime = false;
            if (timeMaster.rewindScreen != null)
            {
                timeMaster.rewindScreen.enabled = false;
            }

            // ヒストリーブロック解除
            history.SetAddHistoryBlock(false);

            // 会議開始後リウィンドのコルーチンが止まるまでポジションがバグるので
            // ここでポジションを上書きする => TMが発動してなくても通るが問題なし
            // それ以外でコードを追加してもいいが最も被害が少ない変更がここ
            CachedShipStatus.Instance.SpawnPlayer(
                CachedPlayerControl.LocalPlayer,
                GameData.Instance.PlayerCount, false);

            CachedPlayerControl.LocalPlayer.PlayerControl.moveable = true;
        }

        public void CleanUp()
        {
            PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;

            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.TimeMasterAbility))
            {
                caller.WriteByte(localPlayer.PlayerId);
                caller.WriteByte((byte)TimeMasterOps.ShieldOff);
            }
            shieldOff(localPlayer.PlayerId);
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
            PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;

            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.TimeMasterAbility))
            {
                caller.WriteByte(localPlayer.PlayerId);
                caller.WriteByte((byte)TimeMasterOps.ShieldOn);
            }
            shieldOn(localPlayer.PlayerId);

            return true;
        }

        public bool IsAbilityUse() => this.IsCommonUse();

        public void RoleAbilityResetOnMeetingStart()
        {

            PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;
            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.TimeMasterAbility))
            {
                caller.WriteByte(localPlayer.PlayerId);
                caller.WriteByte((byte)TimeMasterOps.ResetMeeting);
            }
            resetMeeting(localPlayer.PlayerId);
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
                using (var caller = RPCOperator.CreateCaller(
                    RPCOperator.Command.TimeMasterAbility))
                {
                    caller.WriteByte(rolePlayer.PlayerId);
                    caller.WriteByte((byte)TimeMasterOps.RewindTime);
                }
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
            this.RoleAbilityInit();

            if (history != null) { return; }

            history = ExtremeRolesPlugin.ShipState.Status.AddComponent<
                TimeMasterHistory>();
            history.Initialize(
                OptionHolder.AllOption[GetRoleOptionId(
                    TimeMasterOption.RewindTime)].GetValue());
        }
    }
}
