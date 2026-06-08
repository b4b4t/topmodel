using Microsoft.Extensions.DependencyInjection;
using TopModel.Generator.Core;

namespace TopModel.Generator.Translation;

public class GeneratorRegistration : IGeneratorRegistration<TranslationConfig>
{
    /// <inheritdoc cref="IGeneratorRegistration{T}.Register" />
    public void Register(IServiceCollection services, TranslationConfig config, int number)
    {
        if(config.TranslationType == "json") 
        {
            services.AddGenerator<JsonTranslationGenerator, TranslationConfig>(config, number);
        }
        else
        {
            services.AddGenerator<TranslationOutGenerator, TranslationConfig>(config, number);
        }
    }
}
