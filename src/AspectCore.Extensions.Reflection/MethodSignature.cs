using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace AspectCore.Extensions.Reflection
{
    /// <summary>
    /// 定义一个结构以表示方法签名(方法名称+参数)
    /// </summary>
    public struct MethodSignature
    {
        private static readonly ConcurrentDictionary<Pair<MethodBase,string>, int> signatures = new ConcurrentDictionary<Pair<MethodBase, string>, int>();

        private readonly int _signature;

        /// <summary>
        /// 签名值
        /// </summary>
        public int Value => _signature;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 定义一个结构以表示方法签名
        /// </summary>
        /// <param name="method">方法</param>
        public MethodSignature(MethodBase method)
            : this(method, method?.Name)
        {
        }

        /// <summary>
        /// 定义一个结构以表示方法签名
        /// </summary>
        /// <param name="method">方法</param>
        /// <param name="name">名称</param>
        public MethodSignature(MethodBase method, string name)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            Name = name;
            _signature = signatures.GetOrAdd(Pair.Create(method, name), GetSignatureCode);
        }

        public override bool Equals(object obj)
        {
            if (obj is MethodSignature signature)
            {
                return _signature == signature._signature;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _signature;
        }

        public static bool operator !=(MethodSignature signature, MethodSignature other)
        {
            return signature._signature != other._signature;
        }

        public static bool operator ==(MethodSignature signature, MethodSignature other)
        {
            return signature._signature == other._signature;
        }

        private static int GetSignatureCode(Pair<MethodBase, string> pair)
        {
            var method = pair.Item1;
            var name = pair.Item2 ?? method.Name;
            unchecked
            {
                var signatureCode = name.GetHashCode();
                var parameterTypes = method.GetParameterTypes();
                if (parameterTypes.Length > 0)
                {
                    signatureCode = (signatureCode * 397) ^ parameterTypes.Length.GetHashCode();
                    foreach (var paramterType in parameterTypes)
                    {
                        if (paramterType.IsGenericParameter)
                        {
                            continue;
                        }
                        else if (paramterType.GetTypeInfo().IsGenericType)
                        {
                            signatureCode = GetSignatureCode(signatureCode, paramterType);
                        }
                        else
                        {
                            signatureCode = (signatureCode * 397) ^ paramterType.GetHashCode();
                        }
                    }
                }
                if (method.IsGenericMethod)
                {
                    signatureCode = (signatureCode * 397) ^ method.GetGenericArguments().Length.GetHashCode();
                }
                return signatureCode;
            }
        }

        private static int GetSignatureCode(int signatureCode, Type genericType)
        {
            signatureCode = (signatureCode * 397) ^ genericType.GetGenericTypeDefinition().GetHashCode();
            signatureCode = (signatureCode * 397) ^ genericType.GenericTypeArguments.Length.GetHashCode();
            foreach (var argument in genericType.GenericTypeArguments)
            {
                if (argument.IsGenericParameter)
                {
                    continue;
                }
                else if (argument.GetTypeInfo().IsGenericType)
                {
                    signatureCode = GetSignatureCode(signatureCode, argument);
                }
                else
                {
                    signatureCode = (signatureCode * 397) ^ argument.GetHashCode();
                }
            }
            return signatureCode;
        }
    }
}