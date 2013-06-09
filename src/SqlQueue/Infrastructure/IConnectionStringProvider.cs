namespace SqlQueue.Infrastructure
{
    public interface IConnectionStringProvider
    {
        string ConnectionString { get; }
    }
}