using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.Extensions.Reflection;
using AspectCore.Utils;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 拦截上下文，描述被拦截方法的相关信息
    /// </summary>
    [NonAspect]
    internal sealed class RuntimeAspectContext : AspectContext,IDisposable
    {
        private volatile IDictionary<string, object> _data;
        private IServiceProvider _serviceProvider;
        private MethodInfo _implementationMethod;
        private object _implementation;
        private bool _disposedValue = false;

        /// <summary>
        /// IServiceProvider
        /// </summary>
        public override IServiceProvider ServiceProvider
        {
            get
            {
                if (_serviceProvider == null)
                {
                    throw new NotSupportedException("The current context does not support IServiceProvider.");
                }

                return _serviceProvider;
            }
        }

        /// <summary>
        /// 附加数据
        /// </summary>
        public override IDictionary<string, object> AdditionalData
        {

            get
            {
                if (_data == null)
                {
                    _data = new Dictionary<string, object>();
                }
                return _data;
            }
        }

        /// <summary>
        /// 被拦截的方法的返回值
        /// </summary>
        public override object ReturnValue { get; set; }

        /// <summary>
        /// 暴露服务中的方法，一般指代接口
        /// </summary>
        public override MethodInfo ServiceMethod { get; }

        /// <summary>
        /// 被拦截的方法的参数
        /// </summary>
        public override object[] Parameters { get; }

        /// <summary>
        /// 生成的代理方法
        /// </summary>
        public override MethodInfo ProxyMethod { get; }

        /// <summary>
        /// 代理对象
        /// </summary>
        public override object Proxy { get; }

        /// <summary>
        /// 目标对象的方法
        /// </summary>
        public override MethodInfo ImplementationMethod => _implementationMethod;

        /// <summary>
        /// 目标对象
        /// </summary>
        public override object Implementation => _implementation;

        /// <summary>
        /// 拦截上下文
        /// </summary>
        /// <param name="serviceProvider">IServiceProvider</param>
        /// <param name="serviceMethod">服务方法</param>
        /// <param name="targetMethod">目标方法</param>
        /// <param name="proxyMethod">代理方法</param>
        /// <param name="targetInstance">目标对象</param>
        /// <param name="proxyInstance">代理实例</param>
        /// <param name="parameters">目标方法的参数</param>
        public RuntimeAspectContext(
            IServiceProvider serviceProvider, MethodInfo serviceMethod, MethodInfo targetMethod, MethodInfo proxyMethod,
            object targetInstance, object proxyInstance, object[] parameters)
        {
            _serviceProvider = serviceProvider;
            _implementationMethod = targetMethod;
            _implementation = targetInstance;
            ServiceMethod = serviceMethod;
            ProxyMethod = proxyMethod;
            Proxy = proxyInstance;
            Parameters = parameters;
        }

        /// <summary>
        /// 设置异步完成
        /// </summary>
        /// <returns>异步任务</returns>
        public override async Task Complete()
        {
            if (_implementation == null || _implementationMethod == null)
            {
                await Break();
                return;
            }
            var reflector = AspectContextRuntimeExtensions.reflectorTable.GetOrAdd(_implementationMethod, method => method.GetReflector(method.IsCallvirt() ? CallOptions.Callvirt : CallOptions.Call));
            var returnValue = reflector.Invoke(_implementation, Parameters);
            await this.AwaitIfAsync(returnValue);
            ReturnValue = returnValue;
        }

        /// <summary>
        /// 设置异步跳出
        /// </summary>
        /// <returns>异步任务</returns>
        public override Task Break()
        {
            if (ReturnValue == null)
            {
                ReturnValue = ServiceMethod.ReturnParameter.ParameterType.GetDefaultValue();
            }
            return TaskUtils.CompletedTask;
        }

        /// <summary>
        /// 拦截逻辑
        /// </summary>
        /// <param name="next">后续处理者</param>
        /// <returns>异步任务</returns>
        public override Task Invoke(AspectDelegate next)
        {
            return next(this);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_disposedValue)
            {
                return;
            }

            if (_data == null)
            {
                _disposedValue = true;
                return;
            }

            foreach (var key in _data.Keys.ToArray())
            {
                _data.TryGetValue(key, out object value);

                var disposable = value as IDisposable;

                disposable?.Dispose();

                _data.Remove(key);
            }

            _disposedValue = true;
        }
    }
}