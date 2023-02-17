using System;
using System.Collections.Generic;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.Helper;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.GameMode;

namespace ExtremeRoles.Roles.Solo.Neutral
{
    public sealed class Jackal : SingleRoleBase, IRoleAbility, IRoleSpecialReset
    {
        public enum JackalOption
        {
            SidekickLimitNum,
            RangeSidekickTarget,
            CanLoverSidekick,
            ForceReplaceLover,

            UpgradeSidekickNum,

            CanSetImpostorToSidekick,
            CanSeeImpostorToSidekickImpostor,
            SidekickJackalCanMakeSidekick,
        }

        public List<byte> SidekickPlayerId;

        public RoleAbilityButtonBase Button
        {
            get => this.createSidekick;
            set
            {
                this.createSidekick = value;
            }
        }

        public int NumAbility = 0;
        public int CurRecursion = 0;
        public int SidekickRecursionLimit = 0;

        public bool SidekickJackalCanMakeSidekick = false;
        public bool ForceReplaceLover = false;
        public bool CanSetImpostorToSidekick = false;
        public bool CanSeeImpostorToSidekickImpostor = false;

        public SidekickOptionHolder SidekickOption;

        public PlayerControl Target;

        private RoleAbilityButtonBase createSidekick;

        private bool canLoverSidekick;
        private int numUpgradeSidekick = 0;
        private int createSidekickRange = 0;

        public struct SidekickOptionHolder
        {
            public bool CanKill = false;
            public bool UseSabotage = false;
            public bool UseVent = false;

            public bool HasOtherVison = false;
            public float Vison = 0f;
            public bool ApplyEnvironmentVisionEffect = false;

            public bool HasOtherKillCool = false;
            public float KillCool = 0f;

            public bool HasOtherKillRange = false;
            public int KillRange = 0;

            public int OptionIdOffset = 0;

            private enum SidekickOption
            {
                UseSabotage = 10,
                UseVent,
                CanKill,

                HasOtherKillCool,
                KillCoolDown,
                HasOtherKillRange,
                KillRange,

                HasOtherVison,
                Vison,
                ApplyEnvironmentVisionEffect,
            }

            public SidekickOptionHolder(
                IOption parentOps,
                int optionOffset,
                OptionTab tab)
            {
                this.OptionIdOffset = optionOffset;
                string roleName = ExtremeRoleId.Sidekick.ToString();

                new BoolCustomOption(
                    GetRoleOptionId(SidekickOption.UseSabotage),
                    string.Concat(
                        roleName,
                        SidekickOption.UseSabotage.ToString()),
                    true, parentOps,
                    tab: tab);
                new BoolCustomOption(
                    GetRoleOptionId(SidekickOption.UseVent),
                    string.Concat(
                        roleName,
                        SidekickOption.UseVent.ToString()),
                    true, parentOps,
                    tab: tab);

                var sidekickKillerOps = new BoolCustomOption(
                    GetRoleOptionId(SidekickOption.CanKill),
                    string.Concat(
                        roleName,
                        SidekickOption.CanKill.ToString()),
                    false, parentOps,
                    tab: tab);

                var killCoolOption = new BoolCustomOption(
                    GetRoleOptionId(SidekickOption.HasOtherKillCool),
                    string.Concat(
                        roleName,
                        SidekickOption.HasOtherKillCool.ToString()),
                    false, sidekickKillerOps,
                    tab: tab);
                new FloatCustomOption(
                    GetRoleOptionId(SidekickOption.KillCoolDown),
                    string.Concat(
                        roleName,
                        SidekickOption.KillCoolDown.ToString()),
                    30f, 1.0f, 120f, 0.5f,
                    killCoolOption, format: OptionUnit.Second,
                    tab: tab);

                var killRangeOption = new BoolCustomOption(
                    GetRoleOptionId(SidekickOption.HasOtherKillRange),
                    string.Concat(
                        roleName,
                        SidekickOption.HasOtherKillRange.ToString()),
                    false, sidekickKillerOps,
                    tab: tab);
                new SelectionCustomOption(
                    GetRoleOptionId(SidekickOption.KillRange),
                    string.Concat(
                        roleName,
                        SidekickOption.KillRange.ToString()),
                    OptionHolder.Range,
                    killRangeOption,
                    tab: tab);

                var visonOption = new BoolCustomOption(
                    GetRoleOptionId(SidekickOption.HasOtherVison),
                    string.Concat(
                        roleName,
                        SidekickOption.HasOtherVison.ToString()),
                    false, parentOps,
                    tab: tab);

                new FloatCustomOption(
                    GetRoleOptionId(SidekickOption.Vison),
                    string.Concat(
                        roleName,
                        SidekickOption.Vison.ToString()),
                    2f, 0.25f, 5f, 0.25f,
                    visonOption, format: OptionUnit.Multiplier,
                    tab: tab);
                new BoolCustomOption(
                    GetRoleOptionId(SidekickOption.ApplyEnvironmentVisionEffect),
                    string.Concat(
                        roleName,
                        SidekickOption.ApplyEnvironmentVisionEffect.ToString()),
                    false, visonOption,
                    tab: tab);
            }

