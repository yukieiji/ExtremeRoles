using System;
using System.Collections.Generic;

using Hazel;
using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Neutral
{
    public class Jackal : SingleRoleBase, IRoleAbility, IRoleUpdate
    {
        public enum JackalOption
        {
            SidekickLimitNum,
            RangeSidekickTarget,
            ForceReplaceLover,

            UpgradeSidekickNum,

            CanSetImpostorToSideKick,
            CanSeeImpostorToSideKickImpostor,
            SidekickUseSabotage,
            SidekickUseVent,
            SidekickJackalCanMakeSidekick,
            SideKickCanKill,

            SidekickHasOtherVison,
            SidekickVison,
            SidekickApplyEnvironmentVisionEffect
        }

        public List<byte> SideKickPlayerId = new List<byte>();

        public RoleAbilityButton Button
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
        public bool CanSetImpostorToSideKick = false;
        public bool CanSeeImpostorToSideKickImpostor = false;
        public bool SidekickCanKill = false;
        public bool SidekickUseSabotage = false;
        public bool SidekickUseVent = false;
        public bool SidekickJackalCanMakeSidekick = false;
        public bool SidekickHasOtherVison = false;
        public bool SidekickApplyEnvironmentVisionEffect = false;
        public float SidekickVision = 0f;

        public PlayerControl Target;

        private RoleAbilityButton createSidekick;

        private int numUpgradeSideKick = 0;
        private int createSidekickRange = 0;

        private bool isAlreadyUpgrated = false;


        public Jackal() : base(
            ExtremeRoleId.Jackal,
            ExtremeRoleType.Neutral,
            ExtremeRoleId.Jackal.ToString(),
            ColorPalette.JackalBlue,
            true, false, true, false)
        {
            this.SideKickPlayerId.Clear();
            this.isAlreadyUpgrated = false;
        }

        public static void TargetToSideKick(byte callerId, byte targetId)
        {
            var targetPlayer = Player.GetPlayerControlById(targetId);
            var targetRole = ExtremeRoleManager.GameRole[targetId];
            
            var sourceJackal = (Jackal)ExtremeRoleManager.GameRole[callerId];
            var newSidekick = new Sidekick(
                sourceJackal.GameControlId,
                sourceJackal.CurRecursion,
                targetRole.Team == ExtremeRoleType.Impostor,
                sourceJackal.SidekickCanKill,
                sourceJackal.SidekickUseVent,
                sourceJackal.SidekickUseSabotage,
                sourceJackal.CanSeeImpostorToSideKickImpostor,
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
            this.CreateAbilityButton(
                Translation.GetString("Sidekick"),
                Helper.Resources.LoadSpriteFromResources(
                    Resources.ResourcesPaths.JackalSidekick, 115f),
                abilityNum:10);
        }

        public override Color GetTargetRoleSeeColor(
            SingleRoleBase targetRole,
            byte targetPlayerId)
        {

            if (targetRole.Id == ExtremeRoleId.Sidekick &&
                this.SideKickPlayerId.Contains(targetPlayerId))
            {
                return ColorPalette.JackalBlue;
            }
            
            return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
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
            if (OptionsHolder.Ship.IsSameNeutralSameWin)
            {
                return this.isSameJackalTeam(targetRole);
            }
            else
            {
                return this.isSameJackalTeam(targetRole) && this.IsSameControlId(targetRole);
            }
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

            this.SideKickPlayerId.Add(targetPlayerId);

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                rolePlayer.NetId,
                (byte)RPCOperator.Command.ReplaceRole,
                Hazel.SendOption.Reliable, -1);

            writer.Write(rolePlayer.PlayerId);
            writer.Write(this.Target.PlayerId);
            writer.Write(
                (byte)ExtremeRoleManager.ReplaceOperation.ForceReplaceToSidekick);
            AmongUsClient.Instance.FinishRpcImmediately(writer);

            TargetToSideKick(rolePlayer.PlayerId, targetPlayerId);
            return true;
        }

        public void Update(PlayerControl rolePlayer)
        {
            if (this.SideKickPlayerId.Count == 0 || this.isAlreadyUpgrated) { return; }

            if (rolePlayer.Data.IsDead || rolePlayer.Data.Disconnected)
            {
                this.isAlreadyUpgrated = true;
                for (int i = 0; i < this.numUpgradeSideKick; ++i)
                {
                    int useIndex = UnityEngine.Random.Range(0, this.SideKickPlayerId.Count);
                    byte targetPlayerId = this.SideKickPlayerId[useIndex];
                    this.SideKickPlayerId.RemoveAt(useIndex);

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                        rolePlayer.NetId,
                        (byte)RPCOperator.Command.ReplaceRole,
                        Hazel.SendOption.Reliable, -1);

                    writer.Write(rolePlayer.PlayerId);
                    writer.Write(targetPlayerId);
                    writer.Write(
                        (byte)ExtremeRoleManager.ReplaceOperation.SidekickToJackal);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);

                    Sidekick.BecomeToJackal(rolePlayer.PlayerId, targetPlayerId);
                }
            }
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
            this.isAlreadyUpgrated = false;
            this.CurRecursion = 0;

            var allOption = OptionsHolder.AllOption;

            this.SidekickRecursionLimit = allOption[
                GetRoleOptionId(JackalOption.SidekickLimitNum)].GetValue();

            this.ForceReplaceLover = allOption[
                GetRoleOptionId(JackalOption.ForceReplaceLover)].GetValue();

            this.CanSetImpostorToSideKick = allOption[
                GetRoleOptionId(JackalOption.CanSetImpostorToSideKick)].GetValue();
            this.CanSeeImpostorToSideKickImpostor = allOption[
                GetRoleOptionId(JackalOption.CanSeeImpostorToSideKickImpostor)].GetValue();

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

            this.numUpgradeSideKick = allOption[
                GetRoleOptionId(JackalOption.UpgradeSidekickNum)].GetValue();
            
            this.RoleAbilityInit();
        }

        private void CreateJackalOption(CustomOptionBase parentOps)
        {

            this.CreateRoleAbilityOption(
                parentOps,
                maxAbilityCount: OptionsHolder.VanillaMaxPlayerNum - 1);

            CustomOption.Create(
                GetRoleOptionId(JackalOption.RangeSidekickTarget),
                Design.ConcatString(
                    this.RoleName,
                    JackalOption.RangeSidekickTarget.ToString()),
                OptionsHolder.Range,
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
                1, 1, OptionsHolder.VanillaMaxPlayerNum - 1, 1,
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
                1, 1, OptionsHolder.VanillaMaxPlayerNum / 2, 1,
                sidekickMakeSidekickOps);
        }
        private void CreateSideKickOption(CustomOptionBase parentOps)
        {
            CustomOption.Create(
                GetRoleOptionId(JackalOption.CanSetImpostorToSideKick),
                Design.ConcatString(
                    this.RoleName,
                    JackalOption.CanSetImpostorToSideKick.ToString()),
                false, parentOps);

            CustomOption.Create(
                GetRoleOptionId(JackalOption.CanSeeImpostorToSideKickImpostor),
                Design.ConcatString(
                    this.RoleName,
                    JackalOption.CanSeeImpostorToSideKickImpostor.ToString()),
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
            if (ExtremeRoleManager.GameRole[playerId].Team == ExtremeRoleType.Impostor)
            {
                return this.CanSetImpostorToSideKick;
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

        public void RoleAbilityResetOnMeetingStart()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }
    }

    public class Sidekick : SingleRoleBase
    {

        private int recursion = 0;
        private bool sidekickJackalCanMakeSidekick = false;

        public Sidekick(
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
            var jcakal = targetRole as Jackal;
            if (jcakal != null)
            {
                if (jcakal.SideKickPlayerId.Contains(
                    PlayerControl.LocalPlayer.PlayerId))
                {
                    return this.NameColor;
                }
            }
            return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
        }

        public static void BecomeToJackal(byte callerId, byte targetId)
        {
            var curJackal = (Jackal)ExtremeRoleManager.GameRole[callerId];
            var newJackal = (Jackal)curJackal.Clone();

            bool multiAssignTrigger = false;
            var curRole = ExtremeRoleManager.GameRole[targetId];
            var curSideKick = curRole as Sidekick;
            if (curJackal == null)
            {
                curSideKick = (Sidekick)((MultiAssignRoleBase)curRole).AnotherRole;
                multiAssignTrigger = true;
            }
            
            newJackal.Initialize();
            if (!curSideKick.sidekickJackalCanMakeSidekick || curSideKick.recursion >= newJackal.SidekickRecursionLimit)
            {
                newJackal.Button.UpdateAbilityCount(0);
            }
            newJackal.CurRecursion = curSideKick.recursion + 1;
            newJackal.SideKickPlayerId = new List<byte> (curJackal.SideKickPlayerId);
            newJackal.GameControlId = curSideKick.GameControlId;
            
            if (multiAssignTrigger)
            {
                var multiAssignRole = (MultiAssignRoleBase)curRole;
                multiAssignRole.AnotherRole = null;
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
    }

}
