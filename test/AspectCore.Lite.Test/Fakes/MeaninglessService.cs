using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Lite.Test.Fakes
{
    public class MeaninglessService
    {
        public void MeaninglessFunc(string name, int count, MeaninglessService service, object obj)
        {
        }

        public static MethodInfo MeaninglessFuncMethod = MethodHelper.GetMethodInfo<Action<MeaninglessService, string, int, MeaninglessService, object>>((service, name, count, s, obj) => service.MeaninglessFunc(name, count, s, obj));
        public static ParameterInfo[] Parameters = MeaninglessFuncMethod.GetParameters();
    }
}
