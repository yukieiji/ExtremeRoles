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

            Logger.GlobalInstance.Info(
                "IntroCutscene :: CoBegin() :: Starting intro cutscene", null);

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
                    while (!RoleAssignState.Instance.IsReady)
                    {
                        yield return null;
                    }
                }
                else
                {
                    yield return new WaitForSeconds(0.5f);
                }
                PlayerRoleAssignData.Instance.AllPlayerAssignToExRole();
            }
            else
            {
                // クライアントはここでオプション値を読み込むことで待ち時間を短く見せるトリック
                Module.CustomOption.AllOptionHolder.Load();

                // ラグも有るかもしれないで1フレーム待機
                yield return null;

                // ホスト以外はここまで処理済みである事を送信
                RoleAssignState.SetLocalPlayerReady();
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
