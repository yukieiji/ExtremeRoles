﻿using System.Collections.Generic;

using Hazel;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Combination
{
    public class Avalon : ConstCombinationRoleManagerBase
    {
        public const string Name = "AvalonsRoles";
        public Color SettingColor = Palette.White;
        public Avalon() : base(
            Name, new Color(255f, 255f, 255f), 2,
            OptionHolder.MaxImposterNum)
        {
            this.Roles.Add(new Assassin());
            this.Roles.Add(new Marlin());
        }

    }

    public class Assassin : MultiAssignRoleBase
    {
        public enum AssassinOption
        {
            IsDeadForceMeeting,
            CanSeeRoleBeforeFirstMeeting,
        }

        public bool IsFirstMeeting = false;
        public bool CanSeeRoleBeforeFirstMeeting = false;
        public bool IsDeadForceMeeting = true;

        public Assassin(
            ) : base(
                ExtremeRoleId.Assassin,
                ExtremeRoleType.Impostor,
                ExtremeRoleId.Assassin.ToString(),
                Palette.ImpostorRed,
                true, false, true, true)
        {}
        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {

            var meetingOpt = CustomOption.Create(
                GetRoleOptionId((int)AssassinOption.IsDeadForceMeeting),
                Design.ConcatString(
                    this.RoleName,
                    AssassinOption.IsDeadForceMeeting.ToString()),
                true, parentOps);
            CustomOption.Create(
                GetRoleOptionId((int)AssassinOption.CanSeeRoleBeforeFirstMeeting),
                Design.ConcatString(
                    this.RoleName,
                    AssassinOption.CanSeeRoleBeforeFirstMeeting.ToString()),
                false, meetingOpt);
        }

        public override void ExiledAction(
            GameData.PlayerInfo rolePlayer)
        {
            tryAssassinateMarin(rolePlayer);
            this.IsFirstMeeting = false;
        }

        public override void RolePlayerKilledAction(
            PlayerControl rolePlayer,
            PlayerControl killerPlayer)
        {
            if (!this.IsDeadForceMeeting)
            {
                ExtremeRolesPlugin.GameDataStore.DeadedAssassin.Add(
                    rolePlayer.PlayerId);
                return; 
            }

            rpcAssassinMeetingTriggerOn(rolePlayer.PlayerId);
            killerPlayer.CmdReportDeadBody(rolePlayer.Data);
            this.IsFirstMeeting = false;
        }

        protected override void RoleSpecificInit()
        {
            this.IsDeadForceMeeting = OptionHolder.AllOption[
                GetRoleOptionId((int)AssassinOption.IsDeadForceMeeting)].GetValue();
            this.CanSeeRoleBeforeFirstMeeting = OptionHolder.AllOption[
                GetRoleOptionId((int)AssassinOption.CanSeeRoleBeforeFirstMeeting)].GetValue();
            this.IsFirstMeeting = true;
        }

        private void tryAssassinateMarin(
            GameData.PlayerInfo exileAssasin)
        {
            rpcAssassinMeetingTriggerOn(exileAssasin.PlayerId);

            MeetingRoomManager.Instance.AssignSelf(exileAssasin.Object, null);
            DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(exileAssasin.Object);
            exileAssasin.Object.RpcStartMeeting(null);
        }

        private void rpcAssassinMeetingTriggerOn(byte playerId)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                    PlayerControl.LocalPlayer.NetId,
                    (byte)RPCOperator.Command.AssasinSpecialMeetingOn,
                    Hazel.SendOption.Reliable, -1);
            writer.Write(playerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            AssassinMeetingTriggerOn(playerId);
        }

        public static void AssassinMeetingTriggerOn(
            byte playerId)
        {
            ExtremeRolesPlugin.GameDataStore.AssassinMeetingTrigger = true;
            ExtremeRolesPlugin.GameDataStore.ExiledAssassinId = playerId;
        }

    }


    public class Marlin : MultiAssignRoleBase, IRoleSpecialSetUp
    {
        public enum MarlinOption
        {
            HasTask,
            CanSeeVote,
            CanSeeNeutral,
            CanUseVent,
        }

        public bool IsAssassinate = false;
        public bool CanSeeVote = false;
        public bool CanSeeNeutral = false;

        private Dictionary<byte, PoolablePlayer> PlayerIcon = new Dictionary<byte, PoolablePlayer>();
        public Marlin(
            ) : base(
                ExtremeRoleId.Marlin,
                ExtremeRoleType.Crewmate,
                ExtremeRoleId.Marlin.ToString(),
                ColorPalette.MarineBlue,
                false, false, false, false)
        {}

        public void IntroBeginSetUp()
        {
            return;
        }

        public void IntroEndSetUp()
        {
            this.PlayerIcon = Player.CreatePlayerIcon();
            updateIcons();
        }

        public override Color GetTargetRoleSeeColor(
            SingleRoleBase targetRole,
            byte targetPlayerId)
        {
            if (targetRole.IsImposter())
            {
                return Palette.ImpostorRed;
            }
            else if (targetRole.IsNeutral() && this.CanSeeNeutral)
            {
                return Palette.DisabledGrey;
            }

            var multiAssignRole = targetRole as MultiAssignRoleBase;
            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {
                    return this.GetTargetRoleSeeColor(
                        multiAssignRole.AnotherRole,
                        targetPlayerId);
                }
            }

            return Palette.White;
        }


        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            CustomOption.Create(
                GetRoleOptionId((int)MarlinOption.HasTask),
                Design.ConcatString(
                    this.RoleName,
                    MarlinOption.HasTask.ToString()),
                false, parentOps);
            CustomOption.Create(
                GetRoleOptionId((int)MarlinOption.CanSeeVote),
                Design.ConcatString(
                    this.RoleName,
                    MarlinOption.CanSeeVote.ToString()),
                false, parentOps);
            CustomOption.Create(
                GetRoleOptionId((int)MarlinOption.CanSeeNeutral),
                Design.ConcatString(
                    this.RoleName,
                    MarlinOption.CanSeeNeutral.ToString()),
                false, parentOps);
            CustomOption.Create(
                GetRoleOptionId((int)MarlinOption.CanUseVent),
                Design.ConcatString(
                    this.RoleName,
                    MarlinOption.CanUseVent.ToString()),
                false, parentOps);
        }

        protected override void RoleSpecificInit()
        {
            this.IsAssassinate = false;

            this.HasTask = OptionHolder.AllOption[
                GetRoleOptionId((int)MarlinOption.HasTask)].GetValue();
            this.CanSeeVote = OptionHolder.AllOption[
                GetRoleOptionId((int)MarlinOption.CanSeeVote)].GetValue();
            this.CanSeeNeutral = OptionHolder.AllOption[
                GetRoleOptionId((int)MarlinOption.CanSeeNeutral)].GetValue();
            this.UseVent = OptionHolder.AllOption[
                GetRoleOptionId((int)MarlinOption.CanUseVent)].GetValue();
        }

        private void updateIcons()
        {
            int visibleCounter = 0;
            Vector3 bottomLeft = HudManager.Instance.UseButton.transform.localPosition;
            bottomLeft.x *= -1;
            bottomLeft += new Vector3(-0.25f, -0.25f, 0);

            foreach (KeyValuePair<byte, PoolablePlayer> item in this.PlayerIcon)
            {
                byte playerId = item.Key;
                var poolPlayer = item.Value;
                if (playerId == PlayerControl.LocalPlayer.PlayerId) { continue; }

                PlayerControl player = Player.GetPlayerControlById(playerId);
                SingleRoleBase role = ExtremeRoleManager.GameRole[playerId];
                if (player.Data.IsDead ||
                    player.Data.Disconnected ||
                    role.IsCrewmate() ||
                    (role.IsNeutral() && !this.CanSeeNeutral))
                {
                    poolPlayer.gameObject.SetActive(false);
                }
                else
                {
                    poolPlayer.gameObject.SetActive(true);
                    poolPlayer.transform.localScale = Vector3.one * 0.25f;
                    poolPlayer.transform.localPosition = bottomLeft + Vector3.right * visibleCounter * 0.45f;
                    ++visibleCounter;
                }
            }
        }

    }
}
