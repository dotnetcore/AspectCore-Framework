using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class ReflectionUtilsTests
    {
        // ---- Test types ----

        public interface ITestService
        {
            void VoidMethod();
            int IntMethod(int x);
            string StringMethod(string s);
            Task TaskMethod();
            Task<int> TaskTMethod();
            ValueTask ValueTaskMethod();
            ValueTask<int> ValueTaskTMethod();
            string Name { get; set; }
            int ReadOnly { get; }
            int WriteOnly { set; }
        }

        public class TestClass
        {
            public virtual void VirtualMethod() { }
            public void NonVirtualMethod() { }
            public static void StaticMethod() { }
            public sealed override string ToString() => "test";
            protected virtual void ProtectedVirtualMethod() { }
            protected internal virtual void ProtectedInternalVirtualMethod() { }
            private void PrivateMethod() { }

            public virtual string VirtualProperty { get; set; }
            public string NonVirtualProperty { get; set; }
            public static string StaticProperty { get; set; }
            public int ReadOnlyProperty { get; }
            public int WriteOnlyProperty { set { } }
            public virtual int VirtualReadOnlyProperty { get; }

            public void VoidMethod() { }
            public int IntMethod() => 1;
            public Task TaskMethod() => Task.CompletedTask;
            public Task<int> TaskTMethod() => Task.FromResult(1);
            public ValueTask ValueTaskMethod() => new ValueTask();
            public ValueTask<int> ValueTaskTMethod() => new ValueTask<int>(1);
        }

        public class ExplicitImpl : ITestService
        {
            void ITestService.VoidMethod() { }
            int ITestService.IntMethod(int x) => x;
            string ITestService.StringMethod(string s) => s;
            Task ITestService.TaskMethod() => Task.CompletedTask;
            Task<int> ITestService.TaskTMethod() => Task.FromResult(1);
            ValueTask ITestService.ValueTaskMethod() => new ValueTask();
            ValueTask<int> ITestService.ValueTaskTMethod() => new ValueTask<int>(1);
            string ITestService.Name { get => "e"; set { } }
            int ITestService.ReadOnly => 1;
            int ITestService.WriteOnly { set { } }
        }

        public sealed class SealedClass { }

        public struct TestStruct { }

        public enum TestEnum { A, B }

        public abstract class AbstractClass { }

        internal class InternalClass { }

        public class OuterClass
        {
            public class PublicNested { }
            internal class InternalNested { }
            private class PrivateNested { }
        }

        [NonAspect]
        public class NonAspectType
        {
            public void Method() { }
        }

        public class NonAspectMethodType
        {
            [NonAspect]
            public void NonAspectMethod() { }

            public void RegularMethod() { }
        }

        [Dynamically]
        public class DynamicallyType { }

        // ---- IsProxy ----

        [Fact]
        public void IsProxy_Null_ReturnsFalse()
        {
            object instance = null;
            Assert.False(instance.IsProxy());
        }

        [Fact]
        public void IsProxy_DynamicallyType_ReturnsTrue()
        {
            var instance = new DynamicallyType();
            Assert.True(instance.IsProxy());
        }

        [Fact]
        public void IsProxy_NormalType_ReturnsFalse()
        {
            var instance = new TestClass();
            Assert.False(instance.IsProxy());
        }

        // ---- IsProxyType ----

        [Fact]
        public void IsProxyType_Null_Throws()
        {
            TypeInfo typeInfo = null;
            Assert.Throws<ArgumentNullException>(() => typeInfo.IsProxyType());
        }

        [Fact]
        public void IsProxyType_DynamicallyType_ReturnsTrue()
        {
            Assert.True(typeof(DynamicallyType).GetTypeInfo().IsProxyType());
        }

        [Fact]
        public void IsProxyType_NormalType_ReturnsFalse()
        {
            Assert.False(typeof(TestClass).GetTypeInfo().IsProxyType());
        }

        // ---- CanInherited ----

        [Fact]
        public void CanInherited_Null_Throws()
        {
            TypeInfo typeInfo = null;
            Assert.Throws<ArgumentNullException>(() => typeInfo.CanInherited());
        }

        [Fact]
        public void CanInherited_PublicClass_ReturnsTrue()
        {
            Assert.True(typeof(TestClass).GetTypeInfo().CanInherited());
        }

        [Fact]
        public void CanInherited_AbstractClass_ReturnsTrue()
        {
            Assert.True(typeof(AbstractClass).GetTypeInfo().CanInherited());
        }

        [Fact]
        public void CanInherited_SealedClass_ReturnsFalse()
        {
            Assert.False(typeof(SealedClass).GetTypeInfo().CanInherited());
        }

        [Fact]
        public void CanInherited_ValueType_ReturnsFalse()
        {
            Assert.False(typeof(TestStruct).GetTypeInfo().CanInherited());
        }

        [Fact]
        public void CanInherited_Enum_ReturnsFalse()
        {
            Assert.False(typeof(TestEnum).GetTypeInfo().CanInherited());
        }

        [Fact]
        public void CanInherited_InternalClass_ReturnsFalse()
        {
            Assert.False(typeof(InternalClass).GetTypeInfo().CanInherited());
        }

        [Fact]
        public void CanInherited_DynamicallyType_ReturnsFalse()
        {
            Assert.False(typeof(DynamicallyType).GetTypeInfo().CanInherited());
        }

        [Fact]
        public void CanInherited_PublicNestedClass_ReturnsTrue()
        {
            Assert.True(typeof(OuterClass.PublicNested).GetTypeInfo().CanInherited());
        }

        [Fact]
        public void CanInherited_InternalNestedClass_ReturnsFalse()
        {
            Assert.False(typeof(OuterClass.InternalNested).GetTypeInfo().CanInherited());
        }

        // ---- GetParameterTypes ----

        [Fact]
        public void GetParameterTypes_Null_Throws()
        {
            MethodInfo method = null;
            Assert.Throws<ArgumentNullException>(() => method.GetParameterTypes());
        }

        [Fact]
        public void GetParameterTypes_NoParameters_ReturnsEmpty()
        {
            var method = typeof(TestClass).GetMethod(nameof(TestClass.VoidMethod));
            var types = method.GetParameterTypes();
            Assert.Empty(types);
        }

        [Fact]
        public void GetParameterTypes_WithParameters_ReturnsInOrder()
        {
            var method = typeof(ITestService).GetMethod(nameof(ITestService.IntMethod));
            var types = method.GetParameterTypes();
            Assert.Equal(new[] { typeof(int) }, types);
        }

        [Fact]
        public void GetParameterTypes_MultipleParameters_ReturnsAll()
        {
            var method = typeof(MultiParamType).GetMethod(nameof(MultiParamType.Method));
            var types = method.GetParameterTypes();
            Assert.Equal(new[] { typeof(int), typeof(string), typeof(bool) }, types);
        }

        public class MultiParamType
        {
            public void Method(int a, string b, bool c) { }
        }

        // ---- IsNonAspect(TypeInfo) ----

        [Fact]
        public void IsNonAspect_TypeInfo_Null_Throws()
        {
            TypeInfo typeInfo = null;
            Assert.Throws<ArgumentNullException>(() => typeInfo.IsNonAspect());
        }

        [Fact]
        public void IsNonAspect_TypeInfo_WithAttribute_ReturnsTrue()
        {
            Assert.True(typeof(NonAspectType).GetTypeInfo().IsNonAspect());
        }

        [Fact]
        public void IsNonAspect_TypeInfo_WithoutAttribute_ReturnsFalse()
        {
            Assert.False(typeof(TestClass).GetTypeInfo().IsNonAspect());
        }

        // ---- IsNonAspect(MethodInfo) ----

        [Fact]
        public void IsNonAspect_MethodInfo_Null_Throws()
        {
            MethodInfo method = null;
            Assert.Throws<ArgumentNullException>(() => method.IsNonAspect());
        }

        [Fact]
        public void IsNonAspect_MethodInfo_DeclaringTypeNonAspect_ReturnsTrue()
        {
            var method = typeof(NonAspectType).GetMethod(nameof(NonAspectType.Method));
            Assert.True(method.IsNonAspect());
        }

        [Fact]
        public void IsNonAspect_MethodInfo_MethodAttribute_ReturnsTrue()
        {
            var method = typeof(NonAspectMethodType).GetMethod(nameof(NonAspectMethodType.NonAspectMethod));
            Assert.True(method.IsNonAspect());
        }

        [Fact]
        public void IsNonAspect_MethodInfo_RegularMethod_ReturnsFalse()
        {
            var method = typeof(NonAspectMethodType).GetMethod(nameof(NonAspectMethodType.RegularMethod));
            Assert.False(method.IsNonAspect());
        }

        // ---- IsCallvirt ----

        [Fact]
        public void IsCallvirt_Null_Throws()
        {
            MethodInfo method = null;
            Assert.Throws<ArgumentNullException>(() => method.IsCallvirt());
        }

        [Fact]
        public void IsCallvirt_InterfaceMethod_ReturnsTrue()
        {
            var method = typeof(ITestService).GetMethod(nameof(ITestService.VoidMethod));
            Assert.True(method.IsCallvirt());
        }

        [Fact]
        public void IsCallvirt_ClassMethod_ReturnsFalse()
        {
            var method = typeof(TestClass).GetMethod(nameof(TestClass.VirtualMethod));
            Assert.False(method.IsCallvirt());
        }

        [Fact]
        public void IsCallvirt_ExplicitImplementation_ReturnsTrue()
        {
            var method = GetExplicitMethod(nameof(ITestService.VoidMethod));
            Assert.True(method.IsCallvirt());
        }

        // ---- IsExplicit ----

        [Fact]
        public void IsExplicit_Null_Throws()
        {
            MethodInfo method = null;
            Assert.Throws<ArgumentNullException>(() => method.IsExplicit());
        }

        [Fact]
        public void IsExplicit_ExplicitImplementation_ReturnsTrue()
        {
            var method = GetExplicitMethod(nameof(ITestService.VoidMethod));
            Assert.True(method.IsExplicit());
        }

        [Fact]
        public void IsExplicit_RegularMethod_ReturnsFalse()
        {
            var method = typeof(TestClass).GetMethod(nameof(TestClass.VirtualMethod));
            Assert.False(method.IsExplicit());
        }

        [Fact]
        public void IsExplicit_InterfaceDeclaration_ReturnsFalse()
        {
            var method = typeof(ITestService).GetMethod(nameof(ITestService.VoidMethod));
            Assert.False(method.IsExplicit());
        }

        // ---- IsVoid ----

        [Fact]
        public void IsVoid_VoidMethod_ReturnsTrue()
        {
            var method = typeof(TestClass).GetMethod(nameof(TestClass.VoidMethod));
            Assert.True(method.IsVoid());
        }

        [Fact]
        public void IsVoid_NonVoidMethod_ReturnsFalse()
        {
            var method = typeof(TestClass).GetMethod(nameof(TestClass.IntMethod));
            Assert.False(method.IsVoid());
        }

        [Fact]
        public void IsVoid_Null_Throws()
        {
            MethodInfo method = null;
            Assert.Throws<NullReferenceException>(() => method.IsVoid());
        }

        // ---- GetDisplayName(PropertyInfo) ----

        [Fact]
        public void GetDisplayName_Property_Null_Throws()
        {
            PropertyInfo property = null;
            Assert.Throws<ArgumentNullException>(() => property.GetDisplayName());
        }

        [Fact]
        public void GetDisplayName_Property_Interface_ReturnsFullName()
        {
            var property = typeof(ITestService).GetProperty(nameof(ITestService.Name));
            Assert.Equal("AspectCore.Core.Tests.DynamicProxy.ReflectionUtilsTests.ITestService.Name", property.GetDisplayName());
        }

        [Fact]
        public void GetDisplayName_Property_Class_ReturnsPropertyName()
        {
            var property = typeof(TestClass).GetProperty(nameof(TestClass.VirtualProperty));
            Assert.Equal("VirtualProperty", property.GetDisplayName());
        }

        // ---- GetName(MethodInfo) ----

        [Fact]
        public void GetName_Method_Null_Throws()
        {
            MethodInfo method = null;
            Assert.Throws<ArgumentNullException>(() => method.GetName());
        }

        [Fact]
        public void GetName_Method_Interface_ReturnsFullName()
        {
            var method = typeof(ITestService).GetMethod(nameof(ITestService.VoidMethod));
            Assert.Equal("AspectCore.Core.Tests.DynamicProxy.ReflectionUtilsTests.ITestService.VoidMethod", method.GetName());
        }

        [Fact]
        public void GetName_Method_Class_ReturnsMethodName()
        {
            var method = typeof(TestClass).GetMethod(nameof(TestClass.VoidMethod));
            Assert.Equal("VoidMethod", method.GetName());
        }

        // ---- GetDisplayName(MethodInfo) ----

        [Fact]
        public void GetDisplayName_Method_Null_Throws()
        {
            MethodInfo method = null;
            Assert.Throws<ArgumentNullException>(() => method.GetDisplayName());
        }

        [Fact]
        public void GetDisplayName_Method_Void_ReturnsFullDisplayName()
        {
            var method = typeof(TestClass).GetMethod(nameof(TestClass.VoidMethod));
            Assert.Equal("AspectCore.Core.Tests.DynamicProxy.ReflectionUtilsTests.TestClass.Void VoidMethod()", method.GetDisplayName());
        }

        [Fact]
        public void GetDisplayName_Method_WithParams_ReturnsFullDisplayName()
        {
            var method = typeof(ITestService).GetMethod(nameof(ITestService.IntMethod));
            Assert.Equal("AspectCore.Core.Tests.DynamicProxy.ReflectionUtilsTests.ITestService.Int32 IntMethod(Int32)", method.GetDisplayName());
        }

        // ---- IsReturnTask ----

        [Fact]
        public void IsReturnTask_Null_Throws()
        {
            MethodInfo method = null;
            Assert.Throws<ArgumentNullException>(() => method.IsReturnTask());
        }

        [Fact]
        public void IsReturnTask_TaskOfT_ReturnsTrue()
        {
            var method = typeof(TestClass).GetMethod(nameof(TestClass.TaskTMethod));
            Assert.True(method.IsReturnTask());
        }

        [Fact]
        public void IsReturnTask_Task_ReturnsFalse()
        {
            var method = typeof(TestClass).GetMethod(nameof(TestClass.TaskMethod));
            Assert.False(method.IsReturnTask());
        }

        [Fact]
        public void IsReturnTask_Void_ReturnsFalse()
        {
            var method = typeof(TestClass).GetMethod(nameof(TestClass.VoidMethod));
            Assert.False(method.IsReturnTask());
        }

        [Fact]
        public void IsReturnTask_ValueTaskOfT_ReturnsFalse()
        {
            var method = typeof(TestClass).GetMethod(nameof(TestClass.ValueTaskTMethod));
            Assert.False(method.IsReturnTask());
        }

        // ---- IsReturnValueTask ----

        [Fact]
        public void IsReturnValueTask_Null_Throws()
        {
            MethodInfo method = null;
            Assert.Throws<ArgumentNullException>(() => method.IsReturnValueTask());
        }

        [Fact]
        public void IsReturnValueTask_ValueTaskOfT_ReturnsTrue()
        {
            var method = typeof(TestClass).GetMethod(nameof(TestClass.ValueTaskTMethod));
            Assert.True(method.IsReturnValueTask());
        }

        [Fact]
        public void IsReturnValueTask_ValueTask_ReturnsFalse()
        {
            var method = typeof(TestClass).GetMethod(nameof(TestClass.ValueTaskMethod));
            Assert.False(method.IsReturnValueTask());
        }

        [Fact]
        public void IsReturnValueTask_TaskOfT_ReturnsFalse()
        {
            var method = typeof(TestClass).GetMethod(nameof(TestClass.TaskTMethod));
            Assert.False(method.IsReturnValueTask());
        }

        // ---- IsVisibleAndVirtual(MethodInfo) ----

        [Fact]
        public void IsVisibleAndVirtual_Method_Null_Throws()
        {
            MethodInfo method = null;
            Assert.Throws<ArgumentNullException>(() => method.IsVisibleAndVirtual());
        }

        [Fact]
        public void IsVisibleAndVirtual_Method_PublicVirtual_ReturnsTrue()
        {
            var method = typeof(TestClass).GetMethod(nameof(TestClass.VirtualMethod));
            Assert.True(method.IsVisibleAndVirtual());
        }

        [Fact]
        public void IsVisibleAndVirtual_Method_PublicNonVirtual_ReturnsFalse()
        {
            var method = typeof(TestClass).GetMethod(nameof(TestClass.NonVirtualMethod));
            Assert.False(method.IsVisibleAndVirtual());
        }

        [Fact]
        public void IsVisibleAndVirtual_Method_Static_ReturnsFalse()
        {
            var method = typeof(TestClass).GetMethod(nameof(TestClass.StaticMethod));
            Assert.False(method.IsVisibleAndVirtual());
        }

        [Fact]
        public void IsVisibleAndVirtual_Method_SealedOverride_ReturnsFalse()
        {
            var method = typeof(TestClass).GetMethod(nameof(TestClass.ToString));
            Assert.False(method.IsVisibleAndVirtual());
        }

        [Fact]
        public void IsVisibleAndVirtual_Method_ProtectedVirtual_ReturnsTrue()
        {
            var method = typeof(TestClass).GetMethod("ProtectedVirtualMethod",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.True(method.IsVisibleAndVirtual());
        }

        [Fact]
        public void IsVisibleAndVirtual_Method_ProtectedInternalVirtual_ReturnsTrue()
        {
            var method = typeof(TestClass).GetMethod("ProtectedInternalVirtualMethod",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.True(method.IsVisibleAndVirtual());
        }

        [Fact]
        public void IsVisibleAndVirtual_Method_Private_ReturnsFalse()
        {
            var method = typeof(TestClass).GetMethod("PrivateMethod",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.False(method.IsVisibleAndVirtual());
        }

        // ---- IsVisibleAndVirtual(PropertyInfo) ----

        [Fact]
        public void IsVisibleAndVirtual_Property_Null_Throws()
        {
            PropertyInfo property = null;
            Assert.Throws<ArgumentNullException>(() => property.IsVisibleAndVirtual());
        }

        [Fact]
        public void IsVisibleAndVirtual_Property_PublicVirtual_ReturnsTrue()
        {
            var property = typeof(TestClass).GetProperty(nameof(TestClass.VirtualProperty));
            Assert.True(property.IsVisibleAndVirtual());
        }

        [Fact]
        public void IsVisibleAndVirtual_Property_PublicNonVirtual_ReturnsFalse()
        {
            var property = typeof(TestClass).GetProperty(nameof(TestClass.NonVirtualProperty));
            Assert.False(property.IsVisibleAndVirtual());
        }

        [Fact]
        public void IsVisibleAndVirtual_Property_Static_ReturnsFalse()
        {
            var property = typeof(TestClass).GetProperty(nameof(TestClass.StaticProperty));
            Assert.False(property.IsVisibleAndVirtual());
        }

        [Fact]
        public void IsVisibleAndVirtual_Property_ReadOnlyNonVirtual_ReturnsFalse()
        {
            var property = typeof(TestClass).GetProperty(nameof(TestClass.ReadOnlyProperty));
            Assert.False(property.IsVisibleAndVirtual());
        }

        [Fact]
        public void IsVisibleAndVirtual_Property_ReadOnlyVirtual_ReturnsTrue()
        {
            var property = typeof(TestClass).GetProperty(nameof(TestClass.VirtualReadOnlyProperty));
            Assert.True(property.IsVisibleAndVirtual());
        }

        [Fact]
        public void IsVisibleAndVirtual_Property_WriteOnlyNonVirtual_ReturnsFalse()
        {
            var property = typeof(TestClass).GetProperty(nameof(TestClass.WriteOnlyProperty));
            Assert.False(property.IsVisibleAndVirtual());
        }

        // ---- GetMethodBySignature ----

        [Fact]
        public void GetMethodBySignature_NullTypeInfo_Throws()
        {
            TypeInfo typeInfo = null;
            var method = typeof(TestClass).GetMethod(nameof(TestClass.VoidMethod));
            Assert.Throws<ArgumentNullException>(() => typeInfo.GetMethodBySignature(method));
        }

        [Fact]
        public void GetMethodBySignature_NullMethod_Throws()
        {
            var typeInfo = typeof(TestClass).GetTypeInfo();
            MethodInfo method = null;
            Assert.Throws<ArgumentNullException>(() => typeInfo.GetMethodBySignature(method));
        }

        [Fact]
        public void GetMethodBySignature_SameType_ReturnsMethod()
        {
            var typeInfo = typeof(TestClass).GetTypeInfo();
            var method = typeof(TestClass).GetMethod(nameof(TestClass.IntMethod));
            var result = typeInfo.GetMethodBySignature(method);
            Assert.NotNull(result);
            Assert.Equal(method, result);
        }

        [Fact]
        public void GetMethodBySignature_InterfaceToExplicitImpl_ReturnsImplementation()
        {
            var typeInfo = typeof(ExplicitImpl).GetTypeInfo();
            var interfaceMethod = typeof(ITestService).GetMethod(nameof(ITestService.IntMethod));
            var result = typeInfo.GetMethodBySignature(interfaceMethod);
            Assert.NotNull(result);
            Assert.Equal(typeof(int), result.ReturnType);
            Assert.True(result.IsExplicit());
        }

        [Fact]
        public void GetMethodBySignature_InterfaceToExplicitImpl_Void_ReturnsImplementation()
        {
            var typeInfo = typeof(ExplicitImpl).GetTypeInfo();
            var interfaceMethod = typeof(ITestService).GetMethod(nameof(ITestService.VoidMethod));
            var result = typeInfo.GetMethodBySignature(interfaceMethod);
            Assert.NotNull(result);
            Assert.True(result.IsVoid());
            Assert.True(result.IsExplicit());
        }

        // ---- Helpers ----

        private static MethodInfo GetExplicitMethod(string interfaceMethodName)
        {
            var interfaceType = typeof(ITestService);
            var map = typeof(ExplicitImpl).GetInterfaceMap(interfaceType);
            var index = Array.IndexOf(map.InterfaceMethods,
                interfaceType.GetMethod(interfaceMethodName));
            return map.TargetMethods[index];
        }
    }
}
