using BepInEx;
using BepInEx.Unity.IL2CPP;

namespace AmongUsLibExporter;

[BepInAutoPlugin("me.yukieiji.plugin", "Plugin")]
[BepInProcess("Among Us.exe")]
public partial class Plugin : BasePlugin
{
	public override void Load()
	{
	}
}
