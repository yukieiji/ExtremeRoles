using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using ExtremeRoles.Module.Meeting;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Factory;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class Legislator :
    SingleRoleBase,
	IRoleUpdate,
    IRoleMeetingButtonAbility,
    IRoleVoteModifier
{
    public enum Option
    {
		ChargeVotePerOtherVote,
		IncludeSkipVote,
		DefaultVoteNum,
	}

	public enum AbilityType : byte
    {
        SetVoteTarget,
        ChargeVote,
    }
    public int Order => (int)IRoleVoteModifier.ModOrder.LegislatorSpecialVote;

	public Sprite AbilityImage => Resources.UnityObjectLoader.LoadSpriteFromResources(
		Resources.ObjectPath.CaptainSpecialVote);

    private float curChargedVote;
    private float chargeVoteNum;
    private float defaultVote;
    private byte removeTarget;
	private bool includeSkip;

    private TMPro.TextMeshPro? meetingVoteText = null;
    private Dictionary<byte, SpriteRenderer>? voteCheckMark;

    public Legislator() : base(
		RoleCore.BuildImpostor(ExtremeRoleId.Legislator),
        true, false, true, true)
    { }

    public static void UseAbility(ref Hazel.MessageReader reader)
    {
        AbilityType type = (AbilityType)reader.ReadByte();
        byte rolePlayerId = reader.ReadByte();

		if (!ExtremeRoleManager.TryGetSafeCastedRole<Legislator>(rolePlayerId, out var legislator))
		{
			return;
		}

        switch (type)
        {
            case AbilityType.SetVoteTarget:
                byte targetPlayerId = reader.ReadByte();
				legislator.SetTargetVote(targetPlayerId);
                break;
            case AbilityType.ChargeVote:
				int multi = reader.ReadInt32();
				legislator.ChargeVote(multi);
                break;
            default:
                break;
        }

    }

    public void SetTargetVote(byte targetPlayerId)
    {
        this.removeTarget = targetPlayerId;
    }

    public void ChargeVote(int multi)
    {
        this.curChargedVote += (this.chargeVoteNum * multi);
    }

    public void ModifiedVote(
        byte rolePlayerId,
        ref Dictionary<byte, byte> voteTarget,
        ref Dictionary<byte, int> voteResult)
    {
        // 能力を使ってない
        if (this.removeTarget == PlayerVoteArea.HasNotVoted &&
			voteTarget.TryGetValue(rolePlayerId, out byte voteFor) &&
			!isInvalidVote(voteFor))
        {
			int allVoteExistLegislator = voteResult
				.Where(x => x.Key != rolePlayerId && (this.includeSkip || !isInvalidVote(x.Key)))
				.Sum(x => x.Value);

			using (var caller = RPCOperator.CreateCaller(
				   RPCOperator.Command.LegislatorAbility))
			{
				caller.WriteByte((byte)AbilityType.ChargeVote);
				caller.WriteByte(rolePlayerId);
				caller.WriteInt(allVoteExistLegislator);
			}
			this.ChargeVote(allVoteExistLegislator);
		}
        else
        {
            int removeVote = (int)Math.Floor(this.curChargedVote);

            if (!voteResult.TryGetValue(this.removeTarget, out int curVoteNum))
            {
				return;
            }
			int newVote = Math.Max(curVoteNum - removeVote, 0);
			if (newVote == 0)
			{
				voteResult.Remove(this.removeTarget);
			}
			else
			{
				voteResult[this.removeTarget] = newVote;
			}
		}
    }

    public IEnumerable<VoteInfo> GetModdedVoteInfo(
		VoteInfoCollector collector, NetworkedPlayerInfo rolePlayer)
    {
        if (this.removeTarget == PlayerVoteArea.HasNotVoted)
        {
			yield break;
        }

		int removeVoteNum = (int)Math.Floor(this.curChargedVote);
		foreach (var info in collector.Vote.OrderBy(x => RandomGenerator.Instance.Next()))
		{
			if (removeVoteNum <= 0)
			{
				yield break;
			}

			if (info.TargetId != this.removeTarget)
			{
				continue;
			}
			int delta = removeVoteNum - info.Count;
			if (delta >= 0)
			{
				yield return new VoteInfo(info.VoterId, info.TargetId, -info.Count);
				removeVoteNum = delta;
			}
			else
			{
				yield return new VoteInfo(info.VoterId, info.TargetId, -removeVoteNum);
				removeVoteNum = 0;
			}
		}
	}

    public void ResetModifier()
    {
        if (this.removeTarget != PlayerVoteArea.HasNotVoted)
        {
            this.curChargedVote = this.defaultVote;
        }
        this.removeTarget = PlayerVoteArea.HasNotVoted;
        this.voteCheckMark?.Clear();
    }

    public void ButtonMod(
        PlayerVoteArea instance, UiElement abilityButton)
		=> IRoleMeetingButtonAbility.DefaultButtonMod(instance, abilityButton, "legislatorSpecialVote");

    public Action CreateAbilityAction(PlayerVoteArea instance)
    {
        void setTarget()
        {
			if (this.voteCheckMark is null)
			{
				return;
			}

            using (var caller = RPCOperator.CreateCaller(
                    RPCOperator.Command.LegislatorAbility))
            {
                caller.WriteByte((byte)AbilityType.SetVoteTarget);
                caller.WriteByte(PlayerControl.LocalPlayer.PlayerId);
                caller.WriteByte(instance.TargetPlayerId);
            }
            this.SetTargetVote(
                instance.TargetPlayerId);

            foreach (var vote in this.voteCheckMark.Values)
            {
                if (vote != null)
                {
                    vote.gameObject.SetActive(false);
                }
            }

            if (!this.voteCheckMark.TryGetValue(
                    instance.TargetPlayerId,
                    out SpriteRenderer? checkMark) ||
                checkMark == null)
            {
                checkMark = UnityEngine.Object.Instantiate(
                    instance.Background, instance.LevelNumberText.transform);
                checkMark.name = $"legislator_SpecialVoteCheckMark_{instance.TargetPlayerId}";
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
        if (MeetingHud.Instance == null)
        {
			return;
        }

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
			"legislatorVoteStatus",
			isNotUseSpecialVote() ? Tr.GetString("cannotDo") : Tr.GetString("canDo"),
			this.curChargedVote);
		meetingVoteText.gameObject.SetActive(true);
	}

    protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateFloatOption(
            Option.ChargeVotePerOtherVote,
            0.2f, 0.1f, 100.0f, 0.1f,
            format: OptionUnit.VoteNum);
		factory.CreateBoolOption(Option.IncludeSkipVote, false);
        factory.CreateFloatOption(
            Option.DefaultVoteNum,
            0.0f, 0.0f, 100.0f, 0.1f,
            format: OptionUnit.VoteNum);
    }

    protected override void RoleSpecificInit()
    {

        var loader = this.Loader;

        this.chargeVoteNum = loader.GetValue<Option, float>(
           Option.ChargeVotePerOtherVote);
        this.defaultVote = loader.GetValue<Option, float>(
           Option.DefaultVoteNum);
		this.includeSkip = loader.GetValue<Option, bool>(
			Option.IncludeSkipVote);

        this.curChargedVote = this.defaultVote;

        this.removeTarget = PlayerVoteArea.HasNotVoted;
        this.voteCheckMark = new Dictionary<byte, SpriteRenderer>();
    }
    private bool isNotUseSpecialVote() => this.curChargedVote < 1.0f;

	private static bool isInvalidVote(byte voteFor)
		=> voteFor == PlayerVoteArea.DeadVote ||
			voteFor == PlayerVoteArea.HasNotVoted ||
			voteFor == PlayerVoteArea.MissedVote ||
			voteFor == PlayerVoteArea.SkippedVote;
}
