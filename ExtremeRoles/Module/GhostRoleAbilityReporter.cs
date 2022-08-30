using System.Collections.Generic;
using System.Text;

using ExtremeRoles.GhostRoles;

namespace ExtremeRoles.Module
{
    public sealed class GhostRoleAbilityReporter
    {
        private HashSet<AbilityType> useAbility = new HashSet<AbilityType>();

        public GhostRoleAbilityReporter()
        {
            this.useAbility.Clear();
        }

        public void Clear()
        {
            this.useAbility.Clear();
        }
        public string CreateReport()
        {
            if (this.useAbility.Count == 0) { return string.Empty; }

            StringBuilder creater = new StringBuilder(this.useAbility.Count);
            foreach (AbilityType abilityType in this.useAbility)
            {
                creater.AppendLine(
                    Helper.Translation.GetString(
                        abilityType.ToString()));
            }

            return creater.ToString();
        }

        public void AddAbilityCall(AbilityType abilityType)
        {
            this.useAbility.Add(abilityType);
        }
    }
}
