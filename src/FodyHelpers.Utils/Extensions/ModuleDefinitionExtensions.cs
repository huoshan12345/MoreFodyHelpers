namespace FodyHelpers.Utils.Extensions;

public static class ModuleDefinitionExtensions
{
    public static TypeReference ImportType(this ModuleDefinition module, Type type)
    {
        return module.ImportReference(type);
    }

    public static TypeReference ImportType<T>(this ModuleDefinition module)
    {
        return ImportType(module, typeof(T));
    }

    public static TypeReference ImportVoid(this ModuleDefinition module)
    {
        return ImportType(module, typeof(void));
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
        return module.AddType(@namespace, name, attributes, baseType == null ? null : ImportType(module, baseType));
    }

    public static TypeDefinition GetOrAddIgnoresAccessChecksToAttribute(this ModuleDefinition module)
    {
        const string ns = "System.Runtime.CompilerServices";
        const string name = "IgnoresAccessChecksToAttribute";
        var attr = module.GetType(ns, name);
        if (attr != null)
            return attr;

        var type = module.AddType(ns, name, TypeAttributes.Class | TypeAttributes.NotPublic, typeof(Attribute));
        var property = type.AddAutoProperty<string>("AssemblyName", setterAttributes: MethodAttributes.Private);
        var ctor = type.AddConstructor(instructions: new[]
        {
            Instruction.Create(OpCodes.Ldarg_1),
            Instruction.Create(OpCodes.Callvirt, property.GetMethod),
        });
        ctor.AddParameter<string>("assemblyName");
        return type;
    }
}