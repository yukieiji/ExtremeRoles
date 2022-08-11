using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public sealed class Photographer : SingleRoleBase, IRoleAbility, IRoleAwake<RoleTypes>
    {
        private sealed class PhotoCamera
        {
            public bool IsUpgraded = false;

            private float range;

            public PhotoCamera(float range)
            {
                this.range = range;
            }
            public void Reset()
            {
            }

            public void TakePhoto()
            {
                Vector3 photoCenter = CachedPlayerControl.LocalPlayer.PlayerControl.transform.position;

                foreach (var player in GameData.Instance.AllPlayers.GetFastEnumerator())
                {
                    if (player == null ||
                        player.IsDead ||
                        player.Disconnected ||
                        player.Object == null) { continue; }

                    Vector3 position = player.Object.transform.position;
                    if (this.range >= Vector2.Distance(photoCenter, position))
                    {
                    }

                }

            }
            public override string ToString()
            {

            }
        }

        public enum PhotographerOption
        {
            AwakeTaskGage,
            PhotoRange,
            UpgradePhotoTaskGage
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
            if (!this.awakeRole || !this.photoCreater.IsUpgraded)
            {
                float taskGage = Player.GetPlayerTaskGage(rolePlayer);

                if (taskGage >= this.awakeTaskGage && !this.awakeRole)
                {
                    this.awakeRole = true;
                    this.HasOtherVison = this.awakeHasOtherVision;
                }
                if (taskGage >= this.upgradePhotoTaskGage && !this.photoCreater.IsUpgraded)
                {
                    this.photoCreater.IsUpgraded = true;
                }
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

            CreateFloatOption(
                PhotographerOption.PhotoRange,
                10.0f, 0.0f, 25f, 0.1f,
                parentOps);

            this.CreateAbilityCountOption(
                parentOps, 5, 10);
        }

        protected override void RoleSpecificInit()
        {
            this.awakeTaskGage = (float)OptionHolder.AllOption[
                GetRoleOptionId(PhotographerOption.AwakeTaskGage)].GetValue() / 100.0f;
            this.upgradePhotoTaskGage = (float)OptionHolder.AllOption[
                GetRoleOptionId(PhotographerOption.UpgradePhotoTaskGage)].GetValue() / 100.0f;

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

            this.photoCreater = new PhotoCamera(
                (float)OptionHolder.AllOption[
                    GetRoleOptionId(PhotographerOption.PhotoRange)].GetValue());

            this.photoCreater.IsUpgraded = this.upgradePhotoTaskGage <= 0.0f;

            this.RoleAbilityInit();

        }
    }
}
