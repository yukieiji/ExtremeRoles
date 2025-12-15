using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module.InfoOverlay.Model.Panel;

#nullable enable

public sealed class AllRoleInfoModel : RolePagePanelModelBase
{
	public class LiberalOptionToString(System.Func<string> getRoleSpawnStr, IEnumerable<IOption> global, IEnumerable<IOption> specificOption) : IOptionToStringHelper
	{
		private readonly IEnumerable<IOption> allOption = global.Concat(specificOption);
		private readonly IOption first = specificOption.FirstOrDefault() ?? global.First();
		private readonly System.Func<string> roleSpawnStrGetter = getRoleSpawnStr;

		public bool IsActive => first.IsViewActive;

		public override string ToString()
		{
			var builder = new StringBuilder();
			builder.AppendLine(roleSpawnStrGetter.Invoke());
			foreach (var opt in allOption)
			{
				IInfoOverlayPanelModel.AddHudStringWithChildren(builder, opt, 0);
			}
			return builder.ToString();
		}
	}

	protected override void CreateAllRoleText()
	{
		IOption option;
		string colorRoleName;
		string roleFullDesc;

		var liberalOption = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<LiberalDefaultOptionLoader>();
		foreach (ExtremeRoleId id in new ExtremeRoleId[] { ExtremeRoleId.Leader, ExtremeRoleId.Dove, ExtremeRoleId.Militant })
		{

			var defaultOpt = liberalOption.GlobalOption;
			var additional = id switch
			{
				ExtremeRoleId.Leader => liberalOption.LeaderOption,
				ExtremeRoleId.Militant => liberalOption.MilitantOption,
				_ => []
			};

			roleFullDesc = Tr.GetString($"{id}FullDescription");
			roleFullDesc = Design.CleanPlaceHolder(roleFullDesc);

			AddPage(new RoleInfo(
				$"<color=#F9F06F>{Tr.GetString(id.ToString())}</color>",
				roleFullDesc, new LiberalOptionToString(() => liberalOption.RoleSpawnSetting, defaultOpt, additional)));
		}


		foreach (var role in Roles.ExtremeRoleManager.NormalRole.Values)
		{
			colorRoleName = role.GetColoredRoleName(true);
			option = role.Loader.Get(RoleCommonOption.SpawnRate);

			roleFullDesc = Tr.GetString($"{role.Core.Id}FullDescription");
			roleFullDesc = Design.CleanPlaceHolder(roleFullDesc);

			AddPage(new RoleInfo(colorRoleName, roleFullDesc, new DefaultOptionToString(option)));
		}

		foreach (var combRole in Roles.ExtremeRoleManager.CombRole.Values)
		{
			option = combRole.Loader.Get(RoleCommonOption.SpawnRate);

			if (combRole is ConstCombinationRoleManagerBase)
			{
				foreach (var role in combRole.Roles)
				{
					colorRoleName = role.GetColoredRoleName(true);

					roleFullDesc = Tr.GetString($"{role.Core.Id}FullDescription");
					roleFullDesc = Design.CleanPlaceHolder(roleFullDesc);

					AddPage(new RoleInfo(colorRoleName, roleFullDesc, new DefaultOptionToString(option)));
				}
			}
			else if (combRole is FlexibleCombinationRoleManagerBase flexCombRole)
			{
				colorRoleName = flexCombRole.GetOptionName();

				roleFullDesc = flexCombRole.GetBaseRoleFullDescription();
				roleFullDesc = Design.CleanPlaceHolder(roleFullDesc);

				AddPage(new RoleInfo(colorRoleName, roleFullDesc, new DefaultOptionToString(option)));
			}
		}
	}
}
