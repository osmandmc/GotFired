namespace GotFired.DAL
{
    public class GenericRepository<T> : BaseRepository<T> where T : class
    {
        public GenericRepository(GotFiredDbContext context) : base(context)
        {
        }
    }
}
