using Microsoft.Extensions.Configuration;

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
        }
    }
}