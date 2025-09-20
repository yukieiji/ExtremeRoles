using ExtremeRoles.Extension;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using System;

namespace ExtremeRoles.Module;

public sealed class RoleParentOptionIdGenerator(int offset) : IRoleParentOptionIdGenerator
{
	private const int rankIdShift = 7;
	private const int categoryIdShift = 13;

	private const int rankIdMask = 0b111111; // 6 bits for RankId
	private const int categoryIdMask = 0b111; // 3 bits for CategoryId

	private readonly int offset = offset;

	public const int RoleIdOffset = 50;

	/* 
	 * 1 ～ 7bit(128) : 役職オプション本体 + 予約済みオプション  
	 * 8 ～ 13bit : 役職ID
	 * 13 ～ 16bit : カテゴリID
	*/

	public enum Category
	{
		Normal = 0,
		Combination,
		Ghost
	}

	public int Get(ExtremeRoleId id)
	{
		int normedId = id.FastInt() - RoleIdOffset;
		return get(normedId, Category.Normal.FastInt());
	}

	public int Get(ExtremeGhostRoleId id)
		=> get(id.FastInt(), Category.Ghost.FastInt());

	public int Get(CombinationRoleType roleId)
		=> get(roleId.FastInt(), Category.Ghost.FastInt());

	private int get(int roleId, int categoryId)
	{
		// Validate input values to ensure they fit within the allocated bits
#if DEBUG
		if ((roleId & ~rankIdMask) != 0)
		{
			throw new ArgumentOutOfRangeException(nameof(roleId), "RankId must be between 0 and 63.");
		}
		if ((categoryId & ~categoryIdMask) != 0)
		{
			throw new ArgumentOutOfRangeException(nameof(categoryId), "CategoryId must be between 0 and 7.");
		}
#endif
		// Combine the IDs using bitwise operations
		int generatedId = (roleId << rankIdShift) | (categoryId << categoryIdShift);

		// Apply the offset
		return generatedId + offset;
	}
}
