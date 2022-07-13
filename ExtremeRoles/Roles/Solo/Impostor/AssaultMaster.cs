using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Impostor
{
    public sealed class AssaultMaster : SingleRoleBase, IRoleAbility, IRoleReportHock, IRoleUpdate
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

        public RoleAbilityButtonBase Button
        { 
            get => this.reloadButton;
            set
            {
                this.reloadButton = value;
            }
        }

        private RoleAbilityButtonBase reloadButton;
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
            ExtremeRoleId.AssaultMaster,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.AssaultMaster.ToString(),
            Palette.ImpostorRed,
            true, false, true, true)
        { }

        public void CreateAbility()
        {
            this.CreateNormalAbilityButton(
                Translation.GetString("reload"),
                Loader.CreateSpriteFromResources(
                    Path.AssaultMasterReload));
        }

        public void HockBodyReport(
            PlayerControl rolePlayer,
            GameData.PlayerInfo reporter,
            GameData.PlayerInfo reportBody)
        {
            addStock(this.addStockWhenReport);
        }

        public void HockReportButton(
            PlayerControl rolePlayer, GameData.PlayerInfo reporter)
        {
            addStock(this.addStockWhenMeetingButton);
        }

        public bool IsAbilityUse() => 
            this.IsCommonUse() &&
            this.stock > 0 &&
            PlayerControl.LocalPlayer.killTimer > 0;

        public void RoleAbilityResetOnMeetingEnd()
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

                this.reloadButton.SetAbilityCoolTime(
                    this.curReloadCoolTime);
                
                this.reloadButton.ResetCoolTimer();
            }
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            this.KillCoolTime = this.defaultKillCool;
            if (this.reduceKillCoolText != null)
            {
                this.reduceKillCoolText.gameObject.SetActive(false);
            }
        }

        public bool UseAbility()
        {
            this.reloadButton.SetAbilityCoolTime(
                this.defaultReloadCoolTime);

            this.curReloadCoolTime = this.defaultReloadCoolTime;

            float curKillCool = CachedPlayerControl.LocalPlayer.PlayerControl.killTimer;
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
                    FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText,
                    this.Button.GetTransform());
                this.reduceKillCoolText.enableWordWrapping = false;
                this.reduceKillCoolText.transform.localScale = Vector3.one * 0.5f;
                this.reduceKillCoolText.transform.localPosition += new Vector3(-0.05f, 0.60f, 0);
            }

            if (this.reduceKillCoolText == null) { return; }
            
            if (this.stock == 0)
            {
                this.reduceKillCoolText.text = 
                    Translation.GetString("noStockNow");
            }
            else
            {
                this.reduceKillCoolText.text = string.Format(
                    Translation.GetString("reduceKillCool"),
                    Mathf.CeilToInt(this.stock * this.reloadReduceTimePerStock));
            }

            this.reduceKillCoolText.gameObject.SetActive(true);

        }

        public override bool TryRolePlayerKillTo(
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
                this.Button.ResetCoolTimer();
            }

            return true;
        }

        public override string GetFullDescription() => string.Format(
            base.GetFullDescription(), this.stock,
            this.stockMax, this.curReloadCoolTime);

        protected override void CreateSpecificOption(CustomOptionBase parentOps)
        {
            this.CreateCommonAbilityOption(parentOps);

            CreateIntOption(
                AssaultMasterOption.StockLimit,
                2, 1, 10, 1, parentOps,
                format: OptionUnit.ScrewNum);
            CreateIntOption(
                AssaultMasterOption.StockNumWhenReport,
                1, 1, 5, 1, parentOps,
                format: OptionUnit.ScrewNum);
            CreateIntOption(
                AssaultMasterOption.StockNumWhenMeetingButton,
                3, 1, 10, 1, parentOps,
                format: OptionUnit.ScrewNum);
            CreateFloatOption(
                AssaultMasterOption.CockingKillCoolReduceTime,
                2.0f, 1.0f, 5.0f, 0.1f, parentOps,
                format: OptionUnit.Second);
            CreateFloatOption(
                AssaultMasterOption.ReloadReduceKillCoolTimePerStock,
                5.0f, 2.0f, 10.0f, 0.1f, parentOps,
                format: OptionUnit.Second);
            CreateBoolOption(
                AssaultMasterOption.IsResetReloadCoolTimeWhenKill,
                true, parentOps);
            CreateIntOption(
                AssaultMasterOption.ReloadCoolTimeReduceRatePerHideStock,
                75, 30, 90, 1, parentOps,
                format: OptionUnit.Percentage);
        }

        protected override void RoleSpecificInit()
        {
            var allOpt = OptionHolder.AllOption;

            this.stockMax = allOpt[
                GetRoleOptionId(AssaultMasterOption.StockLimit)].GetValue();
            this.addStockWhenReport = allOpt[
                GetRoleOptionId(AssaultMasterOption.StockNumWhenReport)].GetValue();
            this.addStockWhenMeetingButton = allOpt[
                GetRoleOptionId(AssaultMasterOption.StockNumWhenMeetingButton)].GetValue();
            this.cockingReduceTime = allOpt[
                GetRoleOptionId(AssaultMasterOption.CockingKillCoolReduceTime)].GetValue();
            this.reloadReduceTimePerStock = allOpt[
                GetRoleOptionId(AssaultMasterOption.ReloadReduceKillCoolTimePerStock)].GetValue();
            this.isResetCoolTimeWhenKill = allOpt[
                GetRoleOptionId(AssaultMasterOption.IsResetReloadCoolTimeWhenKill)].GetValue();
            this.timerReduceRate = 1.0f - (float)allOpt[
                GetRoleOptionId(AssaultMasterOption.ReloadCoolTimeReduceRatePerHideStock)].GetValue() / 100.0f;

            this.stock = 0;
            this.timerStock = 0;

            this.defaultReloadCoolTime = allOpt[
                GetRoleOptionId(RoleAbilityCommonOption.AbilityCoolTime)].GetValue();

            this.curReloadCoolTime = this.defaultReloadCoolTime;

            if (!this.HasOtherKillCool)
            {
                this.HasOtherKillCool = true;
                this.defaultKillCool = PlayerControl.GameOptions.KillCooldown;
            }
            else
            {
                this.defaultKillCool = this.KillCoolTime;
            }
            this.RoleAbilityInit();
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
}
