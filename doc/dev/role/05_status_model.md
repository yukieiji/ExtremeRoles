# 状態管理（StatusModel）

`StatusModel` は役職の実行時の状態（データ）を保持するためのクラスです。

## 基本構造

`IStatusModel` インターフェースを実装します。

```csharp
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Crewmate.MyRole;

public sealed class MyRoleStatusModel : IStatusModel
{
    // 例：現在のストレスゲージ
    public float StressGage { get; set; } = 0f;

    // 例：能力が発動中かどうか
    public bool IsActive { get; set; } = false;

    // 例：ロックオンしているターゲットのID
    public byte? TargetId { get; set; }
}
```

## Roleクラスとの紐付け

Roleクラスの `Status` プロパティでこのクラスを返すようにします。

```csharp
public sealed class MyRole : SingleRoleBase
{
    public override IStatusModel? Status => status;
    private MyRoleStatusModel? status;

    protected override void RoleSpecificInit()
    {
        this.status = new MyRoleStatusModel();
    }
}
```

## メリット

- **データの集約**: 役職が持つべきデータを一箇所にまとめることができます。
- **AbilityHandlerとの共有**: `AbilityHandler` のコンストラクタに `StatusModel` を渡すことで、ロジック側から状態を読み書きできるようになります。
- **デバッグの容易性**: 役職の状態が独立しているため、デバッグ時に状態を確認しやすくなります。
