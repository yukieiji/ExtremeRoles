using System;

using UnityEngine;

using ExtremeRoles.Resources;
using System.Collections.Generic;
using TMPro;

namespace ExtremeRoles.Module.CustomMonoBehaviour
{
    [Il2CppRegister]
    public sealed class OptionMenuTab : MonoBehaviour
    {
        public const string StringOptionName = "ExROptionId_";
        public GameObject Tab { get; private set; }

        private SpriteRenderer tabHighLight;
        private GameOptionsMenu menuBody;
        private TextMeshPro tabName;

        private StringOption template;
        private OptionTab tabType;
        private List<int> useOptionId = new List<int>();

        public OptionMenuTab(IntPtr ptr) : base(ptr) { }

        public static OptionMenuTab Create(OptionTab tab, GameObject template)
        {
            GameObject obj = Instantiate(
                template, template.transform.parent);

            OptionMenuTab menu = obj.AddComponent<OptionMenuTab>();
            Transform gameGroupTrans = obj.transform.FindChild("GameGroup");

            menu.menuBody = gameGroupTrans.FindChild("SliderInner").GetComponent<GameOptionsMenu>();
            menu.tabType = tab;
            menu.useOptionId.Clear();
            menu.tabName = gameGroupTrans.FindChild("Text").GetComponent<TextMeshPro>();

            string name = string.Format(ExtremeOptionMenu.MenuNameTemplate, tab.ToString());
            
            obj.gameObject.name = name;
            menu.menuBody.name = $"{name}_menu";

            // どうも中身を消してるテンプレートから作ってるのに生き残りが居るらしい
            foreach (OptionBehaviour option in menu.menuBody.GetComponentsInChildren<OptionBehaviour>())
            {
                Destroy(option.gameObject);
            }
            menu.menuBody.Children = new OptionBehaviour[] { };

            return menu;
        }

        public void OnEnable()
        {
            if (this.menuBody.Children.Length != 0) { return; }

            this.menuBody.Children = new OptionBehaviour[this.useOptionId.Count];

            for (int index = 0; index < this.menuBody.Children.Length; ++index)
            {
                int id = this.useOptionId[index];
                IOption option = OptionHolder.AllOption[id];

                if (option.Body != null) { continue; }
                
                StringOption stringOption = Instantiate(
                    this.template, this.menuBody.transform);
                stringOption.OnValueChanged = new Action<OptionBehaviour>((o) => { });
                stringOption.TitleText.text = option.GetTranslatedName();
                stringOption.Value = stringOption.oldValue = option.CurSelection;
                stringOption.ValueText.text = option.GetTranslatedValue();
                stringOption.gameObject.name = string.Concat(
                    StringOptionName, id);
                stringOption.gameObject.SetActive(true);

                this.menuBody.Children[index] = stringOption;
                option.SetOptionBehaviour(stringOption);
            }
        }

        public void Start()
        {
            this.tabName.SetText(
                Helper.Translation.GetString(this.gameObject.name));
        }

        public void AddOptionId(int newId)
        {
            this.useOptionId.Add(newId);
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

            colorButtonTrans.FindChild("Icon").GetComponent<SpriteRenderer>().sprite =
                Loader.CreateSpriteFromResources(
                    string.Format(Path.TabImagePathFormat, this.tabType), 150f);
            this.tabHighLight.enabled = false;
        }

        public void SetActive(bool activate)
        {
            this.gameObject.SetActive(activate);
            this.tabHighLight.enabled = activate;
        }
        public void SetOptionTemplate(StringOption option)
        {
            this.template = option;
        }
    }
}
