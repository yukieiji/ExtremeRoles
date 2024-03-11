using Hazel;
using System.Collections.Generic;

namespace ExtremeRoles.Module;

public sealed class RolePlayerId(int id, int gameId)
{
	private readonly int internalId = id;
	private readonly int gameId = gameId;

	public override string ToString()
		=> $"InternalId:{this.internalId}  GameId:{this.internalId}";

	public void Serialize(in MessageWriter writer)
	{
		writer.WritePacked(this.internalId);
		writer.WritePacked(this.gameId);
	}

	public static RolePlayerId DeserializeConstruct(in MessageReader reader)
	{
		int id = reader.ReadPackedInt32();
		int gameId = reader.ReadPackedInt32();
		return new RolePlayerId(id, gameId);
	}

	public override int GetHashCode()
		=> this.internalId ^ this.gameId;

	public override bool Equals(object obj)
	{
		if (obj is not RolePlayerId id)
		{
			return false;
		}
		return
			id.internalId == this.internalId &&
			id.gameId == this.gameId;
	}
}

public sealed class RolePlayerIdGenerator
{
	private readonly Dictionary<int, int> contIdToId = new Dictionary<int, int>();

	public RolePlayerId Generate(int controlId)
	{
		if (!this.contIdToId.TryGetValue(controlId, out int id))
		{
			id = 0;
		}
		int newId = id + 1;
		this.contIdToId[controlId] = newId;

		return new RolePlayerId(id, controlId);
	}
}
