// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Narupa.Grpc.Interactive;
using Narupa.Grpc.Stream;
using Narupa.Grpc.Tests.Async;
using Narupa.Protocol.Imd;
using Narupa.Testing.Async;
using NUnit.Framework;

namespace Narupa.Grpc.Tests.Interactive
{
    internal class ImdClientPublishInteractionsTests : ClientOutgoingStreamTests<InteractionService,
        ImdClient, ParticleInteraction, InteractionEndReply>
    {
        private static IEnumerable<AsyncUnitTests.AsyncTestInfo> GetTests()
        {
            return AsyncUnitTests.FindAsyncTestsInClass<ImdClientPublishInteractionsTests>();
        }

        [Test]
        public void TestAsync([ValueSource(nameof(GetTests))] AsyncUnitTests.AsyncTestInfo test)
        {
            AsyncUnitTests.RunAsyncTest(this, test);
        }


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

        [AsyncSetUp]
        public override async Task SetUp()
        {
            await base.SetUp();
        }

        [AsyncTearDown]
        public override async Task TearDown()
        {
            await base.TearDown();
        }

        protected override InteractionService GetService()
        {
            return new InteractionService();
        }

        protected override ImdClient GetClient(GrpcConnection connection)
        {
            return new ImdClient(connection);
        }
        
        protected override OutgoingStream<ParticleInteraction, InteractionEndReply> GetStream(
            ImdClient client)
        {
            return client.PublishInteractions();
        }

        public override void AddServerCallback(Action<ParticleInteraction> callback)
        {
            service.InteractionReceived += callback;
        }
    }
}