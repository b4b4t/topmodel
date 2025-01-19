﻿using TopModel.Core;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Jpa;

/// <summary>
/// Générateur de fichiers de modèles JPA.
/// </summary>
public class JpaModelPropertyGenerator
{
    private readonly IEnumerable<Class> _classes;
    private readonly JpaConfig _config;
    private readonly Dictionary<string, string> _newableTypes;

    public JpaModelPropertyGenerator(JpaConfig config, IEnumerable<Class> classes, Dictionary<string, string> newableTypes)
    {
        _classes = classes;
        _config = config;
        _newableTypes = newableTypes;
    }

    protected string JavaxOrJakarta => _config.JavaxOrJakarta;

    protected JavaAnnotation NotNullAnnotation => new JavaAnnotation("NotNull", $"{JavaxOrJakarta}.validation.constraints.NotNull");

    private JavaAnnotation EnumAnnotation =>
        new JavaAnnotation("Enumerated", $"{JavaxOrJakarta}.persistence.Enumerated")
            .AddAttribute("value", "EnumType.STRING", $"{JavaxOrJakarta}.persistence.EnumType");

    public virtual JavaAnnotation GetColumnAnnotation(IProperty property)
    {
        JavaAnnotation column = new JavaAnnotation("Column", $"{JavaxOrJakarta}.persistence.Column");
        column.AddAttribute("name", $@"""{property.SqlName}""");
        if (property.Required)
        {
            column.AddAttribute("nullable", "false");
        }

        if (property.Domain != null)
        {
            if (property.Domain.Length != null)
            {
                if (_config.GetImplementation(property.Domain)?.Type?.ToUpper() == "STRING")
                {
                    column.AddAttribute("length", $"{property.Domain.Length}");
                }
                else
                {
                    column.AddAttribute("precision", $"{property.Domain.Length}");
                }
            }

            if (property.Domain.Scale != null)
            {
                column.AddAttribute("scale", $"{property.Domain.Scale}");
            }

            column.AddAttribute("columnDefinition", @$"""{property.Domain.Implementations["sql"].Type}""");
        }

        if (property is CompositionProperty && property.Domain is null)
        {
            column.AddAttribute("columnDefinition", @$"""jsonb""");
        }

        return column;
    }

    public IEnumerable<JavaAnnotation> GetDomainAnnotations(IProperty property, string tag)
    {
        foreach (var annotation in _config.GetDomainAnnotationsAndImports(property, tag))
        {
            yield return new JavaAnnotation(annotation.Annotation, annotation.Imports);
        }
    }

    public string GetGetterName(IProperty property)
    {
        var propertyName = GetPropertyName(property);
        var propertyType = GetPropertyType(property);
        var getterPrefix = propertyType == "boolean" ? "is" : "get";
        if (property.Class.PreservePropertyCasing)
        {
            return propertyName.ToFirstUpper().WithPrefix(getterPrefix);
        }

        return propertyName.ToPascalCase().WithPrefix(getterPrefix);
    }

    public IEnumerable<JavaAnnotation> GetJpaAssociationAnnotations(AssociationProperty property, string tag)
    {
        return property.Type switch
        {
            AssociationType.ManyToOne => GetManyToOneAnnotations(property, tag),
            AssociationType.OneToMany => GetOneToManyAnnotations(property),
            AssociationType.ManyToMany => GetManyToManyAnnotations(property),
            AssociationType.OneToOne => GetOneToOneAnnotations(property),
            _ => [],
        };
    }

    public virtual string GetPropertyName(IProperty property)
    {
        var isAssociationNotPersistent = property is AssociationProperty apr && !apr.Association.IsPersistent;
        return isAssociationNotPersistent ? property.NameCamel : property.NameByClassCamel;
    }

    public virtual string GetPropertyType(IProperty property)
    {
        var isAssociationNotPersistent = property is AssociationProperty apr && !apr.Association.IsPersistent;
        var useClassForAssociation = property.Class.IsPersistent && !isAssociationNotPersistent;
        return _config.GetType(property, _classes, useClassForAssociation);
    }

