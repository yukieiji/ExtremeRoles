namespace ExtremeSkins.Module.Interface
{
    public interface ICustomCosmicData<T1>
        where T1 : CosmeticData
    {

        protected const int DisplayOrder = 99;

        public T1 Data { get; }

        public string Author { get; }
        public string Name { get; }

        public string Id { get; }

        public T1 GetData();

    }
}
