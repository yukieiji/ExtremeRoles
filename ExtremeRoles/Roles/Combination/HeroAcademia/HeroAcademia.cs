using ExtremeRoles.Extension.Player;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Factory.OptionBuilder;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.Neutral;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;
using ExtremeRoles.Roles.Combination.Avalon;
using Hazel;
using System.Collections.Generic;
using UnityEngine;




#nullable enable

namespace ExtremeRoles.Roles.Combination.HeroAcademia;

internal sealed class AllPlayerArrows
{
    private Dictionary<byte, PlayerControl> player = new Dictionary<byte, PlayerControl>();
    private Dictionary<byte, Arrow> arrow = new Dictionary<byte, Arrow>();
    private Dictionary<byte, TMPro.TextMeshPro> distance = new Dictionary<byte, TMPro.TextMeshPro>();

    public AllPlayerArrows(byte rolePlayerId)
    {
        player.Clear();
        arrow.Clear();
        distance.Clear();

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

                var text = Object.Instantiate(
                    Prefab.Text, playerArrow.Main.transform);
                text.fontSize = text.fontSizeMax = text.fontSizeMin = 3.25f;
                Object.Destroy(text.fontMaterial);
                text.fontMaterial = Object.Instantiate(
                    HudManager.Instance.UseButton.buttonLabelText.fontMaterial,
                    playerArrow.Main.transform);
                text.gameObject.layer = 5;
                text.alignment = TMPro.TextAlignmentOptions.Center;
                text.transform.localPosition = new Vector3(0.0f, 0.0f, -800f);
                text.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

                distance.Add(player.PlayerId, text);
                this.player.Add(player.PlayerId, player);
                arrow.Add(player.PlayerId, playerArrow);
            }
        }

    }

    public void SetActive(bool active)
    {
        List<byte> removePlayer = new List<byte>();
        foreach (var (playerId, playerCont) in player)
        {
            if (playerCont == null)
            {
                arrow[playerId].SetActive(active);
                distance[playerId].gameObject.SetActive(active);
                removePlayer.Add(playerId);
                continue;
            }

            arrow[playerId].SetActive(active);
            distance[playerId].gameObject.SetActive(active);

            if (playerCont.Data.IsDead ||
                playerCont.Data.Disconnected)
            {
                arrow[playerId].SetActive(false);
                distance[playerId].gameObject.SetActive(false);
            }
        }

        foreach (byte playerId in removePlayer)
        {
            Object.Destroy(distance[playerId]);
            arrow[playerId].Clear();
            distance.Remove(playerId);
            arrow.Remove(playerId);
            player.Remove(playerId);
        }
    }

    public void Update(Vector2 rolePlayerPos)
    {
        foreach(var (playerId, playerCont) in player)
        {
            float diss = Vector2.Distance(rolePlayerPos, playerCont.GetTruePosition());

            distance[playerId].text = Design.ColoredString(
                Color.black, $"{diss:F1}");
            arrow[playerId].UpdateTarget(playerCont.transform.position);
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
        arrow = new Arrow(color);
    }
    public void SetActive(bool active)
    {
        isActive = active;
        arrow.SetActive(active);
    }
    public void ResetTarget()
    {
        targetPlayer = null;
    }

    public void SetTargetPlayer(PlayerControl player)
    {
        targetPlayer = player;
    }

    public void Update()
    {
        if (targetPlayer == null) { return; }

        arrow.UpdateTarget(
            targetPlayer.GetTruePosition());

    }

}

