using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.RoleAbilityButton;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public class Maintainer : SingleRoleBase, IRoleAbility
    {
        public RoleAbilityButtonBase Button
        {
            get => this.maintenanceButton;
            set
            {
                this.maintenanceButton = value;
            }
        }

        private RoleAbilityButtonBase maintenanceButton;

        public Maintainer() : base(
            ExtremeRoleId.Maintainer,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Maintainer.ToString(),
            ColorPalette.MaintainerBlue,
            false, true, false, false)
        { }

        public void CreateAbility()
        {
            this.CreateAbilityCountButton(
                Translation.GetString("maintenance"),
                Helper.Resources.LoadSpriteFromResources(
                    Resources.ResourcesPaths.TestButton, 115f));
        }

        public bool UseAbility()
        {
            foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
            {

                switch (task.TaskType)
                {
                    case TaskTypes.FixLights:

                        RPCOperator.Call(
                            PlayerControl.LocalPlayer.NetId,
                            RPCOperator.Command.FixLightOff);
                        RPCOperator.FixLightOff();
                        break;
                    case TaskTypes.RestoreOxy:
                        ShipStatus.Instance.RpcRepairSystem(
                            SystemTypes.LifeSupp, 0 | 64);
                        ShipStatus.Instance.RpcRepairSystem(
                            SystemTypes.LifeSupp, 1 | 64);
                        break;
                    case TaskTypes.ResetReactor:
                        ShipStatus.Instance.RpcRepairSystem(
                            SystemTypes.Reactor, 16);
                        break;
                    case TaskTypes.ResetSeismic:
                        ShipStatus.Instance.RpcRepairSystem(
                            SystemTypes.Laboratory, 16);
                        break;
                    case TaskTypes.FixComms:
                        ShipStatus.Instance.RpcRepairSystem(
                            SystemTypes.Comms, 16 | 0);
                        ShipStatus.Instance.RpcRepairSystem(
                            SystemTypes.Comms, 16 | 1);
                        break;
                    case TaskTypes.StopCharles:
                        ShipStatus.Instance.RpcRepairSystem(
                            SystemTypes.Reactor, 0 | 16);
                        ShipStatus.Instance.RpcRepairSystem(
                            SystemTypes.Reactor, 1 | 16);
                        break;
                    default:
                        break;
                }
            }

            foreach (var door in ShipStatus.Instance.AllDoors)
            {
                ShipStatus.Instance.RpcRepairSystem(
                    SystemTypes.Doors, door.Id | 64);
                door.SetDoorway(true);
            }


            return true;
        }

        public bool IsAbilityUse()
        {
            bool sabotageActive = false;
            foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
            {
                if (task.TaskType == TaskTypes.FixLights || 
                    task.TaskType == TaskTypes.RestoreOxy || 
                    task.TaskType == TaskTypes.ResetReactor || 
                    task.TaskType == TaskTypes.ResetSeismic || 
                    task.TaskType == TaskTypes.FixComms || 
                    task.TaskType == TaskTypes.StopCharles)
                {
                    sabotageActive = true;
                    break;
                }
            }

            return sabotageActive && this.IsCommonUse();

        }

        public void RoleAbilityResetOnMeetingStart()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            this.CreateAbilityCountOption(
                parentOps, 30);
        }

        protected override void RoleSpecificInit()
        {
            this.RoleAbilityInit();
        }
    }
}
