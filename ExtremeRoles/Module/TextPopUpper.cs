using System.Collections.Generic;

using TMPro;
using UnityEngine;

namespace ExtremeRoles.Module
{
    public sealed class TextPopUpper
    {
        private sealed class Text
        {
            private TextMeshPro body;
            public Text(
                string printString,
                float disapearTime,
                Vector3 pos,
                TextAlignmentOptions offset,
                bool isWrap = true)
            {
                this.body = Object.Instantiate(
                    Prefab.Text, Camera.main.transform, false);

                this.body.enableWordWrapping = isWrap;
                this.body.GetComponent<RectTransform>().sizeDelta = new Vector2(3.0f, 0.75f);

                this.body.transform.localPosition = pos;
                this.body.alignment = offset;
                this.body.gameObject.layer = 5;
                this.body.text = printString;

                this.body.gameObject.SetActive(true);
                Object.Destroy(this.body, disapearTime);
            }

            public void ShiftPos(
                Vector3 pos)
            {
                this.body.transform.localPosition += pos;
            }

            public void Clear()
            {
                if (this.body == null) { return; }
                Object.Destroy(this.body);
            }

        }
        private List<Text> showText = new List<Text>();
        private int indexer = 0;
        private float disapearTime;
        private Vector3 showPos;
        private TextAlignmentOptions textOffest;
        private bool isWrap;

        public TextPopUpper(
            int size,
            float disapearTime,
            Vector3 firstPos,
            TextAlignmentOptions offset,
            bool isWrap = true)
        {
            this.showText = new List<Text>(size);
            for (int i = 0; i < this.showText.Capacity; ++i)
            {
                this.showText.Add(null);
            }
            
            this.disapearTime = disapearTime;
            this.showPos = firstPos;
            this.textOffest = offset;
            this.isWrap = isWrap;

            this.indexer = 0;
        }

        public void AddText(string printString)
        {
            foreach (var text in this.showText)
            {
                if (text == null) { continue; }
                text.ShiftPos(new Vector3(0f, 0.5f, 0f));
            }
         
            var oldText = this.showText[indexer];
            if (oldText != null)
            {
                oldText.Clear();
            }
            this.showText[indexer] = new Text(
                printString,
                this.disapearTime,
                this.showPos,
                this.textOffest,
                this.isWrap);

            ++this.indexer;
            this.indexer = this.indexer % this.showText.Count;
        }
        public void Clear()
        {
            foreach (var text in this.showText)
            {
                if (text == null) { continue; }
                text.Clear();
            }
        }
    }
}
