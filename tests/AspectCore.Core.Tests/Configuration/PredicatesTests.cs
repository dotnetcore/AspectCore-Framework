using System;
using System.Reflection;
using AspectCore.Configuration;
using Xunit;

namespace AspectCore.Core.Tests.Configuration
{
    public class PredicatesTests
    {
        #region Test Types

        private interface ITestService
        {
            void Run();
        }

        private class TestService
        {
            public virtual void Foo() { }

            public virtual void Bar() { }

            public virtual int Echo(int value) => value;

            public string Name { get; set; }
        }

        private class TestDerived : TestService, ITestService
        {
            public void Run() { }
        }

        private class GenericService<T>
        {
            public virtual void Execute() { }
        }

        private sealed class SealedService
        {
            public void DoSomething() { }
        }

        private enum TestEnum
        {
            Value1,
            Value2
        }

        private abstract class AbstractBase
        {
            public abstract void Perform();
        }

        private class ConcreteImpl : AbstractBase
        {
            public override void Perform() { }
        }

        #endregion

        #region ForNameSpace

        [Fact]
        public void ForNameSpace_ExactMatch_ReturnsTrue()
        {
            var predicate = Predicates.ForNameSpace("AspectCore.Core.Tests.Configuration");
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));

            Assert.True(predicate(method));
        }

        [Fact]
        public void ForNameSpace_NonMatchingNamespace_ReturnsFalse()
        {
            var predicate = Predicates.ForNameSpace("Some.Other.Namespace");
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));

            Assert.False(predicate(method));
        }

        [Fact]
        public void ForNameSpace_WildcardSuffix_ReturnsTrue()
        {
            var predicate = Predicates.ForNameSpace("AspectCore.Core.Tests.*");
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));

            Assert.True(predicate(method));
        }

        [Fact]
        public void ForNameSpace_WildcardPrefix_ReturnsTrue()
        {
            var predicate = Predicates.ForNameSpace("*.Configuration");
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));

            Assert.True(predicate(method));
        }

        [Fact]
        public void ForNameSpace_WildcardContains_ReturnsTrue()
        {
            var predicate = Predicates.ForNameSpace("*Tests.Configuration*");
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));

            Assert.True(predicate(method));
        }

        [Fact]
        public void ForNameSpace_SingleCharWildcard_ReturnsTrue()
        {
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));
            var ns = method.DeclaringType.Namespace;

            // '?' matches a single character, including the '.' separators.
            var pattern = ns.Replace('.', '?');

            var predicate = Predicates.ForNameSpace(pattern);

            Assert.True(predicate(method));
        }

        [Fact]
        public void ForNameSpace_WildcardNonMatching_ReturnsFalse()
        {
            var predicate = Predicates.ForNameSpace("Foo.Bar.*");
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));

            Assert.False(predicate(method));
        }

        [Fact]
        public void ForNameSpace_NullArgument_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => Predicates.ForNameSpace(null));
            Assert.Equal("nameSpace", ex.ParamName);
        }

        #endregion

        #region ForService

        [Fact]
        public void ForService_SimpleName_ReturnsTrue()
        {
            var predicate = Predicates.ForService("TestService");
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));

            Assert.True(predicate(method));
        }

        [Fact]
        public void ForService_FullName_ReturnsTrue()
        {
            var predicate = Predicates.ForService(typeof(TestService).FullName);
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));

            Assert.True(predicate(method));
        }

        [Fact]
        public void ForService_NonMatchingName_ReturnsFalse()
        {
            var predicate = Predicates.ForService("NonExistentService");
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));

            Assert.False(predicate(method));
        }

        [Fact]
        public void ForService_WildcardName_ReturnsTrue()
        {
            var predicate = Predicates.ForService("Test*");
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));

            Assert.True(predicate(method));
        }

        [Fact]
        public void ForService_WildcardFullName_ReturnsTrue()
        {
            // Nested type FullName uses '+' (e.g. "...PredicatesTests+TestService"),
            // so a trailing wildcard without a leading dot matches the full name.
            var predicate = Predicates.ForService("*TestService");
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));

            Assert.True(predicate(method));
        }

        [Fact]
        public void ForService_WildcardNonMatching_ReturnsFalse()
        {
            var predicate = Predicates.ForService("Other*");
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));

            Assert.False(predicate(method));
        }

        [Fact]
        public void ForService_GenericType_StripsBacktick_ReturnsTrue()
        {
            var predicate = Predicates.ForService("GenericService");
            var method = typeof(GenericService<int>).GetMethod("Execute");

            Assert.True(predicate(method));
        }

        [Fact]
        public void ForService_GenericType_WildcardName_ReturnsTrue()
        {
            var predicate = Predicates.ForService("Generic*");
            var method = typeof(GenericService<string>).GetMethod("Execute");

            Assert.True(predicate(method));
        }

        [Fact]
        public void ForService_PropertyGetter_MatchesDeclaringType()
        {
            var predicate = Predicates.ForService("TestService");
            var method = typeof(TestService).GetMethod("get_Name");

            Assert.True(predicate(method));
        }

        [Fact]
        public void ForService_NullArgument_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => Predicates.ForService(null));
            Assert.Equal("service", ex.ParamName);
        }

        #endregion

        #region ForMethod(string method)

        [Fact]
        public void ForMethod_ExactName_ReturnsTrue()
        {
            var predicate = Predicates.ForMethod("Foo");
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));

            Assert.True(predicate(method));
        }

        [Fact]
        public void ForMethod_NonMatchingName_ReturnsFalse()
        {
            var predicate = Predicates.ForMethod("NonExistent");
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));

            Assert.False(predicate(method));
        }

        [Fact]
        public void ForMethod_WildcardSuffix_ReturnsTrue()
        {
            var predicate = Predicates.ForMethod("Fo*");
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));

            Assert.True(predicate(method));
        }

        [Fact]
        public void ForMethod_WildcardPrefix_ReturnsTrue()
        {
            var predicate = Predicates.ForMethod("*oo");
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));

            Assert.True(predicate(method));
        }

        [Fact]
        public void ForMethod_SingleCharWildcard_ReturnsTrue()
        {
            var predicate = Predicates.ForMethod("F?o");
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));

            Assert.True(predicate(method));
        }

        [Fact]
        public void ForMethod_PropertyGetter_ReturnsTrue()
        {
            var predicate = Predicates.ForMethod("get_Name");
            var method = typeof(TestService).GetMethod("get_Name");

            Assert.True(predicate(method));
        }

        [Fact]
        public void ForMethod_NullArgument_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => Predicates.ForMethod((string)null));
            Assert.Equal("method", ex.ParamName);
        }

        #endregion

        #region ForMethod(string service, string method)

        [Fact]
        public void ForMethod_ServiceAndMethod_BothMatch_ReturnsTrue()
        {
            var predicate = Predicates.ForMethod("TestService", "Foo");
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));

            Assert.True(predicate(method));
        }

        [Fact]
        public void ForMethod_ServiceMatches_MethodDoesNot_ReturnsFalse()
        {
            var predicate = Predicates.ForMethod("TestService", "NonExistent");
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));

            Assert.False(predicate(method));
        }

        [Fact]
        public void ForMethod_MethodMatches_ServiceDoesNot_ReturnsFalse()
        {
            var predicate = Predicates.ForMethod("NonExistentService", "Foo");
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));

            Assert.False(predicate(method));
        }

        [Fact]
        public void ForMethod_NeitherMatches_ReturnsFalse()
        {
            var predicate = Predicates.ForMethod("NonExistentService", "NonExistent");
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));

            Assert.False(predicate(method));
        }

        [Fact]
        public void ForMethod_WildcardService_WildcardMethod_ReturnsTrue()
        {
            var predicate = Predicates.ForMethod("Test*", "F*");
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));

            Assert.True(predicate(method));
        }

        [Fact]
        public void ForMethod_FullServiceName_ExactMethod_ReturnsTrue()
        {
            var predicate = Predicates.ForMethod(typeof(TestService).FullName, "Foo");
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));

            Assert.True(predicate(method));
        }

        [Fact]
        public void ForMethod_GenericService_WildcardMethod_ReturnsTrue()
        {
            var predicate = Predicates.ForMethod("GenericService", "Exec*");
            var method = typeof(GenericService<int>).GetMethod("Execute");

            Assert.True(predicate(method));
        }

        [Fact]
        public void ForMethod_NullService_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => Predicates.ForMethod(null, "Foo"));
            Assert.Equal("service", ex.ParamName);
        }

        [Fact]
        public void ForMethod_NullMethod_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => Predicates.ForMethod("TestService", null));
            Assert.Equal("method", ex.ParamName);
        }

        #endregion

        #region Implement

        [Fact]
        public void Implement_BaseClass_Assignable_ReturnsTrue()
        {
            var predicate = Predicates.Implement(typeof(TestService));
            var method = typeof(TestDerived).GetMethod(nameof(TestService.Foo));

            Assert.True(predicate(method));
        }

        [Fact]
        public void Implement_Interface_Implemented_ReturnsTrue()
        {
            var predicate = Predicates.Implement(typeof(ITestService));
            var method = typeof(TestDerived).GetMethod(nameof(ITestService.Run));

            Assert.True(predicate(method));
        }

        [Fact]
        public void Implement_AbstractBase_Assignable_ReturnsTrue()
        {
            var predicate = Predicates.Implement(typeof(AbstractBase));
            var method = typeof(ConcreteImpl).GetMethod(nameof(ConcreteImpl.Perform));

            Assert.True(predicate(method));
        }

        [Fact]
        public void Implement_NotAssignable_ReturnsFalse()
        {
            var predicate = Predicates.Implement(typeof(ITestService));
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));

            Assert.False(predicate(method));
        }

        [Fact]
        public void Implement_NullArgument_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => Predicates.Implement(null));
            Assert.Equal("baseOrInterfaceType", ex.ParamName);
        }

        [Fact]
        public void Implement_Enum_ThrowsArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() => Predicates.Implement(typeof(TestEnum)));
            Assert.Equal("The base type must be class or interface.", ex.Message);
        }

        [Fact]
        public void Implement_SealedClass_ThrowsArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() => Predicates.Implement(typeof(SealedService)));
            Assert.Equal("The base type is not allowed to be Sealed.", ex.Message);
        }

        #endregion

        #region Matches (StringExtensions wildcard behavior)

        [Theory]
        [InlineData("AspectCore.Core.Tests", "AspectCore.Core.Tests", true)]
        [InlineData("AspectCore.Core.Tests", "AspectCore.*", true)]
        [InlineData("AspectCore.Core.Tests", "*.Tests", true)]
        [InlineData("AspectCore.Core.Tests", "*Tests*", true)]
        [InlineData("AspectCore.Core.Tests", "*.Tests.Configuration", false)]
        [InlineData("Foo", "F?o", true)]
        [InlineData("Foo", "F?o?", false)]
        [InlineData("Foo", "*", true)]
        [InlineData("Foo", "Bar", false)]
        [InlineData("get_Name", "get_*", true)]
        [InlineData("get_Name", "*_Name", true)]
        [InlineData("GenericService`1", "GenericService", false)]
        public void Matches_VariousPatterns_ReturnsExpected(string input, string pattern, bool expected)
        {
            Assert.Equal(expected, input.Matches(pattern));
        }

        [Fact]
        public void Matches_NullInput_ThrowsArgumentNullException()
        {
            string input = null;
            var ex = Assert.Throws<ArgumentNullException>(() => input.Matches("pattern"));
            Assert.Equal("input", ex.ParamName);
        }

        [Fact]
        public void Matches_EmptyInput_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => string.Empty.Matches("pattern"));
            Assert.Equal("input", ex.ParamName);
        }

        [Fact]
        public void Matches_NullPattern_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => "input".Matches(null));
            Assert.Equal("pattern", ex.ParamName);
        }

        [Fact]
        public void Matches_EmptyPattern_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => "input".Matches(string.Empty));
            Assert.Equal("pattern", ex.ParamName);
        }

        #endregion
    }
}
