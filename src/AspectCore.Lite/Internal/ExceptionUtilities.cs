using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Internal
{
    internal static class ExceptionUtilities
    {
        public static void ThrowArgumentNull<T>(T instance , string paramName)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        public static void ThrowArgumentNullOrEmpty(string instance , string paramName)
        {
            if (string.IsNullOrEmpty(instance))
            {
                throw new ArgumentNullException(paramName);
            }
        }

        public static void ThrowArgument(Func<bool> predicate , string message)
        {
            if (predicate())
            {
                throw new ArgumentException(message);
            }
        }

        public static void ThrowArgument(Func<bool> predicate , string message , string paramName)
        {
            if (predicate())
            {
                throw new ArgumentException(message , paramName);
            }
        }

        public static void Throw<TException>(Func<bool> predicate , params object[] parameters) where TException : Exception
        {
            if (predicate())
            {
                throw (TException)Activator.CreateInstance(typeof(TException) , parameters);
            }
        }
    }
}
