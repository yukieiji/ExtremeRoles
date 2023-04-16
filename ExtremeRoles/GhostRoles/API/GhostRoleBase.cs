using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.AbilityBehavior;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.GhostRoles.API;

public enum GhostRoleOption
{
    IsReportAbility = 40 
}

public abstract class GhostRoleBase
{
    private const float defaultCoolTime = 60.0f;
    private const float minCoolTime = 5.0f;
    private const float maxCoolTime = 120.0f;
    private const float minActiveTime = 0.5f;
    private const float maxActiveTime = 30.0f;
    private const float step = 0.5f;

    public ExtremeRoleType Team => this.TeamType;

    public ExtremeGhostRoleId Id => this.RoleId;

    public int OptionOffset => this.OptionIdOffset;

    public string Name => this.RoleName;

    public Module.ExtremeAbilityButton Button { get; protected set; }

    public Color RoleColor => this.NameColor;
    public bool HasTask => this.Task;

    public int GameControlId => this.controlId;

    protected ExtremeRoleType TeamType;
    protected ExtremeGhostRoleId RoleId;
    protected string RoleName;
    protected Color NameColor;
    protected int OptionIdOffset;

    protected bool Task;

    private OptionTab tab = OptionTab.General;
    private int controlId;

    public GhostRoleBase(
        bool hasTask,
        ExtremeRoleType team,
        ExtremeGhostRoleId id,
        string roleName,
        Color color,
        OptionTab tab = OptionTab.General)
    {
        this.Task = hasTask;
        this.TeamType = team;
        this.RoleId = id;
        this.RoleName = roleName;
        this.NameColor = color;
        
        if (tab == OptionTab.General)
        {
            switch (team)
            {
                case ExtremeRoleType.Crewmate:
                    this.tab = OptionTab.GhostCrewmate;
                    break;
                case ExtremeRoleType.Impostor:
                    this.tab = OptionTab.GhostImpostor;
                    break;
                case ExtremeRoleType.Neutral:
                    this.tab = OptionTab.GhostNeutral;
                    break;
            }
        }
        else
        {
            this.tab = tab;
        }
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
        IOptionInfo parentOps, int optionIdOffset)
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

    public void SetGameControlId(int newId)
    {
        this.controlId = newId;
    }

    public void ResetOnMeetingEnd()
    {
        if (this.Button != null)
        {
            this.Button.OnMeetingEnd();
        }
        this.OnMeetingEndHook();
    }

    public void ResetOnMeetingStart()
    {
        if (this.Button != null)
        {
            this.Button.OnMeetingStart();
        }
        this.OnMeetingStartHook();
    }

    protected void CreateButtonOption(
        IOptionInfo parentOps,
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

        CreateBoolOption(
           GhostRoleOption.IsReportAbility,
           true, parentOps);
    }

    protected void CreateCountButtonOption(
        IOptionInfo parentOps,
        int defaultAbilityCount,
        int maxAbilityCount,
        float defaultActiveTime = float.MaxValue)
    {
        CreateButtonOption(
            parentOps, defaultActiveTime);

        CreateIntOption(
            RoleAbilityCommonOption.AbilityCount,
            defaultAbilityCount, 1,
            maxAbilityCount, 1,
            parentOps, format: OptionUnit.Shot);
    }

    protected void ButtonInit()
    {
        if (this.Button == null) { return; }

        var allOps = AllOptionHolder.Instance;
        this.Button.Behavior.SetCoolTime(
            allOps.GetValue<float>(this.GetRoleOptionId(RoleAbilityCommonOption.AbilityCoolTime)));

        if (allOps.TryGet<float>(
                this.GetRoleOptionId(
                    RoleAbilityCommonOption.AbilityActiveTime), out var activeTimeOtion))
        {
            this.Button.Behavior.SetActiveTime(activeTimeOtion.GetValue());
        }

        if (this.Button.Behavior is AbilityCountBehavior behavior &&
            allOps.TryGet<int>(
                this.GetRoleOptionId(
                    RoleAbilityCommonOption.AbilityCount),
                out var countOption))
        {
            behavior.SetAbilityCount(countOption.GetValue());
        }
        this.Button.OnMeetingEnd();
    }

    protected bool isReportAbility() => OptionHolder.AllOption[
        this.GetRoleOptionId(GhostRoleOption.IsReportAbility)].GetValue();

