using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

using BepInEx.Configuration;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;

#nullable enable

namespace ExtremeRoles.Module.RoleAssign.Model;

public sealed class RoleAssignFilterModel(ConfigEntry<string> config)
{
	public ConfigEntry<string> Config { get; } = config;

	public Dictionary<Guid, RoleFilterData> FilterSet { get; } = new Dictionary<Guid, RoleFilterData>();

	public List<int> Id { get; } = new List<int>();
	public Dictionary<int, ExtremeRoleId> NormalRole { get; } = new Dictionary<int, ExtremeRoleId>();
	public Dictionary<int, CombinationRoleType> CombRole { get; } = new Dictionary<int, CombinationRoleType>();
	public Dictionary<int, ExtremeGhostRoleId> GhostRole { get; } = new Dictionary<int, ExtremeGhostRoleId>();

	private const string version = "v1";

	private const char splitChar = '|';

	public string SerializeToString()
	{
		StringBuilder builder = new StringBuilder();

		foreach (var filter in this.FilterSet.Values)
		{
			DataContractSerializer serializer = new DataContractSerializer(typeof(RoleFilterData));
			using (MemoryStream stream = new MemoryStream())
			{
				serializer.WriteObject(stream, filter);
				byte[] serializedBytes = stream.ToArray();
				string base64String = Convert.ToBase64String(serializedBytes);
				builder.Append(base64String).Append(splitChar);
			}
		}
		string serializeStr = builder.ToString();
		if (serializeStr.EndsWith(splitChar))
		{
			serializeStr = serializeStr.Remove(serializeStr.Length - 1);
		}
		return $"{version}|{serializeStr}";
	}

	public void DeserializeFromString(string serializedStr)
	{
		string[] splitedSerializedStr = serializedStr.Split(splitChar);

		bool liberalMigrateOn = false;
		liberalMigrateOn = splitedSerializedStr[0] != version;
		int startIndex = liberalMigrateOn ? 0 : 1;

		foreach (string encodingFilter in splitedSerializedStr[startIndex..])
		{
			byte[] deserializedBytes = Convert.FromBase64String(encodingFilter);

			object? obj;
			DataContractSerializer serializer = new DataContractSerializer(
				typeof(RoleFilterData));
			using (MemoryStream stream = new MemoryStream(deserializedBytes))
			{
				obj = serializer.ReadObject(stream);
			}

			if (obj is not RoleFilterData model)
			{
				continue;
			}

			if (liberalMigrateOn)
			{
				migrateLiberal(model);
			}
			this.FilterSet.Add(Guid.NewGuid(), model);
		}
	}

	// リベラル実装に伴い0, 1, 2がリベラル陣営の固定役職になったためIDを全体的に3ずらす
	private static void migrateLiberal(RoleFilterData model)
	{
		const int offset = 3;
		model.FilterNormalId = offsetId(model.FilterNormalId, offset);
		model.FilterCombinationId = offsetId(model.FilterCombinationId, offset);
		model.FilterGhostRole = offsetId(model.FilterGhostRole, offset);
	}
	private static Dictionary<int, T> offsetId<T>(Dictionary<int, T> target, int offset)
	{
		var newDict = new Dictionary<int, T>(target.Count);
		foreach (var (k, v) in target)
		{
			newDict[k + offset] = v;
		}
		return newDict;
	}
}
