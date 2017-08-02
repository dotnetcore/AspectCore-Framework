namespace AspectCore.Extensions.Reflection
{
    internal struct AttributeToken
    {
        public static readonly AttributeToken Empty = new AttributeToken();

        private readonly int _token;

        public int Token => _token;

        internal AttributeToken(int token) => _token = token;

        public override int GetHashCode() => _token;

        public override bool Equals(object obj)
        {
            if (obj is AttributeToken other)
            {
                return other._token == _token;
            }
            return false;
        }

        public static bool operator ==(AttributeToken a, AttributeToken b) => a._token == b._token;

        public static bool operator !=(AttributeToken a, AttributeToken b)=> !(a == b);
    }
}