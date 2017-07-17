using AspectCore.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Extensions.Test.Fakes
{
    public class CacheInterceptorAttribute : InterceptorAttribute
    {
        private readonly IDictionary<int, Model> cache = new Dictionary<int, Model>();

        public async override Task Invoke(AspectContext context, AspectDelegate next)
        {
            var parameters = context.Parameters;
            if (parameters.Any())
            {
                var id = default(int);
                if (int.TryParse(parameters[0].Value.ToString(), out id))
                {
                    var result = default(Model);
                    if (cache.TryGetValue(id, out result))
                    {
                        context.ReturnParameter.Value = result;
                        return;
                    }
                }
            }
            await next(context);
            var value = context.ReturnParameter.Value as Model;
            if (value != null)
            {
                cache[value.Id] = value;
            }
        }
    }
}
