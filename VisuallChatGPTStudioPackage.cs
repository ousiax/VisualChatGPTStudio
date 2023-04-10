using Community.VisualStudio.Toolkit;
using JeffPires.VisualChatGPTStudio.Commands;
using JeffPires.VisualChatGPTStudio.Options;
using JeffPires.VisualChatGPTStudio.ToolWindows;
using JeffPires.VisualChatGPTStudio.Utils;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace JeffPires.VisualChatGPTStudio
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.VisuallChatGPTStudioString)]
    [ProvideOptionPage(typeof(OptionPageGridGeneral), "Visual chatGPT Studio", "General", 0, 0, true)]
    [ProvideProfile(typeof(OptionPageGridGeneral), "Visual chatGPT Studio", "General", 0, 0, true)]
    [ProvideOptionPage(typeof(OptionPageGridCommands), "Visual chatGPT Studio", "Commands", 1, 1, true)]
    [ProvideProfile(typeof(OptionPageGridCommands), "Visual chatGPT Studio", "Commands", 1, 1, true)]
    [ProvideToolWindow(typeof(TerminalWindow))]
    [ProvideToolWindow(typeof(TerminalWindowTurbo))]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class VisualChatGPTStudioPackage : ToolkitPackage
    {
        /// <summary>
        /// Gets the OptionPageGridGeneral object.
        /// </summary>
        public OptionPageGridGeneral OptionsGeneral
        {
            get
            {
                return (OptionPageGridGeneral)GetDialogPage(typeof(OptionPageGridGeneral));
            }
        }

        /// <summary>
        /// Gets the OptionPageGridCommands object.
        /// </summary>
        public OptionPageGridCommands OptionsCommands
        {
            get
            {
                return (OptionPageGridCommands)GetDialogPage(typeof(OptionPageGridCommands));
            }
        }

        /// <summary>
        /// Initializes the terminal window commands.
        /// </summary>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            ChatGPT.Options = OptionsGeneral;

            this.RegisterCommandsAsync();
            TerminalWindowCommand.InitializeAsync(this);
            TerminalWindowTurboCommand.InitializeAsync(this);
        }
    }
}