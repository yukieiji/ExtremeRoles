# language: ja

機能: 緊急タスクの制限時間設定

  ゲームマップごとに設定された緊急タスクの制限時間が正しく反映されることを確認する

  シナリオ: マップの緊急タスクの制限時間設定
    前提 <マップ>の<緊急タスク>が<タイム>秒で設定されている
    *  エラー無く<マップ>でゲームが始まっている
    もし <緊急タスク>が起きた
    ならば <緊急タスク>の制限時間が<タイム>秒で開始される

    例:
        | マップ | 緊急タスク | タイム |
        | スケルド | 酸素枯渇 | 5 |
        | エアシップ | 衝突回避 | 20 |
