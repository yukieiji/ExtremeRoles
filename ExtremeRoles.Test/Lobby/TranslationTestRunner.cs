﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtremeRoles.Test.Lobby;

public class TranslationTestRunner : LobbyTestRunnerBase
{
	public override IEnumerator Run()
	{
		var allTrans = TranslationController.Instance.currentLanguage.AllStrings;
		var valKeys = new Dictionary<string, HashSet<string>>(allTrans.Count);

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

		var dupeValue = valKeys.Where(x => x.Value.Count > 1);

		if (!dupeValue.Any())
		{
			yield break;
		}

		var builder = new StringBuilder();
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


		string log = builder.ToString();
		HudManager.Instance.Chat.AddChat(
			PlayerControl.LocalPlayer,
			log);

		Log.LogError(log);
	}
}
