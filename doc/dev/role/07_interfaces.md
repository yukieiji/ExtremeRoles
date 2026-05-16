# インターフェース

役職作成において頻繁に使用する機能拡張のためのインターフェースについて説明します。

## 機能拡張インターフェースの概要

特定のインターフェースを実装することで、様々なゲームイベントにフックできます。
- **`IRole` から始まる名前**: 主に `SingleRoleBase` (Roleクラス) に実装することを目的としています。
- **それ以外の名前**: 主に `AbilityHandler` や `StatusModel` に実装することを目的としています。

---

## Roleクラス (SingleRoleBase) 向け

### 基本イベント
#### `IRoleUpdate`
毎フレームの更新処理を実装します。
- `void Update(PlayerControl rolePlayer)`
  - `rolePlayer`: この役職を持つプレイヤーのインスタンス。

#### `IRoleResetMeeting`
会議の開始時と終了時のリセット処理を実装します。
- `void ResetOnMeetingStart()`
  - 会議が開始された瞬間に呼ばれます。
- `void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer)`
  - 会議が終了し、追放が確定した後に呼ばれます。
  - `exiledPlayer`: 追放されたプレイヤーの情報（誰も追放されなかった場合は `null`）。

#### `IRoleSpecialSetUp`
役職の導入（イントロ）演出に関するセットアップを行います。
- `void IntroBeginSetUp()`
  - イントロ演出が開始される直前に呼ばれます。
- `void IntroEndSetUp()`
  - イントロ演出が終了した直前に呼ばれます。

---

### 投票・会議
#### `IRoleVoteModifier`
投票結果を動的に書き換えます。
- `int Order { get; }`
  - 変更の優先順位。`ModOrder` 列挙型を使用して指定します。
- `void ModifiedVote(byte rolePlayerId, ref Dictionary<byte, byte> voteTarget, ref Dictionary<byte, int> voteResult)`
  - `rolePlayerId`: 役職保持者のプレイヤーID。
  - `voteTarget`: 誰が誰に投票したかの辞書。
  - `voteResult`: 各プレイヤーが獲得した票数の辞書。
- `IEnumerable<VoteInfo> GetModdedVoteInfo(VoteInfoCollector collector, NetworkedPlayerInfo rolePlayer)`
  - 投票の演出（投票画面のバッジ等）を書き換えます。
- `void ResetModifier()`
  - 会議終了後などに内部状態をリセットします。

#### `IRoleMeetingButtonAbility`
会議中の投票画面に特殊なボタンを追加します。
- `bool IsBlockMeetingButtonAbility(PlayerVoteArea instance)`: ボタンを無効化するかどうか。
- `void ButtonMod(PlayerVoteArea instance, UiElement abilityButton)`: ボタンの見た目をカスタマイズ。
- `Action CreateAbilityAction(PlayerVoteArea instance)`: ボタンが押された時の動作を返します。
- `Sprite AbilityImage { get; }`: ボタンのアイコン画像。

#### `IRoleHookVoteEnd`
投票結果が確定した直後に呼ばれます。
- `void HookVoteEnd(MeetingHud instance, NetworkedPlayerInfo rolePlayer, IReadOnlyDictionary<byte, int> voteIndex)`
  - `voteIndex`: 各プレイヤーが得た最終的な票数。

---

### フック・割り込み
#### `IRoleReportHook`
レポートや緊急会議ボタンの挙動に割り込みます。
- `void HookReportButton(PlayerControl rolePlayer, NetworkedPlayerInfo reporter)`
  - `reporter`: ボタンを押したプレイヤー。
- `void HookBodyReport(PlayerControl rolePlayer, NetworkedPlayerInfo reporter, NetworkedPlayerInfo reportBody)`
  - `reportBody`: 通報された死体の情報。

#### `IRolePerformKillHook`
キルアニメーションの開始時と終了時に呼ばれます。
- `void OnStartKill()`
- `void OnEndKill()`

