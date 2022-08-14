using ExtremeRoles.Helper;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Host
{
    public sealed partial class Xion
    {
        protected override void CommonInit()
        {
            return;
        }

        protected override void RoleSpecificInit()
        {
            return;
        }

        private void Init(byte xionPlayerId)
        {

            bool isXion(PlayerControl x) => x.PlayerId == xionPlayerId;

            PlayerControl.AllPlayerControls.RemoveAll(
                (Il2CppSystem.Predicate<PlayerControl>)isXion);

            if (xionPlayerId == CachedPlayerControl.LocalPlayer.PlayerId)
            {
                // まずは適当にダミーにタスクを突っ込む
                foreach (var dummy in bot)
                {
                    if (dummy == null || dummy.Data == null) { continue; }
                    GameData.PlayerInfo playerInfo = dummy.Data;
                    var (_, totalTask) = GameSystem.GetTaskInfo(playerInfo);
                    if (totalTask == 0)
                    {
                        var taskId = GameSystem.GetRandomCommonTaskId();
                        Logging.Debug($"PlayerName:{playerInfo.PlayerName}  AddTask:{taskId}");
                        GameSystem.SetTask(playerInfo, taskId);
                    }

                }
            }
        }

    }
}
