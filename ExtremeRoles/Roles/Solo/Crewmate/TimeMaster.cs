using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.RoleAbilityButton;

using BepInEx.IL2CPP.Utils.Collections;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public class TimeMaster : SingleRoleBase, IRoleAbility
    {
        public enum TimeMasterOption
        {
            RewindTime
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

        public TimeMaster() : base(
            ExtremeRoleId.TimeMaster,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.TimeMaster.ToString(),
            ColorPalette.TimeMasterBlue,
            false, true, false, false)
        { }

        public static void TimeRewind(byte rolePlayerId)
        {
            if (ExtremeRolesPlugin.GameDataStore.History.BlockAddHistory) { return; }

            var localPlayer = PlayerControl.LocalPlayer;
            localPlayer.StartCoroutine(rewind(
                rolePlayerId, localPlayer).WrapToIl2Cpp());

        }

        private static IEnumerator rewind(
            byte rolePlayerId,
            PlayerControl localPlayer)
        {
            // Enable rewind
            var timeMaster = (TimeMaster)ExtremeRoleManager.GameRole[rolePlayerId];
            timeMaster.IsRewindTime = true;

            ExtremeRolesPlugin.GameDataStore.History.BlockAddHistory = true;

            // Screen Initialize
            if (timeMaster.RewindScreen == null)
            {
                timeMaster.RewindScreen = UnityEngine.Object.Instantiate(
                     HudManager.Instance.FullScreen, HudManager.Instance.transform);
                timeMaster.RewindScreen.transform.localPosition = new Vector3(0f, 0f, 20f);
                timeMaster.RewindScreen.gameObject.SetActive(true);
                timeMaster.RewindScreen.enabled = false;
                timeMaster.RewindScreen.color = new Color(0f, 0.5f, 0.8f, 0.3f);
            }
            // Screen On
            timeMaster.RewindScreen.enabled = true;

            // SetUp
            if (MapBehaviour.Instance)
            {
                MapBehaviour.Instance.Close();
            }
            if (Minigame.Instance)
            {
                Minigame.Instance.ForceClose();
            }


            // 梯子とか使っている最中に巻き戻すと色々とおかしくなる
            // => その処理が終わるまで待機、巻き戻しはその後
            //    ただし、処理が終わるまでの間の時間巻き戻し時間は短くなる
            int skipFrame = 0;
            if (!localPlayer.inVent && !localPlayer.moveable)
            {
                do
                {
                    yield return null;
                    ++skipFrame;
                }
                while (!localPlayer.moveable);
            }

            int rewindFrame = ExtremeRolesPlugin.GameDataStore.History.GetSize() - skipFrame;

            Logging.Debug(
                $"History Size:{ExtremeRolesPlugin.GameDataStore.History.GetSize()}   SkipFrame:{skipFrame}");

            Vector3 prevPos = localPlayer.transform.position;
            Vector3 sefePos = prevPos;
            bool isNotSafePos = false;
            int frameCount = 0;

            // Rewind Main Process
            foreach (var item in ExtremeRolesPlugin.GameDataStore.History.GetAllHistory())
            {
                if (rewindFrame == frameCount) { break; }

                yield return null;

                if (localPlayer.PlayerId == rolePlayerId) { continue; }

                ++frameCount;

                localPlayer.moveable = false;

                if (localPlayer.Data.IsDead)
                {
                    localPlayer.transform.position = item.Item1;
                }
                else
                {
                    if (localPlayer.inVent)
                    {
                        foreach (Vent vent in ShipStatus.Instance.AllVents)
                        {
                            bool canUse;
                            bool couldUse;
                            vent.CanUse(PlayerControl.LocalPlayer.Data, out canUse, out couldUse);
                            if (canUse)
                            {
                                PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(vent.Id);
                                vent.SetButtons(false);
                            }
                        }
                    }

                    Vector3 newPos = item.Item1;
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

                    // (間に何もない and 動ける) or ベント内だった場合
                    // => 巻き戻しかつ、安全な座標を更新
                    if ((!isAnythingBetween && item.Item2) || item.Item3)
                    {
                        localPlayer.transform.position = newPos;
                        prevPos = newPos;
                        sefePos = newPos;
                        isNotSafePos = false;
                    }
                    // 何か使っている時(梯子、移動床等)
                    // => 巻き戻すが、安全ではない(壁抜けする)座標として記録
                    else if (item.Item4)
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

            // 最後の巻き戻しが壁抜けする座標だった場合、壁抜けしない場所に飛ばす
            if (isNotSafePos)
            {
                localPlayer.transform.position = sefePos;
            }

            localPlayer.moveable = true;
            timeMaster.IsRewindTime = false;
            timeMaster.RewindScreen.enabled = false;

            ExtremeRolesPlugin.GameDataStore.History.DataClear();
            ExtremeRolesPlugin.GameDataStore.History.BlockAddHistory = false;
        }

        public static void ShieldOn(byte rolePlayerId)
        {
            ((TimeMaster)ExtremeRoleManager.GameRole[rolePlayerId]).IsShieldOn = true;
        }

        public static void ShieldOff(byte rolePlayerId)
        {
            ((TimeMaster)ExtremeRoleManager.GameRole[rolePlayerId]).IsShieldOn = false;
        }

        public static void ResetMeeting(byte rolePlayerId)
        {
            var timeMaster = (TimeMaster)ExtremeRoleManager.GameRole[rolePlayerId];
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
                PlayerControl.LocalPlayer.NetId,
                RPCOperator.Command.TimeMasterShieldOff,
                new List<byte>
                {
                    PlayerControl.LocalPlayer.PlayerId,
                });
            ShieldOff(PlayerControl.LocalPlayer.PlayerId);
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
                PlayerControl.LocalPlayer.NetId,
                RPCOperator.Command.TimeMasterShieldOn,
                new List<byte>
                {
                    PlayerControl.LocalPlayer.PlayerId,
                });
            ShieldOn(PlayerControl.LocalPlayer.PlayerId);

            return true;
        }

        public bool IsAbilityUse() => this.IsCommonUse();

        public void RoleAbilityResetOnMeetingStart()
        {
            RPCOperator.Call(
                PlayerControl.LocalPlayer.NetId,
                RPCOperator.Command.TimeMasterResetMeeting,
                new List<byte>
                {
                    PlayerControl.LocalPlayer.PlayerId,
                });
            ResetMeeting(PlayerControl.LocalPlayer.PlayerId);
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
                    PlayerControl.LocalPlayer.NetId,
                    RPCOperator.Command.TimeMasterRewindTime,
                    new List<byte>
                    {
                        rolePlayer.PlayerId,
                    });
                TimeRewind(rolePlayer.PlayerId);

                return false;
            }

            return true;
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            this.CreateCommonAbilityOption(
                parentOps, 3.0f);

            CustomOption.Create(
                GetRoleOptionId((int)TimeMasterOption.RewindTime),
                string.Concat(
                    this.RoleName,
                    TimeMasterOption.RewindTime),
                5.0f, 1.0f, 60.0f, 0.5f,
                parentOps, format: "unitSeconds");
        }

        protected override void RoleSpecificInit()
        {
            ExtremeRolesPlugin.GameDataStore.History.Initialize(
                OptionHolder.AllOption[
                    GetRoleOptionId((int)TimeMasterOption.RewindTime)].GetValue());
            this.RoleAbilityInit();
        }
    }
}
