using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.RoleAbilityButton;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

using BepInEx.IL2CPP.Utils.Collections;


namespace ExtremeRoles.Roles.Solo.Impostor
{
    public class Mary : SingleRoleBase, IRoleAbility, IRoleUpdate
    {
        public class MaryCamp
        {
            private Vent vent;

            public MaryCamp(
                float activateRange,
                bool canSee, Vector2 pos)
            {
                
            }
        }


        public enum MaryOption
        {
            
        }

       

        public RoleAbilityButtonBase Button
        {
            get => this.bombButton;
            set
            {
                this.bombButton = value;
            }
        }
        private RoleAbilityButtonBase bombButton;


        public Mary() : base(
            ExtremeRoleId.Mary,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.Mary.ToString(),
            Palette.ImpostorRed,
            true, false, true, true)
        {
        }

        public void CreateAbility()
        {

            this.CreateAbilityCountButton(
                Translation.GetString("setBomb"),
                Loader.CreateSpriteFromResources(
                    Path.TestButton),
                abilityCleanUp: CleanUp);
        }

        public bool IsAbilityUse()
        {

            return this.IsCommonUse();
        }

        public void CleanUp()
        {
            
        }

        public bool UseAbility()
        {
            return true;
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            this.CreateAbilityCountOption(
                parentOps, 2, 5, 2.5f);
            
        }

        protected override void RoleSpecificInit()
        {
            this.RoleAbilityInit();
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void Update(PlayerControl rolePlayer)
        {
            
        }
    }
}
