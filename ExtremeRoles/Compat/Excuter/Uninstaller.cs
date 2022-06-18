using System;
using System.IO;
using System.Linq;

using ExtremeRoles.Helper;

namespace ExtremeRoles.Compat.Excuter
{
    internal class Uninstaller : ButtonExcuterBase
    {
        private const string uninstallName = ".uninstalled";
        private string modDllPath;

        internal Uninstaller(string dllName) : base()
        {
            this.modDllPath = Path.Combine(this.modFolderPath, $"{dllName}.dll");
        }

        public override void Excute()
        {
            if (!File.Exists(Path.Combine(this.modDllPath)))
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
                DirectoryInfo d = new DirectoryInfo(this.modFolderPath);
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
}
