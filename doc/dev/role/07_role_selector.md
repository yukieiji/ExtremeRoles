# RoleSelectorとゲームモード

役職がどのようにアサインされるか、およびゲームモードごとの制御について説明します。

## IRoleSelector の役割

`IRoleSelector` は、ゲーム開始時に各プレイヤーにどの役職を割り当てるかを決定するインターフェースです。

- **Classicモード**: `ClassicGameModeRoleSelector` が使用されます。
- **かくれんぼモード**: `HideNSeekGameModeRoleSelector` が使用されます。

役職を新しく作成した際、通常は `ExtremeRoleManager.NormalRole` に登録するだけで、標準的な役職アサインロジックの対象となります。

## 特定の役職の制限

特定のゲームモードでのみ出現させたい、あるいは出現を抑制したい場合は、各 `RoleSelector` クラスの実装を確認・修正する必要があります。

例: かくれんぼモードで特定の能力を無効化する場合
```csharp
// HideNSeekGameModeRoleSelector.cs 等で制御
```

## RoleManager との使い分け

- **ExtremeRoleManager**: MODが提供する全ての役職定義を管理します。
- **RoleManager (Vanilla)**: Among Us 本体の役職システムを管理します。
- **ExtremeRoleManager.GameRole**: 現在の対戦において、どのプレイヤーがどの役職（ExtremeRole）であるかの動的な状態を保持します。

## 役職のアサインフロー

1.  ホストがゲームを開始する。
2.  `ExtremeGameModeManager` が現在の設定に基づき、適切な `IRoleSelector` を選択する。
3.  `RoleSelector` が役職の重み付けや設定を確認し、各プレイヤーに `ExtremeRoleId` を割り当てる。
4.  RPCを通じて全クライアントに割り当て情報が送信される。
5.  各クライアントで `ExtremeRoleManager.SetPlyerIdToSingleRoleId` が呼ばれ、役職クラスのインスタンスが生成・初期化される。
