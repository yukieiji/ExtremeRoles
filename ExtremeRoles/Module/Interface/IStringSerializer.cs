using Hazel;
using System;

namespace ExtremeRoles.Module.Interface;

#nullable enable

public enum StringSerializerType : byte
{

}

public interface IStringSerializer
{
	public StringSerializerType Type { get; }
	public bool IsRpc { get; init; }

	public void Serialize(RPCOperator.RpcCaller caller);
	public void Deserialize(MessageReader reader);
	public string ToString();

	// C#11以降全てのSerializerにこれを強制させる
	public static IStringSerializer DeserializeStatic(MessageReader reader)
		=> ((StringSerializerType)reader.ReadByte()) switch
		{
			_ => throw new ArgumentException("Invalided Type"),
		};
}
