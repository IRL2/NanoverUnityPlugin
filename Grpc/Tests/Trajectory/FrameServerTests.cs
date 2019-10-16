// Copyright (c) Intangible Realities Laboratory. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Narupa.Grpc.Trajectory;
using Narupa.Protocol.Trajectory;
using Narupa.Testing.Async;
using NSubstitute;
using NUnit.Framework;

namespace Narupa.Grpc.Tests.Trajectory
{
    internal class FrameServerTests
    {
        private static IEnumerable<AsyncUnitTests.AsyncTestInfo> GetTests()
        {
            return AsyncUnitTests.FindAsyncTestsInClass<FrameServerTests>();
        }

        [Test]
        public void TestAsync([ValueSource(nameof(GetTests))] AsyncUnitTests.AsyncTestInfo test)
        {
            AsyncUnitTests.RunAsyncTest(this, test);
        }

        [AsyncTest]
        public async Task FrameDataTransmission()
        {
            var data = new FrameData();
            data.SetBondPairs(new[] { 0u, 1u, 1u, 2u });
            data.SetParticleElements(new[] { 1u, 6u, 1u });
            data.SetParticlePositions(new[] { -1f, 1f, 0f, 0f, 0f, 0f, 1f, -1f, 0f });

            var server = new QueueTrajectoryServer(55000, data);
            var connection = new GrpcConnection("localhost", 55000);

            try
            {
                var service = new TrajectoryClient(connection);

                var callback = Substitute.For<Action<GetFrameResponse>>();

                var stream = service.SubscribeLatestFrames();
                stream.MessageReceived += callback;
                var getFrameTask = stream.StartReceiving();

                await Task.WhenAny(getFrameTask, Task.Delay(500));

                callback.Received(1)
                        .Invoke(Arg.Is<GetFrameResponse>(rep => rep.Frame.Equals(data)));
            }
            finally
            {
                await server.CloseAsync();
                await connection.CloseAsync();
            }
        }
    }
}