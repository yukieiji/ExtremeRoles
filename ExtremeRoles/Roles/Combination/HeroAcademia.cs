using System.Collections.Generic;

using UnityEngine;

using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Extension.Neutral;
using ExtremeRoles.Resources;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Extension.Player;
using ExtremeRoles.Module.Ability;




using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.GameResult;

#nullable enable

namespace ExtremeRoles.Roles.Combination;

internal sealed class AllPlayerArrows
{
    private Dictionary<byte, PlayerControl> player = new Dictionary<byte, PlayerControl>();
    private Dictionary<byte, Arrow> arrow = new Dictionary<byte, Arrow>();
    private Dictionary<byte, TMPro.TextMeshPro> distance = new Dictionary<byte, TMPro.TextMeshPro>();

    public AllPlayerArrows(byte rolePlayerId)
    {
        this.player.Clear();
        this.arrow.Clear();
        this.distance.Clear();

        foreach (var player in PlayerCache.AllPlayerControl)
        {
            if (player.PlayerId != rolePlayerId)
            {
                var playerArrow = new Arrow(
                    new Color32(
                        byte.MaxValue,
                        byte.MaxValue,
                        byte.MaxValue,
                        byte.MaxValue));
                playerArrow.SetActive(false);

                var text = GameObject.Instantiate(
                    Prefab.Text, playerArrow.Main.transform);
                text.fontSize = text.fontSizeMax = text.fontSizeMin = 3.25f;
                Object.Destroy(text.fontMaterial);
                text.fontMaterial = UnityEngine.Object.Instantiate(
                    HudManager.Instance.UseButton.buttonLabelText.fontMaterial,
                    playerArrow.Main.transform);
                text.gameObject.layer = 5;
                text.alignment = TMPro.TextAlignmentOptions.Center;
                text.transform.localPosition = new Vector3(0.0f, 0.0f, -800f);
                text.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

                this.distance.Add(player.PlayerId, text);
                this.player.Add(player.PlayerId, player);
                this.arrow.Add(player.PlayerId, playerArrow);
            }
        }

    }

    public void SetActive(bool active)
    {
        List<byte> removePlayer = new List<byte>();
        foreach (var (playerId, playerCont) in this.player)
        {
            if (playerCont == null)
            {
                this.arrow[playerId].SetActive(active);
                this.distance[playerId].gameObject.SetActive(active);
                removePlayer.Add(playerId);
                continue;
            }

            this.arrow[playerId].SetActive(active);
            this.distance[playerId].gameObject.SetActive(active);

            if (playerCont.Data.IsDead ||
                playerCont.Data.Disconnected)
            {
                this.arrow[playerId].SetActive(false);
                this.distance[playerId].gameObject.SetActive(false);
            }
        }

        foreach (byte playerId in removePlayer)
        {
            GameObject.Destroy(this.distance[playerId]);
            this.arrow[playerId].Clear();
            this.distance.Remove(playerId);
            this.arrow.Remove(playerId);
            this.player.Remove(playerId);
        }
    }

    public void Update(Vector2 rolePlayerPos)
    {
        foreach(var (playerId, playerCont) in this.player)
        {
            float diss = Vector2.Distance(rolePlayerPos, playerCont.GetTruePosition());

            this.distance[playerId].text = Design.ColoedString(
                Color.black, $"{diss:F1}");
            this.arrow[playerId].UpdateTarget(playerCont.transform.position);
        }
    }
}

internal sealed class PlayerTargetArrow
{
    public bool isActive;
    private Arrow arrow;
    private PlayerControl? targetPlayer;
    public PlayerTargetArrow(Color color)
    {
        this.arrow = new Arrow(color);
    }
    public void SetActive(bool active)
    {
        this.isActive = active;
        this.arrow.SetActive(active);
    }
    public void ResetTarget()
    {
        this.targetPlayer = null;
    }

    public void SetTargetPlayer(PlayerControl player)
    {
        this.targetPlayer = player;
    }

    public void Update()
    {
        if (this.targetPlayer == null) { return; }

        this.arrow.UpdateTarget(
            targetPlayer.GetTruePosition());

    }

}

public sealed class HeroAcademia : ConstCombinationRoleManagerBase
{
    public enum Command : byte
    {
        UpdateHero,
        UpdateVigilante,
        DrawHeroAndVillan,
        EmergencyCall,
        CleanUpEmergencyCall,
    }
    public enum Condition : byte
    {
        HeroDown,
        VillainDown,
    }

