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
            if (xionPlayerId == CachedPlayerControl.LocalPlayer.PlayerId)
            {
                // まずは適当にダミーにタスクを突っ込む
                foreach (var dummy in bot)
                {
                    if (dummy == null || dummy.Data == null) { continue; }
                    GameData.PlayerInfo playerInfo = dummy.Data;
                    var taskId = GameSystem.GetRandomCommonTaskId();
                    Logging.Debug($"PlayerName:{playerInfo.PlayerName}  AddTask:{taskId}");
                    GameSystem.SetTask(playerInfo, taskId);
                }
            }
        }

    }
}
