using System.Linq;
using System.Collections.Generic;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

using ExtremeRoles.Module;
using ExtremeRoles.Helper;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class SlaveDriver :
    SingleRoleBase,
    IRoleAbility
{
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
        ExtremeRoleId.SlaveDriver,
        ExtremeRoleType.Impostor,
        ExtremeRoleId.SlaveDriver.ToString(),
        Palette.ImpostorRed,
        true, false, true, true)
    { }

	public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId)
	{
		if (this.effectPlayer.Contains(targetPlayerId))
		{
			return Design.ColoedString(this.NameColor, " ★");
		}
		return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
	}

	protected override void CreateSpecificOption(
        IOptionInfo parentOps)
    {
		CreateBoolOption(
			SlaveDriverOption.CanSeeTaskBar,
			true, parentOps);
		this.CreateAbilityCountOption(parentOps, 2, 10);
		CreateIntOption(
			SlaveDriverOption.RevartTaskNum,
			2, 1, 5, 1, parentOps);
		CreateFloatOption(
			SlaveDriverOption.Range,
			0.75f, 0.25f, 3.5f, 0.25f, parentOps);
	}

    protected override void RoleSpecificInit()
    {
		this.CanSeeTaskBar = OptionManager.Instance.GetValue<bool>(
			GetRoleOptionId(SlaveDriverOption.CanSeeTaskBar));
		this.revartTaskNum= OptionManager.Instance.GetValue<int>(
			GetRoleOptionId(SlaveDriverOption.RevartTaskNum));
		this.range = OptionManager.Instance.GetValue<float>(
			GetRoleOptionId(SlaveDriverOption.Range));
		this.RoleAbilityInit();
    }

    public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
    {
        return;
    }

    public void ResetOnMeetingStart()
    {
		this.target = byte.MaxValue;
		string reportMessage = Translation.GetString("SlaveDriverReportMessage");
		foreach (byte playerId in this.effectPlayer)
		{
			GameData.PlayerInfo player = GameData.Instance.GetPlayerById(playerId);

			if (player == null) { continue; }

			int replacedTaskNum = 0;

			foreach (var task in player.Tasks.ToArray().OrderBy(
				x => RandomGenerator.Instance.Next()))
			{
				if (replacedTaskNum >= this.revartTaskNum) { break; }
				if (!task.Complete) { continue; }

				int newTaskId = 0;
				byte taskId = task.TypeId;

				if (CachedShipStatus.Instance.CommonTasks.FirstOrDefault(
				(NormalPlayerTask t) => t.Index == taskId) != null)
				{
					newTaskId = GameSystem.GetRandomCommonTaskId();
				}
				else if (CachedShipStatus.Instance.LongTasks.FirstOrDefault(
					(NormalPlayerTask t) => t.Index == taskId) != null)
				{
					newTaskId = GameSystem.GetRandomLongTask();
				}
				else if (CachedShipStatus.Instance.NormalTasks.FirstOrDefault(
					(NormalPlayerTask t) => t.Index == taskId) != null)
				{
					newTaskId = GameSystem.GetRandomNormalTaskId();
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
					playerId, string.Format(reportMessage, replacedTaskNum));
			}
		}
		this.effectPlayer.Clear();
	}

	public void CreateAbility()
	{
		this.CreateAbilityCountButton(
			"Harassment", Resources.Loader.CreateSpriteFromResources(
				Resources.Path.TestButton));
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
			CachedPlayerControl.LocalPlayer, this, this.range);

		if (target == null) { return false; }

		this.target = target.PlayerId;

		return this.IsCommonUse() && !this.effectPlayer.Contains(this.target);
	}
}
