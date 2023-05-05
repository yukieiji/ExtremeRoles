using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ExtremeVoiceEngine.Translation;

public sealed class Translator
{
    public SupportedLangs DefaultLang => SupportedLangs.Japanese;

    public bool IsSupport(SupportedLangs languageId)
        => languageId == DefaultLang;

    public Dictionary<string, string> GetTranslation(SupportedLangs languageId)
    {
        Dictionary<string, string> transData = new Dictionary<string, string>();

        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        using Stream? stream = assembly.GetManifestResourceStream(
            $"ExtremeVoiceEngine.Resources.{languageId}.csv");
        if (stream is null) { return transData; }
        using StreamReader transCsv = new StreamReader(stream, Encoding.UTF8);
        if (transCsv is null) { return transData; }

        string? transInfoLine;
        while ((transInfoLine = transCsv.ReadLine()) != null)
        {
            string[] transInfo = transInfoLine.Split(',');
            transData.Add(transInfo[0], transInfo[1]);
        }
        return transData;
    }
}
