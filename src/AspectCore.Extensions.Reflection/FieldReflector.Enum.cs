using System;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Extensions.Reflection.Emit;

namespace AspectCore.Extensions.Reflection
{
    public partial class FieldReflector : MemberReflector<FieldInfo>
    {
        /// <summary>
        /// 枚举反射操作
        /// </summary>
        private class EnumFieldReflector : FieldReflector
        {
            /// <summary>
            /// 枚举反射操作
            /// </summary>
            /// <param name="reflectionInfo">字段</param>
            public EnumFieldReflector(FieldInfo reflectionInfo) : base(reflectionInfo)
            {
            }

            /// <summary>
            /// 创建针对此字段的get访问器方法
            /// </summary>
            /// <returns>一个代表get访问器的委托</returns>
            protected override Func<object, object> CreateGetter()
            {
                var dynamicMethod = new DynamicMethod($"getter-{Guid.NewGuid()}", typeof(object), new Type[] { typeof(object) }, _reflectionInfo.Module, true);
                var ilGen = dynamicMethod.GetILGenerator();
                var value = _reflectionInfo.GetValue(null);
                ilGen.EmitConstant(value, _reflectionInfo.FieldType);
                ilGen.EmitConvertToObject(_reflectionInfo.FieldType);
                ilGen.Emit(OpCodes.Ret);
                return (Func<object, object>)dynamicMethod.CreateDelegate(typeof(Func<object, object>));
            }

            /// <summary>
            /// 创建针对此字段的set访问器方法
            /// </summary>
            /// <returns>一个代表set访问器的委托</returns>
            protected override Action<object, object> CreateSetter()
            {
                var dynamicMethod = new DynamicMethod($"setter-{Guid.NewGuid()}", typeof(void), new Type[] { typeof(object), typeof(object) }, _reflectionInfo.Module, true);
                var ilGen = dynamicMethod.GetILGenerator();
                ilGen.Emit(OpCodes.Ret);
                return (Action<object, object>)dynamicMethod.CreateDelegate(typeof(Action<object, object>));
            }

            /// <summary>
            /// 获取值
            /// </summary>
            /// <param name="instance">实例</param>
            /// <returns>值</returns>
            public override object GetValue(object instance)
            {
                return _getter(null);
            }

            /// <summary>
            /// 设置值
            /// </summary>
            /// <param name="instance">实例</param>
            /// <param name="value">值</param>
            public override void SetValue(object instance, object value)
            {
                throw new FieldAccessException("Cannot set a constant field");
            }

            /// <summary>
            /// 静态字段获取值
            /// </summary>
            /// <returns>值</returns>
            public override object GetStaticValue()
            {
                return _getter(null);
            }

            /// <summary>
            /// 静态字段设置值
            /// </summary>
            /// <param name="value">值</param>
            public override void SetStaticValue(object value)
            {
                throw new FieldAccessException("Cannot set a constant field");
            }
        }
    }
}