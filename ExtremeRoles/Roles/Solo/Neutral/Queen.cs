using System;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.Helper;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Neutral
{
    public class Queen : SingleRoleBase, IRoleAbility, IRoleSpecialReset, IRoleMurderPlayerHock, IRoleUpdate
    {
        public enum QueenOption
        {
            Range,
            CanUseVent,
            ServantKillKillCoolReduceRate,
            ServantTaskKillCoolReduceRate,
            ServantSelfKillCool
        }

        public List<byte> ServantPlayerId = new List<byte>();

        public RoleAbilityButtonBase Button
        {
            get => this.createServant;
            set
            {
                this.createServant = value;
            }
        }

        public PlayerControl Target;
        public float ServantSelfKillCool;
        private RoleAbilityButtonBase createServant;
        private float range;
        private float killKillCoolReduceRate;
        private float taskKillCoolReduceRate;
        private Dictionary<byte, float> servantTaskGage = new Dictionary<byte, float>();

        public Queen() : base(
            ExtremeRoleId.Queen,
            ExtremeRoleType.Neutral,
            ExtremeRoleId.Queen.ToString(),
            ColorPalette.QueenWhite,
            true, false, false, false)
        { }

        public static void TargetToServant(
            byte rolePlayerId, byte targetPlayerId)
        {

            Queen queen = ExtremeRoleManager.GetSafeCastedRole<Queen>(rolePlayerId);

            if (queen == null) { return; }

            var targetPlayer = Player.GetPlayerControlById(targetPlayerId);
            var targetRole = ExtremeRoleManager.GameRole[targetPlayerId];

            resetTargetAnotherRole(targetRole, targetPlayerId, targetPlayer);
            replaceVanilaRole(targetRole, targetPlayer);

            Servant servant = new Servant(
                rolePlayerId, queen, targetRole);

            if (CachedPlayerControl.LocalPlayer.PlayerId == targetPlayerId)
            {
                servant.SelfKillAbility(queen.ServantSelfKillCool);
                if (targetRole is IRoleAbility &&
                    targetRole.Team != ExtremeRoleType.Neutral)
                {
                    servant.Button.PositionOffset = new Vector3(0, 2.6f, 0);
                    servant.Button.ReplaceHotKey(KeyCode.C);
                }
            }

            if (targetRole.Team != ExtremeRoleType.Neutral)
            {
                var multiAssignRole = targetRole as MultiAssignRoleBase;
                if (multiAssignRole != null)
                {
                    multiAssignRole.Team = ExtremeRoleType.Neutral;
                    multiAssignRole.AnotherRole = null;
                    multiAssignRole.SetAnotherRole(servant);
                    setNewRole(multiAssignRole, targetPlayerId);
                }
                else
                {
                    targetRole.Team = ExtremeRoleType.Neutral;
                    servant.SetAnotherRole(targetRole);
                    setNewRole(servant, targetPlayerId);
                }
            }
            else
            {
                resetRole(targetRole, targetPlayerId, targetPlayer);
                setNewRole(servant, targetPlayerId);
            }
            queen.ServantPlayerId.Add(targetPlayerId);
        }

        private static void resetTargetAnotherRole(
            SingleRoleBase targetRole,
            byte targetPlayerId,
            PlayerControl targetPlayer)
        {
            var multiAssignRole = targetRole as MultiAssignRoleBase;
            if (multiAssignRole != null)
            {
                if (CachedPlayerControl.LocalPlayer.PlayerId == targetPlayerId)
                {
                    IRoleResetMeeting meetingResetRole = multiAssignRole.AnotherRole as IRoleResetMeeting;
                    if (meetingResetRole != null)
                    {
                        meetingResetRole.ResetOnMeetingStart();
                    }

                    IRoleAbility abilityRole = multiAssignRole.AnotherRole as IRoleAbility;
                    if (abilityRole != null)
                    {
                        abilityRole.ResetOnMeetingStart();
                    }
                }

                IRoleSpecialReset specialResetRole = multiAssignRole.AnotherRole as IRoleSpecialReset;
                if (specialResetRole != null)
                {
                    specialResetRole.AllReset(targetPlayer);
                }
            }
        }
        private static void replaceVanilaRole(
            SingleRoleBase targetRole,
            PlayerControl targetPlayer)
        {
            var multiAssignRole = targetRole as MultiAssignRoleBase;
            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole is VanillaRoleWrapper)
                {
                    FastDestroyableSingleton<RoleManager>.Instance.SetRole(
                        targetPlayer, RoleTypes.Crewmate);
                    return;
                }
            }
            
            switch (targetPlayer.Data.Role.Role)
            {
                case RoleTypes.Crewmate:
                case RoleTypes.Impostor:
                    FastDestroyableSingleton<RoleManager>.Instance.SetRole(
                        targetPlayer, RoleTypes.Crewmate);
                    break;
                default:
                    break;
            }
        }
        private static void resetRole(
            SingleRoleBase targetRole,
            byte targetPlayerId,
            PlayerControl targetPlayer)
        {
            if (CachedPlayerControl.LocalPlayer.PlayerId == targetPlayerId)
            {
                var meetingResetRole = targetRole as IRoleResetMeeting;
                if (meetingResetRole != null)
                {
                    meetingResetRole.ResetOnMeetingStart();
                }
                var abilityRole = targetRole as IRoleAbility;
                if (abilityRole != null)
                {
                    abilityRole.ResetOnMeetingStart();
                }
            }

            var specialResetRole = targetRole as IRoleSpecialReset;
            if (specialResetRole != null)
            {
                specialResetRole.AllReset(targetPlayer);
            }
        }

        private static void setNewRole(
            SingleRoleBase role,
            byte targetPlayerId)
        {
            lock (ExtremeRoleManager.GameRole)
            {
                ExtremeRoleManager.GameRole[targetPlayerId] = role;
            }
        }

        public void HockMuderPlayer(
            PlayerControl source, PlayerControl target)
        {
            if (source.PlayerId != target.PlayerId &&
                this.ServantPlayerId.Contains(source.PlayerId))
            {

                float killcool = CachedPlayerControl.LocalPlayer.PlayerControl.killTimer;
                if (killcool > 0.0f)
                {
                    CachedPlayerControl.LocalPlayer.PlayerControl.killTimer = killcool * this.killKillCoolReduceRate;
                }
            }
        }

        public void Update(PlayerControl rolePlayer)
        {
            float killcool = CachedPlayerControl.LocalPlayer.PlayerControl.killTimer;
            
            if (killcool <= 0.0f) { return; }

            foreach (byte playerId in this.ServantPlayerId)
            {
                var player = Player.GetPlayerControlById(playerId);
                if (player != null)
                {
                    float gage = Player.GetPlayerTaskGage(player);

                    if (!this.servantTaskGage.ContainsKey(playerId))
                    {
                        this.servantTaskGage.Add(playerId, gage);
                    }
                    float prevGage = this.servantTaskGage[playerId];
                    if (gage > prevGage)
                    {
                        CachedPlayerControl.LocalPlayer.PlayerControl.killTimer = killcool * this.taskKillCoolReduceRate;
                    }
                }
            }
        }


        public void AllReset(PlayerControl rolePlayer)
        {
            foreach (var playerId in this.ServantPlayerId)
            {
                RPCOperator.UncheckedMurderPlayer(
                    playerId, playerId,
                    byte.MaxValue);
            }
        }

        public void CreateAbility()
        {
            this.CreateAbilityCountButton(
                Translation.GetString("queenCharm"),
                Loader.CreateSpriteFromResources(
                    Path.TestButton));
        }

        public bool UseAbility()
        {
            byte targetPlayerId = this.Target.PlayerId;

            PlayerControl rolePlayer = CachedPlayerControl.LocalPlayer;

            RPCOperator.Call(
                rolePlayer.NetId,
                RPCOperator.Command.ReplaceRole,
                new List<byte>
                {
                    rolePlayer.PlayerId,
                    this.Target.PlayerId,
                    (byte)ExtremeRoleManager.ReplaceOperation.ForceReplaceToSidekick
                });
            TargetToServant(rolePlayer.PlayerId, targetPlayerId);
            return true;
        }

        public bool IsAbilityUse()
        {
            this.Target = Player.GetPlayerTarget(
                CachedPlayerControl.LocalPlayer,
                this, this.range);

            return this.Target != null && this.IsCommonUse();
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public override void ExiledAction(GameData.PlayerInfo rolePlayer)
        {
            foreach (var playerId in this.ServantPlayerId)
            {
                Player.GetPlayerControlById(playerId)?.Exiled();
            }
        }

        public override void RolePlayerKilledAction(
            PlayerControl rolePlayer, PlayerControl killerPlayer)
        {
            foreach (var playerId in this.ServantPlayerId)
            {
                RPCOperator.UncheckedMurderPlayer(
                    playerId, playerId,
                    byte.MaxValue);
            }
        }

        public override bool IsSameTeam(SingleRoleBase targetRole)
        {
            var multiAssignRole = targetRole as MultiAssignRoleBase;

            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {
                    return this.IsSameTeam(multiAssignRole.AnotherRole);
                }
            }
            if (OptionHolder.Ship.IsSameNeutralSameWin)
            {
                return this.isSameQueenTeam(targetRole);
            }
            else
            {
                return this.isSameQueenTeam(targetRole) && this.IsSameControlId(targetRole);
            }
        }

        protected override void CreateSpecificOption(CustomOptionBase parentOps)
        {
            CreateBoolOption(
                QueenOption.CanUseVent,
                false, parentOps);

            this.CreateAbilityCountOption(
                parentOps, 1, 3);
            
            CreateFloatOption(
                QueenOption.Range,
                1.0f, 0.5f, 2.6f, 0.1f,
                parentOps);
            CreateIntOption(
                QueenOption.ServantKillKillCoolReduceRate,
                25, 5, 75, 1,
                parentOps,
                format:OptionUnit.Percentage);
            CreateIntOption(
                QueenOption.ServantTaskKillCoolReduceRate,
                50, 5, 75, 1,
                parentOps,
                format: OptionUnit.Percentage);
            CreateFloatOption(
                QueenOption.ServantSelfKillCool,
                30.0f, 0.5f, 60.0f, 0.5f,
                parentOps);
        }

        protected override void RoleSpecificInit()
        {
            this.range = OptionHolder.AllOption[
                GetRoleOptionId(QueenOption.Range)].GetValue();
            this.UseVent = OptionHolder.AllOption[
                GetRoleOptionId(QueenOption.CanUseVent)].GetValue();
            this.ServantSelfKillCool = OptionHolder.AllOption[
                GetRoleOptionId(QueenOption.ServantSelfKillCool)].GetValue();
            this.killKillCoolReduceRate = OptionHolder.AllOption[
                GetRoleOptionId(QueenOption.ServantKillKillCoolReduceRate)].GetValue();
            this.taskKillCoolReduceRate = OptionHolder.AllOption[
                GetRoleOptionId(QueenOption.ServantTaskKillCoolReduceRate)].GetValue();

            this.servantTaskGage.Clear();
            this.ServantPlayerId.Clear();
        }

        private bool isSameQueenTeam(SingleRoleBase targetRole)
        {
            return ((targetRole.Id == this.Id) || (targetRole.Id == ExtremeRoleId.Servant));
        }
    }

    public class Servant : MultiAssignRoleBase, IRoleAbility, IRoleMurderPlayerHock
    {
        private byte queenPlayerId;
        private SpriteRenderer killFlash;

        public Servant(
            byte queenPlayerId,
            Queen queen,
            SingleRoleBase baseRole) : 
            base(
                ExtremeRoleId.Servant,
                ExtremeRoleType.Neutral,
                ExtremeRoleId.Servant.ToString(),
                ColorPalette.QueenWhite,
                baseRole.CanKill,
                baseRole.Team == ExtremeRoleType.Crewmate ? true : baseRole.HasTask,
                baseRole.UseVent,
                baseRole.UseSabotage)
        {
            this.GameControlId = queen.GameControlId;
            this.queenPlayerId = queenPlayerId;
            this.FakeImposter = baseRole.Team == ExtremeRoleType.Impostor;
        }

        public RoleAbilityButtonBase Button
        { 
            get => this.selfKillButton;
            set
            {
                this.selfKillButton = value;
            }
        }

        private RoleAbilityButtonBase selfKillButton;

        public void SelfKillAbility(float coolTime)
        {
            this.Button = new ReusableAbilityButton(
                Translation.GetString("selfKill"),
                this.UseAbility,
                this.IsAbilityUse,
                Loader.CreateSpriteFromResources(
                    Path.TestButton),
                new Vector3(-1.8f, -0.06f, 0),
                null, null,
                KeyCode.F, false);
            this.Button.SetAbilityCoolTime(coolTime);
        }

        public void HockMuderPlayer(
            PlayerControl source, PlayerControl target)
        {
            if (this.killFlash == null)
            {
                this.killFlash = UnityEngine.Object.Instantiate(
                     FastDestroyableSingleton<HudManager>.Instance.FullScreen,
                     FastDestroyableSingleton<HudManager>.Instance.transform);
                this.killFlash.transform.localPosition = new Vector3(0f, 0f, 20f);
                this.killFlash.gameObject.SetActive(true);
            }

            Color32 color = Palette.EnabledColor;

            if (source.PlayerId == this.queenPlayerId)
            {
                color = this.NameColor;
            }
            
            this.killFlash.enabled = true;

            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(
                Effects.Lerp(1.0f, new Action<float>((p) =>
                {
                    if (this.killFlash == null) { return; }
                    if (p < 0.5)
                    {
                        this.killFlash.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(p * 2 * 0.75f));

                    }
                    else
                    {
                        this.killFlash.color = new Color(color.r, color.g, color.b, Mathf.Clamp01((1 - p) * 2 * 0.75f));
                    }
                    if (p == 1f)
                    {
                        this.killFlash.enabled = false;
                    }
                }))
            );
        }

        public void CreateAbility()
        {
            throw new Exception("Don't call this class method!!");
        }

        public bool IsAbilityUse() => this.IsCommonUse();

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            if (this.killFlash == null)
            {
                this.killFlash.enabled = false; 
            }
        }

        public bool UseAbility()
        {

            byte playerId = CachedPlayerControl.LocalPlayer.PlayerId;

            RPCOperator.Call(
                CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                RPCOperator.Command.UncheckedMurderPlayer,
                new List<byte> { playerId, playerId, byte.MaxValue });
            RPCOperator.UncheckedMurderPlayer(
                playerId,
                playerId,
                byte.MaxValue);
            return true;
        }

        public override bool TryRolePlayerKillTo(
            PlayerControl rolePlayer, PlayerControl targetPlayer)
        {
            if (this.AnotherRole?.Id == ExtremeRoleId.Sheriff)
            {
                RPCOperator.Call(
                   rolePlayer.NetId,
                   RPCOperator.Command.UncheckedMurderPlayer,
                   new List<byte>
                   {
                        rolePlayer.PlayerId,
                        rolePlayer.PlayerId,
                        byte.MaxValue
                   });
                RPCOperator.UncheckedMurderPlayer(
                    rolePlayer.PlayerId,
                    rolePlayer.PlayerId,
                    byte.MaxValue);

                RPCOperator.Call(
                    rolePlayer.NetId,
                    RPCOperator.Command.ReplaceDeadReason,
                    new List<byte>
                    {
                        rolePlayer.PlayerId,
                        (byte)GameDataContainer.PlayerStatus.MissShot
                    });
                ExtremeRolesPlugin.GameDataStore.ReplaceDeadReason(
                    rolePlayer.PlayerId, GameDataContainer.PlayerStatus.MissShot);
                return false;
            }
            else if (targetPlayer.PlayerId == this.queenPlayerId)
            {
                return false;
            }

            return base.TryRolePlayerKillTo(rolePlayer, targetPlayer);
        }

        public override string GetFullDescription()
        {
            return string.Format(
                base.GetFullDescription(),
                Player.GetPlayerControlById(
                    this.queenPlayerId)?.Data.PlayerName);
        }

        public override Color GetTargetRoleSeeColor(
            SingleRoleBase targetRole,
            byte targetPlayerId)
        {

            if (targetPlayerId == this.queenPlayerId)
            {
                return ColorPalette.QueenWhite;
            }
            return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            throw new Exception("Don't call this class method!!");
        }

        protected override void RoleSpecificInit()
        {
            throw new Exception("Don't call this class method!!");
        }
    }

}
