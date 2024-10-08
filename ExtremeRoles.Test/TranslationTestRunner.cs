﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtremeRoles.Test;

public sealed class TranslationTestRunner : TestRunnerBase
{
	public override void Run()
	{
		var allTrans = TranslationController.Instance.currentLanguage.AllStrings;
		var valKeys = new Dictionary<string, HashSet<string>>(allTrans.Count);


		foreach (var (key, val) in allTrans)
		{
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
			return;
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

		this.Log.LogError(log);
	}
}
