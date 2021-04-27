using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.DataValidation
{
    /// <summary>
    /// 获取数据状态对象的工厂
    /// </summary>
    [NonAspect]
    public interface IDataStateFactory
    {
        /// <summary>
        /// 通过数据检验上下文获取数据状态
        /// </summary>
        /// <param name="dataValidationContext">数据校验上下文</param>
        /// <returns>数据状态</returns>
        IDataState CreateDataState(DataValidationContext dataValidationContext);
    }
}