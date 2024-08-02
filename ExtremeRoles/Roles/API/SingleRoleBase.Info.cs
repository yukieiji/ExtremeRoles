using System;

using ExtremeRoles.Helper;

namespace ExtremeRoles.Roles.API
{
    public abstract partial class SingleRoleBase
    {
        public virtual string GetImportantText(bool isContainFakeTask = true)
        {
            string baseString = Design.ColoedString(
                this.NameColor,
                string.Format("{0}: {1}",
                    Design.ColoedString(
                        this.NameColor,
                        OldTranslation.GetString(this.RoleName)),
                    OldTranslation.GetString(
                        $"{this.Id}ShortDescription")));

            if (isContainFakeTask && !this.HasTask)
            {
                string fakeTaskString = Design.ColoedString(
                    this.NameColor,
                    DestroyableSingleton<TranslationController>.Instance.GetString(
                        StringNames.FakeTasks, Array.Empty<Il2CppSystem.Object>()));
                baseString = $"{baseString}\r\n{fakeTaskString}";
            }

            return baseString;
        }

        public virtual string GetIntroDescription() => OldTranslation.GetString(
            $"{this.Id}IntroDescription");

        public virtual string GetFullDescription() => OldTranslation.GetString(
           $"{this.Id}FullDescription");

        public virtual string GetColoredRoleName(bool isTruthName = false) => Design.ColoedString(
            this.NameColor, OldTranslation.GetString(this.RoleName));
        public virtual string GetRoleTag() => string.Empty;

        public virtual string GetRolePlayerNameTag(
            SingleRoleBase targetRole,
            byte targetPlayerId) => string.Empty;

        public virtual bool IsBlockShowMeetingRoleInfo() => false;
        public virtual bool IsBlockShowPlayingRoleInfo() => false;
    }
}
