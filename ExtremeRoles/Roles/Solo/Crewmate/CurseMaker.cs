using System;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.RoleAbilityButton;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public class CurseMaker : SingleRoleBase, IRoleAbility, IRoleMurderPlayerHock, IRoleUpdate
    {
        public enum CurseMakerOption
        {
            CursingRange,
            AdditionalKillCool,
            IsDeadBodySearch,
            SearchDeadBodyTime,
        }

        public class DeadBodyInfo
        {
            private DateTime killedTime;

            private byte killerPlayerId;
            private byte targetPlayerId;

            public DeadBodyInfo(
                PlayerControl killer,
                PlayerControl target)
            {
                this.killerPlayerId = killer.PlayerId;
                this.targetPlayerId = target.PlayerId;
                this.killedTime = DateTime.UtcNow;
            }

            public float ComputeDeltaTime()
            {
                TimeSpan deltaTime = DateTime.UtcNow - this.killedTime;
                return (float)deltaTime.TotalSeconds;
            }

            public DeadBody GetDeadBody()
            {
                DeadBody[] array = getAllDeadBody();
                for (int i = 0; i < array.Length; ++i)
                {
                    if (GameData.Instance.GetPlayerById(
                            array[i].ParentId).PlayerId == this.targetPlayerId)
                    {
                        return array[i];
                    }
                }

                return null;
            }

            public byte GetKiller() => this.killerPlayerId;

            public byte GetTarget() => this.targetPlayerId;

            public bool IsValid()
            {
                DeadBody[] array = getAllDeadBody();
                for (int i = 0; i < array.Length; ++i)
                {
                    if (GameData.Instance.GetPlayerById(
                            array[i].ParentId).PlayerId == this.targetPlayerId)
                    {
                        return true;
                    }
                }

                return false;
            }

            private DeadBody[] getAllDeadBody() => UnityEngine.Object.FindObjectsOfType<
                DeadBody>();


        }

        private Dictionary<byte, Arrow> deadBodyArrow = new Dictionary<byte, Arrow>();
        private Dictionary<byte, DeadBodyInfo> deadBodyData = new Dictionary<byte, DeadBodyInfo>();

        private GameData.PlayerInfo targetBody;
        private byte deadBodyId;

        private bool isDeadBodySearch = false;
        private bool isDeadBodySearchUsed = false;

        private float additionalKillCool = 1.0f;
        private float searchDeadBodyTime = 1.0f;
        private float deadBodyCheckRange = 1.0f;

        private string defaultButtonText;
        private string cursingText;

        public RoleAbilityButtonBase Button
        {
            get => this.curseButton;
            set
            {
                this.curseButton = value;
            }
        }
        private RoleAbilityButtonBase curseButton;

        public CurseMaker() : base(
            ExtremeRoleId.CurseMaker,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.CurseMaker.ToString(),
            ColorPalette.CurseMakerViolet,
            false, true, false, false)
        { }

        public static void CurseKillCool(
            byte rolePlayerId, byte targetPlayerId)
        {
            PlayerControl player = Player.GetPlayerControlById(targetPlayerId);

            if (player == null) { return; }
            if (player.Data.IsDead || player.Data.Disconnected) { return; }

            var curseMaker = ExtremeRoleManager.GetSafeCastedRole<CurseMaker>(
                rolePlayerId);

            var role = ExtremeRoleManager.GetLocalPlayerRole();

            float baseKillCool = PlayerControl.GameOptions.KillCooldown;

            if (role.HasOtherKillCool)
            {
                baseKillCool = role.KillCoolTime;
            }
            role.HasOtherKillCool = true;
            role.KillCoolTime = baseKillCool + curseMaker.additionalKillCool;

            player.killTimer = role.KillCoolTime;
        }

        public void CreateAbility()
        {
            this.defaultButtonText = Translation.GetString("curse");

            this.CreateAbilityCountButton(
                this.defaultButtonText,
                Loader.CreateSpriteFromResources(
                    Path.TestButton),
                checkAbility: CheckAbility,
                abilityCleanUp: CleanUp);
            this.Button.SetLabelToCrewmate();
        }

        public bool IsAbilityUse()
        {
            this.targetBody = Player.GetDeadBodyInfo(
                this.deadBodyCheckRange);
            return this.IsCommonUse() && this.targetBody != null;
        }

        public void CleanUp()
        {

            var rolePlayer = PlayerControl.LocalPlayer;

            RPCOperator.Call(
                rolePlayer.NetId,
                RPCOperator.Command.CleanDeadBody,
                new List<byte> { this.deadBodyId });
            RPCOperator.CleanDeadBody(this.deadBodyId);

            // 矢印消す
            if (this.deadBodyArrow.ContainsKey(this.deadBodyId))
            {
                deadBodyArrow[this.deadBodyId].Clear();
                deadBodyArrow.Remove(this.deadBodyId);
            }

            // 殺したやつを呪う
            DeadBodyInfo deadbodyInfo = this.deadBodyData[this.deadBodyId];
            byte killer = deadbodyInfo.GetKiller();
            byte target = deadbodyInfo.GetTarget();
            if (killer == target)
            {
                this.deadBodyId = byte.MaxValue;
                return; 
            }

            RPCOperator.Call(
                rolePlayer.NetId,
                RPCOperator.Command.CuresMakerCurseKillCool,
                new List<byte>
                {
                    rolePlayer.PlayerId,
                    killer, 
                });
            CurseKillCool(rolePlayer.PlayerId, killer);
            this.deadBodyId = byte.MaxValue;

        }

        public bool CheckAbility()
        {
            this.targetBody = Player.GetDeadBodyInfo(
                this.deadBodyCheckRange);

            bool result;

            if (this.targetBody == null)
            {
                result = false;
            }
            else
            {
                result = this.deadBodyId == this.targetBody.PlayerId;
            }

            this.Button.ButtonText = result ? this.cursingText : this.defaultButtonText;

            return result;
        }

        public bool UseAbility()
        {
            this.deadBodyId = this.targetBody.PlayerId;
            return true;
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {

            CustomOption.Create(
                GetRoleOptionId((int)CurseMakerOption.CursingRange),
                string.Concat(
                    this.RoleName,
                    CurseMakerOption.CursingRange.ToString()),
                2.5f, 0.5f, 5.0f, 0.5f,
                parentOps);

            CustomOption.Create(
                GetRoleOptionId((int)CurseMakerOption.AdditionalKillCool),
                string.Concat(
                    this.RoleName,
                    CurseMakerOption.AdditionalKillCool.ToString()),
                5.0f, 1.0f, 30.0f, 0.1f,
                parentOps, format: OptionUnit.Second);

            this.CreateAbilityCountOption(
                parentOps, 1, 3, 5.0f);

            var searchDeadBodyOption = CustomOption.Create(
                GetRoleOptionId((int)CurseMakerOption.IsDeadBodySearch),
                string.Concat(
                    this.RoleName,
                    CurseMakerOption.IsDeadBodySearch.ToString()),
                true, parentOps);

            CustomOption.Create(
                GetRoleOptionId((int)CurseMakerOption.SearchDeadBodyTime),
                string.Concat(
                    this.RoleName,
                    CurseMakerOption.SearchDeadBodyTime.ToString()),
                60.0f, 45.0f, 90.0f, 0.5f,
                searchDeadBodyOption, format: OptionUnit.Second,
                invert: true,
                enableCheckOption: parentOps);

        }

        protected override void RoleSpecificInit()
        {
            this.RoleAbilityInit();

            var allOption = OptionHolder.AllOption;

            this.additionalKillCool = allOption[
                GetRoleOptionId((int)CurseMakerOption.AdditionalKillCool)].GetValue();
            this.deadBodyCheckRange = allOption[
                GetRoleOptionId((int)CurseMakerOption.CursingRange)].GetValue();
            this.isDeadBodySearch = allOption[
                GetRoleOptionId((int)CurseMakerOption.IsDeadBodySearch)].GetValue();
            this.searchDeadBodyTime = allOption[
                GetRoleOptionId((int)CurseMakerOption.SearchDeadBodyTime)].GetValue();

            this.cursingText = Translation.GetString("cursing");
            this.deadBodyData.Clear();
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            foreach (var arrow in deadBodyArrow.Values)
            {
                arrow.Clear();
            }

            deadBodyArrow.Clear();
            deadBodyData.Clear();
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void HockMuderPlayer(PlayerControl source, PlayerControl target)
        {
            if (this.isDeadBodySearchUsed && this.isDeadBodySearch) { return; }

            this.deadBodyData.Add(
                target.PlayerId,
                new DeadBodyInfo(source, target));

        }

        public void Update(PlayerControl rolePlayer)
        {
            foreach (var (playerId, arrow) in deadBodyArrow)
            {
                if (this.deadBodyData.ContainsKey(playerId))
                {
                    var deadBodyInfo = this.deadBodyData[playerId];

                    if (deadBodyInfo.IsValid())
                    {
                        arrow.UpdateTarget(
                            deadBodyInfo.GetDeadBody().transform.position);
                        arrow.Update();
                    }

                }
            }

            if (this.isDeadBodySearchUsed && this.isDeadBodySearch) { return; }

            List<byte> removeData = new List<byte>();

            foreach (var (playerId, deadBodyInfo) in this.deadBodyData)
            {
                if (deadBodyInfo.IsValid())
                {
                    if (deadBodyInfo.ComputeDeltaTime() > this.searchDeadBodyTime && 
                        !this.deadBodyArrow.ContainsKey(playerId))
                    {
                        var arrow = new Arrow(this.NameColor);
                        this.deadBodyArrow.Add(playerId, arrow);
                    }
                }
                else
                {
                    removeData.Add(playerId);
                }
            }

            foreach (var deadBodyInfo in removeData)
            {
                this.deadBodyData.Remove(deadBodyInfo);
            }

        }
    }
}
