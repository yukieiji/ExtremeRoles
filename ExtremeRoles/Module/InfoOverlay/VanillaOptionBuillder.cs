using System;

namespace ExtremeRoles.Module.InfoOverlay
{
    public class VanillaOptionBuillder : IShowTextBuilder
    {
		private Il2CppSystem.Text.StringBuilder settings = new Il2CppSystem.Text.StringBuilder();
        public VanillaOptionBuillder() : base()
        { }
        public Tuple<string, string> GetShowText()
        {
			if (settings.Length == 0) { setVanillaOptions(); }
            return Tuple.Create(
				string.Concat(
					$"<size=200%>{Helper.Translation.GetString("vanilaOptions")}</size>\n",
					settings.ToString()), "");
        }
        private void setVanillaOptions()
        {
			var gameOption = PlayerControl.GameOptions;

			this.settings.AppendLine(
				DestroyableSingleton<TranslationController>.Instance.GetString(
					gameOption.isDefaults ? StringNames.GameRecommendedSettings : StringNames.GameCustomSettings,
					Array.Empty<Il2CppSystem.Object>()));
			int num2 = (int)((gameOption.MapId == 0 && Constants.ShouldFlipSkeld()) ? 3 : gameOption.MapId);
			string value = Constants.MapNames[num2];
			gameOption.AppendItem(this.settings, StringNames.GameMapName, value);
			this.settings.Append(
				string.Format(
					"{0}: {1}", DestroyableSingleton<TranslationController>.Instance.GetString(
						StringNames.GameNumImpostors, Array.Empty<Il2CppSystem.Object>()), gameOption.NumImpostors));
			this.settings.AppendLine();
			if (gameOption.gameType == GameType.Normal)
			{
				gameOption.AppendItem(
					this.settings, StringNames.GameConfirmImpostor, gameOption.ConfirmImpostor);
				gameOption.AppendItem(
					this.settings, StringNames.GameNumMeetings, gameOption.NumEmergencyMeetings);
				gameOption.AppendItem(
					this.settings, StringNames.GameAnonymousVotes, gameOption.AnonymousVotes);
				gameOption.AppendItem(
					this.settings, StringNames.GameEmergencyCooldown,
					DestroyableSingleton<TranslationController>.Instance.GetString(
						StringNames.GameSecondsAbbrev, new Il2CppSystem.Object[]
						{
							gameOption.EmergencyCooldown.ToString()
						}));
				gameOption.AppendItem(
					this.settings, StringNames.GameDiscussTime,
					DestroyableSingleton<TranslationController>.Instance.GetString(
						StringNames.GameSecondsAbbrev, new Il2CppSystem.Object[]
						{
							gameOption.DiscussionTime.ToString()
						}));
				if (gameOption.VotingTime > 0)
				{
					gameOption.AppendItem(
						this.settings, StringNames.GameVotingTime,
						DestroyableSingleton<TranslationController>.Instance.GetString(
							StringNames.GameSecondsAbbrev, new Il2CppSystem.Object[]
							{
								gameOption.VotingTime.ToString()
							}));
				}
				else
				{
					gameOption.AppendItem(
						this.settings,
						StringNames.GameVotingTime,
						DestroyableSingleton<TranslationController>.Instance.GetString(
							StringNames.GameSecondsAbbrev, new Il2CppSystem.Object[]
							{
								"∞"
							}));
				}
				gameOption.AppendItem(this.settings, StringNames.GamePlayerSpeed, gameOption.PlayerSpeedMod, "x");
				gameOption.AppendItem(this.settings, StringNames.GameCrewLight, gameOption.CrewLightMod, "x");
				gameOption.AppendItem(this.settings, StringNames.GameImpostorLight, gameOption.ImpostorLightMod, "x");
			}
			gameOption.AppendItem(
				this.settings, StringNames.GameKillCooldown,
				DestroyableSingleton<TranslationController>.Instance.GetString(
					StringNames.GameSecondsAbbrev, new Il2CppSystem.Object[]
					{
						gameOption.KillCooldown.ToString()
					}));
			gameOption.AppendItem(
				this.settings, StringNames.GameKillDistance,
				DestroyableSingleton<TranslationController>.Instance.GetString(
					StringNames.SettingShort + gameOption.KillDistance, Array.Empty<Il2CppSystem.Object>()));
			gameOption.AppendItem(
				this.settings, StringNames.GameTaskBarMode,
				DestroyableSingleton<TranslationController>.Instance.GetString(
					StringNames.SettingNormalTaskMode + (int)gameOption.TaskBarMode, Array.Empty<Il2CppSystem.Object>()));
			gameOption.AppendItem(this.settings, StringNames.GameVisualTasks, gameOption.VisualTasks);
			gameOption.AppendItem(this.settings, StringNames.GameCommonTasks, gameOption.NumCommonTasks);
			gameOption.AppendItem(this.settings, StringNames.GameLongTasks, gameOption.NumLongTasks);
			gameOption.AppendItem(this.settings, StringNames.GameShortTasks, gameOption.NumShortTasks);
			if (gameOption.gameType == GameType.Normal)
			{
				foreach (RoleBehaviour roleBehaviour in DestroyableSingleton<RoleManager>.Instance.AllRoles)
				{
					if (roleBehaviour.Role != RoleTypes.Crewmate && roleBehaviour.Role != RoleTypes.Impostor)
					{
						gameOption.AppendItem(
							this.settings, DestroyableSingleton<TranslationController>.Instance.GetString(
								roleBehaviour.StringName, Array.Empty<Il2CppSystem.Object>()) + ": " + 
								DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.RoleChanceAndQuantity, new Il2CppSystem.Object[]
								{
									gameOption.RoleOptions.GetNumPerGame(roleBehaviour.Role).ToString(),
									gameOption.RoleOptions.GetChancePerGame(roleBehaviour.Role).ToString()
								}));
					}
				}
			}
		}
    }
}
