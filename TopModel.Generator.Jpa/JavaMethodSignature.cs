using TopModel.Utils;

namespace TopModel.Generator.Jpa;

public class JavaMethodSignature
{
    public JavaMethodSignature(string returnType, string name)
    {
        Name = name;
        ReturnType = returnType;
    }

    public JavaMethodSignature(string import, string returnType, string name)
    {
        Name = name;
        ReturnType = returnType;
        Imports.Add(import);
    }

    public JavaMethodSignature(string visibility, string import, string returnType, string name)
    {
        Name = name;
        ReturnType = returnType;
        Visibility = $"{visibility.Trim()} ";
        Imports.Add(import);
    }

    public List<string> Imports { get; } = new();

    private string Name { get; }

    private string Visibility { get; } = string.Empty;

    private string ReturnType { get; }

    private List<string> GenericTypes { get; } = new();

    private List<JavaMethodParameter> Parameters { get; } = new();

    public JavaMethodSignature AddGenericType(string type)
    {
        GenericTypes.Add(type);
        return this;
    }

    public JavaMethodSignature AddParameter(JavaMethodParameter parameter)
    {
        Imports.AddRange(parameter.Imports);
        Parameters.Add(parameter);
        return this;
    }

    public override string ToString()
    {
        return $@"{Visibility}{(GenericTypes.Count() > 0 ? $"{string.Join(", ", GenericTypes)}" : string.Empty)}{ReturnType} {Name}({string.Join(", ", Parameters)})";
    }
}