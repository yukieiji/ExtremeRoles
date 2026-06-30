# 能力（Ability）の実装

`AbilityHandler` は役職の能動的な「能力」やロジックを管理するクラスとして分離すべきです

## 基本構造

`IAbility` インターフェースを実装します。`IAbility` はアクセス用のプレースホルダーインターフェースです

```csharp
using ExtremeRoles.Roles.API.Interface.Ability;

namespace ExtremeRoles.Roles.Solo.Crewmate.MyRole;

public sealed class MyRoleAbilityHandler(MyRoleStatusModel status) : IAbility
{
    private readonly MyRoleStatusModel status = status;

    public void Update(PlayerControl rolePlayer)
    {
        // 毎フレームのロジック（Role.Updateから呼び出される想定）
    }

    public void Reset()
    {
        // リセット処理
    }
}
```

## ボタン能力の実装

プレイヤーがボタンを押して発動する能力を作るには、Roleクラスで `IRoleAutoBuildAbility` インターフェースを実装します。

### 1. ボタンの生成 (Roleクラス)

```csharp
public sealed class MyRole : SingleRoleBase, IRoleAutoBuildAbility
{
    public ExtremeAbilityButton? Button { get; set; }

    public void CreateAbility()
    {
        // ボタンの生成。アイコン画像や翻訳キーを指定します。
        this.CreateNormalAbilityButton(
            "myRoleAbilityKey",
            UnityObjectLoader.LoadFromResources(ExtremeRoleId.MyRole));
    }

    // ボタンが表示されるかどうかの判定
    public bool IsAbilityUse()
    {
        // クールタイム中ではないか、対象が範囲内にいるか等を判定
        return IRoleAbility.IsCommonUse();
    }

    // ボタンが押された時の実行処理
    public bool UseAbility()
    {
        // 能力の実行ロジック
        return true;
    }
}
```

## ロジックの分離

最新の設計では、Roleクラスの `UseAbility` などから `AbilityHandler` のメソッドを呼び出すようにし、Roleクラス自体には複雑なロジックを置かないようにします。

```csharp
// Role.cs
public bool UseAbility()
{
    return this.ability?.Execute(this.targetPlayerId) ?? false;
}

// AbilityHandler.cs
public bool Execute(byte targetId)
{
    // 実際の処理
}
```

## 便利なインターフェース

AbilityHandler で活用できるインターフェースについては、[インターフェース](./07_interfaces.md) を参照してください。
