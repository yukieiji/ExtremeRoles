# 役職作成の概要

Extreme Rolesにおける役職作成の全体像と、推奨される設計指針について説明します。

## 役職の基本構造

新しい役職を作成する際は、以下の3つのクラスに役割を分割して実装することが推奨されます。これにより、コードの可読性とメンテナンス性が向上します。

### 1. Roleクラス (XXXRole)
`SingleRoleBase`を継承したクラスです。役職の「定義」を担います。
- 役職ID、名称、色の定義
- オプション（設定項目）の作成と初期化
- 他のコンポーネント（AbilityHandler, StatusModel）の生成と紐付け
- ゲームイベント（Update, 会議開始/終了など）の起点

### 2. AbilityHandlerクラス (XXXAbilityHandler)
`IAbility`を実装したクラスです。役職の「能力（ロジック）」を担います。
- ボタンが押された時の処理
- 特殊な能力の判定ロジック
- クールタイムや使用回数の管理

### 3. StatusModelクラス (XXXStatusModel)
`IStatusModel`を実装したクラスです。役職の「状態（データ）」を担います。
- ゲージの量、フラグ、ターゲットのプレイヤーIDなど、その役職固有のデータ保持

## 推奨されるディレクトリ構成

既存の `Loner` (孤独な人) のコードが非常に参考になります。

```
ExtremeRoles/Roles/Solo/Crewmate/MyRole/
├── MyRole.cs              (Roleクラス)
├── MyRoleAbilityHandler.cs (AbilityHandlerクラス)
└── MyRoleStatusModel.cs    (StatusModelクラス)
```

## 実装の流れ

1.  **役職IDの定義**: `ExtremeRoleId` 列挙型に新しいIDを追加します。
2.  **クラスの実装**: 上記の3つのクラスを作成します。
3.  **役職の登録とアサインの設定**: `ExtremeRoleManager` への登録とアサインの流れを確認します。
4.  **翻訳の追加**: リソースファイル（`.resx`）に名称や説明を追加します。

次のステップでは、各クラスの詳細な実装方法について説明します。
