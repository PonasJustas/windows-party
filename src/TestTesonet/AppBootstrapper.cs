using AutoMapper;
using Caliburn.Micro;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Configuration;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Threading;
using TestTesonet.Clients;
using TestTesonet.Infrastructure.Events;
using TestTesonet.Infrastructure.Helpers;

namespace TestTesonet
{
    public class AppBootstrapper : BootstrapperBase
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private CompositionContainer _container;

        public AppBootstrapper()
        {
            Initialize();

            ConventionManager.AddElementConvention<PasswordBox>(PasswordBoxHelper.BoundPasswordProperty, "Password", "PasswordChanged");
        }

        protected override void Configure()
        {
            _logger.Debug("Configuring BootStrapper");

            _container = new CompositionContainer(new AggregateCatalog(AssemblySource.Instance.Select(x => new AssemblyCatalog(x)).OfType<ComposablePartCatalog>()));
            
            var batch = new CompositionBatch();

            batch.AddExportedValue<IWindowManager>(new WindowManager());
            batch.AddExportedValue<IEventAggregator>(new EventAggregator());

            batch.AddExportedValue(DialogCoordinator.Instance);
            
            batch.AddExportedValue<IPlaygroundClient>(new PlaygroundClient(ConfigurationManager.AppSettings["PlaygroundServiceAddress"]));

            batch.AddExportedValue(_container);
            
            _container.Compose(batch);

            ConfigureAutomapper();
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            var contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
            var exports = _container.GetExportedValues<object>(contract);

            if (exports.Any())
            {
                return exports.First();
            }

            throw new Exception(string.Format("Could not locate any instances of contract {0}.", contract));
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return _container.GetExportedValues<object>(AttributedModelServices.GetContractName(serviceType));
        }

        protected override void BuildUp(object instance)
        {
            _container.SatisfyImportsOnce(instance);
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            _logger.Debug($"Starting the application");

            DisplayRootViewFor<IShell>();
        }

        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            base.OnUnhandledException(sender, e);

            _container.GetExportedValue<IEventAggregator>().PublishOnUIThread(new UnhandledExceptionEvent(e.Exception));
            _logger.Error(e.Exception, $"Unhandled Exception caught.");
            e.Handled = true;
        }

        private void ConfigureAutomapper()
        {
            Mapper.Initialize(cfg => cfg.CreateMap<Clients.Models.Server, Models.Server>());
        }
    }
}