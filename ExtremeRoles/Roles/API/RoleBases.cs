using System;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;


namespace ExtremeRoles.Roles.API
{
    public abstract class SingleRoleBase : RoleOptionBase
    {
        public bool CanCallMeeting = true;
        public bool HasTask = true;
        public bool UseVent = false;
        public bool UseSabotage = false;
        public bool HasOtherVison = false;
        public bool HasOtherKillCool = false;
        public bool HasOtherKillRange = false;
        public bool IsApplyEnvironmentVision = true;
        public bool IsWin = false;

        public bool FakeImposter = false;

        public float Vison = 0f;
        public float KillCoolTime = 0f;
        public int KillRange = 1;

        public string RoleName;

        public Color NameColor;
        public ExtremeRoleId Id;
        public byte BytedRoleId;
        public ExtremeRoleType Team;

        public int GameControlId = 0;

        public SingleRoleBase()
        { }
        public SingleRoleBase(
            ExtremeRoleId id,
            ExtremeRoleType team,
            string roleName,
            Color roleColor,
            bool canKill,
            bool hasTask,
            bool useVent,
            bool useSabotage,
            bool canCallMeeting=true)
        {
            this.Id = id;
            this.BytedRoleId = (byte)this.Id;
            this.Team = team;
            this.RoleName = roleName;
            this.NameColor = roleColor;
            this.CanKill = canKill;
            this.HasTask = hasTask;
            this.UseVent = useVent;
            this.UseSabotage = useSabotage;
            this.CanCallMeeting = canCallMeeting;
        }

        public virtual SingleRoleBase Clone()
        {
            SingleRoleBase copy = (SingleRoleBase)this.MemberwiseClone();
            Color baseColor = this.NameColor;

            copy.NameColor = new Color(
                baseColor.r,
                baseColor.g,
                baseColor.b,
                baseColor.a);

            return copy;
        }

        public bool IsVanillaRole() => this.Id == ExtremeRoleId.VanillaRole;

        public bool IsCrewmate() => this.Team == ExtremeRoleType.Crewmate;

        public bool IsImposter() => this.Team == ExtremeRoleType.Impostor;

        public bool IsNeutral() => this.Team == ExtremeRoleType.Neutral;

        public virtual void ExiledAction(
            GameData.PlayerInfo rolePlayer)
        {
            return;
        }
        public virtual string GetImportantText(bool isContainFakeTask = true)
        {
            string baseString = Design.ColoedString(
                this.NameColor,
                string.Format("{0}: {1}",
                    this.GetColoredRoleName(),
                    Translation.GetString(
                        string.Format("{0}{1}", this.Id, "ShortDescription"))));

            if (isContainFakeTask && !this.HasTask)
            {
                string fakeTaskString = Design.ColoedString(
                    this.NameColor,
                    DestroyableSingleton<TranslationController>.Instance.GetString(
                        StringNames.FakeTasks, Array.Empty<Il2CppSystem.Object>()));
                baseString = string.Format("{0}\r\n{1}",
                    baseString, fakeTaskString);
            }

            return baseString;
        }

        public virtual string GetIntroDescription() => Translation.GetString(
            string.Format("{0}{1}", this.Id, "IntroDescription"));

        public virtual string GetColoredRoleName() => Design.ColoedString(
            this.NameColor, Translation.GetString(this.RoleName));


        public virtual string GetRolePlayerNameTag(
            SingleRoleBase targetRole,
            byte targetPlayerId)
        {
            var multiAssignRole = targetRole as MultiAssignRoleBase;
            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {
                    return this.GetRolePlayerNameTag(
                        multiAssignRole.AnotherRole, targetPlayerId);
                }
            }

