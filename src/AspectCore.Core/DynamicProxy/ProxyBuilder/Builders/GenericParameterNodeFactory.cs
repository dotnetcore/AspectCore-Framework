using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.DynamicProxy.ProxyBuilder.Nodes;

namespace AspectCore.DynamicProxy.ProxyBuilder.Builders
{
    internal static class GenericParameterNodeFactory
    {
        public static List<GenericParameterNode> FromType(Type targetType)
        {
            if (!targetType.GetTypeInfo().IsGenericTypeDefinition)
                return new List<GenericParameterNode>();

            return BuildFromGenericArguments(targetType.GetTypeInfo().GetGenericArguments(), isTypeLevel: true);
        }

        public static List<GenericParameterNode> FromMethod(MethodInfo method)
        {
            if (!method.IsGenericMethod)
                return new List<GenericParameterNode>();

            return BuildFromGenericArguments(method.GetGenericArguments(), isTypeLevel: false);
        }

        private static List<GenericParameterNode> BuildFromGenericArguments(Type[] genericArguments, bool isTypeLevel)
        {
            var result = new List<GenericParameterNode>(genericArguments.Length);
            foreach (var arg in genericArguments)
            {
                var argInfo = arg.GetTypeInfo();
                var constraints = isTypeLevel
                    ? ToClassGenericParameterAttributes(argInfo.GenericParameterAttributes)
                    : argInfo.GenericParameterAttributes;

                Type baseTypeConstraint = null;
                var interfaceConstraints = new List<Type>();

                foreach (var constraint in argInfo.GetGenericParameterConstraints())
                {
                    var constraintInfo = constraint.GetTypeInfo();
                    if (constraintInfo.IsClass)
                        baseTypeConstraint = constraintInfo.AsType();
                    if (constraintInfo.IsInterface)
                        interfaceConstraints.Add(constraintInfo.AsType());
                }

                var attributes = AttributeNodeFactory.FromCustomAttributes(arg.CustomAttributes).ToArray();

                result.Add(new GenericParameterNode(
                    argInfo.Name,
                    constraints,
                    baseTypeConstraint,
                    interfaceConstraints.ToArray(),
                    attributes));
            }
            return result;
        }

        private static GenericParameterAttributes ToClassGenericParameterAttributes(GenericParameterAttributes attributes)
        {
            if (attributes == GenericParameterAttributes.None)
                return GenericParameterAttributes.None;
            if (attributes.HasFlag(GenericParameterAttributes.SpecialConstraintMask))
                return GenericParameterAttributes.SpecialConstraintMask;
            if (attributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
                return GenericParameterAttributes.NotNullableValueTypeConstraint;
            if (attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint) && attributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
                return GenericParameterAttributes.ReferenceTypeConstraint | GenericParameterAttributes.DefaultConstructorConstraint;
            if (attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
                return GenericParameterAttributes.ReferenceTypeConstraint;
            if (attributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
                return GenericParameterAttributes.DefaultConstructorConstraint;
            return GenericParameterAttributes.None;
        }
    }
}
