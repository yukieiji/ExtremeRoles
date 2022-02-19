using System.Collections.Generic;
using System.Linq;

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
        public class Camp
        {
            private GameObject body;

            public MaryCamp(
                int activeNum,
                float activateRange,
                bool canSee,
                Vector2 pos)
            {
                
            }

            public void Update(PlayerControl rolePlayer)
            {

            }

            public Vent GetConvertedVent()
            {

            }

        }


        public enum MaryOption
        {
            ActiveNum,
            ActiveRange
        }

        public RoleAbilityButtonBase Button
        {
            get => this.bombButton;
            set
            {
                this.bombButton = value;
            }
        }

        public List<Vent> ActiveVent = new List<Vent>();
        public List<Camp> NoneActiveVent = new List<Camp>();
        public int ActiveNum;
        public float ActiveRange;

        private RoleAbilityButtonBase bombButton;


        public Mary() : base(
            ExtremeRoleId.Mary,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.Mary.ToString(),
            Palette.ImpostorRed,
            true, false, true, true)
        { }


        public static void SetCamp(byte callerId)
        {
            var rolePlayer = Player.GetPlayerControlById(callerId);
            var mary = (Mary)ExtremeRoleManager.GameRole[rolePlayer.PlayerId];
            var localPlayerRole = ExtremeRoleManager.GetLocalPlayerRole();

            bool isMarlin = localPlayerRole.Id == ExtremeRoleId.Marlin;

            mary.NoneActiveVent.Add(
                new MaryCamp(
                    mary.ActiveNum,
                    mary.ActiveRange,
                    localPlayerRole.IsImpostor() || isMarlin,
                    rolePlayer.GetTruePosition()));
        }

        public static void ActivateVent(
            byte callerId, int activateVentIndex)
        {
            var rolePlayer = Player.GetPlayerControlById(callerId);
            var mary = (Mary)ExtremeRoleManager.GameRole[rolePlayer.PlayerId];

            Vent newVent = mary.NoneActiveVent[activateVentIndex].GetConvertedVent();

            if (mary.ActiveVent.Count > 0)
            {
                var leftVent = mary.ActiveVent[^1];
                newVent.Left = leftVent;
                leftVent.Right = newVent;
            }
            else
            {
                newVent.Left = null;
            }

            newVent.Right = null;
            newVent.Center = null;

            var allVents = ShipStatus.Instance.AllVents.ToList();
            allVents.Add(newVent);
            ShipStatus.Instance.AllVents = allVents.ToArray();

            mary.ActiveVent.Add(newVent);

            mary.NoneActiveVent.RemoveAt(activateVentIndex);

        }

        public static void StartVentAnimation()
        {

        }

        public void CreateAbility()
        {

            this.CreateAbilityCountButton(
                Translation.GetString("setCamp"),
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
                parentOps, 3, 5, 2.5f);
            CustomOption.Create(
                GetRoleOptionId((int)MaryOption.ActiveNum),
                string.Concat(
                    this.RoleName,
                    MaryOption.ActiveNum.ToString()),
                2, 1, 4, 1, parentOps);
            CustomOption.Create(
                GetRoleOptionId((int)MaryOption.ActiveRange),
                string.Concat(
                    this.RoleName,
                    MaryOption.ActiveRange.ToString()),
                1.0f, 0.1f, 3.0f, 0.1f, parentOps);
        }

        protected override void RoleSpecificInit()
        {
            this.RoleAbilityInit();

            var allOption = OptionHolder.AllOption;

            this.ActiveNum = allOption[
                GetRoleOptionId((int)MaryOption.ActiveNum)].GetValue();
            this.ActiveRange = allOption[
                GetRoleOptionId((int)MaryOption.ActiveRange)].GetValue();

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
            foreach(var camp in this.NoneActiveVent)
            {
                camp.Update(rolePlayer);
            }
        }
    }
}
