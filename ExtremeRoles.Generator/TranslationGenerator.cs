﻿using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

using ExtremeRoles.Generator.Core;

namespace ExtremeRoles.Generator;

[Generator]
public sealed class TranslationGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		// すべてのAdditionalTextのうち、".resx"で終わるファイルを抽出
		var additionalResxProvider = context.AdditionalTextsProvider
			.Where(x => x.Path.EndsWith(".resx", StringComparison.OrdinalIgnoreCase));

		// ベース翻訳用のAdditionalTextを抽出（ファイル名にピリオドが1つのみのもの）
		var baseTransProvider = additionalResxProvider
			.Where(x => x.Path.Split('.').Length == 2)
			.Collect();

		// ベース翻訳からTranslationDictionaryBuilderを生成するプロバイダーを作成
		var builderProvider = baseTransProvider.Select(
			(x, _) => new TranslationDictionaryBuilder(x));

		// TranslationPear.CodeLangPear は静的な辞書と仮定して、各言語についてソース生成の処理を登録
		foreach (var item in TranslationPear.CodeLangPear)
		{
			// "ja-JP" はベース翻訳として後で一括生成するので除外
			if (item.Key == "ja-JP")
			{
				continue;
			}

			string targetLanguage = item.Key;
			string languageName = item.Value.ToString();

			// 対象言語の .resx ファイルを抽出（ファイルパスが ".{targetLanguage}.resx" で終わるもの）
			var targetTransProvider = additionalResxProvider
				.Where(x => x.Path.EndsWith($".{targetLanguage}.resx"))
                .Collect();

			// builderProvider と対象言語の .resx ファイルプロバイダーを結合
			var combinedProvider = builderProvider.Combine(targetTransProvider);

			// 対象言語用のソースを生成する処理を登録
			context.RegisterSourceOutput(combinedProvider, (spc, tuple) =>
			{
				var (builder, targetFiles) = tuple;
				var transData = builder.Build(targetFiles);
				ExportSourceFromDict(spc, transData, targetLanguage, languageName);
			});
		}

		// ベース翻訳("ja-JP")用のソース生成処理を登録
		context.RegisterSourceOutput(builderProvider, (spc, builder) =>
		{
			ExportSourceFromDict(spc, builder.Base, "ja-JP", TranslationPear.Lang.Japanese.ToString());
		});
	}

	/// <summary>
	/// 指定された辞書データから生成するソースコードを構築し、AddSource で追加します。
	/// </summary>
	/// <param name="context">出力先のコンテキスト</param>
	/// <param name="dict">翻訳キーと値の辞書</param>
	/// <param name="target">ターゲットの言語コード (例: "en-US")</param>
	/// <param name="name">翻訳名 (例: "English")</param>
	private static void ExportSourceFromDict(SourceProductionContext context, IReadOnlyDictionary<string, string> dict, string target, string name)
	{
		var strBuilder = new StringBuilder();
		foreach (var item in dict)
		{
			strBuilder
				.Append(@"{ """)
				.Append(item.Key)
				.Append(@""", @""")
				.Append(item.Value)
				.AppendLine(@""" },")
				.Append("        ");
		}

		string source = @$"// <auto-generated translations data/>
using System.Collections.Generic;

namespace ExtremeRoles.Translation;

public static partial class TranslationRawData
{{
    public static IReadOnlyDictionary<string, string> {name} => new Dictionary<string, string>
    {{
        {strBuilder}
    }};
}}

";
		context.AddSource($"TranslationData.{target}.g.cs", SourceText.From(source, Encoding.UTF8));
	}
}
