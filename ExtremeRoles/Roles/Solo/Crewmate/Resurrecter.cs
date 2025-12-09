using System.Linq;

using UnityEngine;
using Hazel;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented;


namespace ExtremeRoles.Roles.Solo.Crewmate;

#nullable enable

public sealed class Resurrecter :
    SingleRoleBase,
    IRoleAwake<RoleTypes>,
    IRoleResetMeeting,
    IRoleOnRevive
{
    public override bool IsAssignGhostRole
    {
        get => false;
    }

    public bool IsAwake
    {
        get
        {
            return GameSystem.IsLobby || this.awakeRole;
        }
    }

    public RoleTypes NoneAwakeRole => RoleTypes.Crewmate;

    public enum ResurrecterOption
    {
        AwakeTaskGage,
        ResurrectTaskGage,
        ResurrectDelayTime,
        IsMeetingCoolResetOnResurrect,
        ResurrectMeetingCooltime,
        ResurrectTaskResetMeetingNum,
        ResurrectTaskResetGage,
        CanResurrectAfterDeath,
        CanResurrectOnExil,
    }

    public enum ResurrecterRpcOps : byte
    {
        UseResurrect,
		ResetFlash,
	}

    private bool awakeRole;
    private float awakeTaskGage;
    private bool awakeHasOtherVision;
    private float resurrectTaskGage;

    private bool canResurrect;
    private bool canResurrectAfterDeath;
    private bool canResurrectOnExil;
    private bool isResurrected;
    private bool isExild;

    private bool isActiveMeetingCount;
    private int meetingCounter;
    private int maxMeetingCount;

    private bool isMeetingCoolResetOnResurrect;
    private float meetingCoolDown;

    private float resetTaskGage;

	private readonly FullScreenFlasher flasher = new FullScreenFlasher(ColorPalette.ResurrecterBlue);
    private PlayerReviver? playerReviver;

    public Resurrecter() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Resurrecter,
			ColorPalette.ResurrecterBlue),
        false, true, false, false)
    {
    }

    public static void RpcAbility(ref MessageReader reader)
    {
		var ops = (ResurrecterRpcOps)reader.ReadByte();
        byte resurrecterPlayerId = reader.ReadByte();

		var resurrecter = ExtremeRoleManager.GetSafeCastedRole<Resurrecter>(resurrecterPlayerId);
		if (resurrecter == null)
		{ 
			return;
		}

		switch (ops)
        {
            case ResurrecterRpcOps.UseResurrect:
                UseResurrect(resurrecter);
                break;
			case ResurrecterRpcOps.ResetFlash:
				resurrecter.flasher.Hide();
				break;
			default:
                break;
        }
    }

    public static void UseResurrect(Resurrecter resurrecter)
    {
        resurrecter.isResurrected = true;
        resurrecter.isActiveMeetingCount = true;
    }

    public void ResetOnMeetingStart()
    {
        if (this.isActiveMeetingCount)
        {
            ++this.meetingCounter;
        }

        playerReviver?.Reset();

		using (var caller = RPCOperator.CreateCaller(
			RPCOperator.Command.ResurrecterRpc))
		{
			caller.WriteByte((byte)ResurrecterRpcOps.ResetFlash);
			caller.WriteByte(PlayerControl.LocalPlayer.PlayerId);
		}
		this.flasher.Hide();
	}

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        if (this.isActiveMeetingCount &&
            this.meetingCounter >= this.maxMeetingCount)
        {
            this.isActiveMeetingCount = false;
            this.meetingCounter = 0;
            replaceTask(PlayerControl.LocalPlayer);
        }
    }

    public void ReviveAction(PlayerControl player)
    {
        // リセット会議クールダウン
        if (this.isMeetingCoolResetOnResurrect)
        {
            ShipStatus.Instance.EmergencyCooldown = this.meetingCoolDown;
        }

        var role = ExtremeRoleManager.GetLocalPlayerRole();
        if (!role.CanKill() || role.IsCrewmate())
        {
            return;
        }

        flasher.Flash();
    }

    public string GetFakeOptionString() => "";

    public void Update(PlayerControl rolePlayer)
    {
        if (rolePlayer.Data.IsDead && this.infoBlock())
        {
            HudManager.Instance.Chat.gameObject.SetActive(false);
        }

        if (!rolePlayer.moveable ||
            !GameProgressSystem.IsTaskPhase)
        {
            return;
        }

        if ((!this.awakeRole ||
            (!this.canResurrect && !this.isResurrected)) &&
            rolePlayer.myTasks.Count != 0)
        {
            float taskGage = Player.GetPlayerTaskGage(rolePlayer);

            if (taskGage >= this.awakeTaskGage && !this.awakeRole)
            {
                this.awakeRole = true;
                this.HasOtherVision = this.awakeHasOtherVision;
            }
            if (taskGage >= this.resurrectTaskGage &&
                !this.canResurrect)
            {
                if (this.canResurrectAfterDeath &&
                    rolePlayer.Data.IsDead)
                {
					// 即復活を行うため、かなり短い時間で作ってReviveさせる
					var reviver = new PlayerReviver(0.0f, revive);
					reviver.Start(rolePlayer);
					reviver.Update();
                }
                else
                {
                    this.canResurrect = true;
                    this.isResurrected = false;
                }
            }
        }

		if (this.isResurrected)
		{
			return;
		}

		playerReviver?.Update();
	}

    public override string GetColoredRoleName(bool isTruthColor = false)
    {
        if (isTruthColor || IsAwake)
        {
            return base.GetColoredRoleName();
        }
        else
        {
            return Design.ColoredString(
                Palette.White, Tr.GetString(RoleTypes.Crewmate.ToString()));
        }
    }
    public override string GetFullDescription()
    {
        if (IsAwake)
        {
            return Tr.GetString(
                $"{this.Core.Id}FullDescription");
        }
        else
        {
            return Tr.GetString(
                $"{RoleTypes.Crewmate}FullDescription");
        }
    }

    public override string GetImportantText(bool isContainFakeTask = true)
    {
        if (IsAwake)
        {
            return base.GetImportantText(isContainFakeTask);

        }
        else
        {
            return Design.ColoredString(
                Palette.White,
                $"{this.GetColoredRoleName()}: {Tr.GetString("crewImportantText")}");
        }
    }

    public override string GetIntroDescription()
    {
        if (IsAwake)
        {
            return base.GetIntroDescription();
        }
        else
        {
            return Design.ColoredString(
                Palette.CrewmateBlue,
                PlayerControl.LocalPlayer.Data.Role.Blurb);
        }
    }

    public override Color GetNameColor(bool isTruthColor = false)
    {
        if (isTruthColor || IsAwake)
        {
            return base.GetNameColor(isTruthColor);
        }
        else
        {
            return Palette.White;
        }
    }

    public override void ExiledAction(
        PlayerControl rolePlayer)
    {
        if (this.isResurrected)
		{
			return;
		}

        this.isExild = true;

        // 追放でオフ時は以下の処理を行わない
        if (!this.canResurrectOnExil)
		{
			return;
		}

        if (this.canResurrect)
        {
            playerReviver?.Start(rolePlayer);
        }
        else if (!this.canResurrectAfterDeath)
        {
            this.isResurrected = true;
        }
    }

    public override void RolePlayerKilledAction(
        PlayerControl rolePlayer,
        PlayerControl killerPlayer)
    {
        if (this.isResurrected)
		{
			return;
		}

        this.isExild = false;

        if (this.canResurrect)
        {
            playerReviver?.Start(rolePlayer);
        }
        else if (!this.canResurrectAfterDeath)
        {
            this.isResurrected = true;
        }
    }

    public override bool IsBlockShowMeetingRoleInfo() => this.infoBlock();

    public override bool IsBlockShowPlayingRoleInfo() => this.infoBlock();


    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateIntOption(
            ResurrecterOption.AwakeTaskGage,
            100, 0, 100, 10,
            format: OptionUnit.Percentage);
        factory.CreateIntOption(
            ResurrecterOption.ResurrectTaskGage,
            100, 50, 100, 10,
            format: OptionUnit.Percentage);

        factory.CreateFloatOption(
            ResurrecterOption.ResurrectDelayTime,
            5.0f, 4.0f, 60.0f, 0.1f,
            format: OptionUnit.Second);

        var meetingResetOpt = factory.CreateBoolOption(
            ResurrecterOption.IsMeetingCoolResetOnResurrect,
            true);

        factory.CreateFloatOption(
            ResurrecterOption.ResurrectMeetingCooltime,
            20.0f, 5.0f, 60.0f, 0.25f,
            new InvertActive(meetingResetOpt),
            format: OptionUnit.Second);

        factory.CreateIntOption(
            ResurrecterOption.ResurrectTaskResetMeetingNum,
            1, 1, 5, 1);

        factory.CreateIntOption(
            ResurrecterOption.ResurrectTaskResetGage,
            20, 10, 50, 5,
            format: OptionUnit.Percentage);
        factory.CreateBoolOption(
            ResurrecterOption.CanResurrectAfterDeath,
            false);
        factory.CreateBoolOption(
            ResurrecterOption.CanResurrectOnExil,
            false);
    }

    protected override void RoleSpecificInit()
    {
        var loader = this.Loader;

        this.awakeTaskGage = loader.GetValue<ResurrecterOption, int>(
            ResurrecterOption.AwakeTaskGage) / 100.0f;
        this.resurrectTaskGage = loader.GetValue<ResurrecterOption, int>(
            ResurrecterOption.ResurrectTaskGage) / 100.0f;
        this.resetTaskGage = loader.GetValue<ResurrecterOption, int>(
            ResurrecterOption.ResurrectTaskResetGage) / 100.0f;

        this.playerReviver = new PlayerReviver(loader.GetValue<ResurrecterOption, float>(
            ResurrecterOption.ResurrectDelayTime), revive);
        this.canResurrectAfterDeath = loader.GetValue<ResurrecterOption, bool>(
            ResurrecterOption.CanResurrectAfterDeath);
        this.canResurrectOnExil = loader.GetValue<ResurrecterOption, bool>(
            ResurrecterOption.CanResurrectOnExil);
        this.maxMeetingCount = loader.GetValue<ResurrecterOption, int>(
            ResurrecterOption.ResurrectTaskResetMeetingNum);
        this.isMeetingCoolResetOnResurrect = loader.GetValue<ResurrecterOption, bool>(
            ResurrecterOption.IsMeetingCoolResetOnResurrect);
        this.meetingCoolDown = loader.GetValue<ResurrecterOption, float>(
            ResurrecterOption.ResurrectMeetingCooltime);

        this.awakeHasOtherVision = this.HasOtherVision;
        this.canResurrect = false;
        this.isResurrected = false;

        if (this.awakeTaskGage <= 0.0f)
        {
            this.awakeRole = true;
            this.HasOtherVision = this.awakeHasOtherVision;
        }
        else
        {
            this.awakeRole = false;
            this.HasOtherVision = false;
        }
    }

    private bool infoBlock()
    {
        // ・詳細
        // 復活を使用後に死亡 => 常に見える
        // 非復活可能状態でキル、死亡後復活出来ない => 常に見える
        // 非復活可能状態でキル、死亡後復活出来る => 復活できるまで見えない
        // 非復活可能状態で追放、死亡後復活できる => 見えない
        // 非復活可能状態で追放、死亡後復活出来ない => 常に見える
        // 復活可能状態で死亡か追放 => 見えない

        if (this.isResurrected)
        {
            return false;
        }
        else if (!this.canResurrect || this.isExild)
        {
            return this.canResurrectAfterDeath || (playerReviver?.IsReviving ?? false);
        }
        else
        {
            return playerReviver?.IsReviving ?? false;
        }
    }

    private void revive(PlayerControl rolePlayer)
    {
        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.ResurrecterRpc))
        {
            caller.WriteByte((byte)ResurrecterRpcOps.UseResurrect);
            caller.WriteByte(rolePlayer.PlayerId);
        }
        UseResurrect(this);
    }

    private void replaceTask(PlayerControl rolePlayer)
    {
        NetworkedPlayerInfo playerInfo = rolePlayer.Data;

        var shuffleTaskIndex = Enumerable.Range(
            0, playerInfo.Tasks.Count).ToList().OrderBy(
                item => RandomGenerator.Instance.Next()).ToList();

        int replaceTaskNum = 0;
        int maxReplaceTaskNum = Mathf.CeilToInt(playerInfo.Tasks.Count * this.resetTaskGage);

        foreach (int i in shuffleTaskIndex)
        {
            if (replaceTaskNum >= maxReplaceTaskNum)
			{
				break;
			}

			var task = playerInfo.Tasks[i];

            if (!task.Complete)
            {
				continue;
            }

			int taskIndex;
			int replaceTaskId = task.TypeId;

			if (ShipStatus.Instance.CommonTasks.Any(
				(NormalPlayerTask t) => t.Index == replaceTaskId))
			{
				taskIndex = GameSystem.GetRandomCommonTaskId();
			}
			else if (ShipStatus.Instance.LongTasks.Any(
				(NormalPlayerTask t) => t.Index == replaceTaskId))
			{
				taskIndex = GameSystem.GetRandomLongTask();
			}
			else if (ShipStatus.Instance.ShortTasks.Any(
				(NormalPlayerTask t) => t.Index == replaceTaskId))
			{
				taskIndex = GameSystem.GetRandomShortTaskId();
			}
			else
			{
				continue;
			}

			GameSystem.RpcReplaceNewTask(
				rolePlayer.PlayerId, i, taskIndex);

			++replaceTaskNum;
		}
    }
}
