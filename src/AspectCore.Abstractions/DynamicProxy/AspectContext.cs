using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 拦截上下文，描述被拦截方法的相关信息
    /// </summary>
    [NonAspect]
    public abstract class AspectContext
    {
        /// <summary>
        /// 附加数据
        /// </summary>
        public abstract IDictionary<string, object> AdditionalData { get; }

        /// <summary>
        /// 被拦截的方法的返回值
        /// </summary>
        public abstract object ReturnValue { get; set; }

        /// <summary>
        /// IServiceProvider
        /// </summary>
        public abstract IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// 暴露服务中的方法，一般指代接口
        /// </summary>
        public abstract MethodInfo ServiceMethod { get; }

        /// <summary>
        /// 实现对象
        /// </summary>
        public abstract object Implementation { get; }

        /// <summary>
        /// 实现对象对应的方法
        /// </summary>
        public abstract MethodInfo ImplementationMethod { get; }

        /// <summary>
        /// 被拦截的方法的参数
        /// </summary>
        public abstract object[] Parameters { get; }

        /// <summary>
        /// 生成的代理方法
        /// </summary>
        public abstract MethodInfo ProxyMethod { get; }

        /// <summary>
        /// 代理类
        /// </summary>
        public abstract object Proxy { get; }

        /// <summary>
        /// 设置跳出
        /// </summary>
        /// <returns>异步任务</returns>
        public abstract Task Break();

        /// <summary>
        /// 拦截逻辑
        /// </summary>
        /// <param name="next">后续处理者</param>
        /// <returns>异步任务</returns>
        public abstract Task Invoke(AspectDelegate next);

        /// <summary>
        /// 设置完成
        /// </summary>
        /// <returns>异步任务</returns>
        public abstract Task Complete();
    }
}