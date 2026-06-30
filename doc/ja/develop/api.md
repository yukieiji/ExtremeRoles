# API 仕様書

Extreme Roles の外部 API 仕様です。
基本となるホスト URL は `http://localhost:57700` です。

## 目次
- [Among Us バニラオプション関連](#among-us-バニラオプション関連)
  - [GET /au/option/](#get-auoption)
  - [PUT /au/option/](#put-auoption)
  - [GET /au/option/ui/](#get-auoptionui)
- [Extreme Roles カスタムオプション関連](#extreme-roles-カスタムオプション関連)
  - [GET /exr/option/](#get-exroption)
  - [PUT /exr/option/](#put-exroption)
  - [GET /exr/option/csv/](#get-exroptioncsv)
  - [POST /exr/option/csv/](#post-exroptioncsv)
- [翻訳関連](#翻訳関連)
  - [POST /au/translation/](#post-autranslation)
  - [POST /au/translation/batch/](#post-autranslationbatch)
  - [GET /au/translation/batch/optionunit/](#get-autranslationbatchoptionunit)
  - [GET /au/translation/batch/role/](#get-autranslationbatchrole)
- [役職割り当てフィルタ関連](#役職割り当てフィルタ関連)
  - [GET /exr/role/filter/](#get-exrrolefilter)
  - [POST /exr/role/filter/](#post-exrrolefilter)

---

## Among Us バニラオプション関連

### GET /au/option/
Among Us のバニラオプション（および組み込みの役職オプション）の一覧を取得します。

- GET : `http://localhost:57700/au/option/`

| ステータスコード | 説明 |
| --- | --- |
| 200 | 取得に成功 |
| 400 | ロビー外など、取得できない状況 |

- レスポンス (JSON 配列)
  - 各要素:
    - `TranslatedTitle` string : カテゴリ名
    - `Options` オブジェクト配列 : オプションのリスト
      - `TranslatedTitle` string : オプション名
      - `TranslatedFormat` string : 単位などのフォーマット
      - `Value` any : 現在の値。`ValueType` が `RoleBase` の場合は以下のオブジェクト
        - `MaxCount` int : 最大出現数
        - `Chance` int : 出現確率 (%)
      - `Info` オブジェクト : オプション識別情報
        - `ValueType` string : 値の型 (`Bool`, `Byte`, `Int`, `UInt`, `Float`, `RoleBase`)
        - `OptionName` int : オプションの内部 ID
      - `Range` any[]? : 選択可能な値の範囲。`ValueType` が `RoleBase` の場合は null

### PUT /au/option/
Among Us のバニラオプションを更新します。ホストのみ実行可能です。

- PUT : `http://localhost:57700/au/option/`

| ステータスコード | 説明 |
| --- | --- |
| 200 | 更新に成功 |
| 400 | ホストではない、または不正なリクエスト |

- パラメーター (Body JSON)
  - `ValueType` string 必須 : 値の型 (`Bool`, `Byte`, `Int`, `UInt`, `Float`, `RoleBase`)
  - `OptionName` int 必須 : オプションの内部 ID
  - `NewValue` any 必須 : 新しい値。`RoleBase` の場合は `{"MaxCount": int, "Chance": int}` の形式。

- レスポンス (JSON)
  - `UpdatedCategory` オブジェクト? : 更新されたカテゴリの情報
    - `Id` int : カテゴリ ID
    - `Name` string : カテゴリ名
    - `ColorCode` string? : カテゴリの色 (RGBA 16進数)
    - `Options` オブジェクト配列 : オプションのリスト
      - `Id` int : オプション ID
      - `IsActive` bool : 現在有効か
      - `TranslatedName` string : 翻訳されたオプション名
      - `Selection` int : 現在の選択値（インデックス）
      - `Format` string : フォーマット
      - `RangeMeta` オブジェクト : 値の範囲に関するメタ情報
        - `Type` string : 範囲の型
        - `Values` any[] : 選択可能な値の配列
      - `Childs` オブジェクト配列 : 子オプション（階層構造がある場合）
        - `Id` int : オプション ID
        - `IsActive` bool : 有効か
        - `TranslatedName` string : 翻訳名
        - `Selection` int : 選択インデックス
        - `Format` string : フォーマット
        - `RangeMeta` オブジェクト :
          - `Type` string : 範囲の型
          - `Values` any[] : 値の配列
        - `Childs` オブジェクト配列 : さらに下位の子オプション
  - `ChainUpdatedOption` オブジェクト配列 : 連動して更新されたオプションのリスト
    - `Id` int : カテゴリ ID
    - `Options` オブジェクト配列 : オプションのリスト
      - `Id` int : オプション ID
      - `IsActive` bool : 現在有効か
      - `TranslatedName` string : 翻訳されたオプション名
      - `Selection` int : 現在の選択値（インデックス）
      - `Format` string : フォーマット
      - `RangeMeta` オブジェクト : 値の範囲に関するメタ情報
        - `Type` string : 範囲の型
        - `Values` any[] : 選択可能な値の配列
      - `Childs` オブジェクト配列 : 子オプション
        - `Id` int : オプション ID
        - `IsActive` bool : 有効か
        - `TranslatedName` string : 翻訳名
        - `Selection` int : 選択インデックス
        - `Format` string : フォーマット
        - `RangeMeta` オブジェクト :
          - `Type` string : 範囲の型
          - `Values` any[] : 値の配列
        - `Childs` オブジェクト配列 : 下位の子オプション
  - `ChainUpdateCategory` オブジェクト? : 連動して更新されたカテゴリの情報
    - `Id` int : カテゴリ ID
    - `Name` string : カテゴリ名
    - `ColorCode` string? : カテゴリの色 (RGBA 16進数)
    - `Options` オブジェクト配列 : オプションのリスト
      - `Id` int : オプション ID
      - `IsActive` bool : 現在有効か
      - `TranslatedName` string : 翻訳されたオプション名
      - `Selection` int : 現在の選択値（インデックス）
      - `Format` string : フォーマット
      - `RangeMeta` オブジェクト : 値の範囲に関するメタ情報
        - `Type` string : 範囲の型
        - `Values` any[] : 選択可能な値の配列
      - `Childs` オブジェクト配列 : 子オプション
        - `Id` int : オプション ID
        - `IsActive` bool : 有効か
        - `TranslatedName` string : 翻訳名
        - `Selection` int : 選択インデックス
        - `Format` string : フォーマット
        - `RangeMeta` オブジェクト :
          - `Type` string : 範囲の型
          - `Values` any[] : 値の配列
        - `Childs` オブジェクト配列 : 下位の子オプション

### GET /au/option/ui/
オプション設定用の Web UI (HTML) を返します。

- GET : `http://localhost:57700/au/option/ui/`

| ステータスコード | 説明 |
| --- | --- |
| 200 | 成功 |
| 404 | リソースが見つからない |

---

## Extreme Roles カスタムオプション関連

### GET /exr/option/
Extreme Roles で追加されたカスタムオプションの一覧を取得します。

- GET : `http://localhost:57700/exr/option/`

| ステータスコード | 説明 |
| --- | --- |
| 200 | 取得に成功 |

- レスポンス (JSON 配列)
  - 各要素:
    - `Id` string : タブ ID
    - `Name` string : タブ名
    - `Categories` オブジェクト配列
      - `Id` int : カテゴリ ID
      - `Name` string : カテゴリ名
      - `ColorCode` string? : カテゴリの色 (RGBA 16進数)
      - `Options` オブジェクト配列
        - `Id` int : オプション ID
        - `IsActive` bool : 現在有効（表示されるべき）か
        - `TranslatedName` string : 翻訳されたオプション名
        - `Selection` int : 現在の選択値（インデックス）
        - `Format` string : フォーマット
        - `RangeMeta` オブジェクト : 値の範囲に関するメタ情報
          - `Type` string : 範囲の型
          - `Values` any[] : 選択可能な値の配列
        - `Childs` オブジェクト配列 : 子オプション
          - `Id` int : オプション ID
          - `IsActive` bool : 有効か
          - `TranslatedName` string : 翻訳名
          - `Selection` int : 選択インデックス
          - `Format` string : フォーマット
          - `RangeMeta` オブジェクト :
            - `Type` string : 範囲の型
            - `Values` any[] : 値の配列
          - `Childs` オブジェクト配列 : 下位の子オプション

### PUT /exr/option/
Extreme Roles のカスタムオプションを更新します。ホストのみ実行可能です。

- PUT : `http://localhost:57700/exr/option/`

| ステータスコード | 説明 |
| --- | --- |
| 200 | 更新に成功 |
| 202 | 更新に成功（プリセット等、全更新が必要な場合。再取得を推奨） |
| 400 | ホストではない、または不正なリクエスト |

- パラメーター (Body JSON)
  - `TabId` int 必須 : タブ ID
  - `CategoryId` int 必須 : カテゴリ ID
  - `OptionId` int 必須 : オプション ID
  - `Selection` int 必須 : 選択する値のインデックス

- レスポンス (JSON)
  - `UpdatedCategory` オブジェクト? : 更新されたカテゴリの情報
    - `Id` int : カテゴリ ID
    - `Name` string : カテゴリ名
    - `ColorCode` string? : カテゴリの色 (RGBA 16進数)
    - `Options` オブジェクト配列 : オプションのリスト
      - `Id` int : オプション ID
      - `IsActive` bool : 現在有効か
      - `TranslatedName` string : 翻訳されたオプション名
      - `Selection` int : 現在の選択値（インデックス）
      - `Format` string : フォーマット
      - `RangeMeta` オブジェクト : 値の範囲に関するメタ情報
        - `Type` string : 範囲の型
        - `Values` any[] : 選択可能な値の配列
      - `Childs` オブジェクト配列 : 子オプション
        - `Id` int : オプション ID
        - `IsActive` bool : 有効か
        - `TranslatedName` string : 翻訳名
        - `Selection` int : 選択インデックス
        - `Format` string : フォーマット
        - `RangeMeta` オブジェクト :
          - `Type` string : 範囲の型
          - `Values` any[] : 値の配列
        - `Childs` オブジェクト配列 : 下位の子オプション
  - `ChainUpdatedOption` オブジェクト配列 : 連動して更新されたオプションのリスト
    - `Id` int : カテゴリ ID
    - `Options` オブジェクト配列 : オプションのリスト
      - `Id` int : オプション ID
      - `IsActive` bool : 現在有効か
      - `TranslatedName` string : 翻訳されたオプション名
      - `Selection` int : 現在の選択値（インデックス）
      - `Format` string : フォーマット
      - `RangeMeta` オブジェクト : 値の範囲に関するメタ情報
        - `Type` string : 範囲の型
        - `Values` any[] : 選択可能な値の配列
      - `Childs` オブジェクト配列 : 子オプション
        - `Id` int : オプション ID
        - `IsActive` bool : 有効か
        - `TranslatedName` string : 翻訳名
        - `Selection` int : 選択インデックス
        - `Format` string : フォーマット
        - `RangeMeta` オブジェクト :
          - `Type` string : 範囲の型
          - `Values` any[] : 値の配列
        - `Childs` オブジェクト配列 : 下位の子オプション
  - `ChainUpdateCategory` オブジェクト? : 連動して更新されたカテゴリの情報
    - `Id` int : カテゴリ ID
    - `Name` string : カテゴリ名
    - `ColorCode` string? : カテゴリの色 (RGBA 16進数)
    - `Options` オブジェクト配列 : オプションのリスト
      - `Id` int : オプション ID
      - `IsActive` bool : 現在有効か
      - `TranslatedName` string : 翻訳されたオプション名
      - `Selection` int : 現在の選択値（インデックス）
      - `Format` string : フォーマット
      - `RangeMeta` オブジェクト : 値の範囲に関するメタ情報
        - `Type` string : 範囲の型
        - `Values` any[] : 選択可能な値の配列
      - `Childs` オブジェクト配列 : 子オプション
        - `Id` int : オプション ID
        - `IsActive` bool : 有効か
        - `TranslatedName` string : 翻訳名
        - `Selection` int : 選択インデックス
        - `Format` string : フォーマット
        - `RangeMeta` オブジェクト :
          - `Type` string : 範囲の型
          - `Values` any[] : 値の配列
        - `Childs` オブジェクト配列 : 下位の子オプション

### GET /exr/option/csv/
現在のオプション設定を CSV 形式でエクスポートします。ホストのみ実行可能です。

- GET : `http://localhost:57700/exr/option/csv/`

| ステータスコード | 説明 |
| --- | --- |
| 200 | 成功 |
| 400 | ホストではない |

- レスポンス (JSON)
  - `ExportAt` string : エクスポート日時
  - `Version` string : MOD バージョン
  - `CsvBody` string : CSV 文字列

### POST /exr/option/csv/
CSV 形式のオプション設定をインポートして適用します。ホストのみ実行可能です。

- POST : `http://localhost:57700/exr/option/csv/`

| ステータスコード | 説明 |
| --- | --- |
| 200 | 成功 |
| 400 | 不正な CSV 形式、またはホストではない |

- パラメーター (Body JSON)
  - `CsvBody` string 必須 : インポートする CSV 文字列

---

## 翻訳関連

### POST /au/translation/
指定されたキーの翻訳文字列を取得します。

- POST : `http://localhost:57700/au/translation/`

| ステータスコード | 説明 |
| --- | --- |
| 200 | 成功 |
| 400 | 不正なリクエスト |

- パラメーター (Body JSON)
  - `Key` any 必須 : 翻訳キー（バニラの StringNames 数値、またはカスタム翻訳文字列キー）
  - `Param` any[]? : 埋め込みパラメータのリスト

- レスポンス (JSON)
  - `Key` any : リクエストされたキー（数値または文字列）
  - `Param` any[] : 翻訳に使用されたパラメータの配列
  - `Result` string : 翻訳・フォーマットされた結果の文字列

### POST /au/translation/batch/
複数の翻訳文字列を一括で取得します。

- POST : `http://localhost:57700/au/translation/batch/`

| ステータスコード | 説明 |
| --- | --- |
| 200 | 成功 |
| 400 | 不正なリクエスト |

- パラメーター (Body JSON 配列)
  - 各要素:
    - `Key` any 必須 : 翻訳キー
    - `Param` any[]? : 埋め込みパラメータのリスト

- レスポンス (JSON 配列)
  - 各要素:
    - `Key` any : リクエストされたキー
    - `Param` any[] : 翻訳に使用されたパラメータの配列
    - `Result` string : 翻訳・フォーマットされた結果の文字列

### GET /au/translation/batch/optionunit/
オプションの単位（秒など）の翻訳一覧を一括取得します。

- GET : `http://localhost:57700/au/translation/batch/optionunit/`

| ステータスコード | 説明 |
| --- | --- |
| 200 | 成功 |

- レスポンス (JSON 配列)
  - 各要素:
    - `Key` any : 単位の識別子（`Second`, `Multiplier` 等の文字列）
    - `Param` any[] : パラメータ（通常は空）
    - `Result` string : 翻訳された単位文字列（例: "秒"）

### GET /au/translation/batch/role/
Extreme Roles で追加された全役職名の翻訳（色付き）を一括取得します。

- GET : `http://localhost:57700/au/translation/batch/role/`

| ステータスコード | 説明 |
| --- | --- |
| 200 | 成功 |

- レスポンス (JSON 配列)
  - 各要素:
    - `Key` any : 役職の内部名（`Sheriff`, `Jester` 等の文字列）
    - `Param` any[] : パラメータ（通常は空）
    - `Result` string : 翻訳・色付けされた役職名（例: "<color=#FF0000>Sheriff</color>"）

---

## 役職割り当てフィルタ関連

### GET /exr/role/filter/
現在の役職割り当てフィルタの設定を取得します。

- GET : `http://localhost:57700/exr/role/filter/`

| ステータスコード | 説明 |
| --- | --- |
| 200 | 成功 |

- レスポンス (JSON)
  - `FilterSet` オブジェクト : フィルタ ID (GUID) をキーとした以下のオブジェクトのマップ
    - `AssignNum` int : このフィルタに割り当てられる人数
    - `FilterNormalId` オブジェクト : このフィルタに含まれるバニラ役職の ID (int) と役職名 (string) のマップ
    - `FilterCombinationId` オブジェクト : このフィルタに含まれる組み合わせ役職の ID (int) と役職名 (string) のマップ
    - `FilterGhostRoleId` オブジェクト : このフィルタに含まれるゴースト役職の ID (int) と役職名 (string) のマップ
  - `FilterRoleId` int[] : フィルタに使用されている全役職の ID リスト
  - `NormalRoleId` オブジェクト : ID (int) からバニラ役職名 (string) へのマッピング
  - `CombinationId` オブジェクト : ID (int) から組み合わせ役職名 (string) へのマッピング
  - `GhostRoleId` オブジェクト : ID (int) からゴースト役職名 (string) へのマッピング

### POST /exr/role/filter/
役職割り当てフィルタを更新します。ホストのみ実行可能です。

- POST : `http://localhost:57700/exr/role/filter/`

| ステータスコード | 説明 |
| --- | --- |
| 200 | 成功 |
| 400 | ホストではない、または不正なリクエスト |

- パラメーター (Body JSON)
  - `Op` string 必須 : 操作内容 (`FilterNewAdd`, `FilterRoleAdd`, `FilterAssignNumIncrease`, `FilterAssignNumDecrease`, `FilterRoleDelete`, `FilterDelete`)
  - `FilterId` string 必須 : フィルタ ID (GUID)
  - `MapRoleId` int? : 役職 ID（役職の追加・削除時のみ必須）
