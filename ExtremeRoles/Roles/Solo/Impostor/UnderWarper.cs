using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using AmongUs.GameOptions;

using Newtonsoft.Json.Linq;

using ExtremeRoles.Compat.Mods;
using ExtremeRoles.Extension.Json;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

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
        Range,
    }

    public bool IsAwake
    {
        get
        {
            return GameSystem.IsLobby || this.isAwake;
        }
    }


    public float VentUseRange => this.range;
    public bool IsNoVentAnime => this.isNoVentAnime;

    public RoleTypes NoneAwakeRole => RoleTypes.Impostor;

    private int killCount;

    private bool isAwake;
    private int awakeKillCount;

    private bool isVentLink;
    private int ventLinkKillCout;

    private bool isNoVentAnime;
    private int noVentAnimeKillCout;

    private float range;

    private bool isAwakedHasOtherVision;
    private bool isAwakedHasOtherKillCool;
    private bool isAwakedHasOtherKillRange;

    private const string ventInfoJson =
        "ExtremeRoles.Resources.JsonData.UnderWarperVentInfo.json";

    private const string skeldKey = "Skeld";
    private const string polusKey = "Polus";
    private const string airShipKey = "AirShip";
    private const string submergedKey = "Submerged";

    private Dictionary<int, Vent> cachedVent;

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

        if (ExtremeRolesPlugin.Compat.IsModMap)
        {
            if (ExtremeRolesPlugin.Compat.ModMap is SubmergedMap)
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
            (Il2CppSystem.Action<RoleEffectAnimation>)(
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
            (Il2CppSystem.Action<RoleEffectAnimation>)(
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
        this.cachedVent = new Dictionary<int, Vent>();
        foreach (Vent vent in CachedShipStatus.Instance.AllVents)
        {
            this.cachedVent.Add(vent.Id, vent);
        }

        if (this.isVentLink)
        {
            this.relinkMapVent();
        }
    }

    public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
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
            relinkMapVent();
        }
        if (!this.isNoVentAnime &&
            this.killCount >= this.noVentAnimeKillCout)
        {
            this.isNoVentAnime = true;
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
            !this.isNoVentAnime)
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
        CreateFloatOption(
            UnderWarperOption.Range,
            1.75f, 0.75f, 4.0f, 0.25f, parentOps);
    }

    protected override void RoleSpecificInit()
    {

        var allOpt = AllOptionHolder.Instance;

        this.awakeKillCount = allOpt.GetValue<int>(
            GetRoleOptionId(UnderWarperOption.AwakeKillCount));
        this.ventLinkKillCout = allOpt.GetValue<int>(
            GetRoleOptionId(UnderWarperOption.VentLinkKillCout));
        this.noVentAnimeKillCout = allOpt.GetValue<int>(
            GetRoleOptionId(UnderWarperOption.NoVentAnimeKillCout));

        this.range = allOpt.GetValue<float>(
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
            this.isNoVentAnime = true;
        }
    }

    private void relinkMapVent()
    {
        JObject linkInfoJson = JsonParser.GetJObjectFromAssembly(ventInfoJson);

        string ventKey;
        byte mapId = GameOptionsManager.Instance.CurrentGameOptions.GetByte(ByteOptionNames.MapId);

        if (ExtremeRolesPlugin.Compat.IsModMap)
        {
            if (ExtremeRolesPlugin.Compat.ModMap is SubmergedMap)
            {
                ventKey = submergedKey;
            }
            else
            {
                return;
            }
        }
        else
        {
            switch (mapId)
            {
                case 0:
                    ventKey = skeldKey;
                    break;
                case 2:
                    ventKey = polusKey;
                    break;
                case 4:
                    ventKey = airShipKey;
                    break;
                default:
                    return;
            }
        }

        JArray linkInfo = linkInfoJson.Get<JArray>(ventKey);
        
        for (int i = 0; i < linkInfo.Count; ++i )
        {
            JArray ventLinkedId = linkInfo.Get<JArray>(i);

            if (this.cachedVent.TryGetValue((int)ventLinkedId[0], out Vent from) &&
                this.cachedVent.TryGetValue((int)ventLinkedId[1], out Vent target))
            {
                linkVent(from, target);
            }
        }
    }

    private static void linkVent(Vent from, Vent target)
    {
        if (from == null || target == null) { return; }

        linkVentToEmptyTarget(from, target);
        linkVentToEmptyTarget(target, from);
    }

    private static void linkVentToEmptyTarget(Vent from, Vent target)
    {
        if (from.Right == null)
        {
            from.Right = target;
        }
        else if (from.Center == null)
        {
            from.Center = target;
        }
        else if (from.Left == null)
        {
            from.Left = target;
        }
        else
        {
            ExtremeRolesPlugin.Logger.LogInfo("Vent Link fail!!");
        }
    }
}
