using UnityEngine;

namespace ExtremeRoles.Roles.API;

public sealed class RoleCore(ExtremeRoleId id, ExtremeRoleType team, Color color, string name)
{
	public Color Color { get; set; } = color;
	public ExtremeRoleType Team { get; set; } = team;

	public ExtremeRoleId Id { get; } = id;
	public string Name { get; } = name;


	public RoleCore(ExtremeRoleId id, ExtremeRoleType team, Color color) : this(id, team, color, id.ToString())
	{

	}

	public RoleCore(RoleCore other) : this(other.Id, other.Team, other.Color, other.Name)
	{

	}

	public static RoleCore BuildImpostor(ExtremeRoleId id)
		=> new RoleCore(id, ExtremeRoleType.Impostor, Palette.ImpostorRed);

	public static RoleCore BuildCrewmate(ExtremeRoleId id, Color color)
		=> new RoleCore(id, ExtremeRoleType.Crewmate, color);

	public static RoleCore BuildNeutral(ExtremeRoleId id, Color color)
		=> new RoleCore(id, ExtremeRoleType.Neutral, color);
}
