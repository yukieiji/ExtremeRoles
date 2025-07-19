using System.Linq;
using System.Collections.Generic;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Extension.State;

using ExtremeRoles.Module;
using ExtremeRoles.Helper;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.Interface;
using Hazel;
using ExtremeRoles.Module.Ability;




using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Impostor.SlaveDriver;

public sealed class SlaveDriverRole :
    SingleRoleBase,
    IRoleAutoBuildAbility
{
	public sealed class HarassmentReportSerializer : IStringSerializer
	{
		public StringSerializerType Type => StringSerializerType.SlaveDriverHarassment;

		public bool IsRpc { get; set; } = true;

		public override string ToString()
			=> Tr.GetString("SlaveDriverReportMessage");

		public void Serialize(RPCOperator.RpcCaller caller)
		{ }

		public void Deserialize(MessageReader reader)
		{ }
	}

	public bool CanSeeTaskBar { get; private set; }
	public ExtremeAbilityButton Button { get; set; }

	private HashSet<byte> effectPlayer = new HashSet<byte>();
	private int revartTaskNum;
	private float range;
	private byte target = byte.MinValue;

    public enum SlaveDriverOption
    {
		CanSeeTaskBar,
		Range,
		RevartTaskNum
    }

    public SlaveDriver() : base(
		RoleCore.BuildImpostor(ExtremeRoleId.SlaveDriver),
        true, false, true, true)
    { }

	public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId)
	{
		if (this.effectPlayer.Contains(targetPlayerId))
		{
			return Design.ColoedString(this.Core.Color, " ★");
		}
		return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
	}

	protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
		factory.CreateBoolOption(
			SlaveDriverOption.CanSeeTaskBar,
			true);
		IRoleAbility.CreateAbilityCountOption(factory, 2, 10);
		factory.CreateIntOption(
			SlaveDriverOption.RevartTaskNum,
			2, 1, 5, 1);
		factory.CreateFloatOption(
			SlaveDriverOption.Range,
			0.75f, 0.25f, 3.5f, 0.25f);
	}

    protected override void RoleSpecificInit()
    {
		var cate = this.Loader;
		this.CanSeeTaskBar = cate.GetValue<SlaveDriverOption, bool>(
			SlaveDriverOption.CanSeeTaskBar);
		this.revartTaskNum= cate.GetValue<SlaveDriverOption, int>(
			SlaveDriverOption.RevartTaskNum);
		this.range = cate.GetValue<SlaveDriverOption, float>(
			SlaveDriverOption.Range);
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        return;
    }

    public void ResetOnMeetingStart()
    {
		this.target = byte.MaxValue;
		foreach (byte playerId in this.effectPlayer)
		{
			NetworkedPlayerInfo player = GameData.Instance.GetPlayerById(playerId);

			if (player == null ||
				!ExtremeRoleManager.GameRole[player.PlayerId].HasTask()) { continue; }

			int replacedTaskNum = 0;

			foreach (var task in player.Tasks.ToArray().OrderBy(
				x => RandomGenerator.Instance.Next()))
			{
				if (replacedTaskNum >= this.revartTaskNum) { break; }
				if (!task.Complete) { continue; }

				int newTaskId = 0;
				byte taskId = task.TypeId;

				if (ShipStatus.Instance.CommonTasks.FirstOrDefault(
					(NormalPlayerTask t) => t.Index == taskId) != null)
				{
					newTaskId = GameSystem.GetRandomCommonTaskId();
				}
				else if (ShipStatus.Instance.LongTasks.FirstOrDefault(
					(NormalPlayerTask t) => t.Index == taskId) != null)
				{
					newTaskId = GameSystem.GetRandomLongTask();
				}
				else if (ShipStatus.Instance.ShortTasks.FirstOrDefault(
					(NormalPlayerTask t) => t.Index == taskId) != null)
				{
					newTaskId = GameSystem.GetRandomShortTaskId();
				}
				else
				{
					continue;
				}

				GameSystem.RpcReplaceNewTask(
					playerId, (int)task.Id, newTaskId);
				replacedTaskNum++;
			}
			if (replacedTaskNum > 0)
			{
				MeetingReporter.RpcAddTargetMeetingChatReport(
					playerId, new HarassmentReportSerializer());
			}
		}
		this.effectPlayer.Clear();
	}

	public void CreateAbility()
	{
		this.CreateAbilityCountButton(
			"Harassment", Resources.UnityObjectLoader.LoadSpriteFromResources(
				Resources.ObjectPath.SlaveDriverHarassment));
	}

	public bool UseAbility()
	{
		this.effectPlayer.Add(this.target);
		this.target = byte.MaxValue;
		return true;
	}

	public bool IsAbilityUse()
	{
		var target = Player.GetClosestPlayerInRange(
			PlayerControl.LocalPlayer, this, this.range);

		if (target == null) { return false; }

		this.target = target.PlayerId;

		return IRoleAbility.IsCommonUse() && !this.effectPlayer.Contains(this.target);
	}
}
