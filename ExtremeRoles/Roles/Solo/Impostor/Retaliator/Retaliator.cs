using System;
using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;
using ExtremeRoles.Extension.Player;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor.Retaliator;

public sealed class RetaliatorRole : SingleRoleBase, IRoleAwake<RoleTypes>, IRoleMeetingButtonAbility, IRoleResetMeeting
{
    public sealed class RetaliatorTargetReportSerializer : IStringSerializer
    {
        public StringSerializerType Type => StringSerializerType.RetaliatorTargetNotification;
        public bool IsRpc { get; set; } = true;

        public void Serialize(RPCOperator.RpcCaller caller)
        {
        }

        public void Deserialize(Hazel.MessageReader reader)
        {
        }

        public override string ToString() => Tr.GetString("RetaliatorTargetedNotification");
    }

    public enum RetaliatorOption
    {
        AwakeKillCount,
        MaxAbilityUseCount,
        MaxTargetPerMeeting,
        TargetTeam,
        NotifyTargetNextMeeting,
    }

    private RetaliatorStatusModel? status;
    private TMPro.TextMeshPro? meetingText;

    public override IStatusModel? Status => this.status;

    public RoleTypes NoneAwakeRole => RoleTypes.Impostor;

    public bool IsAwake => GameSystem.IsLobby || (this.status != null && this.status.IsAwake);

    public Sprite AbilityImage => UnityObjectLoader.LoadFromResources(ExtremeRoleId.Retaliator);

    public RetaliatorRole() : base(
        RoleArgs.BuildImpostor(ExtremeRoleId.Retaliator))
    { }

    public string GetFakeOptionString() => "";

    public void Update(PlayerControl rolePlayer)
    {
        var meeting = MeetingHud.Instance;
        if (meeting != null &&
            rolePlayer.IsAlive() &&
            this.IsAwake &&
            this.status != null)
        {
            if (this.meetingText == null)
            {
                this.meetingText = UnityEngine.Object.Instantiate(
                    HudManager.Instance.TaskPanel.taskText,
                    meeting.transform);
                this.meetingText.alignment = TMPro.TextAlignmentOptions.BottomLeft;
                this.meetingText.transform.position = Vector3.zero;
                this.meetingText.transform.localPosition = new Vector3(-2.85f, 3.15f, -20f);
                this.meetingText.transform.localScale *= 0.9f;
                this.meetingText.color = Palette.White;
                this.meetingText.gameObject.SetActive(false);
            }

            this.meetingText.text = Tr.GetString(
                "retaliatorUiInfo",
                this.status.RemainingAbilityUses,
				this.status.CurrentMeetingTargetCount);
            this.meetingText.gameObject.SetActive(true);
        }
        else if (this.meetingText != null)
        {
            this.meetingText.gameObject.SetActive(false);
        }
    }

    protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateIntOption(
            RetaliatorOption.AwakeKillCount,
            1, 0, 10, 1);

        var maxAbilityUses = factory.CreateIntOption(
            RetaliatorOption.MaxAbilityUseCount,
            3, 1, 15, 1,
            format: OptionUnit.Shot);

        factory.CreateIntDynamicMaxOption(
            RetaliatorOption.MaxTargetPerMeeting,
            1, 1, 1, maxAbilityUses,
            format: OptionUnit.Shot);

        factory.CreateSelectionOption<RetaliatorOption, RetaliatorTargetTeam>(
            RetaliatorOption.TargetTeam);

