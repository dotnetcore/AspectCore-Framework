using System;
using System.Threading;

namespace AspectCore.Core
{
    internal class AspectScope
    {
        private static AsyncLocal<AspectContext> _AspectContextCurrent = new AsyncLocal<AspectContext>();
        internal AspectContext AspectContext
        {
            get
            {
                return _AspectContextCurrent.Value;
            }
            set
            {
                _AspectContextCurrent.Value = value;
            }
        }

        internal Guid ScopeId { get; }

        internal int Level { get; set; }

        internal AspectScope(AspectContext aspectContext)
        {
            ScopeId = Guid.NewGuid();
            AspectContext = aspectContext;
        }
    }
}