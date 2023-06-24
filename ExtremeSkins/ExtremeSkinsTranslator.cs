using System.Collections.Generic;
using System.IO;
using System.Text;

using ExtremeRoles.Module.NewTranslation;

namespace ExtremeSkins;

#nullable enable

public sealed class ExtremeSkinsTranslator : ITranslator
{
	public int Priority => 0;

	public SupportedLangs DefaultLang => SupportedLangs.Japanese;

	public bool IsSupport(SupportedLangs languageId)
		=> (languageId) switch
		{
			SupportedLangs.English  or
			SupportedLangs.Japanese or
			SupportedLangs.SChinese => true,
			_ => false,
		};

	public Dictionary<string, string> GetTranslation(SupportedLangs languageId)
	{
		Dictionary<string, string> transData = new Dictionary<string, string>();

		var assembly = System.Reflection.Assembly.GetExecutingAssembly();
		using Stream? stream = assembly.GetManifestResourceStream(
			$"ExtremeSkins.Resources.LangData.{languageId}.csv");
		if (stream is null) { return transData; }
		using StreamReader transCsv = new StreamReader(stream, Encoding.UTF8);

		string? transInfoLine;
		while ((transInfoLine = transCsv.ReadLine()) != null)
		{
			string[] transInfo = transInfoLine.Split(',');
			string key = transInfo[0];
			string value = transInfo[1].Replace("<br>", System.Environment.NewLine);
			transData.Add(key, value);
		}

		// HatとバイザーとNamePlateの翻訳を取ってくる

		return transData;
	}
}
