using TopModel.Utils;

namespace TopModel.Generator.Jpa;

public class JavaMethod
{
    public JavaMethod(string returnType, string name)
    {
        Name = name;
        ReturnType = returnType;
    }

    public JavaMethod(string import, string returnType, string name)
    {
        Name = name;
        ReturnType = returnType;
        Imports.Add(import);
    }

    public List<JavaAnnotation> Annotations { get; } = new();

    public List<WriterLine> Body { get; } = new();

    public List<string> Imports { get; } = new();

    public string Signature => $@"{_visibility}{(GenericTypes.Count() > 0 ? $"<{string.Join(", ", GenericTypes)}> " : string.Empty)}{ReturnType} {Name}({string.Join(", ", Parameters.Select(p => p.Declaration))})";

    public string Visibility { get => this._visibility.Trim(); set => _visibility = string.IsNullOrEmpty(value) ? string.Empty : $"{value} "; }

    private string _visibility { get; set; } = string.Empty;

    private string Name { get; }

    private string ReturnType { get; }

    private List<string> GenericTypes { get; } = new();

    private List<JavaMethodParameter> Parameters { get; } = new();

    public JavaMethod AddAnnotation(JavaAnnotation annotation)
    {
        Imports.AddRange(annotation.Imports);
        Annotations.Add(annotation);
        return this;
    }

    public JavaMethod AddBodyLine(string line)
    {
        Body.Add(new WriterLine() { Line = line, Indent = 0 });
        return this;
    }

    public JavaMethod AddBodyLine(int indentationLevel, string line)
    {
        Body.Add(new WriterLine() { Line = line, Indent = indentationLevel });
        return this;
    }

    public JavaMethod AddGenericType(string type)
    {
        GenericTypes.Add(type);
        return this;
    }

    public JavaMethod AddParameter(JavaMethodParameter parameter)
    {
        Imports.AddRange(parameter.Imports);
        Parameters.Add(parameter);
        return this;
    }
}
