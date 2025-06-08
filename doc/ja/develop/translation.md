# ExtremeRolesの翻訳開発に関するドキュメント

## ロジック

翻訳システムの調整等を行っていたのでそれぞれのMODで異なる翻訳ロジックが行われている、世代的に言えばExSが第一世代、EvEが第二世代、ExRが第三世代の翻訳となる

- ExtremeRoles
  - 翻訳ファイル => MOD : ExtremeRoles/Translationフォルダ内にあるresx(XML形式)をExtremeRoles.Generatorで解析後、翻訳を直接コードへ埋め込むロジックを採用
  - MOD => ゲームへの翻訳 : AmongUs本体の翻訳システムをそのまま流用、パッチ数も最低限で実装
- ExtremeSkins
  - TOR等と同じロジック
    - 翻訳ファイル => MOD : ExtremeSkinsTransData.xlsxをPrebuild時にPythonを使い解析、翻訳をJsonにしてリソースとして追加
    - MOD => ゲームへの翻訳 : 独自の翻訳システムを使用
- ExtremeVoiceEngine
  - 翻訳ファイル => MOD : ExtremeVoiceEngine/Resources/Japanese.csvをそのままリソースとして追加
  - MOD => ゲームへの翻訳 : AmongUs本体の翻訳システムをそのまま流用、パッチ数も最低限で実装

## 翻訳方法

- ExtremeRoles
  - VisualStudioを使う方法
     1. リポジトリをクローンする
     2. VisualStudioでExtremeRoles.slnを開く
     3. 「表示」=> 「その他のウィンドウ」 => 「ResX Manager」をクリック
     4. それで翻訳を行えます
  - ResX ResourceManagerを使う方法
     1. リポジトリをクローンする
     2. `ResXResourceManager`を[ここから](https://github.com/dotnet/ResXResourceManager/releases/latest)をダウンロードする
     3. `ResXResourceManager.exe`を実行する
     4. ディレクトリをクローンしたフォルダに設定する
     5. それで翻訳を行えます
- ExtremeSkins
  1. リポジトリをクローンする
  2. 「ExtremeSkinsTransData.xlsx」を編集する
- ExtremeVoiceEngine
  1. リポジトリをクローンする
  2. 「ExtremeVoiceEngine/Resources/」の下に対応した言語のcsvを追加


## 今後
 - できればすべてExRのシステムへリプレイスしたい [#372](https://github.com/yukieiji/ExtremeRoles/issues/372)
   - まずEvEのシステムは簡単にExRのシステムへリプレイスできるのでそれを目標とする

