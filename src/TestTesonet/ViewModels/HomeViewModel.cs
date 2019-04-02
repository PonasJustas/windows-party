using AutoMapper;
using Caliburn.Micro;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using TestTesonet.Clients;
using TestTesonet.Infrastructure.Events;
using TestTesonet.Infrastructure.ViewModels;
using TestTesonet.Models;

namespace TestTesonet.ViewModels
{
    [Export(typeof(HomeViewModel))]
    public sealed class HomeViewModel : ValidationScreenViewModelBase, IHandle<LoggedInEvent>, IHandle<RefreshServersEvent>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly IPlaygroundClient _playgroundClient;

        [ImportingConstructor]
        public HomeViewModel(IEventAggregator eventAggregator, IDialogCoordinator dialogCoordinator, IPlaygroundClient playgroundClient, HomeMenuViewModel homeMenuViewModel)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);

            _dialogCoordinator = dialogCoordinator;

            _playgroundClient = playgroundClient;

            HomeMenu = homeMenuViewModel;
            
            Deactivated += OnDeactivated;

            DisplayName = "Home";

            Servers = new BindableCollection<Server>();
        }

        public HomeMenuViewModel HomeMenu { get; }

        public BindableCollection<Server> Servers { get; }

        private void OnDeactivated(object sender, DeactivationEventArgs e)
        {
            _eventAggregator.Unsubscribe(this);
            
            Deactivated -= OnDeactivated;
        }
        
        public void Handle(LoggedInEvent message)
        {
            RefreshServers();
        }

        public void Handle(RefreshServersEvent message)
        {
            RefreshServers();
        }

        private async void RefreshServers()
        {
            var busyEvent = new BusyEvent("home", "Loading", "Fetching servers list...");
            try
            {
                _eventAggregator.PublishOnCurrentThread(busyEvent);

                var servers = Mapper.Map<List<Server>>(await _playgroundClient.GetServers());

                Servers.Clear();
                Servers.AddRange(servers);

                busyEvent.IsBusy = false;
                _eventAggregator.PublishOnCurrentThread(busyEvent);
            }
            catch (Exception e)
            {
                busyEvent.IsBusy = false;
                _eventAggregator.PublishOnCurrentThread(busyEvent);

                await _dialogCoordinator.ShowMessageAsync(this, "Servers fetch failed", $"Something went wrong while trying to fetch servers list. Message: {e.Message}{Environment.NewLine}Logging out...");
                _eventAggregator.PublishOnCurrentThread(new LoggedOutEvent());
            }
        }
    }
}
