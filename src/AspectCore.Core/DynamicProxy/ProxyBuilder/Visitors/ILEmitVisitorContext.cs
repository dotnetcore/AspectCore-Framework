using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Extensions.Reflection.Emit;

namespace AspectCore.DynamicProxy.ProxyBuilder.Visitors
{
    internal class ILEmitVisitorContext
    {
        public ModuleBuilder ModuleBuilder { get; }

        public TypeBuilder TypeBuilder { get; set; }

        public Type ServiceType { get; set; }

        public Dictionary<string, FieldBuilder> Fields { get; } = new Dictionary<string, FieldBuilder>();

        public MethodConstantTable MethodConstants { get; set; }

        public ILGenerator CurrentILGenerator { get; set; }

        public MethodBuilder CurrentMethodBuilder { get; set; }

        public ILEmitVisitorContext(ModuleBuilder moduleBuilder)
        {
            ModuleBuilder = moduleBuilder ?? throw new ArgumentNullException(nameof(moduleBuilder));
        }
    }

    internal class MethodConstantTable
    {
        private readonly TypeBuilder _nestedTypeBuilder;
        private readonly ConstructorBuilder _constructorBuilder;
        private readonly ILGenerator _ilGen;
        private readonly Dictionary<string, FieldBuilder> _fields;

        public MethodConstantTable(TypeBuilder typeBuilder)
        {
            _fields = new Dictionary<string, FieldBuilder>();
            _nestedTypeBuilder = typeBuilder.DefineNestedType("MethodConstant", TypeAttributes.NestedPrivate);
            _constructorBuilder = _nestedTypeBuilder.DefineTypeInitializer();
            _ilGen = _constructorBuilder.GetILGenerator();
        }

        public void AddMethod(string name, MethodInfo method)
        {
            if (!_fields.ContainsKey(name))
            {
                var field = _nestedTypeBuilder.DefineField(name, typeof(MethodInfo), FieldAttributes.Static | FieldAttributes.InitOnly | FieldAttributes.Assembly);
                _fields.Add(name, field);
                if (method != null)
                {
                    _ilGen.EmitMethod(method);
                    _ilGen.Emit(OpCodes.Stsfld, field);
                }
            }
        }

        public void LoadMethod(ILGenerator ilGen, string name)
        {
            if (_fields.TryGetValue(name, out FieldBuilder field))
            {
                ilGen.Emit(OpCodes.Ldsfld, field);
                return;
            }
            throw new InvalidOperationException($"Failed to find the method associated with the specified key {name}.");
        }

        public void Compile()
        {
            _ilGen.Emit(OpCodes.Ret);
            _nestedTypeBuilder.CreateTypeInfo();
        }
    }
}
