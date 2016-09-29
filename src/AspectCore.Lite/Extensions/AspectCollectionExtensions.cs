using AspectCore.Lite.Abstractions;
using System;

namespace AspectCore.Lite.Extensions
{
    public static class AspectCollectionExtensions
    {
        public static void Add(this IAspectCollection aspectCollection, Aspect[] aspects)
        {
            if (aspectCollection == null) throw new ArgumentNullException(nameof(aspectCollection));
            if (aspects == null || aspects.Length == 0) return;

            foreach (Aspect aspect in aspects) aspectCollection.Add(aspect);
        }
    }
}
