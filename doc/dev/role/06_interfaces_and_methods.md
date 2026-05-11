# 重要なメソッドとインターフェース

役職作成において頻繁に使用する `SingleRoleBase` のメソッドと、機能拡張のためのインターフェースについて説明します。

## SingleRoleBase の主要メソッド

Roleクラスでオーバーライドして使用する主なメソッドです。

- **`RoleSpecificInit()`**
  - 役職がプレイヤーにアサインされた直後に一度だけ呼ばれます。`AbilityHandler` や `StatusModel` の生成・初期化、オプション値のロードに最適です。
- **`ExiledAction(PlayerControl rolePlayer)`**
  - 自身が追放された時に呼ばれます。
- **`GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId)`**
  - プレイヤーのネームタグ（頭上の名前）をカスタマイズしたい場合にオーバーライドします。
- **`GetFullDescription()`**
  - 役職説明画面のテキストを動的に変更したい場合にオーバーライドします（例：現在のカウントを表示するなど）。

## 機能拡張インターフェース

特定のインターフェースを実装することで、様々なゲームイベントにフックできます。**`IRole` から始まる名前のインターフェースは、主に `SingleRoleBase` (Roleクラス) に実装することを目的としています。**

### Roleクラス (SingleRoleBase) 向け
これらのインターフェースは通常、役職の定義である Role クラスに実装します。

- **`IRoleUpdate`**
  - `Update(PlayerControl rolePlayer)`: 毎フレーム呼ばれます。
- **`IRoleResetMeeting`**
  - `ResetOnMeetingStart()`: 会議開始時に呼ばれます。
  - `ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer)`: 会議終了時に呼ばれます。

### 能力・ロジック (AbilityHandler) 向け
能動的な能力や詳細なロジックを担う `AbilityHandler` に実装することが推奨されるインターフェースです。

- **`IInvincible`**
  - `IsBlockKillFrom(byte? fromTarget)`: キルをブロックするかどうかを判定します。
- **`IExiledAnimationOverrideWhenExiled`**
  - `OverrideInfo`: 追放時のメッセージやアニメーションを上書きします。
- **`IKilledFrom`**
  - `IKilledFrom(PlayerControl killer, PlayerControl victim)`: キルされた瞬間に呼ばれます。

### 投票・会議
- **`IRoleVoteModifier`**
  - `ModifiedVote(...)`: 投票結果を書き換えます。
  - `GetModdedVoteInfo(...)`: 投票の演出を書き換えます。
- **`IRoleMeetingButtonAbility`**
  - 会議中の投票画面に特殊なボタンを追加したい場合に実装します。

### フック・割り込み
- **`IRoleReportHook`**
  - `HookReportButton(...)`: 緊急会議ボタンが押された時に割り込みます。
  - `HookBodyReport(...)`: 死体レポート時に割り込みます。
- **`IRolePerformKillHook`**
  - キルを実行する瞬間に割り込みます。
- **`IRoleMurderPlayerHook`**
  - プレイヤーが殺害される処理に割り込みます。

### 視覚効果
- **`IVisual`**
  - `UpdateVisual(PlayerControl rolePlayer)`: プレイヤーの見た目（色やエフェクト）を更新します。
- **`ILookedTag`**
  - 他のプレイヤーから見た時のタグを制御します。

## 実装のポイント

これらのインターフェースは、Roleクラス自体に実装することも可能ですが、ロジックの肥大化を防ぐために **AbilityHandler に実装し、Roleクラスから呼び出す** か、**Roleクラスが保持する AbilityClass にそのインスタンスを割り当てる** 設計が推奨されます。
