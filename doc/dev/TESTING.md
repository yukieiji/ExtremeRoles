# テストドキュメントおよびガイドライン

## 1. テストおよびビルド確認
- 必ず `run_tests` スクリプト (`run_tests.sh` / `run_tests.ps1`) を使用して確認すること (`dotnet` コマンド単体での確認は不可)。

## 2. テストコード実装方針
- 変更を加えた箇所には必ず意味のあるユニットテストを1クラス1テストクラスファイルで実装すること。

### カバレッジ網羅
- **全メソッド・全ブランチの網羅**:
  - 原則として、すべてのメソッドにおいてすべての分岐（全ブランチレート / ブランチカバレッジ）を網羅するテストを実装すること。

### Act と Assert の分離および意味のある検証
- **Act と Assert を同時に行わない（完全分離）**:
  - Act（テスト対象の実行）と Assert（検証）を同一行や同一のメソッド呼び出し内で行うことを厳しく禁止する。
  - 必ず Arrange, Act, Assert の各フェーズを明確に分けて記述すること。
- **意味のある Assert の検証**:
  - Assert では単に例外が発生しないことや処理が完了したことの確認ではなく、内部状態の変更や戻り値に対する意味のある検証を行うこと。

#### Act / Assert 分離のコード例

```csharp
// ✕ BAD: ActとAssertを同時に実行している
Assert.That(calculator.Add(1, 2), Is.EqualTo(3));
Assert.IsTrue(userService.IsValidUser(user));

// ◯ GOOD: ActとAssertを明確に分離して記述している
// Act
var result = calculator.Add(1, 2);

// Assert
Assert.That(result, Is.EqualTo(3));

// ◯ GOOD: Boolean検証の場合も分離
// Act
var isValid = userService.IsValidUser(user);

// Assert
Assert.That(isValid, Is.True);
```

### 「無意味なテスト」の絶対禁止（アンチパターン）
- 単にカバレッジ数値（ブランチカバレッジ等）を稼ぐためだけに作られたテスト。
- Assert が存在せず、ただ単にメソッドを実行するだけのテスト。
- コンストラクタを呼び出して初期化されたプロパティをチェックするだけのテスト（単にプロパティ初期化や例外が発生しないことだけを確認するような無意味なコンストラクタ呼び出しや new をするだけのテスト）。
- 入力に対して固定値を返し失敗が存在し得ないテスト。
- ログメッセージのヘッダやプレフィックスなどの整形テキストを含めて Assert の検証対象とするテスト。

### 意味のあるテストの条件
- テストケースごとに「それが何を検証/保証/チェックするのか」と「その成功/失敗によって何が保証され/されないのか」が明確であること。
- 入力や内部状態の変化によって結果が変動し、バグが存在すれば明確に失敗するテストであること。
- テスト対象は、循環的複雑度が2以上、または Moq 対象オブジェクトが3つ以上の処理に対して行うこと。
- テストは原則として (A)AA(A) パターンに従って記述すること (Arrange, Act, Assert, Annihilate)。

### 例外ハンドリングとモック
- Among Us 等のモックコードで発生する `NotImplementedException` や `NullReferenceException` を `Assert.Throws<NotImplementedException>` や `Assert.Throws<NullReferenceException>` で放置することを禁止する。Moq を用いて依存オブジェクトをモック化すること。

### インスタンス生成方針
- 原則としてクラスは `new` により直接インスタンス化すること。
- `new` で直接インスタンス化できない場合は `Moq` を使用してモック化すること。
- `Moq` でも対応が困難な場合は作業を停止すること。
- `ServiceCollection` や `ServiceProvider` をモック化することは禁止（論外）。
