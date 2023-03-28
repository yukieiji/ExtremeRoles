namespace ExtremeRoles.Module.AbilityBehavior.Interface;

public interface ICountBehavior
{
    public const string DefaultButtonCountText = "buttonCountText";
    public int AbilityCount { get; }

    public void SetAbilityCount(int newAbilityNum);

    public void SetButtonTextFormat(string newTextFormat);
}
