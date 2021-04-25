using System.Linq;

namespace AspectCore.Extensions.DataValidation
{
    /// <summary>
    /// 获取数据状态对象的工厂
    /// </summary>
    public class DataStateFactory : IDataStateFactory
    {
        /// <summary>
        /// 通过数据检验上下文获取数据状态
        /// </summary>
        /// <param name="dataValidationContext">数据校验上下文</param>
        /// <returns>数据状态</returns>
        public IDataState CreateDataState(DataValidationContext dataValidationContext)
        {
            var dataValidationErrors = new DataValidationErrorCollection();
            foreach (var error in dataValidationContext.DataMetaDatas.SelectMany(x => x.Errors))
                dataValidationErrors.Add(error);
            var isValid = dataValidationContext.DataMetaDatas.All(x => x.State != DataValidationState.Invalid);
            return new DataState(isValid, dataValidationErrors);
        }
    }
}