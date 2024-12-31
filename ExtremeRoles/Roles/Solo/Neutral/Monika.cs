using Hazel;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.Ability;

using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.ExtremeShipStatus;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral;

public sealed class Monika :
	SingleRoleBase,
	IRoleAutoBuildAbility
{
	public enum Ops
	{
		Range,
	}

    public ExtremeAbilityButton? Button { get; set; }
	private MonikaTrashSystem? system;
	private byte targetPlayer;
	private float range;

	public Monika(): base(
        ExtremeRoleId.Monika,
        ExtremeRoleType.Neutral,
        ExtremeRoleId.Monika.ToString(),
        ColorPalette.MonikaAsakisuou,
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
		this.targetPlayer = byte.MaxValue;
		var player = Player.GetClosestPlayerInRange(
			PlayerControl.LocalPlayer, this,
			this.range);

		if (player == null)
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
			this.system == null)
		{
			return false;
		}

		if (PlayerControl.LocalPlayer != null &&
			ExtremeRoleManager.TryGetRole(targetPlayer, out var role) &&
			role.Id is ExtremeRoleId.Monika)
		{
			// モニカに対して能力を使用したときは殺す
			Player.RpcUncheckMurderPlayer(
				PlayerControl.LocalPlayer.PlayerId,
				this.targetPlayer,
				byte.MaxValue);
			return true;
		}

		this.system.RpcAddTrash(this.targetPlayer);
		return true;
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        IRoleAbility.CreateCommonAbilityOption(
            factory);
		factory.CreateFloatOption(
			Ops.Range, 1.3f, 0.1f, 3.0f, 0.1f);
    }

    protected override void RoleSpecificInit()
    {
		this.system = ExtremeSystemTypeManager.Instance.CreateOrGet<MonikaTrashSystem>(
			ExtremeSystemType.MonikaTrashSystem);
		this.range = this.Loader.GetValue<Ops, float>(Ops.Range);
    }

    public void ResetOnMeetingStart()
    {
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
        return;
    }

}
