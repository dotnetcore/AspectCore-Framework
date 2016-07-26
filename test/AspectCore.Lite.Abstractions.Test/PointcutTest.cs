using AspectCore.Lite.Abstractions.Aspects;
using AspectCore.Lite.Abstractions.Test.Fakes;
using Microsoft.AspNetCore.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace AspectCore.Lite.Abstractions.Test
{
    //public class PointcutTest
    //{

    //    [Theory]
    //    [InlineData("member")]
    //    public void IsMatch_ThrowsArgumentNullException(string paramName)
    //    {
    //        ExceptionAssert.ThrowsArgumentNull(() => new AllMethodPointcut().IsMatch(null), paramName);
    //    }

    //    [Theory]
    //    [InlineData("member", "member AllMethodPointcut is not a Method.")]
    //    public void IsMatch_ThrowsArgumentException(string paramName,string exectionMessage)
    //    {
    //        IPointcut pointcut = new AllMethodPointcut();
    //        ExceptionAssert.ThrowsArgument(() => pointcut.IsMatch(typeof(AllMethodPointcut).GetTypeInfo()), paramName, exectionMessage);
    //    }
    //}
}
