#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0060 // Remove unused parameter
// ReSharper disable UnusedTypeParameter
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Xunit;

namespace AspectCore.Tests.Extensions.TypeExtensionsTests;

public class TestTypes
{
    public interface ICommonService
    {
        object Property { get; }
        object Method();
    }

    public class CommonService : ICommonService
    {
        public virtual object Property { get; } = nameof(Property);
        public virtual object Method() => nameof(Method);
    }

    public class OrdinaryOverrideService : CommonService
    {
        public override object Property { get; } = new BaseResult(nameof(OrdinaryOverrideService));
        public override object Method() => new BaseResult(nameof(OrdinaryOverrideService));
    }

    public class BaseCovariantReturnService : CommonService
    {
        public override BaseResult Property { get; } = new(nameof(BaseCovariantReturnService));
        public override BaseResult Method() => new(nameof(BaseCovariantReturnService));
    }

    public class MidCovariantReturnService : BaseCovariantReturnService
    {
        public override MidResult Property { get; } = new(nameof(MidCovariantReturnService));
        public override MidResult Method() => new(nameof(MidCovariantReturnService));
    }

    public class LeafCovariantReturnService : MidCovariantReturnService
    {
        public override LeafResult Property { get; } = new(nameof(LeafCovariantReturnService));
        public override LeafResult Method() => new(nameof(LeafCovariantReturnService));
    }

    public class DerivedLeafCovariantReturnService : LeafCovariantReturnService;

    public class OrdinaryOverrideLeafService : LeafCovariantReturnService
    {
        public override LeafResult Property { get; } = new(nameof(OrdinaryOverrideLeafService));
        public override LeafResult Method() => new(nameof(OrdinaryOverrideLeafService));
    }

    public class DerivedOrdinaryOverrideLeafService : OrdinaryOverrideLeafService;

    public class GenericMethodBaseService
    {
        public virtual BaseResult Convert<TValue>(TValue value) => new(nameof(GenericMethodBaseService));
    }

    public class GenericMethodLeafService : GenericMethodBaseService
    {
        public override LeafResult Convert<TValue>(TValue value) => new(nameof(GenericMethodLeafService));
    }

    public class ParameterBaseService
    {
        public virtual BaseResult WithBaseParameter(BaseResult value) => value;

        public virtual BaseResult WithTwoParameters(BaseResult value, LeafResult other) => value;
    }

    public class ParameterLeafService : ParameterBaseService
    {
        public override LeafResult WithBaseParameter(BaseResult value) => new(nameof(ParameterLeafService));

        public override LeafResult WithTwoParameters(BaseResult value, LeafResult other) => new(nameof(ParameterLeafService));
    }

    public class MismatchedParameterLeafService
    {
        public LeafResult WithBaseParameter(LeafResult value) => value;
    }

    public class GenericMethodShapeBaseService
    {
        public virtual BaseResult Direct<TValue>(TValue value) => new(nameof(GenericMethodShapeBaseService));

        public virtual BaseResult Array<TValue>(TValue[] value) => new(nameof(GenericMethodShapeBaseService));

        public virtual BaseResult List<TValue>(List<TValue> value) => new(nameof(GenericMethodShapeBaseService));

        public virtual BaseResult Dictionary<TValue>(Dictionary<string, TValue> value) => new(nameof(GenericMethodShapeBaseService));

        public virtual BaseResult ByRef<TValue>(ref TValue value) => new(nameof(GenericMethodShapeBaseService));

        public virtual IEnumerable<TValue> ReturnList<TValue>() => [];
    }

    public class GenericMethodShapeLeafService : GenericMethodShapeBaseService
    {
        public override LeafResult Direct<TValue>(TValue value) => new(nameof(GenericMethodShapeLeafService));

        public override LeafResult Array<TValue>(TValue[] value) => new(nameof(GenericMethodShapeLeafService));

        public override LeafResult List<TValue>(List<TValue> value) => new(nameof(GenericMethodShapeLeafService));

        public override LeafResult Dictionary<TValue>(Dictionary<string, TValue> value) => new(nameof(GenericMethodShapeLeafService));

        public override LeafResult ByRef<TValue>(ref TValue value) => new(nameof(GenericMethodShapeLeafService));

        public override List<TValue> ReturnList<TValue>() => [];
    }

    public class TypeGenericShapeBaseService<TValue>
    {
        public virtual BaseResult Direct(TValue value) => new(nameof(TypeGenericShapeBaseService<TValue>));

        public virtual BaseResult List(List<TValue> value) => new(nameof(TypeGenericShapeBaseService<TValue>));

        public virtual IEnumerable<TValue> ReturnList() => [];
    }

    public class TypeGenericShapeLeafService<TValue> : TypeGenericShapeBaseService<TValue>
    {
        public override LeafResult Direct(TValue value) => new(nameof(TypeGenericShapeLeafService<TValue>));

        public override LeafResult List(List<TValue> value) => new(nameof(TypeGenericShapeLeafService<TValue>));

        public override List<TValue> ReturnList() => [];
    }

    public class MixedGenericShapeBaseService<TType>
    {
        public virtual BaseResult TypeAndMethod<TMethod>(TType typeValue, TMethod methodValue) => new(nameof(MixedGenericShapeBaseService<TType>));

        public virtual BaseResult MethodThenType<TMethod>(TMethod methodValue, TType typeValue) => new(nameof(MixedGenericShapeBaseService<TType>));
    }

