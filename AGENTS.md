# AGENTS.md

## 応答・コミュニケーション規則
- **敬語・前置き・挨拶の禁止**: 簡潔な事実報告と選択肢・確認の提示のみを行うこと。
- **確認・質問のフォーマット**: 不確定要素があり確認が必要な場合は、状況説明や丁寧語を使わず、以下の形式で簡潔に提示すること（詳細な状況説明はユーザーから要求された場合のみ行う）。
  - **パターンA（選択肢）**:
    ```text
    「{対象項目}」の実装方針:
      - A: {案A}
      - B: {案B}
      - C: {案C}
    ```
  - **パターンB（実行確認）**:
    `[CONFIRM] {理由}のため {処理} を実行します。(Y/n)`
- **応答例（BAD / GOOD）**:
  - ✕ BAD: 「ご指摘ありがとうございます。こちらの処理につきまして、〇〇の懸念がございますが、〜〜のように実装してもよろしいでしょうか？」
  - ◯ GOOD: 「[CONFIRM] Loggerが未定義のため Console.WriteLine を使用します。(Y/n)」
  - ◯ GOOD:
    ```text
    「生成処理」の実装方針:
      - A: ファクトリクラスを作成 ※保守性高
      - B: 既存クラス内にファクトリメソッドを追加 ※最小変更
    ```

## 開発ルール
1. **テストおよびビルド確認**:
   - 必ず `run_tests` スクリプト (`run_tests.sh` / `run_tests.ps1`) を使用して確認すること (`dotnet` コマンド単体での確認は不可)。
2. **テストコード実装方針**:
   - 変更を加えた箇所には必ず意味のあるユニットテストを実装すること。
   - **「無意味なテスト」の絶対禁止（アンチパターン）**:
     - 単にカバレッジ数値（ブランチカバレッジ等）を稼ぐためだけに作られたテスト。
     - 入力に対して固定値を返し失敗が存在し得ないテスト。
     - 単にプロパティ初期化や例外が発生しないことだけを確認するような無意味なコンストラクタ単体のテスト。
     - ログメッセージのヘッダやプレフィックスなどの整形テキストを含めてAssertの検証対象とするテスト。
   - **意味のあるテストの条件**:
     - テストケースごとに「何を検証する問いなのか」「成功によって何が保証されるのか」が明確であること。
     - 入力や内部状態の変化によって結果が変動し、バグが存在すれば明確に失敗するテストであること。
   - テスト対象は、循環的複雑度が2以上、またはMoq対象オブジェクトが3つ以上の処理に対して行うこと。
   - テストは原則として (A)AA(A) パターンに従って記述すること (Arrange, Act, Assert, Annihilate)。
   - カバレッジ網羅（ブランチカバレッジ）は「意味のあるテスト」の範囲内で目指すこと。無意味なテストを増やしてまでカバレッジ数値を追求することは禁止する。
   - Among Us等のモックコードで発生する `NotImplementedException` や `NullReferenceException` を `Assert.Throws<NotImplementedException>` や `Assert.Throws<NullReferenceException>` で放置することを禁止する。Moq を用いて依存オブジェクトをモック化すること。
   - **インスタンス生成方針**:
     - 原則としてクラスは `new` により直接インスタンス化すること。
     - `new` で直接インスタンス化できない場合は `Moq` を使用してモック化すること。
     - `Moq` でも対応が困難な場合は作業を停止
     - `ServiceCollection` や `ServiceProvider` をモック化することは禁止（論外）。
3. **プロジェクト構成ファイルの変更禁止**:
   - `.sln` ファイルおよび `.csproj` ファイルの変更・編集は固く禁じる。
4. **最小変更とシンプルさ**:
   - 既存の概念・モジュール・実装を可能な限り再利用し、最小限の変更で解決すること。
   - ストレートに読める最もシンプルな実装を行うこと。
