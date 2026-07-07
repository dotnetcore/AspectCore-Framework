using System.Collections.Generic;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;

namespace AspectCore.Tests;

public class CovariantReturnTypes
{
    public class ReturnTypeInterceptor : AbstractInterceptorAttribute
    {
        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            await context.Invoke(next);

            if (context.ReturnValue is BaseResult returnValue)
            {
                returnValue.Name += nameof(ReturnTypeInterceptor);
            }
        }
    }

    public class BaseResult(string name)
    {
        public string Name { get; set; } = name;
    }

    public class MidResult(string name) : BaseResult(name);

    public class LeafResult(string name) : MidResult(name);

    public interface ICommonService
    {
        object Property { get; }
        object Method();

        object InterceptedProperty { [ReturnTypeInterceptor] get; }
        [ReturnTypeInterceptor]
        object InterceptedMethod();
    }

    public class CommonService : ICommonService
    {
        public virtual object Property { get; } = nameof(Property);
        public virtual object Method() => nameof(Method);

        public virtual object InterceptedProperty { [ReturnTypeInterceptor] get; } = nameof(InterceptedProperty);
        [ReturnTypeInterceptor]
        public virtual object InterceptedMethod() => nameof(InterceptedMethod);
    }

    public class BaseCovariantReturnService : CommonService
    {
        public override BaseResult Property { get; } = new(nameof(BaseCovariantReturnService));
        public override BaseResult Method() => new(nameof(BaseCovariantReturnService));

        public override BaseResult InterceptedProperty { [ReturnTypeInterceptor] get; } = new(nameof(BaseCovariantReturnService));
        [ReturnTypeInterceptor]
        public override BaseResult InterceptedMethod() => new(nameof(BaseCovariantReturnService));
    }

    public class MidCovariantReturnService : BaseCovariantReturnService
    {
        public override MidResult Property { get; } = new(nameof(MidCovariantReturnService));
        public override MidResult Method() => new(nameof(MidCovariantReturnService));

        public override MidResult InterceptedProperty { [ReturnTypeInterceptor] get; } = new(nameof(MidCovariantReturnService));
        [ReturnTypeInterceptor]
        public override MidResult InterceptedMethod() => new(nameof(MidCovariantReturnService));
    }

    public class LeafCovariantReturnService : MidCovariantReturnService
    {
        public override LeafResult Property { get; } = new(nameof(LeafCovariantReturnService));
        public override LeafResult Method() => new(nameof(LeafCovariantReturnService));

        public override LeafResult InterceptedProperty { [ReturnTypeInterceptor] get; } = new(nameof(LeafCovariantReturnService));
        [ReturnTypeInterceptor]
        public override LeafResult InterceptedMethod() => new(nameof(LeafCovariantReturnService));
    }

    // this class just does common overriding.
    public class DerivedLeafCovariantReturnService : LeafCovariantReturnService
    {
        public override LeafResult Property { get; } = new(nameof(DerivedLeafCovariantReturnService));
        public override LeafResult Method() => new(nameof(DerivedLeafCovariantReturnService));

        public override LeafResult InterceptedProperty { [ReturnTypeInterceptor] get; } = new(nameof(DerivedLeafCovariantReturnService));
        [ReturnTypeInterceptor]
        public override LeafResult InterceptedMethod() => new(nameof(DerivedLeafCovariantReturnService));
    }

    public class GenericMethodBaseService
    {
        public virtual BaseResult Convert<TValue>(TValue value) => new(nameof(GenericMethodBaseService));
    }

    public class GenericMethodLeafService : GenericMethodBaseService
    {
        public override LeafResult Convert<TValue>(TValue value) => new(nameof(GenericMethodLeafService));
    }

    public class OrdinaryOverrideBaseService
    {
        public virtual BaseResult Method() => new(nameof(OrdinaryOverrideBaseService));
    }

    public class OrdinaryOverrideLeafService : OrdinaryOverrideBaseService
    {
        public override BaseResult Method() => new(nameof(OrdinaryOverrideLeafService));
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
}
