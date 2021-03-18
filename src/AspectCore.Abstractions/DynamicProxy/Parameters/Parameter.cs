using System;
using System.Reflection;

namespace AspectCore.DynamicProxy.Parameters
{
    /// <summary>
    /// 封装描述参数上下文信息的对象,派生类ReturnParameter代表返回值上下文信息
    /// </summary>
    public class Parameter
    {
        /// <summary>
        /// 拦截上下文
        /// </summary>
        protected readonly AspectContext _context;

        /// <summary>
        /// 参数索引(代表返回值时,此值为-1)
        /// </summary>
        protected readonly int _index;

        /// <summary>
        /// 参数名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 参数类型
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// 当在派生类中重写时，返回当前数组、指针或引用类型包含的或引用的对象的 Type
        /// </summary>
        ///<example>
        /// int[] ----> System.Int32
        ///</example>
        public Type RawType
        {
            get
            {
                if (IsRef)
                {
                    return Type.GetElementType();
                }
                return Type;
            }
        }

        /// <summary>
        /// 获取一个值，该值指示 Type 是否由引用传递。
        /// </summary>
        public bool IsRef { get; }

        /// <summary>
        /// 值
        /// </summary>
        public virtual object Value
        {
            get
            {
                return _context.Parameters[_index];
            }
            set
            {
                _context.Parameters[_index] = value;
            }
        }

        /// <summary>
        /// 参数的类型对象
        /// </summary>
        public ParameterInfo ParameterInfo { get; }

        /// <summary>
        /// 构造一个描述参数信息的对象
        /// </summary>
        /// <param name="context">拦截上下文</param>
        /// <param name="index">参数索引</param>
        /// <param name="parameterInfo">参数的类型对象</param>
        internal Parameter(AspectContext context, int index, ParameterInfo parameterInfo)
        {
            _context = context;
            _index = index;
            Name = parameterInfo.Name;
            Type = parameterInfo.ParameterType;
            IsRef = Type.IsByRef;
            ParameterInfo = parameterInfo;
        }
    }
}