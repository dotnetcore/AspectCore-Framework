using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Abstractions.Extensions
{
    public sealed class PropertyAccessor
    {
        private delegate TValue ByRefFunc<TDeclaringType, TValue>(ref TDeclaringType arg);

        private static readonly MethodInfo CallPropertyGetterOpenGenericMethod =
            typeof(PropertyAccessor).GetTypeInfo().GetDeclaredMethod(nameof(CallPropertyGetter));

        private static readonly MethodInfo CallPropertyGetterByReferenceOpenGenericMethod =
            typeof(PropertyAccessor).GetTypeInfo().GetDeclaredMethod(nameof(CallPropertyGetterByReference));

        private static readonly MethodInfo CallNullSafePropertyGetterOpenGenericMethod =
            typeof(PropertyAccessor).GetTypeInfo().GetDeclaredMethod(nameof(CallNullSafePropertyGetter));

        private static readonly MethodInfo CallNullSafePropertyGetterByReferenceOpenGenericMethod =
            typeof(PropertyAccessor).GetTypeInfo().GetDeclaredMethod(nameof(CallNullSafePropertyGetterByReference));

        private static readonly MethodInfo CallPropertySetterOpenGenericMethod =
            typeof(PropertyAccessor).GetTypeInfo().GetDeclaredMethod(nameof(CallPropertySetter));

        private readonly PropertyInfo property;

        public PropertyAccessor(PropertyInfo property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            this.property = property;
        }

        /// <summary>

        public Func<object, object> MakeFastPropertyGetter(PropertyInfo propertyInfo)
        {
            Debug.Assert(propertyInfo != null);

            return MakeFastPropertyGetter(
                propertyInfo,
                CallPropertyGetterOpenGenericMethod,
                CallPropertyGetterByReferenceOpenGenericMethod);
        }

        private Func<object, object> MakeFastPropertyGetter( PropertyInfo propertyInfo,
            MethodInfo propertyGetterWrapperMethod,
            MethodInfo propertyGetterByRefWrapperMethod)
        {
            var getMethod = propertyInfo.GetMethod;

            if (getMethod.DeclaringType.GetTypeInfo().IsValueType)
            {
                return MakeFastPropertyGetter(
                    typeof(ByRefFunc<,>),
                    getMethod,
                    propertyGetterByRefWrapperMethod);
            }
            else
            {
                return MakeFastPropertyGetter(
                    typeof(Func<,>),
                    getMethod,
                    propertyGetterWrapperMethod);
            }
        }

        private static Func<object, object> MakeFastPropertyGetter(Type openGenericDelegateType, MethodInfo propertyGetMethod, MethodInfo openGenericWrapperMethod)
        {
            var typeInput = propertyGetMethod.DeclaringType;
            var typeOutput = propertyGetMethod.ReturnType;

            var delegateType = openGenericDelegateType.MakeGenericType(typeInput, typeOutput);
            var propertyGetterDelegate = propertyGetMethod.CreateDelegate(delegateType);

            var wrapperDelegateMethod = openGenericWrapperMethod.MakeGenericMethod(typeInput, typeOutput);
            var accessorDelegate = wrapperDelegateMethod.CreateDelegate(
                typeof(Func<object, object>),
                propertyGetterDelegate);

            return (Func<object, object>)accessorDelegate;
        }

        private  Action<object, object> MakeFastPropertySetter(PropertyInfo propertyInfo)
        {

            var setMethod = propertyInfo.SetMethod;
            var parameters = setMethod.GetParameters();

            var typeInput = setMethod.DeclaringType;
            var parameterType = parameters[0].ParameterType;

            var propertySetterAsAction =
                setMethod.CreateDelegate(typeof(Action<,>).MakeGenericType(typeInput, parameterType));
            var callPropertySetterClosedGenericMethod =
                CallPropertySetterOpenGenericMethod.MakeGenericMethod(typeInput, parameterType);
            var callPropertySetterDelegate =
                callPropertySetterClosedGenericMethod.CreateDelegate(
                    typeof(Action<object, object>), propertySetterAsAction);

            return (Action<object, object>)callPropertySetterDelegate;
        }


        private static object CallPropertyGetter<TDeclaringType, TValue>(
            Func<TDeclaringType, TValue> getter,
            object target)
        {
            return getter((TDeclaringType)target);
        }

        private static object CallPropertyGetterByReference<TDeclaringType, TValue>(
            ByRefFunc<TDeclaringType, TValue> getter,
            object target)
        {
            var unboxed = (TDeclaringType)target;
            return getter(ref unboxed);
        }

        private static object CallNullSafePropertyGetter<TDeclaringType, TValue>(
            Func<TDeclaringType, TValue> getter,
            object target)
        {
            if (target == null)
            {
                return null;
            }

            return getter((TDeclaringType)target);
        }


        private static object CallNullSafePropertyGetterByReference<TDeclaringType, TValue>(
            ByRefFunc<TDeclaringType, TValue> getter,
            object target)
        {
            if (target == null)
            {
                return null;
            }

            var unboxed = (TDeclaringType)target;
            return getter(ref unboxed);
        }

        private static void CallPropertySetter<TDeclaringType, TValue>(
            Action<TDeclaringType, TValue> setter,
            object target,
            object value)
        {
            setter((TDeclaringType)target, (TValue)value);
        }
    }
}
