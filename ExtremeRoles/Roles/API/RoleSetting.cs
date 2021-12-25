using ExtremeRoles.Module;

namespace ExtremeRoles.Roles.API
{

    public enum ExtremeRoleType
    {
        Null = -2,
        Neutral = -1,
        Crewmate = 0,
        Impostor = 1
    }
    public enum RoleCommonSetting
    {
        RoleNum = 15,
        SpawnRate = 16,
        HasOtherVison = 17,
        Vison = 18,
        ApplyEnvironmentVisionEffect = 19,
    }
    public enum KillerCommonSetting
    {
        HasOtherKillRange = 11,
        KillRange = 12,
        HasOtherKillCool = 13,
        KillCoolDown = 14,
    }
    public enum CombinationRoleCommonSetting
    {
        IsMultiAssign = 11,
    }

    abstract public class RoleSettingBase
    {

        public bool CanKill = false;
        protected int OptionIdOffset = 0;


        public int GetRoleSettingId(
            RoleCommonSetting setting) => GetRoleSettingId((int)setting);

        public int GetRoleSettingId(
            KillerCommonSetting setting) => GetRoleSettingId((int)setting);

        public int GetRoleSettingId(
            CombinationRoleCommonSetting setting) => GetRoleSettingId((int)setting);

        public int GetRoleSettingId(int setting) => this.OptionIdOffset + setting;

        public void GameInit()
        {
            CommonInit();
            RoleSpecificInit();
        }

        public void CreateRoleAllOption(
            int optionIdOffset)
        {
            this.OptionIdOffset = optionIdOffset;
            var parentOps = CreateSpawnOption();
            CreateVisonOption(parentOps);
            CreateSpecificOption(parentOps);
            if (this.CanKill)
            {
                CreateKillerOption(parentOps);
            }
        }
        public void CreatRoleSpecificOption(
            CustomOption parentOps,
            int optionIdOffset)
        {
            this.OptionIdOffset = optionIdOffset;
            CreateVisonOption(parentOps);
            CreateSpecificOption(parentOps);
            if (this.CanKill)
            {
                CreateKillerOption(parentOps);
            }
        }
        protected abstract void CreateKillerOption(
            CustomOption parentOps);
        protected abstract CustomOption CreateSpawnOption();

        protected abstract void CreateSpecificOption(
            CustomOption parentOps);
        protected abstract void CreateVisonOption(
            CustomOption parentOps);

        protected abstract void CommonInit();

        protected abstract void RoleSpecificInit();

    }
}
