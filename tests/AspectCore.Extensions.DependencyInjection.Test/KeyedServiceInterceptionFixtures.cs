using System.Threading.Tasks;
using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.DependencyInjection.Test
{
    public class KeyedCounterIncrementInterceptor : AbstractInterceptorAttribute
    {
        private readonly int _amount;

        public KeyedCounterIncrementInterceptor(int amount)
        {
            _amount = amount;
        }

        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            await context.Invoke(next);
            if (context.ReturnValue is int value)
            {
                context.ReturnValue = value + _amount;
            }
        }
    }

    [AspectCoreGenerateProxy(typeof(KeyedCounter))]
    public interface IKeyedCounter
    {
        int GetBase();
        int GetIntercepted();
    }

    public class KeyedCounter : IKeyedCounter
    {
        public int GetBase() => 1;

        [KeyedCounterIncrementInterceptor(100)]
        public int GetIntercepted() => 1;
    }
}
