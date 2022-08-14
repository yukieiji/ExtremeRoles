using ExtremeRoles.Module;
using ExtremeRoles.Roles.API.Interface;
using System;
using UnityEngine;

namespace ExtremeRoles.Roles.Solo.Host
{
    public sealed partial class Xion : IRoleResetMeeting, IRoleUpdate
    {
        public void ResetOnMeetingEnd()
        {
            this.resetCoolTime();
        }

        public void ResetOnMeetingStart()
        {
            this.setButtonActive(false);
        }

        public void Update(PlayerControl rolePlayer)
        {
            this.keyBind();
            this.disableButton();
            this.buttonUpdate();
        }
    }
}
