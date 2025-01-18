using AmongUs.GameOptions;

using UnityEngine;

using ExtremeRoles.Module;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

using ExtremeRoles.Helper;

using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Module.SystemType.Roles;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral;

public sealed class IronMate :
	SingleRoleBase,
	IRoleAwake<RoleTypes>,
	IRoleMurderPlayerHook,
	IRoleWinPlayerModifier,
	IDeadBodyReportOverride
{
    public enum Option
    {
        BlockNum,
		SlowTime,
		SlowMod,
		PlayerShowTime,
		DeadBodyShowTimeOnAfterPlayer,
	}

	public bool IsAwake => false;
	public RoleTypes NoneAwakeRole => RoleTypes.Crewmate;
	public bool CanReport => false;

	private IronMateGurdSystem? system;

	private float playerShowTime;
	private float deadBodyShowTime;

    public IronMate(): base(
        ExtremeRoleId.IronMate,
        ExtremeRoleType.Neutral,
        ExtremeRoleId.IronMate.ToString(),
        ColorPalette.IronMateAluminium,
        false, true, false, false)
    { }

	public override string GetColoredRoleName(bool isTruthColor = false)
	{
		if (isTruthColor || IsAwake)
		{
			return base.GetColoredRoleName();
		}
		else
		{
			return Design.ColoedString(
				Palette.White,
				Tr.GetString(RoleTypes.Crewmate.ToString()));
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
		=> Design.ColoedString(
				Palette.White,
				$"{this.GetColoredRoleName()}: {Tr.GetString("crewImportantText")}");

	public override string GetIntroDescription()
		=> Design.ColoedString(
				Palette.CrewmateBlue,
				PlayerControl.LocalPlayer.Data.Role.Blurb);

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

	public override bool TryRolePlayerKilledFrom(PlayerControl rolePlayer, PlayerControl fromPlayer)
	{
		if (this.system is null)
		{
			return true;
		}

		byte playerId = rolePlayer.PlayerId;

		if (!this.system.IsContains(playerId))
		{
			this.system.SetUp(playerId, this.Loader.GetValue<Option, int>(Option.BlockNum));
		}

		if (!this.system.TryGetShield(playerId, out int num))
		{
			return true;
		}

		if (fromPlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
		{
			fromPlayer.SetKillTimer(10.0f);
			Sound.PlaySound(Sound.Type.GuardianAngleGuard, 0.85f);
		}
		this.system.RpcUpdateNum(playerId, num - 1);
		return false;
	}

	protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
		factory.CreateIntOption(
			Option.BlockNum,
			1, 0, 10, 1);

		factory.CreateFloatOption(
			Option.SlowTime,
			10.0f, 0.0f, 30.0f, 0.5f,
			format: OptionUnit.Second);

		factory.CreateFloatOption(
			Option.SlowMod,
			0.7f, 0.1f, 1.0f, 0.1f,
			format: OptionUnit.Multiplier);

		factory.CreateFloatOption(
			Option.PlayerShowTime,
			10f, 0.0f, 30.0f, 0.1f,
			format: OptionUnit.Second);
		factory.CreateFloatOption(
			Option.DeadBodyShowTimeOnAfterPlayer,
			10f, 0.0f, 30.0f, 0.1f,
			format: OptionUnit.Second);
	}

    protected override void RoleSpecificInit()
    {
        var loader = this.Loader;

		this.system = ExtremeSystemTypeManager.Instance.CreateOrGet(
			ExtremeSystemType.IronMateGuard,
			() => new IronMateGurdSystem(
				loader.GetValue<Option, float>(Option.SlowMod),
				loader.GetValue<Option, float>(Option.SlowTime)));
		this.playerShowTime = loader.GetValue<Option, float>(Option.PlayerShowTime);
		this.deadBodyShowTime = loader.GetValue<Option, float>(Option.DeadBodyShowTimeOnAfterPlayer);
	}

    public void ResetOnMeetingStart()
    {
        return;
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
        return;
    }

	public string GetFakeOptionString() => "";

	public void Update(PlayerControl rolePlayer)
	{　}

	public void ModifiedWinPlayer(NetworkedPlayerInfo rolePlayerInfo, GameOverReason reason, in WinnerTempData winner)
	{
		switch (reason)
		{
			case GameOverReason.HumansByVote:
			case GameOverReason.HumansByTask:
			case GameOverReason.HumansDisconnect:
				winner.AddWithPlus(rolePlayerInfo);
				return;
			default:
				return;
		}
	}

	public void HookMuderPlayer(PlayerControl source, PlayerControl target)
	{
		if (PlayerControl.LocalPlayer == null ||
			PlayerControl.LocalPlayer.Data == null ||
			PlayerControl.LocalPlayer.Data.IsDead ||
			PlayerControl.LocalPlayer.Data.Disconnected ||
			target == null ||
			target.Data == null ||
			!target.Data.IsDead ||
			target.Data.Disconnected)
		{
			return;
		}

		var deadBody = Object.FindObjectsOfType<DeadBody>();
		foreach (var body in deadBody)
		{
			if (body.ParentId != target.PlayerId)
			{
				continue;
			}
			var newDeadBody = body.gameObject.AddComponent<IronMateDeadBody>();
			newDeadBody.SetUp(target, this.deadBodyShowTime, this.playerShowTime);
			body.myCollider.enabled = false;
			break;
		}
	}
}
