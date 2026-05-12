using TopModel.Generator.Core;

namespace TopModel.Generator.Rust;

/// <summary>
/// Classe utilitaire destinée à la génération de Rust.
/// </summary>
public static class RustUtils
{
    /// <summary>
    /// Liste des mots-clés réservés en Rust (mots-clés actuels et futurs réservés par la spécification).
    /// Utilisée pour préfixer les identifiants en collision avec `r#`.
    /// </summary>
    private static readonly HashSet<string> _reservedKeywords = new(
        [
            "as",
            "async",
            "await",
            "break",
            "const",
            "continue",
            "crate",
            "dyn",
            "else",
            "enum",
            "extern",
            "false",
            "fn",
            "for",
            "if",
            "impl",
            "in",
            "let",
            "loop",
            "match",
            "mod",
            "move",
            "mut",
            "pub",
            "ref",
            "return",
            "self",
            "Self",
            "static",
            "struct",
            "super",
            "trait",
            "true",
            "type",
            "unsafe",
            "use",
            "where",
            "while",
            "abstract",
            "become",
            "box",
            "do",
            "final",
            "macro",
            "override",
            "priv",
            "try",
            "typeof",
            "unsized",
            "virtual",
            "yield",
        ]
    );

    /// <summary>
    /// Préfixe un nom avec `r#` s'il s'agit d'un mot réservé en Rust.
    /// </summary>
    public static string EscapeKeyword(this string name)
    {
        if (_reservedKeywords.Contains(name))
        {
            return $"r#{name}";
        }

        return name;
    }

    /// <summary>
    /// Ouvre un <see cref="RustWriter"/> pour le fichier indiqué, sans BOM UTF-8 (les fichiers source Rust sont attendus en UTF-8 sans BOM).
    /// </summary>
    public static RustWriter OpenRustWriter(this GeneratorBase<RustConfig> generator, string fileName)
    {
        return new RustWriter(generator.OpenFileWriter(fileName, encoderShouldEmitUTF8Identifier: false));
    }
}
