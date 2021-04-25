using System;
using System.Linq;
using System.Reflection;

namespace AspectCore.Extensions.Reflection
{
    /// <summary>
    /// 参数反射操作
    /// </summary>
    public partial class ParameterReflector : ICustomAttributeReflectorProvider
    {
        private readonly ParameterInfo _reflectionInfo;
        private readonly CustomAttributeReflector[] _customAttributeReflectors;

        public CustomAttributeReflector[] CustomAttributeReflectors => _customAttributeReflectors;

        /// <summary>
        /// 参数名
        /// </summary>
        public string Name => _reflectionInfo.Name;

        /// <summary>
        /// 参数是否有默认值
        /// </summary>
        public bool HasDeflautValue { get; }

        /// <summary>
        /// 参数默认值
        /// </summary>
        public object DefalutValue { get; }

        /// <summary>
        /// 参数在形参表中的位置（从零开始）
        /// </summary>
        public int Position { get; }

        /// <summary>
        /// 参数类型
        /// </summary>
        public Type ParameterType { get; }

        /// <summary>
        /// 参数反射操作对象
        /// </summary>
        /// <param name="reflectionInfo">参数对象</param>
        private ParameterReflector(ParameterInfo reflectionInfo)
        {
            _reflectionInfo = reflectionInfo ?? throw new ArgumentNullException(nameof(reflectionInfo));
            _customAttributeReflectors = _reflectionInfo.CustomAttributes.Select(data => CustomAttributeReflector.Create(data)).ToArray();
            HasDeflautValue = reflectionInfo.HasDefaultValueByAttributes();
            if (HasDeflautValue)
            {
                DefalutValue = reflectionInfo.DefaultValueSafely();
            }
            Position = reflectionInfo.Position;
            ParameterType = reflectionInfo.ParameterType;
        }

        /// <summary>
        /// 获取参数对象
        /// </summary>
        /// <returns>参数对象</returns>
        public ParameterInfo GetParameterInfo()
        {
            return _reflectionInfo;
        }

        /// <summary>
        /// 获取友好的字符串表示
        /// </summary>
        /// <returns>字符串表示</returns>
        public override string ToString() => $"Parameter : {_reflectionInfo}  ParameterType : {ParameterType}";
    }
}