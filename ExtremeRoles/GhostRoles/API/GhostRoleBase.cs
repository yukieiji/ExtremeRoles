using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Hazel;
using UnityEngine;

using ExtremeRoles.GhostRoles.API.Interface;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.GhostRoles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;


namespace ExtremeRoles.GhostRoles.API
{
    public abstract class GhostRoleBase : IGhostRole
    {

        private const float defaultCoolTime = 60.0f;
        private const float minCoolTime = 15.0f;
        private const float maxCoolTime = 120.0f;
        private const float minActiveTime = 0.5f;
        private const float maxActiveTime = 30.0f;
        private const float step = 0.5f;

        public ExtremeRoleType Team => this.TeamType;

        public ExtremeGhostRoleId Id => this.RoleId;

        public int OptionOffset => this.OptionIdOffset;

        public string Name => this.RoleName;

        public GhostRoleAbilityButtonBase Button
        {
            get => this.AbilityButton;
            set
            {
                this.AbilityButton = value;
            }
        }

        public Color RoleColor => this.NameColor;
        public bool HasTask => this.Task;

        protected ExtremeRoleType TeamType;
        protected ExtremeGhostRoleId RoleId;
        protected string RoleName;
        protected Color NameColor;
        protected int OptionIdOffset;
        protected GhostRoleAbilityButtonBase AbilityButton;

        protected bool Task;

        public GhostRoleBase(
            bool hasTask,
            ExtremeRoleType team,
            ExtremeGhostRoleId id,
            string roleName,
            Color color)
        {
            this.Task = hasTask;
            this.TeamType = team;
            this.RoleId = id;
            this.RoleName = roleName;
            this.NameColor = color;
        }

        public virtual GhostRoleBase Clone()
        {
            GhostRoleBase copy = (GhostRoleBase)this.MemberwiseClone();
            Color baseColor = this.NameColor;

            copy.NameColor = new Color(
                baseColor.r,
                baseColor.g,
                baseColor.b,
                baseColor.a);

            return copy;
        }

        public void CreateRoleAllOption(int optionIdOffset)
        {
            this.OptionIdOffset = optionIdOffset;
            var parentOps = createSpawnOption();
            CreateSpecificOption(parentOps);
        }

        public void CreateRoleSpecificOption(
            CustomOptionBase parentOps, int optionIdOffset)
        {
            this.OptionIdOffset = optionIdOffset;
            CreateSpecificOption(parentOps);
        }

        public int GetRoleOptionId<T>(T option) where T : struct, IConvertible
        {
            EnumCheck(option);
            return GetRoleOptionId(Convert.ToInt32(option));
        }

        public int GetRoleOptionId(int option) => this.OptionIdOffset + option;

        public bool IsCrewmate() => this.TeamType == ExtremeRoleType.Crewmate;

        public bool IsImpostor() => this.TeamType == ExtremeRoleType.Impostor;

        public bool IsNeutral() => this.TeamType == ExtremeRoleType.Neutral;

        public bool IsVanillaRole() => this.RoleId == ExtremeGhostRoleId.VanillaRole;

        public virtual string GetColoredRoleName() => Design.ColoedString(
            this.NameColor, Translation.GetString(this.RoleName));

        public virtual string GetFullDescription() => Translation.GetString(
           $"{this.Id}FullDescription");

        public virtual string GetImportantText() => 
            Design.ColoedString(
                this.NameColor,
                string.Format("{0}: {1}",
                    Design.ColoedString(
                        this.NameColor,
                        Translation.GetString(this.RoleName)),
                    Translation.GetString(
                        $"{this.Id}ShortDescription")));

        public virtual Color GetTargetRoleSeeColor(
            byte targetPlayerId, SingleRoleBase targetRole, GhostRoleBase targetGhostRole)
        {
            var overLoader = targetRole as Roles.Solo.Impostor.OverLoader;

            if (overLoader != null)
            {
                if (overLoader.IsOverLoad)
                {
                    return Palette.ImpostorRed;
                }
            }

            bool isGhostRoleImpostor = false;
            if (targetGhostRole != null)
            {
                isGhostRoleImpostor = targetGhostRole.IsImpostor();
            }

            if ((targetRole.IsImpostor() || targetRole.FakeImposter || isGhostRoleImpostor) &&
                this.IsImpostor())
            {
                return Palette.ImpostorRed;
            }

            return Color.clear;
        }

        protected void CreateButtonOption(
            CustomOptionBase parentOps,
            float defaultActiveTime = float.MaxValue)
        {

            CreateFloatOption(
                RoleAbilityCommonOption.AbilityCoolTime,
                defaultCoolTime, minCoolTime,
                maxCoolTime, step,
                parentOps, format: OptionUnit.Second);

            if (defaultActiveTime != float.MaxValue)
            {
                defaultActiveTime = Mathf.Clamp(
                    defaultActiveTime, minActiveTime, maxActiveTime);

                CreateFloatOption(
                    RoleAbilityCommonOption.AbilityActiveTime,
                    defaultActiveTime, minActiveTime, maxActiveTime, step,
                    parentOps, format: OptionUnit.Second);
            }

        }