    public const string Name = "HeroAca";
    public HeroAcademia() : base(
		CombinationRoleType.HeroAca,
        Name, DefaultColor, 3,
        GameSystem.MaxImposterNum)
    {
        this.Roles.Add(new Hero());
        this.Roles.Add(new Villain());
        this.Roles.Add(new Vigilante());
    }

    public static void RpcCommand(
        ref MessageReader reader)
    {

        byte command = reader.ReadByte();

        switch ((Command)command)
        {
            case Command.UpdateHero:
                byte updateHeroPlayerId = reader.ReadByte();
                byte heroNewCond = reader.ReadByte();
                updateHero(updateHeroPlayerId, heroNewCond);
                break;
            case Command.UpdateVigilante:
                byte heroAcaCond = reader.ReadByte();
                byte downPlayerId = reader.ReadByte();
                UpdateVigilante((Condition)heroAcaCond, downPlayerId);
                break;
            case Command.DrawHeroAndVillan:
                byte heroPlayerId = reader.ReadByte();
                byte villanPlayerId = reader.ReadByte();
                drawHeroAndVillan(heroPlayerId, villanPlayerId);
                break;
            case Command.EmergencyCall:
                byte vigilantePlayerId = reader.ReadByte();
                byte targetPlayerId = reader.ReadByte();
                emergencyCall(vigilantePlayerId, targetPlayerId);
                break;
            case Command.CleanUpEmergencyCall:
                cleanUpEmergencyCall();
                break;
            default:
                break;
        }

    }
    public static void RpcCleanUpEmergencyCall()
    {
        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.HeroHeroAcademia))
        {
            caller.WriteByte((byte)Command.CleanUpEmergencyCall);
        }
        cleanUpEmergencyCall();
    }

    public static void RpcEmergencyCall(
        PlayerControl vigilante, byte targetPlayerId)
    {
        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.HeroHeroAcademia))
        {
            caller.WriteByte((byte)Command.EmergencyCall);
            caller.WriteByte(vigilante.PlayerId);
            caller.WriteByte(targetPlayerId);
        }
        emergencyCall(vigilante.PlayerId, targetPlayerId);
    }

    public static void RpcDrawHeroAndVillan(
        PlayerControl hero, PlayerControl villan)
    {
        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.HeroHeroAcademia))
        {
            caller.WriteByte((byte)Command.DrawHeroAndVillan);
            caller.WriteByte(hero.PlayerId);
            caller.WriteByte(villan.PlayerId);
        }
        drawHeroAndVillan(hero.PlayerId, villan.PlayerId);
    }
    public static void RpcUpdateHero(
        PlayerControl hero,
        Hero.OneForAllCondition newCond)
    {
        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.HeroHeroAcademia))
        {
            caller.WriteByte((byte)Command.UpdateHero);
            caller.WriteByte(hero.PlayerId);
            caller.WriteByte((byte)newCond);
        }
        updateHero(hero.PlayerId, (byte)newCond);

    }
    public static void RpcUpdateVigilante(
        Condition cond, byte downPlayerId)
    {
        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.HeroHeroAcademia))
        {
            caller.WriteByte((byte)Command.UpdateHero);
            caller.WriteByte((byte)cond);
            caller.WriteByte(downPlayerId);
        }
        UpdateVigilante(cond, downPlayerId);
    }

    public static void UpdateVigilante(
        Condition cond, byte downPlayerId)
    {

        int crewNum = 0;
        int impNum = 0;

        foreach (NetworkedPlayerInfo player in
            GameData.Instance.AllPlayers.GetFastEnumerator())
        {
            var role = ExtremeRoleManager.GameRole[player.PlayerId];
            if (!player.IsDead &&
                !player.Disconnected &&
                player.PlayerId != downPlayerId)
            {
                if (role.IsCrewmate())
                {
                    ++crewNum;
                }
                else if (role.IsImpostor())
                {
                    ++impNum;
                }
            }

            Vigilante? vigilante = ExtremeRoleManager.GetSafeCastedRole<Vigilante>(player.PlayerId);

            if (vigilante != null)
            {
                switch (cond)
                {
                    case Condition.HeroDown:
                        if (crewNum <= impNum)
                        {
                            vigilante.SetCondition(
                                Vigilante.VigilanteCondition.NewEnemyNeutralForTheShip);
                        }
                        else
                        {
                            vigilante.SetCondition(
                                Vigilante.VigilanteCondition.NewHeroForTheShip);
                        }
                        break;
                    case Condition.VillainDown:
                        if (impNum <= 0)
                        {
                            vigilante.SetCondition(
                                Vigilante.VigilanteCondition.NewEnemyNeutralForTheShip);
                        }
                        else
                        {
                            vigilante.SetCondition(
                                Vigilante.VigilanteCondition.NewVillainForTheShip);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
    private static void cleanUpEmergencyCall()
    {
        var hero = ExtremeRoleManager.GetSafeCastedLocalPlayerRole<Hero>();
        if (hero != null)
        {
            hero.ResetTarget();
        }
        var villan = ExtremeRoleManager.GetSafeCastedLocalPlayerRole<Villain>();
        if (villan != null)
        {
            villan.ResetVigilante();
        }
    }

    private static void emergencyCall(
        byte vigilantePlayerId, byte targetPlayerId)
    {
        PlayerControl vigilantePlayer = Player.GetPlayerControlById(vigilantePlayerId);
        PlayerControl targetPlayer = Player.GetPlayerControlById(targetPlayerId);

        if (vigilantePlayer != null && targetPlayer != null)
        {

            var hero = ExtremeRoleManager.GetSafeCastedLocalPlayerRole<Hero>();
            if (hero != null)
            {
                hero.SetEmergencyCallTarget(targetPlayer);
            }

            var villan = ExtremeRoleManager.GetSafeCastedLocalPlayerRole<Villain>();
            if (villan != null)
            {
                villan.SetVigilante(vigilantePlayer);
            }

        }
    }

    private static void drawHeroAndVillan(
        byte heroPlayerId, byte villanPlayerId)
    {
        PlayerControl heroPlayer = Player.GetPlayerControlById(heroPlayerId);
        PlayerControl villanPlayer = Player.GetPlayerControlById(villanPlayerId);

        if (heroPlayer != null && villanPlayer != null)
        {

            ExtremeRolesPlugin.ShipState.SetDisableWinCheck(true);

            if (heroPlayer.protectedByGuardianId > -1)
            {
                heroPlayer.RemoveProtection();
            }
            if (villanPlayer.protectedByGuardianId > -1)
            {
                villanPlayer.RemoveProtection();
            }

            heroPlayer.MurderPlayer(villanPlayer);
            villanPlayer.MurderPlayer(heroPlayer);

            foreach (var role in ExtremeRoleManager.GameRole.Values)
            {
                if (role.Core.Id == ExtremeRoleId.Vigilante)
                {
                    ((Vigilante)role).SetCondition(
                        Vigilante.VigilanteCondition.NewLawInTheShip);
                }
            }

            ExtremeRolesPlugin.ShipState.SetDisableWinCheck(false);
        }
    }
    private static void updateHero(
        byte heroId,
        byte newCond)
    {
        var hero = ExtremeRoleManager.GetSafeCastedRole<Hero>(heroId);
        if (hero != null)
        {
            hero.SetCondition((Hero.OneForAllCondition)newCond);
        }
    }

}

public sealed class Hero : MultiAssignRoleBase, IRoleAutoBuildAbility, IRoleUpdate, IRoleSpecialReset, ITryKillTo
{
    public enum OneForAllCondition : byte
    {
        NoGuard,
        AwakeHero,
        FeatKill,
        FeatButtonAbility
    }

    public enum HeroOption
    {
        FeatKillPercentage,
        FeatButtonAbilityPercentage,
    }

	private AllPlayerArrows? arrow;
    private PlayerTargetArrow? callTargetArrow;
    private HeroStatusModel status;
    private float featKillPer;
    private float featButtonAbilityPer;
    public override IStatusModel? Status => status;

#pragma warning disable CS8618
	public ExtremeAbilityButton Button { get; set; }
	public Hero(
        ) : base(
			RoleCore.BuildCrewmate(
				ExtremeRoleId.Hero,
				ColorPalette.HeroAmaIro),
            false, true, false, false,
            tab: OptionTab.CombinationTab)
    {
    }
#pragma warning restore CS8618

	public void SetCondition(
        OneForAllCondition cond)
    {
        status.cond = cond;
    }

    public void CreateAbility()
    {
        this.CreateNormalActivatingAbilityButton(
            "search",
			Resources.UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.HiroAcaSearch),
            abilityOff: CleanUp);
        this.Button.SetLabelToCrewmate();
    }

    public bool IsAbilityUse() =>
        status.cond == OneForAllCondition.FeatButtonAbility &&
        IRoleAbility.IsCommonUse();

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
        return;
    }

    public void ResetOnMeetingStart()
    {
        if (this.arrow != null)
        {
            this.arrow.SetActive(false);
        }
        if (this.callTargetArrow != null)
        {
            ResetTarget();
        }
    }

    public void Update(PlayerControl rolePlayer)
    {
        if (MeetingHud.Instance != null ||
            ShipStatus.Instance == null) { return; }
        if (!ShipStatus.Instance.enabled) { return; }

        if (this.callTargetArrow != null)
        {
            if (this.callTargetArrow.isActive)
            {
                this.callTargetArrow.Update();
            }
        }


        switch (status.cond)
        {
            case OneForAllCondition.NoGuard:
            case OneForAllCondition.AwakeHero:
                setButtonActive(false);
                break;
            case OneForAllCondition.FeatKill:
                this.CanKill = true;
                setButtonActive(false);
                break;
            case OneForAllCondition.FeatButtonAbility:
                this.CanKill = true;
                if (this.Button != null)
                {
                    if (this.Button.IsAbilityActive() &&
                        this.arrow != null)
                    {
                        this.arrow.Update(rolePlayer.GetTruePosition());
                    }
                }
                break;
            default:
                break;
        }

        if (status.cond == OneForAllCondition.FeatButtonAbility) { return; }

        int allCrew = 0;
        int deadCrew = 0;

        foreach (var player in GameData.Instance.AllPlayers.GetFastEnumerator())
        {
            if (player.IsDead || player.Disconnected)
            {
                ++deadCrew;
            }
            ++allCrew;
        }

        if (deadCrew > 0 && status.cond == OneForAllCondition.NoGuard)
        {
            status.cond = OneForAllCondition.AwakeHero;
            HeroAcademia.RpcUpdateHero(rolePlayer, OneForAllCondition.AwakeHero);
        }

        float deadPlayerPer = (float)deadCrew / (float)allCrew;
        if (deadPlayerPer > this.featButtonAbilityPer && status.cond != OneForAllCondition.FeatButtonAbility)
        {
            status.cond = OneForAllCondition.FeatButtonAbility;
            this.setButtonActive(true);
        }
        else if (deadPlayerPer > this.featKillPer)
        {
            status.cond = OneForAllCondition.FeatKill;
        }

    }

    public bool UseAbility()
    {
        if (this.arrow == null)
        {
            this.arrow = new AllPlayerArrows(
                PlayerControl.LocalPlayer.PlayerId);
        }
        this.arrow.SetActive(true);
        return true;
    }

    public void CleanUp()
    {
		if (this.arrow != null)
		{
			this.arrow.SetActive(false);
		}
    }

    public void SetEmergencyCallTarget(PlayerControl target)
    {
        if (this.callTargetArrow == null)
        {
            this.callTargetArrow = new PlayerTargetArrow(
                ColorPalette.VigilanteFujiIro);
        }

        this.callTargetArrow.SetActive(true);
        this.callTargetArrow.SetTargetPlayer(target);
    }

    public void ResetTarget()
    {
        this.callTargetArrow?.SetActive(false);
        this.callTargetArrow?.ResetTarget();
    }
    public void AllReset(PlayerControl rolePlayer)
    {
        HeroAcademia.UpdateVigilante(
            HeroAcademia.Condition.HeroDown,
            rolePlayer.PlayerId);
    }

    public override bool TryRolePlayerKillTo(PlayerControl rolePlayer, PlayerControl targetPlayer)
    {
        Assassin? assassin = ExtremeRoleManager.GameRole[targetPlayer.PlayerId] as Assassin;

        if (assassin != null && !assassin.CanKilledFromCrew)
        {
            Player.RpcUncheckMurderPlayer(
                rolePlayer.PlayerId,
                rolePlayer.PlayerId,
                byte.MaxValue);

            ExtremeRolesPlugin.ShipState.RpcReplaceDeadReason(
                rolePlayer.PlayerId,
                ExtremeShipStatus.PlayerStatus.Retaliate);

            return false;
        }

        return true;
    }


    public override void ExiledAction(
        PlayerControl rolePlayer)
    {
        HeroAcademia.UpdateVigilante(
            HeroAcademia.Condition.HeroDown,
            rolePlayer.PlayerId);
    }
    public override void RolePlayerKilledAction(
        PlayerControl rolePlayer, PlayerControl killerPlayer)
    {
        HeroAcademia.UpdateVigilante(
            HeroAcademia.Condition.HeroDown,
            rolePlayer.PlayerId);
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        IRoleAbility.CreateCommonAbilityOption(
            factory, 5.0f);
        factory.CreateIntOption(
            HeroOption.FeatKillPercentage,
            33, 20, 50, 1,
            format: OptionUnit.Percentage);
        factory.CreateIntOption(
            HeroOption.FeatButtonAbilityPercentage,
            66, 50, 80, 1,
            format: OptionUnit.Percentage);
    }

    protected override void RoleSpecificInit()
    {
        status = new HeroStatusModel();
        AbilityClass = new HeroAbilityHandler(status);
		var loader = this.Loader;
        this.featKillPer = loader.GetValue<HeroOption, int>(
            HeroOption.FeatKillPercentage) / 100.0f;
        this.featButtonAbilityPer = loader.GetValue<HeroOption, int>(
            HeroOption.FeatButtonAbilityPercentage) / 100.0f;

    }
    private void setButtonActive(bool active)
    {
        if (this.Button != null)
        {
            this.Button.SetButtonShow(active);
        }
    }
}
public sealed class Villain : MultiAssignRoleBase, IRoleAutoBuildAbility, IRoleUpdate, IRoleSpecialReset
{
    public enum VillanOption
    {
        VigilanteSeeTime,
    }

    private AllPlayerArrows? arrow;
    private Arrow? vigilanteArrow;
    private float vigilanteArrowTimer = 0.0f;
    private float vigilanteArrowTime = 0.0f;

#pragma warning disable CS8618
	public ExtremeAbilityButton Button { get; set; }
	public Villain(
        ) : base(
			RoleCore.BuildImpostor(ExtremeRoleId.Villain),
            true, false, true, true,
            tab: OptionTab.CombinationTab)
    {
    }
#pragma warning restore CS8618

	public void CreateAbility()
    {
        this.CreateNormalActivatingAbilityButton(
            "search",
			Resources.UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.HiroAcaSearch),
            abilityOff: CleanUp);
    }

    public bool IsAbilityUse() => IRoleAbility.IsCommonUse();

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
        return;
    }

    public void ResetOnMeetingStart()
    {
        if (this.arrow != null)
        {
            this.arrow.SetActive(false);
        }
        if (this.vigilanteArrow != null)
        {
            ResetVigilante();
        }
    }

    public void Update(PlayerControl rolePlayer)
    {
        if (MeetingHud.Instance != null) { return; }

        if (this.vigilanteArrow != null)
        {
            if (this.vigilanteArrowTimer > 0f)
            {
                this.vigilanteArrowTimer -= Time.deltaTime;
            }
            if (this.vigilanteArrowTimer <= 0f)
            {
                this.vigilanteArrow.SetActive(false);
            }
        }

        if (this.Button != null)
        {
            if (this.Button.IsAbilityActive() && this.arrow != null)
            {
                this.arrow.Update(rolePlayer.GetTruePosition());
            }
        }
    }

    public bool UseAbility()
    {
        if (this.arrow == null)
        {
            this.arrow = new AllPlayerArrows(
                PlayerControl.LocalPlayer.PlayerId);
        }
        this.arrow.SetActive(true);
        return true;
    }

    public void CleanUp()
    {
		if (this.arrow != null)
		{
			this.arrow.SetActive(false);
		}
    }

    public void SetVigilante(PlayerControl target)
    {
        if (this.vigilanteArrow == null)
        {
            this.vigilanteArrow = new Arrow(
                ColorPalette.VigilanteFujiIro);
        }
        this.vigilanteArrowTimer = this.vigilanteArrowTime;
        this.vigilanteArrow.SetActive(true);
        this.vigilanteArrow.UpdateTarget(target.GetTruePosition());
    }

    public void ResetVigilante()
    {
        this.vigilanteArrowTimer = 0.0f;
        this.vigilanteArrow?.SetActive(false);
    }

    public void AllReset(PlayerControl rolePlayer)
    {
        HeroAcademia.UpdateVigilante(
            HeroAcademia.Condition.VillainDown,
            rolePlayer.PlayerId);
    }

    public override void ExiledAction(
        PlayerControl rolePlayer)
    {
        HeroAcademia.UpdateVigilante(
            HeroAcademia.Condition.VillainDown,
            rolePlayer.PlayerId);
    }
    public override void RolePlayerKilledAction(
        PlayerControl rolePlayer, PlayerControl killerPlayer)
    {
        HeroAcademia.UpdateVigilante(
            HeroAcademia.Condition.VillainDown,
            rolePlayer.PlayerId);
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        IRoleAbility.CreateCommonAbilityOption(
            factory, 5.0f);
        factory.CreateFloatOption(
            VillanOption.VigilanteSeeTime,
            2.5f, 1.0f, 10.0f, 0.5f,
            format: OptionUnit.Second);
    }

    protected override void RoleSpecificInit()
    {
        AbilityClass = new VillainAbilityHandler();
        this.vigilanteArrowTime = this.Loader.GetValue<VillanOption, float>(
            VillanOption.VigilanteSeeTime);
        this.vigilanteArrowTimer = 0.0f;
    }

}
public sealed class Vigilante : MultiAssignRoleBase, IRoleAutoBuildAbility, IRoleUpdate, IRoleWinPlayerModifier
{
    public enum VigilanteCondition : byte
    {
        None,
        NewLawInTheShip,
        NewHeroForTheShip,
        NewVillainForTheShip,
        NewEnemyNeutralForTheShip,
    }

