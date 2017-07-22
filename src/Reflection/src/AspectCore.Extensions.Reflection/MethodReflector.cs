using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;

namespace AspectCore.Extensions.Reflection
{
    public partial class MethodReflector : MemberReflector<MethodInfo>
    {
        private readonly Func<object, object[], object> _invoker;
        private readonly bool _isInstance;

        private MethodReflector(MethodInfo reflectionInfo) : base(reflectionInfo)
        {
            _isInstance = !reflectionInfo.IsStatic;
            _invoker = CreateInvoker();
        }

        #region private methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Func<object, object[], object> CreateInvoker()
        {
            DynamicMethod dynamicMethod = new DynamicMethod($"invoker_{Guid.NewGuid()}",
               typeof(object), new Type[] { typeof(object), typeof(object[]) }, _reflectionInfo.Module, true);

            ILGenerator ilGen = dynamicMethod.GetILGenerator();
            var parameterTypes = _reflectionInfo.GetParameters().Select(p => p.ParameterType).ToArray();

            return (Func<object, object[], object>)dynamicMethod.CreateDelegate(typeof(Func<object, object[], object>));
        }

        #endregion

        #region internal methods
      

        #endregion

        #region public methods

        public MethodInfo AsMethodInfo()
        {
            return _reflectionInfo;
        }

        #endregion
    }
}
