using System;
using System.IO;
using System.Linq;

using ExtremeRoles.Helper;

namespace ExtremeRoles.Compat.Operator;

internal sealed class ExRAddonUninstaller : OperatorBase
{
    private const string uninstallName = ".uninstalled";
    private string modDllPath;

    internal ExRAddonUninstaller(CompatModType addonType) : base()
    {
        this.modDllPath = Path.Combine(this.ModFolderPath, $"{addonType}.dll");
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

}
