using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using AspectCore.Extensions.Reflection.Emit;

namespace AspectCore.Extensions.Reflection
{
    public sealed class FieldReflector : MemberReflector<FieldInfo>
    {
        private readonly Func<object, object> _getter;
        private readonly Action<object, object> _setter;

        private FieldReflector(FieldInfo reflectionInfo) : base(reflectionInfo)
        {
            _getter = CreateGetter();
            _setter = CreateSetter();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Func<object, object> CreateGetter()
        {
            var dynamicMethod = new DynamicMethod($"getter-{Guid.NewGuid()}", typeof(object), new Type[] { typeof(object) }, _reflectionInfo.Module, true);
            var ilGen = dynamicMethod.GetILGenerator();
            if (_reflectionInfo.IsStatic)
            {

            }
            ilGen.EmitConvertToObject(_reflectionInfo.FieldType);
            ilGen.Emit(OpCodes.Ret);
            return (Func<object, object>)dynamicMethod.CreateDelegate(typeof(Func<object, object>));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Action<object, object> CreateSetter()
        {
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetValue(object instance)
        {
            if (_reflectionInfo.IsStatic && instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            return _getter(instance);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(object instance, object value)
        {

        }

        #region internal
        internal static FieldReflector Create(FieldInfo reflectionInfo)
        {
            if (reflectionInfo == null)
            {
                throw new ArgumentNullException(nameof(reflectionInfo));
            }
            return ReflectorCache<FieldInfo, FieldReflector>.GetOrAdd(reflectionInfo, info => new FieldReflector(reflectionInfo));
        }
        #endregion

        public FieldInfo AsFieldInfo()
        {
            return _reflectionInfo;
        }
    }
}