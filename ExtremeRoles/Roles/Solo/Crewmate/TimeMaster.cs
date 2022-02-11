using System;
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
            var timeMaster = (TimeMaster)ExtremeRoleManager.GameRole[rolePlayerId];
            timeMaster.IsRewindTime = true;

            ExtremeRolesPlugin.GameDataStore.History.BlockAddHistory = true;

            if (timeMaster.RewindScreen == null)
            {
                timeMaster.RewindScreen = UnityEngine.Object.Instantiate(
                     HudManager.Instance.FullScreen, HudManager.Instance.transform);
                timeMaster.RewindScreen.transform.localPosition = new Vector3(0f, 0f, 20f);
                timeMaster.RewindScreen.gameObject.SetActive(true);
                timeMaster.RewindScreen.enabled = false;
                timeMaster.RewindScreen.color = new Color(0f, 0.5f, 0.8f, 0.3f);
            }

            timeMaster.RewindScreen.enabled = true;

            if (MapBehaviour.Instance)
            {
                MapBehaviour.Instance.Close();
            }
            if (Minigame.Instance)
            {
                Minigame.Instance.ForceClose();
            }

            Vector3 defaultPos = new Vector3(0f, 0f, 1000f);
            Vector3 prevPos = defaultPos;

            foreach (Tuple<Vector3, bool> item in ExtremeRolesPlugin.GameDataStore.History.GetAllHistory())
            {

                yield return null;

                if (localPlayer.PlayerId == rolePlayerId) { continue; }

                localPlayer.moveable = false;

                if (localPlayer.Data.IsDead)
                {
                    localPlayer.transform.position = item.Item1;
                }
                else
                {
                    // Exit current vent if necessary
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

                    if (prevPos != defaultPos)
                    {
                        if (PhysicsHelpers.AnythingBetween(
                                newPos, prevPos,
                                Constants.ShipAndObjectsMask, false))
                        {
                            localPlayer.transform.position = prevPos;
                        }
                        else
                        {
                            localPlayer.transform.position = newPos;
                            prevPos = newPos;
                        }
                    }
                    else
                    {
                        if (PhysicsHelpers.AnythingBetween(
                                localPlayer.transform.position, newPos,
                                Constants.ShipAndObjectsMask, false))
                        {
                            localPlayer.transform.position = prevPos;
                        }
                        else
                        {
                            localPlayer.transform.position = newPos;
                            prevPos = newPos;
                        }
                    }
                }
            }

            localPlayer.moveable = true;
            timeMaster.IsRewindTime = false;
            timeMaster.RewindScreen.enabled = false;

            ExtremeRolesPlugin.GameDataStore.History.BlockAddHistory = false;
            timeMaster.IsShieldOn = false;
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
            timeMaster.RewindScreen.enabled = false;

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
            this.CreateReclickableAbilityButton(
                Translation.GetString("timeShield"),
                Loader.CreateSpriteFromResources(
                   Path.TestButton),
                this.CleanUp);
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
                1, 1, 30, 1,
                parentOps);
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
