using System.Collections.Generic;

using UnityEngine;

using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Extension.Neutral;
using ExtremeRoles.Resources;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Module.ExtremeShipStatus;

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
        
        foreach (var player in CachedPlayerControl.AllPlayerControls)
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
                text.fontSize = text.fontSizeMax = text.fontSizeMin = 3.8f;
                Object.Destroy(text.fontMaterial);
                text.fontMaterial = UnityEngine.Object.Instantiate(
                    FastDestroyableSingleton<HudManager>.Instance.UseButton.buttonLabelText.fontMaterial,
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
    private PlayerControl targetPlayer;
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
        Name, new Color(255f, 255f, 255f), 3,
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

        foreach (GameData.PlayerInfo player in 
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

            Vigilante vigilante = ExtremeRoleManager.GetSafeCastedRole<Vigilante>(player.PlayerId);

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

            if (heroPlayer.protectedByGuardian)
            {
                heroPlayer.RemoveProtection();
            }
            if (villanPlayer.protectedByGuardian)
            {
                villanPlayer.RemoveProtection();
            }

            heroPlayer.MurderPlayer(villanPlayer);
            villanPlayer.MurderPlayer(heroPlayer);

            foreach (var role in ExtremeRoleManager.GameRole.Values)
            {
                if (role.Id == ExtremeRoleId.Vigilante)
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

public sealed class Hero : MultiAssignRoleBase, IRoleAbility, IRoleUpdate, IRoleSpecialReset
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

    public ExtremeAbilityButton Button
    {
        get => this.searchButton;
        set
        {
            this.searchButton = value;
        }
    }

    private ExtremeAbilityButton searchButton;
    private AllPlayerArrows arrow;
    private PlayerTargetArrow callTargetArrow;
    private OneForAllCondition cond;
    private float featKillPer;
    private float featButtonAbilityPer;

    public Hero(
        ) : base(
            ExtremeRoleId.Hero,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Hero.ToString(),
            ColorPalette.HeroAmaIro,
            false, true, false, false,
            tab: OptionTab.Combination)
    { }
    public void SetCondition(
        OneForAllCondition cond)
    {
        this.cond = cond;
    }

    public void CreateAbility()
    {
        this.CreateNormalAbilityButton(
            "search",
            Loader.CreateSpriteFromResources(
                Path.HiroAcaSearch),
            abilityOff: CleanUp);
        this.Button.SetLabelToCrewmate();
    }

    public bool IsAbilityUse() => 
        this.cond == OneForAllCondition.FeatButtonAbility && 
        this.IsCommonUse();

    public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
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
            CachedShipStatus.Instance == null) { return; }
        if (!CachedShipStatus.Instance.enabled) { return; }

        if (this.callTargetArrow != null)
        {
            if (this.callTargetArrow.isActive)
            {
                this.callTargetArrow.Update();
            }
        }


        switch (this.cond)
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

        if (this.cond == OneForAllCondition.FeatButtonAbility) { return; }

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

        if (deadCrew > 0 && this.cond == OneForAllCondition.NoGuard)
        {
            this.cond = OneForAllCondition.AwakeHero;
            HeroAcademia.RpcUpdateHero(rolePlayer, OneForAllCondition.AwakeHero);
        }

        float deadPlayerPer = (float)deadCrew / (float)allCrew;
        if (deadPlayerPer > this.featButtonAbilityPer && this.cond != OneForAllCondition.FeatButtonAbility)
        {
            this.cond = OneForAllCondition.FeatButtonAbility;
            this.setButtonActive(true);
        }
        else if (deadPlayerPer > this.featKillPer)
        {
            this.cond = OneForAllCondition.FeatKill;
        }

    }

    public bool UseAbility()
    {
        if (this.arrow == null)
        {
            this.arrow = new AllPlayerArrows(
                CachedPlayerControl.LocalPlayer.PlayerId);
        }
        this.arrow.SetActive(true);
        return true;
    }

    public void CleanUp()
    {
        this.arrow.SetActive(false);
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
        Assassin assassin = ExtremeRoleManager.GameRole[targetPlayer.PlayerId] as Assassin;

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

    public override bool TryRolePlayerKilledFrom(
        PlayerControl rolePlayer, PlayerControl fromPlayer)
    {
        var fromRole = ExtremeRoleManager.GameRole[fromPlayer.PlayerId];

        if (fromRole.Id == ExtremeRoleId.Villain)
        {
            HeroAcademia.RpcDrawHeroAndVillan(
                rolePlayer, fromPlayer);
            return false;
        }
        else if (fromRole.IsImpostor() && this.cond != OneForAllCondition.NoGuard)
        {
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
        IOptionInfo parentOps)
    {
        this.CreateCommonAbilityOption(
            parentOps, 5.0f);
        CreateIntOption(
            HeroOption.FeatKillPercentage,
            33, 20, 50, 1, parentOps,
            format: OptionUnit.Percentage);
        CreateIntOption(
            HeroOption.FeatButtonAbilityPercentage,
            66, 50, 80, 1, parentOps,
            format: OptionUnit.Percentage);
    }

    protected override void RoleSpecificInit()
    {
        this.RoleAbilityInit();

        this.featKillPer = AllOptionHolder.Instance.GetValue<int>(
            GetRoleOptionId(HeroOption.FeatKillPercentage)) / 100.0f;
        this.featButtonAbilityPer = AllOptionHolder.Instance.GetValue<int>(
            GetRoleOptionId(HeroOption.FeatButtonAbilityPercentage)) / 100.0f;

    }
    private void setButtonActive(bool active)
    {
        if (this.Button != null)
        {
            this.Button.SetButtonShow(active);
        }
    }
}
public sealed class Villain : MultiAssignRoleBase, IRoleAbility, IRoleUpdate, IRoleSpecialReset
{
    public enum VillanOption
    {
        VigilanteSeeTime,
    }

    public ExtremeAbilityButton Button
    {
        get => this.searchButton;
        set
        {
            this.searchButton = value;
        }
    }

    private ExtremeAbilityButton searchButton;
    private AllPlayerArrows arrow;
    private Arrow vigilanteArrow;
    private float vigilanteArrowTimer = 0.0f;
    private float vigilanteArrowTime = 0.0f;

    public Villain(
        ) : base(
            ExtremeRoleId.Villain,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.Villain.ToString(),
            Palette.ImpostorRed,
            true, false, true, true,
            tab: OptionTab.Combination)
    { }

    public void CreateAbility()
    {
        this.CreateNormalAbilityButton(
            "search",
            Loader.CreateSpriteFromResources(
                Path.HiroAcaSearch),
            abilityOff: CleanUp);
    }

    public bool IsAbilityUse() => this.IsCommonUse();

    public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
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
                CachedPlayerControl.LocalPlayer.PlayerId);
        }
        this.arrow.SetActive(true);
        return true;
    }

    public void CleanUp()
    {
        this.arrow.SetActive(false);
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

    public override bool TryRolePlayerKilledFrom(
        PlayerControl rolePlayer, PlayerControl fromPlayer)
    {
        var fromRole = ExtremeRoleManager.GameRole[fromPlayer.PlayerId];
        if (fromRole.Id == ExtremeRoleId.Hero)
        {
            HeroAcademia.RpcDrawHeroAndVillan(
                fromPlayer, rolePlayer);
            return false;
        }
        else if (fromRole.IsCrewmate())
        {
            return false;
        }
        return true;
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
        IOptionInfo parentOps)
    {
        this.CreateCommonAbilityOption(
            parentOps, 5.0f);
        this.CreateFloatOption(
            VillanOption.VigilanteSeeTime,
            2.5f, 1.0f, 10.0f, 0.5f, parentOps,
            format: OptionUnit.Second);
    }

    protected override void RoleSpecificInit()
    {
        this.RoleAbilityInit();
        this.vigilanteArrowTime = AllOptionHolder.Instance.GetValue<float>(
            GetRoleOptionId(VillanOption.VigilanteSeeTime));
        this.vigilanteArrowTimer = 0.0f;
    }

}
public sealed class Vigilante : MultiAssignRoleBase, IRoleAbility, IRoleUpdate, IRoleWinPlayerModifier
{
    public enum VigilanteCondition
    {
        None = byte.MinValue,
        NewLawInTheShip,
        NewHeroForTheShip,
        NewVillainForTheShip,
        NewEnemyNeutralForTheShip,
    }

