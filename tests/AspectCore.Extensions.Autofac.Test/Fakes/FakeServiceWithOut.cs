using System.Globalization;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.Test.Fakes
{
    public class FakeServiceWithOutInterceptor : AbstractInterceptorAttribute
    {
        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            await next(context);
        }
    }

    [FakeServiceWithOutInterceptor]
    public interface IFakeServiceWithOut
    {
        bool OutDecimal(out decimal num);

        bool OutInt(out int num);
    }

    public class FakeServiceWithOut : IFakeServiceWithOut
    {
        public bool OutDecimal(out decimal num)
        {
            num = 1.0M;
            return true;
        }

        public bool OutInt(out int num)
        {
            num = 1;
            return true;
        }
    }
}