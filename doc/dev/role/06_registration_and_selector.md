# 役職の登録と RoleSelector

作成した役職クラスをMODに認識させ、ゲームで利用可能にするための手順と、役職のアサイン（割り当て）の仕組みについて説明します。

## 1. 役職IDの追加

`ExtremeRoles/Roles/ExtremeRoleManager.cs` 内の `ExtremeRoleId` 列挙型に、新しい役職のIDを追加します。追加する場所（陣営ごとのブロック）に注意してください。

```csharp
public enum ExtremeRoleId : int
{
    // ... 既存のID ...

    MyRole, // 新しい役職IDを追加
}
```

## 2. ExtremeRoleManager への登録

同じファイルの `NormalRole` 辞書に、IDとクラスのインスタンスを紐付けます。

```csharp
public static class ExtremeRoleManager
{
    public static readonly ImmutableDictionary<int, SingleRoleBase> NormalRole =
        new Dictionary<int, SingleRoleBase>()
        {
            // ... 既存の登録 ...
            {(int)ExtremeRoleId.MyRole, new MyRole()},
        }.ToImmutableDictionary();
}
```

## 3. IRoleSelector によるアサインの仕組み

`IRoleSelector` は、ゲーム開始時に各プレイヤーにどの役職を割り当てるかを決定するインターフェースです。

- **Classicモード**: `ClassicGameModeRoleSelector` が使用されます。
- **かくれんぼモード**: `HideNSeekGameModeRoleSelector` が使用されます。

役職を `ExtremeRoleManager.NormalRole` に登録すると、標準的な役職アサインロジックの対象となります。

### 役職のアサインフロー

1.  ホストがゲームを開始する。
2.  `ExtremeGameModeManager` が現在の設定に基づき、適切な `IRoleSelector` を選択する。
3.  `RoleSelector` が役職の重み付けや設定を確認し、各プレイヤーに `ExtremeRoleId` を割り当てる。
4.  RPCを通じて全クライアントに割り当て情報が送信される。
5.  各クライアントで `ExtremeRoleManager.SetPlyerIdToSingleRoleId` が呼ばれ、役職クラスのインスタンスが生成・初期化される。

## 4. (オプション) 勝敗判定の登録

特殊な勝利条件を持つ役職の場合、`ExtremeRoleManager.SpecialWinCheckRole` にIDを追加し、別途 `WinChecker` を実装する必要があります。