        factory.CreateBoolOption(
            RetaliatorOption.NotifyTargetNextMeeting,
            true);
    }

    protected override void RoleSpecificInit()
    {
        var loader = this.Loader;
        int awakeKillCount = loader.GetValue<RetaliatorOption, int>(RetaliatorOption.AwakeKillCount);
        int maxAbilityUses = loader.GetValue<RetaliatorOption, int>(RetaliatorOption.MaxAbilityUseCount);
        int maxTargetPerMeeting = loader.GetValue<RetaliatorOption, int>(RetaliatorOption.MaxTargetPerMeeting);
        var targetTeam = (RetaliatorTargetTeam)loader.GetValue<RetaliatorOption, int>(RetaliatorOption.TargetTeam);
        bool notifyTarget = loader.GetValue<RetaliatorOption, bool>(RetaliatorOption.NotifyTargetNextMeeting);

        this.status = new RetaliatorStatusModel(
            awakeKillCount,
            maxAbilityUses,
            maxTargetPerMeeting,
            targetTeam,
            notifyTarget);

        this.AbilityClass = new RetaliatorAbilityHandler(this.status);
    }

    public void TargetPlayer(byte targetPlayerId)
    {
        this.status?.TargetPlayer(targetPlayerId);
    }

    public bool IsBlockMeetingButtonAbility(PlayerVoteArea instance)
    {
		var localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null ||
			this.status == null || 
			!this.status.IsAwake ||
			!this.status.CanUseAbility)
        {
            return true;
        }

        byte targetId = instance.PlayerId;
        return targetId == localPlayer.PlayerId || this.status.IsTargeted(targetId);
    }

    public void ButtonMod(PlayerVoteArea instance, UiElement abilityButton)
        => IRoleMeetingButtonAbility.DefaultButtonMod(instance, abilityButton, "RetaliatorRetaliate");

    public Action CreateAbilityAction(PlayerVoteArea instance)
    {
        byte rolePlayerId = PlayerControl.LocalPlayer.PlayerId;
        byte targetPlayerId = instance.PlayerId;

        void retaliateAction()
        {
            using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.RetaliatorAbility))
            {
                caller.WriteByte(rolePlayerId);
                caller.WriteByte(targetPlayerId);
            }
            TargetPlayer(targetPlayerId);
        }

        return retaliateAction;
    }

    public void ResetOnMeetingStart()
    {
        if (this.status is null)
        {
            return;
        }

        if (this.status.NotifyTargetNextMeeting)
        {
            foreach (byte targetId in this.status.TargetedPlayerIds)
            {
                MeetingReporter.RpcAddTargetMeetingChatReport(
                    targetId,
                    new RetaliatorTargetReportSerializer());
            }
        }

        this.status.OnMeetingStart();
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
    }

    public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId)
    {
        if (this.status != null && this.status.IsTargeted(targetPlayerId))
        {
            return $" ({Design.ColoredString(Palette.ImpostorRed, Tr.GetString("RetaliatorTargetTag"))})";
        }
        return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
    }

    public override string GetColoredRoleName(bool isTruthColor = false)
        => isTruthColor || IsAwake ?
            base.GetColoredRoleName() : Design.ColoredString(
                Palette.ImpostorRed, Tr.GetString(RoleTypes.Impostor.ToString()));

    public override string GetFullDescription()
        => IsAwake ?
            Tr.GetString($"{this.Core.Id}FullDescription") :
            Tr.GetString($"{RoleTypes.Impostor}FullDescription");

    public override string GetImportantText(bool isContainFakeTask = true)
        => IsAwake ?
            base.GetImportantText(isContainFakeTask) :
            $"{TranslationController.Instance.GetString(
                    StringNames.ImpostorTask, Array.Empty<Il2CppSystem.Object>())}\r\n{Palette.ImpostorRed.ToTextColor()}{TranslationController.Instance.GetString(
                    StringNames.FakeTasks, Array.Empty<Il2CppSystem.Object>())}</color>";

    public override string GetIntroDescription()
        => IsAwake ? base.GetIntroDescription() : Design.ColoredString(
                Palette.ImpostorRed,
                PlayerControl.LocalPlayer.Data.Role.Blurb);

    public override Color GetNameColor(bool isTruthColor = false)
        => isTruthColor || IsAwake ?
            base.GetNameColor(isTruthColor) : Palette.ImpostorRed;
}
