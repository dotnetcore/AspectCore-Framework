using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Extensions.Reflection.Internals
{
    internal static class MethodInfoConstant
    {
        ///<summary>
        /// 一个MethodInfo对象,方法通过RuntimeTypeHandle获取对应的类型引用
        /// </summary>
        ///<remarks>
        /// Type 是一个表示类型的抽象类；RuntimeType 是 Type 针对载入类型信息的具体实现；RuntimeTypeHandle 则是类型唯一的抽象句柄；RuntimeMethodHandle 是方法的内部元数据表示形式的句柄
        /// </remarks>
        /// <see cref="https://docs.microsoft.com/zh-cn/dotnet/api/system.type.gettypefromhandle?view=netcore-3.1#System_Type_GetTypeFromHandle_System_RuntimeTypeHandle_"/>
        internal static readonly MethodInfo GetTypeFromHandle = InternalExtensions.GetMethod<Func<RuntimeTypeHandle, Type>>(handle => Type.GetTypeFromHandle(handle));

        ///<summary>
        /// 一个MethodInfo对象,方法通过RuntimeTypeHandle，RuntimeMethodHandle获取对应的方法引用
        /// </summary>
        ///<remarks>
        /// Type 是一个表示类型的抽象类；RuntimeType 是 Type 针对载入类型信息的具体实现；RuntimeTypeHandle 则是类型唯一的抽象句柄；RuntimeMethodHandle 是方法的内部元数据表示形式的句柄
        /// </remarks>
        /// <see cref="https://docs.microsoft.com/zh-cn/dotnet/api/system.reflection.methodbase.getmethodfromhandle?view=netcore-3.1#System_Reflection_MethodBase_GetMethodFromHandle_System_RuntimeMethodHandle_System_RuntimeTypeHandle_"/>
        internal static readonly MethodInfo GetMethodFromHandle = InternalExtensions.GetMethod<Func<RuntimeMethodHandle, RuntimeTypeHandle, MethodBase>>((h1, h2) => MethodBase.GetMethodFromHandle(h1, h2));

        /// <summary>
        /// ArgumentNullException类型的构造器
        /// </summary>
        internal static readonly ConstructorInfo ArgumentNullExceptionCtor = typeof(ArgumentNullException).GetTypeInfo().GetConstructor(new Type[] { typeof(string) });

        /// <summary>
        /// object类型的构造器
        /// </summary>
        internal static readonly ConstructorInfo ObjectCtor = typeof(object).GetTypeInfo().DeclaredConstructors.Single();
    }
}