    public enum VigilanteOption
    {
        Range,
    }

    public ExtremeAbilityButton Button
    {
        get => this.callButton;
        set
        {
            this.callButton = value;
        }
    }

    public VigilanteCondition Condition => this.condition;

    private ExtremeAbilityButton callButton;
    private VigilanteCondition condition = VigilanteCondition.None;
    private float range;
    private byte target;

    public Vigilante(
        ) : base(
            ExtremeRoleId.Vigilante,
            ExtremeRoleType.Neutral,
            ExtremeRoleId.Vigilante.ToString(),
            ColorPalette.VigilanteFujiIro,
            false, false, false, false,
            tab: OptionTab.Combination)
    { }

    public void SetCondition(
        VigilanteCondition cond)
    {
        if (this.condition == VigilanteCondition.None ||
            cond == VigilanteCondition.NewLawInTheShip)
        {
            this.condition = cond;
        }
    }

    public void CreateAbility()
    {
        this.CreateNormalAbilityButton(
            "call",
            Loader.CreateSpriteFromResources(
                Path.VigilanteEmergencyCall),
            abilityOff: CleanUp);
        this.Button.SetLabelToCrewmate();
    }

    public bool IsAbilityUse()
    {
        this.target = byte.MaxValue;

        PlayerControl player = Player.GetClosestPlayerInRange(
            CachedPlayerControl.LocalPlayer, this, this.range);

        if (player == null) { return false; }
        this.target = player.PlayerId;


        return this.IsCommonUse() && this.target != byte.MaxValue;
    }
    public void CleanUp()
    {
        HeroAcademia.RpcCleanUpEmergencyCall();
    }

