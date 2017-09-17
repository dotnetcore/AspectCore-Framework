using System;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Extensions.Reflection.Emit;

namespace AspectCore.Extensions.Reflection
{
    public partial class PropertyReflector
    {
        private class CallPropertyReflector : PropertyReflector
        {
            public CallPropertyReflector(PropertyInfo reflectionInfo)
                : base(reflectionInfo)
            {
            }

            protected override Func<object, object> CreateGetter()
            {
                var dynamicMethod = new DynamicMethod($"getter-{Guid.NewGuid()}", typeof(object), new Type[] { typeof(object) }, _reflectionInfo.Module, true);
                var ilGen = dynamicMethod.GetILGenerator();
                ilGen.EmitLoadArg(0);
                ilGen.EmitConvertFromObject(_reflectionInfo.DeclaringType);
                if (_reflectionInfo.DeclaringType.GetTypeInfo().IsValueType)
                {
                    var local = ilGen.DeclareLocal(_reflectionInfo.DeclaringType);
                    ilGen.Emit(OpCodes.Stloc, local);
                    ilGen.Emit(OpCodes.Ldloca, local);
                }
                ilGen.Emit(OpCodes.Call, _reflectionInfo.GetMethod);
                if (_reflectionInfo.PropertyType.GetTypeInfo().IsValueType)
                    ilGen.EmitConvertToObject(_reflectionInfo.PropertyType);
                ilGen.Emit(OpCodes.Ret);
                return (Func<object, object>)dynamicMethod.CreateDelegate(typeof(Func<object, object>));
            }

            protected override Action<object, object> CreateSetter()
            {
                var dynamicMethod = new DynamicMethod($"setter-{Guid.NewGuid()}", typeof(void), new Type[] { typeof(object), typeof(object) }, _reflectionInfo.Module, true);
                var ilGen = dynamicMethod.GetILGenerator();
                ilGen.EmitLoadArg(0);
                ilGen.EmitConvertFromObject(_reflectionInfo.DeclaringType);
                if (_reflectionInfo.DeclaringType.GetTypeInfo().IsValueType)
                {
                    var local = ilGen.DeclareLocal(_reflectionInfo.DeclaringType);
                    ilGen.Emit(OpCodes.Stloc, local);
                    ilGen.Emit(OpCodes.Ldloca, local);
                }
                ilGen.EmitLoadArg(1);
                ilGen.EmitConvertFromObject(_reflectionInfo.PropertyType);
                ilGen.Emit(OpCodes.Call, _reflectionInfo.SetMethod);
                ilGen.Emit(OpCodes.Ret);
                return (Action<object, object>)dynamicMethod.CreateDelegate(typeof(Action<object, object>));
            }
        }
    }
}