    public string GetSetterName(IProperty property)
    {
        var propertyName = GetPropertyName(property);
        if (property.Class.PreservePropertyCasing)
        {
            return propertyName.WithPrefix("set");
        }

        return propertyName.ToPascalCase().WithPrefix("set");
    }

    public void WriteGetter(JavaWriter fw, string tag, IProperty property, int indentLevel = 1)
    {
        var propertyName = GetPropertyName(property);
        var propertyType = GetPropertyType(property);
        fw.WriteLine();
        fw.WriteDocStart(indentLevel, $"Getter for {propertyName}");
        fw.WriteReturns(indentLevel, $"value of {{@link {property.Class.GetImport(_config, tag)}#{propertyName} {propertyName}}}");
        fw.WriteDocEnd(indentLevel);
        string getterName = GetGetterName(property);

        fw.WriteLine(indentLevel, @$"public {propertyType} {getterName}() {{");
        var genericType = propertyType.Split('<').First();
        if (_newableTypes.TryGetValue(genericType, out var newableType) && property.Class.IsPersistent)
        {
            fw.WriteLine(indentLevel + 1, $"if(this.{propertyName} == null) {{");
            fw.AddImport($"java.util.{newableType}");
            fw.WriteLine(indentLevel + 2, $"this.{propertyName} = new {newableType}<>();");
            fw.WriteLine(indentLevel + 1, $"}}");
        }

        fw.WriteLine(indentLevel + 1, @$"return this.{propertyName};");
        fw.WriteLine(indentLevel, "}");
    }

    public virtual void WriteProperties(JavaWriter fw, Class classe, string tag)
    {
        foreach (var property in classe.GetProperties(_classes))
        {
            WriteProperty(fw, property, tag);
        }
    }

    public void WriteProperty(JavaWriter fw, IProperty property, string tag)
    {
        fw.WriteLine();
        fw.WriteDocStart(1, property.Comment);
        IEnumerable<JavaAnnotation> annotations = GetAnnotations(property, tag);
        if (property is AliasProperty ap && _classes.Contains(ap.Property.Class))
        {
            fw.WriteLine(1, $" * Alias of {{@link {ap.Property.Class.GetImport(_config, tag)}#get{GetPropertyName(ap.Property).ToFirstUpper()}() {ap.Property.Class.NamePascal}#get{GetPropertyName(ap.Property).ToFirstUpper()}()}} ");
        }

        fw.WriteDocEnd(1);

        if (!property.PrimaryKey || property.Class.PrimaryKey.Count() <= 1)
        {
            annotations = GetDomainAnnotations(property, tag).Concat(annotations).ToList();
        }

        fw.WriteAnnotations(1, annotations);
        string defaultValue = GetDefaultValue(property);
        fw.AddImports(GetDefaultValueImports(property, tag));
        fw.AddImports(property.GetTypeImports(_config, tag));
        fw.WriteLine(1, $"private {GetPropertyType(property)} {GetPropertyName(property)}{defaultValue};");
    }

    public void WriteSetter(JavaWriter fw, string tag, IProperty property, int indentLevel = 1)
    {
        var propertyName = GetPropertyName(property);
        fw.WriteLine();
        fw.WriteDocStart(indentLevel, $"Set the value of {{@link {property.Class.GetImport(_config, tag)}#{propertyName} {propertyName}}}");
        fw.WriteLine(indentLevel, $" * @param {propertyName} value to set");
        fw.WriteDocEnd(indentLevel);
        fw.WriteLine(indentLevel, @$"public void {GetSetterName(property)}({GetPropertyType(property)} {propertyName}) {{");
        fw.WriteLine(indentLevel + 1, @$"this.{propertyName} = {propertyName};");
        fw.WriteLine(indentLevel, "}");
    }

    protected virtual IEnumerable<JavaAnnotation> GetAnnotations(CompositionProperty property, string tag)
    {
        if (property.Class.IsPersistent)
        {
            yield return GetConvertAnnotation(property, tag);
            yield return GetColumnAnnotation(property);
        }

        foreach (var a in GetDomainAnnotations(property, tag))
        {
            yield return a;
        }
    }

