using System.Collections.Generic;
using System.Linq;

using ExtremeRoles.Module;

using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Extension.Neutral;
using ExtremeRoles.Performance;

using ExtremeRoles.Module.NewOption.Factory;

namespace ExtremeRoles.Roles.Solo.Neutral;

public sealed class TaskMaster : SingleRoleBase, IRoleSpecialSetUp, IRoleUpdate
{
    public enum TaskMasterOption
    {
        CanUseSabotage,
        AddCommonTaskNum,
        AddLongTaskNum,
        AddNormalTaskNum,
    }

    private int addLongTask = 0;
    private int addNormalTask = 0;
    private int addCommonTask = 0;
    private List<int> addTask;

    public TaskMaster() : base(
        ExtremeRoleId.TaskMaster,
        ExtremeRoleType.Neutral,
        ExtremeRoleId.TaskMaster.ToString(),
        ColorPalette.NeutralColor,
        false, true, true, true)
    { }

    public void Update(PlayerControl rolePlayer)
    {
        if (CachedShipStatus.Instance == null ||
            this.IsWin ||
            GameData.Instance == null) { return; }

        if (!CachedShipStatus.Instance.enabled) { return; }

        var playerInfo = GameData.Instance.GetPlayerById(
            rolePlayer.PlayerId);
        if (playerInfo.IsDead ||
            playerInfo.Disconnected ||
            playerInfo.Tasks.Count == 0) { return; }

        int compCount = 0;

        for (int i = 0; i < playerInfo.Tasks.Count; ++i)
        {
            if (playerInfo.Tasks[i].Complete)
            {
                if (this.addTask.Count == 0)
                {
                    ++compCount;
                }
                else
                {
                    var shuffled = this.addTask.OrderBy(
                        item => RandomGenerator.Instance.Next()).ToList();
                    int taskIndex = shuffled[0];

                    Logging.Debug($"SetTaskId:{taskIndex}");

                    this.addTask.Remove(taskIndex);
                    GameSystem.RpcReplaceNewTask(rolePlayer.PlayerId, i, taskIndex);
                    break;
                }
            }
        }
        if (compCount == playerInfo.Tasks.Count)
        {
            ExtremeRolesPlugin.ShipState.RpcRoleIsWin(rolePlayer.PlayerId);
            this.IsWin = true;
        }
    }

    public void IntroBeginSetUp()
    {
        return;
    }

    public void IntroEndSetUp()
    {
        for (int i = 0; i < this.addLongTask; ++i)
        {
            this.addTask.Add(GameSystem.GetRandomLongTask());
        }
        for (int i = 0; i < this.addCommonTask; ++i)
        {
            this.addTask.Add(GameSystem.GetRandomCommonTaskId());
        }
        for (int i = 0; i < this.addNormalTask; ++i)
        {
            this.addTask.Add(GameSystem.GetRandomShortTaskId());
        }
    }

    public override bool IsSameTeam(SingleRoleBase targetRole) =>
        this.IsNeutralSameTeam(targetRole);

    public override void ExiledAction(PlayerControl rolePlayer)
    {
        resetTask(rolePlayer.PlayerId);
    }

    public override void RolePlayerKilledAction(
        PlayerControl rolePlayer, PlayerControl killerPlayer)
    {
        resetTask(rolePlayer.PlayerId);
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateBoolOption(
            TaskMasterOption.CanUseSabotage,
            true);
        factory.CreateIntOption(
            TaskMasterOption.AddCommonTaskNum,
            1, 0, 15, 1);
        factory.CreateIntOption(
            TaskMasterOption.AddLongTaskNum,
            1, 0, 15, 1);
        factory.CreateIntOption(
            TaskMasterOption.AddNormalTaskNum,
            1, 0, 15, 1);
    }

    protected override void RoleSpecificInit()
    {
        var cate = this.Loader;
        this.UseSabotage = cate.GetValue<TaskMasterOption, bool>(
            TaskMasterOption.CanUseSabotage);
        this.addLongTask = cate.GetValue<TaskMasterOption, int>(
            TaskMasterOption.AddLongTaskNum);
        this.addNormalTask = cate.GetValue<TaskMasterOption, int>(
            TaskMasterOption.AddNormalTaskNum);
        this.addCommonTask = cate.GetValue<TaskMasterOption, int>(
            TaskMasterOption.AddCommonTaskNum);
        this.addTask = new List<int>();
    }
    private void resetTask(byte playerId)
    {
        this.HasTask = this.IsWin;
        PlayerControl rolePlayer = CachedPlayerControl.LocalPlayer;
        if (rolePlayer.PlayerId == playerId && !this.HasTask)
        {
            rolePlayer.ClearTasks();
        }
    }
}
