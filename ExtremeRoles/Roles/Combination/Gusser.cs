using System;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Combination
{
    public sealed class GuesserManager : FlexibleCombinationRoleManagerBase
    {
        public GuesserManager() : base(new Guesser(), 1)
        { }

    }

    public sealed class Guesser : 
        MultiAssignRoleBase, 
        IRoleSpecialSetUp,
        IRoleMeetingButtonAbility
    {
        private bool isEvil = false;
        private int bulletNum;
        private int maxGuessNum;
        private int curGuessNum;

        private GameObject uiPrefab;
        private GuesserUi guesserUi;

        public Guesser(
            ) : base(
                ExtremeRoleId.Guesser,
                ExtremeRoleType.Crewmate,
                ExtremeRoleId.Guesser.ToString(),
                ColorPalette.SupporterGreen,
                false, true, false, false,
                tab: OptionTab.Combination)
        { }

        public void IntroBeginSetUp()
        {
            this.isEvil = false;
            if (this.IsImpostor())
            {
                this.RoleName = string.Concat("Evil", this.RoleName);
                this.isEvil = true;
            }
            else
            {
                this.RoleName = string.Concat("Nice", this.RoleName);
            }
        }

        public void IntroEndSetUp()
        {
            return;
        }

        public bool IsBlockMeetingButtonAbility(
            PlayerVoteArea instance)
        {
            byte target = instance.TargetPlayerId;

            return
                this.bulletNum <= 0 ||
                this.curGuessNum >= this.maxGuessNum ||
                target == 253;
        }

        public void ButtonMod(PlayerVoteArea instance, UiElement abilityButton)
        {
            
        }

        public Action CreateAbilityAction(PlayerVoteArea instance)
        {
            void openGusserUi()
            {
                if (this.uiPrefab == null)
                {
                    this.uiPrefab = UnityEngine.Object.Instantiate(
                        Loader.GetGameObjectFromResources(
                            Path.GusserUiResources,
                            Path.GusserUiPrefab));

                    this.uiPrefab.SetActive(false);
                }
                if (this.guesserUi == null)
                {
                    GameObject obj = UnityEngine.Object.Instantiate(
                        this.uiPrefab, MeetingHud.Instance.transform);
                    this.guesserUi = obj.GetComponent<GuesserUi>();

                    this.guesserUi.gameObject.SetActive(true);

                    this.guesserUi.SetTitle("TestTitle");
                    this.guesserUi.InitButton(instance.TargetPlayerId);
                }
                this.guesserUi.SetTarget(instance.TargetPlayerId);
                this.guesserUi.gameObject.SetActive(true);
            }
            return openGusserUi;
        }

        public void SetSprite(SpriteRenderer render)
        {
            
        }

        public override string GetFullDescription()
        {
            if (this.isEvil)
            {
                return Translation.GetString(
                    $"{this.Id}ImposterFullDescription");
            }
            else
            {
                return base.GetFullDescription();
            }
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            var imposterSetting = OptionHolder.AllOption[
                GetManagerOptionId(CombinationRoleCommonOption.IsAssignImposter)];
            CreateKillerOption(imposterSetting);

        }

        protected override void RoleSpecificInit()
        {
            this.isEvil = false;
        }
    }
}
