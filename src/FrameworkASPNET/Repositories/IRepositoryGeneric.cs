using System.Data.Entity;

namespace FrameworkAspNetExtended.Repositories
{
    public interface IRepositoryGeneric
    {
        DbContext Context { get; }
    }
}
