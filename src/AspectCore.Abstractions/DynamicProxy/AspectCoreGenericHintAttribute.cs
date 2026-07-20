using System;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// Specifies concrete type arguments for an open generic method to enable full
    /// NativeAOT source-generated delegate coverage. Without this attribute, the
    /// source generator emits diagnostic ACSG0101 and falls back to reflection for
    /// unclosed type parameters.
    /// </summary>
    /// <example>
    /// <code>
    /// [AspectCoreGenericHint(typeof(int), typeof(string))]
    /// T Process&lt;T&gt;(T input);
    /// </code>
    /// This tells the source generator to produce typed delegates for
    /// <c>Process&lt;int&gt;</c> and <c>Process&lt;string&gt;</c>.
    /// </example>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class AspectCoreGenericHintAttribute : Attribute
    {
        /// <summary>
        /// Gets the concrete type arguments to specialize for NativeAOT delegate generation.
        /// </summary>
        public Type[] TypeArguments { get; }

        /// <summary>
        /// Initializes a new instance with the specified concrete type arguments.
        /// Each invocation of this attribute provides one set of type arguments that
        /// the source generator should produce a fully-typed delegate for.
        /// </summary>
        /// <param name="typeArguments">The concrete types to substitute for the method's generic parameters.</param>
        public AspectCoreGenericHintAttribute(params Type[] typeArguments)
        {
            TypeArguments = typeArguments ?? throw new ArgumentNullException(nameof(typeArguments));
        }
    }
}
