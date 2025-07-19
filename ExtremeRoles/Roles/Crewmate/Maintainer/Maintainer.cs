using ExtremeRoles.Compat;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Module.Ability;




using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Maintainer : SingleRoleBase, IRoleAutoBuildAbility
{
    public ExtremeAbilityButton Button
    {
        get => this.maintenanceButton;
        set
        {
            this.maintenanceButton = value;
        }
    }

    private ExtremeAbilityButton maintenanceButton;

    public Maintainer() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Maintainer,
			ColorPalette.MaintainerBlue),
        false, true, false, false)
    { }

    public void CreateAbility()
    {
        this.CreateAbilityCountButton(
            "maintenance",
			Resources.UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.MaintainerRepair));
        this.Button.SetLabelToCrewmate();
    }

    public bool UseAbility()
    {
        GameSystem.RpcRepairAllSabotage();

        foreach (OpenableDoor door in ShipStatus.Instance.AllDoors)
        {
            DeconControl decon = door.GetComponentInChildren<DeconControl>();
            if (decon != null) { continue; }

            ShipStatus.Instance.RpcUpdateSystem(
                SystemTypes.Doors, (byte)(door.Id | 64));
            door.SetDoorway(true);
        }

        return true;
    }

    public bool IsAbilityUse()
    {
        bool sabotageActive = false;
        foreach (PlayerTask task in
            PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
        {
            if (task == null) { continue; }

            TaskTypes taskType = task.TaskType;
            if (CompatModManager.Instance.TryGetModMap(out var modMap) &&
				modMap.IsCustomSabotageTask(taskType))
            {
				sabotageActive = true;
				break;
			}

            if (PlayerTask.TaskIsEmergency(task) ||
				task.TaskType == TaskTypes.MushroomMixupSabotage)
            {
                sabotageActive = true;
                break;
            }
        }

        return sabotageActive && IRoleAbility.IsCommonUse();

    }

    public void ResetOnMeetingStart()
    {
        return;
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        return;
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        IRoleAbility.CreateAbilityCountOption(
            factory, 2, 10);
    }

    protected override void RoleSpecificInit()
    { }
}
