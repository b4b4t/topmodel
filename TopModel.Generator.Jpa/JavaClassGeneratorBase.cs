﻿using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Jpa;

/// <summary>
/// Générateur de fichiers de modèles JPA.
/// </summary>
public abstract class JavaClassGeneratorBase : ClassGeneratorBase<JpaConfig>
{
    private JavaConstructorGenerator? _jpaModelConstructorGenerator;
    private JpaModelPropertyGenerator? _jpaModelPropertyGenerator;

    public JavaClassGeneratorBase(ILogger<JavaClassGeneratorBase> logger)
        : base(logger)
    {
    }

    protected static Dictionary<string, string> NewableTypes => new()
    {
        ["List"] = "ArrayList",
        ["Set"] = "HashSet"
    };

    protected string JavaxOrJakarta => Config.JavaxOrJakarta;

    protected virtual JavaConstructorGenerator ConstructorGenerator
    {
        get
        {
            _jpaModelConstructorGenerator ??= new JavaConstructorGenerator(Config);
            return _jpaModelConstructorGenerator;
        }
    }

    protected JpaModelPropertyGenerator JpaModelPropertyGenerator
    {
        get
        {
            _jpaModelPropertyGenerator ??= new JpaModelPropertyGenerator(Config, Classes, NewableTypes);
            return _jpaModelPropertyGenerator;
        }
    }

    protected virtual void WriteAnnotations(JavaWriter fw, Class classe, string tag)
    {
        fw.WriteDocStart(0, classe.Comment);
        fw.WriteDocEnd(0);
        if (Config.GeneratedHint)
        {
            fw.WriteAnnotation(0, Config.GeneratedAnnotation);
        }

        fw.AddImports(Config.GetDecoratorImports(classe, tag).ToList());
        foreach (var a in Config.GetDecoratorAnnotations(classe, tag))
        {
            fw.WriteLine($"{(a.StartsWith("@") ? string.Empty : "@")}{a}");
        }
    }

    protected void WriteFieldsEnum(JavaWriter fw, Class classe, string tag)
    {
        if (!classe.Properties.Any())
        {
            return;
        }

        if (Config.FieldsEnumInterface != null)
        {
            fw.AddImport(Config.FieldsEnumInterface.Replace("<>", string.Empty));
        }

        fw.WriteLine();
        fw.WriteDocStart(1, $"Enumération des champs de la classe {{@link {classe.GetImport(Config, tag)} {classe.NamePascal}}}");
        fw.WriteDocEnd(1);
        string enumDeclaration = @$"public enum Fields ";
        if (Config.FieldsEnumInterface != null)
        {
            enumDeclaration += $"implements {Config.FieldsEnumInterface.Split(".").Last().Replace("<>", $"<{classe.NamePascal}>")}";
        }

        enumDeclaration += " {";
        fw.WriteLine(1, enumDeclaration);

        var props = classe.GetProperties(Classes).Select(prop =>
        {
            string name;
            if (prop is AssociationProperty ap && ap.Association.IsPersistent && !Config.UseJdbc)
            {
                name = ap.NameByClassCamel.ToConstantCase();
            }
            else
            {
                name = prop.NameCamel.ToConstantCase();
            }

            var javaType = Config.GetType(prop, useClassForAssociation: classe.IsPersistent && !Config.UseJdbc && prop is AssociationProperty asp && asp.Association.IsPersistent);
            javaType = javaType.Split("<")[0];
            return $"        {name}({javaType}.class)";
        });

        fw.WriteLine(string.Join(", //\n", props) + ";");

        fw.WriteLine();

        fw.WriteLine(2, "private final Class<?> type;");
        fw.WriteLine();
        fw.WriteLine(2, "Fields(Class<?> type) {");
        fw.WriteLine(3, "this.type = type;");
        fw.WriteLine(2, "}");

        fw.WriteLine();

        fw.WriteLine(2, "public Class<?> getType() {");
        fw.WriteLine(3, "return this.type;");
        fw.WriteLine(2, "}");

        fw.WriteLine(1, "}");
    }

    protected virtual void WriteGetters(JavaWriter fw, Class classe, string tag)
    {
        foreach (var property in classe.GetProperties(Classes))
        {
            JpaModelPropertyGenerator.WriteGetter(fw, tag, property);
        }
    }

    protected virtual void WriteSetters(JavaWriter fw, Class classe, string tag)
    {
        foreach (var property in classe.GetProperties(Classes))
        {
            JpaModelPropertyGenerator.WriteSetter(fw, tag, property);
        }
    }

    protected void WriteToMappers(JavaWriter fw, Class classe, string tag)
    {
        var toMappers = classe.ToMappers.Where(p => Classes.Contains(p.Class)).Select(m => (classe, m))
        .OrderBy(m => m.m.Name)
        .ToList();

        foreach (var toMapper in toMappers)
        {
            var (clazz, mapper) = toMapper;
            fw.AddImport(mapper.Class.GetImport(Config, tag));
            fw.WriteLine();
            fw.WriteDocStart(1, $"Mappe '{classe}' vers '{mapper.Class.NamePascal}'");
            if (mapper.Comment != null)
            {
                fw.WriteLine(1, $" * {mapper.Comment}");
            }

            fw.WriteParam("target", $"Instance pré-existante de '{mapper.Class.NamePascal}'. Une nouvelle instance sera créée si non spécifié.");
            fw.WriteReturns(1, $"Une instance de '{mapper.Class.NamePascal}'");

            fw.WriteDocEnd(1);
            var (mapperNs, mapperModelPath) = Config.GetMapperLocation(toMapper);

            fw.WriteLine(1, $"public {mapper.Class.NamePascal} {mapper.Name.Value.ToCamelCase()}({mapper.Class.NamePascal} target) {{");
            fw.WriteLine(2, $"return {Config.GetMapperName(mapperNs, mapperModelPath)}.{mapper.Name.Value.ToCamelCase()}(this, target);");
            fw.AddImport(Config.GetMapperImport(mapperNs, mapperModelPath, tag)!);
            fw.WriteLine(1, "}");

            if (toMappers.IndexOf(toMapper) < toMappers.Count - 1)
            {
                fw.WriteLine();
            }
        }
    }
}