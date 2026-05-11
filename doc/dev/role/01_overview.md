# 役職作成の概要

Extreme Rolesにおける役職作成の全体像と、推奨される設計指針について説明します。

## 役職の基本構造

新しい役職を作成する際は、以下の3つのクラスに役割を分割して実装することが推奨されます。これにより、コードの可読性とメンテナンス性が向上します。

### 1. Roleクラス (XXXRole)
`SingleRoleBase`を継承したクラスです。役職の「定義」を担います。
- 役職の基本設定（陣営、色、ID）
- 他のコンポーネント（AbilityHandler, StatusModel）の生成と紐付け
- ゲームイベント（Update, 会議開始/終了など）の起点

### 2. AbilityHandlerクラス (XXXAbilityHandler)
`IAbility`を実装したクラスです。役職の「能力（ロジック）」を担います。
- 実際の能力実行処理
- 特殊な判定ロジック
- クールタイムや使用回数の管理

### 3. StatusModelクラス (XXXStatusModel)
`IStatusModel`を実装したクラスです。役職の「状態（データ）」を担います。
- ゲージの量、フラグ、ターゲットのプレイヤーIDなど、その役職固有のデータ保持

## 推奨されるディレクトリ構成

既存の `Loner` (孤独な人) や `CEO` のコードが非常に参考になります。

```
ExtremeRoles/Roles/Solo/Crewmate/MyRole/
├── MyRole.cs              (Roleクラス)
├── MyRoleAbilityHandler.cs (AbilityHandlerクラス)
└── MyRoleStatusModel.cs    (StatusModelクラス)
```

## 実装と登録の流れ

1.  **Roleクラスの作成**: `SingleRoleBase` を継承してクラスを定義します。
2.  **オプションの実装**: `CreateSpecificOption` で設定項目を作成します。
3.  **能力と状態の実装**: `AbilityHandler` と `StatusModel` を作成します。
4.  **インターフェースの実装**: 必要に応じて `IRoleUpdate` などのインターフェースを実装します。
5.  **役職の登録**: `ExtremeRoleManager` と `RoleSelector` に登録します。
6.  **翻訳の追加**: リソースファイル（`.resx`）に名称や説明を追加します。

各ステップの詳細は、番号順の各ドキュメントを参照してください。
