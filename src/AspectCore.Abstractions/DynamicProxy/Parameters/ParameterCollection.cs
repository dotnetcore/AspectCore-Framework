using System;
using System.Collections;
using System.Collections.Generic;

namespace AspectCore.DynamicProxy.Parameters
{
    /// <summary>
    /// 包装Parameter类型数组的对象
    /// </summary>
    public sealed class ParameterCollection : IEnumerable<Parameter>, IReadOnlyList<Parameter>
    {
        private static readonly object[] emptyValues = new object[0];
        private readonly int _count;
        private readonly Parameter[] _parameterEntries;

        /// <summary>
        /// 通过Parameter数组构造ParameterCollection
        /// </summary>
        /// <param name="parameters">包装Parameter类型数组的对象</param>
        internal ParameterCollection(Parameter[] parameters)
        {
            _count = parameters.Length;
            _parameterEntries = parameters;
        }

        /// <summary>
        /// 访问ParameterCollection对象中此索引对应的Parameter类型对象
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>index索引对应的Parameter对象</returns>
        public Parameter this[int index]
        {
            get
            {
                if (index < 0 || index >= _count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), "index value out of range.");
                }
                return _parameterEntries[index];
            }
        }

        /// <summary>
        /// 访问ParameterCollection对象中此名称索引对应的Parameter类型对象
        /// </summary>
        /// <param name="name">名称索引(通过Parameter类型的Name属性进行判断获取)</param>
        /// <returns>此名称对应的参数对象</returns>
        public Parameter this[string name]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }
                var count = _count;
                var parameters = _parameterEntries;
                if (count == 1)
                {
                    var descriptor = parameters[0];
                    if (descriptor.Name == name)
                    {
                        return descriptor;
                    }
                    throw ThrowNotFound();
                }
                Parameter parameter;
                for (var i = 0; i < count; i++)
                {
                    parameter = parameters[i];
                    if (parameters[i].Name == name)
                        return parameter;
                }
                throw ThrowNotFound();
                InvalidOperationException ThrowNotFound()
                {
                    return new InvalidOperationException($"Not found the parameter named \"{name}\".");
                }
            }
        }

        /// <summary>
        /// 参数个数
        /// </summary>
        public int Count
        {
            get { return _count; }
        }

        /// <summary>
        /// 枚举集合中参数
        /// </summary>
        /// <returns>参数枚举器</returns>
        public IEnumerator<Parameter> GetEnumerator()
        {
            for (var i = 0; i < _count; i++)
            {
                yield return _parameterEntries[i];
            }
        }

        /// <summary>
        /// 获取对象中的所有参数值
        /// </summary>
        /// <returns>对象中的所有参数值</returns>
        public object[] GetValues()
        {
            if (_count == 0)
            {
                return emptyValues;
            }
            var values = new object[_count];
            for (var i = 0; i < _count; i++)
            {
                values[i] = _parameterEntries[i].Value;
            }
            return values;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}