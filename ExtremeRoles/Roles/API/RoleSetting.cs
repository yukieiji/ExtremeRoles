using ExtremeRoles.Module;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.API
{

    public enum ExtremeRoleType
    {
        Null = -2,
        Neutral = -1,
        Crewmate = 0,
        Impostor = 1
    }
    public enum RoleCommonOption
    {
        RoleNum = 20,
        SpawnRate,
        HasOtherVison,
        Vison,
        ApplyEnvironmentVisionEffect,
    }
    public enum KillerCommonOption
    {
        HasOtherKillRange = 25,
        KillRange,
        HasOtherKillCool,
        KillCoolDown,
    }

    abstract public class RoleOptionBase : IRoleOption
    {

        public bool CanKill = false;
        public int OptionIdOffset = 0;

        public int GetRoleOptionId(
            RoleCommonOption option) => GetRoleOptionId((int)option);

        public int GetRoleOptionId(
            KillerCommonOption option) => GetRoleOptionId((int)option);

        public int GetRoleOptionId(
            CombinationRoleCommonOption option) => GetRoleOptionId((int)option);

        public int GetRoleOptionId(int option) => this.OptionIdOffset + option;

        public int GetRoleOptionOffset() => this.OptionIdOffset;

        public void Initialize()
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
            
            if (this.CanKill)
            {
                CreateKillerOption(parentOps);
            }

            CreateSpecificOption(parentOps);
        }
        public void CreatRoleSpecificOption(
            CustomOptionBase parentOps,
            int optionIdOffset)
        {
            this.OptionIdOffset = optionIdOffset;
            CreateVisonOption(parentOps);
            
            if (this.CanKill)
            {
                CreateKillerOption(parentOps);
            }

            CreateSpecificOption(parentOps);
        }
        protected abstract void CreateKillerOption(
            CustomOptionBase parentOps);
        protected abstract CustomOptionBase CreateSpawnOption();

        protected abstract void CreateSpecificOption(
            CustomOptionBase parentOps);
        protected abstract void CreateVisonOption(
            CustomOptionBase parentOps);

        protected abstract void CommonInit();

        protected abstract void RoleSpecificInit();

    }
}
