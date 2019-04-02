using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestTesonet.Clients.Models;

namespace TestTesonet.Clients
{
    public interface IPlaygroundClient : IDisposable
    {
        Task<bool> Authenticate(string username, string password);
        Task<List<Server>> GetServers();
        void DropToken();

        bool LoggedIn { get; }
    }
}