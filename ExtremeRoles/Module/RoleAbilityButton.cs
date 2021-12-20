namespace ExtremeRoles.Module
{
    public class RoleAbilityButton : AbilityButton
	{

		public override void DoClick()
		{
			if (!base.isActiveAndEnabled)
			{
				return;
			}
			if (!PlayerControl.LocalPlayer)
			{
				return;
			}

			var role = Roles.ExtremeRoleManager.GameRole[PlayerControl.LocalPlayer.PlayerId];

			if (role is Roles.IRoleAbility)
			{
				((Roles.IRoleAbility)role).UseAbility();
			}
		}

		public void ButtonInit(string imgPath, string text)
        {
			this.graphic.sprite = Helper.Resources.LoadSpriteFromResources(imgPath, 300f);
			this.graphic.SetCooldownNormalizedUvs();
			this.buttonLabelText.text = text;
		}
	}
}
