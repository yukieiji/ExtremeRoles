# 主要なメソッドとプロパティ

`SingleRoleBase` を継承して役職を作成する際、オーバーライドして挙動をカスタマイズできる主要なメソッドとプロパティについて説明します。

---

## ライフサイクル・初期化

#### `void RoleSpecificInit()`
役職がプレイヤーにアサインされた直後に一度だけ呼ばれます。
- **用途**: `AbilityHandler` や `StatusModel` の生成、オプション値のロード。
- **実装例**:
  ```csharp
  protected override void RoleSpecificInit() {
      this.status = new MyRoleStatusModel();
      this.ability = new MyRoleAbilityHandler(this.status);
  }
  ```

#### `SingleRoleBase Clone()`
役職インスタンスの複製を作成します。
- **戻り値**: 複製された `SingleRoleBase` インスタンス。
- **注意**: 通常は `MemberwiseClone()` を使用しますが、深いコピーが必要な場合はオーバーライドします。

---

## 勝利条件・判定

#### `bool IsTeamsWin()`
自身のチームが勝利したかどうかを判定します。
- **戻り値**: チームが勝利条件を満たしていれば `true`。

#### `bool IsSameTeam(SingleRoleBase targetRole)`
対象の役職と同じチームかどうかを判定します。
- `targetRole`: 比較対象の役職。
- **戻り値**: 同じチームであれば `true`。

#### `bool IsAssignGhostRole`
幽霊状態のプレイヤーにこの役職をアサイン可能かどうか。
- **デフォルト**: `true`

---

## アクション・イベント

#### `void ExiledAction(PlayerControl rolePlayer)`
自身が会議で追放された時に呼ばれます。
- `rolePlayer`: 追放されたプレイヤー（自身）。

#### `void RolePlayerKilledAction(PlayerControl rolePlayer, PlayerControl killerPlayer)`
自身が殺害された時に呼ばれます。
- `rolePlayer`: 殺害されたプレイヤー（自身）。
- `killerPlayer`: 加害者のプレイヤー。

---

## ビジュアル・情報表示

#### `string RoleName`
役職の表示名を返します。
- **用途**: 翻訳キーを指定することが一般的です。

#### `Color GetNameColor(bool isTruthColor = false)`
プレイヤー名の色を取得します。
- `isTruthColor`: 真実の色を要求されているか（偽装を考慮するか）。
- **戻り値**: 表示する `Color`。

#### `Color GetTargetRoleSeeColor(SingleRoleBase targetRole, byte targetPlayerId)`
特定のプレイヤー（`targetRole`）から見た時の、自身（`targetPlayerId`）の名前の色を決定します。
- `targetRole`: 閲覧者の役職。
- `targetPlayerId`: 自身のプレイヤーID。
- **用途**: インポスター同士で名前を赤く見せる、特定の役職からのみ色を変える等。

#### `string GetRoleTag()`
名前の横に表示される短縮タグ（例：`[M]`）を返します。
- **戻り値**: 表示する文字列。

#### `string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId)`
他プレイヤー（`targetRole`）から見た時の自身のネームタグ（頭上の表示）をカスタマイズします。
- **用途**: 役職名の表示や、特殊な状態の表示。

#### `string GetImportantText(bool isContainFakeTask = true)`
役職説明画面の「重要事項」セクションのテキストを生成します。
- `isContainFakeTask`: 偽タスクの案内を含めるか。
- **戻り値**: 装飾された文字列。

#### `string GetIntroDescription()`
ゲーム開始時のイントロ画面で表示される説明文を返します。

#### `string GetFullDescription()`
役職説明画面で表示される詳細な説明文を返します。
- **用途**: 現在の残り回数やカウントなどを動的に表示したい場合にオーバーライドします。

#### `bool IsBlockShowMeetingRoleInfo()` / `IsBlockShowPlayingRoleInfo()`
会議中やプレイ中に役職情報を表示するのを制限するかどうか。
- **戻り値**: `true` で非表示。

---

## オプション設定

#### `AutoParentSetOptionCategoryFactory CreateSpawnOption()` (abstract)
役職の出現率や人数のオプションを生成します。

#### `void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)` (abstract)
役職固有のオプション（例：クールタイム、使用回数など）を生成します。

#### `void CreateVisionOption(AutoParentSetOptionCategoryFactory factory, bool ignorePrefix = true)`
視界に関するオプションを生成します。

---

## 実装のポイント

`SingleRoleBase` は多くの `virtual` プロパティ（`CanUseAdmin`, `UseVent` など）を持っています。これらをコンストラクタや `RoleSpecificInit` で適切に設定することで、役職の基本的な能力を定義できます。

```csharp
public MyRole(RoleArgs arg) : base(arg) {
    this.CanKill = true;
    this.UseVent = true;
    this.HasTask = false;
}
```
