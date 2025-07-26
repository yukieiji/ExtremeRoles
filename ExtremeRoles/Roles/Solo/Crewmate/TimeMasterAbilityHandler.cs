using System.Collections;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Ability;
using UnityEngine;

namespace ExtremeRoles.Roles.Solo.Crewmate.TimeMaster
{
    public class TimeMasterAbilityHandler : IAbility, IKilledFrom
    {
        private TimeMasterStatusModel status;

        public TimeMasterAbilityHandler(TimeMasterStatusModel status)
        {
            this.status = status;
        }

        public bool TryKilledFrom(PlayerControl rolePlayer, PlayerControl fromPlayer)
        {
            if (status.isRewindTime) { return false; }

            if (status.isShieldOn)
            {
                using (var caller = RPCOperator.CreateCaller(
                    RPCOperator.Command.TimeMasterAbility))
                {
                    caller.WriteByte(rolePlayer.PlayerId);
                    caller.WriteByte((byte)TimeMasterRole.TimeMasterOps.RewindTime);
                }
                StartRewind(rolePlayer.PlayerId);

                return false;
            }

            return true;
        }

        public void StartRewind(byte playerId)
        {
            if (TimeMasterRole.history.BlockAddHistory) { return; }

            TimeMasterRole.history.StartCoroutine(coRewind(playerId, PlayerControl.LocalPlayer));
        }

        private IEnumerator coRewind(
            byte rolePlayerId, PlayerControl localPlayer)
        {

            // Enable rewind
            status.isRewindTime = true;

            TimeMasterRole.history.BlockAddHistory = true;

            // Screen Initialize
            if (status.rewindScreen == null)
            {
                status.rewindScreen = Object.Instantiate(
                     HudManager.Instance.FullScreen,
                     HudManager.Instance.transform);
                status.rewindScreen.transform.localPosition = new Vector3(0f, 0f, 20f);
                status.rewindScreen.gameObject.SetActive(true);
                status.rewindScreen.enabled = false;
                status.rewindScreen.color = new Color(0f, 0.5f, 0.8f, 0.3f);
            }
            // Screen On
            status.rewindScreen.enabled = true;

            // SetUp
            if (MapBehaviour.Instance)
            {
                MapBehaviour.Instance.Close();
            }
            if (Minigame.Instance)
            {
                Minigame.Instance.ForceClose();
            }

            float time = Time.fixedDeltaTime;

            // 梯子とか使っている最中に巻き戻すと色々とおかしくなる
            // => その処理が終わるまで待機、巻き戻しはその後
            //    ただし、処理が終わるまでの間の時間巻き戻し時間は短くなる
            int skipFrame = 0;
            if (!localPlayer.inVent && !localPlayer.moveable)
            {
                do
                {
                    yield return new WaitForSeconds(time);
                    ++skipFrame;
                }
                while (!localPlayer.moveable);
            }

            int rewindFrame = TimeMasterRole.history.Size - skipFrame;

            Logging.Debug($"History Size:{TimeMasterRole.history.Size}   SkipFrame:{skipFrame}");

            Vector3 prevPos = localPlayer.transform.position;
            Vector3 sefePos = prevPos;
            bool isNotSafePos = false;
            int frameCount = 0;

            // Rewind Main Process
            foreach (var hist in TimeMasterRole.history.GetAllHistory())
            {
                if (rewindFrame == frameCount) { break; }

                yield return new WaitForSeconds(time);

                if (localPlayer.PlayerId == rolePlayerId) { continue; }

                ++frameCount;

                localPlayer.moveable = false;

                Vector3 newPos = hist.Pos;

                if (localPlayer.Data.IsDead)
                {
                    localPlayer.transform.position = newPos;
                }
                else
                {
                    if (localPlayer.inVent)
                    {
                        foreach (Vent vent in ShipStatus.Instance.AllVents)
                        {
                            bool canUse;
                            bool couldUse;
                            vent.CanUse(
                                localPlayer.Data,
                                out canUse, out couldUse);
                            if (canUse)
                            {
                                localPlayer.MyPhysics.RpcExitVent(vent.Id);
                                vent.SetButtons(false);
                            }
                        }
                    }

                    Vector2 offset = localPlayer.Collider.offset;
                    Vector3 newTruePos = new Vector3(
                        newPos.x + offset.x,
                        newPos.y + offset.y,
                        newPos.z);
                    Vector3 prevTruePos = new Vector3(
                        prevPos.x + offset.x,
                        prevPos.y + offset.y,
                        newPos.z);

                    bool isAnythingBetween = PhysicsHelpers.AnythingBetween(
                        prevTruePos, newTruePos,
                        Constants.ShipAndAllObjectsMask, false);


                    // (間に何もない and 動ける) or ベント内だったの座標だった場合
                    // => 巻き戻しかつ、安全な座標を更新
                    if ((!isAnythingBetween && hist.CanMove) || hist.InVent)
                    {
                        localPlayer.transform.position = newPos;
                        prevPos = newPos;
                        sefePos = newPos;
                        isNotSafePos = false;
                    }
                    // 何か使っている時の座標(梯子、移動床等)
                    // => 巻き戻すが、安全ではない(壁抜けする)座標として記録
                    else if (hist.IsUsed)
                    {
                        localPlayer.transform.position = newPos;
                        prevPos = newPos;
                        isNotSafePos = true;
                    }
                    else
                    {
                        localPlayer.transform.position = prevPos;
                    }
                }
            }

            // 最後の巻き戻しが壁抜けする座標だった場合、壁抜けしない安全な場所に飛ばす
            if (isNotSafePos)
            {
                localPlayer.transform.position = sefePos;
            }

            localPlayer.moveable = true;
            status.isRewindTime = false;
            status.rewindScreen.enabled = false;

            TimeMasterRole.history.ResetAfterRewind();
        }
    }
}
