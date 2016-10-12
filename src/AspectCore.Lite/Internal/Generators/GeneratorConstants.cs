using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Generators
{
    internal static class GeneratorConstants
    {
        internal const string Assembly = "AspectCore.Lite.Runtime$Proxys";

        internal const string Module = "main";

        internal const string Field = "$proxy@field<>_";

        internal const string Property = "$proxy@property<>_";

        internal const string Method = "$proxy@method<>_";

        internal const string Interface = "$proxy@interface<>_";

        internal const string Class = "$proxy@class<>_";

        internal const string ServiceProvider = "serviceProvider";

        internal const string ServiceInstance = "serviceInstance";

        internal const string ExpressionsAssembly = "System.Linq.Expressions";

        internal const string ILGenType = "System.Linq.Expressions.Compiler.ILGen";

        internal const string EmitConvertToType = "EmitConvertToType";

        internal const string GetRequiredService = "GetRequiredService";
    }
}
