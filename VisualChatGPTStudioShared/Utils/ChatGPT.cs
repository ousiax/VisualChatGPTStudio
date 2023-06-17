using JeffPires.VisualChatGPTStudio.Options;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Completions;
using OpenAI_API.Models;
using System;
using System.Threading.Tasks;

namespace JeffPires.VisualChatGPTStudio.Utils
{
    /// <summary>
    /// Static class containing methods for interacting with the ChatGPT API.
    /// </summary>
    static class ChatGPT
    {
        private static OpenAIAPI api;
        private static OpenAIAPI apiForAzureTurboChat;
        private static ChatGPTHttpClientFactory chatGPTHttpClient;

        /// <summary>
        /// Gets or sets the Options property which is of type OptionPageGridGeneral.
        /// </summary>
        public static OptionPageGridGeneral Options { get; set; }

        /// <summary>
        /// Requests a completion from the OpenAI API using the given options.
        /// </summary>
        /// <param name="request">The request to send to the API.</param>
        /// <returns>The completion result.</returns>
        public static async Task<CompletionResult> RequestAsync(string request)
        {
            CreateApiHandler();

            return await api.Completions.CreateCompletionAsync(GetRequest(request));
        }

        /// <summary>
        /// Requests a completion from the OpenAI API using the given options.
        /// </summary>
        /// <param name="request">The request to send to the API.</param>
        /// <param name="stopSequences">Up to 4 sequences where the API will stop generating further tokens.</param>
        /// <returns>The completion result.</returns>
        public static async Task<CompletionResult> RequestAsync(string request, string[] stopSequences)
        {
            CreateApiHandler();

            return await api.Completions.CreateCompletionAsync(GetRequest(request, stopSequences));
        }

        /// <summary>
        /// Requests a completion from the OpenAI API using the given options.
        /// </summary>
        /// <param name="request">The request to send to the API.</param>
        /// <param name="resultHandler">The action to take when the result is received.</param>
        public static async Task RequestAsync(string request, Action<int, CompletionResult> resultHandler)
        {
            CreateApiHandler();

            await api.Completions.StreamCompletionAsync(GetRequest(request), resultHandler);
        }

        /// <summary>
        /// Requests a completion from the OpenAI API using the given options.
        /// </summary>
        /// <param name="request">The request to send to the API.</param>
        /// <param name="resultHandler">The action to take when the result is received.</param>
        /// <param name="stopSequences">Up to 4 sequences where the API will stop generating further tokens.</param>
        public static async Task RequestAsync(string request, Action<int, CompletionResult> resultHandler, string[] stopSequences)
        {
            CreateApiHandler();

            await api.Completions.StreamCompletionAsync(GetRequest(request, stopSequences), resultHandler);
        }

        /// <summary>
        /// Creates a new conversation and appends a system message with the specified TurboChatBehavior.
        /// </summary>
        /// <returns>The newly created conversation.</returns>
        public static Conversation CreateConversation()
        {
            Conversation chat;

            if (Options.Service == OpenAIService.OpenAI || string.IsNullOrWhiteSpace(Options.AzureTurboChatDeploymentId))
            {
                CreateApiHandler();

                chat = api.Chat.CreateConversation();
            }
            else
            {
                CreateApiHandlerForAzureTurboChat();

                chat = apiForAzureTurboChat.Chat.CreateConversation();
            }

            chat.AppendSystemMessage(Options.TurboChatBehavior);

            if (Options.TurboChatModelLanguage == TurboChatModelLanguageEnum.GPT_4)
            {
                chat.Model = Model.GPT4;
            }

            return chat;
        }

        /// <summary>
        /// Creates an API handler with the given API key and proxy.
        /// </summary>
        private static void CreateApiHandler()
        {
            if (api == null)
            {
                chatGPTHttpClient = new();

                if (!string.IsNullOrWhiteSpace(Options.Proxy))
                {
                    chatGPTHttpClient.SetProxy(Options.Proxy);
                }

                if (Options.Service == OpenAIService.AzureOpenAI)
                {
                    api = OpenAIAPI.ForAzure(Options.AzureResourceName, Options.AzureDeploymentId, Options.ApiKey);
                }
                else
                {
                    APIAuthentication auth;

                    if (!string.IsNullOrWhiteSpace(Options.OpenAIOrganization))
                    {
                        auth = new(Options.ApiKey, Options.OpenAIOrganization);
                    }
                    else
                    {
                        auth = new(Options.ApiKey);
                    }

                    api = new(auth);
                }

                api.HttpClientFactory = chatGPTHttpClient;
            }
            else if ((Options.Service == OpenAIService.AzureOpenAI && !api.ApiUrlFormat.ToUpper().Contains("AZURE")) || (Options.Service == OpenAIService.OpenAI && api.ApiUrlFormat.ToUpper().Contains("AZURE")))
            {
                api = null;
                CreateApiHandler();
            }
            else if (api.Auth.ApiKey != Options.ApiKey)
            {
                api.Auth.ApiKey = Options.ApiKey;
            }
        }

        /// <summary>
        /// Creates an API handler for Azure TurboChat using the provided options.
        /// </summary>
        private static void CreateApiHandlerForAzureTurboChat()
        {
            if (apiForAzureTurboChat == null)
            {
                chatGPTHttpClient = new();

                if (!string.IsNullOrWhiteSpace(Options.Proxy))
                {
                    chatGPTHttpClient.SetProxy(Options.Proxy);
                }

                apiForAzureTurboChat = OpenAIAPI.ForAzure(Options.AzureResourceName, Options.AzureTurboChatDeploymentId, Options.ApiKey);

                apiForAzureTurboChat.HttpClientFactory = chatGPTHttpClient;

                if (!string.IsNullOrWhiteSpace(Options.AzureTurboChatApiVersion))
                {
                    apiForAzureTurboChat.ApiVersion = Options.AzureTurboChatApiVersion;
                }
            }
            else if (apiForAzureTurboChat.Auth.ApiKey != Options.ApiKey)
            {
                apiForAzureTurboChat.Auth.ApiKey = Options.ApiKey;
            }
        }

        /// <summary>
        /// Gets a CompletionRequest object based on the given options and request.
        /// </summary>
        /// <param name="request">The request string.</param>
        /// <returns>A CompletionRequest object.</returns>
        private static CompletionRequest GetRequest(string request)
        {
            return GetRequest(request, null);
        }

        /// <summary>
        /// Gets a CompletionRequest object based on the given options and request.
        /// </summary>
        /// <param name="request">The request string.</param>
        /// <param name="stopSequences">Up to 4 sequences where the API will stop generating further tokens.</param>
        /// <returns>A CompletionRequest object.</returns>
        private static CompletionRequest GetRequest(string request, string[] stopSequences)
        {
            Model model = Model.DavinciText;

            switch (Options.Model)
            {
                case ModelLanguageEnum.TextCurie001:
                    model = Model.CurieText;
                    break;
                case ModelLanguageEnum.TextBabbage001:
                    model = Model.BabbageText;
                    break;
                case ModelLanguageEnum.TextAda001:
                    model = Model.AdaText;
                    break;
            }

            if (stopSequences == null || stopSequences.Length == 0)
            {
                stopSequences = Options.StopSequences.Split(',');
            }

            return new(request, model, Options.MaxTokens, Options.Temperature, presencePenalty: Options.PresencePenalty, frequencyPenalty: Options.FrequencyPenalty, top_p: Options.TopP, stopSequences: stopSequences);
        }
    }
}
