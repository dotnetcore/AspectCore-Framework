using System.Reflection;

namespace AspectCore.Abstractions.Internal
{
    public sealed class AspectValidator : IAspectValidator
    {
        private readonly AspectValidationDelegate _aspectValidationDelegate;

        public AspectValidator(AspectValidationDelegate aspectValidationDelegate)
        {
            _aspectValidationDelegate = aspectValidationDelegate;
        }

        public bool Validate(MethodInfo method)
        {
            if (method == null)
            {
                return false;
            }

            return _aspectValidationDelegate(method);
        }

      
        //private static readonly ConcurrentDictionary<MethodInfo, bool> detectorCache = new ConcurrentDictionary<MethodInfo, bool>();

        //private readonly IAspectConfigure _aspectConfigure;

        //public AspectValidator(IAspectConfigure aspectConfigure)
        //{
        //    if (aspectConfigure == null)
        //    {
        //        throw new ArgumentNullException(nameof(aspectConfigure));
        //    }

        //    this._aspectConfigure = aspectConfigure;
        //}

        //public bool Validate(MethodInfo method)
        //{
        //    if (method == null)
        //    {
        //        throw new ArgumentNullException(nameof(method));
        //    }
        //    return detectorCache.GetOrAdd(method, ValidateCache);
        //}

        //private bool ValidateCache(MethodInfo method)
        //{
        //    var declaringType = method.DeclaringType.GetTypeInfo();

        //    if (declaringType.IsDynamically() || method.IsPropertyBinding())
        //    {
        //        return false;
        //    }

        //    if (IsNonAspect(method) || IsNonAspect(declaringType))
        //    {
        //        return false;
        //    }

        //    if (!IsAccessibility(declaringType) || !IsAccessibility(method))
        //    {
        //        return false;
        //    }

        //    if (IsIgnored(_aspectConfigure.GetConfigureOption<bool>(), method))
        //    {
        //        return false;
        //    }

        //    return HasInterceptor(method) || HasInterceptor(declaringType) || HasInterceptor<IInterceptor>(_aspectConfigure.GetConfigureOption<IInterceptor>(), method);
        //}

        //public static bool IsAccessibility(TypeInfo declaringType)
        //{
        //    return !(declaringType.IsNotPublic || declaringType.IsValueType || declaringType.IsSealed);
        //}

        //public static bool IsAccessibility(MethodInfo method)
        //{
        //    return !method.IsStatic && !method.IsFinal && method.IsVirtual && (method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly);
        //}

        //public static bool IsNonAspect(MemberInfo member)
        //{
        //    return member.IsDefined(typeof(NonAspectAttribute), true);
        //}

        //public static bool IsIgnored(IAspectConfigureOption<bool> ignores, MethodInfo method)
        //{
        //    return ignores.Any(configure => configure(method));
        //}

        //public static bool HasInterceptor(MemberInfo member)
        //{
        //    return member.CustomAttributes.Any(data => typeof(IInterceptor).GetTypeInfo().IsAssignableFrom(data.AttributeType.GetTypeInfo()));
        //}

        //public static bool HasInterceptor<T>(IAspectConfigureOption<IInterceptor> aspectConfigure, MethodInfo method)
        //    where T : class, IInterceptor
        //{
        //    return aspectConfigure.Any(config => (config(method) as T) != null);
        //}
    }
}