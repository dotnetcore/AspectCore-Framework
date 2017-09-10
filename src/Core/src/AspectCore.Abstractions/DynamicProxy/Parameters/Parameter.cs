using System.Reflection;

namespace AspectCore.DynamicProxy.Parameters
{
    public class Parameter
    {
        protected readonly AspectContext _context;
        protected readonly int _index;

        public string Name { get; }

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

        public ParameterInfo ParameterInfo { get; }

        internal Parameter(AspectContext context, int index, ParameterInfo parameterInfo)
        {
            _context = context;
            _index = index;
            Name = parameterInfo.Name;
            ParameterInfo = parameterInfo;
        }
    }
}