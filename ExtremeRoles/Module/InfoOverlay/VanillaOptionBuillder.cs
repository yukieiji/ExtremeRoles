

using ExtremeRoles.Module.Interface;
namespace ExtremeRoles.Module.InfoOverlay
{
    public sealed class VanillaOptionBuillder : IShowTextBuilder
    {
		private Il2CppSystem.Text.StringBuilder settings = new Il2CppSystem.Text.StringBuilder();
        public VanillaOptionBuillder() : base()
        { }
        public (string, string, string) GetShowText()
        {
            return
                (
                    $"<size=200%>{Helper.Translation.GetString("vanilaOptions")}</size>\n",
                    IGameOptionsExtensions.SettingsStringBuilder.ToString(),
					""
				);
        }
    }
}
