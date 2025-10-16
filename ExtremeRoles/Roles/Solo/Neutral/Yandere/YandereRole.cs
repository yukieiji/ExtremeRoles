
using System.Collections.Generic;

using UnityEngine;

using AmongUs.GameOptions;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.Neutral;

namespace ExtremeRoles.Roles.Solo.Neutral.Yandere;


using ExtremeRoles.Roles.API.Interface;

public sealed class YandereRole :
    SingleRoleBase,
    IRoleUpdate,
    IRoleMurderPlayerHook,
    IRoleResetMeeting,
	IRoleReviveHook,
	IRoleSpecialSetUp,
	ITryKillTo
{
    public PlayerControl OneSidedLover = null;

    private bool isOneSidedLoverShare = false;

    private bool hasOneSidedArrow = false;
    private Arrow oneSidedArrow = null;

    private int maxTargetNum = 0;

    private int targetKillReduceRate = 0;
    private float noneTargetKillMultiplier = 0.0f;
    private float defaultKillCool;

    private bool isRunawayNextMeetingEnd = false;
    private bool isRunaway = false;

    private float timeLimit = 0f;
    private float timer = 0f;

    private float blockTargetTime = 0f;
    private float blockTimer = 0.0f;

    private float setTargetRange;
    private float setTargetTime;

    private string oneSidePlayerName = string.Empty;

    private KillTarget target;

    private Dictionary<byte, float> progress;

    public sealed class KillTarget
    {
        private bool isUseArrow;

        private Dictionary<byte, Arrow> targetArrow = new Dictionary<byte, Arrow>();
        private Dictionary<byte, PlayerControl> targetPlayer = new Dictionary<byte, PlayerControl>();

        public KillTarget(
            bool isUseArrow)
        {
            this.isUseArrow = isUseArrow;
            targetArrow.Clear();
            targetPlayer.Clear();
        }

        public void Add(byte playerId)
        {
            var player = Player.GetPlayerControlById(playerId);

            targetPlayer.Add(playerId, player);
            if (isUseArrow)
            {
                targetArrow.Add(
                    playerId, new Arrow(
                        new Color32(
                            byte.MaxValue,
                            byte.MaxValue,
                            byte.MaxValue,
                            byte.MaxValue)));
            }
            else
            {
                targetArrow.Add(playerId, null);
            }
        }

        public void ArrowActivate(bool active)
        {
            foreach (var arrow in targetArrow.Values)
            {
                if (arrow != null)
                {
                    arrow.SetActive(active);
                }
            }
        }

        public bool IsContain(byte playerId) => targetPlayer.ContainsKey(playerId);

        public int Count() => targetPlayer.Count;

        public void Remove(byte playerId)
        {
            targetPlayer.Remove(playerId);

            if (targetArrow[playerId] != null)
            {
                targetArrow[playerId].Clear();
            }

            targetArrow.Remove(playerId);
        }

        public void Update()
        {
            List<byte> remove = new List<byte>();

            foreach (var (playerId, playerControl) in targetPlayer)
            {
                if (playerControl == null ||
                    playerControl.Data.Disconnected ||
                    playerControl.Data.IsDead)
                {
                    remove.Add(playerId);
                    continue;
                }

                if (targetArrow[playerId] != null && isUseArrow)
                {
                    targetArrow[playerId].UpdateTarget(
                        playerControl.GetTruePosition());
                }
            }

            foreach (byte playerId in remove)
            {
                Remove(playerId);
            }
        }
    }

    public enum YandereOption
    {
        TargetKilledKillCoolReduceRate,
        NoneTargetKilledKillCoolMultiplier,
        BlockTargetTime,
        SetTargetRange,
        SetTargetTime,
        MaxTargetNum,
        RunawayTime,
        HasOneSidedArrow,
        HasTargetArrow,
    }

    public YandereRole(): base(
		RoleCore.BuildNeutral(
			ExtremeRoleId.Yandere,
			ColorPalette.YandereVioletRed),
        false, false, true, false)
    { }

    public static void SetOneSidedLover(
        byte rolePlayerId, byte oneSidedLoverId)
    {
        var yandere = ExtremeRoleManager.GetSafeCastedRole<YandereRole>(rolePlayerId);
        if (yandere != null)
        {
            yandere.OneSidedLover = Player.GetPlayerControlById(oneSidedLoverId);
        }
    }

    public bool TryRolePlayerKillTo(
        PlayerControl rolePlayer, PlayerControl targetPlayer)
    {
        if (target.IsContain(targetPlayer.PlayerId))
        {
            KillCoolTime = defaultKillCool * (
                (100f - targetKillReduceRate) / 100f);
        }
        else
        {
            KillCoolTime = KillCoolTime * noneTargetKillMultiplier;
        }

        return true;
    }

    public override bool IsSameTeam(SingleRoleBase targetRole) =>
        this.IsNeutralSameTeam(targetRole);

    public override string GetIntroDescription() => string.Format(
        base.GetIntroDescription(),
        oneSidePlayerName);

    public override string GetImportantText(
        bool isContainFakeTask = true)
    {
        return string.Format(
            base.GetImportantText(isContainFakeTask),
            oneSidePlayerName);
    }

    public override string GetRolePlayerNameTag(
        SingleRoleBase targetRole, byte targetPlayerId)
    {
        if (OneSidedLover == null) { return ""; }

        if (targetPlayerId == OneSidedLover.PlayerId)
        {
            return Design.ColoredString(
                ColorPalette.YandereVioletRed,
                $" ♥");
        }

        return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
    }


    public void Update(PlayerControl rolePlayer)
    {
        if (!GameProgressSystem.IsTaskPhase ||
            progress == null ||
            !isOneSidedLoverShare && OneSidedLover == null)
        {
            return;
        }

        var playerInfo = GameData.Instance.GetPlayerById(
           rolePlayer.PlayerId);
        if (playerInfo.IsDead || playerInfo.Disconnected)
        {
            target.ArrowActivate(false);

            if (hasOneSidedArrow && oneSidedArrow != null)
            {
                oneSidedArrow.SetActive(false);
            }
            return;
        }

        if (OneSidedLover == null)
        {
            isRunaway = true;
            target.Update();
            updateCanKill();
            return;
        }

        if (!isOneSidedLoverShare)
        {
            isOneSidedLoverShare = true;

            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.YandereSetOneSidedLover))
            {
                caller.WriteByte(rolePlayer.PlayerId);
                caller.WriteByte(OneSidedLover.PlayerId);
            }
            SetOneSidedLover(
                rolePlayer.PlayerId,
                OneSidedLover.PlayerId);
            CanKill = false;
        }

        // 不必要なデータを削除、役職の人と想い人
        progress.Remove(rolePlayer.PlayerId);
        progress.Remove(OneSidedLover.PlayerId);


        Vector2 oneSideLoverPos = OneSidedLover.GetTruePosition();

        if (blockTimer > blockTargetTime)
        {
            // 片思いびとが生きてる時の処理
            if (!OneSidedLover.Data.Disconnected &&
                !OneSidedLover.Data.IsDead)
            {
                searchTarget(rolePlayer, oneSideLoverPos);
            }
            else
            {
                isRunaway = true;
            }
        }
        else
        {
            blockTimer += Time.deltaTime;
        }

        updateOneSideLoverArrow(oneSideLoverPos);

        target.Update();

        updateCanKill();

        checkRunawayNextMeeting();
    }

    public void HookMuderPlayer(
        PlayerControl source, PlayerControl target)
    {
        if (this.target.IsContain(target.PlayerId))
        {
            this.target.Remove(target.PlayerId);
        }
    }

    public void IntroBeginSetUp()
    {

        int playerIndex;

        do
        {
            playerIndex = Random.RandomRange(
                0, PlayerCache.AllPlayerControl.Count - 1);

            OneSidedLover = PlayerCache.AllPlayerControl[playerIndex];

            if (!ExtremeRoleManager.TryGetRole(OneSidedLover.PlayerId, out var role))
			{
				break;
			}

			var id = role.Core.Id;
            if (id != ExtremeRoleId.Yandere &&
                id != ExtremeRoleId.Xion)
			{
				break;
			}

            if (role is MultiAssignRoleBase multiAssignRole &&
				multiAssignRole.AnotherRole != null &&
				multiAssignRole.AnotherRole.Core.Id != ExtremeRoleId.Yandere)
            {
				break;
			}

        } while (true);
        oneSidePlayerName = OneSidedLover.Data.PlayerName;
        isOneSidedLoverShare = false;
    }

    public void IntroEndSetUp()
    {
        foreach(var player in GameData.Instance.AllPlayers.GetFastEnumerator())
        {
            progress.Add(player.PlayerId, 0.0f);
        }
        CanKill = false;
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        if (isRunawayNextMeetingEnd)
        {
            CanKill = true;
            isRunaway = true;
            isRunawayNextMeetingEnd = false;
        }
        target.ArrowActivate(true);

        if (hasOneSidedArrow && oneSidedArrow != null)
        {
            oneSidedArrow.SetActive(true);
        }
    }

    public void ResetOnMeetingStart()
    {
        KillCoolTime = defaultKillCool;
        CanKill = false;
        isRunaway = false;
        timer = 0f;

        target.ArrowActivate(false);

        if (hasOneSidedArrow && oneSidedArrow != null)
        {
            oneSidedArrow.SetActive(false);
        }

    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateIntOption(
            YandereOption.TargetKilledKillCoolReduceRate,
            85, 25, 99, 1,
            format: OptionUnit.Percentage);

        factory.CreateFloatOption(
            YandereOption.NoneTargetKilledKillCoolMultiplier,
            1.2f, 1.0f, 2.0f, 0.1f,
            format: OptionUnit.Multiplier);

        factory.CreateFloatOption(
            YandereOption.BlockTargetTime,
            5.0f, 0.5f, 30.0f, 0.5f,
			format: OptionUnit.Second);

        factory.CreateFloatOption(
            YandereOption.SetTargetRange,
            1.8f, 0.5f, 5.0f, 0.1f);

        factory.CreateFloatOption(
            YandereOption.SetTargetTime,
            2.0f, 0.1f, 7.5f, 0.1f,
            format: OptionUnit.Second);

        factory.CreateIntOption(
            YandereOption.MaxTargetNum,
            5, 1, GameSystem.VanillaMaxPlayerNum, 1);

        factory.CreateFloatOption(
            YandereOption.RunawayTime,
            60.0f, 25.0f, 120.0f, 0.25f,
            format: OptionUnit.Second);

        factory.CreateBoolOption(
            YandereOption.HasOneSidedArrow,
            true);

        factory.CreateBoolOption(
            YandereOption.HasTargetArrow,
            true);
    }

    protected override void RoleSpecificInit()
    {
		var cate = Loader;

        setTargetRange = cate.GetValue<YandereOption, float>(
            YandereOption.SetTargetRange);
        setTargetTime = cate.GetValue<YandereOption, float>(
            YandereOption.SetTargetTime);

        targetKillReduceRate = cate.GetValue<YandereOption, int>(
            YandereOption.TargetKilledKillCoolReduceRate);
        noneTargetKillMultiplier = cate.GetValue<YandereOption, float>(
            YandereOption.NoneTargetKilledKillCoolMultiplier);

        maxTargetNum = cate.GetValue<YandereOption, int>(
            YandereOption.MaxTargetNum);

        timer = 0.0f;
        timeLimit = cate.GetValue<YandereOption, float>(
            YandereOption.RunawayTime);

        blockTimer = 0.0f;
        blockTargetTime = cate.GetValue<YandereOption, float>(
            YandereOption.BlockTargetTime);

        hasOneSidedArrow = cate.GetValue<YandereOption, bool>(
            YandereOption.HasOneSidedArrow);
        target = new KillTarget(
			cate.GetValue<YandereOption, bool>(YandereOption.HasTargetArrow));

        progress = new Dictionary<byte, float>();

        if (HasOtherKillCool)
        {
            defaultKillCool = KillCoolTime;
        }
        else
        {
            defaultKillCool = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(
                FloatOptionNames.KillCooldown);
            HasOtherKillCool = true;
        }

        isRunaway = false;
        isOneSidedLoverShare = false;
        oneSidePlayerName = string.Empty;
    }

    private void checkRunawayNextMeeting()
    {
        if (isRunaway || isRunawayNextMeetingEnd) { return; }

        if (target.Count() == 0)
        {
            timer += Time.deltaTime;
            if (timer >= timeLimit)
            {
                isRunawayNextMeetingEnd = true;
            }
        }
        else
        {
            timer = 0.0f;
        }
    }

    private void searchTarget(
        PlayerControl rolePlayer,
        Vector2 pos)
    {
        foreach (NetworkedPlayerInfo playerInfo in
            GameData.Instance.AllPlayers.GetFastEnumerator())
        {

			byte playerId = playerInfo.PlayerId;

			if (!progress.TryGetValue(playerId, out float playerProgress))
			{
				continue;
			}

            if (!playerInfo.Disconnected &&
                !playerInfo.IsDead &&
                rolePlayer.PlayerId != playerId &&
                OneSidedLover.PlayerId != playerId &&
                !playerInfo.Object.inVent)
            {
                PlayerControl @object = playerInfo.Object;
                if (@object)
                {
                    Vector2 vector = @object.GetTruePosition() - pos;
                    float magnitude = vector.magnitude;

                    if (magnitude <= setTargetRange &&
                        !PhysicsHelpers.AnyNonTriggersBetween(
                            pos, vector.normalized,
                            magnitude, Constants.ShipAndObjectsMask))
                    {
                        playerProgress += Time.deltaTime;
                    }
                    else
                    {
                        playerProgress = 0.0f;
                    }
                }
            }
            else
            {
                playerProgress = 0.0f;
            }

            if (playerProgress >= setTargetTime &&
                !target.IsContain(playerId) &&
                target.Count() < maxTargetNum)
            {
                target.Add(playerId);
                progress.Remove(playerId);
            }
            else
            {
                progress[playerId] = playerProgress;
            }
        }
    }

    private void updateCanKill()
    {
        if (isRunaway)
        {
            CanKill = true;
            return;
        }

        CanKill = target.Count() > 0;
    }

    private void updateOneSideLoverArrow(Vector2 pos)
    {

        if (!hasOneSidedArrow) { return; }

        if (oneSidedArrow == null)
        {
            oneSidedArrow = new Arrow(Core.Color);
        }
        oneSidedArrow.UpdateTarget(pos);
    }

	public void HookRevive(PlayerControl revivePlayer)
	{
		lock(progress)
		{
			byte playerId = revivePlayer.PlayerId;
			if (progress.ContainsKey(playerId))
			{
				return;
			}
			progress.Add(playerId, 0.0f);
		}
	}
}
