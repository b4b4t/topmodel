using Microsoft.Extensions.DependencyInjection;
using TopModel.Generator.Core;
using static TopModel.Utils.ModelUtils;

namespace TopModel.Generator.Rust;

/// <summary>
/// Enregistrement des générateurs Rust auprès du conteneur d'injection de dépendances.
/// </summary>
public class GeneratorRegistration : IGeneratorRegistration<RustConfig>
{
    /// <inheritdoc cref="IGeneratorRegistration{T}.Register" />
    public void Register(IServiceCollection services, RustConfig config, int number)
    {
        TrimSlashes(config, c => c.NonPersistentModelPath);
        TrimSlashes(config, c => c.PersistentModelPath);
        TrimSlashes(config, c => c.ReferencesModelPath);
        TrimSlashes(config, c => c.MapperRootPath);

        services.AddGenerator<RustClassGenerator, RustConfig>(config, number);
        services.AddGenerator<RustMapperGenerator, RustConfig>(config, number);
    }
}
