public interface IObjectPoolable
{
    public PoolableObjectTypes PoolableObjectType();
    public void OnReturnToPool();
}
