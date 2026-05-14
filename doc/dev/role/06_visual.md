# ビジュアル（Visual）

`Visual` は役職の見た目(他人から見た時/自分から見た時)などのビジュアル情報を管理するクラスデス

## 基本構造

`IVisual` インターフェースを実装します。`IVisual` はアクセス用のプレースホルダーインターフェースです

```csharp
using ExtremeRoles.Roles.API.Interface.Visual;

namespace ExtremeRoles.Roles.Solo.Crewmate.MyRole;

public sealed class MyRoleVisual : IVisual, ILookedTag
{
    public string GetLookedToThisTag(byte from)
    {
        // from から見られた時のタグ
    }
}
```



## ロジックの分離

最新の設計では、Roleクラスの `GetTargetRoleSeeColor` をオーバーライドする際は`Visual`のメソッドを呼び出すようにし、Roleクラス自体には複雑なロジックを置かないようにします。

```csharp
// Role.cs
public Color GetTargetRoleSeeColor(
    SingleRoleBase targetRole,
    byte targetPlayerId
)
{
    return this.visual?.GetTargetRoleSeeColor(targetRole, targetPlayerId) ?? Color.white;
}

// Visual.cs
public Color GetTargetRoleSeeColor(
    SingleRoleBase targetRole,
    byte targetPlayerId)
{
    // 実際の処理
}
```
