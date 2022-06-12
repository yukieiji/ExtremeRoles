using UnityEngine;

namespace ExtremeRoles.Compat.Interface
{
	public enum SystemConsoleType
	{
		SecurityCamera,
		Vital,
	}

	public enum MapRpcCall : byte
    {
		RepairAllSabo,
		RepairCustomSaboType
    }

	public enum CustomMonoBehaviourType
    {
		MovableFloorBehaviour
    }

	public interface IMapMod
    {
		public const byte RpcCallType = 1;
		public ShipStatus.MapType MapType { get; }
		public bool CanPlaceCamera { get; }
		public void Awake();
		public bool IsCustomSabotageNow();
		public bool IsCustomSabotageTask(TaskTypes saboTask);

		public void RpcRepairCustomSabotage();
		public void RpcRepairCustomSabotage(TaskTypes saboTask);
		public void RepairCustomSabotage();
		public void RepairCustomSabotage(TaskTypes saboTask);
		public Console GetConsole(TaskTypes task);
		public SystemConsole GetSystemConsole(SystemConsoleType sysConsole);
		public void AddCustomComponent(
			GameObject addObject, CustomMonoBehaviourType customMonoType);

		public void SetUpNewCamera(SurvCamera camera);

	}
}
