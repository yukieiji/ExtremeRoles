using System.Collections.Generic;

using ExtremeRoles.Module;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Resources;
using ExtremeRoles.Module.Ability;

using ExtremeRoles.Module.CustomOption.Factory;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral;

public sealed class Totocalcio : SingleRoleBase, IRoleAutoBuildAbility, IRoleWinPlayerModifier
{
	private sealed record BetPlayerInfo(byte PlayerId, string PlayerName)
	{
		public BetPlayerInfo(in NetworkedPlayerInfo info) :
			this(info.PlayerId, info.DefaultOutfit.PlayerName)
		{
		}
	}

    public enum TotocalcioOption
    {
        Range,
        FinalCoolTime,
    }

    private static HashSet<ExtremeRoleId> ignoreRole = new HashSet<ExtremeRoleId>()
    {
        ExtremeRoleId.Yoko,
        ExtremeRoleId.Vigilante,
        ExtremeRoleId.Totocalcio,
    };

    public ExtremeAbilityButton? Button { get; set; }


    private float range;
    private BetPlayerInfo? betPlayer;
    private PlayerControl? tmpTarget;

    private float defaultCoolTime;
    private float finalCoolTime;

    public Totocalcio() : base(
       ExtremeRoleId.Totocalcio,
       ExtremeRoleType.Neutral,
       ExtremeRoleId.Totocalcio.ToString(),
       ColorPalette.TotocalcioGreen,
       false, false, false, false)
    { }


    public static void SetBetTarget(
        byte rolePlayerId, byte betTargetPlayerId)
    {
        var totocalcio =  ExtremeRoleManager.GetSafeCastedRole<Totocalcio>(rolePlayerId);
		var target = GameData.Instance.GetPlayerById(betTargetPlayerId);
        if (totocalcio != null &&
			target != null)
        {
            totocalcio.betPlayer = new BetPlayerInfo(target);
        }
    }

    public void CreateAbility()
    {
        this.CreateAbilityCountButton(
            "betPlayer", Resources.UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.TotocalcioBetPlayer));
        this.Button?.SetLabelToCrewmate();
    }

    public bool IsAbilityUse()
    {
        this.tmpTarget = Helper.Player.GetClosestPlayerInRange(
            PlayerControl.LocalPlayer, this,
            this.range);

        if (this.tmpTarget == null ||
            this.tmpTarget.Data == null) { return false; }

        bool commonUse = IRoleAbility.IsCommonUse();

        if (this.betPlayer != null)
        {
            return commonUse &&
                this.tmpTarget.Data.PlayerId != this.betPlayer.PlayerId;
        }
        else
        {
            return commonUse;
        }
    }

    public void ModifiedWinPlayer(
        NetworkedPlayerInfo rolePlayerInfo,
        GameOverReason reason,
		in ExtremeGameResult.WinnerTempData winner)
    {
        if (this.betPlayer == null ||
			ignoreRole.Contains(
				ExtremeRoleManager.GameRole[
					this.betPlayer.PlayerId].Id) ||
			!winner.Contains(this.betPlayer.PlayerName)) { return; }

		winner.AddWithPlus(rolePlayerInfo);
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
        return;
    }

    public void ResetOnMeetingStart()
    {
        if (this.Button == null) { return; }

        int deadNum = 0;

        foreach (var player in
            GameData.Instance.AllPlayers.GetFastEnumerator())
        {
            if (player.IsDead || player.Disconnected) { ++deadNum; }
        }

        if (deadNum == 0) { return; }

        this.Button.Behavior.SetCoolTime(
            this.defaultCoolTime + (
                (this.finalCoolTime - this.defaultCoolTime) * ((float)deadNum / (float)GameData.Instance.PlayerCount)));
        this.Button.OnMeetingEnd();
    }

    public bool UseAbility()
    {
        if (this.tmpTarget == null) { return false; }

        byte localPlayerId = PlayerControl.LocalPlayer.PlayerId;

        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.TotocalcioSetBetPlayer))
        {
            caller.WriteByte(localPlayerId);
            caller.WriteByte(this.tmpTarget.PlayerId);
        }
        SetBetTarget(
            localPlayerId,
            this.tmpTarget.PlayerId);

        this.tmpTarget = null;

        return true;
    }

    public override string GetFullDescription() =>
        string.Format(
            base.GetFullDescription(),
            this.betPlayer != null ?
                this.betPlayer.PlayerName : Helper.OldTranslation.GetString("loseNow"));

    public override string GetRolePlayerNameTag(
        SingleRoleBase targetRole, byte targetPlayerId)
    {
        if (this.betPlayer == null) { return ""; }

        if (targetPlayerId == this.betPlayer.PlayerId)
        {
            return Helper.Design.ColoedString(
                this.NameColor, $" ▲");
        }

        return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {

        factory.CreateFloatOption(
            TotocalcioOption.Range,
            1.0f, 0.0f, 2.0f, 0.1f);

        IRoleAbility.CreateAbilityCountOption(factory, 3, 5);

        factory.CreateFloatOption(
            TotocalcioOption.FinalCoolTime,
            80.0f, 45.0f, 180.0f, 0.1f,
            format: OptionUnit.Second);
    }

    protected override void RoleSpecificInit()
    {
        this.betPlayer = null;
		var cate = this.Loader;
        this.range = cate.GetValue<TotocalcioOption, float>(
            TotocalcioOption.Range);
        this.defaultCoolTime = cate.GetValue<RoleAbilityCommonOption, float>(
            RoleAbilityCommonOption.AbilityCoolTime);
        this.finalCoolTime = cate.GetValue<TotocalcioOption, float>(
            TotocalcioOption.FinalCoolTime);
    }
}
