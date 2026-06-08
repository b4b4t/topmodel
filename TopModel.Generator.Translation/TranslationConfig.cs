using TopModel.Generator.Core;

namespace TopModel.Generator.Translation;

public class TranslationConfig : GeneratorConfigBase
{
    /// <summary>
    /// liste des langues de l'application
    /// </summary>
    public virtual IList<string> Langs { get; set; } = [];

    /// <summary>
    /// liste des langues de l'application
    /// </summary>
    public virtual string RootPath { get; set; } = "{lang}";

    /// <summary>
    /// Type de traduction à générer : js (objets de traduction javascripts) ou json
    /// </summary>
    public virtual string TranslationType { get; set; } = "js";

    public override string[] PropertiesWithTagVariableSupport => [nameof(RootPath)];

    public override string[] PropertiesWithLangVariableSupport => [nameof(RootPath)];
}
