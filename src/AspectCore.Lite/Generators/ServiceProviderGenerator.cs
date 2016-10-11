using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace AspectCore.Lite.Generators
{
    public sealed class ServiceProviderGenerator
    {
        private readonly FieldBuilder builder;
        internal ServiceProviderGenerator(TypeBuilder typeBuilder)
        {
            builder = typeBuilder.DefineField(GeneratorConstants.ServiceProviderFieldName, typeof(IServiceProvider), FieldAttributes.Private | FieldAttributes.InitOnly);
        }

        public FieldBuilder ServiceProviderBuilder => builder;
    }
}