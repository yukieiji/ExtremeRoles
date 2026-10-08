using System.Collections.Generic;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Combination;

public sealed class GuesserRoleInfoContainer : IGuesserRoleInfoContainer
{
	private readonly List<GuessBehaviour.RoleInfo> result = new();
	private readonly Dictionary<ExtremeRoleType, List<ExtremeRoleId>> separatedRoleId = new()
	{
		{ ExtremeRoleType.Crewmate, new List<ExtremeRoleId>() },
		{ ExtremeRoleType.Impostor, new List<ExtremeRoleId>() },
		{ ExtremeRoleType.Neutral, new List<ExtremeRoleId>() },
		{ ExtremeRoleType.Liberal, new List<ExtremeRoleId>() },
	};

	public IReadOnlyList<GuessBehaviour.RoleInfo> Result => this.result;
	public IReadOnlyDictionary<ExtremeRoleType, List<ExtremeRoleId>> SeparatedRoleId => this.separatedRoleId;

	public void Add(
		ExtremeRoleId id,
		ExtremeRoleType team,
		ExtremeRoleId another = ExtremeRoleId.Null)
	{
		this.result.Add(new GuessBehaviour.RoleInfo
		{
			Id = id,
			AnothorId = another,
			Team = team,
		});
	}

	public void AddSeparatedRoleId(ExtremeRoleType team, ExtremeRoleId id)
	{
		if (this.separatedRoleId.TryGetValue(team, out var list))
		{
			list.Add(id);
		}
	}

	public void ListAdd(ExtremeRoleId baseId, ExtremeRoleType team, List<ExtremeRoleId> list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			Add(baseId, team, list[i]);
		}
	}

	public void ListAddTargetTeam(
		ExtremeRoleId baseId,
		ExtremeRoleType team,
		ExtremeRoleType targetType)
	{
		if (this.separatedRoleId.TryGetValue(targetType, out var list))
		{
			ListAdd(baseId, team, list);
		}
	}
}