        protected void ButtonInit()
        {
            if (this.Button == null) { return; }

            var allOps = OptionHolder.AllOption;
            this.Button.SetAbilityCoolTime(
                allOps[this.GetRoleOptionId(RoleAbilityCommonOption.AbilityCoolTime)].GetValue());

            int checkOptionId = this.GetRoleOptionId(RoleAbilityCommonOption.AbilityActiveTime);

            if (allOps.ContainsKey(checkOptionId))
            {
                this.Button.SetAbilityActiveTime(
                    allOps[checkOptionId].GetValue());
            }

            this.Button.ResetCoolTimer();
        }

        protected bool IsCommonUse() => 
            PlayerControl.LocalPlayer && 
            PlayerControl.LocalPlayer.Data.IsDead && 
            PlayerControl.LocalPlayer.CanMove;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected CustomOptionBase CreateFloatOption<T>(
            T option,
            float defaultValue,
            float min, float max, float step,
            CustomOptionBase parent = null,
            bool isHeader = false,
            bool isHidden = false,
            OptionUnit format = OptionUnit.None,
            bool invert = false,
            CustomOptionBase enableCheckOption = null,
            bool colored = false) where T : struct, IConvertible
        {
            EnumCheck(option);

            return new FloatCustomOption(
                GetRoleOptionId(option),
                createAutoOptionString(option, colored),
                defaultValue,
                min, max, step,
                parent, isHeader, isHidden,
                format, invert, enableCheckOption);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected CustomOptionBase CreateFloatDynamicOption<T>(
            T option,
            float defaultValue,
            float min, float step,
            CustomOptionBase parent = null,
            bool isHeader = false,
            bool isHidden = false,
            OptionUnit format = OptionUnit.None,
            bool invert = false,
            CustomOptionBase enableCheckOption = null,
            bool colored = false) where T : struct, IConvertible
        {
            EnumCheck(option);

            return new FloatDynamicCustomOption(
                GetRoleOptionId(option),
                createAutoOptionString(option, colored),
                defaultValue,
                min, step,
                parent, isHeader, isHidden,
                format, invert, enableCheckOption);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected CustomOptionBase CreateIntOption<T>(
            T option,
            int defaultValue,
            int min, int max, int step,
            CustomOptionBase parent = null,
            bool isHeader = false,
            bool isHidden = false,
            OptionUnit format = OptionUnit.None,
            bool invert = false,
            CustomOptionBase enableCheckOption = null,
            bool colored = false) where T : struct, IConvertible
        {
            EnumCheck(option);

            return new IntCustomOption(
                GetRoleOptionId(option),
                createAutoOptionString(option, colored),
                defaultValue,
                min, max, step,
                parent, isHeader, isHidden,
                format, invert, enableCheckOption);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected CustomOptionBase CreateIntDynamicOption<T>(
            T option,
            int defaultValue,
            int min, int step,
            CustomOptionBase parent = null,
            bool isHeader = false,
            bool isHidden = false,
            OptionUnit format = OptionUnit.None,
            bool invert = false,
            CustomOptionBase enableCheckOption = null,
            bool colored = false) where T : struct, IConvertible
        {
            EnumCheck(option);

            return new IntDynamicCustomOption(
                GetRoleOptionId(option),
                createAutoOptionString(option, colored),
                defaultValue,
                min, step,
                parent, isHeader, isHidden,
                format, invert, enableCheckOption);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected CustomOptionBase CreateBoolOption<T>(
            T option,
            bool defaultValue,
            CustomOptionBase parent = null,
            bool isHeader = false,
            bool isHidden = false,
            OptionUnit format = OptionUnit.None,
            bool invert = false,
            CustomOptionBase enableCheckOption = null,
            bool colored = false) where T : struct, IConvertible
        {
            EnumCheck(option);

            return new BoolCustomOption(
                GetRoleOptionId(option),
                createAutoOptionString(option, colored),
                defaultValue,
                parent, isHeader, isHidden,
                format, invert, enableCheckOption);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected CustomOptionBase CreateSelectionOption<T>(
            T option,
            string[] selections,
            CustomOptionBase parent = null,
            bool isHeader = false,
            bool isHidden = false,
            OptionUnit format = OptionUnit.None,
            bool invert = false,
            CustomOptionBase enableCheckOption = null,
            bool colored = false) where T : struct, IConvertible
        {
            EnumCheck(option);

            return new SelectionCustomOption(
                GetRoleOptionId(option),
                createAutoOptionString(option, colored),
                selections,
                parent, isHeader, isHidden,
                format, invert, enableCheckOption);
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

        private CustomOptionBase createSpawnOption()
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

        public abstract void CreateAbility();

        public abstract HashSet<Roles.ExtremeRoleId> GetRoleFilter();

        public abstract void Initialize();

        public abstract void ReseOnMeetingEnd();

        public abstract void ReseOnMeetingStart();

        protected abstract void CreateSpecificOption(CustomOptionBase parentOps);

        protected abstract void UseAbility(MessageWriter writer);

        protected static void EnumCheck<T>(T isEnum) where T : struct, IConvertible
        {
            if (!typeof(int).IsAssignableFrom(Enum.GetUnderlyingType(typeof(T))))
            {
                throw new ArgumentException(nameof(T));
            }
        }
    }
}
