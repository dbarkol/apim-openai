using Azure.AI.OpenAI;
using Azure.Core.Pipeline;
using Azure.Identity;
using dotenv.net;

public static class Program
{
    public static async Task Main(string[] args)
    {
        // Run OpenAI SDK that uses the APIM endpoint sample
        await OpenAIWithApimSample();
    }

    private static async Task OpenAIWithApimSample()
    {
        // Load environment variables from .env file.
        DotEnv.Load(options: new DotEnvOptions(
            ignoreExceptions: true,
        envFilePaths: new[] { ".env" }));

        var azureOpenAIChatDeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME");
        var azureApimEndpoint = Environment.GetEnvironmentVariable("AZURE_APIM_ENDPOINT");
        var azureApimKey = Environment.GetEnvironmentVariable("AZURE_APIM_KEY");

        // Instantiate HttpClient so that we can add default headers to the request.
        // If the APIM endpoint requires a subscription key, add it as a default header
        // along with any other headers that are required.
        var httpClient = new HttpClient();        
        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", azureApimKey);
        httpClient.DefaultRequestHeaders.Add("TestKey", "TestValue");        
        
        // Include the HttpClient in the OpenAI client options so that the default
        // headers are included in the request.
        var clientOptions = new OpenAIClientOptions
        {
            Transport = new HttpClientTransport(httpClient)
        };

        // Create the OpenAI client using the Azure APIM endpoint and Azure credential.
        // If the APIM endpoint is null, throw an exception.
        var client = azureApimEndpoint != null 
            ? new OpenAIClient(new Uri(azureApimEndpoint), new DefaultAzureCredential(), clientOptions) 
            : throw new ArgumentNullException(nameof(azureApimEndpoint));

        // Create a simple chat request
        ChatCompletionsOptions options = new ChatCompletionsOptions()
        {            
            DeploymentName = azureOpenAIChatDeploymentName,            
            Messages = {
                new ChatMessage(ChatRole.System, "You are helpful assistant."),
                new ChatMessage(ChatRole.User, "Hello, how are you?"),
            }
        };

        // Send the chat completion request and output the response
        var responses = await client.GetChatCompletionsAsync(options);
        var chatMessage = responses.Value.Choices[0].Message;
        Console.WriteLine(chatMessage.Content);   
    }
}


