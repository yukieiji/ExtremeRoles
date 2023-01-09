using System;

using UnityEngine;

using ExtremeRoles.Resources;

namespace ExtremeRoles.Module.CustomMonoBehaviour
{
    [Il2CppRegister]
    public sealed class OptionMenuTab : MonoBehaviour
    {
        public GameObject Tab { get; private set; }
       
        private OptionTab tabType;
        private SpriteRenderer tabHighLight;

        public OptionMenuTab(IntPtr ptr) : base(ptr) { }

        public static OptionMenuTab Create(OptionTab tab, GameObject template)
        {
            GameObject obj = Instantiate(
                template, template.transform.parent);

            OptionMenuTab menu = obj.AddComponent<OptionMenuTab>();
            GameOptionsMenu optionMenu = obj.gameObject.transform.FindChild("GameGroup").FindChild(
                "SliderInner").GetComponent<GameOptionsMenu>();

            menu.tabType = tab;

            string name = string.Format(ExtremeOptionMenu.MenuNameTemplate, tab.ToString());

            obj.gameObject.name = name;
            optionMenu.name = $"{name}_menu";
            return menu;
        }

        public void CreateTabButton(GameObject template, GameObject parent)
        {
            GameObject tabParent = Instantiate(template, parent.transform);
            tabParent.name = $"{this.tabType}_Tab";

            // Tabの構造はGameTab => ColorButton（こいつだけ）なので一個だけ取得
            Transform colorButtonTrans = tabParent.transform.GetChild(0);

            this.Tab = colorButtonTrans.gameObject;
            this.tabHighLight = colorButtonTrans.FindChild(
                "Tab Background").GetComponentInChildren<SpriteRenderer>();

            string imgPath = this.tabType switch
            {
                OptionTab.General => Path.TabGlobal,

                OptionTab.Crewmate => Path.TabCrewmate,
                OptionTab.Impostor => Path.TabImpostor,
                OptionTab.Neutral => Path.TabNeutral,

                OptionTab.Combination => Path.TabCombination,

                OptionTab.GhostCrewmate => Path.TabGhostCrewmate,
                OptionTab.GhostImpostor => Path.TabGhostImpostor,
                OptionTab.GhostNeutral => Path.TabGhostNeutral,

                _ => Path.TestButton,
            };


            colorButtonTrans.FindChild("Icon").GetComponent<SpriteRenderer>().sprite =
                Loader.CreateSpriteFromResources(imgPath, 150f);
            this.tabHighLight.enabled = false;
        }

        public void SetActive(bool activate)
        {
            this.gameObject.SetActive(activate);
            this.tabHighLight.enabled = activate;
        }
    }
}
