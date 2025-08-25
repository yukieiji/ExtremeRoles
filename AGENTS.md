# AGENTS.md

## 開発時のルール
 - あなたの環境下ではビルドできませんので、ユーザーの指示があるまでビルドは決して実行してはいけません
    - テストも実行できません
    - そのためslnファイル及びプロジェクトファイルの変更を固く禁じます
 - 日本語で返信をすること
 - コードはコードスタイルに従うこと
    - コードスタイルはAllmanスタイルです
    - 特にワンライナーifやfor文は使用禁止です
 - ユーザーの求める課題に対して最小限の変更かつ最もシンプルな実装を行うこと
    - これは著書「ルールズオブプログラミング」や「ロバストPython」でも述べられ重視されている内容である
        - **最小限の変更** : なるべく既存の概念/モジュール/コード/実装を使用するような変更
        - **最もシンプルな実装** : コードレビュー等を行ったときにストレートに読むことが出来るシンプルなコード
 - 1作業ごとに必ずコミットを行うこと
 - ブランチ名は以下のルールに従うこと
    - **機能追加** : feature/{機能名}
    - **リファクタリング** : refactor/{リファクタ名}
 - コミット名は以下のルールに従うこと
    - **機能追加** : feat: {コミット詳細}
    - **リファクタリング** : refactor: {コミット詳細}
    - **修正** : fix: {コミット詳細}
    - **既存処理変更** : change: {コミット詳細}

## コードスタイル

### 概要

これはExtremeRoles(ExR)のC#コード開発におけるスタイルガイドラインです。
基本的にMicrosoftの.NETのスタイル/コーディングガイドラインに沿って開発されていますが、UnityとMod開発のためいくつかの点で変更が加えられています

### 原則
* **可読性:** コードは簡潔かつわかりやすくすべき
* **柔軟性:** コードは変更が容易い、柔軟性が高い状態を保つべき
* **パフォーマンス:** コードは可読性と柔軟性は維持しつつ、最も効率の高い実装がされるべきである

### 変更箇所

#### アクセサビリティ
* **privateファースト:** 変数のスコープはなるべく狭く、インスタンス変数等は公開する必要がない場合はprivateにする
* **readonly/プロパティを多様:** 不必要な書き換えや代入が発生しないようにする
    * クラス変数はなるべくreadonly変数やGetプロパティにする `private readonly` `public Git git { get; }` 
    * コレクションの受け渡しや変数は可能な限りreadonlyのインターフェースを使用する `IReadOnlyList<Net>` `IReadOnlyDictionary<string, string>`

#### クラス
* **継承をより委譲** 基本的にクラスはsealedをつけてシールし、継承を考える前に委譲できないかを考える
* **不必要なnew** 不必要なnewを防ぐため、インスタンス変数が無いクラスstaticをつけなるべくstaticクラス化する

#### 型
* **var:** 組み込み型は型を使用し、varは型が確実にわかる時に使用する

#### Null
* **Null許容型(Nullable):** Nullの可能性のあるコードはNullableを使用し、Nullチェックを必ず行う
    
    ```csharp
    #nullable enable

    public sealed class MyClassA
    {
        public void MyMethod(MyClassB b) // bはNullではないことを保証 
        {
        }
    }


    public sealed class MyClassB
    {
        public void MyMethod(MyClassA? a) // aはNullの可能性がある
        {
            if (a is not null) // nullチェックは必ずおこなう
            {
                // 処理
            }
        }

        // 必要に応じてNotNullWhen等も使う
        public bool NullCheck([NotNullWhen(true)] out MyClassA? a) // このメソッドがTrueを返す時、aはNullではない
        {
        }
    }

    ```


#### Unity
* **パフォーマンス:** UnityのゲームをMODするが故にパフォーマンスは常に追い求めるべきである
* **可読性とパフォーマンスのトレードオフ:** 可読性とパフォーマンスがトレードオフの場合、可読性を高めることを重要視する
* **Unity独自のNullチェック:** UnityのクラスはNullチェックがオーバライドされ独自実装されている
    * Unityのクラスもしくは継承されたクラスのNullチェックは算術演算子を用いてチェックを行う
        * Null条件演算子やisによる比較は絶対に行わない
    * Unityのクラスもしくは継承されたクラスの変数のみのは使用しない
    ```csharp
    public sealed class MyMono : MonoBehavior
    {
    }

    public sealed class MyClassC
    {
        public void MyMethod(MyMono? mono)
        {
            if (mono != null)
            {
                // 処理
            }
        }
    }
    ```

#### 命名規則
* **変数/定数**
    * **public/protected:** 　パスカルケース: `RoleManager`, `BoolOption`
    * **private:** 　キャメルケース: `gameResult`, `userData`
* **メソッド/関数**
    * **public/protected:** 　パスカルケース: `CalculateTotal()`, `ProcessData()`
    * **private:** 　キャメルケース: `computeResult()`, `resolveData()`
* **クラス/レコード/構造体:** 　パスカルケース: `UserManager`, `PaymentProcessor`
* **インターフェース:** Iから始まるパスカルケース: `IRole`, `IMeetingHud`

#### 名前空間
* **usingディレクティブ:** 以下の順番で宣言し、その中でアルファベット順にソートする 
    * .NET標準
    * 外部ライブラリ/DLL
    * 自身のライブラリ/DLL
* **エイリアス:** 必要に応じてエイリアスをつけ、usingディレクティブと名前空間の宣言の間に追加する
* **名前空間の宣言:** ファイルスコープ名前空間を使用する

    ```csharp
    using System.Collections.Generic;
    using System.Linq;

    using InnerNet;

    using MyMod.Module;

    using Heep = MyMod.Module.Heep;

    namespace MyMod.Collection;
    
    // コード

    ```

### コードスタイル例

```csharp
using System.Collections.Generic;

using SemanticVersioning;

using SupportedLangs = MyLib.Translation.SupportedLangs;

namespace MyLib.Beta;

#nullable enable

public sealed class BetaContentAdder(string version)
{
    public const string NewTransDataPath = "ExtremeRoles.Beta.Resources.JsonData.TextRevamp.json";

	private readonly Version version = new Version(version);

	private const string transKey = "PublicBetaContent";

	public void AddContentText(
		SupportedLangs curLang,
		Dictionary<string, string> transData)
	{
		string content = curLang switch
		{
			SupportedLangs.Japanese =>
				"・役職説明のテキストの改善/変更\n・フィードバックシステムの追加\n・「挙手する」ボタンをトグル式に変更",
			_ => "",
		};
		transData.Add(transKey, content);

		// フィードバックを送るよう
		transData.Add("SendFeedBackToExR", curLang switch
		{
			SupportedLangs.Japanese => "フィードバックを送る",
			_ => "",
		});
	}
}

```