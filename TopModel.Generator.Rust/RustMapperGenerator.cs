using Microsoft.Extensions.Logging;
using TopModel.Core.Model;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Rust;

/// <summary>
/// Générateur de mappers Rust implémentant le trait `Converter&lt;TDao, TDto&gt;`.
/// </summary>
public class RustMapperGenerator(ILogger<RustMapperGenerator> logger, IFileWriterProvider writerProvider)
    : MapperGeneratorBase<RustConfig>(logger, writerProvider)
{
    /// <inheritdoc />
    public override string Name => "RustMapperGen";

    /// <inheritdoc />
    protected override string GetFileName((Class Classe, FromMapper Mapper) mapper, string tag)
    {
        return Config.GetMapperFilePath(mapper, tag);
    }

    /// <inheritdoc />
    protected override string GetFileName((Class Classe, ClassMappings Mapper) mapper, string tag)
    {
        return Config.GetMapperFilePath(mapper, tag);
    }

    /// <inheritdoc />
    protected override void HandleFile(
        string fileName,
        string tag,
        IList<(Class Classe, FromMapper Mapper)> fromMappers,
        IList<(Class Classe, ClassMappings Mapper)> toMappers)
    {
        using var w = this.OpenRustWriter(fileName);

        // Collect use statements for all involved classes.
        var uses = new List<string>();

        foreach (var (classe, mapper) in fromMappers)
        {
            uses.Add(Config.GetClassNamespace(classe, tag));
            foreach (var cp in mapper.ClassParams)
            {
                uses.Add(Config.GetClassNamespace(cp.Class, Config.GetBestClassTag(cp.Class, tag)));
            }
        }

        foreach (var (classe, mapper) in toMappers)
        {
            uses.Add(Config.GetClassNamespace(classe, Config.GetBestClassTag(classe, tag)));
            uses.Add(Config.GetClassNamespace(mapper.Class, Config.GetBestClassTag(mapper.Class, tag)));
        }

        uses.Add("crate::mapper::Converter");

        string currentCrateName = Config.GetCrateName(Config.MapperRootPath);

        w.AddUses(SetCurrentCrate(uses, currentCrateName));

        // Generate fromMappers: creates target class from source class params (to_dao direction).
        foreach (var (classe, mapper) in fromMappers)
        {
            foreach (var classParam in mapper.ClassParams)
            {
                var sourceClass = classParam.Class;
                var targetClass = classe;

                WriteConverterImpl(w, sourceClass, targetClass, classParam.Mappings, tag);

                if (fromMappers.IndexOf((classe, mapper)) < fromMappers.Count - 1)
                {
                    w.WriteLine();
                }
            }
        }

        // Generate toMappers: maps source class to target class (to_dto direction).
        foreach (var (classe, mapper) in toMappers)
        {
            var sourceClass = classe;
            var targetClass = mapper.Class;

            WriteConverterImpl(w, sourceClass, targetClass, mapper.Mappings, tag);

            if (toMappers.IndexOf((classe, mapper)) < toMappers.Count - 1)
            {
                w.WriteLine();
            }
        }
    }

    private static IEnumerable<string> SetCurrentCrate(IEnumerable<string> uses, string currentCrateName)
    {
        foreach (string use in uses)
        {
            string[] parts = use.Split("::");

            if (parts.Length > 0 && parts[0] == currentCrateName)
            {
                yield return string.Join("::", ["crate", .. parts[1..]]);
            }
            yield return use;
        }
    }

    /// <summary>
    /// Écrit une implémentation du trait `Converter` entre deux classes.
    /// </summary>
    private void WriteConverterImpl(
        RustWriter w,
        Class daoClass,
        Class dtoClass,
        IDictionary<IProperty, IProperty> mappings,
        string tag)
    {
        var daoName = daoClass.NamePascal;
        var dtoName = dtoClass.NamePascal;
        var mapperStructName = $"{daoName}Mapper";

        w.WriteLine($"pub struct {mapperStructName};");
        w.WriteLine();
        w.WriteLine($"impl Converter<{daoName}, {dtoName}> for {mapperStructName} {{");

        // Search all (dao) fields that does not exist in the source (dao)
        List<IProperty> properties = mappings.Select(p => p.Value).ToList();
        List<IProperty> daoMissingProperties = daoClass.ExtendedProperties.Where(p => !properties.Contains(p)).ToList();

        // to_dto: converts from DAO to DTO
        string daoMissingParamters = string.Join(", ",
            daoMissingProperties.Select(p => $"{p.NameCamel.ToSnakeCase().EscapeKeyword()}: {Config.GetRustType(p)} "));

        // to_dao: converts from DTO to DAO
        if (daoMissingProperties.Count > 0)
        {
            w.WriteLine(1, $"fn to_dao(dto: &{dtoName}, {daoMissingParamters}) -> {daoName} {{");
        }
        else
        {
            w.WriteLine(1, $"fn to_dao(dto: &{dtoName}) -> {daoName} {{");
        }

        w.WriteLine(2, $"{daoName} {{");

        foreach (var mapping in mappings)
        {
            var targetProp = mapping.Key;
            var sourceProp = mapping.Value;

            var targetField = targetProp.NameCamel.ToSnakeCase().EscapeKeyword();
            var sourceField = sourceProp.NameCamel.ToSnakeCase().EscapeKeyword();
            var value = GetFieldValue($"dto.{sourceField}", sourceProp, targetProp);

            w.WriteLine(3, $"{targetField}: {value},");
        }

        foreach (IProperty parameter in daoMissingProperties)
        {
            w.WriteLine(3, $"{parameter.NameCamel.ToSnakeCase().EscapeKeyword()},");
        }

        w.WriteLine(2, "}");
        w.WriteLine(1, "}");
        w.WriteLine();

        // Search all (dto) fields that does not exist in the source (dao)
        List<IProperty> dtoMissingProperties = dtoClass.ExtendedProperties.Where(p => !properties.Contains(p)).ToList();

        // to_dto: converts from DAO to DTO
        string missingParamters = string.Join(", ",
            dtoMissingProperties.Select(p => $"{p.NameCamel.ToSnakeCase().EscapeKeyword()}: {Config.GetRustType(p)} "));

        if (dtoMissingProperties.Count > 0)
        {
            w.WriteLine(1, $"fn to_dto(dao: &{daoName}, {missingParamters}) -> {dtoName} {{");
        }
        else
        {
            w.WriteLine(1, $"fn to_dto(dao: &{daoName}) -> {dtoName} {{");
        }
        w.WriteLine(2, $"{dtoName} {{");

        foreach (var mapping in mappings)
        {
            var targetProp = mapping.Key;
            var sourceProp = mapping.Value;

            // Inverse direction: DAO -> DTO
            var daoField = targetProp.NameCamel.ToSnakeCase().EscapeKeyword();
            var dtoField = sourceProp.NameCamel.ToSnakeCase().EscapeKeyword();
            var value = GetFieldValue($"dao.{daoField}", targetProp, sourceProp);

            w.WriteLine(3, $"{dtoField}: {value},");
        }
        foreach (IProperty parameter in dtoMissingProperties)
        {
            w.WriteLine(3, $"{parameter.NameCamel.ToSnakeCase().EscapeKeyword()},");
        }

        w.WriteLine(2, "}");
        w.WriteLine(1, "}");
        w.WriteLine(0, "}");
    }

    /// <summary>
    /// Génère la valeur d'un champ en tenant compte des types Option et clone.
    /// </summary>
    private string GetFieldValue(string sourceExpr, IProperty source, IProperty target)
    {
        var sourceType = Config.GetType(source);
        // var targetType = Config.GetType(target);

        // If source is Option but target is not, unwrap with unwrap_or_default()
        if (!source.Required && target.Required)
        {
            return $"{sourceExpr}.unwrap_or_default()";
        }

        // If source is not Option but target is Option, wrap with Some()
        if (source.Required && !target.Required)
        {
            return $"Some({sourceExpr})";
        }

        // If it's a String or complex type, clone
        if (NeedsClone(sourceType))
        {
            return $"{sourceExpr}.clone()";
        }

        return sourceExpr;
    }

    /// <summary>
    /// Détermine si un type Rust nécessite un `.clone()` pour être copié.
    /// </summary>
    private static bool NeedsClone(string rustType)
    {
        // Primitive types that implement Copy don't need clone
        var copyTypes = new HashSet<string>
        {
            "i8", "i16", "i32", "i64", "i128",
            "u8", "u16", "u32", "u64", "u128",
            "f32", "f64",
            "bool", "char",
            "isize", "usize"
        };

        var baseType = rustType.StartsWith("Option<") && rustType.EndsWith(">")
            ? rustType[7..^1]
            : rustType;

        return !copyTypes.Contains(baseType);
    }
}
