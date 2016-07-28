using AspectCore.Lite.Abstractions.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace AspectCore.Lite.Abstractions.Test.Fakes
{
    public class MeaninglessAspectContext : AspectContext
    {
    }

    public class MeaninglessTarget : ITarget
    {
        public MemberInfo GetTargetMetaData()
        {
            throw new NotImplementedException();
        }
    }

    public class MeaninglessProxy : IProxy
    {
        public object GetProxyInstance()
        {
            throw new NotImplementedException();
        }

        public MemberInfo GetProxyMetaData()
        {
            throw new NotImplementedException();
        }
    }


}
