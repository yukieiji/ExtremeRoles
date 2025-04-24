using System;
using UnityEngine.SceneManagement;

namespace ExtremeRoles.Module.ScreenManagerHook;

public static class ScreenManagerHookcs
{
	public static void Hook(Scene scene, LoadSceneMode node)
	{
		MainMenuLoadHook.Hook(scene);
	}

	public static void RegisterLoad()
	{
		SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>)Hook);
	}
}
