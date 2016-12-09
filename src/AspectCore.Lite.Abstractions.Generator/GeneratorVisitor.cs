using System.Reflection.Emit;

namespace AspectCore.Lite.Abstractions.Generator
{
    public abstract class GeneratorVisitor
    {
        public virtual object VisitGenerator(Generator generator)
        {
            var fieldGenerator = generator as FieldGenerator;
            if (fieldGenerator != null)
            {
                return VisitFieldGenerator(fieldGenerator);
            }

            var propertyGenerator = generator as PropertyGenerator;
            if (propertyGenerator != null)
            {
                return VisitPropertyGenerator(propertyGenerator);
            }

            var methodGenerator = generator as MethodGenerator;
            if (methodGenerator != null)
            {
                return VisitMethodGenerator(methodGenerator);
            }

            var methodBodyGenerator = generator as MethodBodyGenerator;
            if (methodBodyGenerator != null)
            {
                return VisitMethodBodyGenerator(methodBodyGenerator);
            }

            var constructorGenerator = generator as ConstructorGenerator;
            if (constructorGenerator != null)
            {
                return VisitConstructorGenerator(constructorGenerator);
            }

            var defaultConstructorGenerator = generator as DefaultConstructorGenerator;
            if (defaultConstructorGenerator != null)
            {
                return VisitDefaultConstructorGenerator(defaultConstructorGenerator);
            }

            var typeGenerator = generator as TypeGenerator;
            if (fieldGenerator != null)
            {
                return VisitTypeGenerator(typeGenerator);
            }

            return default(object);
        }

        protected virtual FieldBuilder VisitFieldGenerator(FieldGenerator generator)
        {
            return generator.Accept(this);
        }

        protected virtual PropertyBuilder VisitPropertyGenerator(PropertyGenerator generator)
        {
            return generator.Accept(this);
        }

        protected virtual MethodBuilder VisitMethodGenerator(MethodGenerator generator)
        {
            return generator.Accept(this);
        }

        protected virtual ILGenerator VisitMethodBodyGenerator(MethodBodyGenerator generator)
        {
            return generator.Accept(this);
        }

        protected virtual ConstructorBuilder VisitConstructorGenerator(ConstructorGenerator generator)
        {
            return generator.Accept(this);
        }

        protected virtual ConstructorBuilder VisitDefaultConstructorGenerator(DefaultConstructorGenerator generator)
        {
            return generator.Accept(this);
        }

        protected virtual TypeBuilder VisitTypeGenerator(TypeGenerator generator)
        {
            return generator.Accept(this);
        }
    }
}
