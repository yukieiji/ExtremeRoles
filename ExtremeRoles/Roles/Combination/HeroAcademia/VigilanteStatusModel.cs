using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Combination.HeroAcademia;

public class VigilanteStatusModel : IStatusModel
{
    public Vigilante.VigilanteCondition Condition { get; set; }
}
