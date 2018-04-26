using System.Threading.Tasks;

namespace AspectCoreTest.Windsor.Fakes
{
    public interface ICacheService
    {
        [CacheInterceptor]
        Model Get(int id);

        [CacheInterceptor]
        Task<Model> GetAsync(int id);
    }
}
