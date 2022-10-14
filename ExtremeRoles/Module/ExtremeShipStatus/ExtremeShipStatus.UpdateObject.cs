using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.CustomMonoBehaviour;

namespace ExtremeRoles.Module.ExtremeShipStatus
{
    public sealed partial class ExtremeShipStatus
    {
        private HostObjectUpdater updater;

        public void AddUpdateObject(IUpdatableObject obj)
        {
            updater.AddObject(obj);
        }

        public void RemoveUpdateObjectAt(int index)
        {
            updater.RemoveObject(index);
        }

        public IUpdatableObject GetUpdateObject(int index) => updater.GetObject(index);

        private void resetUpdateObject()
        {
            this.updater = this.status.AddComponent<HostObjectUpdater>();
        }
    }
}
