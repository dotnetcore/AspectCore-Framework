using AspectCore.Lite.Abstractions.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions
{
    public static class AspectCollectionExtensions
    {
        public static void Add(this IAspectCollection aspectCollection, IAspect[] aspects)
        {
            if (aspectCollection == null) throw new ArgumentNullException(nameof(aspectCollection));
            if (aspects == null || aspects.Length == 0) return;

            foreach (IAspect aspect in aspects) aspectCollection.Add(aspect);
        }
    }
}
