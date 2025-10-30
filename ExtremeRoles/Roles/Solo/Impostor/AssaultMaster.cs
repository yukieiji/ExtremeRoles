using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.Ability;


using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class AssaultMaster : SingleRoleBase, IRoleAutoBuildAbility, IRoleReportHook, IRoleUpdate, ITryKillTo
{
    public enum AssaultMasterOption
    {
        StockLimit,
        StockNumWhenReport,
        StockNumWhenMeetingButton,
        CockingKillCoolReduceTime,
        ReloadReduceKillCoolTimePerStock,
        IsResetReloadCoolTimeWhenKill,
        ReloadCoolTimeReduceRatePerHideStock,
    }

    public ExtremeAbilityButton Button
    {
        get => this.reloadButton;
        set
        {
            this.reloadButton = value;
        }
    }

    private ExtremeAbilityButton reloadButton;
    private TMPro.TextMeshPro reduceKillCoolText;

    private int stock;
    private int stockMax;
    private int addStockWhenReport;
    private int addStockWhenMeetingButton;
    private float cockingReduceTime;
    private float reloadReduceTimePerStock;

    private bool isResetCoolTimeWhenKill;

    private float defaultReloadCoolTime;
    private int timerStock;
    private float timerReduceRate;
    private float curReloadCoolTime;

    private float defaultKillCool;

    public AssaultMaster() : base(
		RoleCore.BuildImpostor(ExtremeRoleId.AssaultMaster),
        true, false, true, true)
    { }

    public void CreateAbility()
    {
        this.CreateNormalAbilityButton(
            "reload",
			Resources.UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.AssaultMasterReload));
    }

    public void HookBodyReport(
        PlayerControl rolePlayer,
        NetworkedPlayerInfo reporter,
        NetworkedPlayerInfo reportBody)
    {
        addStock(this.addStockWhenReport);
    }

    public void HookReportButton(
        PlayerControl rolePlayer, NetworkedPlayerInfo reporter)
    {
        addStock(this.addStockWhenMeetingButton);
    }

    public bool IsAbilityUse() =>
        IRoleAbility.IsCommonUse() &&
        this.stock > 0 &&
        PlayerControl.LocalPlayer.killTimer > 0;

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        if (this.reloadButton != null && this.timerStock > 0)
        {
            float newCoolTime = this.defaultReloadCoolTime;
            for (int i = 0; i < this.timerStock; ++i)
            {
                newCoolTime = newCoolTime * this.timerReduceRate;
            }
            this.curReloadCoolTime = Mathf.Clamp(
                newCoolTime, 0.01f, this.defaultReloadCoolTime);

            this.reloadButton.Behavior.SetCoolTime(
                this.curReloadCoolTime);

            this.reloadButton.OnMeetingEnd();
        }
    }

    public void ResetOnMeetingStart()
    {
        this.KillCoolTime = this.defaultKillCool;
        if (this.reduceKillCoolText != null)
        {
            this.reduceKillCoolText.gameObject.SetActive(false);
        }
    }

    public bool UseAbility()
    {
        this.reloadButton.Behavior.SetCoolTime(
            this.defaultReloadCoolTime);

        this.curReloadCoolTime = this.defaultReloadCoolTime;

        float curKillCool = PlayerControl.LocalPlayer.killTimer;
        float newKillCool = curKillCool;
        int loop = this.stock;
        for (int i = 0; i < loop; ++i)
        {
            newKillCool = newKillCool - this.reloadReduceTimePerStock;
            this.KillCoolTime = this.KillCoolTime - this.reloadReduceTimePerStock;
            --this.stock;
            if (newKillCool < 0.0f)
            {
                break;
            }
        }

        this.KillCoolTime = Mathf.Clamp(
            this.KillCoolTime,
            0.001f, this.defaultKillCool);
        PlayerControl.LocalPlayer.killTimer = Mathf.Clamp(
            newKillCool, 0.001f, curKillCool);

        this.stock = 0;
        this.timerStock = 0;

        return true;
    }
    public void Update(PlayerControl rolePlayer)
    {
        if (this.reduceKillCoolText == null &&
            this.Button != null)
        {
            this.reduceKillCoolText = GameObject.Instantiate(
                HudManager.Instance.KillButton.cooldownTimerText,
                this.Button.Transform);
            this.reduceKillCoolText.enableWordWrapping = false;
            this.reduceKillCoolText.transform.localScale = Vector3.one * 0.5f;
            this.reduceKillCoolText.transform.localPosition += new Vector3(-0.05f, 0.60f, 0);
        }

        if (this.reduceKillCoolText == null) { return; }

        if (this.stock == 0)
        {
            this.reduceKillCoolText.text =
                Tr.GetString("noStockNow");
        }
        else
        {
            this.reduceKillCoolText.text = Tr.GetString(
				"reduceKillCool",
				Mathf.CeilToInt(this.stock * this.reloadReduceTimePerStock));
        }

        this.reduceKillCoolText.gameObject.SetActive(true);

    }

    public bool TryRolePlayerKillTo(
        PlayerControl rolePlayer, PlayerControl targetPlayer)
    {

        if (this.stock > 0)
        {
            this.KillCoolTime = this.defaultKillCool - this.cockingReduceTime;
            --this.stock;
        }
        else
        {
            this.KillCoolTime = this.defaultKillCool;
        }

        if (this.isResetCoolTimeWhenKill && this.Button != null)
        {
            this.Button.OnMeetingEnd();
        }

        return true;
    }

    public override string GetFullDescription() => string.Format(
        base.GetFullDescription(), this.stock,
        this.stockMax, this.curReloadCoolTime);

    protected override void CreateSpecificOption(OptionCategoryScope<AutoParentSetBuilder> categoryScope)
    {
        var factory = categoryScope.Builder;
        IRoleAbility.CreateCommonAbilityOption(factory);

        factory.CreateIntOption(
            AssaultMasterOption.StockLimit,
            2, 1, 10, 1,
            format: OptionUnit.ScrewNum);
        factory.CreateIntOption(
            AssaultMasterOption.StockNumWhenReport,
            1, 1, 5, 1,
            format: OptionUnit.ScrewNum);
        factory.CreateIntOption(
            AssaultMasterOption.StockNumWhenMeetingButton,
            3, 1, 10, 1,
            format: OptionUnit.ScrewNum);
        factory.CreateFloatOption(
            AssaultMasterOption.CockingKillCoolReduceTime,
            2.0f, 1.0f, 5.0f, 0.1f,
            format: OptionUnit.Second);
        factory.CreateFloatOption(
            AssaultMasterOption.ReloadReduceKillCoolTimePerStock,
            5.0f, 2.0f, 10.0f, 0.1f,
            format: OptionUnit.Second);
        factory.CreateBoolOption(
            AssaultMasterOption.IsResetReloadCoolTimeWhenKill,
            true);
        factory.CreateIntOption(
            AssaultMasterOption.ReloadCoolTimeReduceRatePerHideStock,
            75, 30, 90, 1,
            format: OptionUnit.Percentage);
    }

    protected override void RoleSpecificInit()
    {
        var cate = this.Loader;

        this.stockMax = cate.GetValue<AssaultMasterOption, int>(
            AssaultMasterOption.StockLimit);
        this.addStockWhenReport = cate.GetValue<AssaultMasterOption, int>(
            AssaultMasterOption.StockNumWhenReport);
        this.addStockWhenMeetingButton = cate.GetValue<AssaultMasterOption, int>(
            AssaultMasterOption.StockNumWhenMeetingButton);
        this.cockingReduceTime = cate.GetValue<AssaultMasterOption, float>(
            AssaultMasterOption.CockingKillCoolReduceTime);
        this.reloadReduceTimePerStock = cate.GetValue<AssaultMasterOption, float>(
            AssaultMasterOption.ReloadReduceKillCoolTimePerStock);
        this.isResetCoolTimeWhenKill = cate.GetValue<AssaultMasterOption, bool>(
            AssaultMasterOption.IsResetReloadCoolTimeWhenKill);
        this.timerReduceRate = 1.0f - cate.GetValue<AssaultMasterOption, int>(
            AssaultMasterOption.ReloadCoolTimeReduceRatePerHideStock) / 100.0f;

        this.stock = 0;
        this.timerStock = 0;

        this.defaultReloadCoolTime = cate.GetValue<RoleAbilityCommonOption, float>(
            RoleAbilityCommonOption.AbilityCoolTime);

        this.curReloadCoolTime = this.defaultReloadCoolTime;

        if (!this.HasOtherKillCool)
        {
            this.HasOtherKillCool = true;
            this.defaultKillCool = Player.DefaultKillCoolTime;
        }
        else
        {
            this.defaultKillCool = this.KillCoolTime;
        }
    }

    private void addStock(int addNum)
    {
        int checkStock = this.stock + addNum;
        if (checkStock >= this.stockMax)
        {
            this.timerStock = this.timerStock + (checkStock - this.stockMax);
            this.stock = this.stockMax;
        }
        else
        {
            this.stock  = checkStock;
        }
    }
}
