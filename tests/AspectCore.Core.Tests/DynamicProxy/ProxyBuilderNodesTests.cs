using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.ProxyBuilder;
using AspectCore.DynamicProxy.ProxyBuilder.Nodes;
using AspectCore.DynamicProxy.ProxyBuilder.Visitors;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class ProxyBuilderNodesTests
    {
        // ---- Test helper types ----

        public interface ITestService
        {
            int GetValue();
            void DoSomething(int arg);
        }

        [Serializable]
        public class TestAttributeTarget { }

        private static CustomAttributeData GetTestAttributeData()
            => typeof(TestAttributeTarget).GetCustomAttributesData().First();

        private static MethodInfo GetValueMethod => typeof(ITestService).GetMethod(nameof(ITestService.GetValue));

        private static MethodInfo DoSomethingMethod => typeof(ITestService).GetMethod(nameof(ITestService.DoSomething));

        private static (AssemblyBuilder asmBuilder, ModuleBuilder moduleBuilder) CreateModule()
        {
            var asmName = new AssemblyName("TestAsm_" + Guid.NewGuid().ToString("N"));
            var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndCollect);
            var moduleBuilder = asmBuilder.DefineDynamicModule("core");
            return (asmBuilder, moduleBuilder);
        }

        // ---- Recording visitor for Accept dispatch tests ----

        private class RecordingVisitor : IProxyBuilderVisitor
        {
            public string LastCalled { get; private set; }

            public Type VisitProxyType(ProxyTypeNode node) { LastCalled = "VisitProxyType"; return null; }
            public void VisitField(FieldNode node) => LastCalled = "VisitField";
            public void VisitConstructor(ConstructorNode node) => LastCalled = "VisitConstructor";
            public void VisitMethod(MethodNode node) => LastCalled = "VisitMethod";
            public void VisitProperty(PropertyNode node) => LastCalled = "VisitProperty";
            public void VisitParameter(ParameterNode node) => LastCalled = "VisitParameter";
            public void VisitGenericParameter(GenericParameterNode node) => LastCalled = "VisitGenericParameter";
            public void VisitAttribute(AttributeNode node) => LastCalled = "VisitAttribute";
            public void VisitMethodConstant(MethodConstantNode node) => LastCalled = "VisitMethodConstant";
            public void VisitDirectDelegationBody(DirectDelegationBody node) => LastCalled = "VisitDirectDelegationBody";
            public void VisitReflectorDelegationBody(ReflectorDelegationBody node) => LastCalled = "VisitReflectorDelegationBody";
            public void VisitRecordCloneBody(RecordCloneBody node) => LastCalled = "VisitRecordCloneBody";
            public void VisitAspectActivatorBody(AspectActivatorBody node) => LastCalled = "VisitAspectActivatorBody";
            public void VisitStubBody(StubBody node) => LastCalled = "VisitStubBody";
            public void VisitBackingFieldGetBody(BackingFieldGetBody node) => LastCalled = "VisitBackingFieldGetBody";
            public void VisitBackingFieldSetBody(BackingFieldSetBody node) => LastCalled = "VisitBackingFieldSetBody";
        }

        // ====================================================================
        // Accept dispatch tests
        // ====================================================================

        [Fact]
        public void AttributeNode_Accept_CallsVisitAttribute()
        {
            var visitor = new RecordingVisitor();
            var node = new AttributeNode(typeof(SerializableAttribute));
            node.Accept(visitor);
            Assert.Equal("VisitAttribute", visitor.LastCalled);
        }

        [Fact]
        public void ConstructorNode_Accept_CallsVisitConstructor()
        {
            var visitor = new RecordingVisitor();
            var node = new ConstructorNode(
                ConstructorKind.DefaultObjectCtor, MethodAttributes.Public,
                CallingConventions.Standard, Type.EmptyTypes, null, null, null, null, null);
            node.Accept(visitor);
            Assert.Equal("VisitConstructor", visitor.LastCalled);
        }

        [Fact]
        public void FieldNode_Accept_CallsVisitField()
        {
            var visitor = new RecordingVisitor();
            var node = new FieldNode("_field", typeof(int));
            node.Accept(visitor);
            Assert.Equal("VisitField", visitor.LastCalled);
        }

        [Fact]
        public void GenericParameterNode_Accept_CallsVisitGenericParameter()
        {
            var visitor = new RecordingVisitor();
            var node = new GenericParameterNode("T", GenericParameterAttributes.None, null, null, null);
            node.Accept(visitor);
            Assert.Equal("VisitGenericParameter", visitor.LastCalled);
        }

        [Fact]
        public void MethodConstantNode_Accept_CallsVisitMethodConstant()
        {
            var visitor = new RecordingVisitor();
            var node = new MethodConstantNode("key1", GetValueMethod);
            node.Accept(visitor);
            Assert.Equal("VisitMethodConstant", visitor.LastCalled);
        }

        [Fact]
        public void MethodNode_Accept_CallsVisitMethod()
        {
            var visitor = new RecordingVisitor();
            var body = new StubBody(typeof(int));
            var node = new MethodNode(
                GetValueMethod, null, "GetValue",
                MethodAttributes.Public, body, null, null, null, null, null);
            node.Accept(visitor);
            Assert.Equal("VisitMethod", visitor.LastCalled);
        }

        [Fact]
        public void ParameterNode_Accept_CallsVisitParameter()
        {
            var visitor = new RecordingVisitor();
            var node = new ParameterNode(0, "arg", typeof(int), ParameterAttributes.None, false, null, null);
            node.Accept(visitor);
            Assert.Equal("VisitParameter", visitor.LastCalled);
        }

        [Fact]
        public void PropertyNode_Accept_CallsVisitProperty()
        {
            var visitor = new RecordingVisitor();
            var node = new PropertyNode("Value", typeof(int), PropertyAttributes.None, Type.EmptyTypes, Type.EmptyTypes, null, null, null);
            node.Accept(visitor);
            Assert.Equal("VisitProperty", visitor.LastCalled);
        }

        [Fact]
        public void ProxyTypeNode_Accept_CallsVisitProxyType()
        {
            var visitor = new RecordingVisitor();
            var node = new ProxyTypeNode(
                "TestType", ProxyKind.InterfaceImpl, typeof(ITestService), typeof(object),
                new[] { typeof(ITestService) }, null, null, null, null, null, null, null);
            node.Accept(visitor);
            Assert.Equal("VisitProxyType", visitor.LastCalled);
        }

        [Fact]
        public void DirectDelegationBody_Accept_CallsVisitDirectDelegationBody()
        {
            var visitor = new RecordingVisitor();
            var node = new DirectDelegationBody("_impl", GetValueMethod, GetValueMethod, false, typeof(ITestService));
            node.Accept(visitor);
            Assert.Equal("VisitDirectDelegationBody", visitor.LastCalled);
        }

        [Fact]
        public void ReflectorDelegationBody_Accept_CallsVisitReflectorDelegationBody()
        {
            var visitor = new RecordingVisitor();
            var node = new ReflectorDelegationBody(GetValueMethod, GetValueMethod);
            node.Accept(visitor);
            Assert.Equal("VisitReflectorDelegationBody", visitor.LastCalled);
        }

        [Fact]
        public void RecordCloneBody_Accept_CallsVisitRecordCloneBody()
        {
            var visitor = new RecordingVisitor();
            var node = new RecordCloneBody("_implementation");
            node.Accept(visitor);
            Assert.Equal("VisitRecordCloneBody", visitor.LastCalled);
        }

        [Fact]
        public void AspectActivatorBody_Accept_CallsVisitAspectActivatorBody()
        {
            var visitor = new RecordingVisitor();
            var node = new AspectActivatorBody(
                GetValueMethod, GetValueMethod, GetValueMethod, false, ReturnKind.Sync, typeof(int));
            node.Accept(visitor);
            Assert.Equal("VisitAspectActivatorBody", visitor.LastCalled);
        }

        [Fact]
        public void StubBody_Accept_CallsVisitStubBody()
        {
            var visitor = new RecordingVisitor();
            var node = new StubBody(typeof(int));
            node.Accept(visitor);
            Assert.Equal("VisitStubBody", visitor.LastCalled);
        }

        [Fact]
        public void BackingFieldGetBody_Accept_CallsVisitBackingFieldGetBody()
        {
            var visitor = new RecordingVisitor();
            var node = new BackingFieldGetBody("_field");
            node.Accept(visitor);
            Assert.Equal("VisitBackingFieldGetBody", visitor.LastCalled);
        }

        [Fact]
        public void BackingFieldSetBody_Accept_CallsVisitBackingFieldSetBody()
        {
            var visitor = new RecordingVisitor();
            var node = new BackingFieldSetBody("_field");
            node.Accept(visitor);
            Assert.Equal("VisitBackingFieldSetBody", visitor.LastCalled);
        }

        // ====================================================================
        // Construction and property access tests
        // ====================================================================

        [Fact]
        public void AttributeNode_FromMarkerType_SetsProperties()
        {
            var node = new AttributeNode(typeof(SerializableAttribute));
            Assert.NotNull(node.MarkerAttributeType);
            Assert.Equal(typeof(SerializableAttribute), node.MarkerAttributeType);
            Assert.Null(node.CustomAttributeData);
            Assert.True(node.IsMarker);
        }

        [Fact]
        public void AttributeNode_FromCustomAttributeData_SetsProperties()
        {
            var data = GetTestAttributeData();
            var node = new AttributeNode(data);
            Assert.NotNull(node.CustomAttributeData);
            Assert.Equal(data, node.CustomAttributeData);
            Assert.Null(node.MarkerAttributeType);
            Assert.False(node.IsMarker);
        }

        [Fact]
        public void ConstructorNode_SetsAllProperties()
        {
            var param = new ParameterNode(0, "arg", typeof(int), ParameterAttributes.None, false, null, null);
            var attr = new AttributeNode(typeof(SerializableAttribute));
            var fieldAssign = new FieldAssignmentNode("_field", 1);
            var targetCreation = new TargetCreationNode(typeof(object), "_impl");

            var node = new ConstructorNode(
                ConstructorKind.ClassProxyCtorFromBase,
                MethodAttributes.Public,
                CallingConventions.Standard,
                new[] { typeof(int) },
                null,
                new[] { param },
                new[] { attr },
                new[] { fieldAssign },
                targetCreation);

            Assert.Equal(ConstructorKind.ClassProxyCtorFromBase, node.Kind);
            Assert.Equal(MethodAttributes.Public, node.MethodAttributes);
            Assert.Equal(CallingConventions.Standard, node.CallingConvention);
            Assert.Equal(new[] { typeof(int) }, node.ParameterTypes);
            Assert.Null(node.BaseConstructor);
            Assert.Single(node.Parameters);
            Assert.Single(node.Attributes);
            Assert.Single(node.FieldAssignments);
            Assert.NotNull(node.TargetCreation);
        }

        [Fact]
        public void ConstructorNode_NullCollections_DefaultToEmpty()
        {
            var node = new ConstructorNode(
                ConstructorKind.DefaultObjectCtor, MethodAttributes.Public,
                CallingConventions.Standard, null, null, null, null, null, null);

            Assert.Empty(node.Parameters);
            Assert.Empty(node.Attributes);
            Assert.Empty(node.FieldAssignments);
            Assert.Equal(Type.EmptyTypes, node.ParameterTypes);
            Assert.Null(node.TargetCreation);
        }

        [Fact]
        public void FieldNode_SetsProperties()
        {
            var node = new FieldNode("_myField", typeof(string), FieldAttributes.Public);
            Assert.Equal("_myField", node.Name);
            Assert.Equal(typeof(string), node.FieldType);
            Assert.Equal(FieldAttributes.Public, node.Accessibility);
        }

        [Fact]
        public void FieldNode_DefaultAccessibility_IsPrivate()
        {
            var node = new FieldNode("_f", typeof(int));
            Assert.Equal(FieldAttributes.Private, node.Accessibility);
        }

        [Fact]
        public void FieldAssignmentNode_SetsProperties()
        {
            var node = new FieldAssignmentNode("_myField", 3);
            Assert.Equal("_myField", node.FieldName);
            Assert.Equal(3, node.SourceArgIndex);
        }

        [Fact]
        public void GenericParameterNode_SetsProperties()
        {
            var attr = new AttributeNode(typeof(SerializableAttribute));
            var node = new GenericParameterNode(
                "T", GenericParameterAttributes.ReferenceTypeConstraint,
                typeof(IDisposable), new[] { typeof(IComparable) }, new[] { attr });

            Assert.Equal("T", node.Name);
            Assert.Equal(GenericParameterAttributes.ReferenceTypeConstraint, node.Constraints);
            Assert.Equal(typeof(IDisposable), node.BaseTypeConstraint);
            Assert.Equal(new[] { typeof(IComparable) }, node.InterfaceConstraints);
            Assert.Single(node.Attributes);
        }

        [Fact]
        public void GenericParameterNode_NullCollections_DefaultToEmpty()
        {
            var node = new GenericParameterNode("T", GenericParameterAttributes.None, null, null, null);
            Assert.Empty(node.InterfaceConstraints);
            Assert.Empty(node.Attributes);
            Assert.Null(node.BaseTypeConstraint);
        }

        [Fact]
        public void MethodConstantNode_SetsProperties()
        {
            var node = new MethodConstantNode("myKey", GetValueMethod);
            Assert.Equal("myKey", node.Key);
            Assert.Equal(GetValueMethod, node.Method);
        }

        [Fact]
        public void MethodConstantNode_NullMethod_Allowed()
        {
            var node = new MethodConstantNode("key", null);
            Assert.Equal("key", node.Key);
            Assert.Null(node.Method);
        }

        [Fact]
        public void MethodNode_SetsAllProperties()
        {
            var body = new StubBody(typeof(int));
            var param = new ParameterNode(0, "arg", typeof(int), ParameterAttributes.None, false, null, null);
            var genParam = new GenericParameterNode("T", GenericParameterAttributes.None, null, null, null);
            var attr = new AttributeNode(typeof(SerializableAttribute));

            var node = new MethodNode(
                GetValueMethod, GetValueMethod, "GetValue",
                MethodAttributes.Public | MethodAttributes.Virtual, body,
                new[] { param }, new[] { genParam }, new[] { attr },
                GetValueMethod, GetValueMethod);

            Assert.Equal(GetValueMethod, node.ServiceMethod);
            Assert.Equal(GetValueMethod, node.ImplementationMethod);
            Assert.Equal("GetValue", node.Name);
            Assert.Equal(MethodAttributes.Public | MethodAttributes.Virtual, node.MethodAttributes);
            Assert.Same(body, node.Body);
            Assert.Single(node.Parameters);
            Assert.Single(node.GenericParameters);
            Assert.Single(node.Attributes);
            Assert.Equal(GetValueMethod, node.OverridesMethod);
            Assert.Equal(GetValueMethod, node.PredicateMethod);
        }

        [Fact]
        public void MethodNode_NullCollections_DefaultToEmpty()
        {
            var body = new StubBody(typeof(void));
            var node = new MethodNode(
                GetValueMethod, null, "GetValue",
                MethodAttributes.Public, body, null, null, null, null, null);

            Assert.Empty(node.Parameters);
            Assert.Empty(node.GenericParameters);
            Assert.Empty(node.Attributes);
            Assert.Null(node.ImplementationMethod);
            Assert.Null(node.OverridesMethod);
            Assert.Null(node.PredicateMethod);
        }

        [Fact]
        public void ParameterNode_SetsProperties()
        {
            var attr = new AttributeNode(typeof(SerializableAttribute));
            var node = new ParameterNode(
                1, "myParam", typeof(string), ParameterAttributes.Optional,
                true, "defaultVal", new[] { attr });

            Assert.Equal(1, node.Position);
            Assert.Equal("myParam", node.Name);
            Assert.Equal(typeof(string), node.ParameterType);
            Assert.Equal(ParameterAttributes.Optional, node.Attributes);
            Assert.True(node.HasDefaultValue);
            Assert.Equal("defaultVal", node.DefaultValue);
            Assert.Single(node.CustomAttributes);
        }

        [Fact]
        public void ParameterNode_NullCustomAttributes_DefaultToEmpty()
        {
            var node = new ParameterNode(0, "arg", typeof(int), ParameterAttributes.None, false, null, null);
            Assert.Empty(node.CustomAttributes);
        }

        [Fact]
        public void PropertyNode_SetsAllProperties()
        {
            var attr = new AttributeNode(typeof(SerializableAttribute));
            var getBody = new BackingFieldGetBody("_field");
            var getMethod = new MethodNode(
                GetValueMethod, null, "get_Value",
                MethodAttributes.Public, getBody, null, null, null, null, null);
            var backingField = new FieldNode("_field", typeof(int));

            var node = new PropertyNode(
                "Value", typeof(int), PropertyAttributes.HasDefault,
                Type.EmptyTypes, Type.EmptyTypes,
                new[] { attr }, getMethod, null, backingField);

            Assert.Equal("Value", node.Name);
            Assert.Equal(typeof(int), node.PropertyType);
            Assert.Equal(PropertyAttributes.HasDefault, node.PropertyAttributes);
            Assert.Single(node.Attributes);
            Assert.NotNull(node.GetMethod);
            Assert.Null(node.SetMethod);
            Assert.NotNull(node.BackingField);
        }

        [Fact]
        public void PropertyNode_NullCollectionsAndOptional_DefaultToEmpty()
        {
            var node = new PropertyNode("Value", typeof(int), PropertyAttributes.None, Type.EmptyTypes, Type.EmptyTypes, null, null, null);
            Assert.Empty(node.Attributes);
            Assert.Null(node.GetMethod);
            Assert.Null(node.SetMethod);
            Assert.Null(node.BackingField);
        }

        [Fact]
        public void ProxyTypeNode_SetsAllProperties()
        {
            var genParam = new GenericParameterNode("T", GenericParameterAttributes.None, null, null, null);
            var attr = new AttributeNode(typeof(SerializableAttribute));
            var field = new FieldNode("_f", typeof(int));
            var ctor = new ConstructorNode(
                ConstructorKind.DefaultObjectCtor, MethodAttributes.Public,
                CallingConventions.Standard, Type.EmptyTypes, null, null, null, null, null);
            var body = new StubBody(typeof(int));
            var method = new MethodNode(
                GetValueMethod, null, "GetValue",
                MethodAttributes.Public, body, null, null, null, null, null);
            var prop = new PropertyNode("Value", typeof(int), PropertyAttributes.None, Type.EmptyTypes, Type.EmptyTypes, null, null, null);
            var mc = new MethodConstantNode("k", GetValueMethod);

            var node = new ProxyTypeNode(
                "MyProxy", ProxyKind.ClassProxy, typeof(ITestService), typeof(object),
                new[] { typeof(ITestService) }, new[] { genParam }, new[] { attr },
                new[] { field }, new[] { ctor }, new[] { method }, new[] { prop }, new[] { mc });

            Assert.Equal("MyProxy", node.Name);
            Assert.Equal(ProxyKind.ClassProxy, node.ProxyKind);
            Assert.Equal(typeof(ITestService), node.ServiceType);
            Assert.Equal(typeof(object), node.ParentType);
            Assert.Equal(new[] { typeof(ITestService) }, node.Interfaces);
            Assert.Single(node.GenericParameters);
            Assert.Single(node.Attributes);
            Assert.Single(node.Fields);
            Assert.Single(node.Constructors);
            Assert.Single(node.Methods);
            Assert.Single(node.Properties);
            Assert.Single(node.MethodConstants);
        }

        [Fact]
        public void ProxyTypeNode_NullCollections_DefaultToEmpty()
        {
            var node = new ProxyTypeNode(
                "P", ProxyKind.InterfaceImpl, typeof(ITestService), typeof(object),
                null, null, null, null, null, null, null, null);

            Assert.Equal(Type.EmptyTypes, node.Interfaces);
            Assert.Empty(node.GenericParameters);
            Assert.Empty(node.Attributes);
            Assert.Empty(node.Fields);
            Assert.Empty(node.Constructors);
            Assert.Empty(node.Methods);
            Assert.Empty(node.Properties);
            Assert.Empty(node.MethodConstants);
        }

        [Fact]
        public void TargetCreationNode_SetsProperties()
        {
            var node = new TargetCreationNode(typeof(string), "_impl");
            Assert.Equal(typeof(string), node.ImplType);
            Assert.Equal("_impl", node.TargetFieldName);
        }

        [Fact]
        public void DirectDelegationBody_SetsProperties()
        {
            var node = new DirectDelegationBody("_impl", GetValueMethod, GetValueMethod, true, typeof(ITestService));
            Assert.Equal("_impl", node.TargetFieldName);
            Assert.Equal(GetValueMethod, node.TargetMethod);
            Assert.Equal(GetValueMethod, node.ServiceMethod);
            Assert.True(node.IsCallvirt);
            Assert.Equal(typeof(ITestService), node.ServiceType);
        }

        [Fact]
        public void ReflectorDelegationBody_SetsProperties()
        {
            var node = new ReflectorDelegationBody(DoSomethingMethod, GetValueMethod);
            Assert.Equal(DoSomethingMethod, node.ImplementationMethod);
            Assert.Equal(GetValueMethod, node.ServiceMethod);
        }

        [Fact]
        public void RecordCloneBody_SetsProperties()
        {
            var node = new RecordCloneBody("_implementation");
            Assert.Equal("_implementation", node.TargetFieldName);
        }

        [Fact]
        public void AspectActivatorBody_SetsProperties()
        {
            var node = new AspectActivatorBody(
                GetValueMethod, DoSomethingMethod, GetValueMethod, true, ReturnKind.TaskOfT, typeof(Task<int>));
            Assert.Equal(GetValueMethod, node.ServiceMethod);
            Assert.Equal(DoSomethingMethod, node.ImplementationMethod);
            Assert.Equal(GetValueMethod, node.PredicateMethod);
            Assert.True(node.IsGeneric);
            Assert.Equal(ReturnKind.TaskOfT, node.ReturnKind);
            Assert.Equal(typeof(Task<int>), node.ReturnType);
        }

        [Fact]
        public void StubBody_SetsProperties()
        {
            var node = new StubBody(typeof(string));
            Assert.Equal(typeof(string), node.ReturnType);
        }

        [Fact]
        public void StubBody_NullReturnType_Allowed()
        {
            var node = new StubBody(null);
            Assert.Null(node.ReturnType);
        }

        [Fact]
        public void BackingFieldGetBody_SetsProperties()
        {
            var node = new BackingFieldGetBody("_bf");
            Assert.Equal("_bf", node.FieldName);
        }

        [Fact]
        public void BackingFieldSetBody_SetsProperties()
        {
            var node = new BackingFieldSetBody("_bf");
            Assert.Equal("_bf", node.FieldName);
        }

        // ====================================================================
        // Null argument validation tests (edge cases)
        // ====================================================================

        [Fact]
        public void AttributeNode_NullData_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new AttributeNode((CustomAttributeData)null));
        }

        [Fact]
        public void AttributeNode_NullMarkerType_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new AttributeNode((Type)null));
        }

        [Fact]
        public void FieldNode_NullName_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new FieldNode(null, typeof(int)));
        }

        [Fact]
        public void FieldNode_NullFieldType_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new FieldNode("x", null));
        }

        [Fact]
        public void FieldAssignmentNode_NullFieldName_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new FieldAssignmentNode(null, 0));
        }

        [Fact]
        public void GenericParameterNode_NullName_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new GenericParameterNode(null, GenericParameterAttributes.None, null, null, null));
        }

        [Fact]
        public void MethodConstantNode_NullKey_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new MethodConstantNode(null, GetValueMethod));
        }

        [Fact]
        public void MethodNode_NullServiceMethod_Throws()
        {
            var body = new StubBody(typeof(int));
            Assert.Throws<ArgumentNullException>(() => new MethodNode(
                null, null, "M", MethodAttributes.Public, body, null, null, null, null, null));
        }

        [Fact]
        public void MethodNode_NullName_Throws()
        {
            var body = new StubBody(typeof(int));
            Assert.Throws<ArgumentNullException>(() => new MethodNode(
                GetValueMethod, null, null, MethodAttributes.Public, body, null, null, null, null, null));
        }

        [Fact]
        public void MethodNode_NullBody_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new MethodNode(
                GetValueMethod, null, "M", MethodAttributes.Public, null, null, null, null, null, null));
        }

        [Fact]
        public void ParameterNode_NullParameterType_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ParameterNode(0, "arg", null, ParameterAttributes.None, false, null, null));
        }

        [Fact]
        public void PropertyNode_NullName_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new PropertyNode(null, typeof(int), PropertyAttributes.None, Type.EmptyTypes, Type.EmptyTypes, null, null, null));
        }

        [Fact]
        public void PropertyNode_NullPropertyType_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new PropertyNode("X", null, PropertyAttributes.None, Type.EmptyTypes, Type.EmptyTypes, null, null, null));
        }

        [Fact]
        public void ProxyTypeNode_NullName_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ProxyTypeNode(
                null, ProxyKind.InterfaceImpl, typeof(ITestService), typeof(object),
                null, null, null, null, null, null, null, null));
        }

        [Fact]
        public void ProxyTypeNode_NullServiceType_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ProxyTypeNode(
                "X", ProxyKind.InterfaceImpl, null, typeof(object),
                null, null, null, null, null, null, null, null));
        }

        [Fact]
        public void ProxyTypeNode_NullParentType_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ProxyTypeNode(
                "X", ProxyKind.InterfaceImpl, typeof(ITestService), null,
                null, null, null, null, null, null, null, null));
        }

        [Fact]
        public void TargetCreationNode_NullImplType_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new TargetCreationNode(null, "_f"));
        }

        [Fact]
        public void TargetCreationNode_NullTargetFieldName_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new TargetCreationNode(typeof(object), null));
        }

        [Fact]
        public void DirectDelegationBody_NullTargetFieldName_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new DirectDelegationBody(null, GetValueMethod, GetValueMethod, false, typeof(ITestService)));
        }

        [Fact]
        public void DirectDelegationBody_NullTargetMethod_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new DirectDelegationBody("_f", null, GetValueMethod, false, typeof(ITestService)));
        }

        [Fact]
        public void DirectDelegationBody_NullServiceMethod_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new DirectDelegationBody("_f", GetValueMethod, null, false, typeof(ITestService)));
        }

        [Fact]
        public void DirectDelegationBody_NullServiceType_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new DirectDelegationBody("_f", GetValueMethod, GetValueMethod, false, null));
        }

        [Fact]
        public void ReflectorDelegationBody_NullImplementationMethod_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ReflectorDelegationBody(null, GetValueMethod));
        }

        [Fact]
        public void ReflectorDelegationBody_NullServiceMethod_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ReflectorDelegationBody(GetValueMethod, null));
        }

        [Fact]
        public void RecordCloneBody_NullTargetFieldName_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new RecordCloneBody(null));
        }

        [Fact]
        public void AspectActivatorBody_NullServiceMethod_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new AspectActivatorBody(null, GetValueMethod, GetValueMethod, false, ReturnKind.Void, null));
        }

        [Fact]
        public void AspectActivatorBody_NullImplementationMethod_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new AspectActivatorBody(GetValueMethod, null, GetValueMethod, false, ReturnKind.Void, null));
        }

        [Fact]
        public void AspectActivatorBody_NullPredicateMethod_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new AspectActivatorBody(GetValueMethod, GetValueMethod, null, false, ReturnKind.Void, null));
        }

        [Fact]
        public void BackingFieldGetBody_NullFieldName_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new BackingFieldGetBody(null));
        }

        [Fact]
        public void BackingFieldSetBody_NullFieldName_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new BackingFieldSetBody(null));
        }

        // ====================================================================
        // ILEmitVisitor specific tests
        // ====================================================================

        [Fact]
        public void ILEmitVisitor_NullContext_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ILEmitVisitor(null));
        }

        [Fact]
        public void ILEmitVisitor_VisitParameter_DoesNotThrow()
        {
            var (_, moduleBuilder) = CreateModule();
            var ctx = new ILEmitVisitorContext(moduleBuilder);
            var visitor = new ILEmitVisitor(ctx);
            var node = new ParameterNode(0, "arg", typeof(int), ParameterAttributes.None, false, null, null);

            // VisitParameter is a no-op (handled inline in EmitMethodParameters/EmitClassProxyCtor)
            visitor.VisitParameter(node);
        }

        [Fact]
        public void ILEmitVisitor_VisitGenericParameter_DoesNotThrow()
        {
            var (_, moduleBuilder) = CreateModule();
            var ctx = new ILEmitVisitorContext(moduleBuilder);
            var visitor = new ILEmitVisitor(ctx);
            var node = new GenericParameterNode("T", GenericParameterAttributes.None, null, null, null);

            // VisitGenericParameter is a no-op (handled inline in DefineGenericParameters)
            visitor.VisitGenericParameter(node);
        }

        [Fact]
        public void ILEmitVisitor_VisitAttribute_Marker_SetsAttributeOnType()
        {
            var (_, moduleBuilder) = CreateModule();
            var ctx = new ILEmitVisitorContext(moduleBuilder);
            var typeBuilder = moduleBuilder.DefineType("AttrTestType", TypeAttributes.Class | TypeAttributes.Public);
            ctx.TypeBuilder = typeBuilder;
            var visitor = new ILEmitVisitor(ctx);

            var attrNode = new AttributeNode(typeof(SerializableAttribute));
            visitor.VisitAttribute(attrNode);

            var createdType = typeBuilder.CreateTypeInfo().AsType();
            var attr = createdType.GetCustomAttribute(typeof(SerializableAttribute));
            Assert.NotNull(attr);
        }

        [Fact]
        public void ILEmitVisitor_VisitAttribute_FromCustomAttributeData_SetsAttributeOnType()
        {
            var (_, moduleBuilder) = CreateModule();
            var ctx = new ILEmitVisitorContext(moduleBuilder);
            var typeBuilder = moduleBuilder.DefineType("AttrDataTestType", TypeAttributes.Class | TypeAttributes.Public);
            ctx.TypeBuilder = typeBuilder;
            var visitor = new ILEmitVisitor(ctx);

            var data = GetTestAttributeData();
            var attrNode = new AttributeNode(data);
            visitor.VisitAttribute(attrNode);

            var createdType = typeBuilder.CreateTypeInfo().AsType();
            var attr = createdType.GetCustomAttribute(typeof(SerializableAttribute));
            Assert.NotNull(attr);
        }

        // ====================================================================
        // Integration tests: build a ProxyTypeNode and visit it
        // ====================================================================

        [Fact]
        public void ILEmitVisitor_VisitProxyType_InterfaceImpl_BuildsValidType()
        {
            var (_, moduleBuilder) = CreateModule();
            var ctx = new ILEmitVisitorContext(moduleBuilder);
            var visitor = new ILEmitVisitor(ctx);

            var ctor = new ConstructorNode(
                ConstructorKind.DefaultObjectCtor, MethodAttributes.Public,
                CallingConventions.Standard, Type.EmptyTypes, null, null, null, null, null);

            var getValueBody = new StubBody(typeof(int));
            var getValueMethod = new MethodNode(
                GetValueMethod, null, "GetValue",
                MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                getValueBody, null, null, null, GetValueMethod, null);

            var doSomethingBody = new StubBody(typeof(void));
            var doSomethingMethod = new MethodNode(
                DoSomethingMethod, null, "DoSomething",
                MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                doSomethingBody, null, null, null, DoSomethingMethod, null);

            var proxyNode = new ProxyTypeNode(
                "IntegrationTestProxy", ProxyKind.InterfaceImpl, typeof(ITestService), typeof(object),
                new[] { typeof(ITestService) }, null, null, null,
                new[] { ctor }, new[] { getValueMethod, doSomethingMethod }, null, null);

            var resultType = visitor.VisitProxyType(proxyNode);

            Assert.NotNull(resultType);
            Assert.True(typeof(ITestService).IsAssignableFrom(resultType));

            var instance = Activator.CreateInstance(resultType);
            Assert.NotNull(instance);
            Assert.IsAssignableFrom<ITestService>(instance);

            var service = (ITestService)instance;
            Assert.Equal(0, service.GetValue());
            service.DoSomething(42);
        }

        [Fact]
        public void ILEmitVisitor_VisitProxyType_WithTypeAttribute_AppliesAttribute()
        {
            var (_, moduleBuilder) = CreateModule();
            var ctx = new ILEmitVisitorContext(moduleBuilder);
            var visitor = new ILEmitVisitor(ctx);

            var attrNode = new AttributeNode(typeof(SerializableAttribute));

            var ctor = new ConstructorNode(
                ConstructorKind.DefaultObjectCtor, MethodAttributes.Public,
                CallingConventions.Standard, Type.EmptyTypes, null, null, null, null, null);

            var getValueBody = new StubBody(typeof(int));
            var getValueMethod = new MethodNode(
                GetValueMethod, null, "GetValue",
                MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                getValueBody, null, null, null, GetValueMethod, null);

            var doSomethingBody = new StubBody(typeof(void));
            var doSomethingMethod = new MethodNode(
                DoSomethingMethod, null, "DoSomething",
                MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                doSomethingBody, null, null, null, DoSomethingMethod, null);

            var proxyNode = new ProxyTypeNode(
                "AttrProxyType", ProxyKind.InterfaceImpl, typeof(ITestService), typeof(object),
                new[] { typeof(ITestService) }, null, new[] { attrNode }, null,
                new[] { ctor }, new[] { getValueMethod, doSomethingMethod }, null, null);

            var resultType = visitor.VisitProxyType(proxyNode);

            Assert.NotNull(resultType);
            var attr = resultType.GetCustomAttribute(typeof(SerializableAttribute));
            Assert.NotNull(attr);
        }

        [Fact]
        public void ILEmitVisitor_VisitProxyType_WithMethodParameterAttribute_AppliesAttribute()
        {
            var (_, moduleBuilder) = CreateModule();
            var ctx = new ILEmitVisitorContext(moduleBuilder);
            var visitor = new ILEmitVisitor(ctx);

            var ctor = new ConstructorNode(
                ConstructorKind.DefaultObjectCtor, MethodAttributes.Public,
                CallingConventions.Standard, Type.EmptyTypes, null, null, null, null, null);

            var paramAttr = new AttributeNode(typeof(OptionalAttribute));
            var paramNode = new ParameterNode(
                0, "arg", typeof(int), ParameterAttributes.None, false, null, new[] { paramAttr });

            var getValueBody = new StubBody(typeof(int));
            var getValueMethod = new MethodNode(
                GetValueMethod, null, "GetValue",
                MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                getValueBody, null, null, null, GetValueMethod, null);

            var doSomethingBody = new StubBody(typeof(void));
            var doSomethingMethod = new MethodNode(
                DoSomethingMethod, null, "DoSomething",
                MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                doSomethingBody, new[] { paramNode }, null, null, DoSomethingMethod, null);

            var proxyNode = new ProxyTypeNode(
                "ParamAttrProxy", ProxyKind.InterfaceImpl, typeof(ITestService), typeof(object),
                new[] { typeof(ITestService) }, null, null, null,
                new[] { ctor }, new[] { getValueMethod, doSomethingMethod }, null, null);

            var resultType = visitor.VisitProxyType(proxyNode);

            Assert.NotNull(resultType);
            var methodInfo = resultType.GetMethod("DoSomething");
            Assert.NotNull(methodInfo);
            var param = methodInfo.GetParameters().First();
            var attr = param.GetCustomAttribute(typeof(OptionalAttribute));
            Assert.NotNull(attr);
        }

        [Fact]
        public void ILEmitVisitor_VisitProxyType_WithFields_DefinesFields()
        {
            var (_, moduleBuilder) = CreateModule();
            var ctx = new ILEmitVisitorContext(moduleBuilder);
            var visitor = new ILEmitVisitor(ctx);

            var field1 = new FieldNode("_intField", typeof(int), FieldAttributes.Public);
            var field2 = new FieldNode("_stringField", typeof(string), FieldAttributes.Public);

            var ctor = new ConstructorNode(
                ConstructorKind.DefaultObjectCtor, MethodAttributes.Public,
                CallingConventions.Standard, Type.EmptyTypes, null, null, null, null, null);

            var getValueBody = new StubBody(typeof(int));
            var getValueMethod = new MethodNode(
                GetValueMethod, null, "GetValue",
                MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                getValueBody, null, null, null, GetValueMethod, null);

            var doSomethingBody = new StubBody(typeof(void));
            var doSomethingMethod = new MethodNode(
                DoSomethingMethod, null, "DoSomething",
                MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                doSomethingBody, null, null, null, DoSomethingMethod, null);

            var proxyNode = new ProxyTypeNode(
                "FieldTestProxy", ProxyKind.InterfaceImpl, typeof(ITestService), typeof(object),
                new[] { typeof(ITestService) }, null, null,
                new[] { field1, field2 }, new[] { ctor },
                new[] { getValueMethod, doSomethingMethod }, null, null);

            var resultType = visitor.VisitProxyType(proxyNode);

            Assert.NotNull(resultType);
            Assert.NotNull(resultType.GetField("_intField"));
            Assert.NotNull(resultType.GetField("_stringField"));
            Assert.Equal(typeof(int), resultType.GetField("_intField").FieldType);
            Assert.Equal(typeof(string), resultType.GetField("_stringField").FieldType);
        }

        [Fact]
        public void ILEmitVisitor_VisitAll_VisitsMultipleNodes()
        {
            var (_, moduleBuilder) = CreateModule();
            var ctx = new ILEmitVisitorContext(moduleBuilder);
            var visitor = new ILEmitVisitor(ctx);

            var ctor = new ConstructorNode(
                ConstructorKind.DefaultObjectCtor, MethodAttributes.Public,
                CallingConventions.Standard, Type.EmptyTypes, null, null, null, null, null);

            var getValueBody = new StubBody(typeof(int));
            var getValueMethod = new MethodNode(
                GetValueMethod, null, "GetValue",
                MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                getValueBody, null, null, null, GetValueMethod, null);

            var doSomethingBody = new StubBody(typeof(void));
            var doSomethingMethod = new MethodNode(
                DoSomethingMethod, null, "DoSomething",
                MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                doSomethingBody, null, null, null, DoSomethingMethod, null);

            var methods = new[] { getValueMethod, doSomethingMethod };

            var node1 = new ProxyTypeNode(
                "VisitAllType1", ProxyKind.InterfaceImpl, typeof(ITestService), typeof(object),
                new[] { typeof(ITestService) }, null, null, null,
                new[] { ctor }, methods, null, null);

            var node2 = new ProxyTypeNode(
                "VisitAllType2", ProxyKind.InterfaceImpl, typeof(ITestService), typeof(object),
                new[] { typeof(ITestService) }, null, null, null,
                new[] { ctor }, methods, null, null);

            var results = visitor.VisitAll(new[] { node1, node2 });

            Assert.Equal(2, results.Length);
            Assert.NotNull(results[0]);
            Assert.NotNull(results[1]);
            Assert.True(typeof(ITestService).IsAssignableFrom(results[0]));
            Assert.True(typeof(ITestService).IsAssignableFrom(results[1]));
        }
    }
}
