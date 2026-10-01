using ExtremeRoles.Helper;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class Blackmailer : SingleRoleBase, IRoleAutoBuildAbility, IRoleUpdate, IRoleSpecialReset
{
	public enum BlackmailerOption
	{
		Range,
		MultipleBlackmail,
	}

	public ExtremeAbilityButton? Button { get; set; }

	private PlayerControl? tmpTarget;
	private PlayerControl? target;
	private float range;
	private bool multipleBlackmail;
	private bool hasBlackmailedThisRound;
	private BlackmailerSystem? system;

	public Blackmailer() : base(
		RoleArgs.BuildImpostor(
			ExtremeRoleId.Blackmailer,
			RolePropPresets.OptionalDefault))
	{ }

	public void CreateAbility()
	{
		this.CreateActivatingAbilityCountButton(
			"blackmail",
			UnityObjectLoader.LoadSpriteFromResources(ObjectPath.TestButton),
			IsAbilityCheck,
			CleanUp,
			ForceCleanUp,
			isReduceOnActive: true);
	}

	public bool IsAbilityCheck()
		=> this.target != null &&
			Player.IsPlayerInRangeAndDrawOutLine(
				PlayerControl.LocalPlayer,
				this.target,
				this,
				this.range);

	public bool UseAbility()
	{
		this.target = this.tmpTarget;
		return true;
	}

	public bool IsAbilityUse()
	{
		var localPlayer = PlayerControl.LocalPlayer;
		return
			Player.TryGetClosestPlayerInRange(localPlayer, this, this.range, out this.tmpTarget) &&
			(this.multipleBlackmail || !this.hasBlackmailedThisRound) &&
			this.system != null && 
			!this.system.IsBlackmailedBy(localPlayer.PlayerId, this.tmpTarget.PlayerId) &&
			IRoleAbility.IsCommonUse();
	}

	public void CleanUp()
	{
		var localPlayer = PlayerControl.LocalPlayer;
		if (this.target != null && localPlayer != null && this.system != null)
		{
			byte localId = localPlayer.PlayerId;
			if (!this.multipleBlackmail)
			{
				this.system.RpcClearBlackmail(localId);
			}
			this.system.RpcAddBlackmail(localId, this.target.PlayerId);
			this.hasBlackmailedThisRound = true;
		}
		this.target = null;
	}

	public void ForceCleanUp()
	{
		this.target = null;
	}

	public void ResetOnMeetingStart()
	{ }

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
		this.hasBlackmailedThisRound = false;
	}

	public void Update(PlayerControl rolePlayer)
	{ }

	public override void ExiledAction(
		PlayerControl rolePlayer)
		=> AllReset(rolePlayer);

	public override void RolePlayerKilledAction(
		PlayerControl rolePlayer,
		PlayerControl killerPlayer)
		=> AllReset(rolePlayer);

	public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId)
	{
		var localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer != null &&
			this.system != null && 
			this.system.IsBlackmailedBy(localPlayer.PlayerId, targetPlayerId))
		{
			return $" {Design.ColoredString(Palette.ImpostorRed, "◆")}";
		}
		return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		IRoleAbility.CreateAbilityCountOption(
			factory, 1, 15, 3.0f);

		factory.CreateFloatOption(
			BlackmailerOption.Range,
			0.75f, 0.25f, 3.5f, 0.25f);

		factory.CreateBoolOption(
			BlackmailerOption.MultipleBlackmail,
			false);
	}

	protected override void RoleSpecificInit()
	{
		var cate = this.Loader;
		this.range = cate.GetValue<BlackmailerOption, float>(BlackmailerOption.Range);
		this.multipleBlackmail = cate.GetValue<BlackmailerOption, bool>(BlackmailerOption.MultipleBlackmail);
		this.hasBlackmailedThisRound = false;

		this.system = ExtremeSystemTypeManager.Instance.CreateOrGet<BlackmailerSystem>(
			ExtremeSystemType.BlackmailerSystem);
	}

	public void AllReset(PlayerControl rolePlayer)
	{
		this.system?.ClearBlackmail(rolePlayer.PlayerId);
	}
}
