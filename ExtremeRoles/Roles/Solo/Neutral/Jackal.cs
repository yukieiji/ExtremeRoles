using System;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Module.RoleAbilityButton;
using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Neutral
{
    public class Jackal : SingleRoleBase, IRoleAbility
    {
        public enum JackalOption
        {
            SidekickLimitNum,
            RangeSidekickTarget,
            ForceReplaceLover,

            UpgradeSidekickNum,

            CanSetImpostorToSidekick,
            CanSeeImpostorToSidekickImpostor,
            SidekickUseSabotage,
            SidekickUseVent,
            SidekickJackalCanMakeSidekick,
            SideKickCanKill,

            SidekickHasOtherVison,
            SidekickVison,
            SidekickApplyEnvironmentVisionEffect
        }

        public List<byte> SidekickPlayerId = new List<byte>();

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

        public bool ForceReplaceLover = false;
        public bool CanSetImpostorToSidekick = false;
        public bool CanSeeImpostorToSidekickImpostor = false;
        public bool SidekickCanKill = false;
        public bool SidekickUseSabotage = false;
        public bool SidekickUseVent = false;
        public bool SidekickJackalCanMakeSidekick = false;
        public bool SidekickHasOtherVison = false;
        public bool SidekickApplyEnvironmentVisionEffect = false;
        public float SidekickVision = 0f;

        public PlayerControl Target;

        private RoleAbilityButtonBase createSidekick;

        private int numUpgradeSidekick = 0;
        private int createSidekickRange = 0;


        public Jackal() : base(
            ExtremeRoleId.Jackal,
            ExtremeRoleType.Neutral,
            ExtremeRoleId.Jackal.ToString(),
            ColorPalette.JackalBlue,
            true, false, true, false)
        {
            this.SidekickPlayerId.Clear();
        }

        public static void TargetToSideKick(byte callerId, byte targetId)
        {
            var targetPlayer = Player.GetPlayerControlById(targetId);
            var targetRole = ExtremeRoleManager.GameRole[targetId];

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

            var sourceJackal = (Jackal)ExtremeRoleManager.GameRole[callerId];
            var newSidekick = new Sidekick(
                callerId,
                sourceJackal.OptionIdOffset,
                sourceJackal.GameControlId,
                sourceJackal.CurRecursion,
                targetRole.IsImposter(),
                sourceJackal.SidekickCanKill,
                sourceJackal.SidekickUseVent,
                sourceJackal.SidekickUseSabotage,
                sourceJackal.CanSeeImpostorToSidekickImpostor,
                sourceJackal.SidekickJackalCanMakeSidekick,
                sourceJackal.SidekickHasOtherVison,
                sourceJackal.SidekickVision,
                sourceJackal.SidekickApplyEnvironmentVisionEffect);

            DestroyableSingleton<RoleManager>.Instance.SetRole(
                Player.GetPlayerControlById(targetId), RoleTypes.Crewmate);

            if (targetRole.Id != ExtremeRoleId.Lover)
            {
                ExtremeRoleManager.GameRole[targetId] = newSidekick;
            }
            else
            {
                if (sourceJackal.ForceReplaceLover)
                {
                    ExtremeRoleManager.GameRole[targetId] = newSidekick;
                    targetRole.RolePlayerKilledAction(targetPlayer, targetPlayer);
                }
                else
                {
                    var lover = (Combination.Lover)targetRole;
                    lover.CanHasAnotherRole = true;
                    lover.SetAnotherRole(newSidekick);
                }
            }

        }

        public int GetRoleOptionId(JackalOption option) => GetRoleOptionId((int)option);

        public void CreateAbility()
        {
            this.CreateAbilityCountButton(
                Translation.GetString("Sidekick"),
                Helper.Resources.LoadSpriteFromResources(
                    Resources.ResourcesPaths.JackalSidekick, 115f));
        }

        public override Color GetTargetRoleSeeColor(
            SingleRoleBase targetRole,
            byte targetPlayerId)
        {

            if (targetRole.Id == ExtremeRoleId.Sidekick &&
                this.SidekickPlayerId.Contains(targetPlayerId))
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
            var multiAssignRole = targetRole as MultiAssignRoleBase;
            
            if (multiAssignRole != null)
            {
                if(multiAssignRole.AnotherRole != null)
                {
                    return this.IsSameTeam(multiAssignRole.AnotherRole);
                }
            }
            if (OptionHolder.Ship.IsSameNeutralSameWin)
            {
                return this.isSameJackalTeam(targetRole);
            }
            else
            {
                return this.isSameJackalTeam(targetRole) && this.IsSameControlId(targetRole);
            }
        }

        public override void ExiledAction(
            GameData.PlayerInfo rolePlayer)
        {
            sidekickToJackal(rolePlayer.Object);
        }

        public override void RolePlayerKilledAction(
            PlayerControl rolePlayer, PlayerControl killerPlayer)
        {
            sidekickToJackal(rolePlayer);
        }

        public bool IsAbilityUse()
        {
            this.setTarget();
            return this.Target != null && this.IsCommonUse();
        }

        public bool UseAbility()
        {
            byte targetPlayerId = this.Target.PlayerId;
            if (!isImpostorAndSetTarget(targetPlayerId)) { return false; }

            PlayerControl rolePlayer = PlayerControl.LocalPlayer;

            this.SidekickPlayerId.Add(targetPlayerId);

            RPCOperator.Call(
                rolePlayer.NetId,
                RPCOperator.Command.ReplaceRole,
                new List<byte>
                {
                    rolePlayer.PlayerId,
                    this.Target.PlayerId,
                    (byte)ExtremeRoleManager.ReplaceOperation.ForceReplaceToSidekick
                });
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


        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            // JackalOption
            this.CreateJackalOption(parentOps);

            // SideKickOption
            this.CreateSideKickOption(parentOps);
        }

        protected override void RoleSpecificInit()
        {
            this.CurRecursion = 0;
            this.SidekickPlayerId.Clear();

            var allOption = OptionHolder.AllOption;

            this.SidekickRecursionLimit = allOption[
                GetRoleOptionId(JackalOption.SidekickLimitNum)].GetValue();

            this.ForceReplaceLover = allOption[
                GetRoleOptionId(JackalOption.ForceReplaceLover)].GetValue();

            this.CanSetImpostorToSidekick = allOption[
                GetRoleOptionId(JackalOption.CanSetImpostorToSidekick)].GetValue();
            this.CanSeeImpostorToSidekickImpostor = allOption[
                GetRoleOptionId(JackalOption.CanSeeImpostorToSidekickImpostor)].GetValue();

            this.SidekickCanKill = allOption[
                GetRoleOptionId(JackalOption.SideKickCanKill)].GetValue();
            this.SidekickUseSabotage = allOption[
                GetRoleOptionId(JackalOption.SidekickUseSabotage)].GetValue();
            this.SidekickUseVent = allOption[
                GetRoleOptionId(JackalOption.SidekickUseVent)].GetValue();
            this.SidekickJackalCanMakeSidekick = allOption[
                GetRoleOptionId(JackalOption.SidekickJackalCanMakeSidekick)].GetValue();

            this.SidekickHasOtherVison = allOption[
                GetRoleOptionId(JackalOption.SidekickHasOtherVison)].GetValue();
            this.SidekickVision = allOption[
                GetRoleOptionId(JackalOption.SidekickVison)].GetValue();
            this.SidekickApplyEnvironmentVisionEffect = allOption[
                GetRoleOptionId(JackalOption.SidekickApplyEnvironmentVisionEffect)].GetValue();

            this.createSidekickRange = allOption[
                GetRoleOptionId(JackalOption.RangeSidekickTarget)].GetValue();

            this.numUpgradeSidekick = allOption[
                GetRoleOptionId(JackalOption.UpgradeSidekickNum)].GetValue();
            
            this.RoleAbilityInit();
        }

        private void CreateJackalOption(CustomOptionBase parentOps)
        {

            this.CreateAbilityCountOption(
                parentOps, OptionHolder.VanillaMaxPlayerNum - 1);

            CustomOption.Create(
                GetRoleOptionId(JackalOption.RangeSidekickTarget),
                Design.ConcatString(
                    this.RoleName,
                    JackalOption.RangeSidekickTarget.ToString()),
                OptionHolder.Range,
                parentOps);

            CustomOption.Create(
                GetRoleOptionId(JackalOption.ForceReplaceLover),
                Design.ConcatString(
                    this.RoleName,
                    JackalOption.ForceReplaceLover.ToString()),
                true, parentOps);

            CustomOption.Create(
                GetRoleOptionId(JackalOption.UpgradeSidekickNum),
                Design.ConcatString(
                    this.RoleName,
                    JackalOption.UpgradeSidekickNum.ToString()),
                1, 1, OptionHolder.VanillaMaxPlayerNum - 1, 1,
                parentOps);

            var sidekickMakeSidekickOps = CustomOption.Create(
                GetRoleOptionId(JackalOption.SidekickJackalCanMakeSidekick),
                Design.ConcatString(
                    this.RoleName,
                    JackalOption.SidekickJackalCanMakeSidekick.ToString()),
                false, parentOps);

            CustomOption.Create(
                GetRoleOptionId(JackalOption.SidekickLimitNum),
                Design.ConcatString(
                    this.RoleName,
                    JackalOption.SidekickLimitNum.ToString()),
                1, 1, OptionHolder.VanillaMaxPlayerNum / 2, 1,
                sidekickMakeSidekickOps);
        }
        private void CreateSideKickOption(CustomOptionBase parentOps)
        {
            CustomOption.Create(
                GetRoleOptionId(JackalOption.CanSetImpostorToSidekick),
                Design.ConcatString(
                    this.RoleName,
                    JackalOption.CanSetImpostorToSidekick.ToString()),
                false, parentOps);

            CustomOption.Create(
                GetRoleOptionId(JackalOption.CanSeeImpostorToSidekickImpostor),
                Design.ConcatString(
                    this.RoleName,
                    JackalOption.CanSeeImpostorToSidekickImpostor.ToString()),
                false, parentOps);

            CustomOption.Create(
                GetRoleOptionId(JackalOption.SidekickUseSabotage),
                Design.ConcatString(
                    this.RoleName,
                    JackalOption.SidekickUseSabotage.ToString()),
                true, parentOps);
            CustomOption.Create(
                GetRoleOptionId(JackalOption.SidekickUseVent),
                Design.ConcatString(
                    this.RoleName,
                    JackalOption.SidekickUseVent.ToString()),
                true, parentOps);
            CustomOption.Create(
                GetRoleOptionId(JackalOption.SideKickCanKill),
                Design.ConcatString(
                    this.RoleName,
                    JackalOption.SideKickCanKill.ToString()),
                true, parentOps);

            var visonOption = CustomOption.Create(
                GetRoleOptionId(JackalOption.SidekickHasOtherVison),
                Design.ConcatString(
                    this.RoleName,
                    JackalOption.SidekickHasOtherVison.ToString()),
                false, parentOps);

            CustomOption.Create(
                GetRoleOptionId(JackalOption.SidekickVison),
                Design.ConcatString(
                    this.RoleName,
                   JackalOption.SidekickVison.ToString()),
                2f, 0.25f, 5f, 0.25f,
                visonOption, format: "unitMultiplier");
            CustomOption.Create(
               GetRoleOptionId(JackalOption.SidekickApplyEnvironmentVisionEffect),
               Design.ConcatString(
                   this.RoleName,
                   JackalOption.SidekickApplyEnvironmentVisionEffect.ToString()),
               false, visonOption);
        }

        private bool isImpostorAndSetTarget(
            byte playerId)
        {
            if (ExtremeRoleManager.GameRole[playerId].IsImposter())
            {
                return this.CanSetImpostorToSidekick;
            }
            return true;
        }

        private bool isSameJackalTeam(SingleRoleBase targetRole)
        {
            return ((targetRole.Id == this.Id) || (targetRole.Id == ExtremeRoleId.Sidekick));
        }

        private void setTarget()
        {
            PlayerControl result = null;
            float num = GameOptionsData.KillDistances[
                Mathf.Clamp(this.createSidekickRange, 0, 2)];
            if (!ShipStatus.Instance)
            {
                this.Target = null;
                return;
            }
            Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();

            Il2CppSystem.Collections.Generic.List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
            for (int i = 0; i < allPlayers.Count; i++)
            {
                GameData.PlayerInfo playerInfo = allPlayers[i];

                if (!playerInfo.Disconnected &&
                    playerInfo.PlayerId != PlayerControl.LocalPlayer.PlayerId &&
                    !playerInfo.IsDead &&
                    !playerInfo.Object.inVent)
                {
                    PlayerControl @object = playerInfo.Object;
                    if (@object)
                    {
                        Vector2 vector = @object.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= num && 
                            !PhysicsHelpers.AnyNonTriggersBetween(
                                truePosition, vector.normalized,
                                magnitude, Constants.ShipAndObjectsMask))
                        {
                            result = @object;
                            num = magnitude;
                        }
                    }
                }
            }
            
            if (result)
            {
                if (this.IsSameTeam(ExtremeRoleManager.GameRole[result.PlayerId]))
                {
                    result = null;
                }
            }
            
            this.Target = result;
            Player.SetPlayerOutLine(this.Target, this.NameColor);
        }

        private void sidekickToJackal(PlayerControl rolePlayer)
        {

            if (this.SidekickPlayerId.Count == 0) { return; }

            int numUpgrade = this.SidekickPlayerId.Count >= this.numUpgradeSidekick ?
                this.numUpgradeSidekick : this.SidekickPlayerId.Count;

            for (int i = 0; i < numUpgrade; ++i)
            {
                int useIndex = UnityEngine.Random.Range(0, this.SidekickPlayerId.Count);
                byte targetPlayerId = this.SidekickPlayerId[useIndex];
                this.SidekickPlayerId.Remove(targetPlayerId);

                RPCOperator.Call(
                    rolePlayer.NetId,
                    RPCOperator.Command.ReplaceRole,
                    new List<byte>
                    {
                        rolePlayer.PlayerId,
                        targetPlayerId,
                        (byte)ExtremeRoleManager.ReplaceOperation.SidekickToJackal
                    });

                Sidekick.BecomeToJackal(rolePlayer.PlayerId, targetPlayerId);
            }
        }
    }

    public class Sidekick : SingleRoleBase, IRoleUpdate
    {

        public byte JackalPlayerId;

        private int recursion = 0;
        private bool sidekickJackalCanMakeSidekick = false;

        public Sidekick(
            byte jackalPlayerId,
            int optionIdOffset,
            int gameControleId,
            int curRecursion,
            bool isImpostor,
            bool canKill,
            bool useVent,
            bool useSabotage,
            bool canSeeImpostorToSideKickImpostor,
            bool sidekickJackalCanMakeSidekick,
            bool hasOtherVision,
            float vison,
            bool applyEnvironmentVisionEffect) : base(
                ExtremeRoleId.Sidekick,
                ExtremeRoleType.Neutral,
                ExtremeRoleId.Sidekick.ToString(),
                ColorPalette.JackalBlue,
                false, canKill, useVent, useSabotage)
        {
            this.OptionIdOffset = optionIdOffset;
            this.JackalPlayerId = jackalPlayerId;
            this.GameControlId = gameControleId;
            this.HasOtherVison = hasOtherVision;
            this.Vison = vison;
            this.IsApplyEnvironmentVision = applyEnvironmentVisionEffect;

            this.FakeImposter = canSeeImpostorToSideKickImpostor && isImpostor;

            this.recursion = curRecursion;
            this.sidekickJackalCanMakeSidekick = sidekickJackalCanMakeSidekick;
        }

        public override Color GetTargetRoleSeeColor(
            SingleRoleBase targetRole,
            byte targetPlayerId)
        {
            
            if (targetPlayerId == this.JackalPlayerId)
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
                    this.JackalPlayerId).Data.PlayerName);
        }

        public static void BecomeToJackal(byte callerId, byte targetId)
        {

            Jackal curJackal = (Jackal)ExtremeRoleManager.GameRole[callerId];
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
            newJackal.GameControlId = curSideKick.GameControlId;


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
                multiAssignRole.SetAnotherRole(newJackal);
                ExtremeRoleManager.GameRole[targetId] = multiAssignRole;
            }
            else
            {
                ExtremeRoleManager.GameRole[targetId] = newJackal;
            }

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

        public void Update(PlayerControl rolePlayer)
        {
            if (Player.GetPlayerControlById(this.JackalPlayerId).Data.Disconnected)
            {
                RPCOperator.Call(
                    rolePlayer.NetId,
                    RPCOperator.Command.ReplaceRole,
                    new List<byte>
                    {
                        this.JackalPlayerId,
                        rolePlayer.PlayerId,
                        (byte)ExtremeRoleManager.ReplaceOperation.SidekickToJackal
                    });

                BecomeToJackal(this.JackalPlayerId, rolePlayer.PlayerId);
            }
        }
    }

}
