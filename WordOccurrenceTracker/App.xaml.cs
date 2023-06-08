using System.Windows;
using Autofac;
using WordOccurrenceTracker.Services.Contracts;
using WordOccurrenceTracker.Services;
using WordOccurrenceTracker.ViewModels;
using WordOccurrenceTracker.Views;
using System.Threading.Tasks;
using System.Windows.Threading;
using System;
using NLog;

namespace WordOccurrenceTracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var builder = new ContainerBuilder();
            LogManager.Setup().LoadConfigurationFromFile("NLog.config");
            builder.RegisterType<MainViewModel>().AsSelf();
            builder.RegisterType<WordCounterService>().As<IWordCounterService>();
            builder.RegisterInstance(Logger).As<ILogger>();
            var container = builder.Build();
            using var scope = container.BeginLifetimeScope();
            RegisterGlobalExceptionHandling(Logger);
            var viewModel = scope.Resolve<MainViewModel>();
            var mainWindow = new MainWindow { DataContext = viewModel };

            mainWindow.Show();
        }

        private void RegisterGlobalExceptionHandling(ILogger log)
        {
            AppDomain.CurrentDomain.UnhandledException +=
                (_, args) => CurrentDomainOnUnhandledException(args, log);

            Dispatcher.UnhandledException +=
                (_, args) => DispatcherOnUnhandledException(args, log);

            Application.Current.DispatcherUnhandledException +=
                (_, args) => CurrentOnDispatcherUnhandledException(args, log);

            TaskScheduler.UnobservedTaskException +=
                (_, args) => TaskSchedulerOnUnobservedTaskException(args, log);
        }

        private static void TaskSchedulerOnUnobservedTaskException(UnobservedTaskExceptionEventArgs args, ILogger log)
        {
            log.Error(args.Exception, args.Exception.Message);
            args.SetObserved();
        }

        private static void CurrentOnDispatcherUnhandledException(DispatcherUnhandledExceptionEventArgs args, ILogger log)
        {
            log.Error(args.Exception, args.Exception.Message);
            args.Handled = true;
        }

        private static void DispatcherOnUnhandledException(DispatcherUnhandledExceptionEventArgs args, ILogger log)
        {
            log.Error(args.Exception, args.Exception.Message);
            args.Handled = true;
        }

        private static void CurrentDomainOnUnhandledException(UnhandledExceptionEventArgs args, ILogger log)
        {
            var exception = args.ExceptionObject as Exception;
            var terminatingMessage = args.IsTerminating ? " The application is terminating." : string.Empty;
            var exceptionMessage = exception?.Message ?? "An unmanaged exception occured.";
            var message = string.Concat(exceptionMessage, terminatingMessage);
            log.Error(exception, message);
        }
    }
}
