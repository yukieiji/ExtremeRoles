using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Crewmate.Fencer
{
    public class FencerStatusModel : IStatusModel
    {
        public bool IsCounter { get; set; } = false;
        public float Timer { get; set; } = 0.0f;
        public float MaxTime { get; }
        public bool CanKill { get; set; } = false;

        public FencerStatusModel(float maxTime)
        {
            MaxTime = maxTime;
        }
    }
}
