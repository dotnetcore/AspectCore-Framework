using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Lite.Internal
{
    internal class MethodMatcher
    {
        private readonly MethodInfo _methodInfo;
        private readonly ParameterInfo[] _parameters;

        public MethodMatcher(MethodInfo methodinfo)
        {
            _methodInfo = methodinfo;
            _parameters = methodinfo.GetParameters();
        }

        public int Match(object[] givenParameters)
        {
            var applyIndexStart = 0;
            var applyExactLength = 0;
            for (var givenIndex = 0; givenIndex != givenParameters.Length; givenIndex++)
            {
                var givenType = givenParameters[givenIndex] == null ? null : givenParameters[givenIndex].GetType().GetTypeInfo();
                var givenMatched = false;

                for (var applyIndex = applyIndexStart; givenMatched == false && applyIndex != _parameters.Length; ++applyIndex)
                {
                    if (_parameters[applyIndex].ParameterType.GetTypeInfo().IsAssignableFrom(givenType))
                    {
                        givenMatched = true;
                        if (applyIndexStart == applyIndex)
                        {
                            applyIndexStart++;
                            if (applyIndex == givenIndex)
                            {
                                applyExactLength = applyIndex;
                            }
                        }
                    }
                }

                if (givenMatched == false)
                {
                    return -1;
                }
            }
            return applyExactLength;
        }

        public MethodInfo AsMethodInfo()
        {
            return _methodInfo;
        }
    }
}
