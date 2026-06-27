using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtremeRoles.Test.Lobby;

public static class TranslationChecker
{
	public static void Check()
	{
		var allTrans = TranslationController.Instance.currentLanguage.AllStrings;
		var valKeys = new Dictionary<string, HashSet<string>>(allTrans.Count);

		var builder = new StringBuilder();

		AppendText(builder);

		string log = builder.ToString();
		HudManager.Instance.Chat.AddChat(
			PlayerControl.LocalPlayer,
			log);
	}

	private static void AppendText(StringBuilder builder)
	{

		var allTrans = TranslationController.Instance.currentLanguage.AllStrings;
		var valKeys = new Dictionary<string, HashSet<string>>(allTrans.Count);


		builder.AppendLine("--- Current Translation Data ---");

		foreach (var item in allTrans)
		{
			string key = item.Key;
			string val = item.Value;

			if (valKeys.TryGetValue(val, out var set) &&
				set is not null)
			{
				set.Add(key);
			}
			else
			{
				HashSet<string> newSet = [key];
				valKeys.Add(val, newSet);
			}
		}

		builder.AppendLine("--- Current Translation Data END ---");

		var dupeValue = valKeys.Where(x => x.Value.Count > 1);

		if (!dupeValue.Any())
		{
			return;
		}
		builder.AppendLine("--- Dupe values ---");

		foreach (var (val, set) in dupeValue)
		{
			builder.AppendLine($" - Value:{val}");
			foreach (string key in set)
			{
				builder.AppendLine($"   - key:{key}");
			}
			builder.AppendLine();
		}

		builder.AppendLine("--- Dupe values end ---");
	}
}
