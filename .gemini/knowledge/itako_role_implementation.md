# イタコ (Itako) 役職の実装とテストでの注意点

## 1. 役職概要
クルー陣営の副役職追加可能役職 (`MultiAssignRoleBase`)。
死体に対してボタン能力「口寄せ」を使用し、詠唱時間完了後に役職を引き継ぐ。

## 2. 実装上のポイント

### 2.1 口寄せの能力ボタン (`CreateActivatingAbilityCountButton`)
- `isReduceOnActive: false` を指定することで、能力発動時（詠唱開始時）には使用回数を消費せず、詠唱完了時 (`CleanUp`) に使用回数を減らすことができる。
- これにより、死体の移動・消滅・範囲外移動等で途中でキャンセルされた場合 (`ForceCleanUp`) でも手動での回数返却処理 (`SetAbilityCount`) が不要になり、安全に使用回数が保護される。

### 2.2 役職の引き継ぎ (`ExtractInheritedRole` / `InheritTargetRole`)
- 対象がコンビネーション役職 (`Buddy` 等) や副役職を持つ場合、`ExtractInheritedRole` でコンビネーション役職のみをクローンして引き継ぐ。
- 既に別の副役職を引き継いでいるイタコが新たに口寄せを行った場合、既存の副役職の `IRoleSpecialReset.AllReset` および会議リセット処理を行ってから、新しい役職を `ExtremeRoleManager.SetNewAnothorRole` で上書き設定する。

### 2.3 クルー陣営以外の発動時の自爆処理
- 口寄せ発動者または口寄せ対象の死体がクルー陣営以外の場合、`Player.RpcUncheckMurderPlayer` で発動者自身を自殺処理し、発動者と対象の両方の死体を `Player.RpcCleanDeadBody` で消去する。

## 3. ユニットテスト (`ItakoRoleTests`) での注意点

### 3.1 既存コードの変更禁止ルール
- タスク指定で既存共通コード (`HudManagerExtension.cs` 等) の変更が禁止されている場合、テスト環境で `HudManager.Instance.UseButton` が `null` であっても呼び出し側 (`ItakoRole.cs`) で null チェック (`HudManager.InstanceExists && HudManager.Instance.UseButton != null`) を行う。

### 3.2 `Player.GetDeadBodyInfo` / `Physics2D` のモック化
- `Player.GetDeadBodyInfo` をテストコードで呼ぶ場合、内部で `Constants.PlayersOnlyMask` および `Physics2D.OverlapCircleAll` が参照される。
- `MockConstantsget_PlayersOnlyMaskHelper.Instance` および `UnityEngine.MockPhysics2DOverlapCircleAllHelper.Instance` のモックをセットアップすることで、`NullReferenceException` を回避できる。
