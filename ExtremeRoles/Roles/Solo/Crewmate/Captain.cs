using System;
using System.Collections.Generic;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.Meeting;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Captain :
    SingleRoleBase,
    IRoleAwake<RoleTypes>,
    IRoleMeetingButtonAbility,
    IRoleVoteModifier
{
    public enum CaptainOption
    {
        AwakeTaskGage,
        ChargeVoteWhenSkip,
        AwakedDefaultVoteNum,
    }

    public enum AbilityType : byte
    {
        SetVoteTarget,
        ChargeVote,
    }

    public bool IsAwake
    {
        get
        {
            return GameSystem.IsLobby || this.awakeRole;
        }
    }

    public int Order => (int)IRoleVoteModifier.ModOrder.CaptainSpecialVote;

	public Sprite AbilityImage => Resources.UnityObjectLoader.LoadSpriteFromResources(
		Resources.ObjectPath.CaptainSpecialVote);

	public RoleTypes NoneAwakeRole => RoleTypes.Crewmate;

    private bool awakeRole;
    private float awakeTaskGage;
    private bool awakeHasOtherVision;

    private float curChargedVote;
    private float chargeVoteNum;
    private float defaultVote;
    private byte voteTarget;

    private TMPro.TextMeshPro meetingVoteText = null;
    private Dictionary<byte, SpriteRenderer> voteCheckMark;

    public Captain() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Captain,
			ColorPalette.CaptainLightKonjou),
        false, true, false, false)
    { }

    public static void UseAbility(ref Hazel.MessageReader reader)
    {
        AbilityType type = (AbilityType)reader.ReadByte();
        byte rolePlayerId = reader.ReadByte();

        Captain captain = ExtremeRoleManager.GetSafeCastedRole<Captain>(rolePlayerId);

        switch (type)
        {
            case AbilityType.SetVoteTarget:
                byte targetPlayerId = reader.ReadByte();
                if (captain == null)
                {
                    return;
                }
                captain.SetTargetVote(targetPlayerId);
                break;
            case AbilityType.ChargeVote:
                if (captain == null)
                {
                    return;
                }
                captain.ChargeVote();
                break;
            default:
                break;
        }

    }

    public void SetTargetVote(byte targetPlayerId)
    {
        this.voteTarget = targetPlayerId;
    }

    public void ChargeVote()
    {
        this.curChargedVote = this.curChargedVote + this.chargeVoteNum;
    }

    public void ModifiedVote(
        byte rolePlayerId,
        ref Dictionary<byte, byte> voteTarget,
        ref Dictionary<byte, int> voteResult)
    {
        // 能力を使ってない
        if (this.voteTarget == PlayerVoteArea.HasNotVoted)
        {
			byte voteFor = voteTarget[rolePlayerId];

			if (voteFor == PlayerVoteArea.DeadVote)
			{
				return;
			}

            // スキップ => チャージ
            if (voteFor == PlayerVoteArea.HasNotVoted ||
                voteFor == PlayerVoteArea.MissedVote ||
                voteFor == PlayerVoteArea.SkippedVote)
            {
                using (var caller = RPCOperator.CreateCaller(
                    RPCOperator.Command.CaptainAbility))
                {
                    caller.WriteByte((byte)AbilityType.ChargeVote);
                    caller.WriteByte(rolePlayerId);
                }
                this.ChargeVote();
            }
        }
        else
        {
            int curVoteNum;
            int addVoteNum = (int)Math.Floor(this.curChargedVote);

            if (voteResult.TryGetValue(this.voteTarget, out curVoteNum))
            {
                voteResult[this.voteTarget] = curVoteNum + addVoteNum;
            }
            else
            {
                voteResult[this.voteTarget] = addVoteNum;
            }
        }
    }
    public IEnumerable<VoteInfo> GetModdedVoteInfo(NetworkedPlayerInfo rolePlayer)
    {
        if (this.voteTarget == PlayerVoteArea.HasNotVoted)
        {
			yield break;
        }

		int addVoteNum = (int)Math.Floor(this.curChargedVote);
		if (addVoteNum > 0)
		{
			yield return new VoteInfo(rolePlayer.PlayerId, this.voteTarget, addVoteNum);
		}
	}

    public void ResetModifier()
    {
        if (this.voteTarget != PlayerVoteArea.HasNotVoted)
        {
            this.curChargedVote = this.defaultVote;
        }
        this.voteTarget = PlayerVoteArea.HasNotVoted;
        this.voteCheckMark.Clear();
    }

    public void ButtonMod(
        PlayerVoteArea instance, UiElement abilityButton)
		=> IRoleMeetingButtonAbility.DefaultButtonMod(instance, abilityButton, "captainSpecialVote");

    public Action CreateAbilityAction(PlayerVoteArea instance)
    {
        void setTarget()
        {
            using (var caller = RPCOperator.CreateCaller(
                    RPCOperator.Command.CaptainAbility))
            {
                caller.WriteByte((byte)AbilityType.SetVoteTarget);
                caller.WriteByte(PlayerControl.LocalPlayer.PlayerId);
                caller.WriteByte(instance.TargetPlayerId);
            }
            this.SetTargetVote(
                instance.TargetPlayerId);

            foreach (SpriteRenderer vote in this.voteCheckMark.Values)
            {
                if (vote != null)
                {
                    vote.gameObject.SetActive(false);
                }
            }

            if (!this.voteCheckMark.TryGetValue(
                    instance.TargetPlayerId,
                    out SpriteRenderer checkMark) ||
                checkMark == null)
            {
                checkMark = UnityEngine.Object.Instantiate(
                    instance.Background, instance.LevelNumberText.transform);
                checkMark.name = $"captain_SpecialVoteCheckMark_{instance.TargetPlayerId}";
                checkMark.sprite = Resources.UnityObjectLoader.LoadSpriteFromResources(
                    Resources.ObjectPath.CaptainSpecialVoteCheck);
                checkMark.transform.localPosition = new Vector3(7.25f, -0.5f, -3f);
                checkMark.transform.localScale = new Vector3(1.0f, 3.5f, 1.0f);
                checkMark.gameObject.layer = 5;
                this.voteCheckMark[instance.TargetPlayerId] = checkMark;
            }
            checkMark.gameObject.SetActive(true);
        }

        return setTarget;
    }

    public string GetFakeOptionString() => "";

    public bool IsBlockMeetingButtonAbility(PlayerVoteArea instance) => isNotUseSpecialVote();

    public void Update(PlayerControl rolePlayer)
    {
        if (!this.awakeRole &&
			Player.GetPlayerTaskGage(rolePlayer) >= this.awakeTaskGage)
        {
			this.awakeRole = true;
			this.HasOtherVision = this.awakeHasOtherVision;
		}

        if (this.IsAwake && MeetingHud.Instance)
        {
            if (meetingVoteText == null)
            {
                meetingVoteText = UnityEngine.Object.Instantiate(
                    HudManager.Instance.TaskPanel.taskText,
                    MeetingHud.Instance.transform);
                meetingVoteText.alignment = TMPro.TextAlignmentOptions.BottomLeft;
                meetingVoteText.transform.position = Vector3.zero;
                meetingVoteText.transform.localPosition = new Vector3(-2.85f, 3.15f, -20f);
                meetingVoteText.transform.localScale *= 0.9f;
                meetingVoteText.color = Palette.White;
                meetingVoteText.gameObject.SetActive(false);
            }

            meetingVoteText.text = Tr.GetString(
				"captainVoteStatus",
				isNotUseSpecialVote() ? Tr.GetString("cannotDo") : Tr.GetString("canDo"),
				this.curChargedVote);
            meetingVoteText.gameObject.SetActive(true);
        }
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
                Palette.White,
                Tr.GetString(RoleTypes.Crewmate.ToString()));
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

    protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateIntOption(
            CaptainOption.AwakeTaskGage,
            70, 0, 100, 10,
            format: OptionUnit.Percentage);
        factory.CreateFloatOption(
            CaptainOption.ChargeVoteWhenSkip,
            0.7f, 0.1f, 100.0f, 0.1f,
            format: OptionUnit.VoteNum);
        factory.CreateFloatOption(
            CaptainOption.AwakedDefaultVoteNum,
            0.0f, 0.0f, 100.0f, 0.1f,
            format: OptionUnit.VoteNum);
    }

    protected override void RoleSpecificInit()
    {

        var loader = this.Loader;

        this.chargeVoteNum = loader.GetValue<CaptainOption, float>(
           CaptainOption.ChargeVoteWhenSkip);
        this.defaultVote = loader.GetValue<CaptainOption, float>(
           CaptainOption.AwakedDefaultVoteNum);
        this.awakeTaskGage = loader.GetValue<CaptainOption, int>(
           CaptainOption.AwakeTaskGage) / 100.0f;

        this.awakeHasOtherVision = this.HasOtherVision;
        this.curChargedVote = this.defaultVote;

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

        this.voteTarget = byte.MaxValue;
        this.voteCheckMark = new Dictionary<byte, SpriteRenderer>();
    }
    private bool isNotUseSpecialVote() => !this.IsAwake || this.curChargedVote < 1.0f;
}
