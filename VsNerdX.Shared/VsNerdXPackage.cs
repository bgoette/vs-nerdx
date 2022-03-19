﻿using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using VsNerdX.Core;
using VsNerdX.Dispatcher;
using DebugLogger = VsNerdX.Util.DebugLogger;

namespace VsNerdX
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideAutoLoad(UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid(PackageGuidString)]
    public sealed class VsNerdXPackage : AsyncPackage, IAsyncLoadablePackageInitialize
    {
        public const string PackageGuidString = "c8973938-38ba-4db7-9798-11c7f5b4bc1f";
        public static VsNerdXPackage Instance;
        public static DTE2 Dte;

        private CommandProcessor _commandProcessor;
        private ConditionalKeyDispatcher _keyDispatcher;
        private DebugLogger _logger;

        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            BackgroundThreadInitialization();
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            _logger.Log("Getting somewhere with VSNerd");
            await MainThreadInitializationAsync();
        }

        private void BackgroundThreadInitialization()
        {
            _logger = new DebugLogger();
            Instance = this;
        }
        
        private async System.Threading.Tasks.Task MainThreadInitializationAsync()
        {
            Dte = await GetServiceAsync(typeof(_DTE)) as DTE2;
            var solutionExplorerControl = new HierarchyControl(this, _logger);

            _commandProcessor = new CommandProcessor(solutionExplorerControl, _logger);

            _keyDispatcher = new ConditionalKeyDispatcher(
                new SolutionExplorerDispatchCondition(solutionExplorerControl, _logger),
                new KeyDispatcher(_commandProcessor),
                _logger);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _keyDispatcher.Dispose();
        }
    }
}