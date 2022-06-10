using UnityEngine;

namespace ExtremeRoles.Compat.Interface
{
	enum SystemConsoleType
	{
		SecurityCamera,
		Vital,
	}

	internal interface IMapMod
    {
		public ShipStatus.MapType MapType { get; }
		public bool IsCustomSabotageNow();
		public bool IsCustomSabotageTask(TaskTypes saboTask);
		public void RepairCustomSabotage();
		public void RepairCustomSabotage(TaskTypes saboTask);
		public Console GetConsole(TaskTypes task);
		public SystemConsole GetSystemConsole(SystemConsoleType sysConsole);
		public Sprite SystemConsoleUseSprite(SystemConsoleType sysConsole);

	}
}
