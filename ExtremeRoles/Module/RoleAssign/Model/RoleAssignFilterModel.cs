using System;
using System.Collections.Generic;
using System.IO;
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
		return serializeStr;
	}

	public void DeserializeFromString(string serializedStr)
	{
		string[] splitedSerializedStr = serializedStr.Split(splitChar);

		foreach (string encodingFilter in splitedSerializedStr)
		{
			byte[] deserializedBytes = Convert.FromBase64String(encodingFilter);

			object? obj;
			DataContractSerializer serializer = new DataContractSerializer(
				typeof(RoleFilterData));
			using (MemoryStream stream = new MemoryStream(deserializedBytes))
			{
				obj = serializer.ReadObject(stream);
			}

			if (obj is RoleFilterData model)
			{
				this.FilterSet.Add(Guid.NewGuid(), model);
			}
		}
	}
}
