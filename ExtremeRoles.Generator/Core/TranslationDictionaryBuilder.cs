using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Microsoft.CodeAnalysis;

using System.Xml.Linq;


namespace ExtremeRoles.Generator.Core;

public sealed class TranslationDictionaryBuilder
{
	public IReadOnlyDictionary<string, string> Base => baseDict;

	private readonly IReadOnlyDictionary<string, string> baseDict;

	public TranslationDictionaryBuilder(
		IEnumerable<AdditionalText> text)
	{
		var baseTransData = new Dictionary<string, string>();
		foreach (var t in text)
		{
			var source = t.GetText();
			if (source is null)
			{
				continue;
			}

			var data = getData(source);
			foreach (var item in data)
			{
				baseTransData.Add(
					item.Key, item.Value);
			}
		}
		baseDict = baseTransData;
	}

	public IReadOnlyDictionary<string, string> Build(
		IEnumerable<AdditionalText> text)
	{
		var result = new Dictionary<string, string>(baseDict.Count);
		var transed = new HashSet<string>();

		foreach (var t in text)
		{
			var source = t.GetText();
			if (source is null)
			{
				continue;
			}

			var data = getData(source);
			foreach (var item in data)
			{
				string key = item.Key;
				result.Add(key, item.Value);
				transed.Add(key);
			}
		}

		var notTrans = baseDict.Where(x => !transed.Contains(x.Key));
		foreach (var item in notTrans)
		{
			result.Add(item.Key, $"[NOT TRANS]{item.Value}");
		}

		return result;
	}

	private static Dictionary<string, string> getData(in SourceText text)
	{
		// XMLとして解析
		XDocument xDoc = XDocument.Parse(text.ToString());

		// キーと値のペアを辞書に格納
		return xDoc.Descendants("data")
				.ToDictionary(
					element => element.Attribute("name").Value,
					element => element
						.Element("value")
						.Value
						.Replace(@"""", @"""""")
				);
	}
}
