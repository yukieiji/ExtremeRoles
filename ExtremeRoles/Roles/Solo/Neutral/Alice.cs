using System.Collections.Generic;
using System.Linq;

using ExtremeRoles.Module;

using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Extension.Neutral;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption.Factory.Old;

namespace ExtremeRoles.Roles.Solo.Neutral;

public sealed class Alice :
	SingleRoleBase,
	IRoleAutoBuildAbility,
	IRolePerformKillHook
{
    public enum AliceOption
    {
        CanUseSabotage,
        RevartCommonTaskNum,
        RevartLongTaskNum,
        RevartNormalTaskNum,
		WinTaskRate,
		WinKillNum,
    }

    public ExtremeAbilityButton Button { get; set; }

	private int revartLongTask = 0;
	private int revartNormalTask = 0;
	private int revartCommonTask = 0;

	private int killCount;
	private int winKillCount;
	private float winTaskRate;

    public Alice(): base(
		RoleCore.BuildNeutral(
			ExtremeRoleId.Alice,
			ColorPalette.AliceGold),
        true, false, true, true)
    { }

    public void CreateAbility()
    {
        this.CreateAbilityCountButton(
            "shipBroken", UnityObjectLoader.LoadSpriteFromResources(
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
		if (!(
				rolePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId &&
				ExtremeRoleManager.TryGetRole(killerPlayer.PlayerId, out var role) &&
				role.IsImpostor()
			))
		{
			return;
		}

		float taskRate = Player.GetPlayerTaskGage(rolePlayer.Data);
		this.IsWin =
			this.killCount >= this.winKillCount &&
			taskRate >= this.winTaskRate;

		ExtremeRolesPlugin.Logger.LogInfo($"CurKillCount:{this.killCount}");

		if (!this.IsWin)
		{
			return;
		}
		ExtremeRolesPlugin.ShipState.RpcRoleIsWin(rolePlayer.PlayerId);
	}

    public bool UseAbility()
    {

        RandomGenerator.Instance.Next();
        byte localPlayerId = PlayerControl.LocalPlayer.PlayerId;

        foreach (var player in PlayerCache.AllPlayerControl)
        {

            if (!(
					ExtremeRoleManager.TryGetRole(player.PlayerId, out var role) &&
					role.HasTask()
				))
			{
				continue;
			}

			int size = this.revartLongTask + this.revartCommonTask + this.revartNormalTask;

			List<int> addTaskId = new List<int>(size);

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
        OldAutoParentSetOptionCategoryFactory factory)
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
		factory.CreateIntOption(
			AliceOption.WinTaskRate,
			0, 0, 100, 10,
			format: OptionUnit.Percentage);
		factory.CreateIntOption(
			AliceOption.WinKillNum,
			0, 0, 5, 1);
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

		this.winTaskRate = loader.GetValue<AliceOption, int>(
			AliceOption.WinTaskRate) / 100.0f;
		this.winKillCount = loader.GetValue<AliceOption, int>(
			AliceOption.WinKillNum);
		this.HasTask = this.winTaskRate > 0;
		this.killCount = 0;
    }

    public void ResetOnMeetingStart()
    {
        return;
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        return;
    }

	public void OnStartKill()
	{ }

	public void OnEndKill()
	{
		ExtremeRolesPlugin.Logger.LogInfo($"CurKillCount:{this.killCount}");
		this.killCount++;
	}
}
