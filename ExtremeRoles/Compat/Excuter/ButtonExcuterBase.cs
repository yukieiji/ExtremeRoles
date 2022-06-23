using UnityEngine;

namespace ExtremeRoles.Compat.Excuter
{
    internal abstract class ButtonExcuterBase
    {
        protected string modFolderPath;
        protected GenericPopup Popup;

        internal ButtonExcuterBase()
        {

            this.modFolderPath = System.IO.Path.GetDirectoryName(
                Application.dataPath) + @"\BepInEx\plugins";

            this.Popup = Object.Instantiate(Module.Prefab.Prop);
            this.Popup.TextAreaTMP.fontSize *= 0.7f;
            this.Popup.TextAreaTMP.enableAutoSizing = false;
        }

        protected void ShowPopup(string message)
        {
            SetPopupText(message);
            Popup.gameObject.SetActive(true);
        }

        protected void SetPopupText(string message)
        {
            if (Popup == null)
            {
                return;
            }

            if (Popup.TextAreaTMP != null)
            {
                Popup.TextAreaTMP.text = message;
            }
        }

        public abstract void Excute();
    }
}
