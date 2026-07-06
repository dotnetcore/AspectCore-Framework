using System.Linq;
using System.Reflection;
using AspectCore.Extensions;
using Xunit;

namespace AspectCore.Tests.Extensions;

public class TypeExtensionsTests
{
    // Basic covariant override between classes
    private class Animal;

    private class Dog : Animal;

    private class BaseClass
    {
        public virtual Animal Make() => new();
    }

    private class DerivedClass : BaseClass
    {
        public override Dog Make() => new();
    }

    [Fact]
    public void GetCovariantReturnMethods_FindsBasicCovariantReturnMethod()
    {
        var methods = typeof(DerivedClass).GetCovariantReturnMethods();
        Assert.Single(methods);

        var info = methods.Single();
        Assert.Equal(nameof(DerivedClass.Make), info.CovariantReturnMethod.Name);
        Assert.Equal(typeof(Dog), info.CovariantReturnMethod.ReturnType);
        Assert.Equal(typeof(Animal), info.OverriddenMethod.ReturnType);
        Assert.True(info.OverriddenMethod.DeclaringType == typeof(DerivedClass)
                 || info.OverriddenMethod.DeclaringType == typeof(BaseClass));
    }

    // Multi-level inheritance
    private class B1 { public virtual B1 Clone() => new(); }

    private class B2 : B1 { public override B2 Clone() => new(); }

    private class B3 : B2;

    [Fact]
    public void GetCovariantReturnMethods_DoesNotReportEmptyForDeeperHierarchy()
    {
        var methods = typeof(B3).GetCovariantReturnMethods();
        // Should inherit from B2 → B1, so no new covariant method
        Assert.Single(methods);

        methods = typeof(B2).GetCovariantReturnMethods();
        Assert.Single(methods);
    }

    // Interface with covariant return
    private interface IFactory<out T> { T Create(); }

    private class Widget;

    private class FancyWidget : Widget;

    private class WidgetFactory : IFactory<Widget>
    {
        public virtual Widget Create() => new();
    }

    private class FancyWidgetFactory : WidgetFactory, IFactory<FancyWidget>
    {
        public override FancyWidget Create() => new();
    }

    [Fact]
    public void GetCovariantReturnMethods_HandlesInterfaceCovariantReturnCorrectly()
    {
        var info = typeof(FancyWidgetFactory).GetCovariantReturnMethods().Single();
        Assert.Equal(typeof(FancyWidget), info.CovariantReturnMethod.ReturnType);
        Assert.Equal(typeof(Widget), info.OverriddenMethod.ReturnType);
        // Should list IFactory<FancyWidget>.Create() as an interface declaration
        Assert.Contains(info.InterfaceDeclarations, m => m.DeclaringType!.GetGenericTypeDefinition() == typeof(IFactory<>));
    }

    // Explicit interface implementation
    private interface ICreator<out T> { T Create(); }

    private class Creator : ICreator<Animal>
    {
        Animal ICreator<Animal>.Create() => new();
    }

    private class DogCreator : Creator, ICreator<Dog>
    {
        Dog ICreator<Dog>.Create() => new();
    }

    [Fact]
    public void GetCovariantReturnMethods_FindsExplicitInterfaceImplementation()
    {
        var infos = typeof(DogCreator).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                                      .Where(m => m.Name.Contains("ICreator"))
                                      .SelectMany(m => m.GetInterfaceDeclarations())
                                      .ToList();
        Assert.NotEmpty(infos);
        Assert.All(infos, i => Assert.Equal("Create", i.Name));
    }

    // 5Generic method covariance
    private class GenBase<T> { public virtual T Build() => default!; }

    private class GenDerived : GenBase<Animal> { public override Dog Build() => new(); }

    [Fact]
    public void GetCovariantReturnMethods_WorksForGenericCovariantReturn()
    {
        var info = typeof(GenDerived).GetCovariantReturnMethods().Single();
        Assert.Equal(typeof(Dog), info.CovariantReturnMethod.ReturnType);
        Assert.Empty(info.CovariantReturnMethod.GetGenericArguments());
        Assert.Empty(info.OverriddenMethod.GetGenericArguments());
        Assert.NotEqual(info.OverriddenMethod, info.OverriddenMethod.GetBaseDefinition());
    }

    // Same slot verification
    [Fact]
    public void IsSameBaseDefinition_CovariantReturn()
    {
        var m1 = typeof(DerivedClass).GetMethod(nameof(DerivedClass.Make))!;
        var m2 = typeof(BaseClass).GetMethod(nameof(BaseClass.Make))!;
        Assert.False(m1.IsSameBaseDefinition(m2));
    }
}
