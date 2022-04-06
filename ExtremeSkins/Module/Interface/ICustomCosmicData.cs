namespace ExtremeSkins.Module.Interface
{
    public interface ICustomCosmicData<T1, T2>
        where T1 : CosmeticData
        where T2 : class
    {

        protected const int DisplayOrder = 99;

        public T1 Data { get; }
        public T2 ViewData { get; }

        public string Author { get; }
        public string Name { get; }

        public string Id { get; }

        public bool AllDataLoaded { get; }

        public T1 GetData();

        public void LoadAditionalData();

    }
}
