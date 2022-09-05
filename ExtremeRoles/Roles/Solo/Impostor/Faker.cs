using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Impostor
{
    public sealed class Faker : SingleRoleBase, IRoleAbility
    {
        public sealed class FakeDeadBody : IMeetingResetObject
        {
            private SpriteRenderer body;
            public FakeDeadBody(
                PlayerControl rolePlayer,
                PlayerControl targetPlayer)
            {
                var killAnimation = rolePlayer.KillAnimations[0];
                this.body = Object.Instantiate(
                    killAnimation.bodyPrefab.bodyRenderer);
                targetPlayer.SetPlayerMaterialColors(this.body);

                Vector3 vector = rolePlayer.transform.position + killAnimation.BodyOffset;
                vector.z = vector.y / 1000f;
                this.body.transform.position = vector;
                this.body.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);

                if (ExtremeRolesPlugin.Compat.IsModMap)
                {
                    ExtremeRolesPlugin.Compat.ModMap.AddCustomComponent(
                        this.body.gameObject,
                        Compat.Interface.CustomMonoBehaviourType.MovableFloorBehaviour);
                }
            }

            public void Clear()
            {
                Object.Destroy(this.body);
            }

        }

        public sealed class FakePlayer : IMeetingResetObject
        {
            private SpriteRenderer playerImage;

            private HatParent hat;
            private VisorLayer visor;
            private SkinLayer skin;
            private TMPro.TextMeshPro nameText;
            private GameObject colorBindText;

            public FakePlayer(
                PlayerControl rolePlayer,
                PlayerControl targetPlayer)
            {
                this.playerImage = Object.Instantiate(
                    targetPlayer.cosmetics.currentBodySprite.BodySprite);
                this.playerImage.transform.position = rolePlayer.transform.position;

                this.hat = this.playerImage.GetComponentInChildren<HatParent>();
                this.visor = this.playerImage.GetComponentInChildren<VisorLayer>();
                this.skin = this.playerImage.GetComponentInChildren<SkinLayer>();
                this.nameText = this.playerImage.transform.FindChild(
                    "NameText_TMP").GetComponent<TMPro.TextMeshPro>();
                this.colorBindText = this.playerImage.transform.FindChild(
                    "ColorblindName_TMP").gameObject;

                this.nameText.text = "This is fake";

                bool isLeft = rolePlayer.cosmetics.FlipX;

                GameData.PlayerOutfit playerOutfit = targetPlayer.Data.DefaultOutfit;
                int colorId = playerOutfit.ColorId;

                this.hat.SetHat(playerOutfit.HatId, colorId);
                this.visor.SetVisor(playerOutfit.VisorId, colorId);
                this.skin.SetSkin(playerOutfit.SkinId, colorId, isLeft);
                PlayerMaterial.SetColors(colorId, this.playerImage);

                char[] array = FastDestroyableSingleton<TranslationController>.Instance.GetString(
                    Palette.ColorNames[colorId],
                    System.Array.Empty<Il2CppSystem.Object>()).ToCharArray();
                if (array.Length != 0)
                {
                    array[0] = char.ToUpper(array[0]);
                    for (int i = 1; i < array.Length; i++)
                    {
                        array[i] = char.ToLower(array[i]);
                    }
                }

                this.colorBindText.GetComponent<TMPro.TextMeshPro>().text = new string(array);

                if (ExtremeRolesPlugin.Compat.IsModMap)
                {
                    ExtremeRolesPlugin.Compat.ModMap.AddCustomComponent(
                        this.playerImage.gameObject,
                        Compat.Interface.CustomMonoBehaviourType.MovableFloorBehaviour);
                }
                this.playerImage.transform.localScale = rolePlayer.defaultPlayerScale;
                this.skin.transform.localScale = rolePlayer.defaultPlayerScale;
            }

            public void SwitchColorName()
            {
                this.colorBindText.SetActive(SaveManager.ColorBlindMode);
            }

            public void Clear()
            {
                Object.Destroy(this.playerImage);
            }

        }

        public enum FakerDummyOps : byte
        {
            DeadBody,
            Player,
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

        public Faker() : base(
            ExtremeRoleId.Faker,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.Faker.ToString(),
            Palette.ImpostorRed,
            true, false, true, true)
        { }

        public static void CreateDummy(
            byte rolePlayerId, byte targetPlayerId, byte ops)
        {
            PlayerControl rolePlyaer = Player.GetPlayerControlById(rolePlayerId);
            PlayerControl targetPlyaer = Player.GetPlayerControlById(targetPlayerId);

            IMeetingResetObject fake;
            switch((FakerDummyOps)ops)
            {
                case FakerDummyOps.DeadBody:
                    fake = new FakeDeadBody(
                        rolePlyaer,
                        targetPlyaer);
                    break;
                case FakerDummyOps.Player:
                    fake = new FakePlayer(rolePlyaer, targetPlyaer);
                    break;
                default:
                    return;
            }

            ExtremeRolesPlugin.ShipState.AddMeetingResetObject(fake);            
        }

        public void CreateAbility()
        {
            this.CreateNormalAbilityButton(
                Translation.GetString("dummy"),
                Loader.CreateSpriteFromResources(
                   Path.FakerDummy, 115f));
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

            var allPlayer = GameData.Instance.AllPlayers;

            bool isPlayerMode = Input.GetKey(KeyCode.LeftShift);

            bool contine;
            byte targetPlayerId;

            do
            {
                int index = Random.RandomRange(0, allPlayer.Count);
                var player = allPlayer[index];
                contine = player.IsDead || player.Disconnected;
                targetPlayerId = player.PlayerId;

            } while (contine);

            byte ops = isPlayerMode ? (byte)FakerDummyOps.Player : (byte)FakerDummyOps.DeadBody;

            RPCOperator.Call(
                CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                RPCOperator.Command.FakerCreateDummy,
                new List<byte>
                {
                    CachedPlayerControl.LocalPlayer.PlayerId,
                    targetPlayerId,
                    ops
                });
            CreateDummy(
                CachedPlayerControl.LocalPlayer.PlayerId,
                targetPlayerId, ops);
            return true;
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            this.CreateCommonAbilityOption(
                parentOps);
        }

        protected override void RoleSpecificInit()
        {
            this.RoleAbilityInit();
        }
    }
}
