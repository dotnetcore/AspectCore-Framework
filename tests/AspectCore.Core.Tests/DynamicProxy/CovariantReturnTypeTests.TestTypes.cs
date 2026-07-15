using System;
using AspectCore.DynamicProxy;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspectCore.Core.Tests.DynamicProxy;

partial class CovariantReturnTypeTests
{
    public class ReturnTypeInterceptor : AbstractInterceptorAttribute
    {
        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            await context.Invoke(next);

            switch (context.ReturnValue)
            {
                case BaseResult returnValue:
                    {
                        var name = returnValue.Name + nameof(ReturnTypeInterceptor);
                        context.ReturnValue = (BaseResult)Activator.CreateInstance(returnValue.GetType(), [name]);
                        break;
                    }
                case string str:
                    {
                        context.ReturnValue = str + nameof(ReturnTypeInterceptor);
                        break;
                    }
            }
        }
    }

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
        public virtual object Property { get; } = nameof(CommonService);
        public virtual object Method() => nameof(CommonService);

        public virtual object InterceptedProperty { [ReturnTypeInterceptor] get; } = nameof(CommonService);
        [ReturnTypeInterceptor]
        public virtual object InterceptedMethod() => nameof(CommonService);
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

    public class DerivedLeafCovariantReturnService : LeafCovariantReturnService;

    // This class uses ordinary overrides after the covariant-return chain.
    public class OrdinaryOverrideService : LeafCovariantReturnService
    {
        public override LeafResult Property { get; } = new(nameof(OrdinaryOverrideService));
        public override LeafResult Method() => new(nameof(OrdinaryOverrideService));

        public override LeafResult InterceptedProperty { [ReturnTypeInterceptor] get; } = new(nameof(OrdinaryOverrideService));
        [ReturnTypeInterceptor]
        public override LeafResult InterceptedMethod() => new(nameof(OrdinaryOverrideService));
    }

    // This class inherits the ordinary overrides without adding new members.
    public class DerivedOrdinaryOverrideService : OrdinaryOverrideService;

    public class MethodOnlyCommonService
    {
        public virtual object Method() => nameof(Method);

        [ReturnTypeInterceptor]
        public virtual object InterceptedMethod() => nameof(InterceptedMethod);
    }

    public class MethodOnlyBaseCovariantReturnService : MethodOnlyCommonService
    {
        public override BaseResult Method() => new(nameof(MethodOnlyBaseCovariantReturnService));

        [ReturnTypeInterceptor]
        public override BaseResult InterceptedMethod() => new(nameof(MethodOnlyBaseCovariantReturnService));
    }

    public class MethodOnlyLeafCovariantReturnService : MethodOnlyBaseCovariantReturnService
    {
        public override LeafResult Method() => new(nameof(MethodOnlyLeafCovariantReturnService));

        [ReturnTypeInterceptor]
        public override LeafResult InterceptedMethod() => new(nameof(MethodOnlyLeafCovariantReturnService));
    }

    public class MethodOnlyOrdinaryOverrideService : MethodOnlyLeafCovariantReturnService
    {
        public override LeafResult Method() => new(nameof(MethodOnlyOrdinaryOverrideService));

        [ReturnTypeInterceptor]
        public override LeafResult InterceptedMethod() => new(nameof(MethodOnlyOrdinaryOverrideService));
    }

    public class PropertyOnlyCommonService
    {
        public virtual object Property { get; } = nameof(Property);

        public virtual object InterceptedProperty { [ReturnTypeInterceptor] get; } = nameof(InterceptedProperty);
    }

    public class PropertyOnlyBaseCovariantReturnService : PropertyOnlyCommonService
    {
        public override BaseResult Property { get; } = new(nameof(PropertyOnlyBaseCovariantReturnService));

        public override BaseResult InterceptedProperty { [ReturnTypeInterceptor] get; } = new(nameof(PropertyOnlyBaseCovariantReturnService));
    }

    public class PropertyOnlyLeafCovariantReturnService : PropertyOnlyBaseCovariantReturnService
    {
        public override LeafResult Property { get; } = new(nameof(PropertyOnlyLeafCovariantReturnService));

        public override LeafResult InterceptedProperty { [ReturnTypeInterceptor] get; } = new(nameof(PropertyOnlyLeafCovariantReturnService));
    }

    public class PropertyOnlyOrdinaryOverrideService : PropertyOnlyLeafCovariantReturnService
    {
        public override LeafResult Property { get; } = new(nameof(PropertyOnlyOrdinaryOverrideService));

        public override LeafResult InterceptedProperty { [ReturnTypeInterceptor] get; } = new(nameof(PropertyOnlyOrdinaryOverrideService));
    }

    public class InvariantGenericReturnBaseService
    {
        public virtual List<BaseResult> Items { get; } = [];

        public virtual List<BaseResult> Create() => [];
    }

    public class InvariantGenericReturnLeafService : InvariantGenericReturnBaseService
    {
        public new List<LeafResult> Items { get; } = [];

        public new List<LeafResult> Create() => [];
    }

    public interface ICovariantInterfaceBaseService
    {
        BaseResult Property { get; }
        BaseResult Method();

        BaseResult InterceptedProperty { [ReturnTypeInterceptor] get; }
        [ReturnTypeInterceptor]
        BaseResult InterceptedMethod();
    }

    public interface ICovariantInterfaceLeafService : ICovariantInterfaceBaseService
    {
        new LeafResult Property { get; }
        new LeafResult Method();

        new LeafResult InterceptedProperty { [ReturnTypeInterceptor] get; }
        [ReturnTypeInterceptor]
        new LeafResult InterceptedMethod();
    }

    public class CovariantInterfaceLeafImplementation : ICovariantInterfaceLeafService
    {
        public LeafResult Property { get; } = new(nameof(CovariantInterfaceLeafImplementation));
        public LeafResult Method() => new(nameof(CovariantInterfaceLeafImplementation));

        public LeafResult InterceptedProperty { [ReturnTypeInterceptor] get; } = new(nameof(CovariantInterfaceLeafImplementation));
        [ReturnTypeInterceptor]
        public LeafResult InterceptedMethod() => new(nameof(CovariantInterfaceLeafImplementation));

        BaseResult ICovariantInterfaceBaseService.Property => Property;
        BaseResult ICovariantInterfaceBaseService.Method() => Method();

        BaseResult ICovariantInterfaceBaseService.InterceptedProperty => InterceptedProperty;
        BaseResult ICovariantInterfaceBaseService.InterceptedMethod() => InterceptedMethod();
    }

    public class IndexerCommonService
    {
        public virtual object this[int index] => nameof(IndexerCommonService);

        public virtual object this[string key] { [ReturnTypeInterceptor] get => nameof(IndexerCommonService); }
    }

    public class IndexerBaseCovariantReturnService : IndexerCommonService
    {
        public override BaseResult this[int index] => new(nameof(IndexerBaseCovariantReturnService));

        public override BaseResult this[string key] { [ReturnTypeInterceptor] get => new(nameof(IndexerBaseCovariantReturnService)); }
    }

    public class IndexerLeafCovariantReturnService : IndexerBaseCovariantReturnService
    {
        public override LeafResult this[int index] => new(nameof(IndexerLeafCovariantReturnService));

        public override LeafResult this[string key] { [ReturnTypeInterceptor] get => new(nameof(IndexerLeafCovariantReturnService)); }
    }
}

