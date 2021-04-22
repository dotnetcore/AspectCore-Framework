using System;
using System.Reflection;

namespace AspectCore.Extensions.Reflection
{
    public static class ReflectorExtensions
    {
        #region Reflection

        /// <summary>
        /// 通过Type对象获取对应的TypeReflector对象
        /// </summary>
        /// <param name="type">类型对象</param>
        /// <returns>类型反射操作</returns>
        public static TypeReflector GetReflector(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            return TypeReflector.Create(type.GetTypeInfo());
        }

        /// <summary>
        /// 通过TypeInfo对象获取对应的TypeReflector对象
        /// </summary>
        /// <param name="typeInfo">类型对象</param>
        /// <returns>类型反射操作</returns>
        public static TypeReflector GetReflector(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            return TypeReflector.Create(typeInfo);
        }

        /// <summary>
        /// 通过ConstructorInfo对象获取对应的ConstructorReflector对象
        /// </summary>
        /// <param name="constructor">构造器对象</param>
        /// <returns>构造方法反射操作</returns>
        public static ConstructorReflector GetReflector(this ConstructorInfo constructor)
        {
            if (constructor == null)
            {
                throw new ArgumentNullException(nameof(constructor));
            }
            return ConstructorReflector.Create(constructor);
        }

        /// <summary>
        /// 通过FieldInfo对象获取对应的FieldReflector对象
        /// </summary>
        /// <param name="field">字段对象</param>
        /// <returns>字段反射操作</returns>
        public static FieldReflector GetReflector(this FieldInfo field)
        {
            if (field == null)
            {
                throw new ArgumentNullException(nameof(field));
            }
            return FieldReflector.Create(field);
        }

        /// <summary>
        /// 通过MethodInfo对象获取对应的通过Callvirt方式调用的MethodReflector对象
        /// </summary>
        /// <param name="method">方法对象</param>
        /// <returns>方法反射操作</returns>
        public static MethodReflector GetReflector(this MethodInfo method)
        {
            return GetReflector(method, CallOptions.Callvirt);
        }

        /// <summary>
        /// 通过MethodInfo对象和调用方式获取MethodReflector对象
        /// </summary>
        /// <param name="method">方法对象</param>
        /// <param name="callOption">调用方式</param>
        /// <returns>方法反射操作</returns>
        public static MethodReflector GetReflector(this MethodInfo method, CallOptions callOption)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            return MethodReflector.Create(method, callOption);
        }

        /// <summary>
        /// 通过PropertyInfo对象获取对应的通过Callvirt调用的PropertyReflector对象
        /// </summary>
        /// <param name="property">属性对象</param>
        /// <returns>属性反射操作</returns>
        public static PropertyReflector GetReflector(this PropertyInfo property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            return GetReflector(property, CallOptions.Callvirt);
        }

        /// <summary>
        /// 通过PropertyInfo对象和调用方式获取对应的PropertyReflector对象
        /// </summary>
        /// <param name="property">属性对象</param>
        /// <param name="callOption">调用方式</param>
        /// <returns>属性反射操作</returns>
        public static PropertyReflector GetReflector(this PropertyInfo property, CallOptions callOption)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            return PropertyReflector.Create(property, callOption);
        }

        /// <summary>
        /// 通过ParameterInfo对象获取对应的ParameterReflector对象
        /// </summary>
        /// <param name="parameterInfo">参数对象</param>
        /// <returns>参数反射操作</returns>
        public static ParameterReflector GetReflector(this ParameterInfo parameterInfo)
        {
            if (parameterInfo == null)
            {
                throw new ArgumentNullException(nameof(parameterInfo));
            }
            return ParameterReflector.Create(parameterInfo);
        }
        #endregion

        #region Reflectr

        /// <summary>
        /// 通过FieldReflector对象获取对应的FieldInfo对象
        /// </summary>
        /// <param name="reflector">字段反射操作</param>
        /// <returns>字段对象</returns>
        public static FieldInfo GetFieldInfo(this FieldReflector reflector) => reflector?.GetMemberInfo();

        /// <summary>
        /// 通过MethodReflector对象获取对应的MethodInfo对象
        /// </summary>
        /// <param name="reflector">方法反射操作</param>
        /// <returns>方法对象</returns>
        public static MethodInfo GetMethodInfo(this MethodReflector reflector) => reflector?.GetMemberInfo();

        /// <summary>
        /// 通过ConstructorReflector对象获取对应的ConstructorInfo对象
        /// </summary>
        /// <param name="reflector">构造方法反射操作</param>
        /// <returns>构造器对象</returns>
        public static ConstructorInfo GetConstructorInfo(this ConstructorReflector reflector) => reflector?.GetMemberInfo();

        /// <summary>
        /// 通过PropertyReflector对象获取对应的PropertyInfo对象
        /// </summary>
        /// <param name="reflector">属性反射操作</param>
        /// <returns>属性对象</returns>
        public static PropertyInfo GetPropertyInfo(this PropertyReflector reflector) => reflector?.GetMemberInfo();

        #endregion
    }
}
