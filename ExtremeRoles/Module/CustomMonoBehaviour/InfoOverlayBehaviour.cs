using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ExtremeRoles.Module.CustomMonoBehaviour
{
    [Il2CppRegister]
    public sealed class InfoOverlayBehaviour : MonoBehaviour
    {
        private TextMeshProUGUI gameOptionTitle;
        private TextMeshProUGUI gameOptionBody;

        private TextMeshProUGUI basicInfoTitle;
        private TextMeshProUGUI basicInfoBody;

        private TextMeshProUGUI aditionalInfoTitle;
        private TextMeshProUGUI additionalInfoBody;

        private Image bkImage;

        public void Awake()
        {
            Transform trans = base.transform;

            this.bkImage = trans.Find(
                "Background").gameObject.GetComponent<Image>();

            this.aditionalInfoTitle = trans.Find(
                "Anchor/AditionalInfo/Title").gameObject.GetComponent<TextMeshProUGUI>();
            this.additionalInfoBody = trans.Find(
                "Anchor/AditionalInfo/ScrollView/Viewport/BodyText").gameObject.GetComponent<TextMeshProUGUI>();

            this.basicInfoTitle = trans.Find(
                "Anchor/BasicInfo/Title").gameObject.GetComponent<TextMeshProUGUI>();
            this.basicInfoBody = trans.Find(
                "Anchor/BasicInfo/ScrollView/Viewport/BodyText").gameObject.GetComponent<TextMeshProUGUI>();

            this.gameOptionTitle = trans.Find(
                "Anchor/GameOptionInfo/Title").gameObject.GetComponent<TextMeshProUGUI>();
            this.gameOptionBody = trans.Find(
                "Anchor/GameOptionInfo/ScrollView/Viewport/BodyText").gameObject.GetComponent<TextMeshProUGUI>();
        }

        public void SetTextStyle(TextMeshPro baseText)
        {
            setStyle(ref this.gameOptionTitle, baseText);
            setStyle(ref this.gameOptionBody, baseText);
            setStyle(ref this.basicInfoTitle, baseText);
            setStyle(ref this.basicInfoBody, baseText);
            setStyle(ref this.aditionalInfoTitle, baseText);
            setStyle(ref this.additionalInfoBody, baseText);
        }

        public void SetBkColor(Color newColor)
        {
            this.bkImage.color = newColor;
        }

        public void SetTextColor(Color newColor)
        {
            this.gameOptionTitle.color = newColor;
            this.gameOptionBody.color = newColor;
            this.basicInfoTitle.color = newColor;
            this.additionalInfoBody.color = newColor;
        }

        public void SetGameOption(string title, string body)
        {
            this.gameOptionTitle.text = title;
            this.gameOptionBody.text = body;
        }

        public void UpdateBasicInfo(string title, string infoText)
        {
            this.basicInfoTitle.text = title;
            this.basicInfoBody.text = infoText;
        }

        public void UpdateAditionalInfo(string title, string infoText)
        {
            this.aditionalInfoTitle.text = title;
            this.additionalInfoBody.text = infoText;
        }

        private static void setStyle(ref TextMeshProUGUI target, TextMeshPro style)
        {
            
            target.font = Instantiate(style.font);

            target.fontSize = target.fontSizeMin = target.fontSizeMax = 16.5f;
            target.enableWordWrapping = false;
            target.alignment = TextAlignmentOptions.TopLeft;
            target.transform.localScale = Vector3.one;
            target.color = Palette.White;
        }
    }
}