5. **コミット・ブランチ運用**:
   - 1作業ごとに必ずコミットを行う。
   - **ブランチ命名規則**:
     - 機能・役職等の追加: `feat/{機能名}`
     - バグ修正: `fix/{修復名}`
     - リファクタリング: `refactor/{リファクタ名}`
   - **コミットメッセージ命名規則**:
     - 機能追加: `feat: {詳細}`
     - リファクタリング: `refactor: {詳細}`
     - 修正: `fix: {詳細}`
     - 既存処理変更: `change: {詳細}`

## コードスタイルガイドライン

### 基本原則
- **可読性第一**: 単純でストレートに読めるコードを記述する。
- **Allmanスタイルの厳守**: 開き波括弧 `{` は必ず改行して配置する。
- **ワンライナー制御構文の絶対禁止**: `if` 文や `for` 文、`foreach` 文を1行（ワンライナー）で記述することを厳しく禁止する。処理が1行であっても必ず波括弧を用いて改行すること。

#### ワンライナーIf/For禁止コード例
```csharp
// ✕ BAD: ワンライナーif/forおよびAllmanスタイル不遵守の禁止
if (condition) return;
if (a is null) { DoSomething(); }
for (int i = 0; i < count; i++) Process(i);

// ◯ GOOD: 常に改行しAllmanスタイルで記述
if (condition)
{
    return;
}

if (a is not null)
{
    DoSomething();
}

for (int i = 0; i < count; i++)
{
    Process(i);
}
```

### C# コーディング規則

#### アクセサビリティ & 変数
- 変数スコープは可能な限り狭く保持する（`private` ファースト）。
- 不必要な再代入を防ぐため `readonly` や Get 専用プロパティを積極的に活用する (`private readonly`, `public Git Git { get; }`)。
- コレクションの公開や受け渡しには `IReadOnlyList<T>` や `IReadOnlyDictionary<TKey, TValue>` を使用する。

#### クラス設計
- クラスは原則 `sealed` を付与し、継承よりも委譲（Composition）を優先検討する。
- インスタンス変数を保持しないクラスは `static` クラス化する。

#### 型 & Null許容性
- 組み込み型（`int`, `string` 等）は明示的な型名を使用し、`var` は右辺から型が確実に判明する場合のみ使用する。
- `#nullable enable` を使用し、Nullの可能性がある変数にはNullable修飾子 `?` を付与して必ずNullチェックを行う。

#### Unity特有のNullチェックルール
- Unityの `UnityEngine.Object` 継承クラスは、算術比較演算子 (`!= null`, `== null`) を用いてNullチェックを行う。
- Null条件演算子 (`?.`)、Null結合演算子 (`??`)、パターンマッチング (`is null`, `is not null`, `is Object`) によるUnityオブジェクトのNullチェックは正常に動作しないため絶対に行わないこと。

```csharp
// ✕ BAD: Unityオブジェクトへの is / ?. / ?? / is null の使用
if (mono is not null) { }
if (mono is null) { }
mono?.DoSomething();
var obj = mono ?? fallback;

// ◯ GOOD: 算術比較演算子を使用
if (mono != null)
{
    mono.DoSomething();
}
```

#### 命名規則
- **クラス / レコード / 構造体 / 列挙型**: PascalCase (`UserManager`, `PaymentProcessor`)
- **インターフェース**: `I` から始まるPascalCase (`IRole`, `IMeetingHud`)
- **public / protected メンバー（プロパティ / メソッド / フィールド）**: PascalCase (`RoleManager`, `CalculateTotal()`)
- **private メンバー（フィールド / メソッド）**: camelCase (`gameResult`, `computeResult()`)

#### 名前空間 & usingディレクティブ
- 名前空間の宣言はファイルスコープ名前空間を使用する (`namespace MyMod.Collection;`)。
- `using` ディレクティブは以下の順序でアルファベット順にソートして記述する:
  1. .NET 標準ライブラリ (`System.*`)
  2. 外部ライブラリ / DLL (`InnerNet` 等)
  3. 自身のライブラリ / Modモジュール (`ExtremeRoles.*`)
- エイリアス定義は `using` と `namespace` 宣言の間に配置する。
