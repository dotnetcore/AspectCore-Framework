using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Core.Generator
{
    public abstract class PropertyGenerator : AbstractGenerator<TypeBuilder, PropertyBuilder>
    {
        public abstract string PropertyName { get; }

        public abstract PropertyAttributes PropertyAttributes { get; }

        public abstract CallingConventions CallingConventions { get; }

        public abstract Type PropertyType { get; }

        public abstract bool CanRead { get; }

        public abstract bool CanWrite { get; }

        public abstract MethodInfo GetMethod { get; }

        public abstract MethodInfo SetMethod { get; }

        public virtual Type[] ParameterTypes
        {
            get
            {
                return Type.EmptyTypes;
            }
        }

        protected PropertyGenerator(TypeBuilder declaringMember) : base(declaringMember)
        {
        }

        protected override PropertyBuilder ExecuteBuild()
        {
            var propertyBuilder = DeclaringMember.DefineProperty(PropertyName, PropertyAttributes, CallingConventions, PropertyType, ParameterTypes);

            if (CanRead)
            {
                var readMethodGenerator = GetReadMethodGenerator(DeclaringMember);
                propertyBuilder.SetGetMethod(readMethodGenerator.Build());
            }

            if (CanWrite)
            {
                var writeMethodGenerator = GetWriteMethodGenerator(DeclaringMember);
                propertyBuilder.SetSetMethod(writeMethodGenerator.Build());
            }

            return propertyBuilder;
        }

        protected abstract MethodGenerator GetReadMethodGenerator(TypeBuilder declaringType);

        protected abstract MethodGenerator GetWriteMethodGenerator(TypeBuilder declaringType);
    }
}
