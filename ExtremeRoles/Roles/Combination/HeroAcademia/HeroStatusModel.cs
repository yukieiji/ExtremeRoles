using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Combination.HeroAcademia
{
    public class HeroStatusModel : IStatusModel
    {
        public Hero.OneForAllCondition cond { get; set; }
    }
}