            public void ApplyOption()
            {
                var curOption = GameOptionsManager.Instance.CurrentGameOptions;
                var allOption = OptionHolder.AllOption;

                this.UseSabotage = allOption[
                    GetRoleOptionId(SidekickOption.UseSabotage)].GetValue();
                this.UseVent = allOption[
                    GetRoleOptionId(SidekickOption.UseVent)].GetValue();

                this.CanKill = allOption[
                    GetRoleOptionId(SidekickOption.CanKill)].GetValue();

                this.HasOtherKillCool = allOption[
                    GetRoleOptionId(SidekickOption.HasOtherKillCool)].GetValue();
                this.KillCool = curOption.GetFloat(FloatOptionNames.KillCooldown);
                if (this.HasOtherKillCool)
                {
                    this.KillCool = allOption[
                        GetRoleOptionId(SidekickOption.KillCoolDown)].GetValue();
                }

                this.HasOtherKillRange = allOption[
                    GetRoleOptionId(SidekickOption.HasOtherKillRange)].GetValue();
                this.KillRange = curOption.GetInt(Int32OptionNames.KillDistance);
                if (this.HasOtherKillRange)
                {
                    this.KillRange = allOption[
                        GetRoleOptionId(SidekickOption.KillRange)].GetValue();
                }

                this.HasOtherVison = allOption[
                    GetRoleOptionId(SidekickOption.HasOtherVison)].GetValue();
                this.Vison = curOption.GetFloat(FloatOptionNames.CrewLightMod);
                this.ApplyEnvironmentVisionEffect = false;
                if (this.HasOtherVison)
                {
                    this.Vison = allOption[
                        GetRoleOptionId(SidekickOption.Vison)].GetValue();
                    this.ApplyEnvironmentVisionEffect = allOption[
                        GetRoleOptionId(SidekickOption.ApplyEnvironmentVisionEffect)].GetValue();
                }
            }
            private int GetRoleOptionId(SidekickOption ops) => (int)ops + this.OptionIdOffset;

            public SidekickOptionHolder Clone()
            {
                return (SidekickOptionHolder)this.MemberwiseClone();
            }
        }

        public Jackal() : base(
            ExtremeRoleId.Jackal,
            ExtremeRoleType.Neutral,
            ExtremeRoleId.Jackal.ToString(),
            ColorPalette.JackalBlue,
            true, false, true, false)
        { }

        public override SingleRoleBase Clone()
        {
            var jackal = (Jackal)base.Clone();
            jackal.SidekickOption = this.SidekickOption.Clone();

            return jackal;
        }

        public static void TargetToSideKick(byte callerId, byte targetId)
        {
            PlayerControl targetPlayer = Player.GetPlayerControlById(targetId);
            SingleRoleBase targetRole = ExtremeRoleManager.GameRole[targetId];

            IRoleSpecialReset.ResetRole(targetId);

            var sourceJackal = ExtremeRoleManager.GetSafeCastedRole<Jackal>(callerId);
            if (sourceJackal == null) { return; }
            var newSidekick = new Sidekick(
                sourceJackal,
                callerId,
                targetRole.IsImpostor(),
                ref sourceJackal.SidekickOption);

            sourceJackal.SidekickPlayerId.Add(targetId);

            if (targetRole.Id != ExtremeRoleId.Lover)
            {
                ExtremeRoleManager.SetNewRole(targetId, newSidekick);
            }
            else
            {
                if (sourceJackal.ForceReplaceLover)
                {
                    ExtremeRoleManager.SetNewRole(targetId, newSidekick);
                    targetRole.RolePlayerKilledAction(targetPlayer, targetPlayer);
                }
                else
                {
                    var sidekickOption = sourceJackal.SidekickOption;
                    var lover = (Combination.Lover)targetRole;
                    lover.CanHasAnotherRole = true;

                    ExtremeRoleManager.SetNewAnothorRole(targetId, newSidekick);

                    lover.Team = ExtremeRoleType.Neutral;
                    lover.HasTask = false;
                    lover.HasOtherVison = sidekickOption.HasOtherVison;
                    lover.IsApplyEnvironmentVision = sidekickOption.ApplyEnvironmentVisionEffect;
                    lover.Vison = sidekickOption.Vison;
                    lover.KillCoolTime = sidekickOption.KillCool;
                    lover.KillRange = sidekickOption.KillRange;
                }
            }

        }

