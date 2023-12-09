using System;

using Hazel;

using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.GhostRoles.Crewmate;

namespace ExtremeRoles.Module.Interface;

#nullable enable

public enum StringSerializerType : byte
{
	PhotographerPhoto,
	ShutterPhoto,
}

public interface IStringSerializer
{
	public StringSerializerType Type { get; }
	public bool IsRpc { get; set; }

	public void Serialize(RPCOperator.RpcCaller caller);
	public void Deserialize(MessageReader reader);
	public string ToString();

	// TODO: C#11以降全てのSerializerにこれを強制させる
	public static IStringSerializer DeserializeStatic(MessageReader reader)
	{
		IStringSerializer serializer = ((StringSerializerType)reader.ReadByte()) switch
		{
			StringSerializerType.PhotographerPhoto => new Photographer.PhotoSerializer(),
			StringSerializerType.ShutterPhoto => new Shutter.GhostPhotoSerializer(),
			_ => throw new ArgumentException("Invalided Type"),
		};

		serializer.Deserialize(reader);

		return serializer;
	}
}
