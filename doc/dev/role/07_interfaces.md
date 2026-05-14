# インターフェース

役職作成において頻繁に使用する機能拡張のためのインターフェースについて説明します。

## 機能拡張インターフェース

特定のインターフェースを実装することで、様々なゲームイベントにフックできます。**`IRole` から始まる名前のインターフェースは、主に `SingleRoleBase` (Roleクラス) に実装することを目的としています。**

### Roleクラス (SingleRoleBase) 向け
これらのインターフェースは通常、役職の定義である Role クラスに実装します。

- **`IRoleUpdate`**
  - `Update(PlayerControl rolePlayer)`: 毎フレーム呼ばれます。
- **`IRoleResetMeeting`**
  - `ResetOnMeetingStart()`: 会議開始時に呼ばれます。
  - `ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer)`: 会議終了時に呼ばれます。

#### 投票・会議
- **`IRoleVoteModifier`**
  - `ModifiedVote(...)`: 投票結果を書き換えます。
  - `GetModdedVoteInfo(...)`: 投票の演出を書き換えます。
- **`IRoleMeetingButtonAbility`**
  - 会議中の投票画面に特殊なボタンを追加したい場合に実装します。

#### フック・割り込み
- **`IRoleReportHook`**
  - `HookReportButton(...)`: 緊急会議ボタンが押された時に割り込みます。
  - `HookBodyReport(...)`: 死体レポート時に割り込みます。
- **`IRolePerformKillHook`**
  - キルを実行する瞬間に割り込みます。
- **`IRoleMurderPlayerHook`**
  - プレイヤーが殺害される処理に割り込みます。

### 能力・ロジック (AbilityHandler) 向け
能動的な能力や詳細なロジックを担う `AbilityHandler` に実装できるインターフェースです。

- **`IKilledFrom`**
  - `IKilledFrom(PlayerControl rolePlayer, PlayerControl fromPlayer)`: `fromPlayer`が`rolePlayer`に対してキルボタンを押した時に呼ばれます。`true`を返すとキルされます

- **`IInvincible`**
  - `IsValidKillFromSource(byte source)` : `source`からキルの対象になるかどうかの判定
  - `IsValidAbilitySource(byte source)` : `source`から能力の対象になるかどうかの判定
  - `IsBlockKillFrom(byte? fromTarget)`: `fromTarget`からのキルをブロックするかどうかを判定します。

- **`IExiledAnimationOverrideWhenExiled`**
  - `OverrideInfo`: 追放時のメッセージやアニメーションを上書きします。

- **`IVoteCheck`**
  - `VoteTo(byte target)`: `target`へ投票した時のフック

### ビジュアル (IVisual) 向け
役職のビジュアル向けのロジックを担う `Visual` に実装することされるインターフェースです。
- **`IVisual`**
  - `UpdateVisual(PlayerControl rolePlayer)`: プレイヤーの見た目（色やエフェクト）を更新します。
- **`ILookedTag`**
  - 他のプレイヤーから見た時のタグを制御します。