    protected virtual IEnumerable<JavaAnnotation> GetAnnotations(AliasProperty property, string tag)
    {
        var shouldWriteAssociation = property.Class.IsPersistent && property.Property is AssociationProperty;
        if (property.PrimaryKey && property.Class.IsPersistent)
        {
            foreach (var a in GetIdAnnotations(property))
            {
                yield return a;
            }
        }

        if (shouldWriteAssociation)
        {
            foreach (var a in GetJpaAssociationAnnotations((AssociationProperty)property.Property, tag))
            {
                yield return a;
            }
        }
        else if (property.Class.IsPersistent && !(property.PrimaryKey && property.Class.PrimaryKey.Count() > 1))
        {
            yield return GetColumnAnnotation(property);
        }

        if (property.Property is CompositionProperty cp)
        {
            GetAnnotations(cp, tag);
        }

        if (property.Required && !property.PrimaryKey && !property.Class.IsPersistent)
        {
            yield return NotNullAnnotation;
        }

        if (_config.CanClassUseEnums(property.Property.Class, _classes, property.Property) && property.Class.IsPersistent)
        {
            yield return EnumAnnotation;
        }
    }

    private IEnumerable<JavaAnnotation> GetAnnotations(IProperty property, string tag)
    {
        return property switch
        {
            AliasProperty alp => GetAnnotations(alp, tag),
            AssociationProperty ap => GetAnnotations(ap, tag),
            CompositionProperty cp => GetAnnotations(cp, tag),
            IProperty ip => GetAnnotations(ip),
            _ => [],
        };
    }

    protected virtual IEnumerable<JavaAnnotation> GetAnnotations(AssociationProperty property, string tag)
    {
        if (property.Class.IsPersistent)
        {
            if (!property.PrimaryKey || property.Class.PrimaryKey.Count() <= 1)
            {
                foreach (var a in GetJpaAssociationAnnotations(property, tag))
                {
                    yield return a;
                }
            }

            if (property.Type == AssociationType.ManyToMany || property.Type == AssociationType.OneToMany)
            {
                if (property.Association.OrderProperty != null && GetPropertyType(property).Contains("List"))
                {
                    yield return new JavaAnnotation("OrderBy", $@"""{property.Association.OrderProperty.NameByClassCamel} ASC""", $"{JavaxOrJakarta}.persistence.OrderBy");
                }
            }

            if (property.PrimaryKey)
            {
                foreach (var a in GetIdAnnotations(property))
                {
                    yield return a;
                }
            }
        }
    }

    protected virtual IEnumerable<JavaAnnotation> GetAnnotations(IProperty property)
    {
        if (property.Class.IsPersistent)
        {
            if (property.PrimaryKey)
            {
                foreach (var a in GetIdAnnotations(property))
                {
                    yield return a;
                }
            }

            if (!(property.PrimaryKey && property.Class.PrimaryKey.Count() > 1))
            {
                yield return GetColumnAnnotation(property);
            }

            if (_config.CanClassUseEnums(property.Class, _classes, property))
            {
                yield return EnumAnnotation;
            }
        }
        else if (property.Required && !property.PrimaryKey)
        {
            yield return NotNullAnnotation;
        }
    }

    private IEnumerable<JavaAnnotation> GetAutogeneratedAnnotations(Class classe)
    {
        var autoGenerated = new JavaAnnotation("GeneratedValue", $"{JavaxOrJakarta}.persistence.GeneratedValue");
        if (_config.Identity.Mode == IdentityMode.IDENTITY)
        {
            autoGenerated.AddAttribute("strategy", "GenerationType.IDENTITY", $"{JavaxOrJakarta}.persistence.GenerationType");
        }
        else if (_config.Identity.Mode == IdentityMode.SEQUENCE)
        {
            autoGenerated.AddAttribute("strategy", "GenerationType.SEQUENCE", $"{JavaxOrJakarta}.persistence.GenerationType");
            var sequenceGenerator = new JavaAnnotation("SequenceGenerator", $"{JavaxOrJakarta}.persistence.SequenceGenerator");
            var seqName = $"SEQ_{classe.SqlName}";
            sequenceGenerator.AddAttribute("sequenceName", $@"""{seqName}""");
            if (_config.Identity.Start != null)
            {
                sequenceGenerator.AddAttribute("initialValue", $"{_config.Identity.Start}");
            }

            if (_config.Identity.Increment != null)
            {
                sequenceGenerator.AddAttribute("allocationSize", $"{_config.Identity.Increment}");
            }

            yield return sequenceGenerator;
        }

        yield return autoGenerated;
    }

