using System.Linq;
using System.Collections.Generic;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Agency : SingleRoleBase, IRoleAbility, IRoleUpdate
{
    public enum AgencyOption
    {
        MaxTaskNum,
        TakeTaskRange
    }

    public enum TakeTaskType
    {
        Normal,
        Long,
        Common
    }

    public ExtremeAbilityButton Button
    {
        get => this.takeTaskButton;
        set
        {
            this.takeTaskButton = value;
        }
    }

    public byte TargetPlayer = byte.MaxValue;
    public List<TakeTaskType> TakeTask;

    private int maxTakeTask;
    private float takeTaskRange;
    private ExtremeAbilityButton takeTaskButton;

    public Agency() : base(
        ExtremeRoleId.Agency,
        ExtremeRoleType.Crewmate,
        ExtremeRoleId.Agency.ToString(),
        ColorPalette.AgencyYellowGreen,
        false, true, false, false)
    { }

    public static void TakeTargetPlayerTask(
        byte targetPlayerId, List<int> removeTaskId)
    {

        PlayerControl targetPlayer = Player.GetPlayerControlById(
            targetPlayerId);

        foreach (PlayerTask task in targetPlayer.myTasks.GetFastEnumerator())
        {
            if (task == null) { continue; }

            var textTask = task.gameObject.GetComponent<ImportantTextTask>();
            if (textTask != null) { continue; }
            if (GameSystem.SaboTask.Contains(task.TaskType))  { continue; }

            if (removeTaskId.Contains((int)task.Id))
            {
                targetPlayer.CompleteTask(task.Id);
                task.OnRemove();
            }
        }

        if (targetPlayerId == CachedPlayerControl.LocalPlayer.PlayerId)
        {
            Sound.PlaySound(
                Sound.SoundType.AgencyTakeTask, 1.2f);
        }

        GameData.Instance.SetDirtyBit(
            1U << (int)targetPlayer.PlayerId);
    }


    public void CreateAbility()
    {
        this.CreateAbilityCountButton(
            "takeTask",
            Loader.CreateSpriteFromResources(
                Path.AgencyTakeTask));
        this.Button.SetLabelToCrewmate();
    }

    public bool UseAbility()
    {

        var targetRole = ExtremeRoleManager.GameRole[this.TargetPlayer];

        int takeNum = UnityEngine.Random.RandomRange(1, this.maxTakeTask);
        
        if (!targetRole.HasTask())
        {
            int totakTaskNum = GameData.Instance.TotalTasks;
            int compTaskNum = GameData.Instance.CompletedTasks;

            float taskGauge = (float)compTaskNum / (float)totakTaskNum;
            if (taskGauge > 0.9f)
            {
                takeNum = 0;
            }
            else if (0.75 < taskGauge && taskGauge <= 0.9f)
            {
                takeNum = UnityEngine.Random.RandomRange(0, 2);
            }
            else if (0.5 < taskGauge && taskGauge <= 0.75f)
            {
                takeNum = UnityEngine.Random.RandomRange(0, this.maxTakeTask);
            }
        }


        if (takeNum == 0) { return true; }

        byte playerId = PlayerControl.LocalPlayer.PlayerId;

        GameData.PlayerInfo targetPlayerInfo = GameData.Instance.GetPlayerById(
            this.TargetPlayer);

        var shuffleTaskIndex = Enumerable.Range(
            0, targetPlayerInfo.Tasks.Count).ToList().OrderBy(
                item => RandomGenerator.Instance.Next()).ToList();
        int takeTask = 0;
        List<int> getTaskId = new List<int>();

        foreach (int i in shuffleTaskIndex)
        {
            if (takeTask >= takeNum) { break; }

            if (targetPlayerInfo.Tasks[i].Complete) { continue; }

            int taskId = (int)targetPlayerInfo.Tasks[i].TypeId;

            if (CachedShipStatus.Instance.CommonTasks.FirstOrDefault(
                (NormalPlayerTask t) => t.Index == taskId) != null)
            {
                this.TakeTask.Add(TakeTaskType.Common);
            }
            else if (CachedShipStatus.Instance.LongTasks.FirstOrDefault(
                (NormalPlayerTask t) => t.Index == taskId) != null)
            {
                this.TakeTask.Add(TakeTaskType.Long);
            }
            else if (CachedShipStatus.Instance.NormalTasks.FirstOrDefault(
                (NormalPlayerTask t) => t.Index == taskId) != null)
            {
                this.TakeTask.Add(TakeTaskType.Normal);
            }
            ++takeTask;
            getTaskId.Add((int)targetPlayerInfo.Tasks[i].Id);
        }

        if (getTaskId.Count == 0) { return true; }

        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.AgencyTakeTask))
        {
            caller.WriteByte(this.TargetPlayer);
            caller.WriteInt(getTaskId.Count);
            foreach (int taskid in getTaskId)
            {
                caller.WriteInt(taskid);
            }
        }

        TakeTargetPlayerTask(this.TargetPlayer, getTaskId);
        this.TargetPlayer = byte.MaxValue;

        return true;
    }

    public bool IsAbilityUse()
    {

        this.TargetPlayer = byte.MaxValue;

        PlayerControl target = Player.GetClosestPlayerInRange(
            CachedPlayerControl.LocalPlayer, this,
            this.takeTaskRange);

        if (target != null)
        {
            this.TargetPlayer = target.PlayerId;
        }

        return this.IsCommonUse() && this.TargetPlayer != byte.MaxValue;
    }

    public void ResetOnMeetingStart()
    {
        return;
    }

    public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
    {
        return;
    }

    public void Update(PlayerControl rolePlayer)
    {
        if (CachedShipStatus.Instance == null ||
            GameData.Instance == null) { return; }

        if (!CachedShipStatus.Instance.enabled ||
            this.TakeTask.Count == 0) { return; }

        var playerInfo = GameData.Instance.GetPlayerById(
            rolePlayer.PlayerId);

        for (int i = 0; i < playerInfo.Tasks.Count; ++i)
        {
            if (playerInfo.Tasks[i].Complete)
            {
                TakeTaskType taskType = this.TakeTask[0];
                this.TakeTask.RemoveAt(0);

                int taskIndex;

                switch (taskType)
                {
                    case TakeTaskType.Normal:
                        taskIndex = GameSystem.GetRandomNormalTaskId();
                        break;
                    case TakeTaskType.Long:
                        taskIndex = GameSystem.GetRandomLongTask();
                        break;
                    case TakeTaskType.Common:
                        taskIndex = GameSystem.GetRandomCommonTaskId();
                        break;
                    default:
                        continue;
                }

                GameSystem.RpcReplaceNewTask(
                    rolePlayer.PlayerId, i, taskIndex);
                break;
            }
        }
    }

    protected override void CreateSpecificOption(
        IOptionInfo parentOps)
    {

        CreateIntOption(
            AgencyOption.MaxTaskNum,
            2, 1, 3, 1, parentOps);
        CreateFloatOption(
            AgencyOption.TakeTaskRange,
            1.0f, 0.5f, 2.0f, 0.1f,
            parentOps);

        this.CreateAbilityCountOption(
            parentOps, 2, 5);
    }

    protected override void RoleSpecificInit()
    {
        this.maxTakeTask = OptionHolder.AllOption[
            GetRoleOptionId(AgencyOption.MaxTaskNum)].GetValue() + 1;
        this.takeTaskRange = OptionHolder.AllOption[
            GetRoleOptionId(AgencyOption.TakeTaskRange)].GetValue();

        this.RoleAbilityInit();

        this.TakeTask = new List<TakeTaskType>();

    }
}
