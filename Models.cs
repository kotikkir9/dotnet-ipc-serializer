public class MethodCall
{
    public string Type { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public object?[]? Arguments { get; set; } = [];
}

public class MethodResponse
{
    public object Value { get; set; } = new();
    public string? Error { get; set; }
}
