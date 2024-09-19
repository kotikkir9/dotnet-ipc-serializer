using System.IO.Pipes;
using System.Reflection;
using System.Text.Json;

if (args.Length == 0)
{
    while (true)
    {
        using var server = new NamedPipeServerStream("xexe", PipeDirection.InOut);
        Console.WriteLine("Waiting for connection...");
        server.WaitForConnection();

        try
        {
            using var reader = new StreamReader(server);
            using var writer = new StreamWriter(server);

            var jsonRequest = reader.ReadLine();

            if (string.IsNullOrWhiteSpace(jsonRequest))
                throw new Exception("Invalid client request.");

            Console.WriteLine($"Server received: {jsonRequest}");

            var methodCall = JsonSerializer.Deserialize<MethodCall>(jsonRequest);

            if (methodCall == null)
                throw new Exception("Failed to Deserialize \"MethodCall\".");

            // Dummy value for now.
            MethodResponse response = new()
            {
                Value = 69
            };

            // TODO: Find and exec method instead of the dummy response.

            string jsonResponse = JsonSerializer.Serialize(response);
            writer.WriteLine(jsonResponse);
            writer.Flush();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
else
{
    var proxy = IpcProxy<ICalculator>.Create();
    var result = proxy.Add(23, 69);
    Console.WriteLine($"Method \"Add\" returned: {result}");
}

class IpcProxy<T> : DispatchProxy where T : class
{
    private readonly string _pipeName = "xexe";

    public static T Create()
    {
        object proxy = Create<T, IpcProxy<T>>();
        return (T)proxy;
    }

    protected override object? Invoke(MethodInfo? methodInfo, object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(methodInfo);
        ArgumentNullException.ThrowIfNull(methodInfo.DeclaringType);

        var methodCall = new MethodCall
        {
            Type = methodInfo.DeclaringType.Name,
            Method = methodInfo.Name,
            Arguments = args
        };

        object returnValue = CallMethod(methodCall);

        if (methodInfo.ReturnType == typeof(void))
            return default;

        return JsonSerializer.Deserialize(returnValue.ToString()!, methodInfo.ReturnType);
    }

    public object CallMethod(MethodCall methodCall)
    {
        using var client = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut);
        client.Connect();

        using var reader = new StreamReader(client);
        using var writer = new StreamWriter(client);

        string jsonMethodcall = JsonSerializer.Serialize(methodCall);
        writer.WriteLine(jsonMethodcall);
        writer.Flush();

        var jsonResponse = reader.ReadLine();

        if (string.IsNullOrWhiteSpace(jsonResponse))
            throw new Exception("Received invalid reponse from the server.");

        var response = JsonSerializer.Deserialize<MethodResponse>(jsonResponse);

        if (response == null)
            throw new Exception("Failed to deserialize response object");

        if (response.Error != null)
            throw new Exception(response.Error);

        return response.Value;
    }
}