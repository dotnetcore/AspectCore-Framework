using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Lite
{
    public interface IPointcut<TMember> where TMember : MemberInfo
    {
        TMember FindMember(string memberName);
        IEnumerable<TMember> GetMembers();
    }
}