            return string.Empty;
        }
        public virtual Color GetTargetRoleSeeColor(
            SingleRoleBase targetRole,
            byte targetPlayerId)
        {
            var multiAssignRole = targetRole as MultiAssignRoleBase;
            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {
                    return this.GetTargetRoleSeeColor(
                        multiAssignRole.AnotherRole, targetPlayerId);
                }
            }
            else if ((targetRole.IsImposter() || targetRole.FakeImposter) &&
                this.IsImposter())
            {
                return Palette.ImpostorRed;
            }
            return Palette.White;
        }

        public virtual bool IsSameTeam(SingleRoleBase targetRole)
        {
            var multiAssignRole = targetRole as MultiAssignRoleBase;
            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {
                    return this.IsSameTeam(
                        multiAssignRole.AnotherRole);
                }
            }

            if (this.Team == ExtremeRoleType.Crewmate)
            {
                return true;
            }
            else
            {
                return targetRole.Team == this.Team;
            }
        }

        public virtual bool IsTeamsWin() => this.IsWin;


        public virtual void RolePlayerKilledAction(
            PlayerControl rolePlayer,
            PlayerControl killerPlayer)
        {
            return;
        }

        public virtual bool TryRolePlayerKill(
            PlayerControl rolePlayer,
            PlayerControl fromPlayer) => true;

        protected override void CreateKillerOption(
            CustomOptionBase parentOps)
        {
            var killCoolOption = CustomOption.Create(
                GetRoleOptionId(KillerCommonOption.HasOtherKillCool),
                Design.ConcatString(
                    this.RoleName,
                    KillerCommonOption.HasOtherKillCool.ToString()),
                false, parentOps);
            CustomOption.Create(
                GetRoleOptionId(KillerCommonOption.KillCoolDown),
                Design.ConcatString(
                    this.RoleName,
                    KillerCommonOption.KillCoolDown.ToString()),
                30f, 1.0f, 120f, 0.5f,
                killCoolOption, format: "unitSeconds");

            var killRangeOption = CustomOption.Create(
                GetRoleOptionId(KillerCommonOption.HasOtherKillRange),
                Design.ConcatString(
                    this.RoleName,
                    KillerCommonOption.HasOtherKillRange.ToString()),
                false, parentOps);
            CustomOption.Create(
                GetRoleOptionId(KillerCommonOption.KillRange),
                Design.ConcatString(
                    this.RoleName,
                    KillerCommonOption.KillRange.ToString()),
                OptionsHolder.Range,
                killRangeOption);
        }
        protected override CustomOptionBase CreateSpawnOption()
        {
            var roleSetOption = CustomOption.Create(
                GetRoleOptionId(RoleCommonOption.SpawnRate),
                Design.ColoedString(
                    this.NameColor,
                    Design.ConcatString(
                        this.RoleName,
                        RoleCommonOption.SpawnRate.ToString())),
                OptionsHolder.SpawnRate, null, true);

            CustomOption.Create(
                GetRoleOptionId(RoleCommonOption.RoleNum),
                Design.ConcatString(
                    this.RoleName,
                    RoleCommonOption.RoleNum.ToString()),
                1, 1, OptionsHolder.VanillaMaxPlayerNum - 1, 1, roleSetOption);

            return roleSetOption;
        }

        protected override void CreateVisonOption(
            CustomOptionBase parentOps)
        {
            var visonOption = CustomOption.Create(
                GetRoleOptionId(RoleCommonOption.HasOtherVison),
                Design.ConcatString(
                    this.RoleName,
                    RoleCommonOption.HasOtherVison.ToString()),
                false, parentOps);

            CustomOption.Create(
                GetRoleOptionId(RoleCommonOption.Vison),
                Design.ConcatString(
                    this.RoleName,
                    RoleCommonOption.Vison.ToString()),
                2f, 0.25f, 5f, 0.25f,
                visonOption, format: "unitMultiplier");
            CustomOption.Create(
               GetRoleOptionId(RoleCommonOption.ApplyEnvironmentVisionEffect),
               Design.ConcatString(
                   this.RoleName,
                   RoleCommonOption.ApplyEnvironmentVisionEffect.ToString()),
               this.IsCrewmate(), visonOption);
        }
        protected override void CommonInit()
        {
            var allOption = OptionsHolder.AllOption;

            this.HasOtherVison = allOption[
                GetRoleOptionId(RoleCommonOption.HasOtherVison)].GetValue();
            this.Vison = allOption[
                GetRoleOptionId(RoleCommonOption.Vison)].GetValue();
            this.IsApplyEnvironmentVision = allOption[
                GetRoleOptionId(RoleCommonOption.ApplyEnvironmentVisionEffect)].GetValue();

            if (this.CanKill)
            {
                this.HasOtherKillCool = allOption[
                    GetRoleOptionId(KillerCommonOption.HasOtherKillCool)].GetValue();
                this.KillCoolTime = allOption[
                    GetRoleOptionId(KillerCommonOption.KillCoolDown)].GetValue();
                this.HasOtherKillRange = allOption[
                    GetRoleOptionId(KillerCommonOption.HasOtherKillRange)].GetValue();
                this.KillRange = allOption[
                    GetRoleOptionId(KillerCommonOption.KillRange)].GetValue();
            }
        }

        protected bool IsSameControlId(SingleRoleBase tarrgetRole)
        {
            return this.GameControlId == tarrgetRole.GameControlId;
        }

    }
    public abstract class MultiAssignRoleBase : SingleRoleBase
    {
        public int ManagerOptionOffset = 0;
        public SingleRoleBase AnotherRole = null;
        public bool CanHasAnotherRole = false;

        private string prevRoleName = "";

        public MultiAssignRoleBase(
            ExtremeRoleId id,
            ExtremeRoleType team,
            string roleName,
            Color roleColor,
            bool canKill,
            bool hasTask,
            bool useVent,
            bool useSabotage,
            bool canCallMeeting = true) : base(
                id, team, roleName, roleColor,
                canKill, hasTask, useVent,
                useSabotage, canCallMeeting)
        { }

        public void SetAnotherRole(SingleRoleBase role)
        {
            if (this.CanHasAnotherRole && this.AnotherRole == null)
            {
                this.AnotherRole = role;
                OverrideAnotherRoleSetting();
            }
        }

        public override string GetImportantText(bool isContainFakeTask = true)
        {

            if (this.AnotherRole == null)
            {
                return base.GetImportantText();
            }

            string baseString = base.GetImportantText(false);
            string anotherRoleString = this.AnotherRole.GetImportantText(false);

            baseString = string.Format("{0}\r\n{1}",
                baseString, anotherRoleString);

            if (isContainFakeTask && !this.HasTask)
            {
                string fakeTaskString = Design.ColoedString(
                    this.NameColor,
                    DestroyableSingleton<TranslationController>.Instance.GetString(
                        StringNames.FakeTasks, Array.Empty<Il2CppSystem.Object>()));
                baseString = string.Format("{0}\r\n{1}",
                    baseString, fakeTaskString);
            }

            return baseString;
        }

        public override string GetIntroDescription()
        {
            if (this.AnotherRole == null)
            {
                return base.GetIntroDescription();
            }

            string baseIntro = Translation.GetString(string.Format(
            "{0}{1}", this.Id, "IntroDescription"));

            string anotherIntro;

            if (this.AnotherRole.IsVanillaRole())
            {
                RoleBehaviour role = PlayerControl.LocalPlayer.Data.Role;
                anotherIntro = role.Blurb;
            }
            else
            {
                anotherIntro = Translation.GetString(string.Format(
               "{0}{1}", this.AnotherRole.Id, "IntroDescription"));

            }

            string concat = Design.ColoedString(
                Palette.White, "\n and");


            return string.Format("{0}{1}{2}",
                baseIntro, concat, anotherIntro);

        }
        public override string GetColoredRoleName()
        {
            if (this.AnotherRole == null)
            {
                return base.GetColoredRoleName();
            }

            string baseRole = Design.ColoedString(
                this.NameColor,
                Translation.GetString(this.prevRoleName));

            string anotherRole = Design.ColoedString(
                this.AnotherRole.NameColor,
                Translation.GetString(this.AnotherRole.RoleName));

            string concat = Design.ColoedString(
                Palette.White, " + ");

            return string.Format("{0}{1}{2}",
                baseRole, concat, anotherRole);
        }

        protected virtual void OverrideAnotherRoleSetting()
        {
            this.prevRoleName = string.Copy(this.RoleName);

            this.RoleName = string.Format("{0} + {1}",
                string.Copy(this.prevRoleName),
                string.Copy(this.AnotherRole.RoleName));

            this.CanKill = this.CanKill || this.AnotherRole.CanKill;
            this.HasTask = this.HasTask || this.AnotherRole.HasTask;
            this.UseVent = this.UseVent || this.AnotherRole.UseVent;
            this.UseSabotage = this.UseSabotage || this.AnotherRole.UseSabotage;
        }

        protected int GetManagerOptionId(
            RoleCommonOption option) => GetManagerOptionId((int)option);

        protected int GetManagerOptionId(
            KillerCommonOption option) => GetManagerOptionId((int)option);

        protected int GetManagerOptionId(
            CombinationRoleCommonOption option) => GetManagerOptionId((int)option);

        protected int GetManagerOptionId(int option) => this.ManagerOptionOffset + option;
    }
}
