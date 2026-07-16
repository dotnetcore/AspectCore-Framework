// ReSharper disable once CheckNamespace
namespace System.Reflection
{
    /// <summary>
    /// Extension methods for <see cref="MethodBase"/> that provide uniform access
    /// to the <c>IsPartial</c> concept across all target frameworks.
    /// <para>
    /// Partial methods and partial property accessors are a C# 13.0 language feature.
    /// At the IL level (where the runtime proxy builder operates), the compiler has
    /// already merged partial property declarations into a single property with complete
    /// accessor bodies. Therefore we conservatively report <c>false</c> for the runtime
    /// path; the source generator path uses Roslyn's <c>IPropertySymbol.IsPartial</c>
    /// directly.
    /// </para>
    /// </summary>
    internal static class MethodBasePartialExtensions
    {
        /// <summary>
        /// Gets a value that indicates whether the method is a partial method
        /// (including partial property accessors).
        /// </summary>
        public static bool IsPartialMethod(this MethodBase method)
        {
            if (method is null) return false;
            // The runtime proxy builder operates on already-compiled types where
            // partial declarations are merged. Always false for the runtime path.
            return false;
        }
    }
}
