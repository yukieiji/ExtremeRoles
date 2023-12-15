using System;
using System.Linq;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Compat.ModIntegrator;
using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Compat;

using RoleEffectAction = Il2CppSystem.Action<RoleEffectAnimation>;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class UnderWarper :
    SingleRoleBase,
    IRoleAwake<RoleTypes>,
    IRoleResetMeeting,
    IRoleSpecialSetUp
{
    public enum UnderWarperOption
    {
        AwakeKillCount,
        VentLinkKillCout,
        NoVentAnimeKillCout,
		WallHackVent,
		Range
    }

    public bool IsAwake
    {
        get
        {
            return GameSystem.IsLobby || this.isAwake;
        }
    }


    public float VentUseRange { get; private set; }
    public bool IsNoVentAnime { get; private set; }

	public static bool IsWallHackVent
	{
		get
		{
			var underWarper = localUnderWarper;
			return
				underWarper != null &&
				underWarper.IsAwake &&
				underWarper.IsNoVentAnime &&
				underWarper.isWallHackVent;
		}
	}

	public static bool IsNoAnimateVent
	{
		get
		{
			var underWarper = localUnderWarper;
			return
				underWarper != null &&
				underWarper.IsAwake &&
				underWarper.IsNoVentAnime;
		}
	}

	private static UnderWarper? localUnderWarper => ExtremeRoleManager.GetSafeCastedLocalPlayerRole<UnderWarper>();

	public RoleTypes NoneAwakeRole => RoleTypes.Impostor;

    private int killCount;

    private bool isAwake;
    private int awakeKillCount;

    private bool isVentLink;
    private int ventLinkKillCout;

    private int noVentAnimeKillCout;
	private bool isWallHackVent;

    private bool isAwakedHasOtherVision;
    private bool isAwakedHasOtherKillCool;
    private bool isAwakedHasOtherKillRange;

    public UnderWarper() : base(
        ExtremeRoleId.UnderWarper,
        ExtremeRoleType.Impostor,
        ExtremeRoleId.UnderWarper.ToString(),
        Palette.ImpostorRed,
        true, false, true, true)
    { }

    public static void UseVentWithNoAnimation(
        byte playerId, int ventId, bool isEnter)
    {
        PlayerControl targetPlayer = Player.GetPlayerControlById(playerId);
        Vent vent = CachedShipStatus.Instance.AllVents.First(
            (Vent v) => v.Id == ventId);

        if (targetPlayer == null || vent == null) { return; }

        if (isEnter)
        {
            enterVent(targetPlayer, vent);
        }
        else
        {
            exitVent(targetPlayer, vent);
        }
    }

    public static void RpcUseVentWithNoAnimation(
        PlayerControl localPlayer, int ventId, bool isEnter)
    {
        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.UnderWarperUseVentWithNoAnime))
        {
            caller.WriteByte(localPlayer.PlayerId);
            caller.WritePackedInt(ventId);
            caller.WriteBoolean(isEnter);
        }
        UseVentWithNoAnimation(
            localPlayer.PlayerId, ventId, isEnter);

        if (CompatModManager.Instance.TryGetModMap(out var modMap))
        {
            if (modMap is SubmergedIntegrator)
            {
                FastDestroyableSingleton<HudManager>.Instance.PlayerCam.Locked = false;
            }
        }
    }

    private static void enterVent(
        PlayerControl targetPlayer, Vent vent)
    {
        targetPlayer.MyPhysics.StopAllCoroutines();
        if (targetPlayer.AmOwner)
        {
            targetPlayer.MyPhysics.inputHandler.enabled = true;
            Vent.currentVent = vent;
            ConsoleJoystick.SetMode_Vent();
        }

        targetPlayer.moveable = false;
        targetPlayer.NetTransform.SnapTo(vent.transform.position + vent.Offset);
        targetPlayer.cosmetics.AnimateSkinIdle();
        targetPlayer.Visible = false;
        targetPlayer.inVent = true;
        targetPlayer.currentRoleAnimations.ForEach(
            (RoleEffectAction)(
                (RoleEffectAnimation an) =>
                {
                    an.ToggleRenderer(false);
                })
            );

        if (targetPlayer.AmOwner)
        {
            VentilationSystem.Update(
                VentilationSystem.Operation.Enter, vent.Id);
            targetPlayer.MyPhysics.inputHandler.enabled = false;
        }
    }

    private static void exitVent(
        PlayerControl targetPlayer, Vent vent)
    {
        if (targetPlayer.AmOwner)
        {
            targetPlayer.MyPhysics.inputHandler.enabled = true;
            VentilationSystem.Update(
                VentilationSystem.Operation.Exit, vent.Id);
        }

        targetPlayer.Visible = true;
        targetPlayer.inVent = false;

        if (targetPlayer.AmOwner)
        {
            Vent.currentVent = null;
        }
        targetPlayer.cosmetics.AnimateSkinIdle();
        targetPlayer.moveable = true;
        targetPlayer.currentRoleAnimations.ForEach(
            (RoleEffectAction)(
                (RoleEffectAnimation an) =>
                {
                    an.ToggleRenderer(true);
                })
            );

        if (targetPlayer.AmOwner)
        {
            targetPlayer.MyPhysics.inputHandler.enabled = false;
        }
    }

    public string GetFakeOptionString() => "";

    public void IntroBeginSetUp()
    {
        return;
    }

    public void IntroEndSetUp()
    {
        if (this.isVentLink)
        {
			GameSystem.RelinkVent();
        }
    }

    public void ResetOnMeetingEnd(GameData.PlayerInfo? exiledPlayer = null)
    {
        if (!this.isAwake &&
            this.killCount >= this.awakeKillCount)
        {
            this.isAwake = true;
        }
        if (!this.isVentLink &&
            this.killCount >= this.ventLinkKillCout)
        {
            this.isVentLink = true;
			GameSystem.RelinkVent();
		}
        if (!this.IsNoVentAnime &&
            this.killCount >= this.noVentAnimeKillCout)
        {
            this.IsNoVentAnime = true;
        }
    }

    public void ResetOnMeetingStart()
    {
        return;
    }

    public void Update(PlayerControl rolePlayer)
    { }

    public override string GetColoredRoleName(bool isTruthColor = false)
    {
        if (isTruthColor || IsAwake)
        {
            return base.GetColoredRoleName();
        }
        else
        {
            return Design.ColoedString(
                Palette.ImpostorRed, Translation.GetString(RoleTypes.Impostor.ToString()));
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
                $"{RoleTypes.Impostor}FullDescription");
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
            return string.Concat(new string[]
            {
                FastDestroyableSingleton<TranslationController>.Instance.GetString(
                    StringNames.ImpostorTask, Array.Empty<Il2CppSystem.Object>()),
                "\r\n",
                Palette.ImpostorRed.ToTextColor(),
                FastDestroyableSingleton<TranslationController>.Instance.GetString(
                    StringNames.FakeTasks, Array.Empty<Il2CppSystem.Object>()),
                "</color>"
            });
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
                Palette.ImpostorRed,
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
            return Palette.ImpostorRed;
        }
    }

    public override bool TryRolePlayerKillTo(
        PlayerControl rolePlayer, PlayerControl targetPlayer)
    {
        if (!this.isAwake ||
            !this.isVentLink ||
            !this.IsNoVentAnime)
        {
            ++this.killCount;
        }
        return true;
    }

    protected override void CreateSpecificOption(
        IOptionInfo parentOps)
    {
        CreateIntOption(
            UnderWarperOption.AwakeKillCount,
            1, 0, 5, 1, parentOps,
            format: OptionUnit.Shot);
        CreateIntOption(
            UnderWarperOption.VentLinkKillCout,
            2, 0, 5, 1, parentOps,
            format: OptionUnit.Shot);
        CreateIntOption(
            UnderWarperOption.NoVentAnimeKillCout,
            2, 0, 5, 1, parentOps,
            format: OptionUnit.Shot);
		CreateBoolOption(
			UnderWarperOption.WallHackVent,
			false, parentOps);
		CreateFloatOption(
            UnderWarperOption.Range,
            2.75f, 0.75f, 10.0f, 0.25f, parentOps);
	}

    protected override void RoleSpecificInit()
    {

        var allOpt = OptionManager.Instance;

        this.awakeKillCount = allOpt.GetValue<int>(
            GetRoleOptionId(UnderWarperOption.AwakeKillCount));
        this.ventLinkKillCout = allOpt.GetValue<int>(
            GetRoleOptionId(UnderWarperOption.VentLinkKillCout));
        this.noVentAnimeKillCout = allOpt.GetValue<int>(
            GetRoleOptionId(UnderWarperOption.NoVentAnimeKillCout));

		this.isWallHackVent = allOpt.GetValue<bool>(
			GetRoleOptionId(UnderWarperOption.WallHackVent));

        this.VentUseRange = allOpt.GetValue<float>(
            GetRoleOptionId(UnderWarperOption.Range));

        this.isAwakedHasOtherVision = false;
        this.isAwakedHasOtherKillCool = true;
        this.isAwakedHasOtherKillRange = false;

        if (this.HasOtherVision)
        {
            this.HasOtherVision = false;
            this.isAwakedHasOtherVision = true;
        }

        if (this.HasOtherKillCool)
        {
            this.HasOtherKillCool = false;
        }

        if (this.HasOtherKillRange)
        {
            this.HasOtherKillRange = false;
            this.isAwakedHasOtherKillRange = true;
        }

        if (this.awakeKillCount <= 0)
        {
            this.isAwake = true;
            this.HasOtherVision = this.isAwakedHasOtherVision;
            this.HasOtherKillCool = this.isAwakedHasOtherKillCool;
            this.HasOtherKillRange = this.isAwakedHasOtherKillRange;
        }

        if (this.ventLinkKillCout <= 0)
        {
            this.isVentLink = true;
        }

        if (this.noVentAnimeKillCout <= 0)
        {
            this.IsNoVentAnime = true;
        }
    }
}
