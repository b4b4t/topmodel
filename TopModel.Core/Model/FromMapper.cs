﻿using OneOf;

namespace TopModel.Core;

public class FromMapper
{
    public string? Comment { get; set; }

    public List<OneOf<ClassMappings, PropertyMapping>> Params { get; } = [];

    public IEnumerable<ClassMappings> ClassParams => Params.Where(p => p.IsT0).Select(p => p.AsT0);

    public IEnumerable<PropertyMapping> PropertyParams => Params.Where(p => p.IsT1).Select(p => p.AsT1);

#nullable disable
    public Class Class { get; set; }

    internal LocatedString Reference { get; set; }
}
