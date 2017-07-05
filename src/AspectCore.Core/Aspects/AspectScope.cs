using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AspectCore.Core
{
    internal class AspectScope
    {
        internal Guid ScopeId { get; }

        internal AsyncLocal<AspectContext> Local { get; set; }

        internal int Level { get; set; }

        internal AspectScope(AspectContext aspectContext)
        {
            ScopeId = Guid.NewGuid();
            Local = new AsyncLocal<AspectContext>();
            Local.Value = aspectContext;
        }
    }
}
