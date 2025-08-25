using System;

using ExtremeRoles.Helper;

namespace ExtremeRoles.Roles.API
{
    public abstract partial class SingleRoleBase
    {
        public virtual string GetImportantText(bool isContainFakeTask = true)
        {
			var color = this.Core.Color;
            string baseString = Design.ColoredString(
				color,
                string.Format("{0}: {1}",
                    Design.ColoredString(
					   color,
                        Tr.GetString(this.RoleName)),
                    Tr.GetString(
                        $"{this.Core.Id}ShortDescription")));

            if (isContainFakeTask && !this.HasTask)
            {
                string fakeTaskString = Design.ColoredString(
					color,
                    TranslationController.Instance.GetString(
                        StringNames.FakeTasks, Array.Empty<Il2CppSystem.Object>()));
                baseString = $"{baseString}\r\n{fakeTaskString}";
            }

            return baseString;
        }

        public virtual string GetIntroDescription() => Tr.GetString(
            $"{this.Core.Id}IntroDescription");

        public virtual string GetFullDescription() => Tr.GetString(
           $"{this.Core.Id}FullDescription");

        public virtual string GetColoredRoleName(bool isTruthName = false) => Design.ColoredString(
            this.Core.Color, Tr.GetString(this.RoleName));
        public virtual string GetRoleTag() => string.Empty;

        public virtual string GetRolePlayerNameTag(
            SingleRoleBase targetRole,
            byte targetPlayerId) => string.Empty;

        public virtual bool IsBlockShowMeetingRoleInfo() => false;
        public virtual bool IsBlockShowPlayingRoleInfo() => false;
    }
}
