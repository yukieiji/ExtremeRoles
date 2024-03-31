namespace ExtremeRoles.Module.Ability.Behavior.Interface;

public interface IChargingBehavior
{
	public float ChargeGage { get; set; }
	public float ChargeTime { get; set; }
	public bool IsCharging { get;}
}
