# 特殊勝利および勝利決定処理の実装

ExtremeRolesにおける特殊勝利や勝利判定、およびリザルト画面での勝者決定処理の実装方法について解説します。

---

## 全体概要

勝利周りの処理は、役割ごとに大きく3つのフェーズ・機能に分かれています。

1. **ゲーム終了判定ロジック (Game End Logic)**: ゲームを終了させる条件（誰が勝ってゲームが終了するか）を判定する処理。
2. **勝者決定ロジック (Winner Determination Logic)**: ゲーム終了後、リザルト画面に表示する最終的な勝者リスト (`WinnerContainer`) を構築・補正する処理。
3. **リザルト画面表示ロジック (Result Display Logic)**: `EndGameManager` を通じてリザルト画面の勝率テキストや表示色を設定する処理。

---

## 1. ゲーム終了判定ロジック (Game End Logic)

ゲーム終了条件の判定には、役職の性質に応じて3つのパターンが存在します。

### パターンA: Simple Role-based 判定 (`NeutralSpecialWinChecker`)

`Jester`（追放時勝利）や `TaskMaster`（タスク完遂時勝利）のように、役職（またはAbility/Status）内部で条件を満たした時に `IsWin = true` とし、自身で勝利フラグを立てる形式です。

#### 実装手順
1. `Role.IsWin` プロパティが `true` を返すように実装します。
2. `ExtremeRoles/Module/GameEnd/NeutralSpecialWinChecker.cs` の `TryCheckGameEnd` メソッド内にある `switch (role.Core.Id)` にケースを追加します。

```csharp
// NeutralSpecialWinChecker.cs
reason = (GameOverReason)(role.Core.Id switch
{
    ExtremeRoleId.Alice => RoleGameOverReason.AliceKilledByImposter,
    ExtremeRoleId.Jester => RoleGameOverReason.JesterMeetingFavorite,
    ExtremeRoleId.MyRole => RoleGameOverReason.MyRoleSpecialWin, // 追加
    _ => RoleGameOverReason.UnKnown,
});
```

---

### パターンB: カスタム WinChecker 判定 (`SpecialRoleWinChecker` / `IWinChecker`)

`Vigilante`, `Yandere`, `Hatter` のように、複数役職の生存比率や盤面全体の状態を毎フレームチェックして特殊勝利判定を行う形式です。

#### 実装手順
1. **`IWinChecker` の実装クラス作成**: `ExtremeRoles/Module/SpecialWinChecker/` 内に作成します。

```csharp
using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Module.SpecialWinChecker;

internal sealed class MyRoleWinChecker : IWinChecker
{
    public RoleGameOverReason Reason => RoleGameOverReason.MyRoleSpecialWin;

    public void AddAliveRole(byte playerId, SingleRoleBase role) { }

    public bool IsWin(IPlayerStatistics statistics)
    {
        // 盤面情報を元に勝利条件を判定
        return statistics.TotalAlive == 1;
    }
}
```

2. **`SpecialWinCheckRole` への登録**: `ExtremeRoles/Roles/ExtremeRoleManager.cs` の `SpecialWinCheckRole` に役職IDを追加します。

```csharp
public static readonly IReadOnlySet<ExtremeRoleId> SpecialWinCheckRole = new HashSet<ExtremeRoleId>()
{
    // ... 既存役職 ...
    ExtremeRoleId.MyRole,
};
```

3. **`PlayerStatistics` での生成処理**: `ExtremeRoles/Module/GameEnd/PlayerStatistics.cs` の `addSpecialWinCheckRole` メソッド内の switch に生成を追加します。

```csharp
winChecker = roleId switch
{
    ExtremeRoleId.Vigilante => new VigilanteWinChecker(),
    ExtremeRoleId.MyRole => new MyRoleWinChecker(), // 追加
    _ => null,
};
```

---

### パターンC: 単独・陣営生存判定 (`NeutralAliveWinChecker` / `NeutralSeparateTeamBuilder`)

第三陣営が他の陣営を全滅させた（あるいは単独過半数を獲得した）ことによる勝利判定です。

#### 実装手順
1. **`NeutralSeparateTeam` 列挙型への追加**: `ExtremeRoles/Roles/ExtremeRoleManager.cs` 内の `NeutralSeparateTeam` に独自のチームIDを追加します。

```csharp
public enum NeutralSeparateTeam
{
    None = -100,
    Jackal = 0,
    // ...
    MyRoleTeam, // 追加
}
```

2. **`NeutralSeparateTeamBuilder` へのマッピング**: `ExtremeRoles/Module/GameEnd/PlayerStatistics.cs` 内の `NeutralSeparateTeamBuilder.Add` メソッドにケースを追加します。

```csharp
var team = roleId switch
{
    ExtremeRoleId.Jackal or ExtremeRoleId.Sidekick => NeutralSeparateTeam.Jackal,
    ExtremeRoleId.MyRole => NeutralSeparateTeam.MyRoleTeam, // 追加
    _ => NeutralSeparateTeam.None,
};
```

