using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.RoleAssign.Model;
using ExtremeRoles.Module.CustomMonoBehaviour.View;
using ExtremeRoles.Resources;

#nullable enable

namespace ExtremeRoles.Module.RoleAssign;

public sealed class RoleAssignFilter : NullableSingleton<RoleAssignFilter>
{
    private List<RoleFilterSet> filter = new List<RoleFilterSet>();
    private RoleAssignFilterView? view;
    private RoleAssignFilterModel model;

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
        Logging.Debug($" -------- Initialize RoleAssignFilter -------- ");

        foreach (var (guid, filterModel) in model.FilterSet)
        {
            Logging.Debug($" ---- Filter:{guid} ---- ");

            int assignNum = filterModel.AssignNum;

            Logging.Debug($"AssignNum:{assignNum}");

            var filterSet = new RoleFilterSet();
            filterSet.AssignNum = assignNum;

            foreach (var extremeRoleId in filterModel.FilterNormalId.Values)
            {
                Logging.Debug($"NormalRoleId:{extremeRoleId}");
                filterSet.Add(extremeRoleId);
            }
            foreach (var extremeRoleId in filterModel.FilterCombinationId.Values)
            {
                Logging.Debug($"CombinationRoleId:{extremeRoleId}");
                filterSet.Add(extremeRoleId);
            }
            foreach (var extremeRoleId in filterModel.FilterGhostRole.Values)
            {
                Logging.Debug($"GhostRoleId:{extremeRoleId}");
                filterSet.Add(extremeRoleId);
            }

            this.filter.Add(filterSet);
        }
        Logging.Debug($" -------- Initialize Complete!! -------- ");
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
                Loader.GetUnityObjectFromResources<GameObject>(
                    "ExtremeRoles.Resources.Asset.roleassignfilter.asset",
                    "assets/roles/roleassignfilter.prefab"));
            
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
        => new RoleAssignFilterModel()
        {
            Config = ExtremeRolesPlugin.Instance.Config.Bind(
                "RoleAssignFilter", OptionManager.Instance.ConfigPreset, defaultValue),
            Id = new(),
            NormalRole = new(),
            CombRole = new(),
            GhostRole = new(),
            FilterSet = new()
        };
}
