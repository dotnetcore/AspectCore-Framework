using System.Collections.Generic;

namespace AspectCore.Abstractions.Internal
{
    internal class AspectContextDataProvider : IAspectContextDataProvider
    {
        private readonly IDictionary<string, object> items;

        internal AspectContextDataProvider(IDictionary<string, object> items)
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
