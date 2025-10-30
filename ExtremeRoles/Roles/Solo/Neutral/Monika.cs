using ExtremeRoles.Helper;

using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Module.CustomOption.Factory;

using ExtremeRoles.Resources;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;


#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral;

public sealed class Monika :
	SingleRoleBase,
	IRoleAutoBuildAbility,
	IRoleReportHook
{
	public enum Ops
	{
		IsSoloTeam,
		CanUseVent,
		CanUseSabotage,
		UseOtherButton,
		Range,
		CanSeeTrash,
	}

	public bool IsSoloTeam { get; private set; }

	public ExtremeAbilityButton? Button { get; set; }
	private MonikaTrashSystem? trashSystem;
	private MonikaMeetingNumSystem? meetingNumSystem;
	private byte targetPlayer;
	private float range;

	public Monika(): base(
		RoleCore.BuildNeutral(
			ExtremeRoleId.Monika,
			ColorPalette.MonikaRoseSaumon),
        false, false, false, false)
    { }

	public void CreateAbility()
    {
		this.CreateNormalAbilityButton(
			"monikaPlayerTrash",
			UnityObjectLoader.LoadFromResources(ExtremeRoleId.Monika));
	}

	public bool IsAbilityUse()
	{
		if (this.trashSystem is null)
		{
			return false;
		}

		this.targetPlayer = byte.MaxValue;
		var player = Player.GetClosestPlayerInRange(
			PlayerControl.LocalPlayer, this,
			this.range);

		if (player == null ||
			this.trashSystem.InvalidPlayer(player))
		{
			return false;
		}
		this.targetPlayer = player.PlayerId;

		return
			IRoleAbility.IsCommonUse();
	}

	public bool UseAbility()
    {
		if (this.targetPlayer == byte.MaxValue ||
			this.trashSystem == null)
		{
			return false;
		}

		if (PlayerControl.LocalPlayer != null &&
			ExtremeRoleManager.TryGetRole(targetPlayer, out var role) &&
			role.Core.Id is ExtremeRoleId.Monika)
		{
			// モニカに対して能力を使用したときは殺す
			Player.RpcUncheckMurderPlayer(
				PlayerControl.LocalPlayer.PlayerId,
				this.targetPlayer,
				byte.MaxValue);
			return true;
		}

		this.trashSystem.RpcAddTrash(this.targetPlayer);
		return true;
    }

    protected override void CreateSpecificOption(OptionCategoryScope<AutoParentSetBuilder> categoryScope)
    {
        var factory = categoryScope.Builder;
		factory.CreateBoolOption(
			Ops.IsSoloTeam, true);
		factory.CreateBoolOption(
			Ops.CanUseVent, false);
		factory.CreateBoolOption(
			Ops.CanUseSabotage, false);
		factory.CreateBoolOption(
			Ops.UseOtherButton, true);
		IRoleAbility.CreateCommonAbilityOption(
            factory);
		factory.CreateFloatOption(
			Ops.Range, 1.3f, 0.1f, 3.0f, 0.1f);
		factory.CreateBoolOption(
			Ops.CanSeeTrash, false);
	}

    protected override void RoleSpecificInit()
    {
		var loader = this.Loader;
		this.trashSystem = ExtremeSystemTypeManager.Instance.CreateOrGet(
			ExtremeSystemType.MonikaTrashSystem,
			() => new MonikaTrashSystem(loader.GetValue<Ops, bool>(Ops.CanSeeTrash)));

		this.UseVent = loader.GetValue<Ops, bool>(Ops.CanUseVent);
		this.UseSabotage = loader.GetValue<Ops, bool>(Ops.CanUseSabotage);
		this.IsSoloTeam = loader.GetValue<Ops, bool>(Ops.IsSoloTeam);

		if (loader.GetValue<Ops, bool>(Ops.UseOtherButton))
		{
			this.meetingNumSystem = ExtremeSystemTypeManager.Instance.CreateOrGet<MonikaMeetingNumSystem>(
				ExtremeSystemType.MonikaMeetingNumSystem);
		}
		this.range = loader.GetValue<Ops, float>(Ops.Range);
    }

    public void ResetOnMeetingStart()
    {
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
        return;
    }

	public void HookReportButton(PlayerControl rolePlayer, NetworkedPlayerInfo reporter)
	{
		if (this.meetingNumSystem is null)
		{
			return;
		}
		byte reporterPlayerId = reporter.PlayerId;
		if (rolePlayer.PlayerId == reporterPlayerId)
		{
			if (!this.meetingNumSystem.TryReduce())
			{
				return;
			}
			rolePlayer.RemainingEmergencies = GameOptionsManager.Instance.currentNormalGameOptions.NumEmergencyMeetings;
		}
		else
		{
			this.meetingNumSystem.RpcReduceTo(reporterPlayerId, false);
		}
	}

	public void HookBodyReport(PlayerControl rolePlayer, NetworkedPlayerInfo reporter, NetworkedPlayerInfo reportBody)
	{ }
}
