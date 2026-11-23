using System;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Meeting;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API.Interface;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral.FemmeFatale;

/// <summary>
/// ファムファタールの特殊投票処理と会議中の特殊投票周りの表示を管理するハンドラ
/// </summary>
public sealed class FemmeFataleSpecialVoteHandler(
	FemmeFataleStatusModel statusModel,
	float voteMultiplier)
{
    private readonly FemmeFataleStatusModel statusModel = statusModel;
    private readonly float voteMultiplier = voteMultiplier;

    private byte voteTarget = byte.MaxValue;
    private readonly Dictionary<byte, SpriteRenderer> voteCheckMark = [];
    private TMPro.TextMeshPro? meetingVoteText;

    public Sprite AbilityImage => UnityObjectLoader.LoadSpriteFromResources(
        ObjectPath.CaptainSpecialVote);

    public int GetSpecialVoteCount()
    {
        int livingJose = statusModel.GetLivingJosesAndCandidates().Count;
        return (int)Math.Floor(livingJose * voteMultiplier);
    }

    public bool IsBlockMeetingButtonAbility(PlayerVoteArea instance)
    {
        return GetSpecialVoteCount() < 1;
    }

    public void SetTargetVote(byte targetPlayerId)
    {
        this.voteTarget = targetPlayerId;
    }

    public void UpdateMeetingDisplay()
    {
        if (!GameProgressSystem.Is(GameProgressSystem.Progress.Meeting) ||
            voteMultiplier <= 0.0f)
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

        int addVote = GetSpecialVoteCount();
        meetingVoteText.text = Tr.GetString(
            "captainVoteStatus",
            addVote < 1 ? Tr.GetString("cannotDo") : Tr.GetString("canDo"),
            addVote);
        meetingVoteText.gameObject.SetActive(true);
    }

    public void ModifiedVote(
        ref Dictionary<byte, byte> voteTarget,
        ref Dictionary<byte, int> voteResult)
    {
        if (this.voteTarget == byte.MaxValue)
        {
            return;
        }

        int addVoteNum = GetSpecialVoteCount();
        if (addVoteNum <= 0)
        {
            return;
        }

        if (voteResult.TryGetValue(this.voteTarget, out int curVoteNum))
        {
            voteResult[this.voteTarget] = curVoteNum + addVoteNum;
        }
        else
        {
            voteResult[this.voteTarget] = addVoteNum;
        }
    }

    public IEnumerable<VoteInfo> GetModdedVoteInfo(
        VoteInfoCollector collector, NetworkedPlayerInfo rolePlayer)
    {
        if (this.voteTarget == byte.MaxValue)
        {
            yield break;
        }

        int addVoteNum = GetSpecialVoteCount();
        if (addVoteNum > 0)
        {
            yield return new VoteInfo(rolePlayer.PlayerId, this.voteTarget, addVoteNum);
        }
    }

    public void ResetModifier()
    {
        this.voteTarget = byte.MaxValue;
        this.voteCheckMark.Clear();
    }

    public void ButtonMod(PlayerVoteArea instance, UiElement abilityButton)
	{
		abilityButton.name = $"FemmeFataleSpecialVote_{instance.PlayerId}";
		var controllerHighlight = abilityButton.transform.FindChild("ControllerHighlight");
		if (controllerHighlight != null)
		{
			controllerHighlight.localScale *= new Vector2(1.25f, 1.25f);
		}
	}

    public Action CreateAbilityAction(PlayerVoteArea instance)
    {
        void setTarget()
        {
            using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.FemmeFataleAbility))
            {
                caller.WriteByte((byte)FemmeFataleRole.RpcOps.SpecialVote);
                caller.WriteByte(PlayerControl.LocalPlayer.PlayerId);
                caller.WriteByte(instance.PlayerId);
            }
            this.SetTargetVote(instance.PlayerId);

            foreach (SpriteRenderer vote in this.voteCheckMark.Values)
            {
                if (vote != null)
                {
                    vote.gameObject.SetActive(false);
                }
            }

            if (!this.voteCheckMark.TryGetValue(
                    instance.PlayerId,
                    out var checkMark) ||
                checkMark == null)
            {
                checkMark = UnityEngine.Object.Instantiate(
                    instance.Background, instance.LevelNumberText.transform);
                checkMark.name = $"femmeFatale_SpecialVoteCheckMark_{instance.PlayerId}";
                checkMark.sprite = UnityObjectLoader.LoadSpriteFromResources(
                    ObjectPath.CaptainSpecialVoteCheck);
                checkMark.transform.localPosition = new Vector3(7.25f, -0.5f, -3f);
                checkMark.transform.localScale = new Vector3(1.0f, 3.5f, 1.0f);
                checkMark.gameObject.layer = 5;
                this.voteCheckMark[instance.PlayerId] = checkMark;
            }
            checkMark.gameObject.SetActive(true);
        }

        return setTarget;
    }
}
