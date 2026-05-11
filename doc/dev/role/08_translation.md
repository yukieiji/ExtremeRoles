# 翻訳の設定

役職名や説明文を表示するために、リソースファイル (`.resx`) にテキストを追加します。

## 翻訳ファイルの場所

翻訳データは `ExtremeRoles/Translation/resx/` 配下の以下のファイルで管理されています。

- `ExtremeRoles.en-US.resx`: 英語
- `ExtremeRoles.zh-Hans.resx`: 簡体字中国語
- `ExtremeRoles.zh-Hant.resx`: 繁体字中国語

※ 日本語のデータは、ビルド時にこれらのリソースファイルから生成される仕組みになっています。

## 翻訳方法（本ニャク / ResX Manager）

本プロジェクトでは、翻訳管理に **ResX Manager** を使用することを推奨しています。

1.  **ResX Manager** を[こちら](https://github.com/dotnet/ResXResourceManager/releases/latest)からダウンロード・インストールします。
2.  クローンしたリポジトリのルートディレクトリを指定して開きます。
3.  一覧形式で各言語の翻訳を入力・管理できます。

## 基本的なキー

役職を追加する際は、最低限以下のキーを各言語ファイルに追加する必要があります。

- **`{ExtremeRoleId}`**: 役職名（例：`MyRole`）
- **`{ExtremeRoleId}FullDescription`**: 役職の詳細な説明（設定画面などで表示）
- **`{ExtremeRoleId}IntroDescription`**: ゲーム開始時の役職紹介画面で表示される説明
- **`{ExtremeRoleId}ImportantText`**: ゲーム中に画面上部に表示される短い指示や説明

## 注意事項

- 特殊文字（`<`, `>`, `&` など）を直接リソースファイルに入力する場合は、エスケープ（`&lt;`, `&gt;`, `&amp;`）が必要です（ResX Managerを使用する場合は自動で処理されます）。
- 改行を入れたい場合は `\n` を使用します。
