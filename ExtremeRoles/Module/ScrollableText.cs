using System;
using System.Linq;
using UnityEngine;

using TMPro;

namespace ExtremeRoles.Module
{
    public sealed class ScrollableText
    {

        public GameObject AnchorPoint => this.anchorPoint;
        public TextMeshPro Title => this.title;

        public GameObject Body => this.body;
        public TextMeshPro BodyText => this.bodyText;

        private GameObject anchorPoint;

        private TextMeshPro title;

        private GameObject body;
        private TextMeshPro bodyText;
        private Scroller textScroller;

        private string name;
        private int maxRow;
        private float maxHight;
        private float minHight;

        public ScrollableText(
            string name,
            int maxRow,
            float minHight,
            float maxHight)
        {
            this.name = name;
            this.maxRow = maxRow;
            this.maxHight = maxHight;
            this.minHight = minHight;
        }

        public void Clear()
        {
            UnityEngine.Object.Destroy(this.textScroller);
            UnityEngine.Object.Destroy(this.bodyText);
            UnityEngine.Object.Destroy(this.body);
            UnityEngine.Object.Destroy(this.title);
            UnityEngine.Object.Destroy(this.anchorPoint);
            this.anchorPoint = null;
        }

        public void Enable(bool isEnable)
        {
            if (this.title != null)
            {
                this.title.enabled = isEnable;
            }
            if (this.bodyText != null)
            {
                this.bodyText.enabled = isEnable;
            }
        }

        public void Initialize(
            Vector3 pos,
            Vector3 bodyOffset,
            TextMeshPro template,
            Transform parent = null,
            Action<TextMeshPro> textProcess = null)
        {

            if (this.anchorPoint != null) { return; }

            this.anchorPoint = new GameObject($"{this.name}Anchor");

            this.title = UnityEngine.Object.Instantiate(
                template, this.anchorPoint.transform);
            this.title.name = $"{this.name}Title";

            if (textProcess != null)
            {
                textProcess(this.title);
            }

            this.body = new GameObject($"{this.name}Body");
            this.body.transform.SetParent(this.anchorPoint.transform);
            this.bodyText = UnityEngine.Object.Instantiate(
                template, this.body.transform);
            if (textProcess != null)
            {
                textProcess(this.bodyText);
            }
            this.bodyText.name = $"{this.name}Text";

            this.textScroller = this.body.AddComponent<Scroller>();
            this.textScroller.gameObject.layer = 5;
            this.textScroller.transform.localScale = Vector3.one;
            this.textScroller.allowX = false;
            this.textScroller.allowY = true;
            this.textScroller.active = true;
            this.textScroller.velocity = new Vector2(0, 0);
            this.textScroller.ScrollbarYBounds = new FloatRange(0, 0);
            this.textScroller.ContentXBounds = new FloatRange(0.0f, 0.0f);
            this.textScroller.enabled = true;
            this.textScroller.Inner = this.bodyText.transform;
            this.body.transform.SetParent(this.textScroller.transform);

            this.anchorPoint.transform.parent = parent;
            this.anchorPoint.transform.localPosition = pos;
            this.anchorPoint.layer = 5;
            this.body.transform.localPosition += bodyOffset;
        }

        public void UpdateText(string title, string bodyText)
        {
            this.title.text = title;
            this.bodyText.text = bodyText;

            int row = bodyText.Count(c  => c == '\n');
            float maxY = Mathf.Max(this.minHight, row * this.maxHight + (row - this.maxRow) * this.maxHight);
            this.textScroller.ContentYBounds = new FloatRange(this.minHight, maxY);
        }
    }
}
