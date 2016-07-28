using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions.Test.Fakes
{
    public class MeaninglessService
    {
        public void MeaninglessFunc(string name, int count, MeaninglessService service, object obj)
        {

        }

        public static MethodInfo MeaninglessFuncMethod = typeof(MeaninglessService).GetTypeInfo().GetMethod("MeaninglessFunc");
        public static ParameterInfo[] Parameters = MeaninglessFuncMethod.GetParameters();
    }
}
