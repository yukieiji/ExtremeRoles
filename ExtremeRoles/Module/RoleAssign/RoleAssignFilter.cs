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
	public RoleAssignFilterModel Model { get; private set; }

	private readonly List<RoleFilterSet> filter = new List<RoleFilterSet>();
	private const string defaultValue = "";

	public RoleAssignFilter()
	{
		this.filter.Clear();
		this.Model = getNewModel();
		if (this.Model.Config.Value != defaultValue)
		{
			this.Model.DeserializeFromString(this.Model.Config.Value);
		}
	}

	public void DeserializeModel(string value)
	{
		if (value == defaultValue)
		{
			return;
		}

		this.Model.DeserializeFromString(value);
		if (this.view != null)
		{
			this.view.Model = this.Model;
		}
	}

	public void Initialize()
	{
		var logger = ExtremeRolesPlugin.Logger;

		logger.LogInfo($" -------- Initialize RoleAssignFilter -------- ");

		// フィルターをリセット
		this.filter.Clear();

		foreach (var (guid, filterModel) in this.Model.FilterSet)
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

	public void UpdateUi()
	{
		if (this.view != null)
		{
			this.view.ReSync();
		}
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
			this.view.Awake();
			this.view.Model = this.Model;
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

		this.Model = newModel;
		if (this.view != null)
		{
			this.view.Model = this.Model;
		}
	}

	public string SerializeModel() => this.Model.SerializeToString();

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
