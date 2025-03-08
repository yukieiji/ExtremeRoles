using System.Collections.Generic;
using System.Linq;

using ExtremeRoles.Module;

using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Extension.Neutral;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.Ability;

using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Solo.Neutral;

public sealed class Alice : SingleRoleBase, IRoleAutoBuildAbility
{
    public enum AliceOption
    {
        CanUseSabotage,
        RevartCommonTaskNum,
        RevartLongTaskNum,
        RevartNormalTaskNum,
    }

    public ExtremeAbilityButton Button { get; set; }

	private int revartLongTask = 0;
	private int revartNormalTask = 0;
	private int revartCommonTask = 0;

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
            "shipBroken", Resources.UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.AliceShipBroken));
    }

    public override bool IsSameTeam(SingleRoleBase targetRole) =>
        this.IsNeutralSameTeam(targetRole);

    public bool IsAbilityUse()
		=> IRoleAbility.IsCommonUse();

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
        byte localPlayerId = PlayerControl.LocalPlayer.PlayerId;

        foreach (var player in PlayerCache.AllPlayerControl)
        {

            var role = ExtremeRoleManager.GameRole[player.PlayerId];
            if (!role.HasTask())
			{
				continue;
			}

            List<int> addTaskId = new List<int>();

            for (int i = 0; i < this.revartLongTask; ++i)
            {
                addTaskId.Add(Helper.GameSystem.GetRandomLongTask());
            }
            for (int i = 0; i < this.revartCommonTask; ++i)
            {
                addTaskId.Add(Helper.GameSystem.GetRandomCommonTaskId());
            }
            for (int i = 0; i < this.revartNormalTask; ++i)
            {
                addTaskId.Add(Helper.GameSystem.GetRandomShortTaskId());
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

        if (!ExtremeRoleManager.TryGetSafeCastedRole<Alice>(callerId, out var alice))
		{
			return;
		}

        var player = Helper.Player.GetPlayerControlById(targetPlayerId);
        if (player == null)
		{
			return;
		}

        for (int i = 0; i < player.Data.Tasks.Count; ++i)
        {
            if (addTaskId.Count == 0)
			{
				break;
			}

			var task = player.Data.Tasks[i];
			if (!task.Complete)
			{
				continue;
			}
			byte taskId = (byte)addTaskId[0];
			addTaskId.RemoveAt(0);
			uint id = task.Id;
			if (Helper.GameSystem.SetPlayerNewTask(
				player, taskId, id))
			{
				player.Data.Tasks[i] = new(taskId, id);
			}
		}

		player.Data.MarkDirty();

		if (AmongUsClient.Instance != null &&
			AmongUsClient.Instance.AmHost)
		{
			GameData.Instance.RecomputeTaskCounts();
		}
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateBoolOption(
            AliceOption.CanUseSabotage,
            true);

        IRoleAbility.CreateAbilityCountOption(
            factory, 2, 100);
        factory.CreateIntOption(
            AliceOption.RevartLongTaskNum,
            1, 0, 15, 1);
        factory.CreateIntOption(
            AliceOption.RevartCommonTaskNum,
            1, 0, 15, 1);
        factory.CreateIntOption(
            AliceOption.RevartNormalTaskNum,
            1, 0, 15, 1);

    }

    protected override void RoleSpecificInit()
    {
        var loader = this.Loader;

        this.UseSabotage = loader.GetValue<AliceOption, bool>(
            AliceOption.CanUseSabotage);
        this.revartNormalTask = loader.GetValue<AliceOption, int>(
            AliceOption.RevartNormalTaskNum);
        this.revartLongTask = loader.GetValue<AliceOption, int>(
            AliceOption.RevartLongTaskNum);
        this.revartCommonTask = loader.GetValue<AliceOption, int>(
            AliceOption.RevartCommonTaskNum);
    }

    public void ResetOnMeetingStart()
    {
        return;
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        return;
    }
}
