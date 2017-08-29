namespace AspectCore.Abstractions
{
    public sealed class Parameter
    {
        public string Name { get; }

        public object Value { get; set; }

        public Parameter(object value, string name)
        {
            Value = value;
            Name = name;
        }
    }
}