3. **`NeutralAliveWinChecker` へのマッピング**: `ExtremeRoles/Module/GameEnd/NeutralAliveWinChecker.cs` 内の `switch (team)` にケースを追加します。

```csharp
endReason = team switch
{
    NeutralSeparateTeam.Jackal => RoleGameOverReason.JackalKillAllOther,
    NeutralSeparateTeam.MyRoleTeam => RoleGameOverReason.MyRoleKillAllOther, // 追加
    _ => RoleGameOverReason.UnKnown
};
```

---

## 2. 勝者決定ロジック (Winner Determination Logic)

ゲームが終了した際、どのプレイヤーを勝者としてリザルトに含めるかを決定・補正します。

### 1. `RoleGameOverReason` の定義

`ExtremeRoles/Roles/ExtremeRoleManager.cs` の `RoleGameOverReason` 列挙型に、固有の終了理由を追加します。

```csharp
public enum RoleGameOverReason
{
    // ... 既存項目 ...
    MyRoleSpecialWin,
    MyRoleKillAllOther,
}
```

---

### 2. 勝者置換処理 (`ReplaceWinnerProcessor`)

ゲーム終了理由 (`RoleGameOverReason`) に応じて、`WinnerContainer` (勝者リスト) を置き換えます。

`ExtremeRoles/Module/GameResult/WinnerProcessor/ReplaceWinnerProcessor.cs` の `Process` メソッドに処理を追加します。

```csharp
// ReplaceWinnerProcessor.cs
switch ((RoleGameOverReason)ExtremeRolesPlugin.ShipState.EndReason)
{
    case RoleGameOverReason.MyRoleSpecialWin:
    case RoleGameOverReason.MyRoleKillAllOther:
        processor.ReplaceWinnerToSpecificNeutralRolePlayer(
            ExtremeRoleId.MyRole);
        break;
}
```

- **`processor.ReplaceWinnerToSpecificNeutralRolePlayer(...)`**: 指定された第三陣営役職の保持者を勝者リストに設定します。第1引数に `bool isAlive`（生存者のみにするか）を指定することも可能です。
- **`processor.ReplaceWinnerToSpecificRolePlayer(...)`**: 陣営に関わらず指定役職の保持者を勝者リストに設定します。

---

### 3. 勝者修正・追加 (`IRoleWinPlayerModifier`)

メインの勝者以外にも、「特定の条件を満たしたプレイヤーを勝利者に追加する」といった処理を行いたい場合は、Roleクラスに `IRoleWinPlayerModifier` を実装します。

```csharp
public sealed class MyRole : SingleRoleBase, IRoleWinPlayerModifier
{
    public void ModifiedWinPlayer(
        NetworkedPlayerInfo rolePlayerInfo,
        GameOverReason reason,
        in WinnerContainer winner)
    {
        // 例えばインポスター勝利時に条件を満たしていれば追加勝者にする等
        if (reason == GameOverReason.ImpostorsByKill && this.status.IsConditionMet)
        {
            winner.AddPlusWinner(rolePlayerInfo);
        }
    }
}
```

---

## 3. リザルト画面表示ロジック (`EndGameManagerPatch`)

ゲーム終了後、リザルト画面（勝利/敗北画面）に表示される勝利理由テキストや文字色・背景色を設定します。

### 実装手順

`ExtremeRoles/Patches/Manager/EndGameManagerPatch.cs` の `createWinTextInfo` メソッド内の switch にケースを追加します。

```csharp
private static WinTextInfo createWinTextInfo(in RoleGameOverReason reason)
    => reason switch
    {
        // ... 既存ケース ...
        RoleGameOverReason.MyRoleSpecialWin or
        RoleGameOverReason.MyRoleKillAllOther =>
            new(ExtremeRoleId.MyRole, ColorPalette.MyRoleColor),

        _ => new(RoleGameOverReason.UnKnown, Color.black)
    };
```

- `new(ExtremeRoleId.MyRole, ColorPalette.MyRoleColor)`: 役職IDを渡すと、翻訳キーに基づいた役職名テキストと指定色を自動で適用します。

---

## 実装チェックリスト

新しい特殊勝利を持つ役職を追加する際は、以下のステップを確認してください。

- [ ] `RoleGameOverReason` に新しい勝利理由を追加する
- [ ] ゲーム終了判定（`NeutralSpecialWinChecker` / `IWinChecker` / `NeutralAliveWinChecker`）を追加・設定する
- [ ] `ReplaceWinnerProcessor` で勝者リストの置換ロジックを登録する
- [ ] 必要に応じて `IRoleWinPlayerModifier` を実装する
- [ ] `EndGameManagerPatch.createWinTextInfo` に勝利テキスト・色の表示設定を追加する
