namespace TopModel.Generator.Jpa;

public class JavaAnnotation
{
    public JavaAnnotation(string name, string import)
    {
        Name = name.Trim('@');
        Imports = [import];
    }

    public JavaAnnotation(string name, IEnumerable<string> imports)
    {
        Name = name.Trim('@');
        Imports.AddRange(imports);
    }

    public JavaAnnotation(string name, string value, string import)
    {
        Name = name.Trim('@');
        Imports = [import];
        Attributes.Add(("value", value));
    }

    public string Name { get; set; }

    public List<string> Imports { get; set; } = new();

    private List<(string Name, object Value)> Attributes { get; } = new();

    public JavaAnnotation AddAttribute(string name, string value, string import)
    {
        Attributes.Add((name, value));
        Imports.Add(import);
        return this;
    }

    public JavaAnnotation AddAttribute(string name, string value)
    {
        Attributes.Add((name, value));
        return this;
    }

    public JavaAnnotation AddAttribute(string value)
    {
        Attributes.Add(("value", value));
        return this;
    }

    public JavaAnnotation AddAttribute(string name, JavaAnnotation value)
    {
        Attributes.Add((name, value));
        Imports.AddRange(value.Imports);
        return this;
    }

    public override string ToString()
    {
        var name = Name.StartsWith('@') ? Name : $"@{Name}";
        if (!Attributes.Any())
        {
            return name;
        }
        else if (Attributes.Count() == 1 && Attributes.Any(a => a.Name == "value"))
        {
            return $"{name}({Attributes.First().Value})";
        }
        else
        {
            var attributes = string.Join(", ", Attributes.Select(a => $"{a.Name} = {a.Value}"));
            return $"{name}({attributes})";
        }
    }
}