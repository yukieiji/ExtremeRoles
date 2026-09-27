using System.Diagnostics.CodeAnalysis;
using UnityEngine;

using ExtremeRoles.Extension;
using ExtremeRoles.Extension.Manager;
using ExtremeRoles.Extension.Player;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Patches.Button;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.Neutral;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Ability;
using ExtremeRoles.Roles.API.Interface.Status;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral;

public sealed class ImitaterAbilityHandler : IAbility, IKilledFrom
{
	public bool TryKilledFrom(PlayerControl rolePlayer, PlayerControl fromPlayer)
	{
		if (fromPlayer == null)
		{
			return true;
		}

		if (!ExtremeRoleManager.TryGetRole(fromPlayer.PlayerId, out var fromRole) ||
			!Imitater.TryGetExtractInheritedRole(fromRole, out _))
		{
			return true;
		}

		byte imitaterPlayerId = rolePlayer.PlayerId;

		if (fromPlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
		{
			fromPlayer.SetKillTimer(10.0f);
		}

		ExtremeRoleManager.RpcReplaceRole(
			imitaterPlayerId,
			fromPlayer.PlayerId,
			ExtremeRoleManager.ReplaceOperation.ImitaterInherit);

		return false;
	}
}

public sealed class Imitater :
	SingleRoleBase,
	IRoleAutoBuildAbility,
	IRoleResetMeeting
{
	public enum Option
	{
		Range,
	}

	public ExtremeAbilityButton? Button { get; set; }

	private readonly ImitaterAbilityHandler abilityHandler;
	private PlayerControl? targetPlayer;
	private PlayerControl? tmpTargetPlayer;
	private float range;

	public Imitater() : base(
		RoleArgs.BuildNeutral(
			ExtremeRoleId.Imitater,
			ColorPalette.NeutralColor,
			RolePropPresets.OptionalDefault))
	{
		this.abilityHandler = new ImitaterAbilityHandler();
		this.AbilityClass = this.abilityHandler;
	}

	public void CreateAbility()
	{
		this.CreateActivatingAbilityCountButton(
			"ImitaterAbility",
			UnityObjectLoader.LoadSpriteFromResources(ObjectPath.TestButton),
			checkAbility: CheckAbility,
			abilityOff: CleanUp,
			forceAbilityOff: ForceCleanUp,
			isReduceOnActive: false);
	}

	public override bool IsSameTeam(SingleRoleBase targetRole) =>
		this.IsNeutralSameTeam(targetRole);

	public bool IsAbilityUse()
	{
		this.tmpTargetPlayer = Player.GetClosestPlayerInRange(
			PlayerControl.LocalPlayer, this, this.range);
		return IRoleAbility.IsCommonUse() && this.tmpTargetPlayer != null;
	}

	public bool UseAbility()
	{
		if (this.tmpTargetPlayer == null)
		{
			return false;
		}
		this.targetPlayer = this.tmpTargetPlayer;
		return true;
	}

	public bool CheckAbility()
	{
		if (this.targetPlayer == null ||
			!this.targetPlayer.IsAlive() ||
			!PlayerControl.LocalPlayer.IsAlive() ||
			MeetingHud.Instance != null)
		{
			return false;
		}

		float dist = Vector2.Distance(
			PlayerControl.LocalPlayer.GetTruePosition(),
			this.targetPlayer.GetTruePosition());
		return dist <= this.range;
	}

	public void CleanUp()
	{
		if (this.targetPlayer == null ||
			!this.targetPlayer.IsAlive() ||
			!PlayerControl.LocalPlayer.IsAlive() ||
			MeetingHud.Instance != null ||
			!ExtremeRoleManager.TryGetRole(this.targetPlayer.PlayerId, out var targetRole) ||
			!targetRole.CanKill ||
			!TryGetExtractInheritedRole(targetRole, out _))
		{
			ForceCleanUp();
			return;
		}

		float dist = Vector2.Distance(
			PlayerControl.LocalPlayer.GetTruePosition(),
			this.targetPlayer.GetTruePosition());
		if (dist > this.range)
		{
			ForceCleanUp();
			return;
		}

		using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.ImitaterKyugenKill))
		{
			caller.WriteByte(this.targetPlayer.PlayerId);
			caller.WriteByte(PlayerControl.LocalPlayer.PlayerId);
		}
		KyugenKill(this.targetPlayer.PlayerId, PlayerControl.LocalPlayer.PlayerId);

