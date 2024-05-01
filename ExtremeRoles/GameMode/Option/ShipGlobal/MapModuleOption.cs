namespace ExtremeRoles.GameMode.Option.ShipGlobal;

public enum AirShipAdminMode
{
	ModeBoth,
	ModeCockpitOnly,
	ModeArchiveOnly
}

public enum PolusVitalPos
{
	DefaultKey,
	SpecimenKey,
	LaboratoryKey
}

public readonly record struct AdminOption(
	bool Disable = false,
	AirShipAdminMode AirShipEnable = AirShipAdminMode.ModeBoth,
	bool EnableAdminLimit = false,
	float AdminLimitTime = float.MaxValue);

public readonly record struct SecurityOption(
	bool Disable = false,
	bool EnableSecurityLimit = false,
	float SecurityLimitTime = float.MaxValue);

public readonly record struct VitalOption(
	bool Disable = false,
	bool EnableVitalLimit = false,
	float VitalLimitTime = float.MaxValue,
	PolusVitalPos PolusPos = PolusVitalPos.DefaultKey);