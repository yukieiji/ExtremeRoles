using UnityEngine.SceneManagement;

namespace ExtremeRoles.Module.ScreenManagerHook;

public static class MainMenuLoadHook
{
	public static void Hook(Scene scene)
	{
		if (scene.name != "MainMenu")
		{
			return;
		}
		CustomRegion.AddCustomServer();
	}
}
