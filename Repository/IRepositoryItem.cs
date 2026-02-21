namespace RepositoryApp.Repository
{
    /// <summary>
    /// Generic repository item that supports strong typing
    /// </summary>
    /// <typeparam name="TContent">Type of the content being stored</typeparam>
    public interface IRepositoryItem<TContent>
    {
        TContent Content { get; set; }
        int Type { get; set; }
    }

    /// <summary>
    /// Concrete implementation of repository item
    /// </summary>
    /// <typeparam name="TContent">Type of the content being stored</typeparam>
    public class RepositoryItem<TContent> : IRepositoryItem<TContent>
    {
        public TContent Content { get; set; }
        public int Type { get; set; }
    }
}
