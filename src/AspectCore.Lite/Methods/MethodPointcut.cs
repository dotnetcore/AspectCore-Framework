using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Lite.Methods
{
    public abstract class MethodPointcut : IPointcut<MethodInfo>
    {
        protected IEnumerable<MethodInfo> methodInfos;
        public virtual MethodInfo FindMember(string memberName)
        {
            if (string.IsNullOrEmpty(memberName)) throw new ArgumentNullException(nameof(memberName));
            return GetMembers().FirstOrDefault(method => method.Name == memberName);
        }

        public virtual IEnumerable<MethodInfo> GetMembers()
        {
            return methodInfos ?? (methodInfos = ProtectedGetMembers());
        }

        protected abstract IEnumerable<MethodInfo> ProtectedGetMembers();
    }
}
