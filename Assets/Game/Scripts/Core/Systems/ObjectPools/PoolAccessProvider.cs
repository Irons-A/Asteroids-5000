namespace Core.Systems.ObjectPools
{
    public class PoolAccessProvider
    {
        private UniversalObjectPool _universalObjectPool;

        public void SetPool(UniversalObjectPool universalObjectPool)
        {
            _universalObjectPool = universalObjectPool;
        }

        public PoolableObject GetFromPool(PoolableObjectType type)
        {
            return _universalObjectPool.GetFromPool(type);
        }
    }
}
