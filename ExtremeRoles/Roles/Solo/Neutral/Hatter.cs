using UnityEngine;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityBehavior.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.Neutral;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;



using ExtremeRoles.Module.CustomOption.Factory;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral;

public sealed class Hatter : SingleRoleBase, IRoleAutoBuildAbility, IRoleUpdate, IDeadBodyReportOverride
{
    public enum HatterOption
	{
        CanRepairSabotage,
		WinCount,
		MeetingTimerDecreaseLower,
		MeetingTimerDecreaseUpper,
		HideMeetingTimer,
		IncreaseTaskGage,
		IncreseNum
    }

	public bool CanReport => false;

	public ExtremeAbilityButton? Button { get; set; }

	private int winSkipCount = 0;
	private int curSkipCount = 0;
	private bool isAssassinMeeting = false;

	private bool isHideMeetingTimer;
	private int meetingTimerDecreaseLower;
	private int meetingTimerDecreaseUpper;

	private bool isUpgrated = false;
	private float abilityIncreaseTaskGage;
	private int abilityIncreaseNum;

	public Hatter(): base(
        ExtremeRoleId.Hatter,
        ExtremeRoleType.Neutral,
        ExtremeRoleId.Hatter.ToString(),
        ColorPalette.HatterYanagizome,
        false, true, false, false,
		false, false)
    { }

	public void Update(PlayerControl rolePlayer)
	{
		if (this.isUpgrated)
		{
			return;
		}

		float curTaskGage = Player.GetPlayerTaskGage(rolePlayer);
		if (curTaskGage > this.abilityIncreaseTaskGage &&
			this.Button is not null &&
			this.Button.Behavior is ICountBehavior countBehavior)
		{
			this.isUpgrated = true;
			countBehavior.SetAbilityCount(
				countBehavior.AbilityCount + this.abilityIncreaseNum);
		}
	}

	public void CreateAbility()
    {
        this.CreateAbilityCountButton(
            "timeKill", Resources.Loader.CreateSpriteFromResources(
				Path.HatterTimeKill));
    }

    public override bool IsSameTeam(SingleRoleBase targetRole) =>
        this.IsNeutralSameTeam(targetRole);

	public bool IsAbilityUse() => IRoleAbility.IsCommonUse();

    public bool UseAbility()
    {
		if (!ExtremeSystemTypeManager.Instance.TryGet<ModdedMeetingTimeSystem>(
				ExtremeSystemType.ModdedMeetingTimeSystem, out var system) ||
			system == null)
		{
			return false;
		}

		int decrease =
			this.meetingTimerDecreaseLower == this.meetingTimerDecreaseUpper ?
			this.meetingTimerDecreaseUpper :
			RandomGenerator.Instance.Next(this.meetingTimerDecreaseLower, this.meetingTimerDecreaseUpper);

		if (decrease != 0 && GameManager.Instance.LogicOptions.IsTryCast<LogicOptionsNormal>(out var opt))
		{
			int discussionTime = opt!.GetDiscussionTime();
			int voteTime = opt!.GetVotingTime();
			int reduceTime = Mathf.CeilToInt((discussionTime + voteTime) * (decrease / 100.0f));

			ExtremeSystemTypeManager.RpcUpdateSystemOnlyHost(
				ExtremeSystemType.ModdedMeetingTimeSystem, (x) =>
				{
					x.Write((byte)ModdedMeetingTimeSystem.Ops.ChangeMeetingHudTempOffset);
					x.WritePacked(system.TempOffset - reduceTime);
				});
		}

		if (this.isHideMeetingTimer)
		{
			ExtremeSystemTypeManager.RpcUpdateSystemOnlyHost(
				ExtremeSystemType.ModdedMeetingTimeSystem, (x) =>
				{
					x.Write((byte)ModdedMeetingTimeSystem.Ops.ChangeMeetingTimerShower);
					x.Write(false);
				});
		}

		return true;
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
		factory.CreateBoolOption(
		   HatterOption.CanRepairSabotage,
		   false);

		factory.CreateIntOption(
		   HatterOption.WinCount,
		   3, 1, 10, 1);

		IRoleAbility.CreateAbilityCountOption(
			factory, 3, 10, minAbilityCount: 0);

		factory.CreateBoolOption(
			HatterOption.HideMeetingTimer, true);

		var lowerOpt = factory.CreateIntDynamicOption(
			HatterOption.MeetingTimerDecreaseLower,
			0, 0, 5,
			format: OptionUnit.Percentage);

		var upperOpt = factory.CreateIntOption(
			HatterOption.MeetingTimerDecreaseUpper,
			20, 0, 50, 5,
			format: OptionUnit.Percentage);
		upperOpt.AddWithUpdate(lowerOpt);

		factory.CreateIntOption(
			HatterOption.IncreaseTaskGage,
			50, 0, 100, 10,
			format: OptionUnit.Percentage);
		factory.CreateIntOption(
			HatterOption.IncreseNum,
			3, 1, 10, 1);
	}

    protected override void RoleSpecificInit()
    {
		var cate = this.Loader;
		this.CanRepairSabotage = cate.GetValue<HatterOption, bool>(
			HatterOption.CanRepairSabotage);
		this.winSkipCount = cate.GetValue<HatterOption, int>(
			HatterOption.WinCount);

		this.isHideMeetingTimer = cate.GetValue<HatterOption, bool>(
			HatterOption.HideMeetingTimer);

		this.meetingTimerDecreaseLower = cate.GetValue<HatterOption, int>(
			HatterOption.MeetingTimerDecreaseLower);
		this.meetingTimerDecreaseUpper = cate.GetValue<HatterOption, int>(
			HatterOption.MeetingTimerDecreaseUpper);

		this.isUpgrated = false;

		this.abilityIncreaseTaskGage = (float)cate.GetValue<HatterOption, int>(
			HatterOption.IncreaseTaskGage) / 100.0f;
		this.abilityIncreaseNum = cate.GetValue<HatterOption, int>(
			HatterOption.IncreseNum);

		ExtremeSystemTypeManager.Instance.TryAdd(
			ExtremeSystemType.ModdedMeetingTimeSystem, new ModdedMeetingTimeSystem());

		this.curSkipCount = 0;
		this.isAssassinMeeting = false;
		this.IsWin = false;
    }

    public void ResetOnMeetingStart()
    {
		this.isAssassinMeeting = ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger;
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
		PlayerControl localPlayer = PlayerControl.LocalPlayer;

		if (localPlayer == null ||
			exiledPlayer != null ||
			this.isAssassinMeeting ||
			this.IsWin)
		{
			return;
		}

		++this.curSkipCount;

		if (this.curSkipCount >= this.winSkipCount)
		{
			ExtremeRolesPlugin.ShipState.RpcRoleIsWin(localPlayer.PlayerId);
		}
    }
}
