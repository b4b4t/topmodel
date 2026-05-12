using TopModel.Utils;

namespace TopModel.Generator.Rust;

/// <summary>
/// FileWriter avec des méthodes spécialisées pour écrire du Rust.
/// </summary>
public class RustWriter(IFileWriter writer) : IDisposable
{
    /// <summary>
    /// Lignes du fichier mises en tampon avant écriture lors du <see cref="Dispose"/>,
    /// pour pouvoir insérer les `use` accumulés en tête de fichier.
    /// </summary>
    private readonly List<WriterLine> _lines = [];

    /// <summary>
    /// Chemins de `use` collectés au fur et à mesure de la génération, dédupliqués et triés à la fermeture.
    /// </summary>
    private readonly List<string> _uses = [];

    /// <summary>
    /// Active ou désactive l'écriture de l'en-tête de fichier généré.
    /// </summary>
    public bool EnableHeader
    {
        get => writer.EnableHeader;
        set => writer.EnableHeader = value;
    }

    /// <summary>
    /// Message d'en-tête à écrire en haut du fichier généré.
    /// </summary>
    public string HeaderMessage
    {
        get => writer.HeaderMessage;
        set => writer.HeaderMessage = value;
    }

    /// <summary>
    /// Ajoute une déclaration `use` au fichier (sans le mot-clé ni le `;`).
    /// </summary>
    public void AddUse(string path)
    {
        _uses.Add(path);
    }

    /// <summary>
    /// Ajoute plusieurs déclarations `use` au fichier.
    /// </summary>
    public void AddUses(IEnumerable<string> paths)
    {
        _uses.AddRange(paths);
    }

    /// <inheritdoc cref="IDisposable.Dispose" />
    public void Dispose()
    {
        writer.StartCommentToken = "//!";

        if (_uses.Count > 0)
        {
            // Regroupement des `use` en trois blocs successifs (std, crates externes, crate local),
            // chacun trié et dédupliqué — convention courante dans l'écosystème Rust.
            var stdUses = _uses.Where(u => u.StartsWith("std::") || u == "std").Order().Distinct();
            var crateUses = _uses
                .Where(u => u.StartsWith("crate::") || u == "crate")
                .Order(StringComparer.Ordinal)
                .Distinct();
            var otherUses = _uses
                .Except(stdUses)
                .Except(crateUses)
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Order(StringComparer.Ordinal)
                .Distinct();

            foreach (var path in stdUses)
            {
                writer.WriteLine($"use {path};");
            }

            foreach (var path in otherUses)
            {
                writer.WriteLine($"use {path};");
            }

            foreach (var path in crateUses)
            {
                writer.WriteLine($"use {path};");
            }

            writer.WriteLine();
        }

        foreach (var line in _lines)
        {
            writer.WriteLine(line.Indent, line.Line);
        }

        writer.Dispose();
    }

    /// <summary>
    /// Écrit un attribut `#[...]`.
    /// </summary>
    public void WriteAttribute(int indent, string attribute)
    {
        WriteLine(indent, $"#[{attribute}]");
    }

    /// <summary>
    /// Écrit un commentaire `///` (doc comment) sur une ou plusieurs lignes.
    /// </summary>
    public void WriteDoc(int indent, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        var text = value.ReplaceLineEndings("\n").Trim();
        foreach (var line in text.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                WriteLine(indent, "///");
            }
            else
            {
                WriteLine(indent, $"/// {line}");
            }
        }
    }

    /// <summary>
    /// Écrit une ligne sans indentation.
    /// </summary>
    public void WriteLine(string? value = null)
    {
        WriteLine(0, value ?? string.Empty);
    }

    /// <summary>
    /// Écrit une ligne indentée.
    /// </summary>
    public void WriteLine(int indent, string value)
    {
        _lines.Add(new() { Indent = indent, Line = value });
    }
}
