﻿using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.FileModel;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Jpa;

/// <summary>
/// Générateur des objets de traduction javascripts.
/// </summary>
public class FeignClientApiGenerator : SpringServerApiGenerator
{
    private readonly ILogger<FeignClientApiGenerator> _logger;

    public FeignClientApiGenerator(ILogger<FeignClientApiGenerator> logger)
        : base(logger)
    {
        _logger = logger;
    }

    public override string Name => "FeignClientApiGen";

    protected override bool FilterTag(string tag)
    {
        return Config.ResolveVariables(Config.ApiGeneration!, tag) == ApiGeneration.Client && Config.ResolveVariables(Config.ClientApiGeneration!, tag) == ClientApiMode.FeignClient;
    }

    protected override IEnumerable<JavaAnnotation> GetClassAnnotations(ModelFile file)
    {
        var fileName = file.Options.Endpoints.FileName;
        foreach (var a in base.GetClassAnnotations(file))
        {
            yield return a;
        }

        yield return new JavaAnnotation("FeignClient", "org.springframework.cloud.openfeign.FeignClient")
                        .AddAttribute("name", $@"""{file.Namespace.RootModule}""")
                        .AddAttribute("contextId", $@"""{GetClassName(fileName)}""");
    }

    protected override string GetClassName(string fileName)
    {
        return $"{fileName.ToPascalCase()}Api";
    }
}
