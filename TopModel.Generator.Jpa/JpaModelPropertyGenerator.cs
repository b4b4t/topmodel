using TopModel.Core;
using TopModel.Core.Model.Implementation;
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

    private string JavaxOrJakarta => _config.PersistenceMode.ToString().ToLower();

    public JpaModelPropertyGenerator(JpaConfig config, IEnumerable<Class> classes, Dictionary<string, string> newableTypes)
    {
        _classes = classes;
        _config = config;
        _newableTypes = newableTypes;
    }

    public void WriteCompositePrimaryKeyClass(JavaWriter fw, Class classe, string tag)
    {
        if (classe.PrimaryKey.Count() <= 1 || !classe.IsPersistent)
        {
            return;
        }

        fw.WriteLine();
        fw.WriteLine(1, @$"public static class {classe.NamePascal}Id {{");
        foreach (var pk in classe.PrimaryKey)
        {
            fw.WriteLine();
            var annotations = new List<JavaAnnotation>();
            annotations.AddRange(GetDomainAnnotations(pk, tag));
            if (pk is AssociationProperty ap)
            {
                annotations.AddRange(GetJpaAssociationAnnotations(ap, tag));
            }
            else if (ShouldWriteColumnAnnotation(pk))
            {
                annotations.Add(GetColumnAnnotation(pk));
            }

            fw.WriteAnnotations(2, annotations);
            fw.WriteLine(2, $"private {GetPropertyType(pk)} {GetPropertyName(pk)};");
        }

        foreach (var pk in classe.PrimaryKey)
        {
            WriteGetter(fw, tag, pk, 2);
            WriteSetter(fw, tag, pk, 2);
        }

        fw.WriteLine();
        fw.WriteLine(2, "public boolean equals(Object o) {");
        fw.WriteLine(3, "if(o == this) {");
        fw.WriteLine(4, "return true;");
        fw.WriteLine(3, "}");
        fw.WriteLine();
        fw.WriteLine(3, "if(o == null) {");
        fw.WriteLine(4, "return false;");
        fw.WriteLine(3, "}");
        fw.WriteLine();
        fw.WriteLine(3, "if(this.getClass() != o.getClass()) {");
        fw.WriteLine(4, "return false;");
        fw.WriteLine(3, "}");
        fw.WriteLine();
        fw.WriteLine(3, $"{classe.NamePascal}Id oId = ({classe.NamePascal}Id) o;");
        var associations = classe.PrimaryKey.Where(p => p is AssociationProperty || p is AliasProperty ap && ap.Property is AssociationProperty);
        if (associations.Any())
        {
            fw.WriteLine();
            fw.WriteLine(3, @$"if({string.Join(" || ", associations.Select(pk => pk.NameByClassCamel).Select(pk => $"this.{pk} == null || oId.{pk} == null"))}) {{");
            fw.WriteLine(4, "return false;");
            fw.WriteLine(3, "}");
        }

        fw.WriteLine();
        fw.WriteLine(3, $@"return {string.Join("\n && ", classe.PrimaryKey.Select(pk => $@"Objects.equals(this.{pk.NameByClassCamel}{GetterToCompareCompositePkPk(pk)}, oId.{pk.NameByClassCamel}{GetterToCompareCompositePkPk(pk)})"))};");
        fw.WriteLine(2, "}");

        fw.WriteLine();
        fw.WriteLine(2, "@Override");
        fw.WriteLine(2, "public int hashCode() {");
        fw.WriteLine(3, $"return Objects.hash({string.Join(", ", classe.PrimaryKey.Select(pk => $"{(pk is AssociationProperty || pk is AliasProperty ap && ap.Property is AssociationProperty ? $"{pk.NameByClassCamel} == null ? null : " : string.Empty)}{pk.NameByClassCamel}{GetterToCompareCompositePkPk(pk)}"))});");
        fw.AddImport("java.util.Objects");
        fw.WriteLine(2, "}");
        fw.WriteLine(1, "}");
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
        if (property is AssociationProperty ap && ap.Type.IsToMany())
        {
            var type = propertyType.Split('<').First();
            if (_newableTypes.TryGetValue(type, out var newableType))
            {
                fw.WriteLine(indentLevel + 1, $"if(this.{propertyName} == null) {{");
                fw.AddImport($"java.util.{newableType}");
                fw.WriteLine(indentLevel + 2, $"this.{propertyName} = new {newableType}<>();");
                fw.WriteLine(indentLevel + 1, $"}}");
            }
        }

        fw.WriteLine(indentLevel + 1, @$"return this.{propertyName};");
        fw.WriteLine(indentLevel, "}");
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

    public void WriteProperties(JavaWriter fw, Class classe, string tag)
    {
        var properties = _config.UseJdbc ? classe.Properties.Where(p => !(p is AssociationProperty ap && (ap.Type == AssociationType.OneToMany || ap.Type == AssociationType.ManyToMany))) : classe.GetProperties(_classes);
        foreach (var property in properties)
        {
            WriteProperty(fw, property, tag);
        }
    }

    public void WriteProperty(JavaWriter fw, IProperty property, string tag)
    {
        fw.WriteLine();
        fw.AddImports(property.GetTypeImports(_config, tag));
        fw.WriteDocStart(1, property.Comment);
        IEnumerable<JavaAnnotation> annotations = GetAnnotations(property, tag);
        if (property is AliasProperty ap && _classes.Contains(ap.Property.Class))
        {
            fw.WriteLine(1, $" * Alias of {{@link {ap.Property.Class.GetImport(_config, tag)}#get{GetPropertyName(ap.Property).ToFirstUpper()}() {ap.Property.Class.NamePascal}#get{GetPropertyName(ap.Property).ToFirstUpper()}()}} ");
        }

        fw.WriteDocEnd(1);

        if (property.Domain != null && !property.PrimaryKey || property.Class.PrimaryKey.Count() <= 1)
        {
            annotations = GetDomainAnnotations(property, tag).Concat(annotations).ToList();
        }

        fw.WriteAnnotations(1, annotations);
        string defaultValue = GetDefaultValue(property);
        fw.AddImports(GetDefaultValueImports(property, tag));
        fw.WriteLine(1, $"private {GetPropertyType(property)} {GetPropertyName(property)}{defaultValue};");
    }

    public void WriteSetter(JavaWriter fw, string tag, IProperty property, int indentLevel = 1)
    {
        var propertyName = GetPropertyName(property);
        fw.WriteLine();
        fw.WriteDocStart(indentLevel, $"Set the value of {{@link {property.Class.GetImport(_config, tag)}#{propertyName} {propertyName}}}");
        fw.WriteLine(indentLevel, $" * @param {propertyName} value to set");
        fw.WriteDocEnd(indentLevel);
        fw.WriteLine(indentLevel, @$"public void {propertyName.WithPrefix("set")}({GetPropertyType(property)} {propertyName}) {{");
        fw.WriteLine(indentLevel + 1, @$"this.{propertyName} = {propertyName};");
        fw.WriteLine(indentLevel, "}");
    }

    protected IEnumerable<JavaAnnotation> GetAnnotations(CompositionProperty property, string tag)
    {
        if (property.Class.IsPersistent)
        {
            return [GetConvertAnnotation(property, tag), GetColumnAnnotation(property)];
        }

        return [];
    }

    protected string GetPropertyName(IProperty property)
    {
        var isAssociationNotPersistent = property is AssociationProperty apr && !apr.Association.IsPersistent;
        return _config.UseJdbc || isAssociationNotPersistent ? property.NameCamel : property.NameByClassCamel;
    }

    protected string GetPropertyType(IProperty property)
    {
        var isAssociationNotPersistent = property is AssociationProperty apr && !apr.Association.IsPersistent;
        var useClassForAssociation = property.Class.IsPersistent && !isAssociationNotPersistent && !_config.UseJdbc;
        return _config.GetType(property, useClassForAssociation: useClassForAssociation);
    }

    private JavaAnnotation GetEnumAnnotation()
    {
        return new JavaAnnotation("Enumerated", $"{JavaxOrJakarta}.persistence.Enumerated")
            .AddAttribute("value", "EnumType.STRING", $"{JavaxOrJakarta}.persistence.EnumType");
    }

    private string GetterToCompareCompositePkPk(IProperty pk)
    {
        if (pk is AssociationProperty ap)
        {
            return $".{GetGetterName(ap.Property)}()";
        }
        else if (pk is AliasProperty al && al.Property is AssociationProperty asp)
        {
            return $".get{GetGetterName(asp.Property)}()";
        }

        return string.Empty;
    }

    private bool ShouldWriteColumnAnnotation(IProperty property)
    {
        return property.Class.IsPersistent || _config.UseJdbc;
    }

    private IEnumerable<JavaAnnotation> GetAnnotations(AliasProperty property, string tag)
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
        else if (ShouldWriteColumnAnnotation(property) && (_config.UseJdbc || !(property.PrimaryKey && property.Class.PrimaryKey.Count() > 1)))
        {
            yield return GetColumnAnnotation(property);
        }

        if (property.Property is CompositionProperty cp)
        {
            GetAnnotations(cp, tag);
        }

        if (property.Required && !property.PrimaryKey && (!property.Class.IsPersistent || _config.UseJdbc))
        {
            yield return new JavaAnnotation("NotNull", $"{JavaxOrJakarta}.validation.constraints.NotNull");
        }

        if (_config.CanClassUseEnums(property.Property.Class) && property.Property.PrimaryKey && property.Class.IsPersistent && !_config.UseJdbc)
        {
            yield return GetEnumAnnotation();
        }
    }

    private IEnumerable<JavaAnnotation> GetJpaAssociationAnnotations(AssociationProperty property, string tag)
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

    private IEnumerable<JavaAnnotation> GetAnnotations(AssociationProperty property, string tag)
    {
        if (property.Class.IsPersistent)
        {
            if (!_config.UseJdbc)
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
            else
            {
                if (property.PrimaryKey && property.Class.PrimaryKey.Count() <= 1)
                {
                    foreach (var a in GetIdAnnotations(property))
                    {
                        yield return a;
                    }
                }

                yield return new JavaAnnotation("Column", @$"""{((IProperty)property).SqlName.ToLower()}""", "org.springframework.data.relational.core.mapping.Column");
            }
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

    private JavaAnnotation GetColumnAnnotation(IProperty property)
    {
        JavaAnnotation column;
        if (!_config.UseJdbc)
        {
            column = new JavaAnnotation("Column", $"{JavaxOrJakarta}.persistence.Column");
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
        }
        else
        {
            column = new JavaAnnotation("Column", "org.springframework.data.relational.core.mapping.Column");
            column.AddAttribute("value", $@"""{property.SqlName.ToLower()}""");
        }

        return column;
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

    private IEnumerable<JavaAnnotation> GetDomainAnnotations(IProperty property, string tag)
    {
        foreach (var annotation in _config.GetDomainAnnotations(property, tag))
        {
            yield return new JavaAnnotation(annotation.Annotation, annotation.Imports);
        }
    }

    private string GetDefaultValue(IProperty property)
    {
        if (property is AssociationProperty ap)
        {
            if (!_config.UseJdbc && ap.Association.PrimaryKey.Count() == 1 && _config.CanClassUseEnums(ap.Association, _classes, prop: ap.Association.PrimaryKey.Single()))
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
            if (!_config.UseJdbc && ap.Association.PrimaryKey.Count() == 1 && _config.CanClassUseEnums(ap.Association, _classes, prop: ap.Association.PrimaryKey.Single()))
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

    private IEnumerable<JavaAnnotation> GetAnnotations(IProperty property)
    {
        if (property.PrimaryKey && property.Class.IsPersistent)
        {
            foreach (var a in GetIdAnnotations(property))
            {
                yield return a;
            }
        }

        if (ShouldWriteColumnAnnotation(property) && (_config.UseJdbc || !(property.PrimaryKey && property.Class.PrimaryKey.Count() > 1)))
        {
            yield return GetColumnAnnotation(property);
        }

        if (property.Required && !property.PrimaryKey && (!property.Class.IsPersistent || _config.UseJdbc))
        {
            yield return new JavaAnnotation("NotNull", $"{JavaxOrJakarta}.validation.constraints.NotNull");
        }

        if (_config.CanClassUseEnums(property.Class) && property.PrimaryKey && !_config.UseJdbc)
        {
            yield return GetEnumAnnotation();
        }
    }

    private IEnumerable<JavaAnnotation> GetIdAnnotations(IProperty property)
    {
        string idImport;
        if (!_config.UseJdbc)
        {
            idImport = $"{JavaxOrJakarta}.persistence.Id";

            if (property.Domain.AutoGeneratedValue && property.Class.PrimaryKey.Count() == 1)
            {
                foreach (var a in GetAutogeneratedAnnotations(property.Class))
                {
                    yield return a;
                }
            }
        }
        else
        {
            idImport = "org.springframework.data.annotation.Id";
        }

        yield return new JavaAnnotation("Id", idImport);
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
