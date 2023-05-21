using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

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
        ExtremeRoleId.Captain,
        ExtremeRoleType.Crewmate,
        ExtremeRoleId.Captain.ToString(),
        ColorPalette.CaptainLightKonjou,
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
                if (captain == null) { return; }
                captain.SetTargetVote(targetPlayerId);
                break;
            case AbilityType.ChargeVote:
                if (captain == null) { return; }
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
        byte voteFor = voteTarget[rolePlayerId];

        // 能力を使ってない
        if (this.voteTarget == byte.MaxValue)
        {
            // スキップ => チャージ
            if (voteFor == 252 ||
                voteFor == 253 ||
                voteFor == 254 || 
                voteFor == byte.MaxValue)
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
    public void ModifiedVoteAnime(
        MeetingHud instance,
        GameData.PlayerInfo rolePlayer,
        ref Dictionary<byte, int> voteIndex)
    {
        PlayerVoteArea pva = instance.playerStates.FirstOrDefault(
            x => x.TargetPlayerId == this.voteTarget);
        
        if (pva == null) { return; }

        if (!voteIndex.TryGetValue(pva.TargetPlayerId, out int startIndex))
        {
            startIndex = 0;
        }

        int addVoteNum = (int)Math.Floor(this.curChargedVote);
        for (int i = 0; i < addVoteNum; ++i)
        {
            instance.BloopAVoteIcon(rolePlayer, startIndex + i, pva.transform);
        }
        voteIndex[pva.TargetPlayerId] = startIndex + addVoteNum;
    }

    public void ResetModifier()
    {
        if (this.voteTarget != byte.MaxValue)
        {
            this.curChargedVote = this.defaultVote;
        }
        this.voteTarget = byte.MaxValue;
        this.voteCheckMark.Clear();
    }

    public void ButtonMod(
        PlayerVoteArea instance, UiElement abilityButton)
    {
        abilityButton.name = $"captainSpecialVote_{instance.TargetPlayerId}";
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
            using (var caller = RPCOperator.CreateCaller(
                    RPCOperator.Command.CaptainAbility))
            {
                caller.WriteByte((byte)AbilityType.SetVoteTarget);
                caller.WriteByte(CachedPlayerControl.LocalPlayer.PlayerId);
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
                checkMark.sprite = Resources.Loader.CreateSpriteFromResources(
                    Resources.Path.CaptainSpecialVoteCheck);
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

    public bool IsBlockMeetingButtonAbility(PlayerVoteArea instance) => 
        instance.TargetPlayerId == 253 || isNotUseSpecialVote();

    public void SetSprite(SpriteRenderer render)
    {
        render.sprite = Resources.Loader.CreateSpriteFromResources(
            Resources.Path.CaptainSpecialVote);
        render.transform.localScale *= new Vector2(0.625f, 0.625f);
    }

    public void Update(PlayerControl rolePlayer)
    {
        if (!this.awakeRole)
        {
            if (Player.GetPlayerTaskGage(rolePlayer) >= this.awakeTaskGage)
            {
                this.awakeRole = true;
                this.HasOtherVision = this.awakeHasOtherVision;
            }
        }
        if (this.IsAwake && MeetingHud.Instance)
        {
            if (meetingVoteText == null)
            {
                meetingVoteText = UnityEngine.Object.Instantiate(
                    FastDestroyableSingleton<HudManager>.Instance.TaskPanel.taskText,
                    MeetingHud.Instance.transform);
                meetingVoteText.alignment = TMPro.TextAlignmentOptions.BottomLeft;
                meetingVoteText.transform.position = Vector3.zero;
                meetingVoteText.transform.localPosition = new Vector3(-2.85f, 3.15f, -20f);
                meetingVoteText.transform.localScale *= 0.9f;
                meetingVoteText.color = Palette.White;
                meetingVoteText.gameObject.SetActive(false);
            }

            meetingVoteText.text = string.Format(
                Translation.GetString("captainVoteStatus"),
                isNotUseSpecialVote() ? Translation.GetString("cannotDo") : Translation.GetString("canDo"),
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
            return Design.ColoedString(
                Palette.White,
                Translation.GetString(RoleTypes.Crewmate.ToString()));
        }
    }
    public override string GetFullDescription()
    {
        if (IsAwake)
        {
            return Translation.GetString(
                $"{this.Id}FullDescription");
        }
        else
        {
            return Translation.GetString(
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
            return Design.ColoedString(
                Palette.White,
                $"{this.GetColoredRoleName()}: {Translation.GetString("crewImportantText")}");
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
            return Design.ColoedString(
                Palette.CrewmateBlue,
                CachedPlayerControl.LocalPlayer.Data.Role.Blurb);
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

    protected override void CreateSpecificOption(IOptionInfo parentOps)
    {
        CreateIntOption(
            CaptainOption.AwakeTaskGage,
            70, 0, 100, 10,
            parentOps,
            format: OptionUnit.Percentage);
        CreateFloatOption(
            CaptainOption.ChargeVoteWhenSkip,
            0.7f, 0.1f, 100.0f, 0.1f,
            parentOps,
            format: OptionUnit.VoteNum);
        CreateFloatOption(
            CaptainOption.AwakedDefaultVoteNum,
            0.0f, 0.0f, 100.0f, 0.1f,
            parentOps,
            format: OptionUnit.VoteNum);
    }

    protected override void RoleSpecificInit()
    {

        var allOpt = AllOptionHolder.Instance;

        this.chargeVoteNum = allOpt.GetValue<int>(
           GetRoleOptionId(CaptainOption.ChargeVoteWhenSkip));
        this.defaultVote = allOpt.GetValue<float>(
           GetRoleOptionId(CaptainOption.AwakedDefaultVoteNum));
        this.awakeTaskGage = allOpt.GetValue<int>(
           GetRoleOptionId(CaptainOption.AwakeTaskGage)) / 100.0f;

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