#### `IRoleMurderPlayerHook`
プレイヤーが殺害される瞬間の処理に割り込みます（キルボタン押下時ではなく、実際の死亡処理時）。
- `void HookMuderPlayer(PlayerControl source, PlayerControl target)`
  - `source`: 加害者, `target`: 被害者。

---

### 特殊状態・ライフサイクル
#### `IRoleWinPlayerModifier`
勝利条件や勝利チームの判定を書き換えます。
- `void ModifiedWinPlayer(NetworkedPlayerInfo rolePlayerInfo, GameOverReason reason, in WinnerContainer winner)`
  - `winner`: 勝利者リストを含むコンテナ。この内容を書き換えることで勝利者を操作できます。

#### `IRoleAwake<T>`
「覚醒」などの状態変化を持つ役職で使用します。
- `bool IsAwake { get; }`: 覚醒しているかどうか。
- `T NoneAwakeRole { get; }`: 未覚醒状態のID。
- `string GetFakeOptionString()`: オプション画面での表示。

#### `IRoleOnRevive` / `IRoleReviveHook`
蘇生に関する処理を実装します。
- `void ReviveAction(PlayerControl player)`: 自身が蘇生した時の処理。
- `void HookRevive(PlayerControl revivePlayer)`: 誰かが蘇生した時に呼ばれるフック。

#### `IRoleFakeIntro`
イントロ画面で別のチームとして表示させたい場合に実装します。
- `ExtremeRoleType FakeTeam { get; }`: 表示上のチーム（Crewmate, Impostorなど）。

#### `IRoleSpecialReset`
役職が剥奪されたり、特殊なリセットが必要な場合に実装します。
- `void AllReset(PlayerControl rolePlayer)`

---

## 能力・ロジック (AbilityHandler) 向け

#### `IKilledFrom`
キルボタンが押された瞬間の判定を行います。
- `bool TryKilledFrom(PlayerControl rolePlayer, PlayerControl fromPlayer)`
  - `true` を返すと、通常のキル処理を続行（殺害を許可）します。

#### `IInvincible`
無敵状態やキル・能力の対象外判定を行います。
- `bool IsBlockKillFrom(byte? fromPlayer)`: `fromPlayer` からのキルをブロックするか。
- `bool IsValidKillFromSource(byte source)`: 指定されたソースからのキルが有効か。

#### `IExiledAnimationOverrideWhenExiled`
追放時のアニメーションテキストなどを上書きします。
- `OverrideInfo? OverrideInfo { get; }`: `ExiledPlayer` と `AnimationText` を含むレコード。

#### `IVoteCheck`
投票を行った時のフックです。
- `void VoteTo(byte target)`: 投票先のプレイヤーID。

#### `ITryKillTo`
自分が誰かをキルしようとした時の判定です。
- `bool TryRolePlayerKillTo(PlayerControl rolePlayer, PlayerControl targetPlayer)`

---

## 状態管理 (StatusModel) 向け

#### `IDeadBodyReportOverrideStatus`
死体の通報可否を制御します。
- `bool CanReport { get; }`

#### `IFakeImpostorStatus`
ゲームシステム上でインポスターとして扱われるかどうか（通報画面の赤文字など）。
- `bool IsFakeImpostor { get; }`

#### `IStatusMovable`
移動可能かどうかを制御します。
- `bool CanMove { get; }`

#### `ISubTeam`
第三陣営内での所属チーム（例：ジャッカルチーム等）を定義します。
- `NeutralSeparateTeam Main { get; }`
- `NeutralSeparateTeam Sub { get; }`

#### `IUsableOverrideStatus`
ボタンの有効化状態を制御します。
- `bool EnableUseButton { get; }`
- `bool EnableVentButton { get; }`

---

## 実装例

```csharp
public sealed class MyRoleAbilityHandler : IAbility, IKilledFrom
{
    // キルボタンを押された時の処理
    public bool TryKilledFrom(PlayerControl rolePlayer, PlayerControl fromPlayer)
    {
        // 50%の確率でキルを回避
        if (UnityEngine.Random.value > 0.5f)
        {
            return false; // キルを阻止
        }
        return true; // キルを許可
    }
}
```
