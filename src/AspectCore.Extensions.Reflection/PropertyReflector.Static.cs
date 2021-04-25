using System;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Extensions.Reflection.Emit;

namespace AspectCore.Extensions.Reflection
{
    public partial class PropertyReflector
    {
        /// <summary>
        /// 静态属性反射操作
        /// </summary>
        private class StaticPropertyReflector : PropertyReflector
        {
            /// <summary>
            /// 静态属性反射操作
            /// </summary>
            /// <param name="reflectionInfo">属性对象</param>
            public StaticPropertyReflector(PropertyInfo reflectionInfo)
                : base(reflectionInfo)
            {
            }

            /// <summary>
            /// 创建一个代表属性get访问器方法的委托
            /// </summary>
            /// <returns>代表属性get访问器方法的委托</returns>
            protected override Func<object, object> CreateGetter()
            {
                var dynamicMethod = new DynamicMethod($"getter-{Guid.NewGuid()}", typeof(object), new Type[] { typeof(object) }, _reflectionInfo.Module, true);
                var ilGen = dynamicMethod.GetILGenerator();
                ilGen.Emit(OpCodes.Call, _reflectionInfo.GetMethod);
                if (_reflectionInfo.PropertyType.GetTypeInfo().IsValueType)
                    ilGen.EmitConvertToObject(_reflectionInfo.PropertyType);
                ilGen.Emit(OpCodes.Ret);
                return (Func<object, object>)dynamicMethod.CreateDelegate(typeof(Func<object, object>));
            }

            /// <summary>
            /// 创建一个代表属性set访问器方法的委托
            /// </summary>
            /// <returns>代表属性set访问器方法的委托</returns>
            protected override Action<object, object> CreateSetter()
            {
                var dynamicMethod = new DynamicMethod($"setter-{Guid.NewGuid()}", typeof(void), new Type[] { typeof(object), typeof(object) }, _reflectionInfo.Module, true);
                var ilGen = dynamicMethod.GetILGenerator();
                ilGen.EmitLoadArg(1);
                ilGen.EmitConvertFromObject(_reflectionInfo.PropertyType);
                ilGen.Emit(OpCodes.Call, _reflectionInfo.SetMethod);
                ilGen.Emit(OpCodes.Ret);
                return (Action<object, object>)dynamicMethod.CreateDelegate(typeof(Action<object, object>));
            }

            /// <summary>
            /// 获取属性值
            /// </summary>
            /// <param name="instance">静态属性内部传递null</param>
            /// <returns>属性值</returns>
            public override object GetValue(object instance)
            {
                return _getter(null);
            }

            /// <summary>
            /// 设置属性值
            /// </summary>
            /// <param name="instance">静态属性内部传递null</param>
            /// <param name="value">值</param>
            public override void SetValue(object instance, object value)
            {
                _setter(null, value);
            }

            /// <summary>
            /// 获取静态属性值
            /// </summary>
            /// <returns>值</returns>
            public override object GetStaticValue()
            {
                return _getter(null);
            }

            /// <summary>
            /// 设置静态属性值
            /// </summary>
            /// <param name="value">值</param>
            public override void SetStaticValue(object value)
            {
                _setter(null, value);
            }
        }
    }
}