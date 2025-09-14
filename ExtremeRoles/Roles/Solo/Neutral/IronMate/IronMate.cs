using AmongUs.GameOptions;

using UnityEngine;

using ExtremeRoles.Module;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;

using ExtremeRoles.Helper;

using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Module.CustomOption.Factory.Old;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral.IronMate;

public sealed class IronMateRole :
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

	private IronMateStatusModel? status;

	private float playerShowTime;
	private float deadBodyShowTime;
    public override IStatusModel? Status => status;

    public IronMateRole(): base(
		RoleCore.BuildNeutral(
			ExtremeRoleId.IronMate,
			ColorPalette.IronMateAluminium),
        false, true, false, false)
    {
    }

	public override string GetColoredRoleName(bool isTruthColor = false)
	{
		if (isTruthColor || IsAwake)
		{
			return base.GetColoredRoleName();
		}
		else
		{
			return Design.ColoredString(
				Palette.White,
				Tr.GetString(RoleTypes.Crewmate.ToString()));
		}
	}
	public override string GetFullDescription()
	{
		if (IsAwake)
		{
			return Tr.GetString(
				$"{this.Core.Id}FullDescription");
		}
		else
		{
			return Tr.GetString(
				$"{RoleTypes.Crewmate}FullDescription");
		}
	}

	public override string GetImportantText(bool isContainFakeTask = true)
		=> Design.ColoredString(
				Palette.White,
				$"{GetColoredRoleName()}: {Tr.GetString("crewImportantText")}");

	public override string GetIntroDescription()
		=> Design.ColoredString(
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
        var loader = Loader;
        status = new IronMateStatusModel(loader.GetValue<Option, int>(Option.BlockNum));
		var system = ExtremeSystemTypeManager.Instance.CreateOrGet(
			ExtremeSystemType.IronMateGuard,
			() => new IronMateGurdSystem(
				loader.GetValue<Option, float>(Option.SlowMod),
				loader.GetValue<Option, float>(Option.SlowTime)));
		AbilityClass = new IronMateAbilityHandler(status, system);

		playerShowTime = loader.GetValue<Option, float>(Option.PlayerShowTime);
		deadBodyShowTime = loader.GetValue<Option, float>(Option.DeadBodyShowTimeOnAfterPlayer);
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
			case GameOverReason.CrewmatesByVote:
			case GameOverReason.CrewmatesByTask:
			case GameOverReason.CrewmateDisconnect:
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
			newDeadBody.SetUp(target, deadBodyShowTime, playerShowTime);
			body.myCollider.enabled = false;
			break;
		}
	}
}
