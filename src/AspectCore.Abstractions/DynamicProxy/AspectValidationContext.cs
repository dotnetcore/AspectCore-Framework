using System.Reflection;
using System;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 拦截验证上下文(当拦截验证器Invoke方法返回true,表示要对待拦截方法进行拦截处理以生成代理)
    /// </summary>
    public struct AspectValidationContext : IEquatable<AspectValidationContext>
    {
        /// <summary>
        /// 待验证的方法
        /// </summary>
        public MethodInfo Method { get; set; }

        /// <summary>
        /// 严格验证模式
        /// </summary>
        public bool StrictValidation { get; set; }

        /// <summary>
        /// 判断另一个拦截验证上下文是否与之相等(验证的方法和模式相同则两个拦截验证器相等)
        /// </summary>
        /// <param name="other">与之比较的验证上下文</param>
        /// <returns>是否相等</returns>
        public bool Equals(AspectValidationContext other)
        {
            return Method == other.Method && StrictValidation == other.StrictValidation;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is AspectActivatorContext other)
                return Equals(other);
            return false;
        }

        public override int GetHashCode()
        {
            var hash_1 = this.Method?.GetHashCode() ?? 0;
            var hash_2 = StrictValidation.GetHashCode();
            return (hash_1 << 5) + hash_1 ^ hash_2;
        }
    }
}