    private JavaAnnotation GetConvertAnnotation(CompositionProperty property, string tag)
    {
        var convert = new JavaAnnotation("Convert", $"{JavaxOrJakarta}.persistence.Convert");
        var import = _config.CompositionConverterCanonicalName
            .Replace("{class}", property.Composition.Name)
            .Replace("{package}", _config.GetPackageName(property.Composition, _config.GetBestClassTag(property.Composition, tag)));
        convert.AddAttribute("converter", $"{_config.CompositionConverterSimpleName.Replace("{class}", property.Composition.Name)}.class", import);
        return convert;
    }

    private string GetDefaultValue(IProperty property)
    {
        if (property is AssociationProperty ap)
        {
            if (ap.Association.PrimaryKey.Count() == 1 && _config.CanClassUseEnums(ap.Association, _classes, prop: ap.Association.PrimaryKey.Single()))
            {
                var defaultValue = _config.GetValue(property, _classes);
                if (defaultValue != "null")
                {
                    return $" = new {ap.Association.NamePascal}({defaultValue})";
                }
            }

            return string.Empty;
        }
        else
        {
            var defaultValue = _config.GetValue(property, _classes);
            var suffix = defaultValue != "null" ? $" = {defaultValue}" : string.Empty;
            return suffix;
        }
    }

    private IEnumerable<string> GetDefaultValueImports(IProperty property, string tag)
    {
        if (property is AssociationProperty ap)
        {
            if (ap.Association.PrimaryKey.Count() == 1 && _config.CanClassUseEnums(ap.Association, _classes, prop: ap.Association.PrimaryKey.Single()))
            {
                var defaultValue = _config.GetValue(property, _classes);
                if (defaultValue != "null")
                {
                    return [$"{_config.GetEnumPackageName(property.Class, _config.GetBestClassTag(property.Class, tag))}.{GetPropertyType(ap.Association.PrimaryKey.Single())}"];
                }
            }

            return [];
        }
        else
        {
            return _config.GetValueImports(property, tag);
        }
    }

    private IEnumerable<JavaAnnotation> GetIdAnnotations(IProperty property)
    {
        if (property.Domain.AutoGeneratedValue && property.Class.PrimaryKey.Count() == 1)
        {
            foreach (var a in GetAutogeneratedAnnotations(property.Class))
            {
                yield return a;
            }
        }

        yield return new JavaAnnotation("Id", $"{JavaxOrJakarta}.persistence.Id");
    }

    private IEnumerable<JavaAnnotation> GetManyToManyAnnotations(AssociationProperty property)
    {
        var role = property.Role is not null ? "_" + property.Role.ToConstantCase() : string.Empty;
        var fk = ((IProperty)property).SqlName;
        var pk = property.Class.PrimaryKey.Single().SqlName + role;
        var association = new JavaAnnotation($"{property.Type}", $"{JavaxOrJakarta}.persistence.{property.Type}")
            .AddAttribute("fetch", "FetchType.LAZY", $"{JavaxOrJakarta}.persistence.FetchType");
        if (!_config.CanClassUseEnums(property.Association))
        {
            association.AddAttribute("cascade", "{ CascadeType.PERSIST, CascadeType.MERGE }", $"{JavaxOrJakarta}.persistence.CascadeType");
        }

        if (property is ReverseAssociationProperty rap)
        {
            association.AddAttribute("mappedBy", $@"""{rap.ReverseProperty.NameByClassCamel}""");
        }

        yield return association;

        if (property is not ReverseAssociationProperty)
        {
            var joinColumns = new JavaAnnotation("JoinColumn", $"{JavaxOrJakarta}.persistence.JoinColumn").AddAttribute("name", $@"""{pk}""");
            var inverseJoinColumns = new JavaAnnotation("JoinColumn", $"{JavaxOrJakarta}.persistence.JoinColumn").AddAttribute("name", $@"""{fk}""");
            var joinTable = new JavaAnnotation("JoinTable", $"{JavaxOrJakarta}.persistence.JoinTable")
                .AddAttribute("name", $@"""{property.Class.SqlName}_{property.Association.SqlName}{(property.Role != null ? "_" + property.Role.ToConstantCase() : string.Empty)}""")
                .AddAttribute("joinColumns", joinColumns)
                .AddAttribute("inverseJoinColumns", inverseJoinColumns);
            yield return joinTable;
        }
    }

