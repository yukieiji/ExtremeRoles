using System.Collections.Generic;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;

using ExtremeRoles.Performance;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;


using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.GameResult;

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Survivor : SingleRoleBase, IRoleAwake<RoleTypes>, IRoleWinPlayerModifier
{
    public override bool IsAssignGhostRole
    {
        get => this.isDeadWin || this.isNoWinSurvivorAssignGhostRole;
    }

    public bool IsAwake
    {
        get
        {
            return GameSystem.IsLobby || this.awakeRole;
        }
    }

    public RoleTypes NoneAwakeRole => RoleTypes.Crewmate;

    public enum SurvivorOption
    {
        AwakeTaskGage,
        DeadWinTaskGage,
        NoWinSurvivorAssignGhostRole
    }

    private bool awakeRole;
    private float awakeTaskGage;
    private bool awakeHasOtherVision;

    private bool isDeadWin;
    private float deadWinTaskGage;

    private bool isNoWinSurvivorAssignGhostRole;

    public Survivor() : base(
        ExtremeRoleId.Survivor,
        ExtremeRoleType.Crewmate,
        ExtremeRoleId.Survivor.ToString(),
        ColorPalette.SurvivorYellow,
        false, true, false, false)
    { }

    public static void DeadWin(byte rolePlayerId)
    {
        Survivor survivor = ExtremeRoleManager.GetSafeCastedRole<Survivor>(rolePlayerId);
        if (survivor != null)
        {
            survivor.isDeadWin = true;
        }
    }

    public string GetFakeOptionString() => "";

    public void ModifiedWinPlayer(
        NetworkedPlayerInfo rolePlayerInfo,
        GameOverReason reason,
		in WinnerTempData winner)
    {

        if (!rolePlayerInfo.IsDead || this.isDeadWin) { return; }

        switch (reason)
        {
            case GameOverReason.HumansByTask:
            case GameOverReason.HumansByVote:
            case GameOverReason.HumansDisconnect:
            case GameOverReason.HideAndSeek_ByTimer:
                winner.RemoveAll(rolePlayerInfo);
                break;
            default:
                break;
        }
    }
    public void Update(PlayerControl rolePlayer)
    {
        if ((!this.awakeRole || !this.isDeadWin) &&
            rolePlayer.myTasks.Count != 0)
        {
            float taskGage = Player.GetPlayerTaskGage(rolePlayer);

            if (!this.isDeadWin &&
                !rolePlayer.Data.IsDead &&
                taskGage >= this.deadWinTaskGage)
            {
                this.isDeadWin = true;

                using (var caller = RPCOperator.CreateCaller(
                    RPCOperator.Command.SurvivorDeadWin))
                {
                    caller.WriteByte(rolePlayer.PlayerId);
                }
                DeadWin(rolePlayer.PlayerId);
            }

            if (taskGage >= this.awakeTaskGage && !this.awakeRole)
            {
                this.awakeRole = true;
                this.HasOtherVision = this.awakeHasOtherVision;
            }
        }
    }

    public override string GetColoredRoleName(bool isTruthColor = false)
    {
        if (isTruthColor || IsAwake)
        {
            return base.GetColoredRoleName();
        }
        else
        {
            return Design.ColoedString(
                Palette.White, Tr.GetString(RoleTypes.Crewmate.ToString()));
        }
    }
    public override string GetFullDescription()
    {
        if (IsAwake)
        {
            return Tr.GetString(
                $"{this.Id}FullDescription");
        }
        else
        {
            return Tr.GetString(
                $"{RoleTypes.Crewmate}FullDescription");
        }
    }

    public override string GetImportantText(bool isContainFakeTask = true)
    {
        if (IsAwake)
        {
            return base.GetImportantText(isContainFakeTask);

        }
        else
        {
            return Design.ColoedString(
                Palette.White,
                $"{this.GetColoredRoleName()}: {Tr.GetString("crewImportantText")}");
        }
    }

    public override string GetIntroDescription()
    {
        if (IsAwake)
        {
            return base.GetIntroDescription();
        }
        else
        {
            return Design.ColoedString(
                Palette.CrewmateBlue,
                PlayerControl.LocalPlayer.Data.Role.Blurb);
        }
    }

    public override Color GetNameColor(bool isTruthColor = false)
    {
        if (isTruthColor || IsAwake)
        {
            return base.GetNameColor(isTruthColor);
        }
        else
        {
            return Palette.White;
        }
    }

    public override void ExiledAction(
        PlayerControl rolePlayer)
    {
        updateTaskDo();
    }

    public override void RolePlayerKilledAction(
        PlayerControl rolePlayer,
        PlayerControl killerPlayer)
    {
        updateTaskDo();
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateIntOption(
            SurvivorOption.AwakeTaskGage,
            70, 0, 100, 10,
            format: OptionUnit.Percentage);
        factory.CreateIntOption(
            SurvivorOption.DeadWinTaskGage,
            100, 50, 100, 10,
            format: OptionUnit.Percentage);
        factory.CreateBoolOption(
            SurvivorOption.NoWinSurvivorAssignGhostRole,
            true);
    }

    protected override void RoleSpecificInit()
    {
		var loader = this.Loader;
        this.awakeTaskGage = loader.GetValue<SurvivorOption, int>(
            SurvivorOption.AwakeTaskGage) / 100.0f;
        this.deadWinTaskGage = loader.GetValue<SurvivorOption, int>(
            SurvivorOption.DeadWinTaskGage) / 100.0f;
        this.isNoWinSurvivorAssignGhostRole = loader.GetValue<SurvivorOption, bool>(
            SurvivorOption.NoWinSurvivorAssignGhostRole);

        this.awakeHasOtherVision = this.HasOtherVision;
        this.isDeadWin = false;

        if (this.awakeTaskGage <= 0.0f)
        {
            this.awakeRole = true;
            this.HasOtherVision = this.awakeHasOtherVision;
        }
        else
        {
            this.awakeRole = false;
            this.HasOtherVision = false;
        }
    }

    private void updateTaskDo()
    {
        if (!this.isDeadWin)
        {
            this.HasTask = false;
        }
    }
}
