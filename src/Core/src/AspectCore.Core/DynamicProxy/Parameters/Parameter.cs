using AspectCore.Extensions.Reflection;

namespace AspectCore.DynamicProxy.Parameters
{
    public sealed class Parameter
    {
        private readonly object[] _parameters;
        private readonly int _index;

        public string Name { get; }

        public object Value
        {
            get
            {
                return _parameters[_index];
            }
            set
            {
                _parameters[_index] = value;
            }
        }

        public ParameterReflector Reflector { get; }

        internal Parameter(object[] parameters, int index, ParameterReflector reflector)
        {
            _parameters = parameters;
            _index = index;
            Name = reflector.Name;
            Reflector = reflector;
        }
    }
}