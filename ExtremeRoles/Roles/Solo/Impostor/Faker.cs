using System.Collections.Generic;

using UnityEngine;
using TMPro;
using AmongUs.Data;

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
            private GameObject body;
            private GameObject colorBindText;
            private const string defaultPetId = "0";
            private const float petOffset = 0.72f;

            public FakePlayer(
                PlayerControl rolePlayer,
                PlayerControl targetPlayer,
                bool canSeeFake)
            {
                bool flipX = rolePlayer.cosmetics.currentBodySprite.BodySprite.flipX;

                this.body = new GameObject("DummyPlayer");
                SpriteRenderer playerImage = Object.Instantiate(
                    targetPlayer.cosmetics.currentBodySprite.BodySprite,
                    this.body.transform);
                playerImage.flipX = flipX;
                playerImage.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
                this.body.transform.position = rolePlayer.transform.position;

                Transform skinTransform = targetPlayer.transform.FindChild(
                    "Skin");

                HatParent hat = playerImage.GetComponentInChildren<HatParent>();
                VisorLayer visor = playerImage.GetComponentInChildren<VisorLayer>();
                TextMeshPro nameText = playerImage.transform.FindChild(
                    "NameText_TMP").GetComponent<TextMeshPro>();
                this.colorBindText = playerImage.transform.FindChild(
                    "ColorblindName_TMP").gameObject;
                Transform info = playerImage.transform.FindChild(
                    Patches.Manager.HudManagerUpdatePatch.RoleInfoObjectName);
                if (info != null)
                {
                    Object.Destroy(info.gameObject);
                }

                GameData.PlayerOutfit playerOutfit = targetPlayer.Data.DefaultOutfit;

                nameText.text = canSeeFake ? 
                    Translation.GetString("DummyPlayerName") : playerOutfit.PlayerName;
                nameText.color = canSeeFake ? Palette.ImpostorRed : Palette.White;

                int colorId = playerOutfit.ColorId;
                hat.SetHat(playerOutfit.HatId, colorId);
                if (hat.FrontLayer)
                {
                    hat.FrontLayer.flipX = flipX;
                }
                if (hat.BackLayer)
                {
                    hat.BackLayer.flipX = flipX;
                }

                visor.SetVisor(playerOutfit.VisorId, colorId);
                visor.SetFlipX(flipX);
                if (skinTransform != null)
                {
                    GameObject skinObj = Object.Instantiate(
                        skinTransform.gameObject, this.body.transform);
                    SkinLayer skin = skinObj.GetComponent<SkinLayer>();

                    skin.SetSkin(playerOutfit.SkinId, colorId, flipX);
                    skinObj.transform.localScale = new Vector3(0.35f, 0.35f, 1.0f);
                }
                string petId = playerOutfit.PetId;
                if (petId != defaultPetId)
                {
                    PetBehaviour pet = Object.Instantiate(
                        FastDestroyableSingleton<HatManager>.Instance.GetPetById(
                            petId).viewData.viewData,
                        playerImage.transform);
                    pet.SetColor(colorId);
                    pet.transform.localPosition = 
                        Vector2.zero + (flipX ? Vector2.right * petOffset : Vector2.left * petOffset);
                    pet.transform.localScale = Vector3.one;
                    pet.FlipX = flipX;
                }

                PlayerMaterial.SetColors(colorId, playerImage);

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

                this.colorBindText.GetComponent<TextMeshPro>().text = new string(array);

                if (ExtremeRolesPlugin.Compat.IsModMap)
                {
                    ExtremeRolesPlugin.Compat.ModMap.AddCustomComponent(
                        playerImage.gameObject,
                        Compat.Interface.CustomMonoBehaviourType.MovableFloorBehaviour);
                }
            }

            public void SwitchColorName()
            {
                this.colorBindText.SetActive(
                    DataManager.Settings.Accessibility.ColorBlindMode);
            }

            public void Clear()
            {
                Object.Destroy(this.body);
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

        private Sprite deadBodyDummy;
        private Sprite playerDummy;

        private string deadBodyDummyStr;
        private string playerDummyStr;

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
                    SingleRoleBase role = ExtremeRoleManager.GetLocalPlayerRole();
                    fake = new FakePlayer(
                        rolePlyaer, targetPlyaer,
                        role.IsImpostor() || role.Id == ExtremeRoleId.Marlin);
                    break;
                default:
                    return;
            }

            ExtremeRolesPlugin.ShipState.AddMeetingResetObject(fake);            
        }

        public void CreateAbility()
        {
            this.deadBodyDummy = Loader.CreateSpriteFromResources(
                Path.FakerDummyDeadBody, 115f);
            this.playerDummy = Loader.CreateSpriteFromResources(
                Path.FakerDummyPlayer, 115f);

            this.deadBodyDummyStr = Translation.GetString("dummyDeadBody");
            this.playerDummyStr = Translation.GetString("dummyPlayer");

            this.CreateNormalAbilityButton(
                this.deadBodyDummyStr,
                this.deadBodyDummy);
        }

        public bool IsAbilityUse()
        {
            bool isPlayerDummy = Input.GetKey(KeyCode.LeftShift);

            this.Button.SetButtonImage(
                isPlayerDummy ? 
                this.playerDummy : this.deadBodyDummy);
            this.Button.SetButtonText(
                isPlayerDummy ? this.playerDummyStr : this.deadBodyDummyStr);

            return this.IsCommonUse();
        }

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
            bool excludeImp = Input.GetKey(KeyCode.LeftControl);
            bool excludeMe = Input.GetKey(KeyCode.LeftAlt);

            bool contine;
            byte targetPlayerId;

            do
            {
                int index = Random.RandomRange(0, allPlayer.Count);
                var player = allPlayer[index];
                targetPlayerId = player.PlayerId;

                contine = player.IsDead || player.Disconnected;
                if (!contine && excludeImp)
                {
                    contine = ExtremeRoleManager.GameRole[targetPlayerId].IsImpostor();
                }
                else if (!contine && excludeMe)
                {
                    contine = CachedPlayerControl.LocalPlayer.PlayerId == targetPlayerId;
                }

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
