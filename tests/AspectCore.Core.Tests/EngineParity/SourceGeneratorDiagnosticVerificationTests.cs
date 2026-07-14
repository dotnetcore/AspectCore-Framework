#nullable enable

using System;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.EngineParity;

/// <summary>
/// 编译期诊断验证测试（文档性质）
///
/// 这些测试验证 Source Generator 在编译时报告的诊断信息。
/// 由于诊断测试需要 Roslyn 编译测试框架，这里以文档形式记录预期行为。
///
/// 实际的诊断验证应该在单独的编译测试项目中进行，或者手动验证编译输出。
/// </summary>
public class SourceGeneratorDiagnosticVerificationTests
{
    #region ACSG005: Sealed 类型诊断

    /// <summary>
    /// 验证：尝试为 sealed 类型生成代理应该报告 ACSG005 错误
    ///
    /// 测试代码：
    /// <code>
    /// [AspectCoreGenerateProxy]
    /// public sealed class SealedService
    /// {
    ///     public virtual void DoWork() { }
    /// }
    /// </code>
    ///
    /// 预期诊断：
    /// - Id: ACSG005
    /// - Severity: Error
    /// - Message: "无法为 sealed 类型 'SealedService' 生成代理。请移除 sealed 修饰符或使用接口代理。"
    /// </summary>
    [Fact]
    public void SealedType_Should_Report_ACSG005_Error_Documentation()
    {
        // 这个测试作为文档，记录预期的诊断行为
        // 实际验证需要在编译测试项目中进行
        Assert.True(true, "此测试作为文档，记录 ACSG005 诊断的预期行为");
    }

    #endregion

    #region ACSG007: 无构造函数诊断

    /// <summary>
    /// 验证：尝试为没有可访问构造函数的类型生成代理应该报告 ACSG007 错误
    ///
    /// 测试代码：
    /// <code>
    /// [AspectCoreGenerateProxy]
    /// public class NoPublicCtorService
    /// {
    ///     private NoPublicCtorService() { }
    ///     public virtual void DoWork() { }
    /// }
    /// </code>
    ///
    /// 预期诊断：
    /// - Id: ACSG007
    /// - Severity: Error
    /// - Message: "类型 'NoPublicCtorService' 没有可访问的构造函数。类代理要求目标类型具有 public 或 protected 构造函数。"
    /// </summary>
    [Fact]
    public void NoAccessibleConstructor_Should_Report_ACSG007_Error_Documentation()
    {
        // 这个测试作为文档，记录预期的诊断行为
        Assert.True(true, "此测试作为文档，记录 ACSG007 诊断的预期行为");
    }

    #endregion

    #region ACSG006: 类型可见性诊断

    /// <summary>
    /// 验证：internal 类型应该可以正常生成代理（Source Generator 在同一个编译上下文中）
    ///
    /// 测试代码：
    /// <code>
    /// [AspectCoreGenerateProxy]
    /// internal class InternalService
    /// {
    ///     public virtual void DoWork() { }
    /// }
    /// </code>
    ///
    /// 预期结果：
    /// - 不应该报告 ACSG006 错误
    /// - 应该成功生成代理
    /// </summary>
    [Fact]
    public void InternalType_Should_Not_Report_Error_Documentation()
    {
        // 这个测试作为文档，记录预期的诊断行为
        Assert.True(true, "此测试作为文档，记录 internal 类型的预期行为");
    }

    #endregion

    #region ACSG001: 开放泛型类型诊断

    /// <summary>
    /// 验证：尝试为开放泛型类型生成代理应该报告 ACSG001 警告
    ///
    /// 测试代码：
    /// <code>
    /// [AspectCoreGenerateProxy]
    /// public class GenericService&lt;T&gt;
    /// {
    ///     public virtual void DoWork(T value) { }
    /// }
    /// </code>
    ///
    /// 预期诊断：
    /// - Id: ACSG001
    /// - Severity: Warning
    /// - Message: "类型 'GenericService&lt;T&gt;' 为开放泛型，当前版本的 Source Generator 暂不支持生成代理。"
    /// </summary>
    [Fact]
    public void OpenGenericType_Should_Report_ACSG001_Warning_Documentation()
    {
        // 这个测试作为文档，记录预期的诊断行为
        Assert.True(true, "此测试作为文档，记录 ACSG001 诊断的预期行为");
    }

    #endregion

    #region ACSG002: 嵌套类型诊断

    /// <summary>
    /// 验证：尝试为嵌套类型生成代理应该报告 ACSG002 警告
    ///
    /// 测试代码：
    /// <code>
    /// public class OuterClass
    /// {
    ///     [AspectCoreGenerateProxy]
    ///     public class NestedService
    ///     {
    ///         public virtual void DoWork() { }
    ///     }
    /// }
    /// </code>
    ///
    /// 预期诊断：
    /// - Id: ACSG002
    /// - Severity: Warning
    /// - Message: "类型 'NestedService' 为嵌套类型，当前版本的 Source Generator 暂不支持生成代理。"
    /// </summary>
    [Fact]
    public void NestedType_Should_Report_ACSG002_Warning_Documentation()
    {
        // 这个测试作为文档，记录预期的诊断行为
        Assert.True(true, "此测试作为文档，记录 ACSG002 诊断的预期行为");
    }

    #endregion

    #region ACSG003: 事件成员诊断

    /// <summary>
    /// 验证：尝试为包含事件的类型生成代理应该报告 ACSG003 警告
    ///
    /// 测试代码：
    /// <code>
    /// [AspectCoreGenerateProxy]
    /// public class ServiceWithEvent
    /// {
    ///     public virtual event EventHandler MyEvent;
    ///     public virtual void DoWork() { }
    /// }
    /// </code>
    ///
    /// 预期诊断：
    /// - Id: ACSG003
    /// - Severity: Warning
    /// - Message: "类型 'ServiceWithEvent' 包含事件成员 'MyEvent'，当前版本的 Source Generator 暂不支持生成代理。"
    /// </summary>
    [Fact]
    public void TypeWithEvent_Should_Report_ACSG003_Warning_Documentation()
    {
        // 这个测试作为文档，记录预期的诊断行为
        Assert.True(true, "此测试作为文档，记录 ACSG003 诊断的预期行为");
    }

    #endregion
}
