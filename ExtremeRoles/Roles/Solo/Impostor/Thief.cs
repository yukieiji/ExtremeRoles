using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class Thief : SingleRoleBase, IRoleAbility
{
    public enum ThiefOption
	{
		Range,
        SetNum,
		SetTimeOffset,
		PickUpTimeOffset,
    }


    private GameData.PlayerInfo targetBody;
	private byte tagetPlayerId = byte.MaxValue;
	private float activeRange;

	public ExtremeAbilityButton Button { get; set; }

    public Thief() : base(
        ExtremeRoleId.Thief,
        ExtremeRoleType.Impostor,
        ExtremeRoleId.Thief.ToString(),
        Palette.ImpostorRed,
        true, false, true, true)
    { }

    public void CreateAbility()
    {
        this.CreateAbilityCountButton(
            "evolve",
            Loader.CreateSpriteFromResources(
                Path.EvolverEvolved),
            checkAbility: CheckAbility,
            abilityOff: CleanUp,
            forceAbilityOff: ForceCleanUp);
    }

    public bool IsAbilityUse()
    {
        this.targetBody = Player.GetDeadBodyInfo(
            this.activeRange);
        return this.IsCommonUse() && this.targetBody != null;
    }

    public void ForceCleanUp()
    {
        this.targetBody = null;
    }

    public void CleanUp()
    {
		ExtremeSystemTypeManager.RpcUpdateSystemOnlyHost(
			ExtremeSystemType.ThiefMeetingTimeChange,
			x =>
			{
				x.Write((byte)ThiefMeetingTimeStealSystem.Ops.Set);
			});
    }

    public bool CheckAbility()
    {
        this.targetBody = Player.GetDeadBodyInfo(
            this.activeRange);

        bool result;

        if (this.targetBody == null)
        {
            result = false;
        }
        else
        {
            result = this.tagetPlayerId == this.targetBody.PlayerId;
        }

        return result;
    }

    public bool UseAbility()
    {
        this.tagetPlayerId = this.targetBody.PlayerId;
        return true;
    }

    protected override void CreateSpecificOption(
        IOptionInfo parentOps)
    {
		CreateFloatOption(ThiefOption.Range, 0.1f, 1.8f, 3.6f, 0.1f, parentOps);
		this.CreateAbilityCountOption(
            parentOps, 2, 5, 2.0f);

		CreateIntOption(ThiefOption.SetNum, 5, 1, 10, 1, parentOps);
		CreateIntOption(ThiefOption.SetTimeOffset, 30, 10, 360, 5, parentOps);
		CreateIntOption(ThiefOption.PickUpTimeOffset, 6, 1, 60, 1, parentOps);
	}

    protected override void RoleSpecificInit()
    {
        this.RoleAbilityInit();

        var allOption = OptionManager.Instance;

		this.activeRange = allOption.GetValue<float>(GetRoleOptionId(ThiefOption.Range));

		ExtremeSystemTypeManager.Instance.TryAdd(
			ExtremeSystemType.ThiefMeetingTimeChange,
			new ThiefMeetingTimeStealSystem(
				allOption.GetValue<int>(GetRoleOptionId(ThiefOption.SetNum)),
				allOption.GetValue<int>(GetRoleOptionId(ThiefOption.SetTimeOffset)),
				-allOption.GetValue<int>(GetRoleOptionId(ThiefOption.PickUpTimeOffset))));
    }

    public void ResetOnMeetingStart()
    {
        return;
    }

    public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
    {
        return;
    }
}
