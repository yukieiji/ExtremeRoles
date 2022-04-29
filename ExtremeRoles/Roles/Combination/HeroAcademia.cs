using System.Collections.Generic;

using UnityEngine;

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
        public const string Name = "HeroAca";
        public HeroAcademia() : base(
            Name, new Color(255f, 255f, 255f), 3,
            OptionHolder.MaxImposterNum)
        {
            this.Roles.Add(new Hero());
            this.Roles.Add(new Villain());
            this.Roles.Add(new Vigilante());
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

        public Hero(
            ) : base(
                ExtremeRoleId.Hero,
                ExtremeRoleType.Crewmate,
                ExtremeRoleId.Hero.ToString(),
                ColorPalette.HeroAmaIro,
                false, true, false, false)
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
            if (this.arrow != null)
            {
                this.arrow.SetActive(false);
            }
        }

        public void Update(PlayerControl rolePlayer)
        {
            switch (this.cond)
            {
                case OneForAllCondition.FeatKill:
                    this.CanKill = true;
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
                // 相打ち処理を入れる
                return false;
            }
            else if (fromRole.IsCrewmate())
            {
                return false;
            }
            return true;
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

        public static void SetCondition(
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
