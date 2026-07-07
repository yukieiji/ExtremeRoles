# REST API リファレンス

### 役職アサインのシミュレーションを実行

- POST : `http://localhost:57700/exr/role/simulate/`

| ステータスコード | 説明 |
| --- | --- |
| 200 | 成功 |
| 400 | リクエストの失敗（ホストではない、またはゲームが開始できる状態にない場合） |

- リクエスト (JSON)
  - `Cycle` int : 試行回数
  - `Option` オブジェクト :
    - `PlayerNum` int : プレイヤー数
  - `MockPlayerNames` string[]? : モックプレイヤー名のリスト（任意）

- レスポンス (JSON 配列)
  - 各要素:
    - `CycleData` オブジェクト配列 : 1回ごとのアサイン結果
      - `PlayerName` string : プレイヤー名
      - `RoleName` string : 役職名
      - `Team` string : チーム名 (`Null`, `Neutral`, `Crewmate`, `Impostor`, `Liberal`)

### ロビー情報の取得

- GET : `http://localhost:57700/au/lobby/`

| ステータスコード | 説明 |
| --- | --- |
| 200 | 成功 |
| 400 | 取得に失敗（ゲームの情報が取得できない場合） |

- レスポンス (JSON)
  - `Online` オブジェクト? : オンライン情報（ローカルゲームの場合は null）
    - `MaxPlayerNum` int : 最大プレイヤー数
    - `Code` string : 部屋コード
    - `Server` string : サーバー名
  - `CurrentPlayerNames` string[] : 現在入室しているプレイヤー名のリスト
