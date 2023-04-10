using Community.VisualStudio.Toolkit;
using EnvDTE;
using JeffPires.VisualChatGPTStudio.Options;
using JeffPires.VisualChatGPTStudio.Utils;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using OpenAI_API.Completions;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace JeffPires.VisualChatGPTStudio.Snippets
{
    /// <summary>
    /// This class provides methods for generating code snippets for completion.
    /// </summary>
    [Export(typeof(IVsTextViewCreationListener))]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    [ContentType("text")]
    internal class CompletionSnippet : IVsTextViewCreationListener
    {
        [Import]
        internal IVsEditorAdaptersFactoryService AdapterService;

        private readonly OptionPageGridGeneral options;

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            IWpfTextView textView = AdapterService?.GetWpfTextView(textViewAdapter);

            if (textView == null)
            {
                return;
            }

            textView.TextBuffer.Changed += OnTextChanged;

            VS.Events.DocumentEvents.AfterDocumentWindowHide += DocumentEvents_AfterDocumentWindowHide;
        }

        /// <summary>
        /// Unsubscribes from the TextChanged event of the document view's text buffer after the document window is hidden.
        /// </summary>
        /// <param name="documentView">The document view.</param>
        private void DocumentEvents_AfterDocumentWindowHide(DocumentView documentView)
        {
            documentView.TextBuffer.Changed -= OnTextChanged;
        }

        /// <summary>
        /// Handles the TextChanged event of the WPFTextView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextContentChangedEventArgs"/> instance containing the event data.</param>
        private void OnTextChanged(object sender, TextContentChangedEventArgs e)
        {
            try
            {
                ITextBuffer textBuffer = (ITextBuffer)sender;

                string newText = textBuffer.CurrentSnapshot.GetText();

                int chatGptIndex = newText.LastIndexOf("chatgpt:", StringComparison.InvariantCultureIgnoreCase);

                if (chatGptIndex >= 0 && (e.Changes.First().NewText.Contains(Environment.NewLine) || e.Changes.First().NewText.Contains("\n")))
                {
                    int length = e.Changes.First().NewPosition - chatGptIndex - 9;

                    if (length <= 0)
                    {
                        return;
                    }

                    string request = newText.Substring(chatGptIndex + 8, length).Replace(Environment.NewLine, string.Empty).Trim();

                    _ = MakeRequestAndWriteTheResponseAsync(textBuffer, chatGptIndex, request);
                }
            }
            catch (Exception ex)
            {
                _ = VS.StatusBar.ShowProgressAsync(ex.Message, 2, 2);
            }
        }

        /// <summary>
        /// Makes a request to ChatGPT and writes the response to the text buffer.
        /// </summary>
        /// <param name="textBuffer">The text buffer to write the response to.</param>
        /// <param name="chatGptIndex">The index of the ChatGPT request.</param>
        /// <param name="request">The request to send to ChatGPT.</param>
        private async Task MakeRequestAndWriteTheResponseAsync(ITextBuffer textBuffer, int chatGptIndex, string request)
        {
            await VS.StatusBar.ShowProgressAsync(Utils.Constants.MESSAGE_WAITING_CHATGPT, 1, 2);

            CompletionResult result = await ChatGPT.RequestAsync(TextFormat.FormatForCompleteCommand(request, textBuffer.GetFileName()));

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
