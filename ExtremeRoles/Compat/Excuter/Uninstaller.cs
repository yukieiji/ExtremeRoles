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
            this.modDllPath = @$"{this.modFolderPath}\{dllName}.dll";
        }

        public override void Excute()
        {
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
                SetPopupText(Translation.GetString("uninstallFall"));
            }
        }

        private void removeOldUninstallFile()
        {
            try
            {
                DirectoryInfo d = new DirectoryInfo(this.modFolderPath);
                string[] files = d.GetFiles($"*{uninstallName}").Select(x => x.FullName).ToArray(); // Getting old versions
                foreach (string f in files)
                {
                    File.Delete(f);
                }
            }
            catch (Exception e)
            {
                Logging.Error("Exception occured when clearing old versions:\n" + e);
            }
        }

    }
}
