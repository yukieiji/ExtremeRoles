using System.Collections.Generic;

using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;

namespace ExtremeRoles.Module.ExtremeShipStatus
{
    public sealed partial class ExtremeShipStatus
    {
        public bool IsAssassinAssign => isAssignAssassin;
        public bool AssassinMeetingTrigger => this.assassinMeetingTrigger;
        public bool IsAssassinateMarin => this.isAssassinateMarin;
        public byte ExiledAssassinId => this.meetingCallAssassin;
        public byte IsMarinPlayerId => this.isTargetPlayerId;

        private bool isAssassinateMarin = false;
        private bool isAssignAssassin = false;
        private bool assassinMeetingTrigger = false;
        private byte meetingCallAssassin = byte.MaxValue;
        private byte isTargetPlayerId = byte.MaxValue;

        private Queue<byte> deadedAssassin = new Queue<byte>();

        private BakeryUnion union;

        public void AddGlobalActionRole(SingleRoleBase role)
        {
            var allOpt = OptionHolder.AllOption;

            switch (role.Id)
            {
                case ExtremeRoleId.Assassin:
                    this.isAssignAssassin = true;
                    break;
                case ExtremeRoleId.Bakary:
                    if (this.union != null) { return; }
                    this.union = this.status.AddComponent<BakeryUnion>();
                    this.union.SetCookingCondition(
                        allOpt[role.GetRoleOptionId(Bakary.BakaryOption.GoodBakeTime)].GetValue(),
                        allOpt[role.GetRoleOptionId(Bakary.BakaryOption.BadBakeTime)].GetValue(),
                        allOpt[role.GetRoleOptionId(Bakary.BakaryOption.ChangeCooking)].GetValue());
                    break;
                default:
                    break;
            }
        }

        // アサシン周り
        public void AddDeadAssasin(byte playerId)
        {
            this.deadedAssassin.Enqueue(playerId);
        }

        public bool TryGetDeadAssasin(out byte playerId)
        {
            playerId = default(byte);
            
            if (this.deadedAssassin.Count == 0) { return false; }

            playerId = this.deadedAssassin.Dequeue();

            return true;
        }

        public void AssassinMeetingTriggerOn(byte assassinPlayerId)
        {
            this.meetingCallAssassin = assassinPlayerId;
            this.assassinMeetingTrigger = true;
        }

        public void AssassinMeetingTriggerOff()
        {
            this.assassinMeetingTrigger = false;
        }

        public void SetAssassnateTarget(byte targetPlayerId)
        {
            this.isAssassinateMarin = ExtremeRoleManager.GameRole[
                targetPlayerId].Id == ExtremeRoleId.Marlin;
            this.isTargetPlayerId = targetPlayerId;
        }

        public bool isMarinPlayer(byte playerId) => playerId == this.isTargetPlayerId;

        private string getRoleAditionalInfo()
        {
            if (this.union == null) { return string.Empty; }

            return this.union.GetBreadBakingCondition();
        }

        private bool isShowRoleAditionalInfo()
        {
            if (this.union == null) { return false; }

            return this.union.IsEstablish();
        }
        
        private void resetOnMeetingGlobalAction()
        {
            this.union?.ResetTimer();
        }

        private void resetGlobalAction()
        {
            this.isAssassinateMarin = false;
            this.isAssignAssassin = false;
            this.assassinMeetingTrigger = false;
            this.meetingCallAssassin = byte.MaxValue;
            this.isTargetPlayerId = byte.MaxValue;
            this.deadedAssassin.Clear();
        }
    }
}
