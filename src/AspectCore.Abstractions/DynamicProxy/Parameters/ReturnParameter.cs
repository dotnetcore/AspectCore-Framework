using System.Reflection;

namespace AspectCore.DynamicProxy.Parameters
{
    /// <summary>
    /// Parameter的派生类,代表一个返回值
    /// </summary>
    internal sealed class ReturnParameter : Parameter
    {
        /// <summary>
        /// 返回值
        /// </summary>
        public override object Value
        {
            get
            {
                return _context.ReturnValue;
            }
            set
            {
                _context.ReturnValue = value;
            }
        }

        /// <summary>
        /// 构造一个代表返回值的ReturnParameter对象
        /// </summary>
        /// <param name="context">拦截上下文</param>
        /// <param name="reflector">参数类型对象</param>
        internal ReturnParameter(AspectContext context, ParameterInfo reflector)
            : base(context, -1, reflector)
        {
        }
    }
}
