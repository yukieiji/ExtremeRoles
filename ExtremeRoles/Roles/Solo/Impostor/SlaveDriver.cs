using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

using ExtremeRoles.Module;
using System.Collections.Generic;

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

    protected override void CreateSpecificOption(
        IOptionInfo parentOps)
    {

		this.CreateAbilityCountOption(parentOps, 2, 10);
    }

    protected override void RoleSpecificInit()
    {
		this.RoleAbilityInit();
    }

    public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
    {
        return;
    }

    public void ResetOnMeetingStart()
    {

    }

	public void CreateAbility()
	{
		this.CreateAbilityCountButton(
			"NewTask", Resources.Loader.CreateSpriteFromResources(
				Resources.Path.TestButton));
	}

	public bool UseAbility()
	{
		throw new System.NotImplementedException();
	}

	public bool IsAbilityUse()
	{
		throw new System.NotImplementedException();
	}
}
