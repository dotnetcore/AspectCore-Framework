using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions.Aspects
{
    //public abstract class Pointcut<TMember> : IPointcut where TMember : MemberInfo
    //{
    //    protected abstract Task<bool> IsMatch(TMember member);
    //    public Task<bool> IsMatch(MemberInfo member)
    //    {
    //        if (member == null) throw new ArgumentNullException(nameof(member));
    //        if (!(member is TMember)) throw new ArgumentException(
    //            $"member {member.Name} is not a {typeof(TMember).Name.Replace("Info", "")}.", nameof(member));
    //        return IsMatch((TMember)member);
    //    }
    //}
}
