using Microsoft.Extensions.Logging;
using TopModel.Core.Model;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Rust;

/// <summary>
/// Générateur de structs/enums Rust à partir des classes du modèle.
/// </summary>
public class RustClassGenerator(ILogger<RustClassGenerator> logger, IFileWriterProvider writerProvider)
    : ClassGeneratorBase<RustConfig>(logger, writerProvider)
{
    /// <inheritdoc />
    public override string Name => "RustClassGen";

    /// <summary>
    /// Filtre les classes à générer : les classes abstraites ne produisent pas de struct/enum Rust
    /// (Rust n'a pas d'équivalent direct des classes abstraites — les comportements partagés se modélisent via des `trait`).
    /// </summary>
    protected override bool FilterClass(Class classe) => !classe.Abstract;

    /// <summary>
    /// Génère les `use` du fichier en se basant sur les imports déclarés par les domaines.
    /// </summary>
    protected virtual void GenerateUses(RustWriter w, Class classe, string tag)
    {
        if (classe.Enum == EnumMode.Enum)
        {
            if (Config.EnumDerives.Any(NeedsSerdeImport))
            {
                w.AddUse("serde::{Deserialize, Serialize}");
            }

            return;
        }

        var uses = new List<string>();

        if (Config.StructDerives.Any(NeedsSerdeImport))
        {
            uses.Add("serde::{Deserialize, Serialize}");
        }

        foreach (var property in classe.Properties)
        {
            uses.AddRange(Config.GetDomainImports(property, tag));
        }

        foreach (IProperty property in classe.Properties)
        {
            if (
                property is { Composition: Class cpc }
                && Config.AvailableClasses.Contains(cpc)
                && cpc != classe
            )
            {
                uses.Add(Config.GetClassNamespace(cpc, tag));
            }
            else if (
                property is { Association: Class assoc, UseClassForAssociation: true }
                && Config.AvailableClasses.Contains(assoc)
                && assoc != classe
            )
            {
                uses.Add(Config.GetClassNamespace(assoc, tag));
            }
            else if (
                property.EnumProperty is IProperty ep
                && Config.AvailableClasses.Contains(ep.Class)
                && ep.Class != classe
                && ep.Class.Enum == EnumMode.Enum
            )
            {
                uses.Add(Config.GetClassNamespace(ep.Class, tag));
            }
        }

        w.AddUses(uses);
    }

    /// <inheritdoc />
    protected override string GetFileName(Class classe, string tag)
    {
        return Config.GetClassFileName(classe, tag);
    }

    /// <summary>
    /// Récupère le type Rust à utiliser pour une propriété (wrappe avec `Option` si non requise).
    /// </summary>
    protected virtual string GetRustType(IProperty property)
    {
        var type = Config.GetType(property);

        if (string.IsNullOrEmpty(type))
        {
            type = "()";
        }

        if (!property.Required)
        {
            type = $"Option<{type}>";
        }

        return type;
    }

    /// <summary>
    /// Point d'entrée par classe : génère soit un enum Rust (si la classe est marquée `enum: enum`),
    /// soit une struct, en ajoutant en tête les `use` nécessaires.
    /// </summary>
    protected override void HandleClass(string fileName, Class classe, string tag)
    {
        using var w = this.OpenRustWriter(fileName);

        GenerateUses(w, classe, tag);

        if (classe.Enum == EnumMode.Enum)
        {
            WriteEnum(w, classe, tag);
        }
        else
        {
            WriteStruct(w, classe, tag);
        }
    }

    /// <summary>
    /// Écrit un enum Rust à partir d'une classe ayant `enum: enum`.
    /// </summary>
    protected virtual void WriteEnum(RustWriter w, Class classe, string tag)
    {
        w.WriteDoc(0, classe.Comment);

        var enumDerives = Config.UseSqlx && Config.IsPersistent(classe, tag)
            ? [.. Config.EnumDerives, "sqlx::Type"]
            : Config.EnumDerives;

        if (enumDerives.Length > 0)
        {
            w.WriteAttribute(0, $"derive({string.Join(", ", enumDerives)})");
        }

        if (!string.IsNullOrEmpty(Config.EnumSerdeRenameAll))
        {
            w.WriteAttribute(0, $@"serde(rename_all = ""{Config.EnumSerdeRenameAll}"")");
        }

        if (Config.UseSqlx && Config.IsPersistent(classe, tag))
        {
            var sqlxArgs = new List<string>
            {
                $@"type_name = ""{classe.Name.Value.ToSnakeCase()}""",
            };

            if (!string.IsNullOrEmpty(Config.SqlxEnumRenameAll))
            {
                sqlxArgs.Add($@"rename_all = ""{Config.SqlxEnumRenameAll}""");
            }

            w.WriteAttribute(0, $"sqlx({string.Join(", ", sqlxArgs)})");
        }

        w.WriteLine(0, $"pub enum {classe.NamePascal} {{");

        var values = Config.GetAllValues(classe).OrderBy(v => v.Name, StringComparer.Ordinal).ToList();

        for (var i = 0; i < values.Count; i++)
        {
            var refValue = values[i];

            if (i > 0)
            {
                w.WriteLine();
            }

            var label = refValue.GetLabel(classe);
            if (!string.IsNullOrWhiteSpace(label))
            {
                w.WriteDoc(1, label);
            }

            w.WriteLine(1, $"{refValue.Name.ToPascalCase(strictIfUppercase: true)},");
        }

        w.WriteLine(0, "}");
    }

    /// <summary>
    /// Écrit une struct Rust pour la classe demandée.
    /// </summary>
    protected virtual void WriteStruct(RustWriter w, Class classe, string tag)
    {
        w.WriteDoc(0, classe.Comment);

        var structDerives = Config.UseSqlx && Config.IsPersistent(classe, tag)
            ? [.. Config.StructDerives, "sqlx::FromRow"]
            : Config.StructDerives;

        if (structDerives.Length > 0)
        {
            w.WriteAttribute(0, $"derive({string.Join(", ", structDerives)})");
        }

        if (!string.IsNullOrEmpty(Config.StructSerdeRenameAll))
        {
            w.WriteAttribute(0, $@"serde(rename_all = ""{Config.StructSerdeRenameAll}"")");
        }

        w.WriteLine(0, $"pub struct {classe.NamePascal} {{");

        List<IProperty> props = classe.ExtendedProperties.ToList();
        for (var i = 0; i < props.Count; i++)
        {
            var property = props[i];

            if (i > 0)
            {
                w.WriteLine();
            }

            w.WriteDoc(1, property.Comment);

            var fieldName = property.NameCamel.ToSnakeCase().EscapeKeyword();
            var fieldType = GetRustType(property);

            w.WriteLine(1, $"pub {fieldName}: {fieldType},");
        }

        w.WriteLine(0, "}");
    }

    /// <summary>
    /// Indique si un derive nécessite l'import du crate `serde` (les autres derives standards proviennent du prélude Rust).
    /// </summary>
    private static bool NeedsSerdeImport(string derive) => derive is "Serialize" or "Deserialize";
}
