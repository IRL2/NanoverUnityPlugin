// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using NanoVer.Grpc.Tests.Commands;
using NanoVer.Grpc.Tests.Multiplayer;
using NanoVer.Grpc.Tests.Trajectory;
using NanoVer.Grpc.Trajectory;
using NanoVer.Protocol.Trajectory;
using NanoVer.Testing.Async;
using NUnit.Framework;

namespace NanoVer.Grpc.Tests.Session
{
    internal class TrajectorySessionSetupTests
    {
        private QueueTrajectoryService service;
        private GrpcServer server;
        private GrpcConnection connection;

        private static IEnumerable<AsyncUnitTests.AsyncTestInfo> GetTests()
        {
            return AsyncUnitTests.FindAsyncTestsInClass<TrajectorySessionSetupTests>();
        }

        [Test]
        public void TestAsync([ValueSource(nameof(GetTests))] AsyncUnitTests.AsyncTestInfo test)
        {
            AsyncUnitTests.RunAsyncTest(this, test);
        }

        [SetUp]
        public void AsyncSetup()
        {
            AsyncUnitTests.RunAsyncSetUp(this);
        }

        [AsyncSetUp]
        public Task Setup()
        {
            service = new QueueTrajectoryService(new FrameData());
            (server, connection) = GrpcServer.CreateServerAndConnection(service);
            return Task.CompletedTask;
        }

        [TearDown]
        public void AsyncTearDown()
        {
            AsyncUnitTests.RunAsyncTearDown(this);
        }

        [AsyncTearDown]
        public async Task TearDown()
        {
            await connection.CloseAsync();
            await server.CloseAsync();
        }

        [AsyncTest]
        public async Task ReceivesFrameFromServer()
        {
            var session = new TrajectorySession();
            Assert.IsNull(session.CurrentFrame);

            session.OpenClient(connection);

            void HasReceivedFrame() => Assert.IsNotNull(session.CurrentFrame);

            await AsyncAssert.PassesWithinTimeout(HasReceivedFrame);
        }
    }
}