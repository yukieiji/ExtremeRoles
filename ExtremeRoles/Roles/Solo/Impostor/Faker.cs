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
        public List<byte> DummyPlayer = new List<byte>();

        public Faker() : base(
            ExtremeRoleId.Faker,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.Faker.ToString(),
            Palette.ImpostorRed,
            true, false, true, true)
        {
            this.DummyPlayer.Clear();
        }

        public RoleAbilityButtonBase Button
        {
            get => this.createFake;
            set
            {
                this.createFake = value;
            }
        }

        private RoleAbilityButtonBase createFake;

        public static void CreateDummy(
            byte rolePlayerId, byte targetPlayerId)
        {
            PlayerControl rolePlyaer = Player.GetPlayerControlById(rolePlayerId);
            Faker faker = (Faker)ExtremeRoleManager.GameRole[rolePlayerId];

            PlayerControl playerControl = Object.Instantiate<PlayerControl>(
                AmongUsClient.Instance.PlayerPrefab);
            playerControl.PlayerId = (byte)GameData.Instance.GetAvailableId();

            ExtremeRoleManager.GameRole.Add(playerControl.PlayerId, new FakeRole());
            faker.DummyPlayer.Add(playerControl.PlayerId);

            GameData.PlayerInfo playerInfo = GameData.Instance.AddPlayer(playerControl);
            if (AmongUsClient.Instance.AmHost)
            {
                AmongUsClient.Instance.Spawn(
                    playerControl, -2,
                    InnerNet.SpawnFlags.None);
            }
            playerInfo.DefaultOutfit.dontCensorName = true;
            playerControl.isDummy = true;
            playerControl.transform.position = rolePlyaer.GetTruePosition();
            playerControl.GetComponent<DummyBehaviour>().enabled = true;
            // playerControl.GetComponentInChildren<BoxCollider2D>().enabled = false;
            playerControl.NetTransform.enabled = false;
            playerControl.SetName("Dummy");
            byte b = (byte)((0 < (int)SaveManager.BodyColor) ? 0 : (0 + 1));
            playerControl.SetColor((int)b);
            playerControl.SetHat("", (int)b);
            playerControl.SetSkin("");
            playerControl.SetPet("");
            playerControl.SetVisor("");
            playerControl.SetNamePlate("");
            playerControl.SetLevel(0U);
        }

        public static void RemoveAllDummyPlayer(byte rolePlayerId)
        {
            Faker faker = (Faker)ExtremeRoleManager.GameRole[rolePlayerId];

            foreach (var player in faker.DummyPlayer)
            {
                PlayerControl playerControl = Player.GetPlayerControlById(player);
                ExtremeRoleManager.GameRole.Remove(player);
                GameData.Instance.RemovePlayer(player);
                if (AmongUsClient.Instance.AmHost)
                {
                    AmongUsClient.Instance.Despawn(playerControl);
                }
            }
            faker.DummyPlayer.Clear();
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
            RPCOperator.Call(
                PlayerControl.LocalPlayer.NetId,
                RPCOperator.Command.FakerRemoveAllDummy,
                new List<byte> { PlayerControl.LocalPlayer.PlayerId });
            RemoveAllDummyPlayer(PlayerControl.LocalPlayer.PlayerId);
        }

        public bool UseAbility()
        {
            RPCOperator.Call(
                PlayerControl.LocalPlayer.NetId,
                RPCOperator.Command.FakerCreateDummy,
                new List<byte>
                { 
                    PlayerControl.LocalPlayer.PlayerId,
                    PlayerControl.LocalPlayer.PlayerId
                });
            CreateDummy(
                PlayerControl.LocalPlayer.PlayerId,
                PlayerControl.LocalPlayer.PlayerId);
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
            this.DummyPlayer.Clear();
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