        public void CreateAbility()
        {
            this.CreateAbilityCountButton(
                Translation.GetString("Sidekick"),
                Loader.CreateSpriteFromResources(
                    Path.JackalSidekick));
        }

        public override Color GetTargetRoleSeeColor(
            SingleRoleBase targetRole,
            byte targetPlayerId)
        {

            if (((
                    targetRole.Id == ExtremeRoleId.Sidekick &&
                    this.SidekickPlayerId.Contains(targetPlayerId)) ||
                (
                    targetRole.Id == ExtremeRoleId.Jackal
                )) && this.IsSameControlId(targetRole))
            {
                return ColorPalette.JackalBlue;
            }
            
            return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
        }

        public override string GetFullDescription()
        {
            string baseDesc = base.GetFullDescription();

            if (SidekickPlayerId.Count != 0)
            {
                baseDesc = $"{baseDesc}\n{Translation.GetString("curSidekick")}:";

                foreach (var playerId in SidekickPlayerId)
                {
                    string playerName = Player.GetPlayerControlById(playerId).Data.PlayerName;
                    baseDesc += $"{playerName},";
                }
            }

            return baseDesc;
        }

        public override bool IsSameTeam(SingleRoleBase targetRole)
        {
            if (this.isSameJackalTeam(targetRole))
            {
                if (ExtremeGameModeManager.Instance.ShipOption.IsSameNeutralSameWin)
                {
                    return true;
                }
                else
                {
                    return this.IsSameControlId(targetRole);
                }
            }
            else
            {
                return base.IsSameTeam(targetRole);
            }
        }

        public override void ExiledAction(
            PlayerControl rolePlayer)
        {
            SidekickToJackal(rolePlayer);
        }

        public override void RolePlayerKilledAction(
            PlayerControl rolePlayer, PlayerControl killerPlayer)
        {
            SidekickToJackal(rolePlayer);
        }

        public bool IsAbilityUse()
        {
        
            this.Target = Player.GetClosestPlayerInRange(
                CachedPlayerControl.LocalPlayer,
                this, GameOptionsData.KillDistances[
                    Mathf.Clamp(this.createSidekickRange, 0, 2)]);

            return this.Target != null && this.IsCommonUse();
        }

        public bool UseAbility()
        {
            byte targetPlayerId = this.Target.PlayerId;
            if (!isImpostorAndSetTarget(targetPlayerId)) { return false; }
            if (!isLoverAndSetTarget(targetPlayerId)) { return false; }

            PlayerControl rolePlayer = CachedPlayerControl.LocalPlayer;
            
            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.ReplaceRole))
            {
                caller.WriteByte(rolePlayer.PlayerId);
                caller.WriteByte(this.Target.PlayerId);
                caller.WriteByte(
                    (byte)ExtremeRoleManager.ReplaceOperation.ForceReplaceToSidekick);
            }
            TargetToSideKick(rolePlayer.PlayerId, targetPlayerId);
            return true;
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void SidekickToJackal(PlayerControl rolePlayer)
        {

            if (this.SidekickPlayerId.Count == 0) { return; }

            int numUpgrade = this.SidekickPlayerId.Count >= this.numUpgradeSidekick ?
                this.numUpgradeSidekick : this.SidekickPlayerId.Count;

            List<byte> updateSideKick = new List<byte>();

            for (int i = 0; i < numUpgrade; ++i)
            {
                int useIndex = UnityEngine.Random.Range(0, this.SidekickPlayerId.Count);
                byte targetPlayerId = this.SidekickPlayerId[useIndex];
                this.SidekickPlayerId.Remove(targetPlayerId);

                updateSideKick.Add(targetPlayerId);

            }
            foreach (var playerId in updateSideKick)
            {
                Sidekick.BecomeToJackal(rolePlayer.PlayerId, playerId);
            }
        }

        public void AllReset(PlayerControl rolePlayer)
        {
            this.SidekickToJackal(rolePlayer);
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            // JackalOption
            this.createJackalOption(parentOps);

            // SideKickOption
            this.SidekickOption = new SidekickOptionHolder(
                parentOps, this.OptionIdOffset, this.Tab);
        }

