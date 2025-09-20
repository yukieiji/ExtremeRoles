using ExtremeRoles.Extension.UnityEvents;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.API.Interface;
using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;
using ExtremeRoles.Module.RoleAssign.Model;
using ExtremeRoles.Module.RoleAssign.Update;
using ExtremeRoles.Roles;
using Il2CppInterop.Runtime.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour.View;

[Il2CppRegister]
public sealed class AddRoleMenuView : MonoBehaviour
{
#pragma warning disable CS8618
	public TextMeshProUGUI Title { get; private set; }
	public FilterItemProperty FilterItemPrefab { get; private set; }

	private ButtonWrapper buttonPrefab;
	private GridLayoutGroup layout;

	private IGhostRoleCoreProvider? provider;
	private readonly Dictionary<int, ButtonWrapper> allButton = new ();

	public AddRoleMenuView(IntPtr ptr) : base(ptr) { }
#pragma warning restore CS8618
	public void Awake()
	{
		Transform trans = transform;

		this.Title = trans.Find("Title").GetComponent<TextMeshProUGUI>();

		this.FilterItemPrefab = trans.Find(
			"FilterItem").gameObject.GetComponent<FilterItemProperty>();
		this.buttonPrefab = trans.Find("Button").gameObject.GetComponent<ButtonWrapper>();
		this.layout = trans.Find("Scroll/Viewport/Content").gameObject.GetComponent<GridLayoutGroup>();

		var closeButton = trans.Find("CloseButton").gameObject.GetComponent<Button>();
		closeButton.onClick.AddListener(() => base.gameObject.SetActive(false));

		this.provider = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<IGhostRoleCoreProvider>();
	}

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			base.gameObject.SetActive(false);
		}
	}

	[HideFromIl2Cpp]
	public void UpdateView(
		RoleAssignFilterModel model, Guid filterId,
		Transform targetFilterTransform)
	{
		if (this.layout.rectChildren.Count == 0)
		{
			this.allButton.Clear();
			// メニューを作る
			foreach (int id in model.Id)
			{
				var button = Instantiate(this.buttonPrefab, this.layout.transform);
				this.allButton.Add(id, button);
			}
		}

		foreach (var (id, button) in this.allButton)
		{
			button.gameObject.SetActive(true);
			button.ResetButtonAction();
			button.SetButtonClickAction(
				createButton(
					button, model, filterId,
					id, targetFilterTransform));
		}
	}

	[HideFromIl2Cpp]
	private Action createButton(
		ButtonWrapper button, RoleAssignFilterModel model,
		Guid filterId, int id, Transform targetFilterTransform)
	{
		if (model.NormalRole.TryGetValue(id, out var normalRoleId) &&
			ExtremeRoleManager.NormalRole.TryGetValue((int)normalRoleId, out var role) &&
			role is not null)
		{
			string roleName = role.GetColoredRoleName(true);
			button.SetButtonText(roleName);
			return () =>
			{
				base.gameObject.SetActive(false);
				if (RoleAssignFilterModelUpdater.AddRoleData(model, filterId, id, normalRoleId))
				{
					createFilterItem(model, roleName, filterId, id, targetFilterTransform);
				}
			};
		}
		else if (
			model.CombRole.TryGetValue(id, out var combRoleId) &&
			ExtremeRoleManager.CombRole.TryGetValue((byte)combRoleId, out var combRole) &&
			combRole is not null)
		{
			string combRoleName = combRole.GetOptionName();
			button.SetButtonText(combRoleName);
			return () =>
			{
				base.gameObject.SetActive(false);
				if (RoleAssignFilterModelUpdater.AddRoleData(model, filterId, id, combRoleId))
				{
					createFilterItem(model, combRoleName, filterId, id, targetFilterTransform);
				}
			};
		}
		else if (
			model.GhostRole.TryGetValue(id, out var ghostRoleId) &&
			this.provider is not null)
		{
			var core = this.provider.Get((ExtremeGhostRoleId)id);
			string ghostRoleName = DefaultGhostRoleVisual.GetDefaultColoredRoleName(core);

			button.SetButtonText(ghostRoleName);
			return () =>
			{
				base.gameObject.SetActive(false);
				if (RoleAssignFilterModelUpdater.AddRoleData(model, filterId, id, ghostRoleId))
				{
					createFilterItem(model, ghostRoleName, filterId, id, targetFilterTransform);
				}
			};
		}
		else
		{
			return () => { };
		}
	}

	[HideFromIl2Cpp]
	private void createFilterItem(
		RoleAssignFilterModel model, string name,
		Guid targetFilter, int id, Transform parent)
	{
		FilterItemProperty item = Instantiate(
			this.FilterItemPrefab, parent);
		item.gameObject.SetActive(true);
		item.Text.text = name;
		item.RemoveButton.onClick.AddListener(
			() =>
			{
				RoleAssignFilterModelUpdater.RemoveFilterRole(model, targetFilter, id);
				Destroy(item.gameObject);
			});
	}
}
