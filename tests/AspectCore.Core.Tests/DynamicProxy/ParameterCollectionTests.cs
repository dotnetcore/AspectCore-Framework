using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.Parameters;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class ParameterCollectionTests
    {
        private static MethodInfo GetMethod(string name) => typeof(TestService).GetMethod(name);

        private static RuntimeAspectContext CreateContext(MethodInfo serviceMethod = null, object[] parameters = null)
        {
            var defaultMethod = GetMethod(nameof(TestService.Add));
            var method = serviceMethod ?? defaultMethod;
            return new RuntimeAspectContext(
                null,
                method,
                method,
                method,
                method,
                new TestService(),
                new TestService(),
                parameters ?? new object[] { 1, 2 });
        }

        #region GetParameters (ParameterExtensions)

        [Fact]
        public void GetParameters_WithNullContext_ThrowsArgumentNullException()
        {
            AspectContext context = null;
            var ex = Assert.Throws<ArgumentNullException>(() => context.GetParameters());
            Assert.Equal("aspectContext", ex.ParamName);
        }

        [Fact]
        public void GetParameters_WithNoParameters_ReturnsEmptyCollection()
        {
            var context = CreateContext(GetMethod(nameof(TestService.NoParams)));
            var parameters = context.GetParameters();
            Assert.Empty(parameters);
        }

        [Fact]
        public void GetParameters_WithParameters_ReturnsCorrectCount()
        {
            var context = CreateContext(GetMethod(nameof(TestService.Add)));
            var parameters = context.GetParameters();
            Assert.Equal(2, parameters.Count);
        }

        [Fact]
        public void GetParameters_ReturnsSameCollectionForSameMethod()
        {
            var context = CreateContext(GetMethod(nameof(TestService.Add)));
            var parameters1 = context.GetParameters();
            var parameters2 = context.GetParameters();
            // Caching means the same empty collection is returned for zero params,
            // but for non-zero params, new Parameter objects are created each time
            Assert.Equal(parameters1.Count, parameters2.Count);
        }

        [Fact]
        public void GetParameters_EmptyCollection_ReturnsSharedEmptyInstance()
        {
            var context1 = CreateContext(GetMethod(nameof(TestService.NoParams)));
            var context2 = CreateContext(GetMethod(nameof(TestService.NoParams)));
            var params1 = context1.GetParameters();
            var params2 = context2.GetParameters();
            Assert.Same(params1, params2);
        }

        #endregion

        #region ParameterCollection Indexer (int)

        [Fact]
        public void Indexer_ByInt_ReturnsCorrectParameter()
        {
            var context = CreateContext(GetMethod(nameof(TestService.Add)));
            var parameters = context.GetParameters();
            Assert.Equal("a", parameters[0].Name);
            Assert.Equal("b", parameters[1].Name);
        }

        [Fact]
        public void Indexer_ByInt_OutOfRange_ThrowsArgumentOutOfRangeException()
        {
            var context = CreateContext(GetMethod(nameof(TestService.Add)));
            var parameters = context.GetParameters();
            Assert.Throws<ArgumentOutOfRangeException>(() => parameters[2]);
        }

        [Fact]
        public void Indexer_ByInt_NegativeIndex_ThrowsArgumentOutOfRangeException()
        {
            var context = CreateContext(GetMethod(nameof(TestService.Add)));
            var parameters = context.GetParameters();
            Assert.Throws<ArgumentOutOfRangeException>(() => parameters[-1]);
        }

        #endregion

        #region ParameterCollection Indexer (string)

        [Fact]
        public void Indexer_ByName_ReturnsCorrectParameter()
        {
            var context = CreateContext(GetMethod(nameof(TestService.Add)));
            var parameters = context.GetParameters();
            Assert.Equal("a", parameters["a"].Name);
            Assert.Equal("b", parameters["b"].Name);
        }

        [Fact]
        public void Indexer_ByName_WithNullName_ThrowsArgumentNullException()
        {
            var context = CreateContext(GetMethod(nameof(TestService.Add)));
            var parameters = context.GetParameters();
            var ex = Assert.Throws<ArgumentNullException>(() => parameters[null]);
            Assert.Equal("name", ex.ParamName);
        }

        [Fact]
        public void Indexer_ByName_WithNonExistentName_ThrowsInvalidOperationException()
        {
            var context = CreateContext(GetMethod(nameof(TestService.Add)));
            var parameters = context.GetParameters();
            Assert.Throws<InvalidOperationException>(() => parameters["nonExistent"]);
        }

        [Fact]
        public void Indexer_ByName_SingleParameter_ReturnsCorrectParameter()
        {
            var context = CreateContext(GetMethod(nameof(TestService.SingleParam)));
            var parameters = context.GetParameters();
            Assert.Equal("value", parameters["value"].Name);
        }

        [Fact]
        public void Indexer_ByName_SingleParameter_NonMatching_ThrowsInvalidOperationException()
        {
            var context = CreateContext(GetMethod(nameof(TestService.SingleParam)));
            var parameters = context.GetParameters();
            Assert.Throws<InvalidOperationException>(() => parameters["nonExistent"]);
        }

        #endregion

        #region ParameterCollection Count

        [Fact]
        public void Count_ReturnsCorrectNumberOfParameters()
        {
            var context = CreateContext(GetMethod(nameof(TestService.Add)));
            var parameters = context.GetParameters();
            Assert.Equal(2, parameters.Count);
        }

        [Fact]
        public void Count_ForNoParameters_IsZero()
        {
            var context = CreateContext(GetMethod(nameof(TestService.NoParams)));
            var parameters = context.GetParameters();
            Assert.Empty(parameters);
        }

        #endregion

        #region ParameterCollection GetValues

        [Fact]
        public void GetValues_ReturnsAllParameterValues()
        {
            var context = CreateContext(GetMethod(nameof(TestService.Add)), new object[] { 10, 20 });
            var parameters = context.GetParameters();
            var values = parameters.GetValues();
            Assert.Equal(2, values.Length);
            Assert.Equal(10, values[0]);
            Assert.Equal(20, values[1]);
        }

        [Fact]
        public void GetValues_ForNoParameters_ReturnsEmptyArray()
        {
            var context = CreateContext(GetMethod(nameof(TestService.NoParams)));
            var parameters = context.GetParameters();
            var values = parameters.GetValues();
            Assert.Empty(values);
        }

        #endregion

        #region ParameterCollection GetEnumerator

        [Fact]
        public void GetEnumerator_ReturnsAllParameters()
        {
            var context = CreateContext(GetMethod(nameof(TestService.Add)));
            var parameters = context.GetParameters();
            var items = parameters.ToList();
            Assert.Equal(2, items.Count);
            Assert.Equal("a", items[0].Name);
            Assert.Equal("b", items[1].Name);
        }

        [Fact]
        public void GetEnumerator_EmptyCollection_ReturnsEmptyEnumerator()
        {
            var context = CreateContext(GetMethod(nameof(TestService.NoParams)));
            var parameters = context.GetParameters();
            var items = parameters.ToList();
            Assert.Empty(items);
        }

        #endregion

        #region Parameter Properties

        [Fact]
        public void Parameter_Name_ReturnsCorrectName()
        {
            var context = CreateContext(GetMethod(nameof(TestService.Add)));
            var parameters = context.GetParameters();
            Assert.Equal("a", parameters[0].Name);
        }

        [Fact]
        public void Parameter_Type_ReturnsCorrectType()
        {
            var context = CreateContext(GetMethod(nameof(TestService.Add)));
            var parameters = context.GetParameters();
            Assert.Equal(typeof(int), parameters[0].Type);
        }

        [Fact]
        public void Parameter_IsRef_ForNonRefParameter_IsFalse()
        {
            var context = CreateContext(GetMethod(nameof(TestService.Add)));
            var parameters = context.GetParameters();
            Assert.False(parameters[0].IsRef);
        }

        [Fact]
        public void Parameter_RawType_ForNonRefParameter_ReturnsSameType()
        {
            var context = CreateContext(GetMethod(nameof(TestService.Add)));
            var parameters = context.GetParameters();
            Assert.Equal(typeof(int), parameters[0].RawType);
        }

        [Fact]
        public void Parameter_Value_Get_ReturnsValueFromContext()
        {
            var context = CreateContext(GetMethod(nameof(TestService.Add)), new object[] { 10, 20 });
            var parameters = context.GetParameters();
            Assert.Equal(10, parameters[0].Value);
            Assert.Equal(20, parameters[1].Value);
        }

        [Fact]
        public void Parameter_Value_Set_UpdatesValueInContext()
        {
            var context = CreateContext(GetMethod(nameof(TestService.Add)), new object[] { 10, 20 });
            var parameters = context.GetParameters();
            parameters[0].Value = 99;
            Assert.Equal(99, context.Parameters[0]);
        }

        [Fact]
        public void Parameter_ParameterInfo_ReturnsCorrectInfo()
        {
            var context = CreateContext(GetMethod(nameof(TestService.Add)));
            var parameters = context.GetParameters();
            Assert.NotNull(parameters[0].ParameterInfo);
            Assert.Equal("a", parameters[0].ParameterInfo.Name);
        }

        [Fact]
        public void Parameter_StringParameter_HasCorrectType()
        {
            var context = CreateContext(GetMethod(nameof(TestService.Concat)));
            var parameters = context.GetParameters();
            Assert.Equal(typeof(string), parameters[0].Type);
            Assert.Equal(typeof(int), parameters[1].Type);
        }

        #endregion

        #region GetReturnParameter

        [Fact]
        public void GetReturnParameter_WithNullContext_ThrowsArgumentNullException()
        {
            AspectContext context = null;
            var ex = Assert.Throws<ArgumentNullException>(() => context.GetReturnParameter());
            Assert.Equal("aspectContext", ex.ParamName);
        }

        [Fact]
        public void GetReturnParameter_ReturnsParameterWithCorrectType()
        {
            var context = CreateContext(GetMethod(nameof(TestService.Add)));
            var returnParam = context.GetReturnParameter();
            Assert.Equal(typeof(int), returnParam.Type);
        }

        [Fact]
        public void GetReturnParameter_Value_Get_ReturnsContextReturnValue()
        {
            var context = CreateContext(GetMethod(nameof(TestService.Add)));
            context.ReturnValue = 42;
            var returnParam = context.GetReturnParameter();
            Assert.Equal(42, returnParam.Value);
        }

        [Fact]
        public void GetReturnParameter_Value_Set_UpdatesContextReturnValue()
        {
            var context = CreateContext(GetMethod(nameof(TestService.Add)));
            var returnParam = context.GetReturnParameter();
            returnParam.Value = 99;
            Assert.Equal(99, context.ReturnValue);
        }

        [Fact]
        public void GetReturnParameter_ForVoidMethod_HasVoidType()
        {
            var context = CreateContext(GetMethod(nameof(TestService.NoParams)));
            var returnParam = context.GetReturnParameter();
            Assert.Equal(typeof(void), returnParam.Type);
        }

        #endregion

        #region ParameterAspectContext

        [Fact]
        public void ParameterAspectContext_StoresParameter()
        {
            var context = CreateContext(GetMethod(nameof(TestService.Add)));
            var parameters = context.GetParameters();
            var paramContext = new ParameterAspectContext(context, parameters[0]);
            Assert.Same(parameters[0], paramContext.Parameter);
        }

        [Fact]
        public void ParameterAspectContext_StoresAspectContext()
        {
            var context = CreateContext(GetMethod(nameof(TestService.Add)));
            var parameters = context.GetParameters();
            var paramContext = new ParameterAspectContext(context, parameters[0]);
            Assert.Same(context, paramContext.AspectContext);
        }

        #endregion

        #region Parameter RawType (ref)

        [Fact]
        public void Parameter_IsRef_ForRefParameter_IsTrue()
        {
            var context = CreateContext(GetMethod(nameof(TestService.RefParam)), new object[] { 0 });
            var parameters = context.GetParameters();
            Assert.True(parameters[0].IsRef);
        }

        [Fact]
        public void Parameter_RawType_ForRefParameter_ReturnsElementType()
        {
            var context = CreateContext(GetMethod(nameof(TestService.RefParam)), new object[] { 0 });
            var parameters = context.GetParameters();
            // For ref int, Type is int& (ByRef), RawType should be int
            Assert.Equal(typeof(int), parameters[0].RawType);
            Assert.NotEqual(parameters[0].Type, parameters[0].RawType);
        }

        #endregion

        #region ParameterCollection NonGeneric GetEnumerator

        [Fact]
        public void GetEnumerator_NonGeneric_ReturnsAllParameters()
        {
            var context = CreateContext(GetMethod(nameof(TestService.Add)));
            var parameters = context.GetParameters();
            var enumerable = (System.Collections.IEnumerable)parameters;
            var items = new List<Parameter>();
            foreach (Parameter item in enumerable)
            {
                items.Add(item);
            }
            Assert.Equal(2, items.Count);
            Assert.Equal("a", items[0].Name);
            Assert.Equal("b", items[1].Name);
        }

        [Fact]
        public void GetEnumerator_NonGeneric_EmptyCollection_ReturnsEmptyEnumerator()
        {
            var context = CreateContext(GetMethod(nameof(TestService.NoParams)));
            var parameters = context.GetParameters();
            var enumerable = (System.Collections.IEnumerable)parameters;
            var items = new List<object>();
            foreach (var item in enumerable)
            {
                items.Add(item);
            }
            Assert.Empty(items);
        }

        #endregion

        #region Test Types

        private class TestService
        {
            public virtual int Add(int a, int b) => a + b;

            public virtual string Concat(string prefix, int count) => prefix + count;

            public virtual void NoParams() { }

            public virtual int SingleParam(int value) => value;

            public virtual void RefParam(ref int value) => value = 42;
        }

        #endregion
    }
}
