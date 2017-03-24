using System.Collections.Generic;

namespace AspectCore.Abstractions.Internal
{
    internal class AspectContextItemProvider : IAspectContextItemProvider
    {
        private readonly IDictionary<string, object> items;

        internal AspectContextItemProvider(IDictionary<string, object> items)
        {
            this.items = items;
        }

        public IDictionary<string, object> Items
        {
            get
            {
                return items;
            }
        }
    }
}
