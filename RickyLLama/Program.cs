// See https://aka.ms/new-console-template for more information
using OllamaSharp;
using OllamaSharp.Models.Chat;
using RickyLLama;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;

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

var actions = new List<RickyLLama.Action>()
{
    new RickyLLama.Action(){ActionName = "File.Move", Description = "Allows the user to move a file to another folder."},
    new RickyLLama.Action(){ActionName = "Base.Comment", Description = "Allows the user to enter a comment to the flow."}
};

var moveFileExamples = """
    Here are some example statements for action File.Move
    File.Move Files: ['C:\\Users\\Ricky\\Documents\\calculations.xlsx'] Destination: $'''C:\\temp2''' IfFileExists: File.IfExists.DoNothing MovedFiles=> MovedFiles
    File.Move Files: ['C:\\Users\\Ricky\\Documents\\calculations.xlsx', 'C:\\Users\\Ricky\\Documents\\KS Cover letter.docx'] Destination: $'''C:\\temp''' IfFileExists: File.IfExists.DoNothing MovedFiles=> MovedFiles
    File.Move Files: ['C:\\Users\\Ricky\\Documents\\calculations.xlsx'] Destination: $'''C:\\temp2''' IfFileExists: File.IfExists.Overwrite MovedFiles=> MovedFiles
    File.Move Files: ['C:\\Users\\Ricky\\Documents\\calculations.xlsx', 'C:\\Users\\Ricky\\Documents\\KS Cover letter.docx'] Destination: $'''C:\\temp''' IfFileExists: File.IfExists.Overwrite MovedFiles=> MovedFiles
    """;

var commentSamples = """
    Here is an example statement for comment
    # This is a sample comment
    """;

var findActionPrompt = $"""
    You are an expert on identifying which actions are needed to accomplish the needed task.
    To identify the available actions use the following json definition:
    {JsonSerializer.Serialize(actions)}
    Only use actions that already exist in the json definition and don't generate new ones.
    For any user intention that doesn't exist in the json definition return the comment action.
    Return a comma seperated list of ActionName from the json definition".
    For each action you select explain why you selected it.
    """;

//Your response should contain **ONLY** the list of actions and nothing else
//For each action you select explain why you selected it.

var phaseOneUserPrompt = """
    Move file C:\file1.txt to folder C:\NewFolder if the file already exists don't overwrite it then move C:\file2.txt to C:\NewFolder2 if the file already exists overwrite then Delete folder C:\Temp
    """;

var phaseOneResponse = "";


var findExamplePrompt = $"""
    You are an expert on selecting the correct set of statements needed based on what the user tries to achieve.
    The end goal is to combine all statements to produce a script
    The task the user tries to accomplish is:
    {phaseOneUserPrompt}

    Return only statements that exist below and you are certain that they match the user task, never generate new statements that don't exist below.
    If you are not 100% which statement to select or the statement you want to use doesn't exist below then use comment statement with the appropriate text without any note or explaination.
    Update the statements with the user provided data. Make sure you keep the original syntax for the parameters of the statement as this is a custom scripting language.
    When you want to use a string literal use the syntax $'''sample text here''' like it is in the sample statements below
    Respond ONLY with the statements and nothing else.
    Don't use markdown for your response.

    The available statements are:

    {moveFileExamples}

    {commentSamples}

    
    """;

var phaseTwoResponse = "";



//await foreach (var answerToken in ollama.Generate(new OllamaSharp.Models.GenerateRequest() { System = findActionPrompt, Prompt = phaseOneUserPrompt, Model = ollama.SelectedModel,Options = new OllamaSharp.Models.RequestOptions() {Temperature = 0 } }))
//{
//    phaseOneResponse += answerToken.Response;
//    Console.Write(answerToken.Response);
//}
//Console.Write(Environment.NewLine);

await foreach (var answerToken in ollama.Generate(new OllamaSharp.Models.GenerateRequest() { Prompt = findExamplePrompt, Model = ollama.SelectedModel, Options = new OllamaSharp.Models.RequestOptions() { Temperature = 0 } }))
{
    phaseTwoResponse += answerToken.Response;
    Console.Write(answerToken.Response);
}
Console.Write(Environment.NewLine);

// messages including their roles and tool calls will automatically be tracked within the chat object
// and are accessible via the Messages property



Console.ReadKey();