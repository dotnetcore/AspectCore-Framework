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

        protected abstract FieldBuilder VisitFieldGenerator(FieldGenerator generator);

        protected abstract PropertyBuilder VisitPropertyGenerator(PropertyGenerator generator);

        protected abstract MethodBuilder VisitMethodGenerator(MethodGenerator generator);

        protected abstract ILGenerator VisitMethodBodyGenerator(MethodBodyGenerator generator);

        protected abstract ConstructorBuilder VisitConstructorGenerator(ConstructorGenerator generator);

        protected abstract ConstructorBuilder VisitDefaultConstructorGenerator(DefaultConstructorGenerator generator);

        protected abstract TypeBuilder VisitTypeGenerator(TypeGenerator generator);
    }
}
