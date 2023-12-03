using Azure.AI.OpenAI;
using Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DotnetHackathon
{
    internal class Client
    {
        OpenAIClient OpenAiclient;
        ChatCompletionsOptions ChatCompletionsOptions;

        public Client() {
            string proxyUrl = "https://aoai.hacktogether.net";
            string key = "3f5dd857-024d-40a6-87a6-3913c1ab63d1";
            Uri proxyUri = new(proxyUrl + "/v1/api");
            AzureKeyCredential token = new(key + "/Pablo10544");
            OpenAiclient = new(proxyUri, token);
            ChatCompletionsOptions = new()
            {
                MaxTokens = 400,
                Temperature = 0.85f,
                NucleusSamplingFactor = 0.95f,
                DeploymentName = "gpt-35-turbo"
            };    
        }
        public void AddMessage(string message, ChatRole chatRole) {
            ChatCompletionsOptions.Messages.Add(new ChatMessage(chatRole, message));
        }
        public async Task<string> GetMessage() {
            var response = await OpenAiclient.GetChatCompletionsAsync(ChatCompletionsOptions);
            return response.Value.Choices[0].Message.Content.ToString();
        }

    }
}