    public enum VigilanteOption
    {
        Range,
    }

    public VigilanteCondition Condition => status.cond;
    private VigilanteStatusModel status;
    private float range;
    private byte target;
    public override IStatusModel? Status => status;

#pragma warning disable CS8618
	public ExtremeAbilityButton Button { get; set; }
	public Vigilante(
        ) : base(
			RoleCore.BuildNeutral(
				ExtremeRoleId.Vigilante,
				ColorPalette.VigilanteFujiIro),
            false, false, false, false,
            tab: OptionTab.CombinationTab)
    {
    }
#pragma warning restore CS8618
	public void SetCondition(
        VigilanteCondition cond)
    {
        if (status.cond == VigilanteCondition.None ||
            cond == VigilanteCondition.NewLawInTheShip)
        {
            status.cond = cond;
        }
    }

    public void CreateAbility()
    {
        this.CreateNormalActivatingAbilityButton(
            "call",
			Resources.UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.VigilanteEmergencyCall),
            abilityOff: CleanUp);
        this.Button.SetLabelToCrewmate();
    }

    public bool IsAbilityUse()
    {
        this.target = byte.MaxValue;

        PlayerControl player = Player.GetClosestPlayerInRange(
            PlayerControl.LocalPlayer, this, this.range);

        if (player == null) { return false; }
        this.target = player.PlayerId;


        return IRoleAbility.IsCommonUse() && this.target != byte.MaxValue;
    }
    public void CleanUp()
    {
        HeroAcademia.RpcCleanUpEmergencyCall();
    }

    public void ModifiedWinPlayer(
        NetworkedPlayerInfo rolePlayerInfo,
        GameOverReason reason,
		in WinnerTempData winner)
    {
        switch (this.condition)
        {
            case VigilanteCondition.NewLawInTheShip:
                winner.AddWithPlus(rolePlayerInfo);
                break;
            case VigilanteCondition.NewHeroForTheShip:
                if (reason is
					GameOverReason.CrewmatesByTask or
					GameOverReason.CrewmatesByVote or
                    GameOverReason.CrewmateDisconnect)
                {
                    winner.AddWithPlus(rolePlayerInfo);
                }
                break;
            case VigilanteCondition.NewVillainForTheShip:
                if (reason is
					GameOverReason.ImpostorsByVote or
					GameOverReason.ImpostorsByKill or
					GameOverReason.ImpostorsBySabotage or
					GameOverReason.ImpostorDisconnect or
					(GameOverReason)RoleGameOverReason.AssassinationMarin or
					(GameOverReason)RoleGameOverReason.TeroristoTeroWithShip)
                {
					winner.AddWithPlus(rolePlayerInfo);
                }
                break;
            default:
                break;

        }
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
    }

    public void ResetOnMeetingStart()
    {
    }

    public bool UseAbility()
    {
        HeroAcademia.RpcEmergencyCall(
            PlayerControl.LocalPlayer,
            this.target);
        this.target = byte.MaxValue;
        return true;
    }


    public override bool IsSameTeam(SingleRoleBase targetRole) =>
        this.IsNeutralSameTeam(targetRole);

    public override string GetFullDescription()
    {
		var id = Core.Id;
		switch (this.condition)
        {
            case VigilanteCondition.NewHeroForTheShip:
                return Tr.GetString(
                    $"{id}CrewDescription");
            case VigilanteCondition.NewVillainForTheShip:
                return Tr.GetString(
                    $"{id}ImpDescription");
            case VigilanteCondition.NewEnemyNeutralForTheShip:
                return Tr.GetString(
                    $"{id}NeutDescription");
            default:
                return base.GetFullDescription();
        }
    }

    public override string GetRolePlayerNameTag(
        SingleRoleBase targetRole, byte targetPlayerId)
    {
        if (targetRole.Core.Id == ExtremeRoleId.Vigilante &&
            this.IsSameControlId(targetRole))
        {
            return Design.ColoedString(
                ColorPalette.VigilanteFujiIro,
                getInGameTag());
        }
        return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
    }


    public override string GetImportantText(bool isContainFakeTask = true)
    {
        switch (this.condition)
        {
            case VigilanteCondition.NewHeroForTheShip:
            case VigilanteCondition.NewVillainForTheShip:
            case VigilanteCondition.NewEnemyNeutralForTheShip:
                return this.createImportantText(isContainFakeTask);
            default:
                return base.GetImportantText(isContainFakeTask);
        }
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        IRoleAbility.CreateAbilityCountOption(
            factory, 2, 10, 5.0f);
        factory.CreateFloatOption(
            VigilanteOption.Range,
            3.0f, 1.2f, 5.0f, 0.1f);
    }

    protected override void RoleSpecificInit()
    {
        status = new VigilanteStatusModel();
        AbilityClass = new VigilanteAbilityHandler(status);
        this.range = this.Loader.GetValue<VigilanteOption, float>(
            VigilanteOption.Range);
    }

    public void Update(PlayerControl rolePlayer)
    {

        switch (status.cond)
        {
            case VigilanteCondition.None:
                this.UseVent = false;
                this.UseSabotage = false;
                this.CanKill = false;
                foreach (var (playerId, role) in ExtremeRoleManager.GameRole)
                {
                    var playerInfo = GameData.Instance.GetPlayerById(playerId);

                    if (playerInfo == null)
					{
						continue;
					}

					var id = role.Core.Id;
                    if (id == ExtremeRoleId.Hero && playerInfo.Disconnected)
                    {
                        HeroAcademia.RpcUpdateVigilante(
                            HeroAcademia.Condition.HeroDown,
                            playerInfo.PlayerId);
                        return;
                    }
                    else if (id == ExtremeRoleId.Villain && playerInfo.Disconnected)
                    {
                        HeroAcademia.RpcUpdateVigilante(
                            HeroAcademia.Condition.VillainDown,
                            playerInfo.PlayerId);
                        return;
                    }
                }
                break;
            case VigilanteCondition.NewVillainForTheShip:
                this.UseSabotage = true;
                this.UseVent = true;
                break;
            case VigilanteCondition.NewEnemyNeutralForTheShip:
                this.UseSabotage = false;
                this.UseVent = false;
                this.CanKill = true;
                break;
            default:
                break;
        }
    }

    private string createImportantText(bool isContainFakeTask)
    {
		var core = this.Core;
        string baseString = Design.ColoedString(
			core.Color,
            string.Format("{0}: {1}",
                Design.ColoedString(
					core.Color,
                    Tr.GetString(this.RoleName)),
                Tr.GetString(
                    $"{core.Id}{this.condition}ShortDescription")));

        if (isContainFakeTask && !this.HasTask)
        {
            string fakeTaskString = Design.ColoedString(
				core.Color,
                TranslationController.Instance.GetString(
                    StringNames.FakeTasks, System.Array.Empty<Il2CppSystem.Object>()));
            baseString = $"{baseString}\r\n{fakeTaskString}";
        }

        return baseString;
    }

    private string getInGameTag()
    {
        switch (this.condition)
        {
            case VigilanteCondition.NewHeroForTheShip:
                return " ♣";
            case VigilanteCondition.NewVillainForTheShip:
                return " ◆";
            case VigilanteCondition.NewEnemyNeutralForTheShip:
                return " ♠";
            default:
                return string.Empty;
        }
    }

}
