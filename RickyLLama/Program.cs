// See https://aka.ms/new-console-template for more information
using OllamaSharp;
using OllamaSharp.Models.Chat;

Console.WriteLine("Hello, World!");
// set up the client
var uri = new Uri("http://localhost:11434");
var ollama = new OllamaApiClient(uri);

// select a model which should be used for further operations
ollama.SelectedModel = "llama3.1:8b";

var models = await ollama.ListLocalModels();

//await foreach (var status in ollama.PullModel("llama3.1:8b"))
//    Console.WriteLine($"{status.Percent}% {status.Status}");

//await foreach (var stream in ollama.Generate("How are you today?"))
//    Console.Write(stream.Response);

var tool = new Tool()
{
    Function = new Function
    {
        Description = "Get the current weather for a location",
        Name = "GetWeather",
        Parameters = new Parameters
        {
            Properties = new Dictionary<string, Properties>
            {
                ["location"] = new()
                {
                    Type = "string",
                    Description = "The location to get the weather for, e.g. San Francisco, CA"
                },
                ["format"] = new()
                {
                    Type = "string",
                    Description = "The format to return the weather in, e.g. 'celsius' or 'fahrenheit'",
                    Enum = ["celsius", "fahrenheit"]
                },
            },
            Required = ["location", "format"],
        }
    },
    Type = "function"
};

string GetWeather(string location, string format)
{
    //Call the weather API here
    return $"The weather in {location} is 25 degrees {format}";
}


var chat = new Chat(ollama);
while (true)
{
    var message = Console.ReadLine();
    await foreach (var answerToken in chat.Send(message, tools: [tool]))
    {
        Console.Write(answerToken);
    }
    Console.Write(Environment.NewLine);
    //Check the latest message to see if a tool was called
    foreach (var toolCall in chat.Messages.Last().ToolCalls)
    {
        var arguments = string.Join(",", toolCall.Function.Arguments.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
        Console.WriteLine(GetWeather(toolCall.Function.Arguments["location"], toolCall.Function.Arguments["format"]));
    }
}

// messages including their roles and tool calls will automatically be tracked within the chat object
// and are accessible via the Messages property



Console.ReadKey();