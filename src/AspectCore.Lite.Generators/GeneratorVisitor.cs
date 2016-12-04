using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace AspectCore.Lite.Generators
{
    public abstract class GeneratorVisitor
    {
        public virtual object VisitGenerator(MetaDataGenerator generator)
        {

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
