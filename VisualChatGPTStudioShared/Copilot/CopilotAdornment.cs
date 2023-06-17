using Community.VisualStudio.Toolkit;
using JeffPires.VisualChatGPTStudio.Utils;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using OpenAI_API.Completions;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace JeffPires.VisualChatGPTStudioShared.Copilot
{
    /// <summary>
    /// Class that implements the Autopilot functionality
    /// </summary>
    internal class CopilotAdornment
    {
        /// <summary>
        /// The layer of the adornment.
        /// </summary>
        private readonly IAdornmentLayer layer;

        /// <summary>
        /// Text view where the adornment is created.
        /// </summary>
        private readonly IWpfTextView textView;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopilotAdornment"/> class.
        /// </summary>
        /// <param name="view">Text view to create the adornment for</param>
        public CopilotAdornment(IWpfTextView view)
        {
            if (view == null)
            {
                return;
            }

            layer = view.GetAdornmentLayer("CopilotAdornment");
            textView = view;

            string extension = System.IO.Path.GetExtension(textView.TextBuffer.GetFileName()).TrimStart('.');

            if (!extension.Equals("cs", StringComparison.InvariantCultureIgnoreCase) && !extension.Equals("vb", StringComparison.InvariantCultureIgnoreCase))
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

                //If it's not a new line, do nothing.
                if (!e.Changes.First().NewText.Contains(Environment.NewLine) && !e.Changes.First().NewText.Contains("\n"))
                {
                    return;
                }

                layer.RemoveAllAdornments();

                int methodStartedIndex = 0;

                int length = e.Changes.First().NewPosition - methodStartedIndex;

                if (length <= 0)
                {
                    return;
                }

                string request = newText.Substring(methodStartedIndex, length).Trim();

                _ = MakeRequestAndWriteTheResponseAsync(e.Changes.First().NewPosition, request);
            }
            catch (Exception)
            {

            }
        }

        private async System.Threading.Tasks.Task MakeRequestAndWriteTheResponseAsync(int position, string request)
        {
            CompletionResult result = await ChatGPT.RequestAsync(TextFormat.FormatForCompleteCommand("Please complete: ", request, textView.TextBuffer.GetFileName()));

            string resultText = result.ToString();

            while (resultText.StartsWith(Environment.NewLine))
            {
                resultText = resultText.Substring(4);
            }

            while (resultText.StartsWith("\n"))
            {
                resultText = resultText.Substring(2);
            }

            TextBlock textBlock = new() { Text = resultText, FontSize = 12, FontStyle = FontStyles.Italic, FontFamily = new FontFamily("Consolas"), Foreground = new SolidColorBrush(Colors.Purple) };

            SnapshotSpan span = new(textView.TextSnapshot, Span.FromBounds(position, position + 1));

            this.layer.AddAdornment(AdornmentPositioningBehavior.TextRelative, span, null, textBlock, null);
        }
    }
}
