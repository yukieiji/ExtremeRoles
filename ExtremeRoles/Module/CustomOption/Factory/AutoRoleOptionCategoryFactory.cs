using UnityEngine;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.CustomOption.Factory.OptionBuilder;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Module.CustomOption.Factory;

public sealed class AutoRoleOptionCategoryFactory(IRoleOptionCategoryIdGenerator idGenerator, OptionCategoryAssembler assembler)
{
	private readonly IRoleOptionCategoryIdGenerator generator = idGenerator;
	private readonly OptionCategoryAssembler assembler = assembler;

	public OptionCategoryScope<AutoParentSetBuilder> CreateRoleCategory(
		ExtremeRoleId id,
		string name,
		in OptionTab tab,
		Color color)
	{
		int categoryId = generator.Get(id);
		return assembler.CreateAutoParentSetOptionCategory(categoryId, name, tab, color);
	}

	public OptionCategoryScope<AutoParentSetBuilder> CreateRoleCategory(
		ExtremeGhostRoleId id,
		string name,
		in OptionTab tab,
		Color color)
	{
		int categoryId = generator.Get(id);
		return assembler.CreateAutoParentSetOptionCategory(categoryId, name, tab, color);
	}

	public OptionCategoryScope<AutoParentSetBuilder> CreateRoleCategory(
		CombinationRoleType id,
		string name,
		in OptionTab tab,
		Color? color)
	{
		int categoryId = generator.Get(id);
		return assembler.CreateAutoParentSetOptionCategory(categoryId, name, tab, color);
	}

	public OptionCategoryScope<AutoParentSetBuilder> CreateInnnerRoleCategory(
		ExtremeRoleId id,
		OptionCategoryScope<AutoParentSetBuilder> parentCategory)
	{
		int categoryId = generator.Get(id);
		return assembler.CreateSubOptionCategory(categoryId, parentCategory);
	}

	public OptionCategoryScope<AutoParentSetBuilder> CreateInnnerRoleCategory(
		ExtremeGhostRoleId id,
		OptionCategoryScope<AutoParentSetBuilder> parentCategory)
	{
		int categoryId = generator.Get(id);
		return assembler.CreateSubOptionCategory(categoryId, parentCategory);
	}
}
