using System;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Extensions.Reflection.Internals;

namespace AspectCore.Extensions.Reflection.Emit
{
    public static class ILGeneratorExtensions
    {
        /// <summary>
        /// 推送对应索引处的参数到计算堆栈上
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="index">参数索引</param>
        public static void EmitLoadArg(this ILGenerator ilGenerator, int index)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }

            switch (index)
            {
                case 0:
                    ilGenerator.Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    ilGenerator.Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    ilGenerator.Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    ilGenerator.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    if (index <= byte.MaxValue) ilGenerator.Emit(OpCodes.Ldarg_S, (byte)index);
                    else ilGenerator.Emit(OpCodes.Ldarg, index);
                    break;
            }
        }

        /// <summary>
        /// 推送对应索引处的参数地址到计算堆栈上
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="index">参数索引</param>
        public static void EmitLoadArgA(this ILGenerator ilGenerator, int index)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }

            //OpCodes.Ldarga_S:以短格式将自变量地址加载到计算堆栈上
            if (index <= byte.MaxValue) ilGenerator.Emit(OpCodes.Ldarga_S, (byte)index);
            //OpCodes.Ldarga:将参数地址加载到计算堆栈上
            else ilGenerator.Emit(OpCodes.Ldarga, index);
        }

        /// <summary>
        /// 将源类型转化为Object
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="typeFrom">源类型</param>
        public static void EmitConvertToObject(this ILGenerator ilGenerator, Type typeFrom)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }
            if (typeFrom == null)
            {
                throw new ArgumentNullException(nameof(typeFrom));
            }

            if (typeFrom.GetTypeInfo().IsGenericParameter)
            {
                //装箱typeFrom
                ilGenerator.Emit(OpCodes.Box, typeFrom);
            }
            else
            {
                ilGenerator.EmitConvertToType(typeFrom, typeof(object), true);
            }
        }

        /// <summary>
        /// 从object转化为目标类型typeTo
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="typeTo">目标类型</param>
        public static void EmitConvertFromObject(this ILGenerator ilGenerator, Type typeTo)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }
            if (typeTo == null)
            {
                throw new ArgumentNullException(nameof(typeTo));
            }

            if (typeTo.GetTypeInfo().IsGenericParameter)
            {
                //OpCodes.Unbox_Any:将指令中指定类型的已装箱的表示形式转换成未装箱形式
                ilGenerator.Emit(OpCodes.Unbox_Any, typeTo);
            }
            else
            {
                ilGenerator.EmitConvertToType(typeof(object), typeTo, true);
            }
        }

        /// <summary>
        /// 推送0号索引处的参数到计算堆栈上
        /// </summary>
        /// <remarks>
        /// 实例方法第一个参数隐式传递为指向实例的this指针
        /// </remarks>
        /// <param name="ilGenerator"></param>
        public static void EmitThis(this ILGenerator ilGenerator)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }

            ilGenerator.EmitLoadArg(0);
        }

        /// <summary>
        /// 发出对类型type的获取
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="type">类型</param>
        public static void EmitType(this ILGenerator ilGenerator, Type type)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            ilGenerator.Emit(OpCodes.Ldtoken, type);
            ilGenerator.Emit(OpCodes.Call, MethodInfoConstant.GetTypeFromHandle);
        }

        /// <summary>
        /// 发出对方法method的调用
        /// </summary>
        /// <param name="ilGenerator">IL生成器</param>
        /// <param name="method">方法</param>
        public static void EmitMethod(this ILGenerator ilGenerator, MethodInfo method)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            EmitMethod(ilGenerator, method, method.DeclaringType);
        }

        /// <summary>
        /// 发出对类型declaringType中method方法的调用
        /// </summary>
        /// <param name="ilGenerator">生成 Microsoft 中间语言 (MSIL) 指令</param>
        /// <param name="method">方法</param>
        /// <param name="declaringType">类型</param>
        public static void EmitMethod(this ILGenerator ilGenerator, MethodInfo method, Type declaringType)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            if (declaringType == null)
            {
                throw new ArgumentNullException(nameof(declaringType));
            }

            //将类型和方法的运行时表示形式推送到计算堆栈上
            //OpCodes.Ldtoken:将元数据标记转换为其运行时表示形式，并将其推送到计算堆栈上
            ilGenerator.Emit(OpCodes.Ldtoken, method);
            ilGenerator.Emit(OpCodes.Ldtoken, method.DeclaringType);
            ilGenerator.Emit(OpCodes.Call, MethodInfoConstant.GetMethodFromHandle);
            ilGenerator.EmitConvertToType(typeof(MethodBase), typeof(MethodInfo));
        }

        /// <summary>
        /// 发出类型转化
        /// </summary>
        /// <param name="ilGenerator">IL生成器</param>
        /// <param name="typeFrom">源类型</param>
        /// <param name="typeTo">目标类型</param>
        /// <param name="isChecked">溢出检查</param>
        public static void EmitConvertToType(this ILGenerator ilGenerator, Type typeFrom, Type typeTo, bool isChecked = true)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }
            if (typeFrom == null)
            {
                throw new ArgumentNullException(nameof(typeFrom));
            }
            if (typeTo == null)
            {
                throw new ArgumentNullException(nameof(typeTo));
            }

            var typeFromInfo = typeFrom.GetTypeInfo();
            var typeToInfo = typeTo.GetTypeInfo();

            var nnExprType = typeFromInfo.GetNonNullableType();
            var nnType = typeToInfo.GetNonNullableType();

            if (TypeInfoUtils.AreEquivalent(typeFromInfo, typeToInfo))
            {
                return;
            }

            if (typeFromInfo.IsInterface || // interface cast
              typeToInfo.IsInterface ||
               typeFrom == typeof(object) || // boxing cast
               typeTo == typeof(object) ||
               typeFrom == typeof(System.Enum) ||
               typeFrom == typeof(System.ValueType) ||
               TypeInfoUtils.IsLegalExplicitVariantDelegateConversion(typeFromInfo, typeToInfo))
            {
                ilGenerator.EmitCastToType(typeFromInfo, typeToInfo);
            }
            else if (typeFromInfo.IsNullableType() || typeToInfo.IsNullableType())
            {
                ilGenerator.EmitNullableConversion(typeFromInfo, typeToInfo, isChecked);
            }
            else if (!(typeFromInfo.IsConvertible() && typeToInfo.IsConvertible()) // primitive runtime conversion
                     &&
                     (nnExprType.GetTypeInfo().IsAssignableFrom(nnType) || // down cast
                     nnType.GetTypeInfo().IsAssignableFrom(nnExprType))) // up cast
            {
                ilGenerator.EmitCastToType(typeFromInfo, typeToInfo);
            }
            else if (typeFromInfo.IsArray && typeToInfo.IsArray)
            {
                // See DevDiv Bugs #94657.
                ilGenerator.EmitCastToType(typeFromInfo, typeToInfo);
            }
            else
            {
                ilGenerator.EmitNumericConversion(typeFromInfo, typeToInfo, isChecked);
            }
        }

        /// <summary>
        /// 发出类型转化
        /// </summary>
        /// <param name="ilGenerator">IL生成器</param>
        /// <param name="typeFrom">源类型</param>
        /// <param name="typeTo">目标类型</param>
        public static void EmitCastToType(this ILGenerator ilGenerator, TypeInfo typeFrom, TypeInfo typeTo)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }
            //引用类型到值类型
            if (!typeFrom.IsValueType && typeTo.IsValueType)
            {
                //拆箱
                ilGenerator.Emit(OpCodes.Unbox_Any, typeTo.AsType());
            }
            //值类型到引用类型
            else if (typeFrom.IsValueType && !typeTo.IsValueType)
            {
                //装箱
                ilGenerator.Emit(OpCodes.Box, typeFrom.AsType());
                if (typeTo.AsType() != typeof(object))
                {
                    //OpCodes.Castclass: 尝试将引用传递的对象转换为指定的类
                    ilGenerator.Emit(OpCodes.Castclass, typeTo.AsType());
                }
            }
            //引用到引用
            else if (!typeFrom.IsValueType && !typeTo.IsValueType)
            {
                ilGenerator.Emit(OpCodes.Castclass, typeTo.AsType());
            }
            else
            {
                throw new InvalidCastException($"Caanot cast {typeFrom} to {typeTo}.");
            }
        }

        /// <summary>
        /// 发出对可空类型的HasValue属性的get访问器的调用
        /// </summary>
        /// <param name="ilGenerator">IL生成器</param>
        /// <param name="nullableType">可空类型</param>
        public static void EmitHasValue(this ILGenerator ilGenerator, Type nullableType)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }
            MethodInfo mi = nullableType.GetTypeInfo().GetMethod("get_HasValue", BindingFlags.Instance | BindingFlags.Public);
            ilGenerator.Emit(OpCodes.Call, mi);
        }

        /// <summary>
        /// 发出对可空类型的GetValueOrDefault方法的调用
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="nullableType">可空类型</param>
        public static void EmitGetValueOrDefault(this ILGenerator ilGenerator, Type nullableType)
        {
            MethodInfo mi = nullableType.GetTypeInfo().GetMethod("GetValueOrDefault", Type.EmptyTypes);
            ilGenerator.Emit(OpCodes.Call, mi);
        }

        /// <summary>
        /// 发出对可空类型Value属性的get访问器的调用
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="nullableType">可空类型</param>
        public static void EmitGetValue(this ILGenerator ilGenerator, Type nullableType)
        {
            MethodInfo mi = nullableType.GetTypeInfo().GetMethod("get_Value", BindingFlags.Instance | BindingFlags.Public);
            ilGenerator.Emit(OpCodes.Call, mi);
        }

        public static void EmitConstant(this ILGenerator ilGenerator, object value, Type valueType)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }
            if (valueType == null)
            {
                throw new ArgumentNullException(nameof(valueType));
            }
            if (value == null)
            {
                EmitDefault(ilGenerator, valueType);
                return;
            }

            if (ilGenerator.TryEmitILConstant(value, valueType))
            {
                return;
            }

            var t = value as Type;
            if (t != null)
            {
                ilGenerator.EmitType(t);
                if (valueType != typeof(Type))
                {
                    //OpCodes.Castclass: 尝试将引用传递的对象转换为指定的类
                    ilGenerator.Emit(OpCodes.Castclass, valueType);
                }
                return;
            }

            var mb = value as MethodBase;
            if (mb != null)
            {
                ilGenerator.EmitMethod((MethodInfo)mb);
                return;
            }

            if (valueType.GetTypeInfo().IsArray)
            {
                var array = (Array)value;
                ilGenerator.EmitArray(array, valueType.GetElementType());
            }

            throw new InvalidOperationException("Code supposed to be unreachable.");
        }

        /// <summary>
        /// 发出类型的默认值
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="type">类型</param>
        public static void EmitDefault(this ILGenerator ilGenerator, Type type)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Object:
                case TypeCode.DateTime:
                    if (type.GetTypeInfo().IsValueType)
                    {
                        // Type.GetTypeCode on an enum returns the underlying
                        // integer TypeCode, so we won't get here.
                        // This is the IL for default(T) if T is a generic type
                        // parameter, so it should work for any type. It's also
                        // the standard pattern for structs.

                        //DeclareLocal: 声明指定类型的局部变量
                        LocalBuilder lb = ilGenerator.DeclareLocal(type);
                        //OpCodes.Ldloca: 将位于特定索引处的局部变量的地址加载到计算堆栈上
                        ilGenerator.Emit(OpCodes.Ldloca, lb);
                        //OpCodes.Initobj: 将位于指定地址的值类型的每个字段初始化为空引用或适当的基元类型的 0
                        ilGenerator.Emit(OpCodes.Initobj, type);
                        //OpCodes.Ldloc: 将指定索引处的局部变量加载到计算堆栈上
                        ilGenerator.Emit(OpCodes.Ldloc, lb);
                    }
                    else
                    {
                        //OpCodes.Ldnull: 将空引用（O 类型）推送到计算堆栈上
                        ilGenerator.Emit(OpCodes.Ldnull);
                    }
                    break;

                case TypeCode.Empty:
                case TypeCode.String:
                    ilGenerator.Emit(OpCodes.Ldnull);
                    break;

                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    ilGenerator.Emit(OpCodes.Ldc_I4_0);
                    break;

                case TypeCode.Int64:
                case TypeCode.UInt64:
                    ilGenerator.Emit(OpCodes.Ldc_I4_0);
                    ilGenerator.Emit(OpCodes.Conv_I8);
                    break;

                case TypeCode.Single:
                    ilGenerator.Emit(OpCodes.Ldc_R4, default(Single));
                    break;

                case TypeCode.Double:
                    ilGenerator.Emit(OpCodes.Ldc_R8, default(Double));
                    break;

                case TypeCode.Decimal:
                    ilGenerator.Emit(OpCodes.Ldc_I4_0);
                    //OpCodes.Newobj: 创建一个值类型的新对象或新实例，并将对象引用（O 类型）推送到计算堆栈上
                    ilGenerator.Emit(OpCodes.Newobj, typeof(Decimal).GetTypeInfo().GetConstructor(new Type[] { typeof(int) }));
                    break;

                default:
                    throw new InvalidOperationException("Code supposed to be unreachable.");
            }
        }

        /// <summary>
        /// 是否能将元数据标记转换为其运行时表示形式，并将其推送到计算堆栈上
        /// </summary>
        /// <param name="value">类型</param>
        /// <param name="type">值</param>
        /// <returns>判断结果</returns>
        public static bool CanEmitConstant(object value, Type type)
        {
            if (value == null || CanEmitILConstant(type))
            {
                return true;
            }

            Type t = value as Type;
            if (t != null && ShouldLdtoken(t))
            {
                return true;
            }

            MethodBase mb = value as MethodInfo;
            if (mb != null && ShouldLdtoken(mb))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 推送decimal类型的值到计算堆栈上
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="value">值</param>
        public static void EmitDecimal(this ILGenerator ilGenerator, decimal value)
        {
            if (Decimal.Truncate(value) == value)
            {
                if (Int32.MinValue <= value && value <= Int32.MaxValue)
                {
                    int intValue = Decimal.ToInt32(value);
                    ilGenerator.EmitInt(intValue);
                    ilGenerator.EmitNew(typeof(Decimal).GetTypeInfo().GetConstructor(new Type[] { typeof(int) }));
                }
                else if (Int64.MinValue <= value && value <= Int64.MaxValue)
                {
                    long longValue = Decimal.ToInt64(value);
                    ilGenerator.EmitLong(longValue);
                    ilGenerator.EmitNew(typeof(Decimal).GetTypeInfo().GetConstructor(new Type[] { typeof(long) }));
                }
                else
                {
                    ilGenerator.EmitDecimalBits(value);
                }
            }
            else
            {
                ilGenerator.EmitDecimalBits(value);
            }
        }

        /// <summary>
        /// 分配未初始化的对象或值类型，并调用构造函数方法
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="ci">构造函数</param>
        public static void EmitNew(this ILGenerator ilGenerator, ConstructorInfo ci)
        {
            ilGenerator.Emit(OpCodes.Newobj, ci);
        }

        /// <summary>
        /// 将空引用（O 类型）推送到计算堆栈上
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        public static void EmitNull(this ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Ldnull);
        }

        /// <summary>
        /// 推送对元数据中存储的字符串的新对象引用(即value)
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="value">字符串</param>
        public static void EmitString(this ILGenerator ilGenerator, string value)
        {
            ilGenerator.Emit(OpCodes.Ldstr, value);
        }

        /// <summary>
        /// 将整数值 0（当value为false） 或 1（当value为true） 作为 int32 推送到计算堆栈上
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="value">布尔值</param>
        public static void EmitBoolean(this ILGenerator ilGenerator, bool value)
        {
            if (value)
            {
                ilGenerator.Emit(OpCodes.Ldc_I4_1);
            }
            else
            {
                ilGenerator.Emit(OpCodes.Ldc_I4_0);
            }
        }

        /// <summary>
        /// 推送char类型的值到计算堆栈上
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="value">值</param>
        public static void EmitChar(this ILGenerator ilGenerator, char value)
        {
            ilGenerator.EmitInt(value);
            //OpCodes.Conv_U2: 将位于计算堆栈顶部的值转换为 unsigned int16，然后将其扩展为 int32
            ilGenerator.Emit(OpCodes.Conv_U2);
        }

        /// <summary>
        /// 推送byte类型的值到计算堆栈上
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="value">值</param>
        public static void EmitByte(this ILGenerator ilGenerator, byte value)
        {
            ilGenerator.EmitInt(value);
            //OpCodes.Conv_U1: 将位于计算堆栈顶部的值转换为 unsigned int8，然后将其扩展为 int32
            ilGenerator.Emit(OpCodes.Conv_U1);
        }

        /// <summary>
        /// 推送sbyte类型的值到计算堆栈上
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="value">值</param>
        public static void EmitSByte(this ILGenerator ilGenerator, sbyte value)
        {
            ilGenerator.EmitInt(value);
            //OpCodes.Conv_I1: 将位于计算堆栈顶部的值转换为 int8，然后将其扩展（填充）为 int32
            ilGenerator.Emit(OpCodes.Conv_I1);
        }

        /// <summary>
        /// 推送short类型的值到计算堆栈上
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="value">值</param>
        public static void EmitShort(this ILGenerator ilGenerator, short value)
        {
            ilGenerator.EmitInt(value);
            //OpCodes.Conv_I2: 将位于计算堆栈顶部的值转换为 int16，然后将其扩展（填充）为 int32
            ilGenerator.Emit(OpCodes.Conv_I2);
        }

        /// <summary>
        /// 推送ushort类型的值到计算堆栈上
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="value">值</param>
        public static void EmitUShort(this ILGenerator ilGenerator, ushort value)
        {
            ilGenerator.EmitInt(value);
            //OpCodes.Conv_U2: 将位于计算堆栈顶部的值转换为 unsigned int16，然后将其扩展为 int32
            ilGenerator.Emit(OpCodes.Conv_U2);
        }

        /// <summary>
        /// 推送int类型的值到计算堆栈上
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="value">值</param>
        public static void EmitInt(this ILGenerator ilGenerator, int value)
        {
            OpCode c;
            switch (value)
            {
                case -1:
                    //OpCodes.Ldc_I4_M1: 将整数值 -1 作为 int32 推送到计算堆栈上
                    c = OpCodes.Ldc_I4_M1;
                    break;
                case 0:
                    c = OpCodes.Ldc_I4_0;
                    break;
                case 1:
                    c = OpCodes.Ldc_I4_1;
                    break;
                case 2:
                    c = OpCodes.Ldc_I4_2;
                    break;
                case 3:
                    c = OpCodes.Ldc_I4_3;
                    break;
                case 4:
                    c = OpCodes.Ldc_I4_4;
                    break;
                case 5:
                    c = OpCodes.Ldc_I4_5;
                    break;
                case 6:
                    c = OpCodes.Ldc_I4_6;
                    break;
                case 7:
                    c = OpCodes.Ldc_I4_7;
                    break;
                case 8:
                    c = OpCodes.Ldc_I4_8;
                    break;
                default:
                    if (value >= -128 && value <= 127)
                    {
                        //OpCodes.Ldc_I4_S: 将提供的 int8 值作为 int32 推送到计算堆栈上（短格式）
                        ilGenerator.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
                    }
                    else
                    {
                        //OpCodes.Ldc_I4: 将所提供的 int32 类型的值作为 int32 推送到计算堆栈上
                        ilGenerator.Emit(OpCodes.Ldc_I4, value);
                    }
                    return;
            }
            ilGenerator.Emit(c);
        }

        /// <summary>
        /// 推送uint类型的值到计算堆栈上
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="value">值</param>
        public static void EmitUInt(this ILGenerator ilGenerator, uint value)
        {
            ilGenerator.EmitInt((int)value);
            //OpCodes.Conv_U4: 将位于计算堆栈顶部的值转换为 unsigned int32，然后将其扩展为 int32
            ilGenerator.Emit(OpCodes.Conv_U4);
        }

        /// <summary>
        /// 推送long类型的值到计算堆栈上
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="value">值</param>
        public static void EmitLong(this ILGenerator ilGenerator, long value)
        {
            //OpCodes.Ldc_I8: 将所提供的 int64 类型的值作为 int64 推送到计算堆栈上
            ilGenerator.Emit(OpCodes.Ldc_I8, value);

            //
            // Now, emit convert to give the constant type information.
            //
            // Otherwise, it is treated as unsigned and overflow is not
            // detected if it's used in checked ops.
            //
            //OpCodes.Conv_I8: 将位于计算堆栈顶部的值转换为 int64
            ilGenerator.Emit(OpCodes.Conv_I8);
        }

        /// <summary>
        /// 推送ulong类型的值到计算堆栈上
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="value">值</param>
        public static void EmitULong(this ILGenerator ilGenerator, ulong value)
        {
            //OpCodes.Ldc_I8: 将所提供的 int64 类型的值作为 int64 推送到计算堆栈上
            ilGenerator.Emit(OpCodes.Ldc_I8, (long)value);
            //OpCodes.Conv_U8: 将位于计算堆栈顶部的值转换为 unsigned int64，然后将其扩展为 int64
            ilGenerator.Emit(OpCodes.Conv_U8);
        }

        /// <summary>
        /// 推送double类型的值到计算堆栈上
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="value">值</param>
        public static void EmitDouble(this ILGenerator ilGenerator, double value)
        {
            //OpCodes.Ldc_R8: 将所提供的 float64 类型的值作为 F(float) 类型推送到计算堆栈上
            ilGenerator.Emit(OpCodes.Ldc_R8, value);
        }

        /// <summary>
        /// 推送float类型的值到计算堆栈上
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="value">值</param>
        public static void EmitSingle(this ILGenerator ilGenerator, float value)
        {
            //OpCodes.Ldc_R4: 将所提供的 float32 类型的值作为 F (float) 类型推送到计算堆栈上
            ilGenerator.Emit(OpCodes.Ldc_R4, value);
        }

        /// <summary>
        /// 发出对构建数组的调用
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="items">数组</param>
        /// <param name="elementType">元素类型</param>
        public static void EmitArray(this ILGenerator ilGenerator, Array items, Type elementType)
        {
            ilGenerator.EmitInt(items.Length);
            //OpCodes.Newarr: 将对新的从零开始的一维数组（其元素属于特定类型）的对象引用推送到计算堆栈上
            ilGenerator.Emit(OpCodes.Newarr, elementType);
            for (int i = 0; i < items.Length; i++)
            {
                //OpCodes.Dup: 复制计算堆栈上当前最顶端的值，然后将副本推送到计算堆栈上
                ilGenerator.Emit(OpCodes.Dup);
                ilGenerator.EmitInt(i);
                var constantType = items.GetValue(i).GetType();
                if (constantType == elementType)
                {
                    ilGenerator.EmitConstant(items.GetValue(i), elementType);
                }
                else
                {
                    ilGenerator.EmitConstant(items.GetValue(i), constantType);
                    ilGenerator.EmitConvertToObject(constantType);
                }
                ilGenerator.EmitStoreElement(elementType);
            }
        }

        /// <summary>
        /// 用计算堆栈中的值替换给定索引处的数组元素
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="type">元素类型</param>
        public static void EmitStoreElement(this ILGenerator ilGenerator, Type type)
        {
            if (type.GetTypeInfo().IsEnum)
            {
                //OpCodes.Stelem: 用计算堆栈中的值替换给定索引处的数组元素，其类型在指令中指定
                ilGenerator.Emit(OpCodes.Stelem, type);
                return;
            }
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.SByte:
                case TypeCode.Byte:
                    //OpCodes.Stelem_I1: 用计算堆栈上的 int8 值替换给定索引处的数组元素
                    ilGenerator.Emit(OpCodes.Stelem_I1);
                    break;
                case TypeCode.Char:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                    //OpCodes.Stelem_I2: 用计算堆栈上的 int16 值替换给定索引处的数组元素
                    ilGenerator.Emit(OpCodes.Stelem_I2);
                    break;
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    //OpCodes.Stelem_I4: 用计算堆栈上的 int32 值替换给定索引处的数组元素
                    ilGenerator.Emit(OpCodes.Stelem_I4);
                    break;
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    //OpCodes.Stelem_I8: 用计算堆栈上的 int64 值替换给定索引处的数组元素
                    ilGenerator.Emit(OpCodes.Stelem_I8);
                    break;
                case TypeCode.Single:
                    //OpCodes.Stelem_R4: 用计算堆栈上的 float32 值替换给定索引处的数组元素 
                    ilGenerator.Emit(OpCodes.Stelem_R4);
                    break;
                case TypeCode.Double:
                    //OpCodes.Stelem_R8: 用计算堆栈上的 float64 值替换给定索引处的数组元素
                    ilGenerator.Emit(OpCodes.Stelem_R8);
                    break;
                default:
                    if (type.GetTypeInfo().IsValueType)
                    {
                        //OpCodes.Stelem: 用计算堆栈中的值替换给定索引处的数组元素，其类型在指令中指定
                        ilGenerator.Emit(OpCodes.Stelem, type);
                    }
                    else
                    {
                        //OpCodes.Stelem_Ref: 用计算堆栈上的对象 ref 值（O 类型）替换给定索引处的数组元素
                        ilGenerator.Emit(OpCodes.Stelem_Ref);
                    }
                    break;
            }
        }

        /// <summary>
        /// 将位于指定数组索引处的值或引用加载到计算堆栈
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="type">元素类型</param>
        public static void EmitLoadElement(this ILGenerator ilGenerator, Type type)
        {
            if (!type.GetTypeInfo().IsValueType)
            {
                //OpCodes.Ldelem_Ref: 将位于指定数组索引处的包含对象引用的元素作为 O 类型（对象引用）加载到计算堆栈的顶部
                ilGenerator.Emit(OpCodes.Ldelem_Ref);
            }
            else if (type.GetTypeInfo().IsEnum)
            {
                ilGenerator.Emit(OpCodes.Ldelem, type);
            }
            else
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                    case TypeCode.SByte:
                        ilGenerator.Emit(OpCodes.Ldelem_I1);
                        break;
                    case TypeCode.Byte:
                        //OpCodes.Ldelem_U1: 将位于指定数组索引处的 unsigned int8 类型的元素作为 int32 加载到计算堆栈的顶部
                        ilGenerator.Emit(OpCodes.Ldelem_U1);
                        break;
                    case TypeCode.Int16:
                        ilGenerator.Emit(OpCodes.Ldelem_I2);
                        break;
                    case TypeCode.Char:
                    case TypeCode.UInt16:
                        ilGenerator.Emit(OpCodes.Ldelem_U2);
                        break;
                    case TypeCode.Int32:
                        ilGenerator.Emit(OpCodes.Ldelem_I4);
                        break;
                    case TypeCode.UInt32:
                        ilGenerator.Emit(OpCodes.Ldelem_U4);
                        break;
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        ilGenerator.Emit(OpCodes.Ldelem_I8);
                        break;
                    case TypeCode.Single:
                        ilGenerator.Emit(OpCodes.Ldelem_R4);
                        break;
                    case TypeCode.Double:
                        ilGenerator.Emit(OpCodes.Ldelem_R8);
                        break;
                    default:
                        ilGenerator.Emit(OpCodes.Ldelem, type);
                        break;
                }
            }
        }

        /// <summary>
        /// 加载所提供地址处的值或引用
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="type">类型</param>
        public static void EmitLdRef(this ILGenerator ilGenerator, Type type)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            if (type == typeof(short))
            {
                ilGenerator.Emit(OpCodes.Ldind_I1);
            }
            else if (type == typeof(Int16))
            {
                ilGenerator.Emit(OpCodes.Ldind_I2);
            }
            else if (type == typeof(Int32))
            {
                ilGenerator.Emit(OpCodes.Ldind_I4);
            }
            else if (type == typeof(Int64))
            {
                ilGenerator.Emit(OpCodes.Ldind_I8);
            }
            else if (type == typeof(float))
            {
                ilGenerator.Emit(OpCodes.Ldind_R4);
            }
            else if (type == typeof(double))
            {
                ilGenerator.Emit(OpCodes.Ldind_R8);
            }
            else if (type == typeof(ushort))
            {
                ilGenerator.Emit(OpCodes.Ldind_U1);
            }
            else if (type == typeof(UInt16))
            {
                ilGenerator.Emit(OpCodes.Ldind_U2);
            }
            else if (type == typeof(UInt32))
            {
                //OpCodes.Ldind_U4: 将 unsigned int32 类型的值作为 int32 间接加载到计算堆栈上
                ilGenerator.Emit(OpCodes.Ldind_U4);
            }
            else if (type.GetTypeInfo().IsValueType)
            {
                //OpCodes.Ldobj: 将地址指向的值类型对象复制到计算堆栈的顶部
                ilGenerator.Emit(OpCodes.Ldobj);
            }
            else
            {
                //OpCodes.Ldind_Ref: 将对象引用作为 O（对象引用）类型间接加载到计算堆栈上
                ilGenerator.Emit(OpCodes.Ldind_Ref);
            }
        }

        /// <summary>
        /// 存储值或对象引用
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="type">类型</param>
        public static void EmitStRef(this ILGenerator ilGenerator, Type type)
        {
            if (ilGenerator == null)
            {
                throw new ArgumentNullException(nameof(ilGenerator));
            }
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            if (type == typeof(short))
            {
                //OpCodes.Stind_I1: 在所提供的地址存储 int8 类型的值
                ilGenerator.Emit(OpCodes.Stind_I1);
            }
            else if (type == typeof(Int16))
            {
                //OpCodes.Stind_I2: 在所提供的地址存储 int16 类型的值
                ilGenerator.Emit(OpCodes.Stind_I2);
            }
            else if (type == typeof(Int32))
            {
                ilGenerator.Emit(OpCodes.Stind_I4);
            }
            else if (type == typeof(Int64))
            {
                ilGenerator.Emit(OpCodes.Stind_I8);
            }
            else if (type == typeof(float))
            {
                //OpCodes.Stind_R4: 在所提供的地址存储 float32 类型的值
                ilGenerator.Emit(OpCodes.Stind_R4);
            }
            else if (type == typeof(double))
            {
                //OpCodes.Stind_R8: 在所提供的地址存储 float64 类型的值
                ilGenerator.Emit(OpCodes.Stind_R8);
            }
            else if (type.GetTypeInfo().IsValueType)
            {
                //OpCodes.Stobj: 将指定类型的值从计算堆栈复制到所提供的内存地址中
                ilGenerator.Emit(OpCodes.Stobj);
            }
            else
            {
                //OpCodes.Stind_Ref: 存储所提供地址处的对象引用值
                ilGenerator.Emit(OpCodes.Stind_Ref);
            }
        }

        #region private

        /// <summary>
        /// 发出可能为可空类型直接的转化
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="typeFrom">源类型</param>
        /// <param name="typeTo">目标类型</param>
        /// <param name="isChecked">溢出检查</param>
        private static void EmitNullableConversion(this ILGenerator ilGenerator, TypeInfo typeFrom, TypeInfo typeTo, bool isChecked)
        {
            bool isTypeFromNullable = TypeInfoUtils.IsNullableType(typeFrom);
            bool isTypeToNullable = TypeInfoUtils.IsNullableType(typeTo);
            if (isTypeFromNullable && isTypeToNullable)
                ilGenerator.EmitNullableToNullableConversion(typeFrom, typeTo, isChecked);
            else if (isTypeFromNullable)
                ilGenerator.EmitNullableToNonNullableConversion(typeFrom, typeTo, isChecked);
            else
                ilGenerator.EmitNonNullableToNullableConversion(typeFrom, typeTo, isChecked);
        }

        /// <summary>
        /// 发出可空类型之间的转化
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="typeFrom">源类型</param>
        /// <param name="typeTo">目标类型</param>
        /// <param name="isChecked">溢出检查</param>
        private static void EmitNullableToNullableConversion(this ILGenerator ilGenerator, TypeInfo typeFrom, TypeInfo typeTo, bool isChecked)
        {
            Label labIfNull = default(Label);
            Label labEnd = default(Label);
            LocalBuilder locFrom = null;
            LocalBuilder locTo = null;
            locFrom = ilGenerator.DeclareLocal(typeFrom.AsType());
            ilGenerator.Emit(OpCodes.Stloc, locFrom);
            locTo = ilGenerator.DeclareLocal(typeTo.AsType());
            // test for null
            ilGenerator.Emit(OpCodes.Ldloca, locFrom);
            ilGenerator.EmitHasValue(typeFrom.AsType());
            labIfNull = ilGenerator.DefineLabel();
            //OpCodes.Brfalse_S: 短格式，如果 value 为 false、空引用或零，则将控制转移到目标指令
            ilGenerator.Emit(OpCodes.Brfalse_S, labIfNull);
            ilGenerator.Emit(OpCodes.Ldloca, locFrom);
            ilGenerator.EmitGetValueOrDefault(typeFrom.AsType());
            Type nnTypeFrom = TypeInfoUtils.GetNonNullableType(typeFrom);
            Type nnTypeTo = TypeInfoUtils.GetNonNullableType(typeTo);
            ilGenerator.EmitConvertToType(nnTypeFrom, nnTypeTo, isChecked);
            // construct result type
            ConstructorInfo ci = typeTo.GetConstructor(new Type[] { nnTypeTo });
            ilGenerator.Emit(OpCodes.Newobj, ci);
            ilGenerator.Emit(OpCodes.Stloc, locTo);
            labEnd = ilGenerator.DefineLabel();
            //OpCodes.Br_S: 无条件地将控制转移到目标指令（短格式）
            ilGenerator.Emit(OpCodes.Br_S, labEnd);
            // if null then create a default one
            ilGenerator.MarkLabel(labIfNull);
            ilGenerator.Emit(OpCodes.Ldloca, locTo);
            ilGenerator.Emit(OpCodes.Initobj, typeTo.AsType());
            ilGenerator.MarkLabel(labEnd);
            ilGenerator.Emit(OpCodes.Ldloc, locTo);
        }

        /// <summary>
        /// 发出可空类型到非空类型的转化
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="typeFrom">源类型</param>
        /// <param name="typeTo">目标类型</param>
        /// <param name="isChecked">溢出检查</param>
        private static void EmitNullableToNonNullableConversion(this ILGenerator ilGenerator, TypeInfo typeFrom, TypeInfo typeTo, bool isChecked)
        {
            if (typeTo.IsValueType)
                ilGenerator.EmitNullableToNonNullableStructConversion(typeFrom, typeTo, isChecked);
            else
                ilGenerator.EmitNullableToReferenceConversion(typeFrom);
        }

        /// <summary>
        /// 发出可空类型到非空值类型的转化
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="typeFrom">源类型</param>
        /// <param name="typeTo">目标类型</param>
        /// <param name="isChecked">溢出检查</param>
        private static void EmitNullableToNonNullableStructConversion(this ILGenerator ilGenerator, TypeInfo typeFrom, TypeInfo typeTo, bool isChecked)
        {
            LocalBuilder locFrom = null;
            locFrom = ilGenerator.DeclareLocal(typeFrom.AsType());
            ilGenerator.Emit(OpCodes.Stloc, locFrom);
            ilGenerator.Emit(OpCodes.Ldloca, locFrom);
            ilGenerator.EmitGetValue(typeFrom.AsType());
            Type nnTypeFrom = TypeInfoUtils.GetNonNullableType(typeFrom);
            ilGenerator.EmitConvertToType(nnTypeFrom, typeTo.AsType(), isChecked);
        }

        /// <summary>
        /// 发出可空类型到引用类型的转化
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="typeFrom">源类型</param>
        private static void EmitNullableToReferenceConversion(this ILGenerator ilGenerator, TypeInfo typeFrom)
        {
            // We've got a conversion from nullable to Object, ValueType, Enum, etc.  Just box it so that
            // we get the nullable semantics.

            ilGenerator.Emit(OpCodes.Box, typeFrom.AsType());
        }

        /// <summary>
        /// 发出非空类型到可空类型的转化
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="typeFrom">源类型</param>
        /// <param name="typeTo">目标类型</param>
        /// <param name="isChecked">溢出检查</param>
        private static void EmitNonNullableToNullableConversion(this ILGenerator ilGenerator, TypeInfo typeFrom, TypeInfo typeTo, bool isChecked)
        {
            LocalBuilder locTo = null;
            locTo = ilGenerator.DeclareLocal(typeTo.AsType());
            Type nnTypeTo = TypeInfoUtils.GetNonNullableType(typeTo);
            ilGenerator.EmitConvertToType(typeFrom.AsType(), nnTypeTo, isChecked);
            ConstructorInfo ci = typeTo.GetConstructor(new Type[] { nnTypeTo });
            ilGenerator.Emit(OpCodes.Newobj, ci);
            ilGenerator.Emit(OpCodes.Stloc, locTo);
            ilGenerator.Emit(OpCodes.Ldloc, locTo);
        }

        /// <summary>
        /// 发出数值类型转换
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="typeFrom">源类型</param>
        /// <param name="typeTo">目标类型</param>
        /// <param name="isChecked">溢出检查</param>
        private static void EmitNumericConversion(this ILGenerator ilGenerator, TypeInfo typeFrom, TypeInfo typeTo, bool isChecked)
        {
            bool isFromUnsigned = TypeInfoUtils.IsUnsigned(typeFrom);
            bool isFromFloatingPoint = TypeInfoUtils.IsFloatingPoint(typeFrom);
            if (typeTo.AsType() == typeof(Single))
            {
                if (isFromUnsigned)
                    ilGenerator.Emit(OpCodes.Conv_R_Un);
                ilGenerator.Emit(OpCodes.Conv_R4);
            }
            else if (typeTo.AsType() == typeof(Double))
            {
                if (isFromUnsigned)
                    ilGenerator.Emit(OpCodes.Conv_R_Un);
                ilGenerator.Emit(OpCodes.Conv_R8);
            }
            else
            {
                TypeCode tc = Type.GetTypeCode(typeTo.AsType());
                if (isChecked)
                {
                    // Overflow checking needs to know if the source value on the IL stack is unsigned or not.
                    if (isFromUnsigned)
                    {
                        switch (tc)
                        {
                            case TypeCode.SByte:
                                ilGenerator.Emit(OpCodes.Conv_Ovf_I1_Un);
                                break;
                            case TypeCode.Int16:
                                ilGenerator.Emit(OpCodes.Conv_Ovf_I2_Un);
                                break;
                            case TypeCode.Int32:
                                ilGenerator.Emit(OpCodes.Conv_Ovf_I4_Un);
                                break;
                            case TypeCode.Int64:
                                ilGenerator.Emit(OpCodes.Conv_Ovf_I8_Un);
                                break;
                            case TypeCode.Byte:
                                ilGenerator.Emit(OpCodes.Conv_Ovf_U1_Un);
                                break;
                            case TypeCode.UInt16:
                            case TypeCode.Char:
                                ilGenerator.Emit(OpCodes.Conv_Ovf_U2_Un);
                                break;
                            case TypeCode.UInt32:
                                ilGenerator.Emit(OpCodes.Conv_Ovf_U4_Un);
                                break;
                            case TypeCode.UInt64:
                                ilGenerator.Emit(OpCodes.Conv_Ovf_U8_Un);
                                break;
                            default:
                                throw new InvalidCastException();
                        }
                    }
                    else
                    {
                        switch (tc)
                        {
                            case TypeCode.SByte:
                                ilGenerator.Emit(OpCodes.Conv_Ovf_I1);
                                break;
                            case TypeCode.Int16:
                                ilGenerator.Emit(OpCodes.Conv_Ovf_I2);
                                break;
                            case TypeCode.Int32:
                                ilGenerator.Emit(OpCodes.Conv_Ovf_I4);
                                break;
                            case TypeCode.Int64:
                                ilGenerator.Emit(OpCodes.Conv_Ovf_I8);
                                break;
                            case TypeCode.Byte:
                                ilGenerator.Emit(OpCodes.Conv_Ovf_U1);
                                break;
                            case TypeCode.UInt16:
                            case TypeCode.Char:
                                ilGenerator.Emit(OpCodes.Conv_Ovf_U2);
                                break;
                            case TypeCode.UInt32:
                                ilGenerator.Emit(OpCodes.Conv_Ovf_U4);
                                break;
                            case TypeCode.UInt64:
                                ilGenerator.Emit(OpCodes.Conv_Ovf_U8);
                                break;
                            default:
                                throw new InvalidCastException();
                        }
                    }
                }
                else
                {
                    switch (tc)
                    {
                        case TypeCode.SByte:
                            ilGenerator.Emit(OpCodes.Conv_I1);
                            break;
                        case TypeCode.Byte:
                            ilGenerator.Emit(OpCodes.Conv_U1);
                            break;
                        case TypeCode.Int16:
                            ilGenerator.Emit(OpCodes.Conv_I2);
                            break;
                        case TypeCode.UInt16:
                        case TypeCode.Char:
                            ilGenerator.Emit(OpCodes.Conv_U2);
                            break;
                        case TypeCode.Int32:
                            ilGenerator.Emit(OpCodes.Conv_I4);
                            break;
                        case TypeCode.UInt32:
                            ilGenerator.Emit(OpCodes.Conv_U4);
                            break;
                        case TypeCode.Int64:
                            if (isFromUnsigned)
                            {
                                ilGenerator.Emit(OpCodes.Conv_U8);
                            }
                            else
                            {
                                ilGenerator.Emit(OpCodes.Conv_I8);
                            }
                            break;
                        case TypeCode.UInt64:
                            if (isFromUnsigned || isFromFloatingPoint)
                            {
                                ilGenerator.Emit(OpCodes.Conv_U8);
                            }
                            else
                            {
                                ilGenerator.Emit(OpCodes.Conv_I8);
                            }
                            break;
                        default:
                            throw new InvalidCastException();
                    }
                }
            }
        }

        /// <summary>
        /// 是否将类型的元数据标记转换为其运行时表示形式，并将其推送到计算堆栈上
        /// </summary>
        /// <param name="t">类型</param>
        /// <returns>判断结果,true:需要推送到计算堆栈;false:不需要推送计算堆栈</returns>
        private static bool ShouldLdtoken(Type t)
        {
            return t.IsGenericParameter || t.GetTypeInfo().IsVisible;
        }

        /// <summary>
        /// 是否将方法声明类型的元数据标记转换为其运行时表示形式，并将其推送到计算堆栈上
        /// </summary>
        /// <param name="mb">方法</param>
        /// <returns>判断结果,true:需要推送到计算堆栈;false:不需要推送计算堆栈</returns>
        private static bool ShouldLdtoken(MethodBase mb)
        {
            // Can't ldtoken on a DynamicMethod
            if (mb is DynamicMethod)
            {
                return false;
            }

            Type dt = mb.DeclaringType;
            return dt == null || ShouldLdtoken(dt);
        }

        /// <summary>
        /// 推送值(基本类型的值)到计算堆栈上
        /// </summary>
        /// <param name="ilGenerator">ILGenerator</param>
        /// <param name="value">值</param>
        /// <param name="type">类型</param>
        /// <returns>是否推送成功</returns>
        private static bool TryEmitILConstant(this ILGenerator ilGenerator, object value, Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    ilGenerator.EmitBoolean((bool)value);
                    return true;
                case TypeCode.SByte:
                    ilGenerator.EmitSByte((sbyte)value);
                    return true;
                case TypeCode.Int16:
                    ilGenerator.EmitShort((short)value);
                    return true;
                case TypeCode.Int32:
                    ilGenerator.EmitInt((int)value);
                    return true;
                case TypeCode.Int64:
                    ilGenerator.EmitLong((long)value);
                    return true;
                case TypeCode.Single:
                    ilGenerator.EmitSingle((float)value);
                    return true;
                case TypeCode.Double:
                    ilGenerator.EmitDouble((double)value);
                    return true;
                case TypeCode.Char:
                    ilGenerator.EmitChar((char)value);
                    return true;
                case TypeCode.Byte:
                    ilGenerator.EmitByte((byte)value);
                    return true;
                case TypeCode.UInt16:
                    ilGenerator.EmitUShort((ushort)value);
                    return true;
                case TypeCode.UInt32:
                    ilGenerator.EmitUInt((uint)value);
                    return true;
                case TypeCode.UInt64:
                    ilGenerator.EmitULong((ulong)value);
                    return true;
                case TypeCode.Decimal:
                    ilGenerator.EmitDecimal((decimal)value);
                    return true;
                case TypeCode.String:
                    ilGenerator.EmitString((string)value);
                    return true;
                default:
                    return false;
            }
        }

        private static void EmitDecimalBits(this ILGenerator ilGenerator, decimal value)
        {
            int[] bits = Decimal.GetBits(value);
            ilGenerator.EmitInt(bits[0]);
            ilGenerator.EmitInt(bits[1]);
            ilGenerator.EmitInt(bits[2]);
            ilGenerator.EmitBoolean((bits[3] & 0x80000000) != 0);
            ilGenerator.EmitByte((byte)(bits[3] >> 16));
            ilGenerator.EmitNew(typeof(decimal).GetTypeInfo().GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(bool), typeof(byte) }));
        }

        /// <summary>
        /// 基本类型,string,decimal等的基础类型返回true
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>基本类型,string,decimal等的基础类型返回true</returns>
        private static bool CanEmitILConstant(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Char:
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Decimal:
                case TypeCode.String:
                    return true;
                default:
                    return false;
            }
        }
        #endregion
    }
}
