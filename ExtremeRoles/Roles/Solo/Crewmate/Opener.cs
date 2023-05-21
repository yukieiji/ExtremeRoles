using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityBehavior;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Opener : SingleRoleBase, IRoleAbility, IRoleUpdate
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
    private PlainDoor targetDoor;
    private bool isUpgraded = false;
    private float range;
    private float reduceRate;
    private int plusAbilityNum;

    public Opener() : base(
        ExtremeRoleId.Opener,
        ExtremeRoleType.Crewmate,
        ExtremeRoleId.Opener.ToString(),
        ColorPalette.OpenerSpringGreen,
        false, true, false, false)
    { }

    public void CreateAbility()
    {
        this.CreateAbilityCountButton(
            "openDoor",
            Loader.CreateSpriteFromResources(
                Path.OpenerOpenDoor));
        this.Button.SetLabelToCrewmate();
    }
    public bool UseAbility()
    {
        if (this.targetDoor == null) { return false; }
        
        CachedShipStatus.Instance.RpcRepairSystem(
            SystemTypes.Doors, this.targetDoor.Id | 64);
        this.targetDoor.SetDoorway(true);
        this.targetDoor = null;

        return true;
    }

    public bool IsAbilityUse()
    {
        if (CachedShipStatus.Instance == null) { return false; }
        
        this.targetDoor = null;

        foreach (PlainDoor door in CachedShipStatus.Instance.AllDoors)
        {
            DeconControl decon = door.GetComponentInChildren<DeconControl>();
            if (decon != null) { continue; }

            if (Vector3.Distance(
                    CachedPlayerControl.LocalPlayer.PlayerControl.transform.position,
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

        return this.IsCommonUse() && !this.targetDoor.Open;
    }

    public void ResetOnMeetingStart()
    {
        return;
    }

    public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
    {
        this.targetDoor = null;
        return;
    }
    public void Update(
        PlayerControl rolePlayer)
    {
        if (CachedShipStatus.Instance == null ||
            GameData.Instance == null) { return; }
        if (!CachedShipStatus.Instance.enabled ||
            this.Button == null) { return; }

        if (rolePlayer.Data.IsDead || rolePlayer.Data.Disconnected || this.isUpgraded) { return; }

        foreach (var task in rolePlayer.Data.Tasks)
        {
            if (!task.Complete) { return; }
        }

        this.isUpgraded = true;

        float rate = 1.0f - ((float)this.reduceRate / 100f);

        this.Button.Behavior.SetCoolTime(
            OptionManager.Instance.GetValue<float>(
                GetRoleOptionId(RoleAbilityCommonOption.AbilityCoolTime)) * rate);

        if (this.Button.Behavior is AbilityCountBehavior countBehavior)
        {
            countBehavior.SetAbilityCount(
                countBehavior.AbilityCount + plusAbilityNum);
        }
    }

    protected override void CreateSpecificOption(
        IOptionInfo parentOps)
    {
        this.CreateAbilityCountOption(
            parentOps, 2, 5);
        CreateFloatOption(
            OpenerOption.Range,
            2.0f, 0.5f, 5.0f, 0.1f,
            parentOps);
        CreateIntOption(
            OpenerOption.ReduceRate,
            45, 5, 95, 1,
            parentOps,
            format: OptionUnit.Percentage);
        CreateIntOption(
            OpenerOption.PlusAbility,
            5, 1, 10, 1,
            parentOps,
            format: OptionUnit.Shot);
    }

    protected override void RoleSpecificInit()
    {
        this.RoleAbilityInit();
        this.isUpgraded = false;
        this.range = OptionManager.Instance.GetValue<float>(
            GetRoleOptionId(OpenerOption.Range));
        this.reduceRate = OptionManager.Instance.GetValue<int>(
            GetRoleOptionId(OpenerOption.ReduceRate));
        this.plusAbilityNum = OptionManager.Instance.GetValue<int>(
            GetRoleOptionId(OpenerOption.PlusAbility));
    }
}
