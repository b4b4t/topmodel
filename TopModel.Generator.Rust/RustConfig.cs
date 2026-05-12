using TopModel.Core.FileModel;
using TopModel.Core.Model;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Rust;

/// <summary>
/// Paramètres pour la génération de Rust.
/// </summary>
public class RustConfig : GeneratorConfigBase
{
    /// <summary>
    /// Localisation du modèle généré, relative au répertoire de génération. Par défaut : "src/{module:snake}".
    /// </summary>
    public virtual string ModelRootPath { get; set; } = "src/{module:snake}";

    /// <summary>
    /// Nom du fichier `mod.rs` généré listant les sous-modules. Par défaut : `mod`.
    /// </summary>
    public virtual string ModFileName { get; set; } = "mod";

    /// <summary>
    /// Génère un fichier `mod.rs` par module qui réexporte chaque struct/enum générée. Par défaut : 'true'.
    /// </summary>
    public virtual bool GenerateModFiles { get; set; } = true;

    /// <summary>
    /// Liste des derives appliqués par défaut sur les structs générées. Par défaut : `Debug, Clone, Serialize, Deserialize`.
    /// </summary>
    public virtual string[] StructDerives { get; set; } = ["Debug", "Clone", "Serialize", "Deserialize"];

    /// <summary>
    /// Liste des derives appliqués par défaut sur les enums générées. Par défaut : `Debug, Clone, Copy, PartialEq, Eq, Serialize, Deserialize`.
    /// </summary>
    public virtual string[] EnumDerives { get; set; } =
        ["Debug", "Clone", "Copy", "PartialEq", "Eq", "Serialize", "Deserialize"];

    /// <summary>
    /// Attribut `#[serde(rename_all = "...")]` à appliquer sur les enums. Par défaut : `snake_case`. Mettre à vide pour ne rien générer.
    /// </summary>
    public virtual string EnumSerdeRenameAll { get; set; } = "snake_case";

    /// <summary>
    /// Génère un attribut `#[serde(rename_all = "...")]` sur chaque struct (par exemple `camelCase` pour matcher des APIs JSON). Vide par défaut.
    /// </summary>
    public virtual string? StructSerdeRenameAll { get; set; }

    /// <summary>
    /// Ajoute les derives nécessaires pour `sqlx` (`sqlx::FromRow` sur les structs, `sqlx::Type` sur les enums) et un attribut `#[sqlx(type_name = "...")]` sur les enums pour cibler un type SQL custom. Par défaut : 'false'.
    /// </summary>
    public virtual bool UseSqlx { get; set; }

    /// <summary>
    /// Valeur de l'attribut `rename_all` ajouté dans `#[sqlx(...)]` sur les enums lorsque `useSqlx` est activé (ex: `lowercase`). Vide par défaut.
    /// </summary>
    public virtual string? SqlxEnumRenameAll { get; set; }

    public override string? DefaultLanguage => "rust";

    public override string[] PropertiesWithModuleVariableSupport => [nameof(ModelRootPath)];

    public override string[] PropertiesWithTagVariableSupport => [nameof(ModelRootPath)];

    protected override bool UseValueNameForValues => true;

    protected override string NullValue => "None";

    /// <summary>
    /// Récupère le chemin absolu vers le fichier d'une classe.
    /// </summary>
    public virtual string GetClassFileName(Class classe, string tag)
    {
        return Path.Combine(
                OutputDirectory,
                ResolveVariables(ModelRootPath, tag, classe.Namespace.Module.ToSnakeCase()),
                $"{classe.Name.Value.ToSnakeCase()}.rs"
            )
            .Replace('\\', '/');
    }

    /// <summary>
    /// Récupère le chemin du fichier `mod.rs` pour un module donné.
    /// </summary>
    public virtual string GetModFileName(Namespace ns, string tag)
    {
        return Path.Combine(
                OutputDirectory,
                ResolveVariables(ModelRootPath, tag, ns.Module.ToSnakeCase()),
                $"{ModFileName}.rs"
            )
            .Replace('\\', '/');
    }
}
