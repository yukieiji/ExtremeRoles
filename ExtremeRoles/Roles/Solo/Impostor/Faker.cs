using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.RoleAbilityButton;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Impostor
{
    public class Faker : SingleRoleBase, IRoleAbility
    {
        public List<byte> dummy;

        public Faker() : base(
            ExtremeRoleId.Faker,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.Faker.ToString(),
            Palette.ImpostorRed,
            true, false, true, true)
        {
            dummy.Clear();
        }

        public RoleAbilityButtonBase Button
        {
            get => this.paintButton;
            set
            {
                this.paintButton = value;
            }
        }

        private RoleAbilityButtonBase paintButton;

        public static void CreateDummy(
            byte rolePlayerId, byte targetPlayerId)
        {

        }

        public void CreateAbility()
        {
            this.CreateNormalAbilityButton(
                Translation.GetString("Dummy"),
                Loader.CreateSpriteFromResources(
                   Path.TestButton, 115f));
        }

        public bool IsAbilityUse() => this.IsCommonUse();

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            return;
        }

        public bool UseAbility()
        {
            return true;
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            this.CreateCommonAbilityOption(
                parentOps);
        }

        protected override void RoleSpecificInit()
        {
            this.RoleAbilityInit();
        }
    }

    public class FakeRole : SingleRoleBase
    {
        public const RoleTypes VanilaRoleId = RoleTypes.Crewmate;
        public FakeRole() : base(
            ExtremeRoleId.VanillaRole,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.VanillaRole.ToString(),
            Palette.White,
            false, false, false,
            false, false, false,
            false, false, false)
        { }

        public override string GetFullDescription() => string.Empty;

        public override string GetImportantText(bool isContainFakeTask = true) => string.Empty;

        protected override void CommonInit()
        {
            return;
        }
        protected override void RoleSpecificInit()
        {
            return;
        }

        protected override void CreateSpecificOption(CustomOptionBase parentOps)
        {
            throw new System.Exception("Don't call this class method!!");
        }
        protected override CustomOptionBase CreateSpawnOption()
        {
            throw new System.Exception("Don't call this class method!!");
        }

    }
}
