using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class ProxyEngineOptionsTests
    {
        #region Default Values

        [Fact]
        public void Engine_DefaultsToDynamicProxy()
        {
            var options = new ProxyEngineOptions();
            Assert.Equal(ProxyEngine.DynamicProxy, options.Engine);
        }

        [Fact]
        public void AllowRuntimeFallback_DefaultsToNull()
        {
            var options = new ProxyEngineOptions();
            Assert.Null(options.AllowRuntimeFallback);
        }

        [Fact]
        public void Strict_DefaultsToFalse()
        {
            var options = new ProxyEngineOptions();
            Assert.False(options.Strict);
        }

        #endregion

        #region Engine

        [Theory]
        [InlineData(ProxyEngine.DynamicProxy)]
        [InlineData(ProxyEngine.SourceGenerator)]
        [InlineData(ProxyEngine.Auto)]
        public void Engine_CanBeSetToAllValues(ProxyEngine engine)
        {
            var options = new ProxyEngineOptions { Engine = engine };
            Assert.Equal(engine, options.Engine);
        }

        [Fact]
        public void Engine_CanBeChanged()
        {
            var options = new ProxyEngineOptions();
            options.Engine = ProxyEngine.SourceGenerator;
            Assert.Equal(ProxyEngine.SourceGenerator, options.Engine);
            options.Engine = ProxyEngine.Auto;
            Assert.Equal(ProxyEngine.Auto, options.Engine);
        }

        #endregion

        #region AllowRuntimeFallback

        [Fact]
        public void AllowRuntimeFallback_CanBeSetToTrue()
        {
            var options = new ProxyEngineOptions { AllowRuntimeFallback = true };
            Assert.True(options.AllowRuntimeFallback);
        }

        [Fact]
        public void AllowRuntimeFallback_CanBeSetToFalse()
        {
            var options = new ProxyEngineOptions { AllowRuntimeFallback = false };
            Assert.False(options.AllowRuntimeFallback);
        }

        [Fact]
        public void AllowRuntimeFallback_CanBeSetToNull()
        {
            var options = new ProxyEngineOptions { AllowRuntimeFallback = true };
            options.AllowRuntimeFallback = null;
            Assert.Null(options.AllowRuntimeFallback);
        }

        #endregion

        #region Strict

        [Fact]
        public void Strict_CanBeSetToTrue()
        {
            var options = new ProxyEngineOptions { Strict = true };
            Assert.True(options.Strict);
        }

        [Fact]
        public void Strict_CanBeToggled()
        {
            var options = new ProxyEngineOptions();
            Assert.False(options.Strict);
            options.Strict = true;
            Assert.True(options.Strict);
            options.Strict = false;
            Assert.False(options.Strict);
        }

        #endregion

        #region ProxyEngine Enum

        [Fact]
        public void ProxyEngine_DynamicProxy_HasValueZero()
        {
            Assert.Equal(0, (int)ProxyEngine.DynamicProxy);
        }

        [Fact]
        public void ProxyEngine_SourceGenerator_HasValueOne()
        {
            Assert.Equal(1, (int)ProxyEngine.SourceGenerator);
        }

        [Fact]
        public void ProxyEngine_Auto_HasValueTwo()
        {
            Assert.Equal(2, (int)ProxyEngine.Auto);
        }

        #endregion
    }
}
