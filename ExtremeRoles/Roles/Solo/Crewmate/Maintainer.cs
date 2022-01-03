using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public class Maintainer : SingleRoleBase, IRoleAbility
    {
        public RoleAbilityButton Button
        {
            get => this.maintenanceButton;
            set
            {
                this.maintenanceButton = value;
            }
        }

        private RoleAbilityButton maintenanceButton;

        public Maintainer() : base(
            ExtremeRoleId.Maintainer,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Maintainer.ToString(),
            ColorPalette.MaintainerBlue,
            false, true, false, false)
        { }

        public void CreateAbility()
        {
            this.CreateAbilityButton(
                Translation.GetString("maintenance"),
                Helper.Resources.LoadSpriteFromResources(
                    Resources.ResourcesPaths.TestButton, 115f),
                abilityNum: 30);
        }

        public bool UseAbility()
        {
            foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
            {
                if (task.TaskType == TaskTypes.FixLights)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                        PlayerControl.LocalPlayer.NetId,
                        (byte)RPCOperator.Command.FixLightOff,
                        Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCOperator.FixLightOff();
                }
                else if (task.TaskType == TaskTypes.RestoreOxy)
                {
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 0 | 64);
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 1 | 64);
                }
                else if (task.TaskType == TaskTypes.ResetReactor)
                {
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 16);
                }
                else if (task.TaskType == TaskTypes.ResetSeismic)
                {
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.Laboratory, 16);
                }
                else if (task.TaskType == TaskTypes.FixComms)
                {
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 0);
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 1);
                }
                else if (task.TaskType == TaskTypes.StopCharles)
                {
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 0 | 16);
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 1 | 16);
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
            this.CreateRoleAbilityOption(
                parentOps, maxAbilityCount:30);
        }

        protected override void RoleSpecificInit()
        {
            this.RoleAbilityInit();
        }
    }
}
