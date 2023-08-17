using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;
using Microsoft.SemanticKernel.Skills.Web;
using Microsoft.SemanticKernel.Skills.Web.Bing;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;

namespace SemanticKernelLab
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            string azureOpenAiServiceEndpoint = configuration.GetSection("AzureOpenAiServiceEndpoint").Value;
            string azureOpenAiServiceKey = configuration.GetSection("AzureOpenAiServiceKey").Value;
            string azureOpenAiServiceDeploymentName = configuration.GetSection("AzureOpenAiServiceDeploymentName").Value;
            string bingSearchServiceKey = configuration.GetSection("BingSearchServiceKey").Value;

            //Create Kernel builder
            var builder = new KernelBuilder();


            // Configure AI service credentials used by the kernel
            builder.WithAzureChatCompletionService(
                     azureOpenAiServiceDeploymentName,                  // Azure OpenAI Deployment Name
                     azureOpenAiServiceEndpoint, // Azure OpenAI Endpoint
                     azureOpenAiServiceKey);      // Azure OpenAI Key

            IKernel kernel = builder.Build();
            IKernel bingSearchKernel = builder.Build();
            IKernel chatGptKernel = builder.Build();

            // Load the Skills Directory
            var skillsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "skills");

            // #1
            // Load the FunSkill from the Skills Directory
            var funSkillFunctions = kernel.ImportSemanticSkillFromDirectory(skillsDirectory, "FunSkill");

            string funInput = "programmer and fishing";

            // Run the Function called Joke
            var funResult = await funSkillFunctions["Joke"].InvokeAsync(funInput);

            // #2
            // Load the ExcusesSkill from the Skills Directory
            var excusesSkillFunctions = kernel.ImportSemanticSkillFromDirectory(skillsDirectory, "FunSkill");

            string excusesInput = "I'm late to my online meeting";

            // Run the Function called Excuses
            var excusesResult = await excusesSkillFunctions["Excuses"].InvokeAsync(excusesInput);

            // #3
            // Load the SummarizationSkill from the Skills Directory
            var summarizationSkillFunctions = kernel.ImportSemanticSkillFromDirectory(skillsDirectory, "SummarizeSkill");

            string textToSummarizeFilename = "TheUglyDuckling.txt";

            //Load text to summarize from file on disk
            string textToSummarize = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "Data", textToSummarizeFilename));

            // Run the Function called Summarize
            var summarizeResult = await summarizationSkillFunctions["Summarize"].InvokeAsync(textToSummarize);


            // #4
            // Bing Search
            var bingConnector = new BingConnector(bingSearchServiceKey);
            // Load Bing skill
            bingSearchKernel.ImportSkill(new WebSearchEngineSkill(bingConnector), "bing");

            var bingQuestion = "What is quantum computing?";
            var bingResult = await bingSearchKernel.Func("bing", "search").InvokeAsync(bingQuestion);

            // #5

            // Get AI service instance used to manage the user chat
            var chatGPT = chatGptKernel.GetService<IChatCompletion>();

            var systemMessage = "You are a chat bot that answers user's questions";

            var chat = (OpenAIChatHistory)chatGPT.CreateNewChat(systemMessage);

            // 1. Ask the user for a message. The user enters a message.  Add the user message into the Chat History object.
            var userMessage = "What is the highest mountain in the world?";
            chat.AddUserMessage(userMessage);

            // 2. Send the chat object to AI asking to generate a response. Add the bot message into the Chat History object.
            string assistantReply = await chatGPT.GenerateMessageAsync(chat, new ChatRequestSettings());
            chat.AddAssistantMessage(assistantReply);

            // 3. Ask another question. Add the user message into the Chat History object.
            var userMessage2 = "How far it is from the Unitest States?";
            chat.AddUserMessage(userMessage2);

            // 4. Send the chat object to AI asking to generate a response. Add the bot message into the Chat History object.
            string assistantReply2 = await chatGPT.GenerateMessageAsync(chat, new ChatRequestSettings());
            chat.AddAssistantMessage(assistantReply2);

            // 5. Ask another question. Add the user message into the Chat History object.
            var userMessage3 = "If I'm a United States citizen do I need a visa to go there?";
            chat.AddUserMessage(userMessage3);

            // 6. Send the chat object to AI asking to generate a response. Add the bot message into the Chat History object.
            string assistantReply3 = await chatGPT.GenerateMessageAsync(chat, new ChatRequestSettings());
            chat.AddAssistantMessage(assistantReply3);

            // Return the result to the Notebook
            Console.WriteLine($"Fun Skill: [{funInput}]\n----------------------------------------------------\n{funResult}\n");
            Console.WriteLine($"Excuses Skill: [{excusesInput}]\n----------------------------------------------------\n{excusesResult}\n");
            Console.WriteLine($"Summarization Skill: Filename: [{textToSummarizeFilename}]\n----------------------------------------------------\n{summarizeResult}\n");
            Console.WriteLine($"Bing Search: Question: [{bingQuestion}]\n----------------------------------------------------\n{bingResult}\n");
            Console.WriteLine($"ChatGPT with Symantic Kernel (with memory):\n----------------------------------------------------\nUser Message: [{userMessage}]\nChat GPT Reply: {assistantReply}\nUser Message: [{userMessage2}]\nChat GPT Reply: {assistantReply2}\nUser Message: [{userMessage3}]\nChat GPT Reply: {assistantReply3}\n");




        }
    }
}