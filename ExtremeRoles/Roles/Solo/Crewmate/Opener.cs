using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.RoleAbilityButton;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public class Opener : SingleRoleBase, IRoleAbility, IRoleUpdate
    {
        public enum OpenerOption
        {
            Range,
            ReduceRate,
            PlusAbility,
        }

        public RoleAbilityButtonBase Button
        {
            get => this.open;
            set
            {
                this.open = value;
            }
        }

        private RoleAbilityButtonBase open;
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
                Translation.GetString("openDoor"),
                Loader.CreateSpriteFromResources(
                    Path.OpenerOpenDoor));
            this.Button.SetLabelToCrewmate();
        }
        public bool UseAbility()
        {
            if (this.targetDoor == null) { return false; }
            
            ShipStatus.Instance.RpcRepairSystem(
                SystemTypes.Doors, this.targetDoor.Id | 64);
            this.targetDoor.SetDoorway(true);
            this.targetDoor = null;

            return true;
        }

        public bool IsAbilityUse()
        {
            if (ShipStatus.Instance == null) { return false; }
            
            this.targetDoor = null;

            foreach (var door in ShipStatus.Instance.AllDoors)
            {
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

            return this.IsCommonUse() && !this.targetDoor.Open;
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingEnd()
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

            this.Button.SetAbilityCoolTime(
                OptionHolder.AllOption[
                    GetRoleOptionId(RoleAbilityCommonOption.AbilityCoolTime)].GetValue() * rate);
            var button = (AbilityCountButton)this.Button;
            button.UpdateAbilityCount(button.CurAbilityNum + plusAbilityNum);
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
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
            this.range = OptionHolder.AllOption[
                GetRoleOptionId(OpenerOption.Range)].GetValue();
            this.reduceRate = OptionHolder.AllOption[
                GetRoleOptionId(OpenerOption.ReduceRate)].GetValue();
            this.plusAbilityNum = OptionHolder.AllOption[
                GetRoleOptionId(OpenerOption.PlusAbility)].GetValue();
        }
    }
}
