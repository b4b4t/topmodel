﻿using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.Model.Implementation;
using TopModel.Generator.Core;

namespace TopModel.Generator.Jpa;

/// <summary>
/// Générateur de fichiers de modèles JPA.
/// </summary>
public class JdbcEntityGenerator : JavaClassGeneratorBase
{
    private readonly ILogger<JdbcEntityGenerator> _logger;

    private JavaEnumConstructorGenerator? _javaEnumConstructorGenerator;

    public JdbcEntityGenerator(ILogger<JdbcEntityGenerator> logger)
        : base(logger)
    {
        _logger = logger;
    }

    public override string Name => "JdbcEntityGen";

    protected override JavaEnumConstructorGenerator ConstructorGenerator
    {
        get
        {
            _javaEnumConstructorGenerator ??= new JavaEnumConstructorGenerator(Config);
            return _javaEnumConstructorGenerator;
        }
    }

    protected override bool FilterClass(Class classe)
    {
        return !classe.Abstract && classe.IsPersistent;
    }

    protected override string GetFileName(Class classe, string tag)
    {
        return Config.GetClassFileName(classe, tag);
    }

    protected override void HandleClass(string fileName, Class classe, string tag)
    {
        var packageName = Config.GetPackageName(classe, tag);
        using var fw = new JavaWriter(fileName, _logger, packageName, null);

        fw.WriteLine();

        WriteAnnotations(fw, classe, tag);

        var extends = Config.GetClassExtends(classe);
        if (classe.Extends is not null)
        {
            fw.AddImport($"{Config.GetPackageName(classe.Extends, tag)}.{classe.Extends.NamePascal}");
        }

        var implements = Config.GetClassImplements(classe).ToList();

        if (!classe.IsPersistent)
        {
            implements.Add("Serializable");
            fw.AddImport("java.io.Serializable");
        }

        fw.WriteClassDeclaration(classe.NamePascal, null, extends, implements);

        if (!classe.IsPersistent)
        {
            fw.WriteLine("	/** Serial ID */");
            fw.WriteLine(1, "private static final long serialVersionUID = 1L;");
        }

        if (Config.CanClassUseEnums(classe, Classes))
        {
            fw.WriteLine();
            var codeProperty = classe.EnumKey!;
            foreach (var refValue in classe.Values.OrderBy(x => x.Name, StringComparer.Ordinal))
            {
                var code = refValue.Value[codeProperty];
                if (classe.IsPersistent)
                {
                    fw.AddImport($"{JavaxOrJakarta}.persistence.Transient");
                    fw.WriteLine(1, "@Transient");
                }

                fw.WriteLine(1, $@"public static final {classe.NamePascal} {code} = new {classe.NamePascal}({Config.GetEnumName(codeProperty, classe)}.{code});");
            }
        }

        JpaModelPropertyGenerator.WriteProperties(fw, classe, tag);

        if (Config.CanClassUseEnums(classe, Classes)
            || Config.MappersInClass && classe.FromMappers.Any(c => c.ClassParams.All(p => Classes.Contains(p.Class)))
            || classe.Extends != null
            || Classes.Any(c => c.Extends == classe)
            || classe.Decorators.Any(d => Config.GetImplementation(d.Decorator)?.Extends is not null))
        {
            ConstructorGenerator.WriteNoArgConstructor(fw, classe);
        }

        if (Config.MappersInClass)
        {
            ConstructorGenerator.WriteFromMappers(fw, classe, Classes, tag);
        }

        if (Config.CanClassUseEnums(classe, Classes))
        {
            ConstructorGenerator.WriteEnumConstructor(fw, classe, Classes, tag);
        }

        WriteGetters(fw, classe, tag);
        WriteSetters(fw, classe, tag);

        if (Config.MappersInClass)
        {
            WriteToMappers(fw, classe, tag);
        }

        if ((Config.FieldsEnum & Target.Persisted) > 0 && classe.IsPersistent
            || (Config.FieldsEnum & Target.Dto) > 0 && !classe.IsPersistent)
        {
            WriteFieldsEnum(fw, classe, tag);
        }

        fw.WriteLine("}");
    }

    protected override void WriteAnnotations(JavaWriter fw, Class classe, string tag)
    {
        base.WriteAnnotations(fw, classe, tag);
        var table = @$"@Table(name = ""{classe.SqlName.ToLower()}"")";
        fw.AddImport($"org.springframework.data.relational.core.mapping.Table");
        fw.WriteLine(table);
    }

    protected override void WriteGetters(JavaWriter fw, Class classe, string tag)
    {
        var properties = Config.UseJdbc ? classe.Properties.Where(p => !(p is AssociationProperty ap && (ap.Type == AssociationType.OneToMany || ap.Type == AssociationType.ManyToMany))) : classe.GetProperties(Classes);
        foreach (var property in properties)
        {
            JpaModelPropertyGenerator!.WriteGetter(fw, tag, property);
        }
    }

    protected override void WriteSetters(JavaWriter fw, Class classe, string tag)
    {
        var properties = Config.UseJdbc ? classe.Properties.Where(p => !(p is AssociationProperty ap && (ap.Type == AssociationType.OneToMany || ap.Type == AssociationType.ManyToMany))) : classe.GetProperties(Classes);
        if (Config.CanClassUseEnums(classe, Classes))
        {
            return;
        }

        foreach (var property in properties)
        {
            JpaModelPropertyGenerator!.WriteSetter(fw, tag, property);
        }
    }
}