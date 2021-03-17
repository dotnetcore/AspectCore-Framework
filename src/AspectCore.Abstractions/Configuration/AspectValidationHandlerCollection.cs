using System;
using System.Collections;
using System.Collections.Generic;
using AspectCore.DynamicProxy;

namespace AspectCore.Configuration
{
    /// <summary>
    /// 代表AspectCore验证处理器集合的对象
    /// </summary>
    public class AspectValidationHandlerCollection: IEnumerable<IAspectValidationHandler>
    {
        private readonly HashSet<IAspectValidationHandler> _sets = new HashSet<IAspectValidationHandler>(new ValidationHandlerEqualityComparer());

        /// <summary>
        /// 添加验证处理器
        /// </summary>
        /// <param name="aspectValidationHandler">验证处理器</param>
        /// <returns>代表AspectCore验证处理器集合的对象</returns>
        public AspectValidationHandlerCollection Add(IAspectValidationHandler aspectValidationHandler)
        {
            if (aspectValidationHandler == null)
            {
                throw new ArgumentNullException(nameof(aspectValidationHandler));
            }
            _sets.Add(aspectValidationHandler);
            return this;
        }

        /// <summary>
        /// 验证处理器数量
        /// </summary>
        public int Count => _sets.Count;

        /// <summary>
        /// 迭代AspectValidationHandlerCollection包含的验证处理器
        /// </summary>
        /// <returns>迭代器,用以迭代此对象包含的验证处理器</returns>
        public IEnumerator<IAspectValidationHandler> GetEnumerator()
        {
            return _sets.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// IAspectValidationHandler相等比较器
        /// </summary>
        private class ValidationHandlerEqualityComparer : IEqualityComparer<IAspectValidationHandler>
        {
            public bool Equals(IAspectValidationHandler x, IAspectValidationHandler y)
            {
                if (x == null || y == null) return false;
                return x.GetType().Equals(y.GetType());
            }

            public int GetHashCode(IAspectValidationHandler obj)
            {
                return obj.GetType().GetHashCode();
            }
        }
    }
}