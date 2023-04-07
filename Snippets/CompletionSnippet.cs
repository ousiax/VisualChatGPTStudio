using Community.VisualStudio.Toolkit;
using EnvDTE;
using JeffPires.VisualChatGPTStudio.Options;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using OpenAI_API.Completions;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace JeffPires.VisualChatGPTStudio.Snippets
{
    /// <summary>
    /// This class provides methods for generating code snippets for completion.
    /// </summary>
    internal static class CompletionSnippet
    {
        private static OptionPageGridGeneral _options;

        /// <summary>
        /// Initializes the class.
        /// </summary>
        /// <param name="options">The options.</param>
        public static async Task Initialize(OptionPageGridGeneral options)
        {
            _options = options;

            DocumentView docView = await VS.Documents.GetActiveDocumentViewAsync();

            docView.TextBuffer.Changed += OnTextChanged;

            VS.Events.DocumentEvents.BeforeDocumentWindowShow += DocumentEvents_BeforeDocumentWindowShow;

            VS.Events.DocumentEvents.AfterDocumentWindowHide += DocumentEvents_AfterDocumentWindowHide;
        }

        /// <summary>
        /// Subscribes to the TextBuffer.Changed event of the document view.
        /// </summary>
        /// <param name="documentView">The document view.</param>
        private static void DocumentEvents_BeforeDocumentWindowShow(DocumentView documentView)
        {
            documentView.TextBuffer.Changed += OnTextChanged;
        }

        /// <summary>
        /// Unsubscribes from the TextChanged event of the document view's text buffer after the document window is hidden.
        /// </summary>
        /// <param name="documentView">The document view.</param>
        private static void DocumentEvents_AfterDocumentWindowHide(DocumentView documentView)
        {
            documentView.TextBuffer.Changed -= OnTextChanged;
        }

        /// <summary>
        /// Handles the TextChanged event of the WPFTextView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextContentChangedEventArgs"/> instance containing the event data.</param>
        private static void OnTextChanged(object sender, TextContentChangedEventArgs e)
        {
            try
            {
                ITextBuffer textBuffer = (ITextBuffer)sender;

                string newText = textBuffer.CurrentSnapshot.GetText();

                int chatGptIndex = newText.LastIndexOf("chatgpt:", StringComparison.InvariantCultureIgnoreCase);

                if (chatGptIndex >= 0 && (e.Changes.First().NewText.Contains(Environment.NewLine) || e.Changes.First().NewText.Contains("\n")))
                {
                    string request = newText.Substring(chatGptIndex + 8, e.Changes.First().NewPosition - chatGptIndex - 9).Replace(Environment.NewLine, string.Empty).Trim();

                    _ = MakeRequestAndWriteTheResponseAsync(textBuffer, chatGptIndex, request);
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Makes a request to ChatGPT and writes the response to the text buffer.
        /// </summary>
        /// <param name="textBuffer">The text buffer to write the response to.</param>
        /// <param name="chatGptIndex">The index of the ChatGPT request.</param>
        /// <param name="request">The request to send to ChatGPT.</param>
        private static async Task MakeRequestAndWriteTheResponseAsync(ITextBuffer textBuffer, int chatGptIndex, string request)
        {
            await VS.StatusBar.ShowProgressAsync(Utils.Constants.MESSAGE_WAITING_CHATGPT, 1, 2);

            CompletionResult result = await Utils.ChatGPT.RequestAsync(_options, request);

            string resultText = result.ToString();

            while (resultText.StartsWith(Environment.NewLine))
            {
                resultText = resultText.Substring(4);
            }

            while (resultText.StartsWith("\n"))
            {
                resultText = resultText.Substring(2);
            }

            textBuffer.Replace(new Span(chatGptIndex, request.Length + 11), resultText.ToString());

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            try
            {
                //Some documents does not has format
                (await VS.GetServiceAsync<DTE, DTE>()).ExecuteCommand("Edit.FormatDocument", string.Empty);
            }
            catch (Exception)
            {

            }

            await VS.StatusBar.ShowProgressAsync(Utils.Constants.MESSAGE_RECEIVING_CHATGPT, 2, 2);
        }
    }
}
