﻿using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.FileModel;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Jpa;

/// <summary>
/// Générateur des objets de traduction javascripts.
/// </summary>
public class SpringClientApiGenerator : EndpointsGeneratorBase<JpaConfig>
{
    private readonly ILogger<SpringClientApiGenerator> _logger;

    public SpringClientApiGenerator(ILogger<SpringClientApiGenerator> logger)
        : base(logger)
    {
        _logger = logger;
    }

    public override string Name => "SpringApiClientGen";

    protected override bool FilterTag(string tag)
    {
        return Config.ResolveVariables(Config.ApiGeneration!, tag) == ApiGeneration.Client;
    }

    protected override string GetFilePath(ModelFile file, string tag)
    {
        return Path.Combine(Config.GetApiPath(file, tag), $"{GetClassName(file.Options.Endpoints.FileName)}.java");
    }

    protected override void HandleFile(string filePath, string fileName, string tag, IList<Endpoint> endpoints)
    {
        var className = GetClassName(fileName);
        var packageName = Config.GetPackageName(endpoints.First(), tag);
        using var fw = new JavaWriter(filePath, _logger, packageName, null);

        WriteImports(endpoints, fw, tag);
        fw.WriteLine();
        if (endpoints.First().ModelFile.Options.Endpoints.Prefix != null)
        {
            fw.WriteLine($@"@HttpExchange(""{endpoints.First().ModelFile.Options.Endpoints.Prefix}"")");
            fw.AddImport("org.springframework.web.service.annotation.HttpExchange");
        }

        var javaxOrJakarta = Config.PersistenceMode.ToString().ToLower();
        if (Config.GeneratedHint)
        {
            fw.AddImport($"{javaxOrJakarta}.annotation.Generated");
            fw.WriteLine("@Generated(\"TopModel : https://github.com/klee-contrib/topmodel\")");
        }

        fw.WriteLine($"public interface {className} {{");

        fw.WriteLine();

        foreach (var endpoint in endpoints)
        {
            WriteEndpoint(fw, endpoint, tag);
        }

        fw.WriteLine("}");
    }

    private static string GetClassName(string fileName)
    {
        return $"{fileName.ToPascalCase()}Client";
    }

    private IEnumerable<string> GetTypeImports(IEnumerable<Endpoint> endpoints, string tag)
    {
        var properties = endpoints.SelectMany(endpoint => endpoint.Params)
            .Concat(endpoints.Where(endpoint => endpoint.Returns is not null)
            .Select(endpoint => endpoint.Returns));
        return properties.SelectMany(property => property!.GetTypeImports(Config, tag))
                .Concat(endpoints.Where(endpoint => endpoint.Returns is not null)
                .Select(e => e.Returns).OfType<CompositionProperty>()
                .SelectMany(c => c.GetKindImports(Config, tag)));
    }

