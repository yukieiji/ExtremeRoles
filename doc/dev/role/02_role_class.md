# Roleクラスの作成

Roleクラスは役職の中核となるクラスで、`SingleRoleBase`を継承します。

## 基本構造

以下は最も基本的なRoleクラスの実装例です。

```csharp
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;
using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Solo.Crewmate.MyRole;

#nullable enable

public sealed class MyRole : SingleRoleBase, IRoleUpdate
{
    public override IStatusModel? Status => status;
    private MyRoleStatusModel? status;
    private MyRoleAbilityHandler? ability;

    public MyRole() : base(
        RoleArgs.BuildCrewmate(
            ExtremeRoleId.MyRole,
            ColorPalette.MyRoleColor))
    {
    }

    public void Update(PlayerControl rolePlayer)
    {
        this.ability?.Update(rolePlayer);
    }

    protected override void RoleSpecificInit()
    {
        var loader = this.Loader;
        // StatusModelの初期化
        this.status = new MyRoleStatusModel();

        // AbilityHandlerの初期化
        this.ability = new MyRoleAbilityHandler(this.status);
        this.AbilityClass = this.ability;
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        // オプションの作成
    }
}
```

## RoleArgsによる基本設定

コンストラクタで `base(RoleArgs.Build...)` を呼び出すことで、役職の基本属性を設定します。

- `RoleArgs.BuildCrewmate`: クルーメイト陣営の役職
- `RoleArgs.BuildImpostor`: インポスター陣営の役職
- `RoleArgs.BuildNeutral`: 第三陣営の役職

### RolePropPresets

`RolePropPresets` を使用して、その役職が持つ基本的な権限（キルができる、ベントが使える、タスクがある等）をまとめて設定できます。

```csharp
public MyRole() : base(
    RoleArgs.BuildCrewmate(
        ExtremeRoleId.MyRole,
        ColorPalette.MyRoleColor,
        RolePropPresets.CrewmateDefault)) // 標準的なクルーメイトの権限
{
}
```

利用可能なプリセット:
- `CrewmateDefault`: タスクあり + 基本機能（会議、修理、管理端末など）
- `ImpostorDefault`: キル、ベント、サボタージュ、基本機能
- `OptionalDefault`: 基本機能のみ（会議、修理、管理端末など）

## 重要なメソッドとインターフェース

利用可能な主要メソッドや拡張インターフェースについては、[重要なメソッドとインターフェース](./08_interfaces_and_methods.md) を参照してください。
