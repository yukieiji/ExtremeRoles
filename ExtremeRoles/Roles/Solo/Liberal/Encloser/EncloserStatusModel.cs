using UnityEngine;

using ExtremeRoles.Core.Abstract;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API.Interface.Status;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal.Encloser;

public sealed class EncloserStatusModel(
	int stakeCount,
	int metsuKillMoney,
	int metsuLimit,
	IUnityObjectFactory? factory = null,
	IIl2CppObjectProvider? il2cppProvider = null,
	IResourcesProvider? resourcesProvider = null
) : IStatusModel
{
	public int StakeCount { get; } = stakeCount;
	public int MetsuKillMoney { get; } = metsuKillMoney;
	public int RemainingMetsuCount { get; set; } = metsuLimit;

	public int CurStakeCount => this.polygon.Count;
	public bool IsUseMetsu => this.polygon.IsCompleted;

	private readonly EncloserPolygon polygon = new EncloserPolygon(factory, il2cppProvider, resourcesProvider);

	public void Update()
	{
		this.polygon.UpdateVisuals(true);
	}

	public bool IsKillPosition(Vector2 pos)
		=> this.polygon.IsPointInside(pos);

	public void ClearStake()
	{
		this.polygon.Clear();
	}

	public void PlaceStake(Vector2 pos, bool isLocalPlayerEncloser)
	{
		this.polygon.AddStake(pos, this.StakeCount);
		this.polygon.UpdateVisuals(isLocalPlayerEncloser);
	}
}
