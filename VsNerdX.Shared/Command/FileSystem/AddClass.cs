using System;
using System.Windows.Forms;
using VsNerdX.Core;
using static VsNerdX.VsNerdXPackage;

namespace VsNerdX.Command.Navigation
{
    public class AddClass : ICommand
    {
        private readonly IHierarchyControl _hierarchyControl;

        public AddClass(IHierarchyControl hierarchyControl)
        {
            _hierarchyControl = hierarchyControl;
        }

        public ExecutionResult Execute(IExecutionContext executionContext, Keys key)
        {
            try
            {
                Dte.ExecuteCommand("Project.AddClass");
            }
            catch (Exception e) { }

            executionContext = executionContext
                .Clear()
                .With(mode: InputMode.Normal);

            return new ExecutionResult(executionContext, CommandState.Handled);
        }
    }
}
