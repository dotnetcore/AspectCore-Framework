using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace AspectCore.Lite.Generators
{
    public class InterceptedMethodBodyGenerator : MethodBodyGenerator
    {
        public InterceptedMethodBodyGenerator(InterfaceMethodGenerator methodGenerator , FieldGenerator serviceInstanceGenerator) 
            : base(methodGenerator , serviceInstanceGenerator)
        {
        }

        public override void GenerateMethodBody()
        {
            
        }
    }
}
