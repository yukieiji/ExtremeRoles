using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.RoleAssign.Model;
using ExtremeRoles.Module.CustomMonoBehaviour.View;
using ExtremeRoles.Resources;


#nullable enable

namespace ExtremeRoles.Module.RoleAssign;

public sealed class RoleAssignFilter : NullableSingleton<RoleAssignFilter>
{
	private RoleAssignFilterView? view;
	private RoleAssignFilterModel model;

	private readonly List<RoleFilterSet> filter = new List<RoleFilterSet>();
	private const string defaultValue = "";

	public RoleAssignFilter()
	{
		this.filter.Clear();
		this.model = getNewModel();
		if (this.model.Config.Value != defaultValue)
		{
			this.model.DeserializeFromString(this.model.Config.Value);
		}
	}

	public void DeserializeModel(string value)
	{
		if (value == defaultValue) { return; }

		this.model.DeserializeFromString(value);
		if (this.view != null)
		{
			this.view.Model = this.model;
		}
	}

	public void Initialize()
	{
		var logger = ExtremeRolesPlugin.Logger;

		logger.LogInfo($" -------- Initialize RoleAssignFilter -------- ");

		// フィルターをリセット
		this.filter.Clear();

		foreach (var (guid, filterModel) in this.model.FilterSet)
		{
			logger.LogInfo($" ---- Filter:{guid} ---- ");

			int assignNum = filterModel.AssignNum;

			logger.LogInfo($"AssignNum:{assignNum}");

			var filterSet = new RoleFilterSet(assignNum);

			foreach (var extremeRoleId in filterModel.FilterNormalId.Values)
			{
				logger.LogInfo($"NormalRoleId:{extremeRoleId}");
				filterSet.Add(extremeRoleId);
			}
			foreach (var extremeRoleId in filterModel.FilterCombinationId.Values)
			{
				logger.LogInfo($"CombinationRoleId:{extremeRoleId}");
				filterSet.Add(extremeRoleId);
			}
			foreach (var extremeRoleId in filterModel.FilterGhostRole.Values)
			{
				logger.LogInfo($"GhostRoleId:{extremeRoleId}");
				filterSet.Add(extremeRoleId);
			}

			this.filter.Add(filterSet);
		}
		logger.LogInfo($" -------- Initialize Complete!! -------- ");
	}

	public bool IsBlock(int intedRoleId) => this.filter.Any(x => x.IsBlock(intedRoleId));
	public bool IsBlock(byte bytedCombRoleId) => this.filter.Any(
		x => x.IsBlock(bytedCombRoleId));
	public bool IsBlock(ExtremeGhostRoleId roleId) => this.filter.Any(x => x.IsBlock(roleId));

	// UIを見せる
	public void OpenEditor(GameObject hideObj)
	{
		if (this.view == null)
		{
			hideObj.SetActive(false);

			// アセットバンドルからロード
			GameObject viewObj = Object.Instantiate(
				UnityObjectLoader.LoadFromResources<GameObject>(
					ObjectPath.SettingTabAsset,
					ObjectPath.RoleAssignFilterPrefab));

			viewObj.SetActive(false);

			this.view = viewObj.GetComponent<RoleAssignFilterView>();
			this.view.HideObject = hideObj;
			this.view.Model = this.model;
			this.view.Awake();
		}
		this.view.gameObject.SetActive(true);
	}

	public void SwitchPreset()
	{
		var newModel = getNewModel();
		string value = newModel.Config.Value;

		if (value != defaultValue)
		{
			newModel.DeserializeFromString(value);
		}

		this.model = newModel;
		if (this.view != null)
		{
			this.view.Model = this.model;
		}
	}

	public string SerializeModel() => this.model.SerializeToString();

	public void Update(int intedRoleId)
	{
		foreach (var fil in this.filter)
		{
			fil.Update(intedRoleId);
		}
	}
	public void Update(byte bytedCombRoleId)
	{
		foreach (var fil in this.filter)
		{
			fil.Update(bytedCombRoleId);
		}
	}
	public void Update(ExtremeGhostRoleId roleId)
	{
		foreach (var fil in this.filter)
		{
			fil.Update(roleId);
		}
	}

	private static RoleAssignFilterModel getNewModel()
	{
		var config = ExtremeRolesPlugin.Instance.Config.Bind(
			"RoleAssignFilter", OptionManager.Instance.ConfigPreset, defaultValue);
		return new RoleAssignFilterModel(config);
	}
}
