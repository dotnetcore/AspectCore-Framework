using System.Reflection;

namespace AspectCore.Extensions.Reflection
{
    public partial class MethodReflector : MemberReflector<MethodInfo>
    {
        private class CallMethodReflector : MethodReflector
        {
            public CallMethodReflector(MethodInfo reflectionInfo)
               : base(reflectionInfo)
            {
            }
        }
    }
}
