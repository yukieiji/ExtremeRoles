namespace ExtremeRoles.Module
{
    public class NullableSingleton<T> where T : new()
    {
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new T();
                }
                return instance;
            }
        }
        public static bool IsExist => instance != null;

        private static T instance = default(T);

        public void Destroy()
        {
            instance = default(T);
        }
    }
}
