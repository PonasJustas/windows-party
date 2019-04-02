using NUnit.Framework;
using System;
using System.Configuration;
using System.Security.Authentication;
using System.Threading.Tasks;
using TestTesonet.Clients;

namespace TestTesonet.IntegrationTests
{
    [TestFixture]
    public class PlaygroundClientTests
    {
        private IPlaygroundClient _playgroundClient;

        [SetUp]
        public void SetUp()
        {
            _playgroundClient = new PlaygroundClient(ConfigurationManager.AppSettings["PlaygroundServiceAddress"]);
        }
        
        [TestCase("tesonet", "partyanimal")]
        public async Task TestAuthenticateSuccess(string username, string password)
        {
            var result = await _playgroundClient.Authenticate(username, password);
            Assert.AreEqual(result, true);
            Assert.AreEqual(_playgroundClient.LoggedIn, true);
        }

        [TestCase("fakeUser", "fakePassword")]
        public async Task TestAuthenticateFail(string username, string password)
        {
            Assert.ThrowsAsync<Exception>(() => _playgroundClient.Authenticate(username, password));
            Assert.AreEqual(_playgroundClient.LoggedIn, false);
        }
        
        [TestCase("tesonet", "partyanimal")]
        public async Task TestGetServersSuccess(string username, string password)
        {
            await _playgroundClient.Authenticate(username, password);
            var result = await _playgroundClient.GetServers();
            Assert.IsTrue(result.Count > 0);
        }

        [Test]
        public async Task TestGetServersFail()
        {
            Assert.ThrowsAsync<AuthenticationException>(() => _playgroundClient.GetServers());
        }
    }
}