    public class MixedGenericShapeLeafService<TType> : MixedGenericShapeBaseService<TType>
    {
        public override LeafResult TypeAndMethod<TMethod>(TType typeValue, TMethod methodValue) => new(nameof(MixedGenericShapeLeafService<TType>));

        public override LeafResult MethodThenType<TMethod>(TMethod methodValue, TType typeValue) => new(nameof(MixedGenericShapeLeafService<TType>));
    }

    public class TypeGenericParameterBaseService<TValue>
    {
        public virtual BaseResult Compare(TValue value) => new(nameof(TypeGenericParameterBaseService<TValue>));
    }

    public class MethodGenericParameterLeafService
    {
        public LeafResult Compare<TValue>(TValue value) => new(nameof(MethodGenericParameterLeafService));
    }

    public class GenericPositionZeroBaseService
    {
        public virtual BaseResult Compare<TFirst, TSecond>(TFirst value) => new(nameof(GenericPositionZeroBaseService));
    }

    public class GenericPositionOneLeafService
    {
        public LeafResult Compare<TFirst, TSecond>(TSecond value) => new(nameof(GenericPositionOneLeafService));
    }

    public class ConstrainedGenericReturnBaseService
    {
        public virtual BaseResult Create<TValue>(TValue value)
            where TValue : LeafResult
            => value;
    }

    public class ConstrainedGenericReturnLeafService : ConstrainedGenericReturnBaseService
    {
        public override TValue Create<TValue>(TValue value) => value;
    }

    public class TypeGenericParameterSourceBaseService<TBase>
    {
        public virtual BaseResult Convert(TBase value) => new(nameof(TypeGenericParameterSourceBaseService<TBase>));
    }

    public class TypeGenericParameterSourceLeafService<TLeaf> : TypeGenericParameterSourceBaseService<BaseResult>
    {
        public LeafResult Convert(TLeaf value) => new(nameof(TypeGenericParameterSourceLeafService<TLeaf>));
    }

    public class ArrayRankBaseService
    {
        public virtual BaseResult Convert<TValue>(TValue[] value) => new(nameof(ArrayRankBaseService));
    }

    public class ArrayRankLeafService : ArrayRankBaseService
    {
        public LeafResult Convert<TValue>(TValue[,] value) => new(nameof(ArrayRankLeafService));
    }

    public class JaggedArrayRankBaseService
    {
        public virtual BaseResult Convert<TValue>(TValue[][] value) => new(nameof(JaggedArrayRankBaseService));
    }

    public class JaggedArrayRankLeafService : JaggedArrayRankBaseService
    {
        public LeafResult Convert<TValue>(TValue[][,] value) => new(nameof(JaggedArrayRankLeafService));
    }

    public class ArrayNestingBaseService
    {
        public virtual BaseResult Convert<TValue>(TValue[] value) => new(nameof(ArrayNestingBaseService));
    }

    public class ArrayNestingLeafService : ArrayNestingBaseService
    {
        public LeafResult Convert<TValue>(TValue[][] value) => new(nameof(ArrayNestingLeafService));
    }

    public class InvariantGenericReturnBaseService
    {
        public virtual List<BaseResult> Create() => [];
    }

    public class InvariantGenericReturnLeafService : InvariantGenericReturnBaseService
    {
        public new List<LeafResult> Create() => [];
    }

    public class GenericMethodWithoutParameterBaseService
    {
        public virtual BaseResult Create<TValue>() => new(nameof(GenericMethodWithoutParameterBaseService));
    }

    public class GenericMethodWithoutParameterLeafService : GenericMethodWithoutParameterBaseService
    {
        public override LeafResult Create<TValue>() => new(nameof(GenericMethodWithoutParameterLeafService));
    }

    private static Type CreateTypeGenericParameterSourceLeafService()
    {
        var assemblyName = new AssemblyName("DynamicTestTypes");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicTestTypes");

        var typeBuilder = moduleBuilder.DefineType(
            "TypeGenericParameterSourceLeafService`1",
            TypeAttributes.Public | TypeAttributes.Class,
            typeof(TypeGenericParameterSourceBaseService<>).MakeGenericType(typeof(BaseResult)));

        var genericParameters = typeBuilder.DefineGenericParameters("TLeaf");
        var tLeaf = genericParameters[0];

        var methodBuilder = typeBuilder.DefineMethod(
            "Convert",
            MethodAttributes.Public
            | MethodAttributes.Virtual
            | MethodAttributes.NewSlot
            | MethodAttributes.HideBySig,
            typeof(LeafResult),
            [tLeaf]);

        var preserve = AspectCore.Extensions.TypeExtensions.PreserveBaseOverridesAttribute;
        Assert.NotNull(preserve);

        var ctor = preserve.GetConstructor(Type.EmptyTypes);
        Assert.NotNull(ctor);

        methodBuilder.SetCustomAttribute(new CustomAttributeBuilder(ctor, []));

        var il = methodBuilder.GetILGenerator();
        il.Emit(OpCodes.Ldstr, "TypeGenericParameterSourceLeafService<TLeaf>");
        il.Emit(OpCodes.Newobj, typeof(LeafResult).GetConstructor([typeof(string)])!);
        il.Emit(OpCodes.Ret);

        return typeBuilder.CreateTypeInfo().AsType();
    }

    public static readonly Type DynamicTypeGenericParameterSourceLeafService = CreateTypeGenericParameterSourceLeafService();
}
