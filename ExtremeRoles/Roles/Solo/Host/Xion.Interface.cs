using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Host
{
    public sealed partial class Xion : IRoleResetMeeting, IRoleSpecialSetUp, IRoleUpdate
    {
        public void IntroBeginSetUp()
        {
            return;
        }

        public void IntroEndSetUp()
        {
            CreateButton();
        }

        public void ResetOnMeetingEnd()
        {
            this.resetCoolTime();
        }

        public void ResetOnMeetingStart()
        {
            this.setButtonActive(false);
            
            foreach (var body in this.dummyDeadBody)
            {
                if (body != null)
                {
                    UnityEngine.Object.Destroy(body);
                }
            }
            this.dummyDeadBody.Clear();
        }

        public void Update(PlayerControl rolePlayer)
        {
            if (this.isNoXion())
            {
                this.RpcForceEndGame();
                return;
            }
            var hudManager = FastDestroyableSingleton<HudManager>.Instance;

            hudManager.ShadowQuad.gameObject.SetActive(false);
            hudManager.Chat.gameObject.SetActive(true);

            this.keyBind();
            this.disableButton();
            this.buttonUpdate();
        }
    }
}
