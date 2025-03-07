﻿using System;

using UnityEngine;

using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.Roles.API;

public abstract partial class SingleRoleBase : RoleOptionBase
{
    public virtual bool IsAssignGhostRole => true;

    public OptionTab Tab { get; } = OptionTab.GeneralTab;
    public virtual string RoleName => this.RawRoleName;

    public bool CanCallMeeting = true;
    public bool CanRepairSabotage = true;

    public bool CanUseAdmin = true;
    public bool CanUseSecurity = true;
    public bool CanUseVital = true;

    public bool HasTask = true;
    public bool UseVent = false;
    public bool UseSabotage = false;
    public bool HasOtherVision = false;
    public bool HasOtherKillCool = false;
    public bool HasOtherKillRange = false;
    public bool IsApplyEnvironmentVision = true;
    public bool IsWin = false;

    public bool FakeImposter = false;
    public bool IsBoost = false;
    public float MoveSpeed = 1.0f;

    public float Vision = 0f;
    public float KillCoolTime = 0f;
    public int KillRange = 1;

    public readonly ExtremeRoleId Id;
	public ExtremeRoleType Team;

    protected Color NameColor;

    public readonly string RawRoleName;

	public override IOptionLoader Loader
	{
		get
		{
			if (!OptionManager.Instance.TryGetCategory(
					this.Tab,
					ExtremeRoleManager.GetRoleGroupId(this.Id),
					out var cate))
			{
				throw new ArgumentException("Can't find category");
			}
			return cate;
		}
	}

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
        OptionTab tab = OptionTab.GeneralTab)
    {
        this.Id = id;
        this.Team = team;
        this.RawRoleName = roleName;
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

        if (tab == OptionTab.GeneralTab)
        {
            switch (this.Team)
            {
                case ExtremeRoleType.Crewmate:
                    this.Tab = OptionTab.CrewmateTab;
                    break;
                case ExtremeRoleType.Impostor:
                    this.Tab = OptionTab.ImpostorTab;
                    break;
                case ExtremeRoleType.Neutral:
                    this.Tab = OptionTab.NeutralTab;
                    break;
                default:
                    this.Tab = OptionTab.GeneralTab;
                    break;
            }
        }
        else
        {
            this.Tab = tab;
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

    public virtual bool IsTeamsWin() => this.IsWin;

    protected override void CommonInit()
    {
        var baseOption = GameOptionsManager.Instance.CurrentGameOptions;

		var loader = this.Loader;

        this.Vision = this.IsImpostor() ?
            baseOption.GetFloat(FloatOptionNames.ImpostorLightMod) :
            baseOption.GetFloat(FloatOptionNames.CrewLightMod);

        this.KillCoolTime = Player.DefaultKillCoolTime;
        this.KillRange = baseOption.GetInt(Int32OptionNames.KillDistance);

        this.IsApplyEnvironmentVision = !this.IsImpostor();


        this.HasOtherVision = loader.GetValue<RoleCommonOption, bool>(
			RoleCommonOption.HasOtherVision);
        if (this.HasOtherVision)
        {
            this.Vision = loader.GetValue<RoleCommonOption, float>(
				RoleCommonOption.Vision);
            this.IsApplyEnvironmentVision = loader.GetValue<RoleCommonOption, bool>(
                RoleCommonOption.ApplyEnvironmentVisionEffect);
        }

        if (this.CanKill)
        {
            this.HasOtherKillCool = loader.GetValue<KillerCommonOption, bool>(
                KillerCommonOption.HasOtherKillCool);
            if (this.HasOtherKillCool)
            {
                this.KillCoolTime = loader.GetValue<KillerCommonOption, float>(
                    KillerCommonOption.KillCoolDown);
            }

            this.HasOtherKillRange = loader.GetValue<KillerCommonOption, bool>(
				KillerCommonOption.HasOtherKillRange);

            if (this.HasOtherKillRange)
            {
                this.KillRange = loader.GetValue<KillerCommonOption, int>(
					KillerCommonOption.KillRange);
            }
        }
    }
}
