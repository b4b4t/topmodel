using System.Collections.ObjectModel;

namespace TopModel.Generator.Jpa;

public class JavaMethodParameter
{
    public JavaMethodParameter(string type, string name)
    {
        this.Name = name;
        this.Type = type;
    }

    public JavaMethodParameter(string import, string type, string name)
    {
        this.Name = name;
        this.Type = type;
        Imports.Add(import);
    }

    public string Name { get; }

    public string Type { get; }

    public List<string> Imports { get; } = new();

    public List<JavaAnnotation> Annotations { get; } = new();

    public JavaMethodParameter AddAnnotation(JavaAnnotation annotation)
    {
        Imports.AddRange(annotation.Imports);
        Annotations.Add(annotation);
        return this;
    }


    public JavaMethodParameter AddAnnotations(IEnumerable<JavaAnnotation> annotations)
    {
        foreach (var a in annotations)
        {
            AddAnnotation(a);
        }

        return this;
    }

    public override string ToString()
    {
        return $@"{string.Join(' ', Annotations)} {Type} {Name}";
    }
}