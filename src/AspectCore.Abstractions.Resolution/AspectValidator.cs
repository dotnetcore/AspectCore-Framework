using AspectCore.Abstractions.Extensions;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace AspectCore.Abstractions.Resolution
{
    public sealed class AspectValidator : IAspectValidator
    {
        private static readonly ConcurrentDictionary<MethodInfo, bool> DetectorCache= new ConcurrentDictionary<MethodInfo, bool>();

        private readonly IAspectConfiguration aspectConfiguration;

        public AspectValidator(IAspectConfiguration aspectConfiguration)
        {
            if (aspectConfiguration == null)
            {
                throw new ArgumentNullException(nameof(aspectConfiguration));
            }

            this.aspectConfiguration = aspectConfiguration;
        }

        public bool Validate(MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            return DetectorCache.GetOrAdd(method, ValidateCache);
        }

        private bool ValidateCache(MethodInfo method)
        {
            var declaringType = method.DeclaringType.GetTypeInfo();

            if (ValidateDynamically(declaringType) || ValidatePropertyMethod(method))
            {
                return false;
            }

            if (ValidateNonAspect(method) || ValidateNonAspect(declaringType))
            {
                return false;
            }

            if (!ValidateDeclaringType(declaringType) || !ValidateDeclaringMethod(method))
            {
                return false;
            }

            if (ValidateIgnoredList(aspectConfiguration.GetConfigurationOption<bool>(), method))
            {
                return false;
            }

            return ValidateInterceptor(method) || ValidateInterceptor(declaringType) || ValidateInterceptor(aspectConfiguration.GetConfigurationOption<IInterceptor>(), method);
        }

        private bool ValidateDeclaringType(TypeInfo declaringType)
        {
            return !(declaringType.IsNotPublic || declaringType.IsValueType || declaringType.IsSealed);
        }

        private bool ValidateDeclaringMethod(MethodInfo method)
        {
            return !method.IsStatic && !method.IsFinal && method.IsVirtual && (method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly);
        }

        private bool ValidateDynamically(TypeInfo typeInfo)
        {
            return typeInfo.IsDynamically();
        }

        private bool ValidateNonAspect(MemberInfo member)
        {
            return member.IsDefined(typeof(NonAspectAttribute), true);
        }

        private bool ValidatePropertyMethod(MethodInfo method)
        {
            return method.IsPropertyMethod();
        }

        private bool ValidateIgnoredList(IConfigurationOption<bool> ignores, MethodInfo method)
        {
            return ignores.Any(configure => configure(method));
        }

        private bool ValidateInterceptor(MemberInfo member)
        {
            return member.CustomAttributes.Any(data => typeof(IInterceptor).GetTypeInfo().IsAssignableFrom(data.AttributeType.GetTypeInfo()));
        }

        private bool ValidateInterceptor(IConfigurationOption<IInterceptor> aspectConfiguration, MethodInfo method)
        {
            return aspectConfiguration.Any(config => config(method) != null);
        }
    }
}
