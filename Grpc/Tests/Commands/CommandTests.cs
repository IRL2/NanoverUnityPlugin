using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Narupa.Grpc.Tests.Trajectory;
using Narupa.Grpc.Trajectory;
using Narupa.Testing.Async;
using NSubstitute;
using NUnit.Framework;

namespace Narupa.Grpc.Tests.Commands
{
    internal class CommandTests
    {
        [SetUp]
        public void AsyncSetUp()
        {
            AsyncUnitTests.RunAsyncSetUp(this);
        }

        [TearDown]
        public void AsyncTearDown()
        {
            AsyncUnitTests.RunAsyncTearDown(this);
        }

        private static IEnumerable<AsyncUnitTests.AsyncTestInfo> GetTests()
        {
            return AsyncUnitTests.FindAsyncTestsInClass<CommandTests>();
        }

        [Test]
        public void TestAsync([ValueSource(nameof(GetTests))] AsyncUnitTests.AsyncTestInfo test)
        {
            AsyncUnitTests.RunAsyncTest(this, test);
        }

        [AsyncSetUp]
        public virtual Task SetUp()
        {
            server = new TrajectoryWithCommandServer(55000);
            connection = new GrpcConnection("localhost", 55000);
            session = new TrajectorySession();
            session.OpenClient(connection);

            return Task.CompletedTask;
        }

        [AsyncTearDown]
        public virtual async Task TearDown()
        {
            session.CloseClient();
            await connection.CloseAsync();
            await server.CloseAsync();
        }

        private TrajectoryWithCommandServer server;
        private GrpcConnection connection;
        private TrajectorySession session;

        [AsyncTest]
        public async Task CommandPlay()
        {
            var callback = Substitute.For<Action<string, Struct>>();

            server.Commands.RecievedCommand += callback;

            session.Play();

            await Task.Delay(100);

            callback.Received(1)
                    .Invoke(TrajectoryClient.CommandPlay, Arg.Any<Struct>());
        }
        
        [AsyncTest]
        public async Task CommandPause()
        {
            var callback = Substitute.For<Action<string, Struct>>();

            server.Commands.RecievedCommand += callback;

            session.Pause();

            await Task.Delay(100);

            callback.Received(1)
                    .Invoke(TrajectoryClient.CommandPause, Arg.Any<Struct>());
        }
        
        [AsyncTest]
        public async Task CommandReset()
        {
            var callback = Substitute.For<Action<string, Struct>>();

            server.Commands.RecievedCommand += callback;

            session.Reset();

            await Task.Delay(100);

            callback.Received(1)
                    .Invoke(TrajectoryClient.CommandReset, Arg.Any<Struct>());
        }
        
        [AsyncTest]
        public async Task CommandStep()
        {
            var callback = Substitute.For<Action<string, Struct>>();

            server.Commands.RecievedCommand += callback;

            session.Step();

            await Task.Delay(100);

            callback.Received(1)
                    .Invoke(TrajectoryClient.CommandStep, Arg.Any<Struct>());
        }
    }
}