partial class CovariantReturnTypeTests
{
    public class GenericMethodBaseService
    {
        public virtual BaseResult Convert<TValue>(TValue value) => new(nameof(GenericMethodBaseService));
    }

    public class GenericMethodLeafService : GenericMethodBaseService
    {
        public override LeafResult Convert<TValue>(TValue value) => new(nameof(GenericMethodLeafService));
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

        public virtual IEnumerable<TValue> ReturnList => [];
    }

    public class TypeGenericShapeLeafService<TValue> : TypeGenericShapeBaseService<TValue>
    {
        public override LeafResult Direct(TValue value) => new(nameof(TypeGenericShapeLeafService<TValue>));

        public override LeafResult List(List<TValue> value) => new(nameof(TypeGenericShapeLeafService<TValue>));

        public override List<TValue> ReturnList { get; } = [];
    }

    public interface IGenericCommonService<in TValue>
    {
        object Property { get; }
        object Method(TValue value);

        object InterceptedProperty { [ReturnTypeInterceptor] get; }
        [ReturnTypeInterceptor]
        object InterceptedMethod(TValue value);
    }

    public class GenericCommonService<TValue> : IGenericCommonService<TValue>
    {
        public virtual object Property { get; } = nameof(GenericCommonService<TValue>);
        public virtual object Method(TValue value) => nameof(GenericCommonService<TValue>);

        public virtual object InterceptedProperty { [ReturnTypeInterceptor] get; } = nameof(GenericCommonService<TValue>);
        [ReturnTypeInterceptor]
        public virtual object InterceptedMethod(TValue value) => nameof(GenericCommonService<TValue>);
    }

    public class GenericCovariantReturnService<TValue> : GenericCommonService<TValue>
    {
        public override BaseResult Property { get; } = new(nameof(GenericCovariantReturnService<TValue>));
        public override BaseResult Method(TValue value) => new(nameof(GenericCovariantReturnService<TValue>));

        public override BaseResult InterceptedProperty { [ReturnTypeInterceptor] get; } = new(nameof(GenericCovariantReturnService<TValue>));
        [ReturnTypeInterceptor]
        public override BaseResult InterceptedMethod(TValue value) => new(nameof(GenericCovariantReturnService<TValue>));
    }

    public class GenericLeafCovariantReturnService<TValue> : GenericCovariantReturnService<TValue>
    {
        public override LeafResult Property { get; } = new(nameof(GenericLeafCovariantReturnService<TValue>));
        public override LeafResult Method(TValue value) => new(nameof(GenericLeafCovariantReturnService<TValue>));

        public override LeafResult InterceptedProperty { [ReturnTypeInterceptor] get; } = new(nameof(GenericLeafCovariantReturnService<TValue>));
        [ReturnTypeInterceptor]
        public override LeafResult InterceptedMethod(TValue value) => new(nameof(GenericLeafCovariantReturnService<TValue>));
    }

    public class GenericDerivedLeafCovariantReturnService<TValue> : GenericLeafCovariantReturnService<TValue>;

    public class GenericParameterSubstitutionBaseService<TBase>
    {
        public virtual BaseResult Convert(TBase value) => new(nameof(GenericParameterSubstitutionBaseService<TBase>));
    }

    public class GenericParameterSubstitutionLeafService<TLeaf> : GenericParameterSubstitutionBaseService<BaseResult>
    {
        public LeafResult Convert(TLeaf value) => new(nameof(GenericParameterSubstitutionLeafService<TLeaf>));
    }

    public class ConstrainedGenericReturnBaseService
    {
        public virtual BaseResult Create<TValue>(TValue value) where TValue : LeafResult
            => value;
    }

    public class ConstrainedGenericReturnLeafService : ConstrainedGenericReturnBaseService
    {
        public override TValue Create<TValue>(TValue value) => value;
    }
}

