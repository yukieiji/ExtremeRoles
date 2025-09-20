using ExtremeRoles.Extension.UnityEvents;
using ExtremeRoles.GameMode;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.API.Interface;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;
using ExtremeRoles.Module.RoleAssign.Model;
using ExtremeRoles.Module.RoleAssign.Update;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using Il2CppInterop.Runtime.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour.View;

[Il2CppRegister]
public sealed class RoleAssignFilterView : MonoBehaviour
{
	[HideFromIl2Cpp]
	public RoleAssignFilterModel Model
	{
		private get => this.model;
		set
		{
			this.initialize(value);
			this.model = value;
		}
	}
	[HideFromIl2Cpp]
	public GameObject HideObject { private get; set; }

#pragma warning disable CS8618
	private ButtonWrapper addFilterButton;
	private RoleFilterSetProperty filterSetPrefab;

	private VerticalLayoutGroup layout;

	private AddRoleMenuView addRoleMenu;

	private RoleAssignFilterModel model;

	private IGhostRoleCoreProvider? provider;

	public RoleAssignFilterView(IntPtr ptr) : base(ptr) { }
#pragma warning restore CS8618
	public void Awake()
	{
		Transform trans = base.transform;

		this.addFilterButton = trans.Find(
			"Body/AddFilterButton").gameObject.GetComponent<ButtonWrapper>();
		this.addFilterButton.Awake();
		this.addFilterButton.SetButtonText(
			Tr.GetString("RoleAssignFilterAddFilter"));

		this.layout = trans.Find(
			"Body/Scroll/Viewport/Content").gameObject.GetComponent<VerticalLayoutGroup>();
		this.filterSetPrefab = trans.Find(
			"Body/FillterSet").gameObject.GetComponent<RoleFilterSetProperty>();

		this.addRoleMenu = trans.Find(
			"Body/AddRoleMenu").gameObject.GetComponent<AddRoleMenuView>();
		this.addRoleMenu.Awake();
		this.addRoleMenu.Title.text = Tr.GetString("RoleAssignFilterAddRoleMenuTitle");

		var title = trans.Find("Body/Title").gameObject.GetComponent<TextMeshProUGUI>();
		title.text = Tr.GetString("RoleAssignFilter");

		var closeButton = trans.Find(
			"Body/CloseButton").gameObject.GetComponent<Button>();
		closeButton.onClick.AddListener(() => base.gameObject.SetActive(false));

		// Create Actions
		this.addFilterButton.ResetButtonAction();
		this.addFilterButton.SetButtonClickAction((UnityAction)addNewFilterSet);

		this.provider = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<IGhostRoleCoreProvider>();
	}

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			base.gameObject.SetActive(false);
		}
	}

	public void OnEnable()
	{
		HudManager.Instance.gameObject.SetActive(false);
		if (this.HideObject != null)
		{
			this.HideObject.SetActive(false);
		}
		this.addRoleMenu.gameObject.SetActive(false);

		if (this.Model == null) { return; }

		this.Model.Id.Clear();
		this.Model.NormalRole.Clear();
		this.Model.CombRole.Clear();
		this.Model.GhostRole.Clear();

		var roleSelector = ExtremeGameModeManager.Instance.RoleSelector;
		int id = 0;
		foreach (var roleId in roleSelector.UseNormalRoleId)
		{
			this.Model.Id.Add(id);
			this.Model.NormalRole.Add(id, roleId);
			id++;
		}
		foreach (var roleId in roleSelector.UseCombRoleType)
		{
			this.Model.Id.Add(id);
			this.Model.CombRole.Add(id, roleId);
			id++;
		}
		foreach (var roleId in roleSelector.UseGhostRoleId)
		{
			this.Model.Id.Add(id);
			this.Model.GhostRole.Add(id, roleId);
			id++;
		}
	}

	public void OnDisable()
	{
		if (this.HideObject != null)
		{
			this.HideObject.SetActive(true);
		}
		HudManager.Instance.gameObject.SetActive(true);
	}

	[HideFromIl2Cpp]
	private void addNewFilterSet()
	{
		if (this.Model == null) { return; }

		Guid id = Guid.NewGuid();

		// Update model
		RoleAssignFilterModelUpdater.AddFilter(this.Model, id);
		this.createFilterSet(id);
	}

	[HideFromIl2Cpp]
	private RoleFilterSetProperty createFilterSet(Guid id)
	{
		var filterSet = Instantiate(this.filterSetPrefab, this.layout.transform);
		filterSet.Awake();
		filterSet.gameObject.SetActive(true);

		filterSet.AssignText.text = Tr.GetString("RoleAssignFilterAssignNum");
		filterSet.DeleteThisButton.SetButtonText(
			Tr.GetString("RoleAssignFilterDeleteThis"));
		filterSet.DeleteAllRoleButton.SetButtonText(
			Tr.GetString("RoleAssignFilterDeleteAllRole"));
		filterSet.AddRoleButton.SetButtonText(
			Tr.GetString("RoleAssignFilterAddRole"));

		filterSet.DeleteThisButton.SetButtonClickAction(
			() =>
			{
				RoleAssignFilterModelUpdater.RemoveFilter(this.Model, id);
				Destroy(filterSet.gameObject);
			});
		filterSet.DeleteAllRoleButton.SetButtonClickAction(
			() =>
			{
				RoleAssignFilterModelUpdater.ResetFilter(this.Model, id);
				foreach (var child in filterSet.Layout.rectChildren)
				{
					Destroy(child.gameObject);
				}
			});
		filterSet.AddRoleButton.SetButtonClickAction(
			() =>
			{
				this.addRoleMenu.gameObject.SetActive(true);
				this.addRoleMenu.UpdateView(
					this.Model, id, filterSet.Layout.transform);
			});
		filterSet.IncreseButton.onClick.AddListener(
			() =>
			{
				RoleAssignFilterModelUpdater.IncreseFilterAssignNum(this.Model, id);
				filterSet.AssignNumText.text = $"{this.Model.FilterSet[id].AssignNum}";
			});
		filterSet.DecreseButton.onClick.AddListener(
			() =>
			{
				RoleAssignFilterModelUpdater.DecreseFilterAssignNum(this.Model, id);
				filterSet.AssignNumText.text = $"{this.Model.FilterSet[id].AssignNum}";
			});
		return filterSet;
	}

	[HideFromIl2Cpp]
	private void initialize(RoleAssignFilterModel model)
	{
		if (this.layout != null)
		{
			this.layout.DestroyChildren();
		}

		foreach (var (filterId, filter) in model.FilterSet)
		{
			var filterProp = this.createFilterSet(filterId);
			var parent = filterProp.Layout.transform;
			filterProp.AssignNumText.text = $"{filter.AssignNum}";

			foreach (var (id, roleId) in filter.FilterNormalId)
			{
				string roleName =
					ExtremeRoleManager.NormalRole.TryGetValue((int)roleId, out var role) &&
					role is not null ? role.GetColoredRoleName(true) : string.Empty;
				createFilterItem(parent, roleName, filterId, id);
			}
			foreach (var (id, roleId) in filter.FilterCombinationId)
			{
				string combRoleName =
					ExtremeRoleManager.CombRole.TryGetValue((byte)roleId, out var role) &&
					role is not null ? role.GetOptionName() : string.Empty;
				createFilterItem(parent, combRoleName, filterId, id);
			}
			foreach (var (id, roleId) in filter.FilterGhostRole)
			{
				if (this.provider is null)
				{
					continue;
				}

				var core = this.provider.Get((ExtremeGhostRoleId)id);
				string ghostRoleName = DefaultGhostRoleVisual.GetDefaultColoredRoleName(core);

				createFilterItem(parent, ghostRoleName, filterId, id);
			}
		}
	}

	[HideFromIl2Cpp]
	private void createFilterItem(
		Transform parent, string name, Guid filterId, int id)
	{
		FilterItemProperty item = Instantiate(
			this.addRoleMenu.FilterItemPrefab, parent);
		item.Awake();
		item.gameObject.SetActive(true);
		item.Text.text = name;
		item.RemoveButton.onClick.AddListener(
			() =>
			{
				RoleAssignFilterModelUpdater.RemoveFilterRole(this.Model, filterId, id);
				Destroy(item.gameObject);
			});
	}
}
