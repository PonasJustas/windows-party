using Caliburn.Micro;
using System.ComponentModel.Composition;
using TestTesonet.Infrastructure.Events;

namespace TestTesonet.ViewModels
{
    [Export(typeof(HomeMenuViewModel))]
    public class HomeMenuViewModel : PropertyChangedBase
    {
        private readonly IEventAggregator _eventAggregator;
        
        [ImportingConstructor]
        public HomeMenuViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public void Logout()
        {
            _eventAggregator.PublishOnCurrentThread(new LoggedOutEvent());
        }

        public void RefreshServers()
        {
            _eventAggregator.PublishOnCurrentThread(new RefreshServersEvent());
        }
    }
}
