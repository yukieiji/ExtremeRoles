﻿using System.Text;

using ExtremeRoles.Generator.Core;
using Microsoft.CodeAnalysis;

namespace ExtremeRoles.Generator;

[Generator]
public sealed class TranslationGenerator : ISourceGenerator
{
	public void Execute(GeneratorExecutionContext context)
	{
		var allResx = context.AdditionalFiles.Where(
			x => x.Path.EndsWith(".resx"));

		var baseTrans = allResx.Where(
			x => x.Path.Split('.').Length == 2);


		var builder = new TranslationDictionaryBuilder(
			baseTrans);

		var pear = TranslationPear.CodeLangPear;
		foreach(var item in pear)
		{
			string key = item.Key;
			if (key == "ja-JP")
			{
				continue;
			}

			createSource(
				builder,
				context,
				allResx, 
				key,
				item.Value.ToString());
		}

		exportSourceFromDict(
			context,
			builder.Base,
			"ja-JP",
			TranslationPear.Lang.Japanese.ToString());
	}

	public void Initialize(GeneratorInitializationContext context)
	{

	}

	private static void createSource(
		in TranslationDictionaryBuilder builder,
		in GeneratorExecutionContext context,
		in IEnumerable<AdditionalText> texts,
		string target, string name)
	{
		var targetText = texts.Where(x => x.Path.EndsWith($".{target}.resx"));
		var transData = builder.Build(targetText);
		exportSourceFromDict(context, transData, target, name);
	}

	private static void exportSourceFromDict(
		in GeneratorExecutionContext context,
		in IReadOnlyDictionary<string, string> dict,
		string target, string name)
	{
		var strbuilder = new StringBuilder();
		foreach (var item in dict)
		{
			strbuilder
				.Append(@"{ """)
				.Append(item.Key)
				.Append(@" "", @""")
				.Append(item.Value)
				.AppendLine(@""" },")
				.Append("        ");
		}
		context.AddSource(
			$"TranslationData.{target}.g.cs",
			@$"// <auto-generated translations data/>
using System.Collections.Generic;

namespace ExtremeRoles.Translation;

public static partial class TranslationRawData
{{
    public static IReadOnlyDictionary<string, string> {name} => new Dictionary<string, string>
	{{
		{strbuilder}
	}};
}}

");
	}
}
