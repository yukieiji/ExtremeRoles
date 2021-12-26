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
        public enum JackalSetting
        {
            SidekickNum,
            SidekickLimitNum,
            RangeSidekickTarget,

            UpgradeSidekickNum,

            CanSetImpostorToSideKick,
            CanSeeImpostorToSideKickImpostor,
            SidekickUseSabotage,
            SidekickUseVent,
            SidekickJackalCanMakeSidekick,

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

        public bool CanSetImpostorToSideKick = false;
        public bool CanSeeImpostorToSideKickImpostor = false;
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
            var targetRole = ExtremeRoleManager.GameRole[targetId];
            
            var sourceJackal = (Jackal)ExtremeRoleManager.GameRole[callerId];
            var newSidekick = new Sidekick(
                sourceJackal.CurRecursion,
                targetRole.Teams == ExtremeRoleType.Impostor,
                sourceJackal.SidekickUseSabotage,
                sourceJackal.SidekickUseVent,
                sourceJackal.CanSeeImpostorToSideKickImpostor,
                sourceJackal.SidekickJackalCanMakeSidekick,
                sourceJackal.SidekickHasOtherVison,
                sourceJackal.SidekickVision,
                sourceJackal.SidekickApplyEnvironmentVisionEffect);

            ExtremeRoleManager.GameRole[targetId] = newSidekick;

        }

        public void CreateAbility()
        {
            this.Button = this.CreateAbilityButton(
                Helper.Resources.LoadSpriteFromResources(
                    Resources.ResourcesPaths.TestButton, 115f));
        }

        public bool IsAbilityUse()
        {
            this.setTarget();
            return this.Target != null && this.IsCommonUse();
        }

        public void UseAbility()
        {
            byte targetPlayerId = this.Target.PlayerId;
            PlayerControl rolePlayer = PlayerControl.LocalPlayer;

            this.SideKickPlayerId.Add(targetPlayerId);

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                rolePlayer.NetId, (byte)CustomRPC.ReplaceRole, Hazel.SendOption.Reliable, -1);

            writer.Write(rolePlayer.PlayerId);
            writer.Write(this.Target.PlayerId);
            writer.Write(
                (byte)ExtremeRoleManager.ReplaceOperation.ForceReplaceToSidekick);
            AmongUsClient.Instance.FinishRpcImmediately(writer);

            TargetToSideKick(rolePlayer.PlayerId, targetPlayerId);
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
                        rolePlayer.NetId, (byte)CustomRPC.ReplaceRole, Hazel.SendOption.Reliable, -1);

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
            // JackalSetting
            this.CreateJackalOption(parentOps);

            // SideKickOption
            this.CreateSideKickOption(parentOps);
        }

        protected override void RoleSpecificInit()
        {
            this.isAlreadyUpgrated = false;
            this.CurRecursion = 0;

            var allOption = OptionsHolder.AllOptions;

            this.NumAbility = allOption[
                GetRoleSettingId((int)JackalSetting.SidekickNum)].GetValue();
            this.SidekickRecursionLimit = allOption[
                GetRoleSettingId((int)JackalSetting.SidekickLimitNum)].GetValue();

            this.CanSetImpostorToSideKick = allOption[
                GetRoleSettingId((int)JackalSetting.CanSetImpostorToSideKick)].GetValue();
            this.CanSeeImpostorToSideKickImpostor = allOption[
                GetRoleSettingId((int)JackalSetting.CanSeeImpostorToSideKickImpostor)].GetValue();

            this.SidekickUseSabotage = allOption[
                GetRoleSettingId((int)JackalSetting.SidekickUseSabotage)].GetValue();
            this.SidekickUseVent = allOption[
                GetRoleSettingId((int)JackalSetting.SidekickUseVent)].GetValue();
            this.SidekickJackalCanMakeSidekick = allOption[
                GetRoleSettingId((int)JackalSetting.SidekickJackalCanMakeSidekick)].GetValue();

            this.SidekickHasOtherVison = allOption[
                GetRoleSettingId((int)JackalSetting.SidekickHasOtherVison)].GetValue();
            this.SidekickVision = allOption[
                GetRoleSettingId((int)JackalSetting.SidekickVison)].GetValue();
            this.SidekickApplyEnvironmentVisionEffect = allOption[
                GetRoleSettingId((int)JackalSetting.SidekickApplyEnvironmentVisionEffect)].GetValue();

            this.createSidekickRange = allOption[
                GetRoleSettingId((int)JackalSetting.RangeSidekickTarget)].GetValue();

            this.numUpgradeSideKick = allOption[
                GetRoleSettingId((int)JackalSetting.UpgradeSidekickNum)].GetValue();
            
            this.RoleAbilityInit();
        
        }

        private void CreateJackalOption(CustomOptionBase parentOps)
        {
            CustomOption.Create(
                GetRoleSettingId((int)JackalSetting.SidekickNum),
                Design.ConcatString(
                    this.RoleName.ToString(),
                    JackalSetting.SidekickNum.ToString()),
                1, 1, OptionsHolder.VanillaMaxPlayerNum - 1, 1,
                parentOps);

            CustomOption.Create(
                GetRoleSettingId((int)JackalSetting.RangeSidekickTarget),
                Design.ConcatString(
                    this.RoleName,
                    JackalSetting.RangeSidekickTarget.ToString()),
                OptionsHolder.KillRange,
                parentOps);

            CustomOption.Create(
                GetRoleSettingId((int)JackalSetting.UpgradeSidekickNum),
                Design.ConcatString(
                    this.RoleName,
                    JackalSetting.UpgradeSidekickNum.ToString()),
                1, 1, OptionsHolder.VanillaMaxPlayerNum - 1, 1,
                parentOps);

            var sidekickMakeSidekickOps = CustomOption.Create(
                GetRoleSettingId((int)JackalSetting.SidekickJackalCanMakeSidekick),
                Design.ConcatString(
                    this.RoleName,
                    JackalSetting.SidekickJackalCanMakeSidekick.ToString()),
                false, parentOps);

            CustomOption.Create(
                GetRoleSettingId((int)JackalSetting.SidekickLimitNum),
                Design.ConcatString(
                    this.RoleName,
                    JackalSetting.SidekickLimitNum.ToString()),
                1, 1, OptionsHolder.VanillaMaxPlayerNum / 2, 1,
                sidekickMakeSidekickOps);

            this.CreateRoleAbilityOption(parentOps);
        }
        private void CreateSideKickOption(CustomOptionBase parentOps)
        {
            CustomOption.Create(
                GetRoleSettingId((int)JackalSetting.CanSetImpostorToSideKick),
                Design.ConcatString(
                    this.RoleName,
                    JackalSetting.CanSetImpostorToSideKick.ToString()),
                false, parentOps);

            CustomOption.Create(
                GetRoleSettingId((int)JackalSetting.CanSeeImpostorToSideKickImpostor),
                Design.ConcatString(
                    this.RoleName,
                    JackalSetting.CanSeeImpostorToSideKickImpostor.ToString()),
                false, parentOps);

            CustomOption.Create(
                GetRoleSettingId((int)JackalSetting.SidekickUseSabotage),
                Design.ConcatString(
                    this.RoleName,
                    JackalSetting.SidekickUseSabotage.ToString()),
                true, parentOps);
            CustomOption.Create(
                GetRoleSettingId((int)JackalSetting.SidekickUseVent),
                Design.ConcatString(
                    this.RoleName,
                    JackalSetting.SidekickUseVent.ToString()),
                true, parentOps);

            var visonOption = CustomOption.Create(
                GetRoleSettingId((int)JackalSetting.SidekickHasOtherVison),
                Design.ConcatString(
                    this.RoleName,
                    JackalSetting.SidekickHasOtherVison.ToString()),
                false, parentOps);

            CustomOption.Create(
                GetRoleSettingId((int)JackalSetting.SidekickVison),
                Design.ConcatString(
                    this.RoleName,
                   JackalSetting.SidekickVison.ToString()),
                2f, 0.25f, 5f, 0.25f,
                visonOption, format: "unitMultiplier");
            CustomOption.Create(
               GetRoleSettingId((int)JackalSetting.SidekickApplyEnvironmentVisionEffect),
               Design.ConcatString(
                   this.RoleName,
                   JackalSetting.SidekickApplyEnvironmentVisionEffect.ToString()),
               false, visonOption);
        }

        private bool isImpostorToTarget(
            GameData.PlayerInfo playerInfo)
        {
            if (ExtremeRoleManager.GameRole[playerInfo.PlayerId].Teams == ExtremeRoleType.Impostor)
            {
                return this.CanSetImpostorToSideKick;
            }
            return true;
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
                    isImpostorToTarget(playerInfo) &&
                    !playerInfo.Object.inVent)
                {
                    PlayerControl @object = playerInfo.Object;
                    if (@object && @object.Collider.enabled)
                    {
                        Vector2 vector = @object.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(
                            truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
                        {
                            result = @object;
                            num = magnitude;
                        }
                    }
                }
            }
            this.Target = result;
        }

    }

    public class Sidekick : SingleRoleBase
    {
        public bool IsPrevRoleImpostor = false;
        public bool CanSeeImpostorToSideKickImpostor = false;

        private int recursion = 0;
        private bool sidekickJackalCanMakeSidekick = false;

        public Sidekick(
            int curRecursion,
            bool isImpostor,
            bool useSabotage,
            bool useVent,
            bool canSeeImpostorToSideKickImpostor,
            bool sidekickJackalCanMakeSidekick,
            bool hasOtherVision,
            float vison,
            bool applyEnvironmentVisionEffect
            ) : base(
                ExtremeRoleId.Sidekick,
                ExtremeRoleType.Neutral,
                ExtremeRoleId.Sidekick.ToString(),
                ColorPalette.JackalBlue,
                false, false, useVent, useSabotage)
        {

            this.HasOtherVison = hasOtherVision;
            this.Vison = vison;
            this.IsApplyEnvironmentVision = applyEnvironmentVisionEffect;

            this.IsPrevRoleImpostor = isImpostor;
            this.CanSeeImpostorToSideKickImpostor = canSeeImpostorToSideKickImpostor;

            this.recursion = curRecursion;
            this.sidekickJackalCanMakeSidekick = sidekickJackalCanMakeSidekick;
        }

        public static void BecomeToJackal(byte callerId, byte targetId)
        {

            var newJackal = new Jackal();
            var curJackal = (Jackal)ExtremeRoleManager.GameRole[callerId];
            var curSideKick = (Sidekick)ExtremeRoleManager.GameRole[targetId];
            
            newJackal.GameInit();
            if (!curSideKick.sidekickJackalCanMakeSidekick || curSideKick.recursion >= newJackal.SidekickRecursionLimit)
            {
                newJackal.NumAbility = 0;
            }
            newJackal.CurRecursion = curSideKick.recursion + 1;
            newJackal.SideKickPlayerId = new List<byte> (curJackal.SideKickPlayerId);

            ExtremeRoleManager.GameRole[targetId] = newJackal;

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
