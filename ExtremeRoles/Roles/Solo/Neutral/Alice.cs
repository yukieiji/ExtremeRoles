using System.Collections.Generic;
using System.Linq;

using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Extension.Neutral;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Neutral;

public sealed class Alice : SingleRoleBase, IRoleAbility
{
    public enum AliceOption
    {
        CanUseSabotage,
        RevartCommonTaskNum,
        RevartLongTaskNum,
        RevartNormalTaskNum,
    }

    public ExtremeAbilityButton Button
    { 
        get => this.aliceShipBroken;
        set
        {
            this.aliceShipBroken = value;
        }
    }

    public int RevartLongTask = 0;
    public int RevartNormalTask = 0;
    public int RevartCommonTask = 0;

    private ExtremeAbilityButton aliceShipBroken;

    public Alice(): base(
        ExtremeRoleId.Alice,
        ExtremeRoleType.Neutral,
        ExtremeRoleId.Alice.ToString(),
        ColorPalette.AliceGold,
        true, false, true, true)
    { }

    public void CreateAbility()
    {
        this.CreateAbilityCountButton(
            "shipBroken", Loader.CreateSpriteFromResources(
                Path.AliceShipBroken));
    }

    public override bool IsSameTeam(SingleRoleBase targetRole) => 
        this.IsNeutralSameTeam(targetRole);

    public bool IsAbilityUse()
    {
        return this.IsCommonUse();
    }

    public override void RolePlayerKilledAction(
        PlayerControl rolePlayer,
        PlayerControl killerPlayer)
    {
       if (ExtremeRoleManager.GameRole[killerPlayer.PlayerId].IsImpostor())
       {
            this.IsWin = true;
       }
    }

    public bool UseAbility()
    {

        RandomGenerator.Instance.Next();
        byte localPlayerId = CachedPlayerControl.LocalPlayer.PlayerId;

        foreach (var player in CachedPlayerControl.AllPlayerControls)
        {

            var role = ExtremeRoleManager.GameRole[player.PlayerId];
            if (!role.HasTask()) { continue; }

            List<int> addTaskId = new List<int>();

            for (int i = 0; i < this.RevartLongTask; ++i)
            {
                addTaskId.Add(Helper.GameSystem.GetRandomLongTask());
            }
            for (int i = 0; i < this.RevartCommonTask; ++i)
            {
                addTaskId.Add(Helper.GameSystem.GetRandomCommonTaskId());
            }
            for (int i = 0; i < this.RevartNormalTask; ++i)
            {
                addTaskId.Add(Helper.GameSystem.GetRandomNormalTaskId());
            }

            var shuffled = addTaskId.OrderBy(
                item => RandomGenerator.Instance.Next()).ToList();

            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.AliceShipBroken))
            {
                caller.WriteByte(localPlayerId);
                caller.WriteByte(player.PlayerId);
                caller.WriteInt(addTaskId.Count);
                foreach (int taskId in shuffled)
                {
                    caller.WriteInt(taskId);
                }
            }
            ShipBroken(localPlayerId, player.PlayerId, addTaskId);
        }

        return true;
    }

    public static void ShipBroken(
        byte callerId, byte targetPlayerId, List<int> addTaskId)
    {

        var alice = ExtremeRoleManager.GetSafeCastedRole<Alice>(callerId);
        if (alice == null) { return; }
        var player = Helper.Player.GetPlayerControlById(targetPlayerId);
        if (player == null) { return; }
        
        for (int i = 0; i < player.Data.Tasks.Count; ++i)
        {
            if (addTaskId.Count == 0) { break; }

            if (player.Data.Tasks[i].Complete)
            {
                byte taskId = (byte)addTaskId[0];
                addTaskId.RemoveAt(0);

                if (Helper.GameSystem.SetPlayerNewTask(
                    ref player, taskId, player.Data.Tasks[i].Id))
                {
                    player.Data.Tasks[i] = new GameData.TaskInfo(
                        taskId, player.Data.Tasks[i].Id);
                }
            }
        }
        GameData.Instance.SetDirtyBit(
            1U << (int)player.PlayerId);

    }

    protected override void CreateSpecificOption(
        IOptionInfo parentOps)
    {
        CreateBoolOption(
            AliceOption.CanUseSabotage,
            true, parentOps);

        this.CreateAbilityCountOption(
            parentOps, 2, 100);
        CreateIntOption(
            AliceOption.RevartLongTaskNum,
            1, 0, 15, 1, parentOps);
        CreateIntOption(
            AliceOption.RevartCommonTaskNum,
            1, 0, 15, 1, parentOps);
        CreateIntOption(
            AliceOption.RevartNormalTaskNum,
            1, 0, 15, 1, parentOps);

    }

    protected override void RoleSpecificInit()
    {
        var allOption = OptionManager.Instance;

        this.UseSabotage = allOption.GetValue<bool>(
            GetRoleOptionId(AliceOption.CanUseSabotage));
        this.RevartNormalTask = allOption.GetValue<int>(
            GetRoleOptionId(AliceOption.RevartNormalTaskNum));
        this.RevartLongTask = allOption.GetValue<int>(
            GetRoleOptionId(AliceOption.RevartLongTaskNum));
        this.RevartCommonTask = allOption.GetValue<int>(
            GetRoleOptionId(AliceOption.RevartCommonTaskNum));

        this.RoleAbilityInit();
    }

    public void ResetOnMeetingStart()
    {
        return;
    }

    public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
    {
        return;
    }
}
