using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.CustomOption.Factory.Old;

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Opener : SingleRoleBase, IRoleAutoBuildAbility, IRoleUpdate
{
    public enum OpenerOption
    {
        Range,
        ReduceRate,
        PlusAbility,
    }

    public ExtremeAbilityButton Button
    {
        get => this.open;
        set
        {
            this.open = value;
        }
    }

    private ExtremeAbilityButton open;
    private OpenableDoor targetDoor;
    private bool isUpgraded = false;
    private float range;
    private float reduceRate;
    private int plusAbilityNum;
	private float abilityCoolTime;

    public Opener() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Opener,
			ColorPalette.OpenerSpringGreen),
        false, true, false, false)
    { }

    public void CreateAbility()
    {
        this.CreateAbilityCountButton(
            "openDoor",
			Resources.UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.OpenerOpenDoor));
        this.Button.SetLabelToCrewmate();
    }
    public bool UseAbility()
    {
        if (this.targetDoor == null) { return false; }

        ShipStatus.Instance.RpcUpdateSystem(
            SystemTypes.Doors, (byte)(this.targetDoor.Id | 64));
        this.targetDoor.SetDoorway(true);
        this.targetDoor = null;

        return true;
    }

    public bool IsAbilityUse()
    {
        if (ShipStatus.Instance == null) { return false; }

        this.targetDoor = null;

        foreach (OpenableDoor door in ShipStatus.Instance.AllDoors)
        {
            DeconControl decon = door.GetComponentInChildren<DeconControl>();
            if (decon != null) { continue; }

            if (Vector3.Distance(
                    PlayerControl.LocalPlayer.transform.position,
                    door.transform.position) < this.range)
            {
                this.targetDoor = door;
                break;
            }

        }
        if (this.targetDoor == null)
        {
            return false;
        }

        return IRoleAbility.IsCommonUse() && !this.targetDoor.IsOpen;
    }

    public void ResetOnMeetingStart()
    {
        return;
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        this.targetDoor = null;
        return;
    }
    public void Update(
        PlayerControl rolePlayer)
    {
        if (ShipStatus.Instance == null ||
            GameData.Instance == null) { return; }
        if (!ShipStatus.Instance.enabled ||
            this.Button == null) { return; }

        if (rolePlayer.Data.IsDead || rolePlayer.Data.Disconnected || this.isUpgraded) { return; }

        foreach (var task in rolePlayer.Data.Tasks)
        {
            if (!task.Complete) { return; }
        }

        this.isUpgraded = true;

        float rate = 1.0f - ((float)this.reduceRate / 100f);

        this.Button.Behavior.SetCoolTime(this.abilityCoolTime * rate);

        if (this.Button.Behavior is ICountBehavior countBehavior)
        {
            countBehavior.SetAbilityCount(
                countBehavior.AbilityCount + plusAbilityNum);
        }
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        IRoleAbility.CreateAbilityCountOption(
            factory, 2, 5);
        factory.CreateFloatOption(
            OpenerOption.Range,
            2.0f, 0.5f, 5.0f, 0.1f);
        factory.CreateIntOption(
            OpenerOption.ReduceRate,
            45, 5, 95, 1,
            format: OptionUnit.Percentage);
        factory.CreateIntOption(
            OpenerOption.PlusAbility,
            5, 1, 10, 1,
            format: OptionUnit.Shot);
    }

    protected override void RoleSpecificInit()
    {
        this.isUpgraded = false;

		var loader = this.Loader;
        this.range = loader.GetValue<OpenerOption, float>(
            OpenerOption.Range);
        this.reduceRate = loader.GetValue<OpenerOption, int>(
            OpenerOption.ReduceRate);
        this.plusAbilityNum = loader.GetValue<OpenerOption, int>(
            OpenerOption.PlusAbility);
		this.abilityCoolTime = loader.GetValue<RoleAbilityCommonOption, float>(
			RoleAbilityCommonOption.AbilityCoolTime);
    }
}