    private void WriteEndpoint(JavaWriter fw, Endpoint endpoint, string tag)
    {
        fw.WriteLine();
        fw.WriteDocStart(1, endpoint.Description);

        foreach (var param in endpoint.Params)
        {
            fw.WriteLine(1, $" * @param {param.GetParamName()} {param.Comment}");
        }

        if (endpoint.Returns != null)
        {
            fw.WriteLine(1, $" * @return {endpoint.Returns.Comment}");
        }

        fw.WriteLine(1, " */");
        var returnType = "void";

        if (endpoint.Returns != null)
        {
            returnType = Config.GetType(endpoint.Returns);
        }

        {
            var accept = string.Empty;
            if (endpoint.Returns != null && endpoint.Returns.Domain?.MediaType != null)
            {
                accept = @$", accept = {{ ""{endpoint.Returns.Domain.MediaType}"" }}";
            }

            var contentType = string.Empty;
            if (endpoint.Params.Any(p => p.Domain?.MediaType != null))
            {
                contentType = @$", contentType = {string.Join(", ", endpoint.Params.Where(p => p.Domain?.MediaType != null).Select(p => $@"""{p.Domain.MediaType}""")).First()} ";
            }

            foreach (var annotation in Config.GetDecoratorAnnotations(endpoint, tag))
            {
                fw.WriteLine(1, $"{(annotation.StartsWith("@") ? string.Empty : "@")}{annotation}");
            }

            fw.WriteLine(1, @$"@{endpoint.Method.ToPascalCase(true)}Exchange(value = ""{endpoint.Route}""{contentType}{accept})");
        }

        var methodParams = new List<string>();
        foreach (var param in endpoint.GetRouteParams())
        {
            var pathParamAnnotation = @$"@PathVariable(""{param.GetParamName()}"")";

            fw.AddImport("org.springframework.web.bind.annotation.PathVariable");
            fw.AddImports(Config.GetDomainImports(param, tag));
            var decoratorAnnotations = string.Join(' ', Config.GetDomainAnnotations(param, tag).Select(a => a.Annotation).Select(a => a.StartsWith("@") ? a : "@" + a));
            methodParams.Add($"{pathParamAnnotation}{(decoratorAnnotations.Length > 0 ? $" {decoratorAnnotations}" : string.Empty)} {Config.GetType(param)} {param.GetParamName()}");
        }

        foreach (var param in endpoint.GetQueryParams())
        {
            var ann = string.Empty;
            ann += @$"@RequestParam(value = ""{param.GetParamName()}"", required = {param.Required.ToString().ToFirstLower()}) ";
            fw.AddImport("org.springframework.web.bind.annotation.RequestParam");
            fw.AddImports(Config.GetDomainImports(param, tag));
            var decoratorAnnotations = string.Join(' ', Config.GetDomainAnnotations(param, tag).Select(a => a.Annotation).Select(a => a.StartsWith("@") ? a : "@" + a));
            methodParams.Add($"{ann}{(decoratorAnnotations.Length > 0 ? $" {decoratorAnnotations}" : string.Empty)}{Config.GetType(param)} {param.GetParamName()}");
        }

        if (endpoint.IsMultipart)
        {
            foreach (var param in endpoint.Params.Where(param => param is CompositionProperty || (param.Domain?.BodyParam ?? false) || (param.Domain?.IsMultipart ?? false)))
            {
                var ann = string.Empty;

                if (param is CompositionProperty cp)
                {
                    fw.AddImport("org.springframework.web.bind.annotation.RequestPart");
                    fw.AddImport("org.springframework.util.MultiValueMap");
                    ann += @$"@RequestPart ";
                    methodParams.Add($"{ann}MultiValueMap<K, V> {param.GetParamName()}");
                }
                else
                {
                    ann += @$"@RequestPart(value = ""{param.GetParamName()}"", required = {param.Required.ToString().ToFirstLower()}) ";
                    fw.AddImport("org.springframework.web.bind.annotation.RequestPart");
                    methodParams.Add($"{ann}{Config.GetType(param)} {param.GetParamName()}");
                }
            }
        }
        else
        {
            var bodyParam = endpoint.GetJsonBodyParam();
            if (bodyParam != null)
            {
                var ann = string.Empty;
                ann += @$"@RequestBody @Valid ";
                fw.AddImport("org.springframework.web.bind.annotation.RequestBody");
                fw.AddImport(Config.PersistenceMode.ToString().ToLower() + ".validation.Valid");
                methodParams.Add($"{ann}{Config.GetType(bodyParam)} {bodyParam.GetParamName()}");
            }
        }

        fw.WriteLine(1, $"{(endpoint.IsMultipart ? "<K, V> " : string.Empty)}{returnType} {endpoint.NameCamel}({string.Join(", ", methodParams)});");
    }

    private void WriteImports(IEnumerable<Endpoint> endpoints, JavaWriter fw, string tag)
    {
        fw.AddImports(endpoints.Select(e => $"org.springframework.web.service.annotation.{e.Method.ToPascalCase(true)}Exchange"));
        fw.AddImports(GetTypeImports(endpoints, tag));
        fw.AddImports(endpoints.SelectMany(e => Config.GetDecoratorImports(e, tag)));
    }
}