        protected override void RoleSpecificInit()
        {
            this.CurRecursion = 0;
            this.SidekickPlayerId = new List<byte>();

            var allOption = OptionHolder.AllOption;

            this.SidekickRecursionLimit = allOption[
                GetRoleOptionId(JackalOption.SidekickLimitNum)].GetValue();

            this.canLoverSidekick = allOption[
                GetRoleOptionId(JackalOption.CanLoverSidekick)].GetValue();

            this.ForceReplaceLover = allOption[
                GetRoleOptionId(JackalOption.ForceReplaceLover)].GetValue();

            this.CanSetImpostorToSidekick = allOption[
                GetRoleOptionId(JackalOption.CanSetImpostorToSidekick)].GetValue();
            this.CanSeeImpostorToSidekickImpostor = allOption[
                GetRoleOptionId(JackalOption.CanSeeImpostorToSidekickImpostor)].GetValue();

            this.SidekickJackalCanMakeSidekick = allOption[
                GetRoleOptionId(JackalOption.SidekickJackalCanMakeSidekick)].GetValue();

            this.createSidekickRange = allOption[
                GetRoleOptionId(JackalOption.RangeSidekickTarget)].GetValue();

            this.numUpgradeSidekick = allOption[
                GetRoleOptionId(JackalOption.UpgradeSidekickNum)].GetValue();

            this.SidekickOption.ApplyOption();
            
            this.RoleAbilityInit();
        }

        private void createJackalOption(IOption parentOps)
        {

            this.CreateAbilityCountOption(
                parentOps, 1, GameSystem.VanillaMaxPlayerNum - 1);

            CreateSelectionOption(
                JackalOption.RangeSidekickTarget,
                OptionHolder.Range,
                parentOps);

            var loverSkOpt = CreateBoolOption(
                JackalOption.CanLoverSidekick,
                true, parentOps);

            CreateBoolOption(
                JackalOption.ForceReplaceLover,
                true, loverSkOpt,
                invert: true, enableCheckOption: parentOps);

            CreateIntOption(
                JackalOption.UpgradeSidekickNum,
                1, 1, GameSystem.VanillaMaxPlayerNum - 1, 1,
                parentOps);

            var sidekickMakeSidekickOps = CreateBoolOption(
                JackalOption.SidekickJackalCanMakeSidekick,
                false, parentOps);

            CreateIntOption(
                JackalOption.SidekickLimitNum,
                1, 1, GameSystem.VanillaMaxPlayerNum / 2, 1,
                sidekickMakeSidekickOps);

            CreateBoolOption(
                JackalOption.CanSetImpostorToSidekick,
                false, parentOps);

            CreateBoolOption(
                JackalOption.CanSeeImpostorToSidekickImpostor,
                false, parentOps);

        }

        private bool isImpostorAndSetTarget(
            byte playerId)
        {
            if (ExtremeRoleManager.GameRole[playerId].IsImpostor())
            {
                return this.CanSetImpostorToSidekick;
            }
            return true;
        }

        private bool isLoverAndSetTarget(byte playerId)
        {
            if (ExtremeRoleManager.GameRole[playerId].Id == ExtremeRoleId.Lover)
            {
                return this.canLoverSidekick;
            }
            return true;
        }

