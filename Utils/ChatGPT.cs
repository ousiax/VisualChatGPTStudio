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
        public static OptionPageGridGeneral Options { get; set; }

        private static OpenAIAPI api;

        /// <summary>
        /// Requests a completion from the OpenAI API using the given options.
        /// </summary>
        /// <param name="request">The request to send to the API.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async Task<CompletionResult> RequestAsync(string request)
        {
            CreateApiHandler();

            return await api.Completions.CreateCompletionAsync(GetRequest(request));
        }

        /// <summary>
        /// Requests a completion from the OpenAI API using the given options.
        /// </summary>
        /// <param name="request">The request to send to the API.</param>
        /// <param name="resultHandler">The action to take when the result is received.</param>
        /// <returns>A task representing the completion request.</returns>
        public static async Task RequestAsync(string request, Action<int, CompletionResult> resultHandler)
        {
            CreateApiHandler();

            await api.Completions.StreamCompletionAsync(GetRequest(request), resultHandler);
        }

        /// <summary>
        /// Creates a new conversation and appends a system message with the specified TurboChatBehavior.
        /// </summary>
        /// <returns>The newly created conversation.</returns>
        public static Conversation CreateConversation()
        {
            CreateApiHandler();

            Conversation chat = api.Chat.CreateConversation();

            chat.AppendSystemMessage(Options.TurboChatBehavior);

            return chat;
        }

        /// <summary>
        /// Creates an API handler with the given API key.
        /// </summary>
        private static void CreateApiHandler()
        {
            if (Options == null)
            {
                return;
            }

            if (api == null)
            {
                api = new(Options.ApiKey);
            }
            else if (api.Auth.ApiKey != Options.ApiKey)
            {
                api.Auth.ApiKey = Options.ApiKey;
            }
        }

        /// <summary>
        /// Gets a CompletionRequest object based on the given options and request.
        /// </summary>
        /// <param name="request">The request string.</param>
        /// <returns>A CompletionRequest object.</returns>
        private static CompletionRequest GetRequest(string request)
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
                case ModelLanguageEnum.CodeDavinci:
                    model = Model.DavinciCode;
                    break;
                case ModelLanguageEnum.CodeCushman:
                    model = Model.CushmanCode;
                    break;
            }

            return new(request, model, Options.MaxTokens, Options.Temperature, presencePenalty: Options.PresencePenalty, frequencyPenalty: Options.FrequencyPenalty, top_p: Options.TopP, stopSequences: Options.StopSequences.Split(','));
        }
    }
}