    public void ModifiedWinPlayer(
        GameData.PlayerInfo rolePlayerInfo,
        GameOverReason reason,
        ref Il2CppSystem.Collections.Generic.List<WinningPlayerData> winner,
        ref List<GameData.PlayerInfo> pulsWinner)
    {
        switch (this.condition)
        {
            case VigilanteCondition.NewLawInTheShip:
                this.AddWinner(rolePlayerInfo, winner, pulsWinner);
                break;
            case VigilanteCondition.NewHeroForTheShip:
                if (reason == GameOverReason.HumansByTask ||
                    reason == GameOverReason.HumansByVote ||
                    reason == GameOverReason.HumansDisconnect)
                {
                    this.AddWinner(rolePlayerInfo, winner, pulsWinner);
                }
                break;
            case VigilanteCondition.NewVillainForTheShip:
                if (reason == GameOverReason.ImpostorByVote ||
                    reason == GameOverReason.ImpostorByKill ||
                    reason == GameOverReason.ImpostorBySabotage ||
                    reason == GameOverReason.ImpostorDisconnect ||
                    reason == (GameOverReason)RoleGameOverReason.AssassinationMarin)
                {
                    this.AddWinner(rolePlayerInfo, winner, pulsWinner);
                }
                break;
            default:
                break;

        }
    }

    public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
    {
        return;
    }

    public void ResetOnMeetingStart()
    {
        return;
    }

    public bool UseAbility()
    {
        HeroAcademia.RpcEmergencyCall(
            PlayerControl.LocalPlayer,
            this.target);
        this.target = byte.MaxValue;
        return true;
    }

    public override bool TryRolePlayerKilledFrom(
        PlayerControl rolePlayer, PlayerControl fromPlayer)
    {
        var fromRole = ExtremeRoleManager.GameRole[fromPlayer.PlayerId];
        if (fromRole.Id == ExtremeRoleId.Hero && 
            this.condition != VigilanteCondition.NewEnemyNeutralForTheShip)
        {
            return false;
        }
        return true;
    }

    public override bool IsSameTeam(SingleRoleBase targetRole) => 
        this.IsNeutralSameTeam(targetRole);

    public override string GetFullDescription()
    {
        switch (this.condition)
        {
            case VigilanteCondition.NewHeroForTheShip:
                return Translation.GetString(
                    $"{this.Id}CrewDescription");
            case VigilanteCondition.NewVillainForTheShip:
                return Translation.GetString(
                    $"{this.Id}ImpDescription");
            case VigilanteCondition.NewEnemyNeutralForTheShip:
                return Translation.GetString(
                    $"{this.Id}NeutDescription");
            default:
                return base.GetFullDescription();
        }
    }

    public override string GetRolePlayerNameTag(
        SingleRoleBase targetRole, byte targetPlayerId)
    {
        if (targetRole.Id == ExtremeRoleId.Vigilante &&
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
        IOptionInfo parentOps)
    {
        this.CreateAbilityCountOption(
            parentOps, 2, 10, 5.0f);
        CreateFloatOption(
            VigilanteOption.Range,
            3.0f, 1.2f, 5.0f, 0.1f, parentOps);
    }

    protected override void RoleSpecificInit()
    {
        this.RoleAbilityInit();
        this.range = AllOptionHolder.Instance.GetValue<float>(
            GetRoleOptionId(VigilanteOption.Range));
    }

    public void Update(PlayerControl rolePlayer)
    {

        switch (this.condition)
        {
            case VigilanteCondition.None:
                this.UseVent = false;
                this.UseSabotage = false;
                this.CanKill = false;
                foreach (var (playerId, role) in ExtremeRoleManager.GameRole)
                {
                    var playerInfo = GameData.Instance.GetPlayerById(playerId);

                    if (playerInfo == null) { continue; }

                    if (role.Id == ExtremeRoleId.Hero && playerInfo.Disconnected)
                    {
                        HeroAcademia.RpcUpdateVigilante(
                            HeroAcademia.Condition.HeroDown,
                            playerInfo.PlayerId);
                        return;
                    }
                    else if (role.Id == ExtremeRoleId.Villain && playerInfo.Disconnected)
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
        string baseString = Design.ColoedString(
            this.NameColor,
            string.Format("{0}: {1}",
                Design.ColoedString(
                    this.NameColor,
                    Translation.GetString(this.RoleName)),
                Translation.GetString(
                    $"{this.Id}{this.condition}ShortDescription")));

        if (isContainFakeTask && !this.HasTask)
        {
            string fakeTaskString = Design.ColoedString(
                this.NameColor,
                FastDestroyableSingleton<TranslationController>.Instance.GetString(
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
