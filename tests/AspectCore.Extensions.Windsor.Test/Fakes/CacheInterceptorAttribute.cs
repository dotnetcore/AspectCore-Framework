using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.Parameters;

namespace AspectCoreTest.Windsor.Fakes
{
    public class CacheInterceptorAttribute : AbstractInterceptorAttribute
    {
        private readonly IDictionary<int, Model> _cache = new Dictionary<int, Model>();

        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            var parameters = context.GetParameters();
            if (parameters.Any())
            {
                if (int.TryParse(parameters[0].Value.ToString(), out int id))
                {
                    if (_cache.TryGetValue(id, out var result))
                    {
                        var returnType = context.ServiceMethod.ReturnType;
                        if (returnType == typeof(Task<Model>))
                        {
                            context.ReturnValue = Task.FromResult(result);
                        }
                        else if (returnType == typeof(Model))
                        {
                            context.ReturnValue = result;
                        }
                        return;
                    }
                }
            }
            await next(context);
            if (context.ReturnValue is Model value)
            {
                _cache[value.Id] = value;
            }
            else if(context.ReturnValue is Task<Model> task && task.IsCompleted)
            {
                var obj = await task;
                _cache[obj.Id] = obj;
            }
        }
    }
}
