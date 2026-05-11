# 翻訳の設定

役職名や説明文を表示するために、リソースファイル (`.resx`) にテキストを追加します。

## 翻訳ファイルの場所

翻訳データは `ExtremeRoles/Translation/resx/` 配下の以下のファイルで管理されています。

- `ExtremeRoles.en-US.resx`: 英語
- `ExtremeRoles.zh-Hans.resx`: 簡体字中国語
- `ExtremeRoles.zh-Hant.resx`: 繁体字中国語

※ 日本語のデータは、ビルド時にこれらのリソースファイルから生成される仕組みになっています。

## 基本的なキー

役職を追加する際は、最低限以下のキーを各言語ファイルに追加する必要があります。

- **`{ExtremeRoleId}`**: 役職名（例：`MyRole`）
- **`{ExtremeRoleId}FullDescription`**: 役職の詳細な説明（設定画面などで表示）
- **`{ExtremeRoleId}IntroDescription`**: ゲーム開始時の役職紹介画面で表示される説明
- **`{ExtremeRoleId}ImportantText`**: ゲーム中に画面上部に表示される短い指示や説明

## 能力ボタンやオプションの翻訳

能力ボタンやカスタムオプションで使用するキーも、これらのファイルに追加します。

```csharp
// ボタン作成時
this.CreateNormalAbilityButton(
    "myRoleAbilityKey", // このキーを .resx に追加
    ...);
```

## 注意事項

- 特殊文字（`<`, `>`, `&` など）を `.resx` ファイルの `<value>` タグ内で使用する場合は、エスケープ（`&lt;`, `&gt;`, `&amp;`）が必要です。
- 改行を入れたい場合は `\n` を使用します。
