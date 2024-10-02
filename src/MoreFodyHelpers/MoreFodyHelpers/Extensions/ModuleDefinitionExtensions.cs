namespace MoreFodyHelpers.Extensions;

public static class ModuleDefinitionExtensions
{
    public static TypeReference ImportReference<T>(this ModuleDefinition module)
    {
        return module.ImportReference(typeof(T));
    }

    public static TypeReference ImportVoid(this ModuleDefinition module)
    {
        return module.ImportReference(typeof(void));
    }

    public static MethodReference GetConstructor<T>(this ModuleDefinition module, params Type[] parameters)
    {
        return module.GetConstructor(typeof(T), parameters);
    }

    public static MethodReference GetConstructor(this ModuleDefinition module, Type type, params Type[] parameters)
    {
        var result = module.ImportReference(type.GetConstructor(parameters));
        return result ?? throw new ArgumentException($"There's no constructor with those parameters in type {type.FullName}");
    }

    public static TypeDefinition AddType(this ModuleDefinition module, string @namespace, string name, TypeAttributes attributes, TypeReference? baseType = null)
    {
        var typeDef = new TypeDefinition(@namespace, name, attributes, baseType);
        module.Types.Add(typeDef);
        return typeDef;
    }

    public static TypeDefinition AddType(this ModuleDefinition module, string @namespace, string name, TypeAttributes attributes, Type? baseType = null)
    {
        return module.AddType(@namespace, name, attributes, baseType == null ? null : module.ImportReference(baseType));
    }

    public static TypeDefinition GetOrAddIgnoresAccessChecksToAttribute(this ModuleDefinition module)
    {
        const string ns = "System.Runtime.CompilerServices";
        const string name = "IgnoresAccessChecksToAttribute";
        var attr = module.GetType(ns, name);
        if (attr != null)
            return attr;

        var attrRef = module.ImportReference<Attribute>();
        var type = module.AddType(ns, name, TypeAttributes.Class | TypeAttributes.NotPublic | TypeAttributes.BeforeFieldInit, attrRef);
        var property = type.AddAutoProperty<string>("AssemblyName", setterAttributes: MethodAttributes.Private);
        var baseCtor = attrRef.Resolve().GetConstructor();
        var ctor = type.AddConstructor(instructions: new[]
        {
            Instruction.Create(OpCodes.Call, module.ImportReference(baseCtor)),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldarg_1),
            Instruction.Create(OpCodes.Callvirt, property.SetMethod),
        });
        ctor.AddParameter<string>("assemblyName");
        return type;
    }

    public static ModuleDefinition AddIgnoresAccessCheck(this ModuleDefinition module, string? assemblyName = null)
    {
        var attr = module.GetOrAddIgnoresAccessChecksToAttribute();
        var stringType = attr.Module.ImportReference<string>();
        var ctor = attr.GetConstructor(stringType);
        var attribute = new CustomAttribute(ctor);
        var arg = new CustomAttributeArgument(stringType, assemblyName ?? attr.Module.Assembly.Name.Name);
        attribute.ConstructorArguments.Add(arg);
        attr.Module.Assembly.CustomAttributes.Add(attribute);
        return module;
    }
}