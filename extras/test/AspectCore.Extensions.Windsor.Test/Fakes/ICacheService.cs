using System.Threading.Tasks;

namespace AspectCore.Extensions.Windsor.Test.Fakes
{
    public interface ICacheService
    {
        [CacheInterceptor]
        Model Get(int id);

        [CacheInterceptor]
        Task<Model> GetAsync(int id);
    }
}
