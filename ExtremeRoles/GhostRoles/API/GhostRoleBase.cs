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


namespace ExtremeRoles.GhostRoles.API
{
    public abstract class GhostRoleBase : IGhostRole
    {
        public ExtremeRoleType Team => this.Team;

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

        public bool IsCrewmate() => this.Team == ExtremeRoleType.Crewmate;

        public bool IsImpostor() => this.Team == ExtremeRoleType.Impostor;

        public bool IsNeutral() => this.Team == ExtremeRoleType.Neutral;

        public bool IsVanillaRole() => this.RoleId == ExtremeGhostRoleId.VanillaRole;

        public virtual string GetImportantText() => 
            Design.ColoedString(
                this.NameColor,
                string.Format("{0}: {1}",
                    Design.ColoedString(
                        this.NameColor,
                        Translation.GetString(this.RoleName)),
                    Translation.GetString(
                        $"{this.Id}ShortDescription")));
        
        public virtual string GetFullDescription() => Translation.GetString(
           $"{this.Id}FullDescription");

        public virtual string GetColoredRoleName(bool isTruthName = false) => Design.ColoedString(
            this.NameColor, Translation.GetString(this.RoleName));

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

        public abstract HashSet<ExtremeGhostRoleId> GetRoleFilter();

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
