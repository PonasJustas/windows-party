using Caliburn.Micro;
using MahApps.Metro.Controls.Dialogs;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using TestTesonet.Clients;
using TestTesonet.Infrastructure.Events;

namespace TestTesonet.ViewModels
{
    [Export(typeof(IShell))]
    public class ShellViewModel : Conductor<object>, IShell, IHandle<LoggedInEvent>, IHandle<LoggedOutEvent>, IHandle<BusyEvent>, IHandle<UnhandledExceptionEvent>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly IPlaygroundClient _playgroundClient;

        private List<BusyEvent> _busyStack;
        
        private bool _busySpinnerIsVisible;
        private bool _loginIsVisible;
        private bool _homeIsVisible;
        private string _busySpinnerTitle;
        private string _busySpinnerText;

        [ImportingConstructor]
        public ShellViewModel(IEventAggregator eventAggregator, IDialogCoordinator dialogCoordinator, IPlaygroundClient playgroundClient, HomeViewModel homeViewModel, LoginViewModel loginViewModel)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);

            _dialogCoordinator = dialogCoordinator;

            _playgroundClient = playgroundClient;

            Home = homeViewModel;
            Login = loginViewModel;
            LoginIsVisible = true;

            _busyStack = new List<BusyEvent>();
        }

        public HomeViewModel Home { get; }

        public LoginViewModel Login { get; }

        public string BusySpinnerTitle
        {
            get => _busySpinnerTitle;
            set
            {
                _busySpinnerTitle = value;
                NotifyOfPropertyChange();
            }
        }

        public string BusySpinnerText
        {
            get => _busySpinnerText;
            set
            {
                _busySpinnerText = value;
                NotifyOfPropertyChange();
            }
        }

        public bool BusySpinnerIsVisible
        {
            get => _busySpinnerIsVisible;
            set
            {
                _busySpinnerIsVisible = value;
                NotifyOfPropertyChange();
            }
        }

        public bool LoginIsVisible
        {
            get => _loginIsVisible;
            set
            {
                _loginIsVisible = value;
                HomeIsVisible = !value;
                NotifyOfPropertyChange();
            }
        }

        public bool HomeIsVisible
        {
            get => _homeIsVisible;
            set
            {
                _homeIsVisible = value;
                NotifyOfPropertyChange();
            }
        }

        public void Handle(LoggedInEvent message)
        {
            LoginIsVisible = false;
        }

        public void Handle(LoggedOutEvent message)
        {
            _playgroundClient.DropToken();
            LoginIsVisible = true;
        }

        public void Handle(BusyEvent message)
        {
            if (message.IsBusy)
            {
                _busyStack.Add(message);
            }
            else
            {
                var busyEvent = _busyStack.FirstOrDefault(x => x.Event == message.Event);
                if (busyEvent != null)
                {
                    _busyStack.Remove(busyEvent);
                }
            }

            var latestBusyEvent = _busyStack.LastOrDefault();

            if (latestBusyEvent != null)
            {
                BusySpinnerIsVisible = latestBusyEvent.IsBusy;
                BusySpinnerTitle = latestBusyEvent.Title;
                BusySpinnerText = latestBusyEvent.Text;
            }
            else
            {
                BusySpinnerIsVisible = false;
                BusySpinnerTitle = null;
                BusySpinnerText = null;
            }
        }

        public async void Handle(UnhandledExceptionEvent message)
        {
            await _dialogCoordinator.ShowMessageAsync(this, "Unhandled exception occured.", message.Exception.Message);
        }
    }
}