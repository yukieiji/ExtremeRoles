using System;
using System.Runtime.CompilerServices;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Performance;


namespace ExtremeRoles.Roles.API
{
    public abstract class SingleRoleBase : RoleOptionBase
    {
        public virtual bool IsAssignGhostRole => true;

        public bool CanCallMeeting = true;
        public bool CanRepairSabotage = true;

        public bool CanUseAdmin = true;
        public bool CanUseSecurity = true;
        public bool CanUseVital = true;

        public bool HasTask = true;
        public bool UseVent = false;
        public bool UseSabotage = false;
        public bool HasOtherVison = false;
        public bool HasOtherKillCool = false;
        public bool HasOtherKillRange = false;
        public bool IsApplyEnvironmentVision = true;
        public bool IsWin = false;

        public bool FakeImposter = false;
        public bool IsBoost = false;
        public float MoveSpeed = 1.0f;

        public float Vison = 0f;
        public float KillCoolTime = 0f;
        public int KillRange = 1;

        public string RoleName;

        public ExtremeRoleId Id;
        public ExtremeRoleType Team;

        public int GameControlId = 0;
        protected Color NameColor;

        public OptionTab Tab => this.tab;

        private OptionTab tab = OptionTab.General;

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
            bool canCallMeeting = true,
            bool canRepairSabotage = true,
            bool canUseAdmin = true,
            bool canUseSecurity = true,
            bool canUseVital = true,
            OptionTab tab = OptionTab.General)
        {
            this.Id = id;
            this.Team = team;
            this.RoleName = roleName;
            this.NameColor = roleColor;
            this.CanKill = canKill;
            this.HasTask = hasTask;
            this.UseVent = useVent;
            this.UseSabotage = useSabotage;

            this.CanCallMeeting = canCallMeeting;
            this.CanRepairSabotage = canRepairSabotage;

            this.CanUseAdmin = canUseAdmin;
            this.CanUseSecurity = canUseSecurity;
            this.CanUseVital = canUseVital;

            if (tab == OptionTab.General)
            {
                switch (this.Team)
                {
                    case ExtremeRoleType.Crewmate:
                        this.tab = OptionTab.Crewmate;
                        break;
                    case ExtremeRoleType.Impostor:
                        this.tab = OptionTab.Impostor;
                        break;
                    case ExtremeRoleType.Neutral:
                        this.tab = OptionTab.Neutral;
                        break;
                    default:
                        this.tab = OptionTab.General;
                        break;
                }
            }
            else
            {
                this.tab = tab;
            }
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

        public bool IsImpostor() => this.Team == ExtremeRoleType.Impostor;

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
                    Design.ColoedString(
                        this.NameColor,
                        Translation.GetString(this.RoleName)),
                    Translation.GetString(
                        $"{this.Id}ShortDescription")));

            if (isContainFakeTask && !this.HasTask)
            {
                string fakeTaskString = Design.ColoedString(
                    this.NameColor,
                    DestroyableSingleton<TranslationController>.Instance.GetString(
                        StringNames.FakeTasks, Array.Empty<Il2CppSystem.Object>()));
                baseString = $"{baseString}\r\n{fakeTaskString}";
            }

            return baseString;
        }

        public void SetNameColor(Color newColor)
        {
            this.NameColor = newColor;
        }

        public virtual Color GetNameColor(bool isTruthColor = false) => this.NameColor;

        public virtual string GetIntroDescription() => Translation.GetString(
            $"{this.Id}IntroDescription");

        public virtual string GetFullDescription() => Translation.GetString(
           $"{this.Id}FullDescription");

        public virtual string GetColoredRoleName(bool isTruthName = false) => Design.ColoedString(
            this.NameColor, Translation.GetString(this.RoleName));


        public virtual string GetRoleTag() => string.Empty;

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
                        multiAssignRole.AnotherRole,
                        targetPlayerId);
                }
            }

            return string.Empty;
        }
        public virtual Color GetTargetRoleSeeColor(
            SingleRoleBase targetRole,
            byte targetPlayerId)
        {
            var overLoader = targetRole as Solo.Impostor.OverLoader;

            if (overLoader != null)
            {
                if(overLoader.IsOverLoad)
                {
                    return Palette.ImpostorRed;
                }
            }
            
            if ((targetRole.IsImpostor() || targetRole.FakeImposter) &&
                this.IsImpostor())
            {
                return Palette.ImpostorRed;
            }
            var multiAssignRole = targetRole as MultiAssignRoleBase;
            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {
                    return this.GetTargetRoleSeeColor(
                        multiAssignRole.AnotherRole, targetPlayerId);
                }
            }

            return Palette.White;
        }

        public virtual bool IsSameTeam(SingleRoleBase targetRole)
        {

            if (this.IsImpostor())
            {
                return targetRole.Team == ExtremeRoleType.Impostor;
            }

            var multiAssignRole = targetRole as MultiAssignRoleBase;
            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {
                    return this.IsSameTeam(
                        multiAssignRole.AnotherRole);
                }
            }

            return false;
        }

        public virtual bool IsTeamsWin() => this.IsWin;

        public virtual bool IsBlockShowMeetingRoleInfo() => false;
        public virtual bool IsBlockShowPlayingRoleInfo() => false;

        public virtual void RolePlayerKilledAction(
            PlayerControl rolePlayer,
            PlayerControl killerPlayer)
        {
            return;
        }

        public virtual bool TryRolePlayerKillTo(
            PlayerControl rolePlayer,
            PlayerControl targetPlayer) => true;

        public virtual bool TryRolePlayerKilledFrom(
            PlayerControl rolePlayer,
            PlayerControl fromPlayer) => true;

        protected override void CreateKillerOption(
            IOption parentOps)
        {
            var killCoolOption = CreateBoolOption(
                KillerCommonOption.HasOtherKillCool,
                false, parentOps);
            CreateFloatOption(
                KillerCommonOption.KillCoolDown,
                30f, 1.0f, 120f, 0.5f,
                killCoolOption, format: OptionUnit.Second);

            var killRangeOption = CreateBoolOption(
                KillerCommonOption.HasOtherKillRange,
                false, parentOps);
            CreateSelectionOption(
                KillerCommonOption.KillRange,
                OptionHolder.Range,
                killRangeOption);
        }
        protected override IOption CreateSpawnOption()
        {
            var roleSetOption = CreateSelectionOption(
                RoleCommonOption.SpawnRate,
                OptionHolder.SpawnRate, null, true,
                colored: true);

            int spawnNum = this.IsImpostor() ? OptionHolder.MaxImposterNum : OptionHolder.VanillaMaxPlayerNum - 1;

            CreateIntOption(
                RoleCommonOption.RoleNum,
                1, 1, spawnNum, 1, roleSetOption);

            return roleSetOption;
        }

        protected override void CreateVisonOption(
            IOption parentOps)
        {
            var visonOption = CreateBoolOption(
                RoleCommonOption.HasOtherVison,
                false, parentOps);
            CreateFloatOption(RoleCommonOption.Vison,
                2f, 0.25f, 5.0f, 0.25f,
                visonOption, format: OptionUnit.Multiplier);

            CreateBoolOption(
                RoleCommonOption.ApplyEnvironmentVisionEffect,
               this.IsCrewmate(), visonOption);
        }
        protected override void CommonInit()
        {
            var baseOption = PlayerControl.GameOptions;
            var allOption = OptionHolder.AllOption;

            this.Vison = this.IsImpostor() ? baseOption.ImpostorLightMod : baseOption.CrewLightMod;
            
            this.KillCoolTime = baseOption.KillCooldown;
            this.KillRange = baseOption.KillDistance;

            this.IsApplyEnvironmentVision = !this.IsImpostor();


            this.HasOtherVison = allOption[
                GetRoleOptionId(RoleCommonOption.HasOtherVison)].GetValue();
            if (this.HasOtherVison)
            {
                this.Vison = allOption[
                    GetRoleOptionId(RoleCommonOption.Vison)].GetValue();
                this.IsApplyEnvironmentVision = allOption[
                    GetRoleOptionId(RoleCommonOption.ApplyEnvironmentVisionEffect)].GetValue();
            }

            if (this.CanKill)
            {
                this.HasOtherKillCool = allOption[
                    GetRoleOptionId(KillerCommonOption.HasOtherKillCool)].GetValue();
                if (this.HasOtherKillCool)
                {
                    this.KillCoolTime = allOption[
                        GetRoleOptionId(KillerCommonOption.KillCoolDown)].GetValue();
                }

                this.HasOtherKillRange = allOption[
                    GetRoleOptionId(KillerCommonOption.HasOtherKillRange)].GetValue();

                if (this.HasOtherKillRange)
                {
                    this.KillRange = allOption[
                        GetRoleOptionId(KillerCommonOption.KillRange)].GetValue();
                }
            }
        }
        protected bool IsSameControlId(SingleRoleBase tarrgetRole)
        {
            return this.GameControlId == tarrgetRole.GameControlId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected FloatCustomOption CreateFloatOption<T>(
            T option,
            float defaultValue,
            float min, float max, float step,
            IOption parent = null,
            bool isHeader = false,
            bool isHidden = false,
            OptionUnit format = OptionUnit.None,
            bool invert = false,
            IOption enableCheckOption = null,
            bool colored = false) where T : struct, IConvertible
        {
            EnumCheck(option);

            return new FloatCustomOption(
                GetRoleOptionId(option),
                createAutoOptionString(option, colored),
                defaultValue,
                min, max, step,
                parent, isHeader, isHidden,
                format, invert, enableCheckOption, this.tab);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected FloatDynamicCustomOption CreateFloatDynamicOption<T>(
            T option,
            float defaultValue,
            float min, float step,
            IOption parent = null,
            bool isHeader = false,
            bool isHidden = false,
            OptionUnit format = OptionUnit.None,
            bool invert = false,
            IOption enableCheckOption = null,
            bool colored = false,
            float tempMaxValue = 0.0f) where T : struct, IConvertible
        {
            EnumCheck(option);

            return new FloatDynamicCustomOption(
                GetRoleOptionId(option),
                createAutoOptionString(option, colored),
                defaultValue,
                min, step,
                parent, isHeader, isHidden,
                format, invert, enableCheckOption,
                this.tab, tempMaxValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected IntCustomOption CreateIntOption<T>(
            T option,
            int defaultValue,
            int min, int max, int step,
            IOption parent = null,
            bool isHeader = false,
            bool isHidden = false,
            OptionUnit format = OptionUnit.None,
            bool invert = false,
            IOption enableCheckOption = null,
            bool colored = false) where T : struct, IConvertible
        {
            EnumCheck(option);

            return new IntCustomOption(
                GetRoleOptionId(option),
                createAutoOptionString(option, colored),
                defaultValue,
                min, max, step,
                parent, isHeader, isHidden,
                format, invert, enableCheckOption, this.tab);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected IntDynamicCustomOption CreateIntDynamicOption<T>(
            T option,
            int defaultValue,
            int min, int step,
            IOption parent = null,
            bool isHeader = false,
            bool isHidden = false,
            OptionUnit format = OptionUnit.None,
            bool invert = false,
            IOption enableCheckOption = null,
            bool colored = false,
            int tempMaxValue = 0) where T : struct, IConvertible
        {
            EnumCheck(option);

            return new IntDynamicCustomOption(
                GetRoleOptionId(option),
                createAutoOptionString(option, colored),
                defaultValue,
                min, step,
                parent, isHeader, isHidden,
                format, invert, enableCheckOption,
                this.tab, tempMaxValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected BoolCustomOption CreateBoolOption<T>(
            T option,
            bool defaultValue,
            IOption parent = null,
            bool isHeader = false,
            bool isHidden = false,
            OptionUnit format = OptionUnit.None,
            bool invert = false,
            IOption enableCheckOption = null,
            bool colored = false) where T : struct, IConvertible
        {
            EnumCheck(option);

            return new BoolCustomOption(
                GetRoleOptionId(option),
                createAutoOptionString(option, colored),
                defaultValue,
                parent, isHeader, isHidden,
                format, invert, enableCheckOption, this.tab);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected SelectionCustomOption CreateSelectionOption<T>(
            T option,
            string[] selections,
            IOption parent = null,
            bool isHeader = false,
            bool isHidden = false,
            OptionUnit format = OptionUnit.None,
            bool invert = false,
            IOption enableCheckOption = null,
            bool colored = false) where T : struct, IConvertible
        {
            EnumCheck(option);

            return new SelectionCustomOption(
                GetRoleOptionId(option),
                createAutoOptionString(option, colored),
                selections,
                parent, isHeader, isHidden,
                format, invert, enableCheckOption, this.tab);
        }

        private string createAutoOptionString<T>(
            T option, bool colored) where T : struct, IConvertible
        {
            if (!colored)
            {
                return string.Concat(
                    this.RoleName, option.ToString());
            }
            else
            {
                return Design.ColoedString(
                    this.NameColor,
                    string.Concat(
                        this.RoleName,
                        RoleCommonOption.SpawnRate.ToString()));
            }
        }
    }
    public abstract class MultiAssignRoleBase : SingleRoleBase
    {
        
        public SingleRoleBase AnotherRole = null;
        public bool CanHasAnotherRole = false;
        protected int ManagerOptionOffset = 0;

        public MultiAssignRoleBase(
            ExtremeRoleId id,
            ExtremeRoleType team,
            string roleName,
            Color roleColor,
            bool canKill,
            bool hasTask,
            bool useVent,
            bool useSabotage,
            bool canCallMeeting = true,
            bool canRepairSabotage = true,
            bool canUseAdmin = true,
            bool canUseSecurity = true,
            bool canUseVital = true,
            OptionTab tab = OptionTab.General) : base(
                id, team, roleName, roleColor,
                canKill, hasTask, useVent,
                useSabotage, canCallMeeting,
                canRepairSabotage, canUseAdmin,
                canUseSecurity, canUseVital, tab)
        { }

        public void SetRoleType(RoleTypes roleType)
        {
            switch (roleType)
            {
                case RoleTypes.Shapeshifter:
                case RoleTypes.Impostor:
                    this.Team = ExtremeRoleType.Impostor;
                    this.NameColor = Palette.ImpostorRed;
                    this.CanKill = true;
                    this.UseVent = true;
                    this.UseSabotage = true;
                    this.HasTask = false;
                    break;
                case RoleTypes.Crewmate:
                case RoleTypes.Engineer:
                case RoleTypes.Scientist:
                    this.CanKill = false;
                    this.UseVent = false;
                    this.UseSabotage = false;
                    this.HasTask = true;
                    break;
                default:
                    break;
            };
        }

        public void SetAnotherRole(SingleRoleBase role)
        {

            if (this.CanHasAnotherRole)
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

            baseString = $"{baseString}\r\n{anotherRoleString}";

            if (isContainFakeTask && (!this.HasTask || !this.AnotherRole.HasTask))
            {
                string fakeTaskString = Design.ColoedString(
                    this.NameColor,
                    FastDestroyableSingleton<TranslationController>.Instance.GetString(
                        StringNames.FakeTasks, Array.Empty<Il2CppSystem.Object>()));
                baseString = $"{baseString}\r\n{fakeTaskString}";
            }

            return baseString;
        }

        public override string GetIntroDescription()
        {

            string baseIntro = Translation.GetString(
                $"{this.Id}IntroDescription");

            if (this.AnotherRole == null)
            {
                return baseIntro;
            }

            string anotherIntro;

            if (this.AnotherRole.IsVanillaRole())
            {
                RoleBehaviour role = CachedPlayerControl.LocalPlayer.Data.Role;
                anotherIntro = role.Blurb;
            }
            else
            {
                anotherIntro = this.AnotherRole.GetIntroDescription();
            }

            string concat = Design.ColoedString(
                Palette.White,
                string.Concat(
                    "\n ", Translation.GetString("introAnd")));


            return string.Concat(baseIntro, concat, Design.ColoedString(
                this.AnotherRole.GetNameColor(),
                anotherIntro));

        }
        public override string GetColoredRoleName(bool isTruthColor = false)
        {
            if (this.AnotherRole == null)
            {
                return base.GetColoredRoleName(isTruthColor);
            }

            string baseRole = Design.ColoedString(
                this.NameColor,
                Translation.GetString(this.RoleName));

            string anotherRole = this.AnotherRole.GetColoredRoleName(isTruthColor);

            string concat = Design.ColoedString(
                Palette.White, " + ");

            return string.Concat(
                baseRole, concat, anotherRole);
        }

        public override Color GetTargetRoleSeeColor(
            SingleRoleBase targetRole,
            byte targetPlayerId)
        {

            if (this.CanHasAnotherRole && this.AnotherRole != null)
            {
                Color color = this.AnotherRole.GetTargetRoleSeeColor(
                    targetRole, targetPlayerId);

                if (color != Palette.White) { return color; }
            }

            return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
        }

        public virtual void OverrideAnotherRoleSetting()
        {
            this.CanKill = this.CanKill || this.AnotherRole.CanKill;
            this.HasTask = this.HasTask || this.AnotherRole.HasTask;
            this.UseVent = this.UseVent || this.AnotherRole.UseVent;
            this.UseSabotage = this.UseSabotage || this.AnotherRole.UseSabotage;
            this.CanUseAdmin = this.CanUseAdmin || this.AnotherRole.CanUseAdmin;
            this.CanUseSecurity = this.CanUseSecurity || this.AnotherRole.CanUseSecurity;
            this.CanUseVital = this.CanUseVital || this.AnotherRole.CanUseVital;

            this.HasOtherVison = this.HasOtherVison || this.AnotherRole.HasOtherVison;
            
            this.IsBoost = this.IsBoost || this.AnotherRole.IsBoost;

            if (this.HasOtherVison)
            {
                this.IsApplyEnvironmentVision = this.IsApplyEnvironmentVision || this.AnotherRole.IsApplyEnvironmentVision;
                this.Vison = this.Vison > this.AnotherRole.Vison ? this.Vison : this.AnotherRole.Vison;
            }

            if (this.CanKill)
            {
                this.HasOtherKillCool = this.HasOtherKillCool || this.AnotherRole.HasOtherKillCool;
                this.HasOtherKillRange = this.HasOtherKillRange || this.AnotherRole.HasOtherKillRange;
                if (this.HasOtherKillCool)
                {
                    this.KillCoolTime = 
                        this.KillCoolTime < this.AnotherRole.KillCoolTime ?
                           this.KillCoolTime : this.AnotherRole.KillCoolTime;
                }
                if (this.HasOtherKillRange)
                {
                    this.KillRange = this.KillRange > this.AnotherRole.KillRange ?
                           this.KillRange : this.AnotherRole.KillRange;
                }
            }

            if (this.IsBoost)
            {
                this.MoveSpeed = this.MoveSpeed > this.AnotherRole.MoveSpeed ?
                    this.MoveSpeed : this.AnotherRole.MoveSpeed;
            }

        }
        public int GetManagerOptionId<T>(T option) where T : struct, IConvertible
        {
            EnumCheck(option);

            return GetManagerOptionId(Convert.ToInt32(option));
        }

        public int GetManagerOptionId(int option) => this.ManagerOptionOffset + option;

        public void SetManagerOptionOffset(int offset)
        {
            this.ManagerOptionOffset = offset;
        }

        public int GetManagerOptionOffset() => this.ManagerOptionOffset;
    }
}
