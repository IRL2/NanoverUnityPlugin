using System.Collections.Generic;
using System.Threading.Tasks;
using Narupa.Grpc.Multiplayer;
using Narupa.Session;
using Narupa.Testing.Async;
using NUnit.Framework;

namespace Narupa.Grpc.Tests.Multiplayer
{
    public class MultiplayerResourceTests
    {
        private MultiplayerService service;
        private GrpcServer server;
        private MultiplayerSession session;
        private GrpcConnection connection;

        [SetUp]
        public void AsyncSetup()
        {
            AsyncUnitTests.RunAsyncSetUp(this);
        }

        [AsyncSetUp]
        public async Task Setup()
        {
            service = new MultiplayerService();
            server = new GrpcServer(service);

            session = new MultiplayerSession();

            connection = new GrpcConnection("localhost", server.Port);
            session.OpenClient(connection);
            await session.JoinMultiplayer("alex");

            await Task.Delay(500);
        }

        [TearDown]
        public void AsyncTearDown()
        {
            AsyncUnitTests.RunAsyncTearDown(this);
        }

        [AsyncTearDown]
        public async Task TearDown()
        {
            session?.CloseClient();
            if (connection != null)
                await connection.CloseAsync();
            if (server != null)
                await server.CloseAsync();
        }

        private const string Key = "abc";
        
        private static IEnumerable<AsyncUnitTests.AsyncTestInfo> GetTests()
        {
            return AsyncUnitTests.FindAsyncTestsInClass<MultiplayerSessionTests>();
        }

        [Test]
        public void TestAsync([ValueSource(nameof(GetTests))] AsyncUnitTests.AsyncTestInfo test)
        {
            AsyncUnitTests.RunAsyncTest(this, test);
        }

        public MultiplayerResource<string> GetResource()
        {
            return new MultiplayerResource<string>(session, Key);
        }

        [AsyncTest]
        public async Task GetResource_InitialValue()
        {
            service.SetValueDirect(Key, "my_value");
            await Task.Delay(25);
            var resource = GetResource();
            Assert.AreEqual("my_value", resource.Value);
            
            TestFunc(resource.)
        }
    }
}