    private IEnumerable<JavaAnnotation> GetManyToOneAnnotations(AssociationProperty property, string tag)
    {
        var association = new JavaAnnotation(@$"{property.Type}", $"{JavaxOrJakarta}.persistence.{property.Type}")
            .AddAttribute("fetch", "FetchType.LAZY", $"{JavaxOrJakarta}.persistence.FetchType")
            .AddAttribute("optional", property.Required ? "false" : "true")
            .AddAttribute("targetEntity", $"{property.Association.NamePascal}.class", property.Association.GetImport(_config, _config.GetBestClassTag(property.Association, tag)));
        yield return association;

        var fk = ((IProperty)property).SqlName;
        var apk = property.Property.SqlName;
        var joinColumn = new JavaAnnotation("JoinColumn", $"{JavaxOrJakarta}.persistence.JoinColumn")
            .AddAttribute("name", $@"""{fk}""")
            .AddAttribute("referencedColumnName", $@"""{apk}""");
        yield return joinColumn;
    }

    private IEnumerable<JavaAnnotation> GetOneToManyAnnotations(AssociationProperty property)
    {
        var association = new JavaAnnotation(@$"{property.Type}", $"{JavaxOrJakarta}.persistence.{property.Type}");
        if (property is ReverseAssociationProperty rap)
        {
            association
                .AddAttribute("cascade", "{CascadeType.PERSIST, CascadeType.MERGE}", $"{JavaxOrJakarta}.persistence.CascadeType")
                .AddAttribute("fetch", "FetchType.LAZY", $"{JavaxOrJakarta}.persistence.FetchType")
                .AddAttribute("mappedBy", $@"""{rap.ReverseProperty.NameByClassCamel}""");
        }
        else
        {
            var pk = property.Class.PrimaryKey.Single().SqlName;
            var hasReverse = property.Class.Namespace.RootModule == property.Association.Namespace.RootModule;

            association
                .AddAttribute("cascade", "CascadeType.ALL", $"{JavaxOrJakarta}.persistence.CascadeType")
                .AddAttribute("fetch", "FetchType.LAZY", $"{JavaxOrJakarta}.persistence.FetchType");
            if (hasReverse)
            {
                association.AddAttribute("mappedBy", @$"""{property.Class.NameCamel}{property.Role ?? string.Empty}""");
            }
            else
            {
                var joinColumn = new JavaAnnotation("JoinColumn", $"{JavaxOrJakarta}.persistence.JoinColumn")
                    .AddAttribute("name", $@"""{pk}""")
                    .AddAttribute("referencedColumnName", $@"""{pk}""");
                yield return joinColumn;
            }
        }

        yield return association;
    }

    private IEnumerable<JavaAnnotation> GetOneToOneAnnotations(AssociationProperty property)
    {
        var fk = ((IProperty)property).SqlName;
        var apk = property.Property.SqlName;
        var association = new JavaAnnotation(@$"{property.Type}", $"{JavaxOrJakarta}.persistence.{property.Type}")
                .AddAttribute("fetch", "FetchType.LAZY", $"{JavaxOrJakarta}.persistence.FetchType")
                .AddAttribute("cascade", @"CascadeType.ALL", $"{JavaxOrJakarta}.persistence.CascadeType")
                .AddAttribute("optional", (!property.Required).ToString().ToLower());
        yield return association;

        var joinColumn = new JavaAnnotation("JoinColumn", $"{JavaxOrJakarta}.persistence.JoinColumn")
            .AddAttribute("name", $@"""{fk}""")
            .AddAttribute("referencedColumnName", $@"""{apk}""")
            .AddAttribute("unique", "true");
        yield return joinColumn;
    }
}
