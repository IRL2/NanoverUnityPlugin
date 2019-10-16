// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Narupa.Grpc.Tests.Trajectory;
using Narupa.Grpc.Trajectory;
using Narupa.Protocol.Trajectory;
using Narupa.Testing.Async;
using NUnit.Framework;

namespace Narupa.Grpc.Tests.Session
{
    internal class TrajectorySessionTests
    {
        private static IEnumerable<AsyncUnitTests.AsyncTestInfo> GetTests()
        {
            return AsyncUnitTests.FindAsyncTestsInClass<TrajectorySessionTests>();
        }

        [Test]
        public void TestAsync([ValueSource(nameof(GetTests))] AsyncUnitTests.AsyncTestInfo test)
        {
            AsyncUnitTests.RunAsyncTest(this, test);
        }

        [AsyncTest]
        public async Task Trajectory()
        {
            var server = new QueueTrajectoryServer(54321, new FrameData());
            var connection = new GrpcConnection("localhost", 54321);

            try
            {
                var session = new TrajectorySession();

                Assert.IsNull(session.CurrentFrame);

                session.OpenClient(connection);

                await Task.Delay(200);

                Assert.IsNotNull(session.CurrentFrame);
            }
            finally
            {
                await connection.CloseAsync();
                await server.CloseAsync();
            }
        }
    }
}