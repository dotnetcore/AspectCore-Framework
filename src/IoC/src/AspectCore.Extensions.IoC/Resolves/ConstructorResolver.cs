using System;
using AspectCore.Abstractions;
using AspectCore.Extensions.Reflection;

namespace AspectCore.Extensions.IoC.Resolves
{
    internal sealed class ConstructorResolver
    {
        private readonly ConstructorReflector _reflector;
        private readonly Func<IServiceResolver, object>[] _parameterFactories;

        public ConstructorResolver(Func<IServiceResolver, object>[] parameterFactories, ConstructorReflector reflector)
        {
            _parameterFactories = parameterFactories;
            _reflector = reflector;
        }

        public object Resolve(IServiceResolver resolver)
        {
            var length = _parameterFactories.Length;
            if (length == 0)
            {
                return _reflector.Invoke();
            }
            var parameters = new object[length];
            for (var i = 0; i < length; i++)
            {
                parameters[i] = _parameterFactories[i](resolver);
            }
            return _reflector.Invoke(parameters);
        }
    }
}