using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public sealed class Photographer : SingleRoleBase, IRoleAbility, IRoleAwake<RoleTypes>, IRoleReportHock
    {
        private struct PlayerPosInfo
        {
            public string PlayerName;
            public SystemTypes? Room;

            public PlayerPosInfo(
                GameData.PlayerInfo player,
                ContactFilter2D filter)
            {
                this.PlayerName = player.PlayerName;
                this.Room = null;

                UnhollowerBaseLib.Il2CppReferenceArray<Collider2D> buffer = 
                    new UnhollowerBaseLib.Il2CppReferenceArray<Collider2D>(10);
                Collider2D playerCollinder = player.Object.GetComponent<Collider2D>();

                foreach (PlainShipRoom room in CachedShipStatus.Instance.AllRooms)
                {
                    if (room != null && room.roomArea)
                    {
                        int hitCount = room.roomArea.OverlapCollider(filter, buffer);
                        if (isHit(playerCollinder, buffer, hitCount))
                        {
                            this.Room = room.RoomId;
                        }
                    }
                }
            }

            private bool isHit(
                Collider2D playerCollinder,
                Collider2D[] buffer,
                int hitCount)
            {
                for (int i = 0; i < hitCount; i++)
                {
                    Helper.Logging.Debug($"Null?:{buffer[i] == null}");

                    if (buffer[i] == playerCollinder)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private struct Photo
        {
            private List<PlayerPosInfo> player;
            private DateTime takeTime;

            private const string separateLine = "-----------";
            private static readonly string[] randomStr = new string[]
            { 
                "NoName",
                "NewFile",
                "NewPhoto",
                "sudo",
                "HelloWorld",
                "AmongUs",
                "yukieiji",
                "ExtremeRoles",
                "ExtremeSkins",
                "ExtremeHat",
                "ExtremeVisor",
                "ExtremeNamePlate",
                "ExR", "ExS","ExV",
                "ExH", "ExN"
            };
            
            public Photo(float range, ContactFilter2D filter)
            {
                this.takeTime = DateTime.UtcNow;
                this.player = new List<PlayerPosInfo>();

                Vector3 photoCenter = CachedPlayerControl.LocalPlayer.PlayerControl.transform.position;

                foreach (var player in GameData.Instance.AllPlayers.GetFastEnumerator())
                {
                    if (player == null ||
                        player.IsDead ||
                        player.Disconnected ||
                        player.Object == null) { continue; }

                    Vector3 position = player.Object.transform.position;
                    if (range >= Vector2.Distance(photoCenter, position))
                    {
                        this.player.Add(new PlayerPosInfo(player, filter));
                    }

                }
            }

            public string ToString(bool isUpgrade)
            {
                StringBuilder photoInfoBuilder = new StringBuilder();
                photoInfoBuilder.AppendLine(
                    string.Format(
                        "{0}:{1}<br>{2}:{3}",
                        Translation.GetString("takePhotoTime"),
                        this.takeTime,
                        Translation.GetString("photoName"),
                        getRandomPhotoName()));
                photoInfoBuilder.AppendLine("");
                if (this.player.Count == 0)
                {
                    photoInfoBuilder.AppendLine(
                        Translation.GetString("noPlayerInPhoto"));
                }
                else
                {
                    foreach (PlayerPosInfo playerInfo in this.player)
                    {

                        string roomInfo = string.Empty;

                        if (isUpgrade &&
                            playerInfo.Room != null)
                        {
                            roomInfo =
                                FastDestroyableSingleton<TranslationController>.Instance.GetString(
                                    playerInfo.Room.Value);
                        }

                        string photoPlayerInfo =
                            roomInfo == string.Empty ?
                            $"{playerInfo.PlayerName}" :
                            $"{playerInfo.PlayerName}   {roomInfo}";

                        photoInfoBuilder.AppendLine(photoPlayerInfo);
                    }
                }
                return photoInfoBuilder.ToString();
            }
            private string getRandomPhotoName()
            {
                // 適当な役職名とかを写真名にする
                List<string> photoName = new List<string>();

                // 適当な陣営
                int maxTeamId = Enum.GetValues(typeof(ExtremeRoleType)).Cast<int>().Max();
                ExtremeRoleType intedTeamId = (ExtremeRoleType)RandomGenerator.Instance.Next(
                    maxTeamId + 1);
                photoName.Add(Translation.GetString(intedTeamId.ToString()));


                // 適当な役職名
                int maxRoleId = Enum.GetValues(typeof(ExtremeRoleId)).Cast<int>().Max();
                ExtremeRoleId roleId = (ExtremeRoleId)RandomGenerator.Instance.Next(
                    maxRoleId + 1);

                if (roleId != ExtremeRoleId.Null || 
                    roleId != ExtremeRoleId.VanillaRole)
                {
                    photoName.Add(Translation.GetString(roleId.ToString()));
                }
                else
                {
                    int maxAmongUsRoleId = Enum.GetValues(typeof(RoleTypes)).Cast<int>().Max();
                    RoleTypes amongUsRoleId = (RoleTypes)RandomGenerator.Instance.Next(
                        maxAmongUsRoleId + 1);

                    photoName.Add(Translation.GetString(amongUsRoleId.ToString()));
                }
                
                photoName.Add(randomStr[RandomGenerator.Instance.Next(randomStr.Length)]);
                return string.Concat(photoName.OrderBy(
                    item => RandomGenerator.Instance.Next()));
            }
        }

        private sealed class PhotoCamera
        {
            public bool IsUpgraded = false;

            private const string separateLine = "---------------------------------";
            private float range;
            private List<Photo> film = new List<Photo>();
            private ContactFilter2D filter;

            public PhotoCamera(float range)
            {
                this.range = range;
                
                this.filter = default(ContactFilter2D);
                this.filter.layerMask = Constants.PlayersOnlyMask;
                this.filter.useLayerMask = true;
                this.filter.useTriggers = false;

                this.film.Clear();
            }
            public void Reset()
            {
                this.film.Clear();
            }

            public void TakePhoto()
            {
                this.film.Add(new Photo(this.range, this.filter));
            }
            public override string ToString()
            {
                if (this.film.Count == 0) { return string.Empty; }

                StringBuilder builder = new StringBuilder();
                
                builder.AppendLine(separateLine);

                foreach (Photo photo in this.film)
                {
                    builder.AppendLine(
                        photo.ToString(this.IsUpgraded));
                }

                builder.AppendLine(separateLine);

                return builder.ToString();
            }
        }

        public enum PhotographerOption
        {
            AwakeTaskGage,
            UpgradePhotoTaskGage,
            EnableAllSendChat,
            UpgradeAllSendChatTaskGage,
            PhotoRange,
        }

        public RoleAbilityButtonBase Button
        {
            get => this.takePhotoButton;
            set
            {
                this.takePhotoButton = value;
            }
        }
        public bool IsAwake
        {
            get
            {
                return GameSystem.IsLobby || this.awakeRole;
            }
        }

        public RoleTypes NoneAwakeRole => RoleTypes.Crewmate;

        private RoleAbilityButtonBase takePhotoButton;

        private bool awakeRole;
        private float awakeTaskGage;
        private bool awakeHasOtherVision;

        private float upgradePhotoTaskGage;
        private bool enableAllSend;
        private float upgradeAllSendChatTaskGage;
        private bool isUpgradeChat;
        private PhotoCamera photoCreater;

        public Photographer() : base(
            ExtremeRoleId.Photographer,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Photographer.ToString(),
            ColorPalette.AgencyYellowGreen,
            false, true, false, false)
        { }


        public void CreateAbility()
        {
            this.CreateAbilityCountButton(
                Translation.GetString("takePhoto"),
                Loader.CreateSpriteFromResources(
                    Path.AgencyTakeTask));
            this.Button.SetLabelToCrewmate();
        }

        public bool UseAbility()
        {
            this.photoCreater.TakePhoto();
            return true;
        }

        public bool IsAbilityUse() => this.IsCommonUse();

        public string GetFakeOptionString() => "";

        public void HockReportButton(
            PlayerControl rolePlayer, GameData.PlayerInfo reporter)
        {
            sendPhotoInfo();
        }

        public void HockBodyReport(
            PlayerControl rolePlayer,
            GameData.PlayerInfo reporter,
            GameData.PlayerInfo reportBody)
        {
            sendPhotoInfo();
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            this.photoCreater.Reset();
            return;
        }

        public void Update(PlayerControl rolePlayer)
        {
            float taskGage = Player.GetPlayerTaskGage(rolePlayer);

            if (!this.awakeRole)
            {
                if (taskGage >= this.awakeTaskGage && !this.awakeRole)
                {
                    this.awakeRole = true;
                    this.HasOtherVison = this.awakeHasOtherVision;
                }
                else
                {
                    this.takePhotoButton.SetActive(false);
                }
            }

            if (taskGage >= this.upgradePhotoTaskGage &&
                !this.photoCreater.IsUpgraded)
            {
                this.photoCreater.IsUpgraded = true;
            }
            if (this.enableAllSend &&
                taskGage >= this.upgradeAllSendChatTaskGage &&
                !this.isUpgradeChat)
            {
                this.isUpgradeChat = true;
            }
        }

        public override string GetColoredRoleName(bool isTruthColor = false)
        {
            if (isTruthColor || IsAwake)
            {
                return base.GetColoredRoleName();
            }
            else
            {
                return Design.ColoedString(
                    Palette.White, Translation.GetString(RoleTypes.Crewmate.ToString()));
            }
        }
        public override string GetFullDescription()
        {
            if (IsAwake)
            {
                return Translation.GetString(
                    $"{this.Id}FullDescription");
            }
            else
            {
                return Translation.GetString(
                    $"{RoleTypes.Crewmate}FullDescription");
            }
        }

        public override string GetImportantText(bool isContainFakeTask = true)
        {
            if (IsAwake)
            {
                return base.GetImportantText(isContainFakeTask);

            }
            else
            {
                return Design.ColoedString(
                    Palette.White,
                    $"{this.GetColoredRoleName()}: {Translation.GetString("crewImportantText")}");
            }
        }

        public override string GetIntroDescription()
        {
            if (IsAwake)
            {
                return base.GetIntroDescription();
            }
            else
            {
                return Design.ColoedString(
                    Palette.CrewmateBlue,
                    CachedPlayerControl.LocalPlayer.Data.Role.Blurb);
            }
        }

        public override Color GetNameColor(bool isTruthColor = false)
        {
            if (isTruthColor || IsAwake)
            {
                return base.GetNameColor(isTruthColor);
            }
            else
            {
                return Palette.White;
            }
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            CreateIntOption(
                PhotographerOption.AwakeTaskGage,
                30, 0, 100, 10,
                parentOps,
                format: OptionUnit.Percentage);

            CreateIntOption(
                PhotographerOption.UpgradePhotoTaskGage,
                60, 0, 100, 10,
                parentOps,
                format: OptionUnit.Percentage);
            
            var chatUpgradeOpt = CreateBoolOption(
                PhotographerOption.EnableAllSendChat,
                false, parentOps);

            CreateIntOption(
                PhotographerOption.UpgradeAllSendChatTaskGage,
                40, 0, 100, 10,
                chatUpgradeOpt,
                format: OptionUnit.Percentage);

            CreateFloatOption(
                PhotographerOption.PhotoRange,
                10.0f, 0.0f, 50f, 0.5f,
                parentOps);

            this.CreateAbilityCountOption(
                parentOps, 5, 10);
        }

        protected override void RoleSpecificInit()
        {
            var allOpt = OptionHolder.AllOption;

            this.awakeTaskGage = allOpt[
                GetRoleOptionId(PhotographerOption.AwakeTaskGage)].GetValue() / 100.0f;
            this.upgradePhotoTaskGage = allOpt[
                GetRoleOptionId(PhotographerOption.UpgradePhotoTaskGage)].GetValue() / 100.0f;
            this.enableAllSend = allOpt[
                GetRoleOptionId(PhotographerOption.EnableAllSendChat)].GetValue();
            this.upgradeAllSendChatTaskGage = allOpt[
                GetRoleOptionId(PhotographerOption.UpgradeAllSendChatTaskGage)].GetValue() / 100.0f;

            this.awakeHasOtherVision = this.HasOtherVison;

            if (this.awakeTaskGage <= 0.0f)
            {
                this.awakeRole = true;
                this.HasOtherVison = this.awakeHasOtherVision;
            }
            else
            {
                this.awakeRole = false;
                this.HasOtherVison = false;
            }

            this.isUpgradeChat = this.enableAllSend && 
                this.upgradeAllSendChatTaskGage <= 0.0f;

            this.photoCreater = new PhotoCamera(
                (float)OptionHolder.AllOption[
                    GetRoleOptionId(PhotographerOption.PhotoRange)].GetValue());

            this.photoCreater.IsUpgraded = this.upgradePhotoTaskGage <= 0.0f;

            this.RoleAbilityInit();

        }

        private void sendPhotoInfo()
        {
            string photoInfo = this.photoCreater.ToString();

            HudManager hud = FastDestroyableSingleton<HudManager>.Instance;

            if (photoInfo == string.Empty ||
                !AmongUsClient.Instance.AmClient ||
                !hud) { return; }

            string chatText = string.Format(
                Translation.GetString("photoChat"),
                photoInfo);
            if (this.enableAllSend &&
                this.isUpgradeChat)
            {
                hud.Chat.TextArea.text = chatText;
                hud.Chat.SendChat();
            }
            else
            {
                hud.Chat.AddChat(
                    CachedPlayerControl.LocalPlayer,
                    chatText);
            }
        }
    }
}