        private bool isSameJackalTeam(SingleRoleBase targetRole)
        {
            return ((targetRole.Id == this.Id) || (targetRole.Id == ExtremeRoleId.Sidekick));
        }
    }

    public sealed class Sidekick : SingleRoleBase, IRoleUpdate, IRoleHasParent
    {
        public byte Parent => this.jackalPlayerId;

        private byte jackalPlayerId;
        private Jackal jackal;
        private int recursion = 0;
        private bool sidekickJackalCanMakeSidekick = false;

        public Sidekick(
            Jackal jackal,
            byte jackalPlayerId,
            bool isImpostor,
            ref Jackal.SidekickOptionHolder option) : base(
                ExtremeRoleId.Sidekick,
                ExtremeRoleType.Neutral,
                ExtremeRoleId.Sidekick.ToString(),
                ColorPalette.JackalBlue,
                option.CanKill, false,
                option.UseVent, option.UseSabotage)
        {
            this.jackal = jackal;
            this.OptionIdOffset = option.OptionIdOffset;
            this.jackalPlayerId = jackalPlayerId;
            this.SetControlId(jackal.GameControlId);

            this.HasOtherKillCool = option.HasOtherKillCool;
            this.KillCoolTime = option.KillCool;
            this.HasOtherKillRange = option.HasOtherKillRange;
            this.KillRange = option.KillRange;

            this.HasOtherVison = option.HasOtherVison;
            this.Vison = option.Vison;
            this.IsApplyEnvironmentVision = option.ApplyEnvironmentVisionEffect;

            this.FakeImposter = jackal.CanSeeImpostorToSidekickImpostor && isImpostor;

            this.recursion = jackal.CurRecursion;
            this.sidekickJackalCanMakeSidekick = jackal.SidekickJackalCanMakeSidekick;
        }

        public void RemoveParent(byte rolePlayerId)
        {
            jackal.SidekickPlayerId.Remove(rolePlayerId);
        }

        public override bool IsSameTeam(SingleRoleBase targetRole)
        {
            if (this.isSameJackalTeam(targetRole))
            {
                if (ExtremeGameModeManager.Instance.ShipOption.IsSameNeutralSameWin)
                {
                    return true;
                }
                else
                {
                    return this.IsSameControlId(targetRole);
                }
            }
            else
            {
                return base.IsSameTeam(targetRole);
            }
        }

        public override Color GetTargetRoleSeeColor(
            SingleRoleBase targetRole,
            byte targetPlayerId)
        {
            
            if (targetPlayerId == this.jackalPlayerId)
            {
                return ColorPalette.JackalBlue;
            }
            return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
        }

        public override string GetFullDescription()
        {
            return string.Format(
                base.GetFullDescription(),
                Player.GetPlayerControlById(
                    this.jackalPlayerId)?.Data.PlayerName);
        }

        public static void BecomeToJackal(byte callerId, byte targetId)
        {

            Jackal curJackal = ExtremeRoleManager.GetSafeCastedRole<Jackal>(callerId);
            if (curJackal == null) { return; }
            Jackal newJackal = (Jackal)curJackal.Clone();

            bool multiAssignTrigger = false;
            var curRole = ExtremeRoleManager.GameRole[targetId];
            var curSideKick = curRole as Sidekick;
            if (curSideKick == null)
            {
                curSideKick = (Sidekick)((MultiAssignRoleBase)curRole).AnotherRole;
                multiAssignTrigger = true;
            }


            newJackal.Initialize();
            newJackal.CreateAbility();

            if (!curSideKick.sidekickJackalCanMakeSidekick || curSideKick.recursion >= newJackal.SidekickRecursionLimit)
            {
                ((AbilityCountButton)newJackal.Button).UpdateAbilityCount(0);
            }


            newJackal.CurRecursion = curSideKick.recursion + 1;
            newJackal.SidekickPlayerId = new List<byte> (curJackal.SidekickPlayerId);
            newJackal.SetControlId(curSideKick.GameControlId);


            if (newJackal.SidekickPlayerId.Contains(targetId))
            {
                newJackal.SidekickPlayerId.Remove(targetId);
            }


            if (multiAssignTrigger)
            {
                var multiAssignRole = (MultiAssignRoleBase)curRole;
                multiAssignRole.AnotherRole = null;
                multiAssignRole.CanKill = false;
                multiAssignRole.HasTask = false;
                multiAssignRole.UseSabotage = false;
                multiAssignRole.UseVent = false;

                ExtremeRoleManager.SetNewAnothorRole(targetId, newJackal);
            }
            else
            {
                ExtremeRoleManager.SetNewRole(targetId, newJackal);
            }

        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            throw new Exception("Don't call this class method!!");
        }

        protected override void RoleSpecificInit()
        {
            throw new Exception("Don't call this class method!!");
        }

        public void Update(PlayerControl rolePlayer)
        {
            if (Player.GetPlayerControlById(this.jackalPlayerId).Data.Disconnected)
            {
                this.jackal.SidekickPlayerId.Clear();

                using (var caller = RPCOperator.CreateCaller(
                    RPCOperator.Command.ReplaceRole))
                {
                    caller.WriteByte(this.jackalPlayerId);
                    caller.WriteByte(rolePlayer.PlayerId);
                    caller.WriteByte((byte)ExtremeRoleManager.ReplaceOperation.SidekickToJackal);
                }
                BecomeToJackal(this.jackalPlayerId, rolePlayer.PlayerId);
            }
        }

        private bool isSameJackalTeam(SingleRoleBase targetRole)
        {
            return ((targetRole.Id == this.Id) || (targetRole.Id == ExtremeRoleId.Jackal));
        }
    }

}
