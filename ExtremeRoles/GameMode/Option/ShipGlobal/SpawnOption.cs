namespace ExtremeRoles.GameMode.Option.ShipGlobal;

public sealed class SpawnOption
{
	public bool IsEnableRandom { get; init; } = true;

	public bool Skeld   { get; init; } = false;
	public bool MiraHq  { get; init; } = false;
	public bool Polus   { get; init; } = false;
	public bool AirShip { get; init; } = true;
	public bool Fungle  { get; init; } = false;

	public bool IsAutoSelectRandom { get; init; } = false;
}
