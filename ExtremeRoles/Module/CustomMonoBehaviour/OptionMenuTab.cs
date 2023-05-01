using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using TMPro;

using Il2CppInterop.Runtime.Attributes;

using ExtremeRoles.Resources;

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
        private Scroller scroller;
        private StringOption template;

        private OptionTab tabType;
        private List<IOption> useOption = new List<IOption>();

        private Memory<ValueTuple<IOption, OptionBehaviour>> childrenBody;
        
        private float blockTimer = -10.0f;

        private const float posOffsetInit = 2.75f;
        private const string menuNameTemplate = "ExtremeRoles_{0}Settings";

        public OptionMenuTab(IntPtr ptr) : base(ptr) { }

        public static OptionMenuTab Create(OptionTab tab, GameObject template)
        {
            GameObject obj = Instantiate(
                template, template.transform.parent);

            OptionMenuTab menu = obj.AddComponent<OptionMenuTab>();
            Transform gameGroupTrans = obj.transform.FindChild("GameGroup");

            menu.menuBody = gameGroupTrans.FindChild("SliderInner").GetComponent<GameOptionsMenu>();
            menu.scroller = gameGroupTrans.GetComponent<Scroller>();
            menu.tabType = tab;
            menu.useOption.Clear();
            menu.tabName = gameGroupTrans.FindChild("Text").GetComponent<TextMeshPro>();

            string name = string.Format(menuNameTemplate, tab.ToString());
            
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

            this.menuBody.Children = new OptionBehaviour[this.useOption.Count];

            for (int index = 0; index < this.menuBody.Children.Length; ++index)
            {
                IOption option = this.useOption[index];

                if (option.Body != null) { continue; }
                
                StringOption stringOption = Instantiate(
                    this.template, this.menuBody.transform);
                stringOption.OnValueChanged = new Action<OptionBehaviour>((o) => { });
                stringOption.TitleText.text = option.GetTranslatedName();
                stringOption.Value = stringOption.oldValue = option.CurSelection;
                stringOption.ValueText.text = option.GetTranslatedValue();
                stringOption.gameObject.name = string.Concat(
                    StringOptionName, option.Id);
                stringOption.gameObject.SetActive(true);

                this.menuBody.Children[index] = stringOption;
                option.SetOptionBehaviour(stringOption);
                
                this.blockTimer = 1.0f;
            }

            this.childrenBody = this.useOption.Zip(this.menuBody.Children, ValueTuple.Create).ToArray();
        }

        public void Start()
        {
            this.tabName.SetText(
                Helper.Translation.GetString(this.gameObject.name));
        }

        public void FixedUpdate()
        {

            this.blockTimer += Time.fixedDeltaTime;
            int itemLength = this.menuBody.Children.Length;

            if (itemLength == 0 || this.blockTimer < 0.1f) { return; }

            this.blockTimer = 0.0f;

            float itemOffset = (float)itemLength;
            float posOffset = posOffsetInit;

            foreach (var (option, optionBody) in this.childrenBody.Span)
            {
                if (optionBody == null) { continue; }

                bool enabled = option.IsActive();

                optionBody.gameObject.SetActive(enabled);

                if (enabled)
                {
                    bool isHeader = option.IsHeader;
                    posOffset -= isHeader ? 0.75f : 0.5f;
                    optionBody.transform.localPosition = new Vector3(
                        optionBody.transform.localPosition.x, posOffset,
                        optionBody.transform.localPosition.z);

                    if (isHeader)
                    {
                        itemOffset += 0.5f;
                    }
                }
                else
                {
                    itemOffset--;
                }
            }

            this.scroller.ContentYBounds.max = -4.0f + itemOffset * 0.5f;
        }

        [HideFromIl2Cpp]
        public void AddOption(IOption option)
        {
            this.useOption.Add(option);
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
