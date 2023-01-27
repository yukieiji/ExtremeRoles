using System.Collections;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.RoleAssign;

namespace ExtremeRoles.GameMode.IntroRunner
{
    public interface IIntroRunner
    {
        public IEnumerator CoRunModeIntro(IntroCutscene instance, GameObject roleAssignText);

        public IEnumerator CoRunIntro(IntroCutscene instance)
        {
            GameObject roleAssignText = new GameObject("roleAssignText");
            var text = roleAssignText.AddComponent<Module.CustomMonoBehaviour.LoadingText>();
            text.SetFontSize(3.0f);
            text.SetMessage(Translation.GetString("roleAssignNow"));

            roleAssignText.gameObject.SetActive(true);

            yield return waitRoleAssign();

            SoundManager.Instance.PlaySound(instance.IntroStinger, false, 1f);

            yield return CoRunModeIntro(instance, roleAssignText);
            
            Object.Destroy(instance.gameObject);
            
            yield break;
        }

        private static IEnumerator waitRoleAssign()
        {
            if (AmongUsClient.Instance.AmHost)
            {
                if (AmongUsClient.Instance.NetworkMode != NetworkModes.LocalGame ||
                    !isAllPlyerDummy())
                {
                    // ホストは全員の処理が終わるまで待つ
                    while (!Patches.Manager.RoleManagerSelectRolesPatch.IsReady)
                    {
                        yield return null;
                    }
                }
                else
                {
                    yield return new WaitForSeconds(0.5f);
                }
                Patches.Manager.RoleManagerSelectRolesPatch.AllPlayerAssignToExRole();
            }
            else
            {
                // ホスト以外はここまで処理済みである事を送信
                Patches.Manager.RoleManagerSelectRolesPatch.SetLocalPlayerReady();
            }

            // バニラの役職アサイン後すぐこの処理が走るので全員の役職が入るまで待機
            while (!RoleAssignState.Instance.IsRoleSetUpEnd)
            {
                yield return null;
            }
            yield break;
        }

        private static bool isAllPlyerDummy()
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId) { continue; }

                if (!player.GetComponent<DummyBehaviour>().enabled)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
