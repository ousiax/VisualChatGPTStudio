﻿using Community.VisualStudio.Toolkit;
using JeffPires.VisualChatGPTStudio.Utils;

namespace JeffPires.VisualChatGPTStudio.Commands
{
    [Command(PackageIds.Complete)]
    internal sealed class Complete : BaseChatGPTCommand<Complete>
    {
        protected override CommandType GetCommandType(string selectedText)
        {
            return CommandType.InsertAfter;
        }

        protected override string GetCommand(string selectedText)
        {
            return TextFormat.FormatForCompleteCommand(OptionsCommands.Complete, docView.FilePath, selectedText);
        }
    }
}
