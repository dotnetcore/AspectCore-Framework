using System;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;

namespace AspectCoreTest.LightInject
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AsyncIncreamentAttribute : AbstractInterceptorAttribute
    {
        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            await context.Invoke(next);
            await Task.Delay(100); // 此处模拟一个真.异步方法，用于测试线程上下文切换

            if (context.ReturnValue is Task<int> task)
            {
                var result = await task;
                context.ReturnValue = Task.FromResult(result + 1);
            }
            else if (context.ReturnValue is ValueTask<int> valueTask)
            {
                var result = await valueTask;
                context.ReturnValue = new ValueTask<int>(result + 1);
            }
            else if (context.ReturnValue is int result)
            {
                context.ReturnValue = result + 1;
            }
        }
    }
}