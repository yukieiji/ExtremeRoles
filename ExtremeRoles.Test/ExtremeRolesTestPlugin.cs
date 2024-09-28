using BepInEx.Unity.IL2CPP;
using BepInEx;

using HarmonyLib;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module;
using ExtremeRoles.Test.Helper;

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

		var assembly = System.Reflection.Assembly.GetAssembly(this.GetType());
		if (assembly is null) { return; }
		Il2CppRegisterAttribute.Registration(assembly);
	}

	public static void RunReleaseTest()
	{
		try
		{
			TestRunnerBase.Run<AllAssetLoadRunner>();
			TestRunnerBase.Run<TranslationTestRunner>();
			TestRunnerBase.Run<OptionRunner>();
		}
		catch(System.Exception ex)
		{
			Instance.Log.LogError(ex);
		}
	}
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
public static class ChatControllerSendChatPatch
{
	public static void Prefix(ChatController __instance)
	{
		if (__instance.freeChatField.Text == "/RunTest")
		{

			GameUtility.ChangePresetTo(19);
#if RELEASE
			ExtremeRolesTestPlugin.RunReleaseTest();
#else
			TestRunnerBase.Run<GameTestRunner>();
#endif
		}
	}
}