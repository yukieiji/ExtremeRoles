using UnityEngine;

using ExtremeRoles.Extension.Manager;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Module.CustomOption.OLDS;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class ItakoRole :
	MultiAssignRoleBase,
	IRoleAutoBuildAbility,
	IRoleResetMeeting
{
	public enum Option
	{
		Range,
		RequiredTaskRate,
	}

	public override IOptionLoader Loader
	{
		get
		{
			if (this.OffsetInfo != null &&
				OptionManager.Instance.TryGetCategory(
					this.Tab,
					ExtremeRoleManager.GetCombRoleGroupId(this.OffsetInfo.RoleId),
					out var combCate))
			{
				return new OptionLoadWrapper(combCate, this.OffsetInfo.IdOffset);
			}

			if (OptionManager.Instance.TryGetCategory(
					this.Tab,
					ExtremeRoleManager.GetRoleGroupId(this.Core.Id),
					out var cate))
			{
				return cate;
			}

			return base.Loader;
		}
	}

	public ExtremeAbilityButton? Button { get; set; }

	private NetworkedPlayerInfo? targetBody;
	private NetworkedPlayerInfo? tmpTargetBody;
	private byte activeTargetBodyId = byte.MaxValue;

	private float range;
	private float requiredTaskRate;

	public ItakoRole() : base(
		RoleArgs.BuildCrewmate(
			ExtremeRoleId.Itako,
			ColorPalette.ItakoSkyBlue,
			RolePropPresets.OptionalDefault))
	{
		this.CanHasAnotherRole = true;
	}

	public void CreateAbility()
	{
		this.CreateActivatingAbilityCountButton(
			"ItakoAbility",
			UnityObjectLoader.LoadFromResources(ExtremeRoleId.Itako),
			checkAbility: CheckAbility,
			abilityOff: CleanUp,
			forceAbilityOff: ForceCleanUp,
			isReduceOnActive: false);
		this.Button?.SetLabelToCrewmate();
	}

	public bool IsAbilityUse()
	{
		this.tmpTargetBody = Player.GetDeadBodyInfo(this.range);
		return IRoleAbility.IsCommonUse() && this.tmpTargetBody != null;
	}

	public bool UseAbility()
	{
		if (this.tmpTargetBody == null)
		{
			return false;
		}
		this.targetBody = this.tmpTargetBody;
		this.activeTargetBodyId = this.targetBody.PlayerId;
		return true;
	}

	public bool CheckAbility()
	{
		var check = Player.GetDeadBodyInfo(this.range);
		return check != null && check.PlayerId == this.activeTargetBodyId;
	}

	public void CleanUp()
	{
		if (this.targetBody == null || PlayerControl.LocalPlayer == null)
		{
			ForceCleanUp();
			return;
		}

		byte localPlayerId = PlayerControl.LocalPlayer.PlayerId;
		byte targetPlayerId = this.targetBody.PlayerId;

		bool callerIsCrewmate = this.IsCrewmate();
		bool targetIsCrewmate = ExtremeRoleManager.TryGetRole(targetPlayerId, out var targetRole) && targetRole.IsCrewmate();

		if (!callerIsCrewmate || !targetIsCrewmate)
		{
			Player.RpcUncheckMurderPlayer(localPlayerId, localPlayerId, byte.MaxValue);
			Player.RpcCleanDeadBody(targetPlayerId);
			Player.RpcCleanDeadBody(localPlayerId);
		}
		else
		{
			float myTaskRate = Player.GetPlayerTaskGage(PlayerControl.LocalPlayer);
			if (myTaskRate < this.requiredTaskRate)
			{
				Player.RpcCleanDeadBody(targetPlayerId);
			}

			ExtremeRoleManager.RpcReplaceRole(
				localPlayerId, targetPlayerId,
				ExtremeRoleManager.ReplaceOperation.ItakoInherit);
		}

		ForceCleanUp();
	}

	public void ForceCleanUp()
	{
		this.targetBody = null;
		this.tmpTargetBody = null;
		this.activeTargetBodyId = byte.MaxValue;
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
		factory.Create0To100Percentage10StepOption(
			Option.RequiredTaskRate, defaultGage: 50);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;
		this.range = loader.GetValue<Option, float>(Option.Range);
		this.requiredTaskRate = loader.GetValue<Option, int>(Option.RequiredTaskRate) / 100.0f;
	}

	public static void InheritTargetRole(byte itakoPlayerId, byte targetPlayerId)
	{
		if (ExtremeRoleManager.TryGetRole(targetPlayerId, out var targetRole) &&
			ExtremeRoleManager.TryGetRole(itakoPlayerId, out var itakoRole) &&
			itakoRole is MultiAssignRoleBase multiItako)
		{
			SingleRoleBase inheritedRole = ExtractInheritedRole(targetRole);

			clonedNewRole(multiItako, itakoPlayerId, inheritedRole);
		}
	}

	public static SingleRoleBase ExtractInheritedRole(SingleRoleBase targetRole)
	{
		if (targetRole is MultiAssignRoleBase multiAssign)
		{
			if (multiAssign.OffsetInfo != null)
			{
				var cloned = multiAssign.Clone();
				if (cloned is MultiAssignRoleBase multiCloned)
				{
					multiCloned.AnotherRole = null;
				}
				return cloned;
			}
			if (multiAssign.AnotherRole is MultiAssignRoleBase anotherMulti && anotherMulti.OffsetInfo != null)
			{
				var cloned = anotherMulti.Clone();
				if (cloned is MultiAssignRoleBase multiCloned)
				{
					multiCloned.AnotherRole = null;
				}
				return cloned;
			}
			if (multiAssign.AnotherRole != null && multiAssign is Solo.VanillaRoleWrapper)
			{
				return multiAssign.AnotherRole.Clone();
			}
			if (multiAssign.AnotherRole != null)
			{
				return multiAssign.AnotherRole.Clone();
			}
		}

		return targetRole.Clone();
	}

	private static void clonedNewRole(MultiAssignRoleBase multiItako, byte itakoPlayerId, SingleRoleBase inheritedRole)
	{
		var prevAnotherRole = multiItako.AnotherRole;
		if (prevAnotherRole != null)
		{
			if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.PlayerId == itakoPlayerId)
			{
				if (prevAnotherRole is IRoleAbility prevAbility && prevAbility.Button != null)
				{
					prevAbility.Button.OnMeetingStart();
				}
				if (prevAnotherRole is IRoleResetMeeting meetingResetRole)
				{
					meetingResetRole.ResetOnMeetingStart();
				}
			}

			if (Player.TryGetPlayerControl(itakoPlayerId, out var itakoPlayer))
			{
				if (prevAnotherRole is IRoleSpecialReset specialResetRole)
				{
					specialResetRole.AllReset(itakoPlayer);
				}
			}
		}

		SingleRoleBase newRole = inheritedRole.Clone();
		newRole.SetControlId(multiItako.GameControlId);
		newRole.Initialize();

		ExtremeRoleManager.SetNewAnothorRole(itakoPlayerId, newRole);

		if (PlayerControl.LocalPlayer != null &&
			PlayerControl.LocalPlayer.PlayerId == itakoPlayerId)
		{
			if (newRole is IRoleAbility newAbility)
			{
				newAbility.CreateAbility();
				if (newAbility.Button != null)
				{
					newAbility.Button.HotKey = KeyCode.C;
				}
			}
			if (HudManager.InstanceExists)
			{
				HudManager.Instance.ReGridButtons();
			}
		}
	}
}
