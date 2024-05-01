namespace ExtremeRoles.GameMode.Option.ShipGlobal;

public readonly record struct SpawnOption(
	bool EnableSpecialSetting = true,
	bool Skeld = false,
	bool MiraHq = false,
	bool Polus = false,
	bool AirShip = true,
	bool Fungle = false,
	bool IsAutoSelectRandom = false);