		ForceCleanUp();
	}

	public void ForceCleanUp()
	{
		this.targetPlayer = null;
		this.tmpTargetPlayer = null;
	}

	public static void KyugenKill(byte killerId, byte targetId)
	{
		PlayerControl killer = Player.GetPlayerControlById(killerId);
		PlayerControl target = Player.GetPlayerControlById(targetId);

		if (killer == null || target == null)
		{
			return;
		}

		if (!ExtremeRoleManager.TryGetRole(killer.PlayerId, out var killerRole))
		{
			return;
		}

		if (!KillButtonDoClickPatch.CheckPreKillConditionWithBool(killerRole, killer, target))
		{
			return;
		}

		Player.RpcUncheckMurderPlayer(killerId, targetId, byte.MaxValue);
	}

	public static void InheritTargetRole(byte imitaterPlayerId, byte targetPlayerId)
	{
		if (!ExtremeRoleManager.TryGetRole(targetPlayerId, out var targetRole) ||
			!ExtremeRoleManager.TryGetSafeCastedRole<Imitater>(imitaterPlayerId, out var imitaterRole))
		{
			return;
		}

		clonedNewRole(imitaterRole, imitaterPlayerId, targetRole);
	}

	private static void clonedNewRole(Imitater imitater, byte imitaterPlayerId, SingleRoleBase targetRole)
	{
		IRoleSpecialReset.ResetRole(imitaterPlayerId);
		IRoleSpecialReset.ResetLover(imitaterPlayerId);

		SingleRoleBase templateRole;
		if (!TryGetExtractInheritedRole(targetRole, out var extracted))
		{
			templateRole = targetRole;
		}
		else
		{
			templateRole = extracted;
		}

		if (templateRole is Solo.VanillaRoleWrapper vanillaRole)
		{
			RoleManager.Instance.SetRole(
				Player.GetPlayerControlById(imitaterPlayerId),
				vanillaRole.VanilaRoleId);
		}
		else if (templateRole.IsImpostor())
		{
			RoleManager.Instance.SetRole(
				Player.GetPlayerControlById(imitaterPlayerId),
				AmongUs.GameOptions.RoleTypes.Impostor);
		}
		else
		{
			RoleManager.Instance.SetRole(
				Player.GetPlayerControlById(imitaterPlayerId),
				AmongUs.GameOptions.RoleTypes.Crewmate);
		}

		var newRole = templateRole.Clone();

		if (PlayerControl.LocalPlayer != null &&
			PlayerControl.LocalPlayer.PlayerId == imitaterPlayerId)
		{
			if (newRole is IRoleAbility newAbility)
			{
				newAbility.CreateAbility();
				if (newAbility.Button != null)
				{
					newAbility.Button.HotKey = KeyCode.C;
				}
			}
			if (HudManager.InstanceExists && HudManager.Instance.UseButton != null)
			{
				HudManager.Instance.ReGridButtons();
			}
		}

		newRole.SetControlId(imitater.GameControlId);
		newRole.Initialize();

		ExtremeRoleManager.SetNewRole(imitaterPlayerId, newRole);
	}

	public static bool TryGetExtractInheritedRole(SingleRoleBase? targetRole, [NotNullWhen(true)] out SingleRoleBase? role)
	{
		if (targetRole is null)
		{
			role = null;
			return false;
		}

		if (targetRole is Solo.VanillaRoleWrapper vanillaWrapper)
		{
			role = vanillaWrapper;
			return true;
		}

		int intedId = (int)targetRole.Core.Id;
		if (ExtremeRoleManager.NormalRole.TryGetValue(intedId, out role) &&
			role is not null)
		{
			return true;
		}
		foreach (var combRole in ExtremeRoleManager.CombRole.Values)
		{
			role = combRole.GetRole(intedId, AmongUs.GameOptions.RoleTypes.Crewmate);
			if (role is not null)
			{
				return true;
			}
		}
		role = null;
		return false;
	}

	public void ResetOnMeetingStart()
	{
		ForceCleanUp();
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
		ForceCleanUp();
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		IRoleAbility.CreateAbilityCountOption(
			factory,
			defaultAbilityCount: 1,
			maxAbilityCount: 10,
			defaultActiveTime: 3.0f);
		factory.CreateFloatOption(
			Option.Range,
			1.0f, 0.5f, 3.5f, 0.1f);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;
		this.range = loader.GetValue<Option, float>(Option.Range);
	}
}
