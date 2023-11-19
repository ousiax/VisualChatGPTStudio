﻿using Newtonsoft.Json;
using System;
    /// <summary>
    /// Represents a user control for displaying and interacting with option commands.
    /// </summary>
    public partial class OptionCommandsWindow : UserControl
        #region Delegates
        /// <summary>
        /// Delegate for updating a list of commands.
        /// </summary>
        /// <param name="commands">The list of commands to update.</param>
        public delegate void DelegateUpdateCommands(List<Commands> commands);

        #endregion Delegates
        #region Properties
        private readonly string originalCommands;
        private List<Commands> commands;

        #endregion Properties
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the OptionCommandsWindow class.
        /// </summary>
        /// <param name="commands">The list of commands to display.</param>
        public OptionCommandsWindow(List<Commands> commands)

        #endregion Constructors
        #region Event Handlers
        /// <summary>
        /// Handles the click event of the Cancel button. 
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)

        /// <summary>
        /// Removes the selected command from the grid.
        /// </summary>
        private void RemoveCommand_Click(object sender, RoutedEventArgs e)

        /// <summary>
        /// Event handler for the AddCommand button click event. Adds a new Commands object to the commands collection and refreshes the grid view.
        /// </summary>
        private void AddCommand_Click(object sender, RoutedEventArgs e)

        /// <summary>
        /// Saves the commands and updates the command list.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void SaveCommands_Click(object sender, RoutedEventArgs e)

        #endregion Event Handlers
    }