using UnityEngine;

using TMPro;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.CustomMonoBehaviour
{
    [Il2CppRegister]
    public sealed class LoadingText : MonoBehaviour
    {
        private TextMeshPro text;
        private string message;
        private int frameCount = 0;

        private const int addCommaFrame = 45;
        private const int maxFrame = 135;
        private const string comma = "・";
        private static readonly Color[] commaColor =
            { Palette.CrewmateBlue, Palette.ImpostorRed, ColorPalette.NeutralColor };

        public void Awake()
        {
            this.frameCount = 0;

            var hudManager = FastDestroyableSingleton<HudManager>.Instance;
            this.text = Instantiate(
                hudManager.TaskText,
                hudManager.transform.parent);
            this.text.transform.localPosition = new Vector3(0.0f, 0.0f, -910f);
            this.text.alignment = TMPro.TextAlignmentOptions.Center;
            this.text.gameObject.layer = 5;
        }

        public void FixedUpdate()
        {
            if (this.frameCount == 0)
            {
                this.text.text = message;
            }

            ++this.frameCount;

            if (this.frameCount % addCommaFrame == 0)
            {
                this.text.text += Helper.Design.ColoedString(
                    commaColor[RandomGenerator.Instance.Next(3)],
                    comma);
            }

            this.frameCount = this.frameCount % maxFrame;
        }

        public void OnEnable()
        {
            this.text.gameObject.SetActive(true);
        }

        public void OnDisable()
        {
            this.text.gameObject.SetActive(false);
        }

        public void OnDestroy()
        {
            Destroy(this.text);
        }

        public void SetMessage(string message)
        {
            this.message = message;
        }
        public void SetFontSize(float size)
        {
            this.text.fontSizeMin = this.text.fontSizeMax = size;
        }
    }
}
