using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using TopModel.Core.FileModel;
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

        foreach (IProperty property in classe.ExtendedProperties)
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

        string currentCrateName = Config.GetCrateName(Config.GetModelRootPath(classe, tag));

        w.AddUses(Config.SetCurrentCrate(uses, currentCrateName));
    }

    /// <inheritdoc />
    protected override string GetFileName(Class classe, string tag)
    {
        return Config.GetClassFileName(classe, tag);
    }

    /// <inheritdoc />
    protected override void HandleFiles(IEnumerable<ModelFile> files)
    {
        // Génération des classes en parallèle par fichier, puis par classe,
        // puis par tag (généralement un tag = un module de destination différent)
        base.HandleFiles(files);

        // Génération des mod.rs pour chaque module en parallèle
        Dictionary<string, List<Class>> classesByModule = [];
        foreach (Class classe in files.SelectMany(f => f.Classes.Where(FilterClass)))
        {
            List<string> modFiles = Config
                                .Tags.Intersect(classe.Tags)
                                .Select(tag => Config.GetModFileName(classe, tag)).ToList();
            foreach (string modFile in modFiles)
            {
                if (!classesByModule.TryGetValue(modFile, out List<Class>? value))
                {
                    value = [];
                    classesByModule[modFile] = value;
                }

                value.Add(classe);
            }
        }

        Parallel.ForEach(
            classesByModule,
            moduleGroup =>
            {
                string modFile = moduleGroup.Key;
                List<Class> classes = moduleGroup.Value.ToList();
                GenerateModuleModRs(modFile, classes);
            }
        );
    }

    /// <summary>
    /// Génère un fichier `mod.rs` pour le module donné, listant tous les sous-modules correspondant aux classes du module.
    /// </summary>
    /// <param name="fileName">Le chemin du fichier `mod.rs` à générer.</param>
    /// <param name="classes">La liste des classes du module.</param>
    private void GenerateModuleModRs(string fileName, List<Class> classes)
    {
        using var w = this.OpenRustWriter(fileName);
        foreach (var classe in classes.OrderBy(c => c.NamePascal))
        {
            w.WriteLine($"pub mod {classe.NamePascal.ToSnakeCase()};");
        }
    }

    /// <summary>
    /// Point d'entrée par classe : génère soit un enum Rust (si la classe est marquée `enum: enum`),
    /// soit une struct, en ajoutant en tête les `use` nécessaires.
    /// </summary>
    protected override void HandleClass(string fileName, Class classe, string tag)
    {
        using var w = this.OpenRustWriter(fileName);

        GenerateUses(w, classe, tag);

        if (classe.Enum != EnumMode.Enum)
        {
            WriteStruct(w, classe, tag);

            foreach (var prop in classe.ExtendedProperties.Where(p => p.EnumProperty == p))
            {
                w.WriteLine();
                WriteEnum(w, prop, tag);
                w.WriteLine();
                WriteParseError(w, prop);
            }
        }
        else
        {
            w.WriteLine();
            WriteEnum(w, classe.EnumKey!, tag);
            w.WriteLine();
            WriteParseError(w, classe.EnumKey!);
        }
    }

    /// <summary>
    /// Écrit un enum Rust à partir d'une classe ayant `enum: enum`.
    /// </summary>
    protected virtual void WriteEnum(RustWriter w, IProperty prop, string tag)
    {
        if (prop.UniqueValuedProperty == null)
        {
            return;
        }

        Class classe = prop.UniqueValuedProperty!.Class;

        w.WriteDoc(0, prop.Class.Comment);

        string enumName = Config.GetEnumType(prop, internalReference: true);

        // var enumDerives = Config.UseSqlx && Config.IsPersistent(classe, tag)
        //     ? [.. Config.EnumDerives, "sqlx::Type"]
        //     : Config.EnumDerives;
        var enumDerives = Config.EnumDerives;

        if (enumDerives.Length > 0)
        {
            w.WriteAttribute(0, $"derive({string.Join(", ", enumDerives)})");
        }

        if (!string.IsNullOrEmpty(Config.EnumSerdeRenameAll))
        {
            w.WriteAttribute(0, $@"serde(rename_all = ""{Config.EnumSerdeRenameAll}"")");
        }

        // if (Config.UseSqlx && Config.IsPersistent(classe, tag))
        // {
        //     var sqlxArgs = new List<string>
        //     {
        //         $@"type_name = ""{enumName.ToSnakeCase()}""",
        //     };

        //     if (!string.IsNullOrEmpty(Config.SqlxEnumRenameAll))
        //     {
        //         sqlxArgs.Add($@"rename_all = ""{Config.SqlxEnumRenameAll}""");
        //     }

        //     w.WriteAttribute(0, $"sqlx({string.Join(", ", sqlxArgs)})");
        // }

        w.WriteLine(0, $"pub enum {enumName} {{");

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
    /// Écrit un enum Rust à partir d'une classe ayant `enum: enum`.
    /// </summary>
    protected virtual void WriteParseError(RustWriter w, IProperty prop)
    {
        if (prop.UniqueValuedProperty == null)
        {
            return;
        }

        Class classe = prop.UniqueValuedProperty!.Class;

        string enumType = classe.Enum == EnumMode.Enum ?
            classe.NamePascal : Config.GetEnumType(prop, internalReference: true);
        string parseErrorType = $"{enumType}ParseError";

        w.AddUses(["std::convert::TryFrom", "std::error::Error", "std::fmt"]);

        w.WriteDoc(0, "Error parsing type");
        w.WriteAttribute(0, "derive(Debug)");
        w.WriteLine(0, $"pub struct {parseErrorType};");
        w.WriteLine();

        w.WriteDoc(0, "Display support");
        w.WriteLine(0, $"impl fmt::Display for {parseErrorType} {{");
        w.WriteLine(1, "fn fmt(&self, f: &mut fmt::Formatter) -> fmt::Result {");
        w.WriteLine(2, $"write!(f, \"Failed to parse {classe.NamePascal}\")");
        w.WriteLine(1, "}");
        w.WriteLine(0, "}");
        w.WriteLine();

        w.WriteLine(0, $"impl Error for {parseErrorType} {{}}");
        w.WriteLine();

        w.WriteDoc(0, $"Convert String to {enumType}");
        w.WriteLine(0, $"impl TryFrom<String> for {enumType} {{");
        w.WriteLine(1, $"type Error = {parseErrorType};");
        w.WriteLine();
        w.WriteLine(1, $"fn try_from(value: String) -> Result<Self, {parseErrorType}> {{");
        w.WriteLine(2, "match value.as_ref() {");

        List<ClassValue> values = Config.GetAllValues(classe).OrderBy(v => v.Name, StringComparer.Ordinal).ToList();

        for (int i = 0; i < values.Count; i++)
        {
            ClassValue refValue = values[i];

            w.WriteLine(3, $"\"{refValue.Value[prop]}\" => Ok({enumType}::{refValue.Name.ToPascalCase(strictIfUppercase: true)}),");
        }
        w.WriteLine(3, $"_ => Err({parseErrorType}),");
        w.WriteLine(2, "}");
        w.WriteLine(1, "}");
        w.WriteLine(0, "}");

        w.WriteLine();

        w.WriteDoc(0, $"Convert {enumType} to String");
        w.WriteLine(0, $"impl From<{enumType}> for String {{");
        w.WriteLine(1, $"fn from(val: {enumType}) -> Self {{");
        w.WriteLine(2, "match val {");

        for (int i = 0; i < values.Count; i++)
        {
            ClassValue refValue = values[i];

            w.WriteLine(3, $"{enumType}::{refValue.Name.ToPascalCase(strictIfUppercase: true)} => \"{refValue.Value[prop]}\".to_string(),");
        }

        w.WriteLine(2, "}");
        w.WriteLine(1, "}");
        w.WriteLine(0, "}");

        w.WriteLine(0, $"impl sqlx::Type<sqlx::Postgres> for {enumType} {{");
        w.WriteLine(1, "fn type_info() -> sqlx::postgres::PgTypeInfo {");
        w.WriteLine(2, "<String as sqlx::Type<sqlx::Postgres>>::type_info()");
        w.WriteLine(1, "}");
        w.WriteLine(1, "fn compatible(ty: &sqlx::postgres::PgTypeInfo) -> bool {");
        w.WriteLine(2, "<String as sqlx::Type<sqlx::Postgres>>::compatible(ty)");
        w.WriteLine(1, "}");
        w.WriteLine(0, "}");

        w.WriteLine(0, $"impl<'r> sqlx::Decode<'r, sqlx::Postgres> for {enumType} {{");
        w.WriteLine(1, "fn decode(");
        w.WriteLine(2, "value: sqlx::postgres::PgValueRef<'r>,");
        w.WriteLine(1, ") -> Result<Self, Box<dyn Error + Send + Sync>> {");
        w.WriteLine(2, "let value = <String as sqlx::Decode<sqlx::Postgres>>::decode(value)?;");
        w.WriteLine(2, $"{enumType}::try_from(value)");
        w.WriteLine(3, ".map_err(|e| Box::new(e) as Box<dyn Error + Send + Sync>)");
        w.WriteLine(1, "}");
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

            if (
                property.PersistentClass != null
                && !Config.NoPersistence(tag)
                && !property.AssociationMultiple
                && !property.UseClassForAssociation
            )
            {
                w.WriteAttribute(1, $@"sqlx(rename = ""{property.SqlName.ToLowerInvariant()}"")");
            }

            var fieldName = property.NameCamel.ToSnakeCase().EscapeKeyword();
            var fieldType = Config.GetRustType(property);

            w.WriteLine(1, $"pub {fieldName}: {fieldType},");
        }

        w.WriteLine(0, "}");
    }

    /// <summary>
    /// Indique si un derive nécessite l'import du crate `serde` (les autres derives standards proviennent du prélude Rust).
    /// </summary>
    private static bool NeedsSerdeImport(string derive) => derive is "Serialize" or "Deserialize";
}
