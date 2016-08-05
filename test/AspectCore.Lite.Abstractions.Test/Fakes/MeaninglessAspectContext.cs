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
        public MeaninglessAspectContext(ITarget target, IProxy proxy) : base(target, proxy)
        {
        }
    }

    public class MeaninglessTarget : ITarget
    {
        public MemberInfo GetTargetMemberInfo()
        {
            throw new NotImplementedException();
        }

        public TypeInfo GetTargetTypeInfo()
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

        public MemberInfo GetProxyMemberInfo()
        {
            throw new NotImplementedException();
        }

        public MemberInfo GetProxyMetaDataInfo()
        {
            throw new NotImplementedException();
        }

        public TypeInfo GetProxyTypeInfo()
        {
            throw new NotImplementedException();
        }
    }


}