    protected bool IsCommonUse() => 
        PlayerControl.LocalPlayer && 
        PlayerControl.LocalPlayer.Data.IsDead && 
        PlayerControl.LocalPlayer.CanMove;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected FloatCustomOption CreateFloatOption<T>(
        T option,
        float defaultValue,
        float min, float max, float step,
        IOptionInfo parent = null,
        bool isHeader = false,
        bool isHidden = false,
        OptionUnit format = OptionUnit.None,
        bool invert = false,
        IOptionInfo enableCheckOption = null,
        bool colored = false) where T : struct, IConvertible
    {
        EnumCheck(option);

        return new FloatCustomOption(
            GetRoleOptionId(option),
            createAutoOptionString(option, colored),
            defaultValue,
            min, max, step,
            parent, isHeader, isHidden,
            format, invert, enableCheckOption,
            tab: this.tab);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected FloatDynamicCustomOption CreateFloatDynamicOption<T>(
        T option,
        float defaultValue,
        float min, float step,
        IOptionInfo parent = null,
        bool isHeader = false,
        bool isHidden = false,
        OptionUnit format = OptionUnit.None,
        bool invert = false,
        IOptionInfo enableCheckOption = null,
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
        IOptionInfo parent = null,
        bool isHeader = false,
        bool isHidden = false,
        OptionUnit format = OptionUnit.None,
        bool invert = false,
        IOptionInfo enableCheckOption = null,
        bool colored = false) where T : struct, IConvertible
    {
        EnumCheck(option);

        return new IntCustomOption(
            GetRoleOptionId(option),
            createAutoOptionString(option, colored),
            defaultValue,
            min, max, step,
            parent, isHeader, isHidden,
            format, invert, enableCheckOption,
            tab: this.tab);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected IntDynamicCustomOption CreateIntDynamicOption<T>(
        T option,
        int defaultValue,
        int min, int step,
        IOptionInfo parent = null,
        bool isHeader = false,
        bool isHidden = false,
        OptionUnit format = OptionUnit.None,
        bool invert = false,
        IOptionInfo enableCheckOption = null,
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
        IOptionInfo parent = null,
        bool isHeader = false,
        bool isHidden = false,
        OptionUnit format = OptionUnit.None,
        bool invert = false,
        IOptionInfo enableCheckOption = null,
        bool colored = false) where T : struct, IConvertible
    {
        EnumCheck(option);

        return new BoolCustomOption(
            GetRoleOptionId(option),
            createAutoOptionString(option, colored),
            defaultValue,
            parent, isHeader, isHidden,
            format, invert, enableCheckOption,
            tab: this.tab);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected SelectionCustomOption CreateSelectionOption<T>(
        T option,
        string[] selections,
        IOptionInfo parent = null,
        bool isHeader = false,
        bool isHidden = false,
        OptionUnit format = OptionUnit.None,
        bool invert = false,
        IOptionInfo enableCheckOption = null,
        bool colored = false) where T : struct, IConvertible
    {
        EnumCheck(option);

        return new SelectionCustomOption(
            GetRoleOptionId(option),
            createAutoOptionString(option, colored),
            selections,
            parent, isHeader, isHidden,
            format, invert, enableCheckOption,
            tab: this.tab);
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

    private IOptionInfo createSpawnOption()
    {
        var roleSetOption = CreateSelectionOption(
            RoleCommonOption.SpawnRate,
            OptionHolder.SpawnRate, null, true,
            colored: true);

        int spawnNum = this.IsImpostor() ? GameSystem.MaxImposterNum : GameSystem.VanillaMaxPlayerNum - 1;

        CreateIntOption(
            RoleCommonOption.RoleNum,
            1, 1, spawnNum, 1, roleSetOption);

        return roleSetOption;
    }

    public abstract void CreateAbility();

    public abstract HashSet<Roles.ExtremeRoleId> GetRoleFilter();

    public abstract void Initialize();

    protected abstract void OnMeetingEndHook();

    protected abstract void OnMeetingStartHook();

    protected abstract void CreateSpecificOption(IOptionInfo parentOps);

    protected abstract void UseAbility(RPCOperator.RpcCaller caller);

    protected static void EnumCheck<T>(T isEnum) where T : struct, IConvertible
    {
        if (!typeof(int).IsAssignableFrom(Enum.GetUnderlyingType(typeof(T))))
        {
            throw new ArgumentException(nameof(T));
        }
    }
}
