using TopModel.Core.FileModel;
using TopModel.Core.Model;
using TopModel.Generator.Core;
using TopModel.Utils;
using YamlDotNet.Serialization;

namespace TopModel.Generator.Rust;

/// <summary>
/// Paramètres pour la génération de Rust.
/// </summary>
public class RustConfig : GeneratorConfigBase
{
    /// <summary>
    /// Localisation du modèle persisté, relative au répertoire de génération. Par défaut : {app}.{module}.Models.
    /// </summary>
    public virtual string PersistentModelPath { get; set; } = "{app}.{module}.Models";

    /// <summary>
    /// Localisation des classes de références, relative au répertoire de génération.
    /// Si non renseigné, ces classes seront générées comme les autres (selon si elles sont persistantes ou non).
    /// </summary>
    public virtual string? ReferencesModelPath { get; set; }

    /// <summary>
    /// Localisation du modèle non persisté, relative au répertoire de génération. Par défaut : {app}.{module}.Models/Dto.
    /// </summary>
    public virtual string NonPersistentModelPath { get; set; } = "{app}.{module}.Models/Dto";

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

    /// <summary>
    /// Considère toutes les classes comme étant non-persistantes (= pas d'attribut SQL).
    /// </summary>
    [YamlMember(Alias = "noPersistence")]
    public virtual string? NoPersistenceParam { get; set; }

    public override string? DefaultLanguage => "rust";

    /// <summary>
    /// Localisation des mappers générés, relative au répertoire de génération. Par défaut : "src/{module:snake}/mapper".
    /// </summary>
    public virtual string MapperRootPath { get; set; } = "src/{module:snake}/mapper";

    public override string[] PropertiesWithModuleVariableSupport => [nameof(NonPersistentModelPath), nameof(PersistentModelPath), nameof(ReferencesModelPath), nameof(MapperRootPath)];

    public override string[] PropertiesWithTagVariableSupport => [nameof(NonPersistentModelPath), nameof(PersistentModelPath), nameof(ReferencesModelPath), nameof(MapperRootPath), nameof(NoPersistenceParam)];

    protected override bool UseValueNameForValues => true;

    /// <summary>
    /// Détermine si la persistence est désactivée pour le tag donné.
    /// </summary>
    public virtual bool NoPersistence(string tag)
    {
        return ResolveVariables(NoPersistenceParam ?? string.Empty, tag) == true.ToString();
    }

    /// <inheritdoc />
    public override bool IsPersistent(Class classe, string tag)
    {
        return base.IsPersistent(classe, tag) && !NoPersistence(tag);
    }

    protected override string NullValue => "None";

    /// <summary>
    /// Localisation du modèle généré, relative au répertoire de génération. Par défaut : "src/{module:snake}".
    /// </summary>
    public virtual string GetModelRootPath(Class classe, string tag)
    {
        if (classe.Reference)
        {
            return ReferencesModelPath ?? PersistentModelPath;
        }
        else if (IsPersistent(classe, tag))
        {
            return PersistentModelPath;
        }
        else
        {
            return NonPersistentModelPath;
        }
    }

    /// <summary>
    /// Récupère le chemin absolu vers le fichier d'une classe.
    /// </summary>
    public virtual string GetClassFileName(Class classe, string tag)
    {
        return Path.Combine(
                OutputDirectory,
                ResolveVariables(GetModelRootPath(classe, tag), tag, classe.Namespace.Module.ToSnakeCase()),
                $"{classe.Name.Value.ToSnakeCase()}.rs"
            )
            .Replace('\\', '/');
    }

    /// <summary>
    /// Calcule le chemin `use` (de la forme `crate::module::sous_module::nom_fichier::NomType`)
    /// permettant d'importer la struct/enum d'une autre classe générée du modèle.
    /// </summary>
    public virtual string GetClassNamespace(Class classe, string tag)
    {
        string modulePath = Path.Combine(
                ResolveVariables(GetModelRootPath(classe, tag), tag, classe.Namespace.Module.ToSnakeCase())
            )
            .Replace("\\", "::").Replace("/", "::").Replace('-', '_').Replace("src::", string.Empty);
        string fileName = classe.Name.Value.ToSnakeCase();

        return $"{string.Join("::", modulePath)}::{fileName}::{classe.NamePascal}";
    }

    /// <summary>
    /// Récupère le chemin du fichier `mod.rs` pour un module donné.
    /// </summary>
    public virtual string GetModFileName(Class classe, string tag)
    {
        return Path.Combine(
                OutputDirectory,
                ResolveVariables(GetModelRootPath(classe, tag), tag, classe.Namespace.Module.ToSnakeCase()),
                $"{ModFileName}.rs"
            )
            .Replace('\\', '/');
    }

    /// <summary>
    /// Récupère le chemin du fichier mapper pour un fromMapper.
    /// </summary>
    public virtual string GetMapperFilePath((Class Classe, FromMapper Mapper) mapper, string tag)
    {
        var module = mapper.Classe.Namespace.Module.ToSnakeCase();
        return Path.Combine(
                OutputDirectory,
                ResolveVariables(MapperRootPath, tag, module),
                $"{mapper.Classe.Name.Value.ToSnakeCase()}_mapper.rs"
            )
            .Replace('\\', '/');
    }

    /// <summary>
    /// Récupère le chemin du fichier mapper pour un toMapper.
    /// </summary>
    public virtual string GetMapperFilePath((Class Classe, ClassMappings Mapper) mapper, string tag)
    {
        var module = mapper.Classe.Namespace.Module.ToSnakeCase();
        return Path.Combine(
                OutputDirectory,
                ResolveVariables(MapperRootPath, tag, module),
                $"{mapper.Classe.Name.Value.ToSnakeCase()}_mapper.rs"
            )
            .Replace('\\', '/');
    }

    /// <summary>
    /// Récupère le nom du crate à partir du chemin du fichier   
    /// </summary>
    /// <param name="path">Chemin du fichier</param>
    /// <returns>Nom du crate</returns>
    public virtual string GetCrateName(string path)
    {
        string[] parts = path.Replace("\\", "/").Replace('-', '_').Split('/');
        string crateName = string.Empty;
        foreach (string part in parts)
        {
            if (part == "src")
            {
                return crateName;
            }
            crateName = part;
        }

        throw new NotSupportedException("Cannot find the crate name.");
    }

    /// <summary>
    /// Récupère le type Rust à utiliser pour une propriété (wrappe avec `Option` si non requise).
    /// </summary>
    public virtual string GetRustType(IProperty property)
    {
        var type = GetType(property);

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
    /// Remplace le nom du crate par crate si le nom est le même. 
    /// </summary>
    public IEnumerable<string> SetCurrentCrate(IEnumerable<string> uses, string currentCrateName)
    {
        foreach (string use in uses)
        {
            string[] parts = use.Split("::");

            if (parts.Length > 0 && parts[0] == currentCrateName)
            {
                yield return string.Join("::", ["crate", .. parts[1..]]);
            }
            else
            {
                yield return use;
            }
        }
    }
}
