# AGENTS.md

## 応答・コミュニケーション規則
- **無駄に謙遜した文言の禁止**: 「ご指摘ありがとうございます」「ご要望があるか確認できますでしょうか？」「恐れ入りますが」などの過度な挨拶、クッション言葉、不必要な状況確認・謙遜表現は一切使用禁止。
- **簡潔かつプロフェッショナルな記述**: 結論、確認事項、進捗報告のみを直接的かつ簡潔な日本語で伝えること。

## 開発ルール
1. **テストおよびビルド確認**:
   - 必ず `run_tests` スクリプト (`run_tests.sh` / `run_tests.ps1`) を使用して確認すること (`dotnet` コマンド単体での確認は不可)。
2. **テストコード実装方針**:
   - 変更を加えた箇所には必ず意味のあるユニットテストを実装すること（固定値検証のみなどの形式的なテストは不可）。
   - Among Us 側の削除されたコードで発生する `NotImplementedException` を `Assert.Throws<NotImplementedException>` で放置することを禁止する。Moq を用いて依存オブジェクトをモック化すること。
3. **プロジェクト構成ファイルの変更禁止**:
   - `.sln` ファイルおよび `.csproj` ファイルの変更・編集は固く禁じる。
4. **最小変更とシンプルさ**:
   - 既存の概念・モジュール・実装を可能な限り再利用し、最小限の変更で解決すること。
   - ストレートに読める最もシンプルな実装を行うこと。
5. **コミット・ブランチ運用**:
   - 1作業ごとに必ずコミットを行う。
   - **ブランチ命名規則**:
     - 機能追加: `feature/{機能名}`
     - 機能以外の追加（役職等）: `feat/{機能名}`
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
- Null条件演算子 (`?.`) や `is` / `is not` 演算子によるUnityオブジェクトのNullチェックは動作しないため絶対に行わないこと。

```csharp
// ✕ BAD: Unityオブジェクトへの is / ?. の使用
if (mono is not null) { }
mono?.DoSomething();

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
