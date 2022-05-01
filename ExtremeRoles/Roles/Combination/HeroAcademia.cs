using System.Collections.Generic;

using UnityEngine;

using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.RoleAbilityButton;
using ExtremeRoles.Resources;

namespace ExtremeRoles.Roles.Combination
{
    internal class AllPlayerArrows
    {
        private Dictionary<byte, PlayerControl> player = new Dictionary<byte, PlayerControl>();
        private Dictionary<byte, Arrow> arrow = new Dictionary<byte, Arrow>();
        private Dictionary<byte, TMPro.TextMeshPro> distance = new Dictionary<byte, TMPro.TextMeshPro>();

        public AllPlayerArrows(byte rolePlayerId)
        {
            this.player.Clear();
            this.arrow.Clear();
            this.distance.Clear();
            
            foreach (var player in PlayerControl.AllPlayerControls)
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
                    text.fontSize = 2;
                    text.alignment = TMPro.TextAlignmentOptions.Center;
                    text.transform.position = text.transform.position + new Vector3(0, 0, 800f);

                    this.distance.Add(player.PlayerId, text);
                    this.player.Add(player.PlayerId, player);
                    this.arrow.Add(player.PlayerId, playerArrow);
                }
            }

        }

        public void SetActive(bool active)
        {
            foreach (var playerId in this.player.Keys)
            {
                this.arrow[playerId].SetActive(active);
                this.distance[playerId].gameObject.SetActive(active);
            }
        }

        public void Update(Vector2 rolePlayerPos)
        {
            foreach(var (playerId, playerCont) in this.player)
            {
                var diss = Vector2.Distance(rolePlayerPos, playerCont.GetTruePosition());
                this.distance[playerId].text = $"{diss:F1}";
                this.arrow[playerId].UpdateTarget(playerCont.transform.position);
            }
        }
    }

    public class HeroAcademia : ConstCombinationRoleManagerBase
    {
        public enum Command
        {
            UpdateHero,
            UpdateVigilante,
            DrawHeroAndVillan
        }
        public enum Condition
        {
            HeroDown,
            VillainDown,
        }

        public const string Name = "HeroAca";
        public HeroAcademia() : base(
            Name, new Color(255f, 255f, 255f), 3,
            OptionHolder.MaxImposterNum)
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
                case Command.DrawHeroAndVillan:
                    byte heroPlayerId = reader.ReadByte();
                    byte villanPlayerId = reader.ReadByte();
                    drawHeroAndVillan(heroPlayerId, villanPlayerId);
                    break;
                default:
                    break;
            }

        }

        public static void RpcDrawHeroAndVillan(
            PlayerControl hero, PlayerControl villan)
        {
            RPCOperator.Call(
                PlayerControl.LocalPlayer.NetId,
                RPCOperator.Command.HeroHeroAcademia,
                new List<byte>
                {
                    (byte)Command.DrawHeroAndVillan,
                    hero.PlayerId,
                    villan.PlayerId,
                });
            drawHeroAndVillan(hero.PlayerId, villan.PlayerId);
        }
        public static void RpcUpdateHero(
            PlayerControl hero,
            Hero.OneForAllCondition newCond)
        {
            RPCOperator.Call(
                PlayerControl.LocalPlayer.NetId,
                RPCOperator.Command.HeroHeroAcademia,
                new List<byte>
                {
                    (byte)Command.UpdateHero,
                    hero.PlayerId,
                    (byte)newCond,
                });
            
        }

        public static void UpdateVigilante(
            Condition cond)
        {

            int crewNum = 0;
            int impNum = 0;
            Vigilante vigilante = null;

            foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers)
            {
                var role = ExtremeRoleManager.GameRole[playerInfo.PlayerId];
                if (role.IsCrewmate())
                {
                    ++impNum;
                }
                else if (role.IsImpostor())
                {
                    ++crewNum;
                }
                if (role.Id == ExtremeRoleId.Vigilante)
                {
                    vigilante = (Vigilante)role;
                }
            }

            if (vigilante == null) { return; }

            switch (cond)
            {
                case Condition.HeroDown:
                    if ((crewNum - 1) <= impNum)
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
                    if (impNum - 1 <= 0)
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

        private static void drawHeroAndVillan(
            byte heroPlayerId, byte villanPlayerId)
        {
            PlayerControl heroPlayer = Player.GetPlayerControlById(heroPlayerId);
            PlayerControl villanPlayer = Player.GetPlayerControlById(villanPlayerId);

            if (heroPlayer != null && villanPlayer != null)
            {

                ExtremeRolesPlugin.GameDataStore.WinCheckDisable = true;

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

                var hero = ExtremeRoleManager.GameRole[heroPlayerId] as MultiAssignRoleBase;
                var villain = ExtremeRoleManager.GameRole[villanPlayerId] as MultiAssignRoleBase;

                if (hero?.AnotherRole != null)
                {
                    hero.AnotherRole.RolePlayerKilledAction(
                        heroPlayer, villanPlayer);
                }
                if (villain?.AnotherRole != null)
                {
                    villain.AnotherRole.RolePlayerKilledAction(
                        heroPlayer, villanPlayer);
                }

                var player = PlayerControl.LocalPlayer;

                var localRole = ExtremeRoleManager.GameRole[player.PlayerId];
                var vigilante = localRole as Vigilante;
                if (vigilante != null)
                {
                    vigilante.SetCondition(
                        Vigilante.VigilanteCondition.NewLawInTheShip);
                }

                ExtremeRolesPlugin.GameDataStore.WinCheckDisable = false;

                if (player.PlayerId != heroPlayerId &&
                    player.PlayerId != villanPlayerId)
                {
                    var hockRole = localRole as IRoleMurderPlayerHock;
                    var multiAssignRole = localRole as MultiAssignRoleBase;

                    if (hockRole != null)
                    {
                        hockRole.HockMuderPlayer(
                            heroPlayer, villanPlayer);
                        hockRole.HockMuderPlayer(
                            villanPlayer, heroPlayer);
                    }
                    if (multiAssignRole != null)
                    {
                        hockRole = multiAssignRole.AnotherRole as IRoleMurderPlayerHock;
                        if (hockRole != null)
                        {
                            hockRole.HockMuderPlayer(
                                heroPlayer, villanPlayer);
                            hockRole.HockMuderPlayer(
                                villanPlayer, heroPlayer);
                        }
                    }
                }

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

    public class Hero : MultiAssignRoleBase, IRoleAbility, IRoleUpdate
    {
        public enum OneForAllCondition
        {
            NoGuard = byte.MinValue,
            AwakeHero,
            FeatKill,
            FeatButtonAbility
        }

        public RoleAbilityButtonBase Button
        {
            get => this.searchButton;
            set
            {
                this.searchButton = value;
            }
        }

        private RoleAbilityButtonBase searchButton;
        private AllPlayerArrows arrow;
        private OneForAllCondition cond;
        private float featKillPer;
        private float featButtonAbilityPer;

        public Hero(
            ) : base(
                ExtremeRoleId.Hero,
                ExtremeRoleType.Crewmate,
                ExtremeRoleId.Hero.ToString(),
                ColorPalette.HeroAmaIro,
                false, true, false, false)
        { }
        public void SetCondition(
            OneForAllCondition cond)
        {
            var hero = ExtremeRoleManager.GetLocalPlayerRole() as Hero;
            if (hero != null)
            {
                hero.cond = cond;
            }
        }

        public void CreateAbility()
        {
            this.CreateNormalAbilityButton(
                Translation.GetString("search"),
                Loader.CreateSpriteFromResources(
                    Path.TestButton),
                abilityCleanUp: CleanUp);
            this.Button.SetLabelToCrewmate();
        }

        public bool IsAbilityUse() => this.IsCommonUse();

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            if (this.arrow != null)
            {
                this.arrow.SetActive(false);
            }
        }

        public void Update(PlayerControl rolePlayer)
        {
            if (MeetingHud.Instance != null ||
                ShipStatus.Instance != null) { return; }

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
                        if (this.Button.IsAbilityActive() && this.arrow != null)
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

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.IsDead || player.Data.Disconnected)
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
                    PlayerControl.LocalPlayer.PlayerId);
            }
            this.arrow.SetActive(true);
            return true;
        }

        public void CleanUp()
        {
            this.arrow.SetActive(false);
        }

        public override bool TryRolePlayerKilledFrom(
            PlayerControl rolePlayer, PlayerControl fromPlayer)
        {
            var fromRole = ExtremeRoleManager.GameRole[fromPlayer.PlayerId];

            // ヴィランのキルは特殊ロジックなのでここでは相打ち処理を入れない
            if (fromRole.IsImpostor() && this.cond != OneForAllCondition.NoGuard)
            {
                return false;
            }
            return true;
        }

        public override void ExiledAction(
            GameData.PlayerInfo rolePlayer)
        {
            HeroAcademia.UpdateVigilante(
                HeroAcademia.Condition.HeroDown);
        }
        public override void RolePlayerKilledAction(
            PlayerControl rolePlayer, PlayerControl killerPlayer)
        {
            HeroAcademia.UpdateVigilante(
                HeroAcademia.Condition.HeroDown);
        }


        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            throw new System.NotImplementedException();
        }

        protected override void RoleSpecificInit()
        {
            throw new System.NotImplementedException();
        }
        private void setButtonActive(bool active)
        {
            if (this.Button != null)
            {
                this.Button.SetActive(active);
            }
        }
    }
    public class Villain : MultiAssignRoleBase, IRoleAbility, IRoleUpdate
    {
        public RoleAbilityButtonBase Button
        {
            get => this.searchButton;
            set
            {
                this.searchButton = value;
            }
        }

        private RoleAbilityButtonBase searchButton;
        private AllPlayerArrows arrow;

        public Villain(
            ) : base(
                ExtremeRoleId.Villain,
                ExtremeRoleType.Impostor,
                ExtremeRoleId.Villain.ToString(),
                Palette.ImpostorRed,
                true, false, true, true)
        { }

        public void CreateAbility()
        {
            this.CreateNormalAbilityButton(
                Translation.GetString("search"),
                Loader.CreateSpriteFromResources(
                    Path.TestButton),
                abilityCleanUp: CleanUp);
        }

        public bool IsAbilityUse() => this.IsCommonUse();

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            this.arrow.SetActive(false);
        }

        public void Update(PlayerControl rolePlayer)
        {
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
            this.arrow.SetActive(false);
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
            GameData.PlayerInfo rolePlayer)
        {
            HeroAcademia.UpdateVigilante(
                HeroAcademia.Condition.VillainDown);
        }
        public override void RolePlayerKilledAction(
            PlayerControl rolePlayer, PlayerControl killerPlayer)
        {
            HeroAcademia.UpdateVigilante(
                HeroAcademia.Condition.VillainDown);
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            throw new System.NotImplementedException();
        }

        protected override void RoleSpecificInit()
        {
            throw new System.NotImplementedException();
        }

    }
    public class Vigilante : MultiAssignRoleBase, IRoleAbility, IRoleWinPlayerModifier
    {
        public enum VigilanteCondition
        {
            None = byte.MinValue,
            NewLawInTheShip,
            NewHeroForTheShip,
            NewVillainForTheShip,
            NewEnemyNeutralForTheShip,
        }

        public RoleAbilityButtonBase Button
        {
            get => this.callButton;
            set
            {
                this.callButton = value;
            }
        }

        private RoleAbilityButtonBase callButton;
        private VigilanteCondition condition;

        public Vigilante(
            ) : base(
                ExtremeRoleId.Vigilante,
                ExtremeRoleType.Neutral,
                ExtremeRoleId.Vigilante.ToString(),
                ColorPalette.VigilanteFujiIro,
                false, false, false, false)
        { }

        public void SetCondition(
            VigilanteCondition cond)
        {
            var vigilante = ExtremeRoleManager.GetLocalPlayerRole() as Vigilante;
            if (vigilante != null)
            {
                vigilante.condition = cond;
            }
        }

        public void CreateAbility()
        {
            throw new System.NotImplementedException();
        }

        public bool IsAbilityUse()
        {
            throw new System.NotImplementedException();
        }

        public void ModifiedWinPlayer(
            GameData.PlayerInfo rolePlayerInfo,
            GameOverReason reason,
            Il2CppSystem.Collections.Generic.List<WinningPlayerData> winner)
        {
            throw new System.NotImplementedException();
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            throw new System.NotImplementedException();
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            throw new System.NotImplementedException();
        }

        public bool UseAbility()
        {
            throw new System.NotImplementedException();
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            throw new System.NotImplementedException();
        }

        protected override void RoleSpecificInit()
        {
            throw new System.NotImplementedException();
        }

    }
}