public sealed class HeroAcademiaRole : ConstCombinationRoleManagerBase
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
    public HeroAcademiaRole() : base(
		CombinationRoleType.HeroAca,
        Name, DefaultColor, 3,
        GameSystem.MaxImposterNum)
    {
        Roles.Add(new Hero());
        Roles.Add(new Villain());
        Roles.Add(new Vigilante());
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
        status.Cond = cond;
    }

    public void CreateAbility()
    {
        this.CreateNormalActivatingAbilityButton(
            "search",
			UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.HiroAcaSearch),
            abilityOff: CleanUp);
        Button.SetLabelToCrewmate();
    }

    public bool IsAbilityUse() =>
        status.Cond == OneForAllCondition.FeatButtonAbility &&
        IRoleAbility.IsCommonUse();

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
        return;
    }

    public void ResetOnMeetingStart()
    {
        if (arrow != null)
        {
            arrow.SetActive(false);
        }
        if (callTargetArrow != null)
        {
            ResetTarget();
        }
    }

    public void Update(PlayerControl rolePlayer)
    {
        if (!GameProgressSystem.IsTaskPhase)
		{
			return;
		}

        if (callTargetArrow != null &&
			callTargetArrow.isActive)
        {
			callTargetArrow.Update();
		}


        switch (status.Cond)
        {
            case OneForAllCondition.NoGuard:
            case OneForAllCondition.AwakeHero:
                setButtonActive(false);
                break;
            case OneForAllCondition.FeatKill:
                CanKill = true;
                setButtonActive(false);
                break;
            case OneForAllCondition.FeatButtonAbility:
                CanKill = true;
                if (Button != null)
                {
                    if (Button.IsAbilityActive() &&
                        arrow != null)
                    {
                        arrow.Update(rolePlayer.GetTruePosition());
                    }
                }
                break;
            default:
                break;
        }

        if (status.Cond == OneForAllCondition.FeatButtonAbility) { return; }

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

        if (deadCrew > 0 && status.Cond == OneForAllCondition.NoGuard)
        {
            status.Cond = OneForAllCondition.AwakeHero;
            HeroAcademiaRole.RpcUpdateHero(rolePlayer, OneForAllCondition.AwakeHero);
        }

        float deadPlayerPer = deadCrew / (float)allCrew;
        if (deadPlayerPer > featButtonAbilityPer && status.Cond != OneForAllCondition.FeatButtonAbility)
        {
            status.Cond = OneForAllCondition.FeatButtonAbility;
            setButtonActive(true);
        }
        else if (deadPlayerPer > featKillPer)
        {
            status.Cond = OneForAllCondition.FeatKill;
        }

    }

    public bool UseAbility()
    {
        if (arrow == null)
        {
            arrow = new AllPlayerArrows(
                PlayerControl.LocalPlayer.PlayerId);
        }
        arrow.SetActive(true);
        return true;
    }

    public void CleanUp()
    {
		if (arrow != null)
		{
			arrow.SetActive(false);
		}
    }

    public void SetEmergencyCallTarget(PlayerControl target)
    {
        if (callTargetArrow == null)
        {
            callTargetArrow = new PlayerTargetArrow(
                ColorPalette.VigilanteFujiIro);
        }

        callTargetArrow.SetActive(true);
        callTargetArrow.SetTargetPlayer(target);
    }

    public void ResetTarget()
    {
        callTargetArrow?.SetActive(false);
        callTargetArrow?.ResetTarget();
    }
    public void AllReset(PlayerControl rolePlayer)
    {
        HeroAcademiaRole.UpdateVigilante(
            HeroAcademiaRole.Condition.HeroDown,
            rolePlayer.PlayerId);
    }

    public bool TryRolePlayerKillTo(PlayerControl rolePlayer, PlayerControl targetPlayer)
    {
        Assassin? assassin = ExtremeRoleManager.GameRole[targetPlayer.PlayerId] as Assassin;

        if (assassin != null && 
			assassin.Status is AssassinStatusModel status &&
			!status.CanKilledFromCrew)
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
        HeroAcademiaRole.UpdateVigilante(
            HeroAcademiaRole.Condition.HeroDown,
            rolePlayer.PlayerId);
    }
    public override void RolePlayerKilledAction(
        PlayerControl rolePlayer, PlayerControl killerPlayer)
    {
        HeroAcademiaRole.UpdateVigilante(
            HeroAcademiaRole.Condition.HeroDown,
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
		var loader = Loader;
        featKillPer = loader.GetValue<HeroOption, int>(
            HeroOption.FeatKillPercentage) / 100.0f;
        featButtonAbilityPer = loader.GetValue<HeroOption, int>(
            HeroOption.FeatButtonAbilityPercentage) / 100.0f;

    }
    private void setButtonActive(bool active)
    {
        if (Button != null)
        {
            Button.SetButtonShow(active);
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
			UnityObjectLoader.LoadSpriteFromResources(
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
        if (arrow != null)
        {
            arrow.SetActive(false);
        }
        if (vigilanteArrow != null)
        {
            ResetVigilante();
        }
    }

    public void Update(PlayerControl rolePlayer)
    {
        if (MeetingHud.Instance != null) { return; }

        if (vigilanteArrow != null)
        {
            if (vigilanteArrowTimer > 0f)
            {
                vigilanteArrowTimer -= Time.deltaTime;
            }
            if (vigilanteArrowTimer <= 0f)
            {
                vigilanteArrow.SetActive(false);
            }
        }

        if (Button != null)
        {
            if (Button.IsAbilityActive() && arrow != null)
            {
                arrow.Update(rolePlayer.GetTruePosition());
            }
        }
    }

    public bool UseAbility()
    {
        if (arrow == null)
        {
            arrow = new AllPlayerArrows(
                PlayerControl.LocalPlayer.PlayerId);
        }
        arrow.SetActive(true);
        return true;
    }

    public void CleanUp()
    {
		if (arrow != null)
		{
			arrow.SetActive(false);
		}
    }

    public void SetVigilante(PlayerControl target)
    {
        if (vigilanteArrow == null)
        {
            vigilanteArrow = new Arrow(
                ColorPalette.VigilanteFujiIro);
        }
        vigilanteArrowTimer = vigilanteArrowTime;
        vigilanteArrow.SetActive(true);
        vigilanteArrow.UpdateTarget(target.GetTruePosition());
    }

    public void ResetVigilante()
    {
        vigilanteArrowTimer = 0.0f;
        vigilanteArrow?.SetActive(false);
    }

    public void AllReset(PlayerControl rolePlayer)
    {
        HeroAcademiaRole.UpdateVigilante(
            HeroAcademiaRole.Condition.VillainDown,
            rolePlayer.PlayerId);
    }

    public override void ExiledAction(
        PlayerControl rolePlayer)
    {
        HeroAcademiaRole.UpdateVigilante(
            HeroAcademiaRole.Condition.VillainDown,
            rolePlayer.PlayerId);
    }
    public override void RolePlayerKilledAction(
        PlayerControl rolePlayer, PlayerControl killerPlayer)
    {
        HeroAcademiaRole.UpdateVigilante(
            HeroAcademiaRole.Condition.VillainDown,
            rolePlayer.PlayerId);
    }

    protected override void CreateSpecificOption(OptionCategoryScope<AutoParentSetBuilder> categoryScope)
	{
		var factory = categoryScope.Builder;
        IRoleAbility.CreateCommonAbilityOption(
            factory, 5.0f);
        factory.CreateFloatOption(
            VillanOption.VigilanteSeeTime,
            2.5f, 1.0f, 10.0f, 0.5f,
            format: OptionUnit.Second);
    }

    protected override void RoleSpecificInit()
    {
        vigilanteArrowTime = Loader.GetValue<VillanOption, float>(
            VillanOption.VigilanteSeeTime);
        vigilanteArrowTimer = 0.0f;
		this.AbilityClass = new VillainAbilityHandler();
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

    public VigilanteCondition Condition => this.status is null ? VigilanteCondition.None : this.status.Condition;
    private VigilanteStatusModel? status;
    private float range;
    private byte target;
    public override IStatusModel? Status => status;

#pragma warning disable CS8618
	public ExtremeAbilityButton Button { get; set; }
	public Vigilante(
        ) : base(
			RoleCore.BuildNeutral(ExtremeRoleId.Vigilante, ColorPalette.VigilanteFujiIro),
            false, false, false, false,
            tab: OptionTab.CombinationTab)
    {
    }
#pragma warning restore CS8618
	public void SetCondition(
        VigilanteCondition cond)
    {
		if (this.status is null)
		{
			return;
		}

        if (this.Condition == VigilanteCondition.None ||
            cond == VigilanteCondition.NewLawInTheShip)
        {
            this.status.Condition = cond;
        }
    }

    public void CreateAbility()
    {
        this.CreateNormalActivatingAbilityButton(
            "call",
			UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.VigilanteEmergencyCall),
            abilityOff: CleanUp);
        Button.SetLabelToCrewmate();
    }

    public bool IsAbilityUse()
    {
        target = byte.MaxValue;

        PlayerControl player = Player.GetClosestPlayerInRange(
            PlayerControl.LocalPlayer, this, range);

        if (player == null) { return false; }
        target = player.PlayerId;


        return IRoleAbility.IsCommonUse() && target != byte.MaxValue;
    }
    public void CleanUp()
    {
        HeroAcademiaRole.RpcCleanUpEmergencyCall();
    }

    public void ModifiedWinPlayer(
        NetworkedPlayerInfo rolePlayerInfo,
        GameOverReason reason,
		in WinnerTempData winner)
    {
        switch (this.Condition)
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
        HeroAcademiaRole.RpcEmergencyCall(
            PlayerControl.LocalPlayer,
            target);
        target = byte.MaxValue;
        return true;
    }


    public override bool IsSameTeam(SingleRoleBase targetRole) =>
        this.IsNeutralSameTeam(targetRole);

    public override string GetFullDescription()
    {
		switch (this.Condition)
        {
            case VigilanteCondition.NewHeroForTheShip:
                return Tr.GetString(
                    $"{this.Core.Id}CrewDescription");
            case VigilanteCondition.NewVillainForTheShip:
                return Tr.GetString(
                    $"{this.Core.Id}ImpDescription");
            case VigilanteCondition.NewEnemyNeutralForTheShip:
                return Tr.GetString(
                    $"{this.Core.Id}NeutDescription");
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
            return Design.ColoredString(
                ColorPalette.VigilanteFujiIro,
                getInGameTag());
        }
        return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
    }


    public override string GetImportantText(bool isContainFakeTask = true)
    {
        switch (this.Condition)
        {
            case VigilanteCondition.NewHeroForTheShip:
            case VigilanteCondition.NewVillainForTheShip:
            case VigilanteCondition.NewEnemyNeutralForTheShip:
                return createImportantText(isContainFakeTask);
            default:
                return base.GetImportantText(isContainFakeTask);
        }
    }

    protected override void CreateSpecificOption(OptionCategoryScope<AutoParentSetBuilder> categoryScope)
	{
		var factory = categoryScope.Builder;
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
        range = Loader.GetValue<VigilanteOption, float>(
            VigilanteOption.Range);
    }

    public void Update(PlayerControl rolePlayer)
    {

        switch (this.Condition)
        {
            case VigilanteCondition.None:
                UseVent = false;
                UseSabotage = false;
                CanKill = false;
                foreach (var (playerId, role) in ExtremeRoleManager.GameRole)
                {
                    var playerInfo = GameData.Instance.GetPlayerById(playerId);

                    if (playerInfo == null)
					{
						continue;
					}

					var id = this.Core.Id;
                    if (id == ExtremeRoleId.Hero && playerInfo.Disconnected)
                    {
                        HeroAcademiaRole.RpcUpdateVigilante(
                            HeroAcademiaRole.Condition.HeroDown,
                            playerInfo.PlayerId);
                        return;
                    }
                    else if (id == ExtremeRoleId.Villain && playerInfo.Disconnected)
                    {
                        HeroAcademiaRole.RpcUpdateVigilante(
                            HeroAcademiaRole.Condition.VillainDown,
                            playerInfo.PlayerId);
                        return;
                    }
                }
                break;
            case VigilanteCondition.NewVillainForTheShip:
                UseSabotage = true;
                UseVent = true;
                break;
            case VigilanteCondition.NewEnemyNeutralForTheShip:
                UseSabotage = false;
                UseVent = false;
                CanKill = true;
                break;
            default:
                break;
        }
    }

    private string createImportantText(bool isContainFakeTask)
    {
        string baseString = Design.ColoredString(
			this.Core.Color,
            string.Format("{0}: {1}",
                Design.ColoredString(
					this.Core.Color,
					Tr.GetString(RoleName)),
                Tr.GetString(
                    $"{this.Core.Id}{this.Condition}ShortDescription")));

        if (isContainFakeTask && !HasTask)
        {
            string fakeTaskString = Design.ColoredString(
				this.Core.Color,
				TranslationController.Instance.GetString(
                    StringNames.FakeTasks, System.Array.Empty<Il2CppSystem.Object>()));
            baseString = $"{baseString}\r\n{fakeTaskString}";
        }

        return baseString;
    }

    private string getInGameTag()
    {
        switch (this.Condition)
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
