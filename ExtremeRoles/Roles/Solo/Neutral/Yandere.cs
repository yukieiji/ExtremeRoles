using System.Collections.Generic;

using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Neutral
{
    public class Yandere : SingleRoleBase, IRoleUpdate, IRoleMurderPlayerHock, IRoleSpecialSetUp
    {

        public enum YandereOption
        {
            
        }

        public Yandere(): base(
            ExtremeRoleId.Yandere,
            ExtremeRoleType.Neutral,
            ExtremeRoleId.Yandere.ToString(),
            ColorPalette.YandereVioletRed,
            false, false, true, false)
        { }


        public override bool IsSameTeam(SingleRoleBase targetRole)
        {
            var multiAssignRole = targetRole as MultiAssignRoleBase;

            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {
                    return this.IsSameTeam(multiAssignRole.AnotherRole);
                }
            }
            if (OptionHolder.Ship.IsSameNeutralSameWin)
            {
                return this.Id == targetRole.Id;
            }
            else
            {
                return (this.Id == targetRole.Id) && this.IsSameControlId(targetRole);
            }
        }

        public void Update(PlayerControl rolePlayer)
        {

        }

        public void HockMuderPlayer(
            PlayerControl source, PlayerControl target)
        {

        }

        public void IntroBeginSetUp()
        {

        }

        public void IntroEndSetUp()
        {
            return;
        }


        public override void RolePlayerKilledAction(
            PlayerControl rolePlayer,
            PlayerControl killerPlayer)
        {
           if (ExtremeRoleManager.GameRole[killerPlayer.PlayerId].IsImpostor())
           {
                this.IsWin = true;
           }
        }

        public bool UseAbility()
        {
            RPCOperator.Call(
                PlayerControl.LocalPlayer.NetId,
                RPCOperator.Command.AliceShipBroken,
                new List<byte> { PlayerControl.LocalPlayer.PlayerId });
            RPCOperator.AliceShipBroken(
                PlayerControl.LocalPlayer.PlayerId);

            return true;
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            

        }

        protected override void RoleSpecificInit()
        {
            var allOption = OptionHolder.AllOption;
            
        }
    }
}
