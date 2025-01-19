﻿using Microsoft.OpenApi.Models;
using NuGet.Packaging;

namespace TopModel.ModelGenerator.OpenApi;

public static class OpenApiUtils
{
    public static string Format(this string? description)
    {
        if (description == null)
        {
            return string.Empty;
        }

        if (description.Contains('"') || description.Contains('\n') || description.Contains(':') || description.Contains('#'))
        {
            var lines = description.ReplaceLineEndings().Split(Environment.NewLine);
            if (string.IsNullOrWhiteSpace(lines.Last()))
            {
                lines = lines.SkipLast(1).ToArray();
            }

            return $"|{Environment.NewLine}{string.Join(Environment.NewLine, lines.Select(line => $"        {line}"))}";
        }
        else
        {
            return description;
        }
    }

    public static string GetDomain(this OpenApiConfig config, string name, OpenApiSchema schema)
    {
        var resolvedDomain = TmdGenUtils.GetDomainString(config.Domains, name: name);
        if (resolvedDomain == name)
        {
            return GetDomainSchema(config, schema);
        }

        return resolvedDomain;
    }

    public static IDictionary<string, OpenApiSchema> GetProperties(this OpenApiSchema schema)
    {
        if (schema.Type == "array")
        {
            return schema.Items.GetProperties();
        }

        return schema.Properties
            .Concat(schema.AllOf.Where(a => a.Type == "object").SelectMany(a => a.Properties))
            .ToDictionary(a => a.Key, a => a.Value);
    }

    public static OpenApiSchema? GetRequestBodySchema(this OpenApiOperation operation)
    {
        var schema = operation.RequestBody?.Content.First().Value.Schema;
        if (schema != null && schema.Reference == null)
        {
            schema.Reference = operation.RequestBody?.Reference;
        }

        return schema;
    }

    public static IDictionary<string, OpenApiSchema> GetSchemas(this OpenApiDocument model, HashSet<string>? references = null)
    {
        var schemas = model.Components.Schemas;
        foreach (var s in model.Components.RequestBodies.ToDictionary(r => r.Key, r =>
        {
            var schema = r.Value.Content.First().Value.Schema;
            if (schema.Reference == null)
            {
                schema.Reference = r.Value.Reference;
            }

            return schema;
        }))
        {
            if (!schemas.ContainsKey(s.Key))
            {
                schemas.Add(s);
            }
        }

        foreach (var s in model.Paths
            .SelectMany(p => p.Value.Operations.Where(o => o.Value.Tags.Any()))
            .Select(o => o.Value)
            .Where(o => o.RequestBody != null)
            .ToDictionary(r => $"{r.OperationId}Body", r => r.RequestBody.Content.First().Value.Schema))
        {
            if (!schemas.ContainsKey(s.Key))
            {
                schemas.Add(s);
            }
        }

        return schemas.Where(s =>
             s.Value.Type == "object"
             || s.Value.AllOf.Any() && s.Value.AllOf.All(a => a.Type == "object" || a.Reference != null)
             || s.Value.Type == "array" && s.Value.Items.Type == "object"
             || s.Value.AnyOf.Any()
             || s.Value.OneOf.Any()
             || s.Value.Type == "string" && s.Value.Enum.Any())
         .Where(s => references == null || references.Contains(s.Key))
         .ToDictionary(a => a.Key, a => a.Value);
    }

    public static string Unplurialize(this string name)
    {
        return name.EndsWith("ies") ? $"{name[..^3]}y" : name.TrimEnd('s');
    }

    private static string GetDomainCore(this OpenApiSchema schema)
    {
        var length = schema.MaxLength != null ? $"{schema.MaxLength}" : string.Empty;

        if (schema.Format != null)
        {
            return schema.Format + length;
        }
        else if (schema.Type == "array")
        {
            return $"{GetDomainCore(schema.Items)}-array";
        }
        else if (schema.Type == "object" && schema.AdditionalProperties != null)
        {
            return $"{GetDomainCore(schema.AdditionalProperties)}-map";
        }

        return schema.Type + length;
    }

    private static string GetDomainSchema(this OpenApiConfig config, OpenApiSchema schema)
    {
        var domain = GetDomainCore(schema);
        return TmdGenUtils.GetDomainString(config.Domains, type: domain);
    }
}
