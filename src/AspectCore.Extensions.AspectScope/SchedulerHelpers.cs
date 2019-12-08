using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using AspectCore.DynamicProxy;
using System.Linq;

namespace AspectCore.Extensions.AspectScope
{
    //internal static class SchedulerHelpers
    //{
    //    private static readonly HashSet<string> _invokes = new HashSet<string> { "Invoke", "InvokeTask", "InvokeValueTask" };
    //    private static readonly ConcurrentDictionary<MethodInfo, bool> _aspectMap = new ConcurrentDictionary<MethodInfo, bool>();

    //    public static bool IsAspectInvoke(MethodInfo method)
    //    {
    //        if (method == null)
    //        {
    //            return false;
    //        }
    //        return _aspectMap.GetOrAdd(method, m => _invokes.Contains(m.Name) && typeof(IAspectActivator).IsAssignableFrom(m.DeclaringType));
    //    }

    //    public static MethodInfo[] GetInvokeMethods()
    //    {
    //        var frames =new StackTrace().GetFrames();
    //        return frames.Select(x => x.GetMethod() as MethodInfo).Where(x => IsAspectInvoke(x)).ToArray();
    //    }
    //}
}