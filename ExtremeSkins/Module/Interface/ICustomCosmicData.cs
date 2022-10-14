namespace ExtremeSkins.Module.Interface
{
    public interface ICustomCosmicData<T>
        where T : CosmeticData
    {
        public T Data { get; }

        public string Author { get; }
        public string Name { get; }

        public string Id { get; }

        public T GetData();
    }
}
