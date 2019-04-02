using Caliburn.Micro;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using TestTesonet.Clients;
using TestTesonet.Infrastructure.Events;
using TestTesonet.Infrastructure.ViewModels;

namespace TestTesonet.ViewModels
{
    [Export(typeof(LoginViewModel))]
    public class LoginViewModel : ValidationPropertyChangedViewModelBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly IPlaygroundClient _playgroundClient;

        private string _username;
        private string _password;

        [ImportingConstructor]
        public LoginViewModel(IEventAggregator eventAggregator, IDialogCoordinator dialogCoordinator, IPlaygroundClient playgroundClient)
        {
            _playgroundClient = playgroundClient;
            _eventAggregator = eventAggregator;
            _dialogCoordinator = dialogCoordinator;
        }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Username is required.")]
        public string Username
        {
            get => _username;
            set
            {
                _username = value;

                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(CanLogin));
            }
        }
        
        [Required(AllowEmptyStrings = false, ErrorMessage = "Password is required.")]
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(CanLogin));
            }
        }

        public bool CanLogin => string.IsNullOrWhiteSpace(Error);

        public async void Login()
        {
            var busyEvent = new BusyEvent("login", "Login", "Logging in...");

            try
            {
                _eventAggregator.PublishOnCurrentThread(busyEvent);
                
                if (await _playgroundClient.Authenticate(Username, Password))
                {
                    _eventAggregator.PublishOnCurrentThread(new LoggedInEvent { Username = Username });

                    Username = null;
                    Password = null;
                }

                busyEvent.IsBusy = false;
                _eventAggregator.PublishOnCurrentThread(busyEvent);
            }
            catch (Exception e)
            {
                busyEvent.IsBusy = false;
                _eventAggregator.PublishOnCurrentThread(busyEvent);
                await _dialogCoordinator.ShowMessageAsync(this, "Login failed", $"Something went wrong while trying to login. Message: {e.Message}");
            }
        }
    }
}
