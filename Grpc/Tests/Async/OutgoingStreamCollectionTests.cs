// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Narupa.Grpc.Interactive;
using Narupa.Grpc.Stream;
using Narupa.Grpc.Tests.Interactive;
using Narupa.Protocol.Imd;
using Narupa.Testing.Async;
using NSubstitute;
using NUnit.Framework;

namespace Narupa.Grpc.Tests.Async
{
    internal class OutgoingStreamCollectionTests
    {
        private static IEnumerable<AsyncUnitTests.AsyncTestInfo> GetTests()
        {
            return AsyncUnitTests.FindAsyncTestsInClass<OutgoingStreamCollectionTests>();
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

        [Test]
        public void TestAsync([ValueSource(nameof(GetTests))] AsyncUnitTests.AsyncTestInfo test)
        {
            AsyncUnitTests.RunAsyncTest(this, test);
        }

        private InteractionServer server;
        private GrpcConnection connection;
        private ImdClient client;
        private OutgoingStreamCollection<ParticleInteraction, InteractionEndReply> streamCollection;
        private Action<ParticleInteraction> callback;
        private CancellationTokenSource collectionCancellationTokenSource;

        [AsyncSetUp]
        public Task SetUp()
        {
            server = new InteractionServer(54321);
            connection = new GrpcConnection("localhost", 54321);
            client = new ImdClient(connection);
            collectionCancellationTokenSource = new CancellationTokenSource();
            streamCollection =
                new OutgoingStreamCollection<ParticleInteraction, InteractionEndReply>(
                    client.PublishInteractions, collectionCancellationTokenSource.Token);
            callback = Substitute.For<Action<ParticleInteraction>>();
            server.Service.InteractionReceived += callback;

            return Task.CompletedTask;
        }

        [AsyncTearDown]
        public async Task TearDown()
        {
            if (server != null)
                await server.CloseAsync();
            if (streamCollection != null)
                await streamCollection.CloseAsync();
            if (connection != null)
                await connection.CloseAsync();
            if (client != null)
                await client.CloseAsync();
            streamCollection?.Dispose();
            client?.Dispose();
        }


        [AsyncTest]
        public Task CloseAsync_Works()
        {
            return Task.CompletedTask;
        }

        [AsyncTest]
        public Task IsCancelled_Initial_False()
        {
            Assert.IsFalse(streamCollection.IsCancelled);

            return Task.CompletedTask;
        }

        [AsyncTest]
        public Task StartAsync_NullKey_Exception()
        {
            Assert.Throws<ArgumentNullException>(() => streamCollection.StartStream(null));

            return Task.CompletedTask;
        }

        [AsyncTest]
        public async Task SendAsync_Works()
        {
            var streamTask = streamCollection.StartStream("key");
            await streamCollection.QueueMessageAsync("key", new ParticleInteraction());
            await Task.WhenAny(streamTask, Task.Delay(50));
            callback.ReceivedWithAnyArgs(1).Invoke(default);
        }

        [AsyncTest]
        public async Task SendAsync_NullMessage_Exception()
        {
            var streamTask = streamCollection.StartStream("key");
            await AsyncAssert.ThrowsAsync<ArgumentNullException>(
                async () => await streamCollection.QueueMessageAsync("key", null));
        }

        [AsyncTest]
        public async Task SendAsync_NullKey_Exception()
        {
            var streamTask = streamCollection.StartStream("key");
            await AsyncAssert.ThrowsAsync<ArgumentNullException>(
                async () => await streamCollection.QueueMessageAsync(
                                null, new ParticleInteraction()));
        }

        [AsyncTest]
        public async Task SendAsync_MissingKey_Exception()
        {
            var streamTask = streamCollection.StartStream("key");
            await AsyncAssert.ThrowsAsync<KeyNotFoundException>(
                async () => await streamCollection.QueueMessageAsync(
                                "other", new ParticleInteraction()));
        }

        [AsyncTest]
        public async Task EndAsync_NullKey_Exception()
        {
            var streamTask = streamCollection.StartStream("key");
            await AsyncAssert.ThrowsAsync<ArgumentNullException>(
                async () => await streamCollection.EndStreamAsync(null));
        }

        [AsyncTest]
        public async Task EndAsync_MissingKey_Exception()
        {
            var streamTask = streamCollection.StartStream("key");
            await AsyncAssert.ThrowsAsync<KeyNotFoundException>(
                async () => await streamCollection.EndStreamAsync("other"));
        }

        [AsyncTest]
        public Task StartAsync_AfterCancellation_Exception()
        {
            collectionCancellationTokenSource.Cancel();

            Assert.Throws<InvalidOperationException>(() => streamCollection.StartStream("abc"));

            return Task.CompletedTask;
        }

        [AsyncTest]
        public async Task SendAsync_AfterCancellation_Exception()
        {
            collectionCancellationTokenSource.Cancel();

            await AsyncAssert.ThrowsAsync<InvalidOperationException>(
                async () => await streamCollection.QueueMessageAsync(
                                "key", new ParticleInteraction()));
        }

        [AsyncTest]
        public async Task EndAsync_AfterCancellation_Exception()
        {
            collectionCancellationTokenSource.Cancel();

            await AsyncAssert.ThrowsAsync<InvalidOperationException>(
                async () => await streamCollection.EndStreamAsync("key"));
        }

        [AsyncTest]
        public async Task StartAsync_AfterCloseAsync_Exception()
        {
            await streamCollection.CloseAsync();

            Assert.Throws<InvalidOperationException>(() => streamCollection.StartStream("abc"));
        }

        [AsyncTest]
        public async Task SendAsync_AfterCloseAsync_Exception()
        {
            await streamCollection.CloseAsync();

            await AsyncAssert.ThrowsAsync<InvalidOperationException>(
                async () => await streamCollection.QueueMessageAsync(
                                "key", new ParticleInteraction()));
        }

        [AsyncTest]
        public async Task EndAsync_AfterCloseAsync_Exception()
        {
            await streamCollection.CloseAsync();

            await AsyncAssert.ThrowsAsync<InvalidOperationException>(
                async () => await streamCollection.EndStreamAsync("key"));
        }

        [AsyncTest]
        public async Task SimultaneousInteractions()
        {
            var streamTask1 = streamCollection.StartStream("abc");
            var streamTask2 = streamCollection.StartStream("def");

            await streamCollection.QueueMessageAsync("abc", new ParticleInteraction());
            await streamCollection.QueueMessageAsync("def", new ParticleInteraction());

            await Task.WhenAny(streamTask1, streamTask2, Task.Delay(150));

            await streamCollection.CloseAsync();

            callback.ReceivedWithAnyArgs(2).Invoke(default);
        }

        [AsyncTest]
        public async Task ClosedOneBeforeAnotherInteractions()
        {
            var streamTask1 = streamCollection.StartStream("abc");

            await streamCollection.QueueMessageAsync("abc", new ParticleInteraction());

            await Task.WhenAny(streamTask1, Task.Delay(100));

            var streamTask2 = streamCollection.StartStream("def");

            await streamCollection.QueueMessageAsync("def", new ParticleInteraction());

            await Task.WhenAny(streamTask1, streamTask2, Task.Delay(100));

            await streamCollection.EndStreamAsync("abc");

            await streamCollection.QueueMessageAsync("def", new ParticleInteraction());

            await Task.WhenAny(streamTask2, Task.Delay(100));

            await streamCollection.CloseAsync();

            callback.ReceivedWithAnyArgs(3).Invoke(default);
        }

        [AsyncTest]
        public async Task SubsequentInteractions_SameName()
        {
            var streamTask1 = streamCollection.StartStream("abc");

            await streamCollection.QueueMessageAsync("abc", new ParticleInteraction());

            await Task.WhenAny(streamTask1, Task.Delay(50));

            await streamCollection.EndStreamAsync("abc");

            var streamTask2 = streamCollection.StartStream("abc");

            await streamCollection.QueueMessageAsync("abc", new ParticleInteraction());

            await Task.WhenAny(streamTask2, Task.Delay(50));

            await streamCollection.EndStreamAsync("abc");

            await streamCollection.CloseAsync();

            callback.ReceivedWithAnyArgs(2).Invoke(default);
        }

        [AsyncTest]
        public async Task SingleMessage_Received()
        {
            var streamTask1 = streamCollection.StartStream("abc");

            await streamCollection.QueueMessageAsync("abc", new ParticleInteraction());

            await Task.WhenAny(streamTask1, Task.Delay(50));

            await streamCollection.EndStreamAsync("abc");

            callback.ReceivedWithAnyArgs(1).Invoke(default);
        }
    }
}