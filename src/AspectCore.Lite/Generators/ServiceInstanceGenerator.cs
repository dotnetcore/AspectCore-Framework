using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace AspectCore.Lite.Generators
{
    public class ServiceInstanceGenerator
    {
        private readonly FieldBuilder builder;
        internal ServiceInstanceGenerator(TypeBuilder typeBuilder, Type serviceType)
        {
            builder = typeBuilder.DefineField(GeneratorConstants.ServiceInstanceFieldName, serviceType, FieldAttributes.Private | FieldAttributes.InitOnly);
        }

        public FieldBuilder ServiceInstanceBuilder => builder;
    }
}
