using System;

using Hazel;

using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Impostor;
using ExtremeRoles.GhostRoles.Crewmate;

namespace ExtremeRoles.Module.Interface;

#nullable enable

public enum StringSerializerType : byte
{
	PhotographerPhoto,

	SlaveDriverHarassment,

	ShutterPhoto,
}

public interface IStringSerializer
{
	public StringSerializerType Type { get; }
	public bool IsRpc { get; set; }

	public void Serialize(RPCOperator.RpcCaller caller);
	public void Deserialize(MessageReader reader);
	public string ToString();

	public static void SerializeStatic(IStringSerializer serializer, RPCOperator.RpcCaller caller)
	{
		caller.WriteByte((byte)serializer.Type);
		serializer.Serialize(caller);
	}

	// TODO: C#11以降全てのSerializerにこれを強制させる
	public static IStringSerializer DeserializeStatic(MessageReader reader)
	{
		IStringSerializer serializer = ((StringSerializerType)reader.ReadByte()) switch
		{
			StringSerializerType.PhotographerPhoto => new Photographer.PhotoSerializer(),
			StringSerializerType.SlaveDriverHarassment => new SlaveDriver.HarassmentReportSerializer(),
			StringSerializerType.ShutterPhoto => new Shutter.GhostPhotoSerializer(),
			_ => throw new ArgumentException("Invalided Type"),
		};

		serializer.Deserialize(reader);

		return serializer;
	}
}
