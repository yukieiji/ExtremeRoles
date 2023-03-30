namespace ExtremeRoles.Roles.API.Interface;

public interface IRoleKillAnimationChecker
{
    public bool IsKillAnimating { get; protected set; }

    public static void SetKillAnimating<T>(
        T role, bool value) where T : IRoleKillAnimationChecker
    {
        if (role is null) { return; }

        role.IsKillAnimating = value;
    }
}
