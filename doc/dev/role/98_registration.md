# 役職の登録（Registration / Selector）

作成した役職クラスをMODに登録し、ゲーム内で選択可能にするための手順を説明します。

## 1. 役職IDの追加

`ExtremeRoles/Roles/ExtremeRoleManager.cs` 内の `ExtremeRoleId` 列挙型に、新しい役職のIDを追加します。

```csharp
public enum ExtremeRoleId : int
{
    // ... 既存のID ...

    MyRole, // 新しい役職IDを追加
}
```

## 2. ExtremeRoleManager への登録

同じファイルの `NormalRole` 辞書に、IDとクラスのインスタンスを紐付けます。これにより、役職の基本情報の初期化やRPCハンドリングが可能になります。

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

## 3. Selector への登録（出現リストへの追加）

役職をゲームモードの抽選対象に含めるには、各種 `RoleSelector` に登録する必要があります。

### Classicモード
`ExtremeRoles/GameMode/RoleSelector/ClassicGameModeRoleSelector.cs` の `getUseNormalRoleId()` メソッド内の配列に、作成した `ExtremeRoleId` を追加します。

```csharp
private static ExtremeRoleId[] getUseNormalRoleId() =>
[
    // ... 既存の役職 ...
    ExtremeRoleId.MyRole,
];
```

### かくれんぼ（HideNSeek）モード
かくれんぼモードでも出現させたい場合は、 `ExtremeRoles/GameMode/RoleSelector/HideNSeekGameModeRoleSelector.cs` の `getUseNormalId()` にも追加します。
