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

            private struct PlayerCosmicInfo
            {
                public PlayerControl TargetPlayer;
                public GameData.PlayerOutfit OutfitInfo;
                public bool FlipX;
                public int ColorInfo;
            }

            public FakePlayer(
                PlayerControl rolePlayer,
                PlayerControl targetPlayer,
                bool canSeeFake)
            {
                GameData.PlayerOutfit playerOutfit = targetPlayer.Data.DefaultOutfit;
                PlayerCosmicInfo cosmicInfo = new PlayerCosmicInfo()
                {
                    TargetPlayer = targetPlayer,
                    FlipX = rolePlayer.cosmetics.currentBodySprite.BodySprite.flipX,
                    OutfitInfo = playerOutfit,
                    ColorInfo = playerOutfit.ColorId,
                };

                this.body = new GameObject("DummyPlayer");
                this.body.transform.position = rolePlayer.transform.position;
                SpriteRenderer baseImage = createBaseImage(cosmicInfo);

                createDummyImageSkin(cosmicInfo);
                createDummyImageHat(baseImage, cosmicInfo);
                createDummyImageVisor(baseImage, cosmicInfo);
                createDummyImagePet(baseImage, cosmicInfo);

                changeDummyName(baseImage, cosmicInfo, canSeeFake);

                updateColorName(baseImage, cosmicInfo.ColorInfo);
                removeRoleInfo(baseImage);

                PlayerMaterial.SetColors(cosmicInfo.ColorInfo, baseImage);

                if (ExtremeRolesPlugin.Compat.IsModMap)
                {
                    ExtremeRolesPlugin.Compat.ModMap.AddCustomComponent(
                        baseImage.gameObject,
                        Compat.Interface.CustomMonoBehaviourType.MovableFloorBehaviour);
                }
            }

            public void SwitchColorName()
            {
                if (this.colorBindText != null)
                {
                    this.colorBindText.SetActive(
                        DataManager.Settings.Accessibility.ColorBlindMode);
                }
            }

            public void Clear()
            {
                Object.Destroy(this.body);
            }

            private SpriteRenderer createBaseImage(PlayerCosmicInfo info)
            {
                SpriteRenderer playerImage = Object.Instantiate(
                    info.TargetPlayer.cosmetics.currentBodySprite.BodySprite,
                    this.body.transform);
                playerImage.flipX = info.FlipX;
                playerImage.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
                return playerImage;
            }

            private void createDummyImageHat(
                SpriteRenderer playerImage, PlayerCosmicInfo info)
            {
                HatParent hat = playerImage.GetComponentInChildren<HatParent>();
                if (hat != null)
                {
                    hat.SetHat(info.OutfitInfo.HatId, info.ColorInfo);
                    
                    bool flip = info.FlipX;
                    if (hat.FrontLayer)
                    {
                        hat.FrontLayer.flipX = flip;
                    }
                    if (hat.BackLayer)
                    {
                        hat.BackLayer.flipX = flip;
                    }
                }
            }

            private void createDummyImageVisor(
                SpriteRenderer playerImage, PlayerCosmicInfo info)
            {
                VisorLayer visor = playerImage.GetComponentInChildren<VisorLayer>();
                if (visor != null)
                {
                    visor.SetVisor(info.OutfitInfo.VisorId, info.ColorInfo);
                    visor.SetFlipX(info.FlipX);
                }
            }

            private void createDummyImageSkin(PlayerCosmicInfo info)
            {
                Transform skinTransform = info.TargetPlayer.transform.FindChild(
                    "Skin");

                if (skinTransform != null)
                {
                    GameObject skinObj = Object.Instantiate(
                        skinTransform.gameObject, this.body.transform);
                    SkinLayer skin = skinObj.GetComponent<SkinLayer>();

                    skin.SetSkin(info.OutfitInfo.SkinId, info.ColorInfo, info.FlipX);
                    skinObj.transform.localScale = new Vector3(0.35f, 0.35f, 1.0f);
                }
            }
            private void createDummyImagePet(
                SpriteRenderer playerImage, PlayerCosmicInfo info)
            {
                Transform emptyPet = playerImage.transform.FindChild(
                    "EmptyPet(Clone)");
                if (emptyPet != null)
                {
                    destroyAllColider(emptyPet.gameObject);
                }
                string petId = info.OutfitInfo.PetId;
                if (petId != defaultPetId)
                {
                    PetBehaviour pet = Object.Instantiate(
                        FastDestroyableSingleton<HatManager>.Instance.GetPetById(
                            petId).viewData.viewData,
                        playerImage.transform);
                    pet.SetColor(info.ColorInfo);
                    pet.transform.localPosition =
                        Vector2.zero + (
                            info.FlipX ? Vector2.right * petOffset : Vector2.left * petOffset);
                    pet.transform.localScale = Vector3.one;
                    pet.FlipX = info.FlipX;
                    destroyAllColider(pet.gameObject);
                }
            }

            private void changeDummyName(
                SpriteRenderer playerImage, PlayerCosmicInfo info, bool canSeeFake)
            {
                TextMeshPro nameText = playerImage.transform.FindChild(
                    "NameText_TMP").GetComponent<TextMeshPro>();
                if (nameText != null)
                {
                    nameText.text = canSeeFake ?
                        Translation.GetString("DummyPlayerName") : info.OutfitInfo.PlayerName;
                    nameText.color = canSeeFake ? Palette.ImpostorRed : Palette.White;
                }
            }

            private void removeRoleInfo(SpriteRenderer playerImage)
            {
                Transform info = playerImage.transform.FindChild(
                    Patches.Manager.HudManagerUpdatePatch.RoleInfoObjectName);
                if (info != null)
                {
                    Object.Destroy(info.gameObject);
                }
            }

            private void updateColorName(
                SpriteRenderer playerImage, int colorId)
            {
                this.colorBindText = playerImage.transform.FindChild(
                    "ColorblindName_TMP").gameObject;

                if (this.colorBindText == null) { return; }

                TextMeshPro text = this.colorBindText.GetComponent<TextMeshPro>();

                if (text != null)
                {
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
                    text.text = new string(array);
                }
            }

            private static void destroyAllColider(GameObject obj)
            {
                destroyCollider<Collider2D>(obj);
                destroyCollider<PolygonCollider2D>(obj);
                destroyCollider<BoxCollider2D>(obj);
                destroyCollider<CircleCollider2D>(obj);
            }

            private static void destroyCollider<T>(GameObject obj) where T : Collider2D
            {
                T component = obj.GetComponent<T>();
                if (component != null)
                {
                    Object.Destroy(component);
                }
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

            byte localPlayerId = CachedPlayerControl.LocalPlayer.PlayerId;

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
                    contine = localPlayerId == targetPlayerId;
                }

            } while (contine);

            byte ops = isPlayerMode ? (byte)FakerDummyOps.Player : (byte)FakerDummyOps.DeadBody;

            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.FakerCreateDummy))
            {
                caller.WriteByte(localPlayerId);
                caller.WriteByte(targetPlayerId);
                caller.WriteByte(ops);
            }
            CreateDummy(localPlayerId, targetPlayerId, ops);

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
