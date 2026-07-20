using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace AspectCore.SourceGenerator;

/// <summary>
/// Emits IAspectInvokeDelegate implementation classes and the __InvokeDelegates
/// nested class that holds static singleton instances for each intercepted method.
/// All delegate classes are emitted as nested classes inside the proxy class so they
/// inherit the enclosing generic type parameters.
/// </summary>
internal static class DelegateEmitter
{
    /// <summary>
    /// Emits the __InvokeDelegates nested class inside the proxy class.
    /// Each intercepted method gets a static readonly field holding the singleton delegate instance.
    /// Also emits the delegate implementation classes as siblings nested inside the proxy.
    /// </summary>
    public static void EmitInvokeDelegatesClass(
        StringBuilder sb,
        IReadOnlyList<(IMethodSymbol Method, string MethodId)> methods,
        bool isClassProxy = false,
        string proxyTypeName = null)
    {
        // First emit individual delegate implementation classes (nested inside proxy).
        foreach (var (method, methodId) in methods)
        {
            EmitSingleDelegateClass(sb, method, methodId, isClassProxy, proxyTypeName);
            sb.AppendLine();
        }

        // For class proxies, emit base-call trampoline methods that the delegates invoke.
        if (isClassProxy)
        {
            foreach (var (method, methodId) in methods)
            {
                if (!method.IsGenericMethod)
                {
                    EmitBaseCallTrampoline(sb, method, methodId);
                }
            }
            sb.AppendLine();
        }

        // Then emit the __InvokeDelegates holder class with singleton instances.
        sb.AppendLine("        [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
        sb.AppendLine("        private static class __InvokeDelegates");
        sb.AppendLine("        {");

        foreach (var (method, methodId) in methods)
        {
            var delegateClassName = GetDelegateClassName(methodId);
            sb.AppendLine($"            internal static readonly global::AspectCore.DynamicProxy.IAspectInvokeDelegate {methodId} = new {delegateClassName}();");
        }

        sb.AppendLine("        }");
        sb.AppendLine();
    }

    private static void EmitSingleDelegateClass(
        StringBuilder sb,
        IMethodSymbol method,
        string methodId,
        bool isClassProxy = false,
        string proxyTypeName = null)
    {
        var delegateClassName = GetDelegateClassName(methodId);
        // For class proxies, the delegate calls a trampoline on the proxy (to do base.Method()).
        // For interface proxies, cast to the declaring type and call directly.
        var targetTypeName = isClassProxy && proxyTypeName != null
            ? proxyTypeName
            : method.ContainingType.ToGlobalName();

        sb.AppendLine($"        [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
        sb.AppendLine($"        private sealed class {delegateClassName} : global::AspectCore.DynamicProxy.IAspectInvokeDelegate");
        sb.AppendLine("        {");
        sb.AppendLine("            public object Invoke(object instance, object[] parameters)");
        sb.AppendLine("            {");

        // Determine if this is a property accessor.
        var isPropertyAccessor = method.MethodKind is MethodKind.PropertyGet or MethodKind.PropertySet;

        if (method.IsGenericMethod)
        {
            EmitGenericMethodBody(sb, method, targetTypeName);
        }
        else if (isClassProxy && proxyTypeName != null)
        {
            // Class proxy: for init-only property setters, use MethodReflector with Call
            // (non-virtual dispatch) because we cannot generate a base.Prop = value trampoline
            // for init-only properties, and MethodBase.Invoke dispatches virtually causing recursion.
            if (isPropertyAccessor && method.MethodKind == MethodKind.PropertySet && method.IsInitOnly)
            {
                EmitReflectorFallbackBody(sb, method);
            }
            else
            {
                EmitTrampolineCallBody(sb, method, methodId, proxyTypeName);
            }
        }
        else if (isPropertyAccessor)
        {
            EmitPropertyAccessorBody(sb, method, targetTypeName);
        }
        else
        {
            EmitDirectMethodBody(sb, method, targetTypeName);
        }

        sb.AppendLine("            }");
        sb.AppendLine("        }");
    }

    private static void EmitGenericMethodBody(StringBuilder sb, IMethodSymbol method, string targetTypeName)
    {
        // Generic method delegates are not invoked at runtime for the normal interception
        // path. SourceGeneratedAspectContext.Complete() uses _implementationMethod.Invoke()
        // directly for generic methods because type arguments are erased at the
        // IAspectInvokeDelegate.Invoke(object, object[]) level.
        //
        // This body exists as a defensive fallback. It throws NotSupportedException to
        // surface the problem clearly rather than failing with a confusing
        // ContainsGenericParameters error.
        sb.AppendLine($"                // Generic method: type arguments are erased at the delegate level.");
        sb.AppendLine($"                // SourceGeneratedAspectContext.Complete() handles generic methods");
        sb.AppendLine($"                // by invoking the closed MethodInfo directly via reflection.");
        sb.AppendLine($"                throw new global::System.NotSupportedException(");
        sb.AppendLine($"                    \"Generic method '{method.Name}' cannot be invoked through IAspectInvokeDelegate. \" +");
        sb.AppendLine($"                    \"This delegate should not be called for generic methods; \" +");
        sb.AppendLine($"                    \"SourceGeneratedAspectContext.Complete() handles them via MethodInfo.Invoke.\");");
    }

    /// <summary>
    /// Emits a delegate body that uses MethodReflector with CallOptions.Call for non-virtual dispatch.
    /// Used for class proxy init-only property setters where neither a trampoline nor direct
    /// MethodBase.Invoke (virtual dispatch → recursion) is viable.
    /// </summary>
    private static void EmitReflectorFallbackBody(StringBuilder sb, IMethodSymbol method)
    {
        // Get the base class's method (not the proxy override) to avoid virtual dispatch recursion.
        var baseTypeName = method.ContainingType.ToGlobalName();
        sb.AppendLine($"                // Init-only setter on class proxy: cannot use trampoline (base.Prop = value");
        sb.AppendLine($"                // is invalid outside constructor) and MethodBase.Invoke dispatches virtually.");
        sb.AppendLine($"                // Use MethodReflector with CallOptions.Call on the BASE type's method.");
        sb.AppendLine($"                var __method = typeof({baseTypeName}).GetMethod(\"{method.Name}\",");
        sb.AppendLine($"                    global::System.Reflection.BindingFlags.Instance | global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.NonPublic);");
        sb.AppendLine($"                var __reflector = global::AspectCore.Extensions.Reflection.ReflectorExtensions.GetReflector(");
        sb.AppendLine($"                    __method, global::AspectCore.Extensions.Reflection.CallOptions.Call);");
        sb.AppendLine($"                return __reflector.Invoke(instance, parameters);");
    }

    private static void EmitPropertyAccessorBody(StringBuilder sb, IMethodSymbol method, string targetTypeName)
    {
        // Find the associated property.
        var prop = (IPropertySymbol)method.AssociatedSymbol!;
        var isGetter = method.MethodKind == MethodKind.PropertyGet;

        sb.AppendLine($"                var __typed = ({targetTypeName})instance;");

        if (prop.IsIndexer)
        {
            // Indexer: parameters are the index arguments.
            if (isGetter)
            {
                var indexArgs = new List<string>();
                for (var i = 0; i < method.Parameters.Length; i++)
                {
                    var p = method.Parameters[i];
                    indexArgs.Add($"({p.Type.ToGlobalName()})parameters[{i}]");
                }
                // For ref readonly returns, just read the value (boxing loses the ref, which is fine
                // since the pipeline materialises into StrongBox at a higher level).
                sb.AppendLine($"                return __typed[{string.Join(", ", indexArgs)}];");
            }
            else
            {
                // Setter: last parameter is value, rest are indices.
                var indexArgs = new List<string>();
                for (var i = 0; i < method.Parameters.Length - 1; i++)
                {
                    var p = method.Parameters[i];
                    indexArgs.Add($"({p.Type.ToGlobalName()})parameters[{i}]");
                }
                var valueParam = method.Parameters[method.Parameters.Length - 1];
                sb.AppendLine($"                __typed[{string.Join(", ", indexArgs)}] = ({valueParam.Type.ToGlobalName()})parameters[{method.Parameters.Length - 1}];");
                sb.AppendLine("                return null;");
            }
        }
        else
        {
            // Regular property.
            if (isGetter)
            {
                // For ref/ref readonly returns, just read the value. Boxing to object
                // loses the ref semantics, which is fine since the proxy method body
                // handles ref materialisation via StrongBox<T>.
                sb.AppendLine($"                return __typed.{prop.Name};");
            }
            else
            {
                // Setter/init: value is the first (and only) parameter.
                // Init-only setters cannot be called directly from outside a
                // constructor/init context. Use reflection for init-only properties.
                var valueParam = method.Parameters[0];
                if (method.IsInitOnly)
                {
                    // Use reflection to invoke the init setter.
                    sb.AppendLine($"                typeof({targetTypeName}).GetProperty(\"{prop.Name}\",");
                    sb.AppendLine($"                    global::System.Reflection.BindingFlags.Instance | global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.NonPublic)");
                    sb.AppendLine($"                    .SetMethod.Invoke(instance, new object[] {{ ({valueParam.Type.ToGlobalName()})parameters[0] }});");
                    sb.AppendLine("                return null;");
                }
                else
                {
                    sb.AppendLine($"                __typed.{prop.Name} = ({valueParam.Type.ToGlobalName()})parameters[0];");
                    sb.AppendLine("                return null;");
                }
            }
        }
    }

    private static void EmitDirectMethodBody(StringBuilder sb, IMethodSymbol method, string targetTypeName)
    {
        sb.AppendLine($"                var __typed = ({targetTypeName})instance;");

        // Cast parameters and prepare locals for ref/out.
        for (var i = 0; i < method.Parameters.Length; i++)
        {
            var p = method.Parameters[i];
            var paramTypeName = p.Type.ToGlobalName();
            if (p.RefKind == RefKind.Out)
            {
                sb.AppendLine($"                {paramTypeName} __p{i} = default;");
            }
            else
            {
                sb.AppendLine($"                var __p{i} = ({paramTypeName})parameters[{i}];");
            }
        }

        // Build the call arguments.
        var argExprs = new List<string>();
        for (var i = 0; i < method.Parameters.Length; i++)
        {
            var p = method.Parameters[i];
            var prefix = p.RefKind switch
            {
                RefKind.Ref => "ref ",
                RefKind.Out => "out ",
                RefKind.In => "in ",
                _ => ""
            };
            argExprs.Add($"{prefix}__p{i}");
        }
        var callArgs = string.Join(", ", argExprs);

        if (method.ReturnsVoid)
        {
            sb.AppendLine($"                __typed.{method.Name}({callArgs});");
            EmitRefOutWriteBack(sb, method);
            sb.AppendLine("                return null;");
        }
        else if (method.RefKind == RefKind.Ref)
        {
            // ref return: take ref and box the value.
            sb.AppendLine($"                ref var __refResult = ref __typed.{method.Name}({callArgs});");
            EmitRefOutWriteBack(sb, method);
            sb.AppendLine("                return __refResult;");
        }
        else if (method.RefKind == RefKind.RefReadOnly)
        {
            // ref readonly return: cannot assign to ref var. Just read the value.
            sb.AppendLine($"                var __result = __typed.{method.Name}({callArgs});");
            EmitRefOutWriteBack(sb, method);
            sb.AppendLine("                return __result;");
        }
        else
        {
            sb.AppendLine($"                var __result = __typed.{method.Name}({callArgs});");
            EmitRefOutWriteBack(sb, method);
            sb.AppendLine("                return __result;");
        }
    }

    private static void EmitRefOutWriteBack(StringBuilder sb, IMethodSymbol method)
    {
        for (var i = 0; i < method.Parameters.Length; i++)
        {
            var p = method.Parameters[i];
            if (p.RefKind is RefKind.Ref or RefKind.Out)
            {
                sb.AppendLine($"                parameters[{i}] = __p{i};");
            }
        }
    }

    /// <summary>
    /// Gets the delegate class name for a given method (nested, so no proxy name prefix needed).
    /// </summary>
    public static string GetDelegateClassName(string methodId)
    {
        return $"__Delegate_{methodId}";
    }

    /// <summary>
    /// Returns the __InvokeDelegates field access expression for use in proxy method bodies.
    /// </summary>
    public static string GetDelegateFieldExpr(string methodId)
    {
        return $"__InvokeDelegates.{methodId}";
    }

    private static string GetBaseCallTrampolineName(string methodId)
    {
        return $"__BaseCall_{methodId}";
    }

    /// <summary>
    /// Emits the delegate body for class proxies: calls the trampoline method on the proxy instance.
    /// </summary>
    private static void EmitTrampolineCallBody(StringBuilder sb, IMethodSymbol method, string methodId, string proxyTypeName)
    {
        var trampolineName = GetBaseCallTrampolineName(methodId);
        sb.AppendLine($"                var __proxy = ({proxyTypeName})instance;");

        // Build args for the trampoline call
        var argExprs = new List<string>();
        for (var i = 0; i < method.Parameters.Length; i++)
        {
            var p = method.Parameters[i];
            var paramTypeName = p.Type.ToGlobalName();
            if (p.RefKind == RefKind.Out)
            {
                sb.AppendLine($"                {paramTypeName} __p{i} = default;");
                argExprs.Add($"out __p{i}");
            }
            else if (p.RefKind == RefKind.Ref)
            {
                sb.AppendLine($"                var __p{i} = ({paramTypeName})parameters[{i}];");
                argExprs.Add($"ref __p{i}");
            }
            else if (p.RefKind == RefKind.In)
            {
                sb.AppendLine($"                var __p{i} = ({paramTypeName})parameters[{i}];");
                argExprs.Add($"in __p{i}");
            }
            else
            {
                argExprs.Add($"({paramTypeName})parameters[{i}]");
            }
        }
        var callArgs = string.Join(", ", argExprs);

        if (method.ReturnsVoid)
        {
            sb.AppendLine($"                __proxy.{trampolineName}({callArgs});");
            EmitRefOutWriteBack(sb, method);
            sb.AppendLine("                return null;");
        }
        else
        {
            sb.AppendLine($"                var __result = __proxy.{trampolineName}({callArgs});");
            EmitRefOutWriteBack(sb, method);
            sb.AppendLine("                return __result;");
        }
    }

    /// <summary>
    /// Emits a private non-virtual trampoline method in the proxy class that calls base.Method().
    /// This allows the delegate (which is a nested class and cannot call base directly) to
    /// invoke the base implementation without triggering virtual dispatch recursion.
    /// </summary>
    private static void EmitBaseCallTrampoline(StringBuilder sb, IMethodSymbol method, string methodId)
    {
        var trampolineName = GetBaseCallTrampolineName(methodId);
        var returnTypeName = method.ReturnsVoid ? "void" : method.ReturnType.ToGlobalName();
        var isPropertyAccessor = method.MethodKind is MethodKind.PropertyGet or MethodKind.PropertySet;

        // For init-only property setters, we cannot call base.Prop = value from a regular method.
        // Skip generating a trampoline — the delegate will use reflection for init-only setters.
        if (isPropertyAccessor && method.MethodKind == MethodKind.PropertySet)
        {
            var prop = (IPropertySymbol)method.AssociatedSymbol!;
            if (method.IsInitOnly)
            {
                // No trampoline for init-only setters; delegate falls back to reflection.
                return;
            }
        }

        // For ref/ref readonly return properties, the trampoline must return by ref.
        var refModifier = "";
        if (isPropertyAccessor && method.MethodKind == MethodKind.PropertyGet)
        {
            var prop = (IPropertySymbol)method.AssociatedSymbol!;
            if (prop.RefKind == RefKind.Ref)
                refModifier = "ref ";
            else if (prop.RefKind == RefKind.RefReadOnly)
                refModifier = "ref readonly ";
        }
        else
        {
            refModifier = method.RefKind switch
            {
                RefKind.Ref => "ref ",
                RefKind.RefReadOnly => "ref readonly ",
                _ => ""
            };
        }

        // Build parameter list
        var paramList = new List<string>();
        var argList = new List<string>();
        for (var i = 0; i < method.Parameters.Length; i++)
        {
            var p = method.Parameters[i];
            var paramTypeName = p.Type.ToGlobalName();
            var prefix = p.RefKind switch
            {
                RefKind.Ref => "ref ",
                RefKind.Out => "out ",
                RefKind.In => "in ",
                _ => ""
            };
            paramList.Add($"{prefix}{paramTypeName} p{i}");
            argList.Add($"{prefix}p{i}");
        }
        var paramListStr = string.Join(", ", paramList);
        var argListStr = string.Join(", ", argList);

        sb.AppendLine($"        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine($"        private {refModifier}{returnTypeName} {trampolineName}({paramListStr})");
        sb.AppendLine("        {");

        if (isPropertyAccessor)
        {
            var prop = (IPropertySymbol)method.AssociatedSymbol!;
            if (method.MethodKind == MethodKind.PropertyGet)
            {
                var returnRefKeyword = (prop.RefKind == RefKind.Ref || prop.RefKind == RefKind.RefReadOnly) ? "ref " : "";
                if (prop.IsIndexer)
                {
                    sb.AppendLine($"            return {returnRefKeyword}base[{argListStr}];");
                }
                else
                {
                    sb.AppendLine($"            return {returnRefKeyword}base.{prop.Name};");
                }
            }
            else
            {
                // Regular setter (init-only already returned above)
                if (prop.IsIndexer)
                {
                    var indexArgs = argList.Take(argList.Count - 1);
                    sb.AppendLine($"            base[{string.Join(", ", indexArgs)}] = p{method.Parameters.Length - 1};");
                }
                else
                {
                    sb.AppendLine($"            base.{prop.Name} = p0;");
                }
            }
        }
        else if (method.ReturnsVoid)
        {
            sb.AppendLine($"            base.{method.Name}({argListStr});");
        }
        else if (method.RefKind == RefKind.Ref || method.RefKind == RefKind.RefReadOnly)
        {
            sb.AppendLine($"            return ref base.{method.Name}({argListStr});");
        }
        else
        {
            sb.AppendLine($"            return base.{method.Name}({argListStr});");
        }

        sb.AppendLine("        }");
        sb.AppendLine();
    }
}
