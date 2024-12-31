using Hazel;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.Neutral;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.Ability;



using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Module.SystemType;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral;

public sealed class Monika :
	SingleRoleBase,
	IRoleAutoBuildAbility
{

    public ExtremeAbilityButton? Button { get; set; }
	private MonikaTrashSystem? system;
	private byte targetPlayer;

	public Monika(): base(
        ExtremeRoleId.Monika,
        ExtremeRoleType.Neutral,
        ExtremeRoleId.Monika.ToString(),
        ColorPalette.MonikaChenChuWhowan,
        false, false, false, false)
    { }

	public void CreateAbility()
    {
		this.CreateNormalAbilityButton(
			"除外", UnityObjectLoader.LoadSpriteFromResources(ObjectPath.TestButton));
	}

	public bool IsAbilityUse()
	{
		this.targetPlayer = byte.MaxValue;
		var player = Player.GetClosestPlayerInRange(
			PlayerControl.LocalPlayer, this,
			1.3f);

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
		this.system.RpcAddTrash(this.targetPlayer);
		return true;
    }

	public void CleanUp()
	{
	}

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        IRoleAbility.CreateCommonAbilityOption(
            factory);
    }

    protected override void RoleSpecificInit()
    {
        var cate = this.Loader;

		this.system = ExtremeSystemTypeManager.Instance.CreateOrGet<MonikaTrashSystem>(
			ExtremeSystemType.MonikaTrashSystem);
    }

    public void ResetOnMeetingStart()
    {
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
        return;
    }

}
