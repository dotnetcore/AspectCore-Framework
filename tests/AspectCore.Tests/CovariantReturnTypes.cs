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
}
