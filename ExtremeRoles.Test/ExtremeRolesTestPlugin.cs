using BepInEx.Unity.IL2CPP;
using BepInEx;

using HarmonyLib;
using ExtremeRoles.GameMode;

namespace ExtremeRoles.Test;

[BepInAutoPlugin("me.yukieiji.extremeroles.test", "Extreme Roles Test")]
[BepInDependency(
	ExtremeRolesPlugin.Id,
	BepInDependency.DependencyFlags.HardDependency)] // Never change it!
[BepInProcess("Among Us.exe")]
public partial class ExtremeRolesTestPlugin : BasePlugin
{
	public Harmony Harmony { get; } = new Harmony(Id);
#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
	public static ExtremeRolesTestPlugin Instance;
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

	public override void Load()
	{
		Harmony.PatchAll();
		Instance = this;
	}
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
public static class ChatControllerSendChatPatch
{
	public static void Prefix(ChatController __instance)
	{
		if (__instance.freeChatField.Text == "/RunTest")
		{
			var runner = new Options();
			runner.Logger = ExtremeRolesTestPlugin.Instance.Log;
			runner.Run();
		}
	}
}