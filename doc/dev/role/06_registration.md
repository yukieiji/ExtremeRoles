# 役職の登録

作成した役職クラスをMODに認識させ、ゲームで利用可能にするための手順を説明します。

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

## 3. 翻訳テキストの追加

役職名や説明文を表示するために、リソースファイル (`.resx`) にテキストを追加します。
ファイル場所: `ExtremeRoles/Translation/resx/`

- `ExtremeRoles.en-US.resx` (英語)
- `ExtremeRoles.zh-Hans.resx` (簡体字中国語)
- `ExtremeRoles.zh-Hant.resx` (繁体字中国語)

追加が必要なキーの例:
- `{ExtremeRoleId}`: 役職名
- `{ExtremeRoleId}FullDescription`: 役職の詳細な説明
- `{ExtremeRoleId}IntroDescription`: ゲーム開始時の紹介文
- `{ExtremeRoleId}ImportantText`: 画面上部に表示される重要テキスト

## 4. (オプション) 勝敗判定の登録

特殊な勝利条件を持つ役職の場合、`ExtremeRoleManager.SpecialWinCheckRole` にIDを追加し、別途 `WinChecker` を実装する必要があります。
