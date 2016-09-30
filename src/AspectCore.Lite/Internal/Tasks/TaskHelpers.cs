using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Internal.Tasks
{
    internal static class TaskHelpers
    {
        public static Task FromVoid { get; } = Task.FromResult(default(VoidResult));

    }
}
