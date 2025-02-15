﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using VsNerdX.Command;
using VsNerdX.Command.Directory;
using VsNerdX.Command.General;
using VsNerdX.Command.Navigation;
using VsNerdX.Dispatcher;
using VsNerdX.Util;

namespace VsNerdX.Core
{
    public class CommandProcessor : IKeyDispatcherTarget
    {
        private Dictionary<CommandKey, ICommand> commands = new Dictionary<CommandKey, ICommand>();
        private IExecutionContext executionContext;

        private readonly ILogger logger;
        private readonly IHierarchyControl _hierarchyControl;

        public CommandProcessor(IHierarchyControl hierarchyControl, ILogger logger, IExecutionContext initialExecutionContext = null)
        {
            this._hierarchyControl = hierarchyControl;
            this.logger = logger;
            this.executionContext = initialExecutionContext ?? new ExecutionContext();
            this.InitializeCommands();
        }

        public IExecutionContext ExecutionContext => this.executionContext;

        public bool OnKey(Keys key)
        {
            if (key == Keys.Return)
            {
                this.executionContext = this.executionContext.Clear();
                return false;
            }

            var handledKey = false;
            if (this.executionContext.DeferredExecutable != null)
            {
                var result = this.executionContext.DeferredExecutable.Execute(this.executionContext, key);
                this.executionContext = result.ExecutionContext;
                handledKey = result.State == CommandState.Handled;
            }

            if (!handledKey)
            {
                var commandKey = new CommandKey(this.executionContext.Mode, key);
                if (this.commands.TryGetValue(commandKey, out ICommand command))
                {
                    try
                    {
                        var result = command.Execute(this.executionContext, key);
                        this.executionContext = result.ExecutionContext;
                        handledKey = result.State == CommandState.Handled;
                    } catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                } else if (this.executionContext.Mode == InputMode.Yank || this.executionContext.Mode == InputMode.Go)
                {
                    handledKey = true;
                    this.executionContext = this.executionContext.Clear();
                }
            }

            this.logger.Log($"{key} handled = {handledKey}");
            return handledKey;
        }

        private void InitializeCommands()
        {
            commands.Add(new CommandKey(InputMode.Normal, Keys.X), new CloseParentNode(_hierarchyControl));
            commands.Add(new CommandKey(InputMode.Normal, Keys.O), new OpenOrCloseNode(_hierarchyControl));
            commands.Add(new CommandKey(InputMode.Normal, Keys.O | Keys.Shift), new OpenNodeRecursively(_hierarchyControl));
            commands.Add(new CommandKey(InputMode.Normal, Keys.X | Keys.Shift), new CloseNodeRecursively(_hierarchyControl));

            commands.Add(new CommandKey(InputMode.Normal, Keys.J), new GoDown(_hierarchyControl));
            commands.Add(new CommandKey(InputMode.Normal, Keys.J | Keys.Shift), new GoToLastChild(_hierarchyControl));
            commands.Add(new CommandKey(InputMode.Normal, Keys.K), new GoUp(_hierarchyControl));
            commands.Add(new CommandKey(InputMode.Normal, Keys.K | Keys.Shift), new GoToFirtsChild(_hierarchyControl));
            commands.Add(new CommandKey(InputMode.Normal, Keys.P | Keys.Shift), new GoToParent(_hierarchyControl));
            commands.Add(new CommandKey(InputMode.Normal, Keys.G | Keys.Shift), new GoToBottom(_hierarchyControl));

            commands.Add(new CommandKey(InputMode.Normal, Keys.G), new EnterGoMode(CommandState.Handled));
            commands.Add(new CommandKey(InputMode.Go, Keys.G), new GoToTop(_hierarchyControl));
            commands.Add(new CommandKey(InputMode.Go, Keys.O), new PreviewFile(_hierarchyControl));
            commands.Add(new CommandKey(InputMode.Normal, Keys.I), new OpenSplit(_hierarchyControl));
            commands.Add(new CommandKey(InputMode.Normal, Keys.S), new OpenVSplit(_hierarchyControl));
            commands.Add(new CommandKey(InputMode.Normal, Keys.I | Keys.Shift), new ShowAllFiles(_hierarchyControl));

            commands.Add(new CommandKey(InputMode.Normal, Keys.D), new Delete(_hierarchyControl));
            commands.Add(new CommandKey(InputMode.Normal, Keys.C), new CutFile(_hierarchyControl));
            commands.Add(new CommandKey(InputMode.Normal, Keys.P), new Paste(_hierarchyControl));
            commands.Add(new CommandKey(InputMode.Normal, Keys.R), new Rename(_hierarchyControl));
            commands.Add(new CommandKey(InputMode.Normal, Keys.Y), new EnterYankMode(CommandState.Handled));
            commands.Add(new CommandKey(InputMode.Normal, Keys.A), new AddItem(_hierarchyControl));
            commands.Add(new CommandKey(InputMode.Normal, Keys.N), new AddClass(_hierarchyControl));
            commands.Add(new CommandKey(InputMode.Yank, Keys.Y), new CopyFile(_hierarchyControl));
            commands.Add(new CommandKey(InputMode.Yank, Keys.P), new CopyPath(_hierarchyControl));
            commands.Add(new CommandKey(InputMode.Yank, Keys.W), new CopyText(_hierarchyControl));
            
            commands.Add(new CommandKey(InputMode.Normal, Keys.Divide), new EnterFindMode(CommandState.Handled));
            commands.Add(new CommandKey(InputMode.Normal, Keys.OemQuestion), new EnterFindMode(CommandState.Handled));
            commands.Add(new CommandKey(InputMode.Normal, Keys.Escape), new ClearExecutionStack());
            commands.Add(new CommandKey(InputMode.Normal, Keys.Oem2 | Keys.Shift), new ToggleHelp(_hierarchyControl));
            commands.Add(new CommandKey(InputMode.Find, Keys.Escape), new LeaveFindMode());
        }

    }
}