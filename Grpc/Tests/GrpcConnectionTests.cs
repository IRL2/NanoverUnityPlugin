// Copyright (c) Intangible Realities Laboratory. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Narupa.Grpc;
using Narupa.Grpc.Tests.Trajectory;
using Narupa.Grpc.Trajectory;
using Narupa.Testing.Async;
using NUnit.Framework;

namespace Narupa.Network.Tests
{
    public class GrpcConnectionTests
    {
        private const string serverAddress = "localhost";
        private const int serverPort = 55000;

        private static IEnumerable<AsyncUnitTests.AsyncTestInfo> GetTests()
        {
            return AsyncUnitTests.FindAsyncTestsInClass<GrpcConnectionTests>();
        }

        [Test]
        public void TestAsync([ValueSource(nameof(GetTests))] AsyncUnitTests.AsyncTestInfo test)
        {
            AsyncUnitTests.RunAsyncTest(this, test);
        }

        [Test]
        public void Constructor_NullAddress_Exception()
        {
            GrpcConnection connection;

            Assert.Throws<ArgumentException>(
                () => connection = new GrpcConnection(null, 54321));
        }

        [Test]
        public void Constructor_NegativePort_Exception()
        {
            GrpcConnection connection;

            Assert.Throws<ArgumentOutOfRangeException>(
                () => connection = new GrpcConnection(serverAddress, -1));
        }

        [AsyncTest]
        public async Task NoServer_IsChannelIdle()
        {
            var connection = new GrpcConnection(serverAddress, serverPort);

            try
            {
                await Task.Delay(100);
                Assert.AreEqual(ChannelState.Idle, connection.Channel.State);
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        [AsyncTest]
        public async Task Server_IsChannelReady()
        {
            var server = new QueueTrajectoryServer(serverPort);
            var connection = new GrpcConnection(serverAddress, serverPort);

            try
            {
                var service = new TrajectoryClient(connection);

                using (var token = new CancellationTokenSource())
                {
                    var stream = service.SubscribeLatestFrames(externalToken: token.Token);
                    var task = stream.StartReceiving();

                    await Task.WhenAny(task, Task.Delay(100));

                    token.Cancel();

                    Assert.AreEqual(ChannelState.Ready, connection.Channel.State);
                }
            }
            finally
            {
                await server.CloseAsync();
                await connection.CloseAsync();
            }
        }

        [AsyncTest]
        public async Task DisconnectedServer_IsChannelIdle()
        {
            var server = new QueueTrajectoryServer(serverPort);
            var connection = new GrpcConnection(serverAddress, serverPort);

            try
            {
                var service = new TrajectoryClient(connection);

                using (var token = new CancellationTokenSource())
                {
                    var stream = service.SubscribeLatestFrames(externalToken: token.Token);
                    var task = stream.StartReceiving();

                    await Task.WhenAny(task, Task.Delay(100));

                    await server.CloseAsync();

                    await Task.Delay(100);

                    Assert.AreEqual(ChannelState.Idle, connection.Channel.State);

                    token.Cancel();
                }
            }
            finally
            {
                await server.CloseAsync();
                await connection.CloseAsync();
            }
        }

        [AsyncTest]
        public async Task Connection_CloseAsync_IsAtomic()
        {
            var server = new QueueTrajectoryServer(serverPort);
            var connection = new GrpcConnection(serverAddress, serverPort);

            await connection.CloseAsync();
            await connection.CloseAsync();

            await server.CloseAsync();
        }
    }
}