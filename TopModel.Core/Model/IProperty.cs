﻿using TopModel.Utils;

namespace TopModel.Core;

public interface IProperty
{
    string Name { get; }

    string NamePascal { get; }

    string NameCamel { get; }

    string NameByClassPascal { get; }

    string NameByClassCamel { get; }

    string? Label { get; }

    bool PrimaryKey { get; }

    bool Required { get; }

    Domain Domain { get; }

    string[] DomainParameters { get; }

    string Comment { get; }

    string? DefaultValue { get; }

    bool Readonly { get; set; }

    LocatedString? Trigram { get; set; }

    Dictionary<string, string> CustomProperties { get; }

    Class Class { get; set; }

    Endpoint Endpoint { get; set; }

    Decorator Decorator { get; set; }

    PropertyMapping PropertyMapping { get; set; }

    IPropertyContainer Parent => Class ?? (IPropertyContainer)Endpoint ?? (IPropertyContainer)Decorator ?? PropertyMapping;

    string SqlName
    {
        get
        {
            var prop = (this as AliasProperty)?.PersistentProperty ?? this;

            var ap = (prop as AssociationProperty) ?? ((prop as AliasProperty)?.Property as AssociationProperty);

            var apPk = ap switch
            {
                { Property: IProperty p } => p,
                { Association: Class classe } => classe.Properties.FirstOrDefault(),
                _ => null
            };

            var apPkTrigram = apPk?.Trigram ?? apPk?.Class.Trigram;

            var sqlName = prop switch
            {
                AssociationProperty or AliasProperty { Property: AssociationProperty } => apPkTrigram != null ? apPk?.SqlName.Replace($"{apPkTrigram}_", string.Empty) : apPk?.SqlName,
                { Class.Extends: not null, PrimaryKey: true } => prop.Name.Replace(prop.Class.Name, string.Empty).ToConstantCase(),
                _ => prop.Name.ToConstantCase()
            };

            string? prefix = prop.Trigram ?? (apPk != null ? apPkTrigram : prop.Class.Trigram);
            prefix = !string.IsNullOrWhiteSpace(prefix) ? $"{prefix}_" : string.Empty;
            var suffix = ap?.Role != null
                ? UseLegacyRoleName
                    ? $"_{ap.Role.Replace(" ", "_").ToUpper()}"
                    : $"_{ap.Role.ToConstantCase()}"
                : string.Empty;

            return $"{prefix}{sqlName}{suffix}";
        }
    }

    IProperty ResourceProperty => Decorator != null && Parent != Decorator
       ? Decorator.Properties.First(p => p.Name == Name).ResourceProperty
       : this is AliasProperty alp && alp.Label == alp.OriginalProperty?.Label
       ? alp.OriginalProperty!.ResourceProperty
       : this;

    string ResourceKey => $"{ResourceProperty.Parent.Namespace.ModuleCamel}.{ResourceProperty.Parent.NameCamel}.{ResourceProperty.NameCamel}";

    IProperty CommentResourceProperty => Decorator != null && Parent != Decorator
        ? Decorator.Properties.First(p => p.Name == Name).CommentResourceProperty
        : this is AliasProperty alp && alp.Comment == alp.OriginalProperty?.Comment
        ? alp.OriginalProperty!.CommentResourceProperty
        : this;

    string CommentResourceKey => $"comments.{CommentResourceProperty.Parent.Namespace.ModuleCamel}.{CommentResourceProperty.Parent.NameCamel}.{CommentResourceProperty.NameCamel}";

    bool UseLegacyRoleName { get; init; }

    IProperty CloneWithClassOrEndpoint(Class? classe = null, Endpoint? endpoint = null);
}