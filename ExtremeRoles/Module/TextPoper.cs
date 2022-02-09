using System.Collections.Generic;

using TMPro;
using UnityEngine;

namespace ExtremeRoles.Module
{
    public class TextPoper
    {
        /*
            ・テキスト追加時のロジック
            1. 新しいテキストを目標座標に表示
            2. 今までのテキストの座標をずらす
            3. 設定されたサイズを超えている場合、古いテキストをデストロイ

            ・クリアロジック
            1. 全てのテキストをデストロイ
            2. 内部データ構造をリセット

            ・各テキストアップデートロジック
            1. 各テキストのタイマーをアップデート(タイマーを減らす)
            2. タイマーが0になったものをデストロイ

            # 上記を踏まえて
            ・このTextGeneratorクラスのメソッド
                ・コンストラクタ
                ・void AddText(string printString)
                ・void Update()
                ・void Clear()
            ・内部データ構造の案
                ・List:
                    データイン:O(1)、データアウト:O(1)(RemoveAt)、アップデート(全要素アクセス):O(n)
                ・Queue:
                    データイン:O(1)、データアウト:O(1)、アップデート(全要素アクセス):O(n×n)
                    (公式ドキュメントにQueueのGetEnumerator()でQueueに先頭と末尾に以外にアクセスしたときの計算量が無い
                     情報科学的には、Queueは先頭と末尾以外のアクセスの計算量はO(n)なので全要素アクセスはO(n×n)のはず)
                => データ構造をListとして、追加する場所を指定するindexerをメンバ変数で管理する
        */



        private class Text
        {
            private float timer;
            private TextMeshPro body;
            public Text(
                string printString,
                float disapearTime,
                Vector3 pos,
                TextAlignmentOptions offset)
            {
                this.body = Object.Instantiate(
                    Prefab.Text, Camera.main.transform, false);
                this.body.transform.localPosition = pos;
                this.body.alignment = offset;
                this.body.gameObject.layer = 5;
                this.body.text = printString;

                this.body.gameObject.SetActive(true);

                this.timer = disapearTime;
            }

            public void ShiftPos(
                Vector3 pos)
            {
                this.body.transform.localPosition += pos;
            }

            public void Update()
            {
                if (this.body == null) { return; }
                this.timer -= Time.deltaTime;
                if (this.timer < 0)
                {
                    Clear();
                }
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

        public TextPoper(
            int size,
            float disapearTime,
            Vector3 firstPos,
            TextAlignmentOptions offset)
        {
            this.showText = new List<Text>(size);
            for (int i = 0; i < this.showText.Capacity; ++i)
            {
                this.showText[i] = null;
            }
            
            this.disapearTime = disapearTime;
            this.showPos = firstPos;
            this.textOffest = offset;

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
                this.textOffest);

            ++this.indexer;
            this.indexer = this.indexer % this.showText.Count;
        }

        public void Update()
        {
            foreach (var text in this.showText)
            {
                if (text == null) { continue; }
                text.Update();
            }
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
