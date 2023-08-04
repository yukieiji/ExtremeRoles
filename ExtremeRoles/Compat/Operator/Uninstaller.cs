using System;
using System.IO;
using System.Linq;

using ExtremeRoles.Helper;

#nullable enable

namespace ExtremeRoles.Compat.Operator;

internal sealed class Uninstaller : OperatorBase
{
	private const string uninstallName = ".uninstalled";
	private string modDllPath;
	private string guid;

	internal Uninstaller(CompatModInfo modInfo) : base()
	{
		this.modDllPath = Path.Combine(this.ModFolderPath, $"{modInfo.Name}.dll");
		this.guid = modInfo.Guid;
	}

	public override void Excute()
	{
		if (!File.Exists(this.modDllPath))
		{
			Popup.Show(Translation.GetString("alreadyUninstall"));
			return;
		}

		string info = Translation.GetString("checkUninstallNow");
		Popup.Show(info);

		if (File.Exists(this.modDllPath))
		{
			removeOldUninstallFile();
			SetPopupText(Translation.GetString("uninstallNow"));
			if (!isNotRemoveReactor())
			{
				string reactorPath = Path.Combine(this.ModFolderPath, ReactorDll);
				File.Move(reactorPath, $"{reactorPath}{uninstallName}");
			}
			File.Move(this.modDllPath, $"{this.modDllPath}{uninstallName}");
			ShowPopup(Translation.GetString("uninstallRestart"));
		}
		else
		{
			SetPopupText(Translation.GetString("uninstallManual"));
		}
	}

	private void removeOldUninstallFile()
	{
		try
		{
			DirectoryInfo d = new DirectoryInfo(this.ModFolderPath);
			string[] files = d.GetFiles($"*{uninstallName}").Select(x => x.FullName).ToArray(); // Remove uninstall versions
			foreach (string f in files)
			{
				File.Delete(f);
			}
		}
		catch (Exception e)
		{
			Logging.Error($"Exception occured when clearing old versions:\n{e}");
		}
	}
	private bool isNotRemoveReactor()
		=> CompatModManager.Instance.LoadedMod.Keys.Any(
			x =>
			{
				return
					CompatModManager.ModInfo.TryGetValue(x, out var modInfo) &&
					modInfo.IsRequireReactor &&
					modInfo.Guid != this.guid;
			});
}
