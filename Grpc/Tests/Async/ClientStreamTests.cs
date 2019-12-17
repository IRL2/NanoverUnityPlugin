// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Narupa.Core.Async;
using Narupa.Testing.Async;
using NUnit.Framework;

namespace Narupa.Grpc.Tests.Async
{
    internal abstract class ClientStreamTests<TService, TClient, TStream> :
        ClientTests<TService, TClient>
        where TService : IBindableService
        where TClient : IAsyncClosable, ICancellable
        where TStream : IAsyncClosable, ICancellable
    {
        protected abstract TStream GetStream(TClient client);

        [AsyncTest]
        public async Task CloseConnection_Stream_IsCancelled()
        {
            var stream = GetStream(client);

            await connection.CloseAsync();

            Assert.IsTrue(stream.IsCancelled);
        }

        [AsyncTest]
        public async Task CloseConnection_StartStreamAfter_Exception()
        {
            await connection.CloseAsync();

            Assert.Throws<InvalidOperationException>(() => GetStream(client));
        }

        [AsyncTest]
        public async Task CloseConnection_StreamToken_IsCancelled()
        {
            var stream = GetStream(client);

            var streamToken = stream.GetCancellationToken();

            await connection.CloseAsync();

            Assert.IsTrue(streamToken.IsCancellationRequested);
        }

        [AsyncTest]
        public async Task CloseConnection_StreamTokenAfter_Exception()
        {
            var stream = GetStream(client);

            CancellationToken streamToken;

            await connection.CloseAsync();

            Assert.Throws<InvalidOperationException>(
                () => streamToken = stream.GetCancellationToken());
        }

        [AsyncTest]
        public async Task CloseClient_Stream_IsCancelled()
        {
            var stream = GetStream(client);

            await client.CloseAsync();

            Assert.IsTrue(stream.IsCancelled);
        }

        [AsyncTest]
        public async Task CancelClient_Stream_IsCancelled()
        {
            var stream = GetStream(client);

            await client.CloseAsync();

            Assert.IsTrue(stream.IsCancelled);
        }

        [AsyncTest]
        public async Task CloseClient_StartStreamAfter_Exception()
        {
            await client.CloseAsync();

            Assert.Throws<InvalidOperationException>(() => GetStream(client));
        }

        [AsyncTest]
        public Task CancelClient_StartStreamAfter_Exception()
        {
            client.Cancel();

            Assert.Throws<InvalidOperationException>(() => GetStream(client));

            return Task.CompletedTask;
        }

        [AsyncTest]
        public async Task CloseClient_StreamToken_IsCancelled()
        {
            var stream = GetStream(client);

            var streamToken = stream.GetCancellationToken();

            await client.CloseAsync();

            Assert.IsTrue(streamToken.IsCancellationRequested);
        }

        [AsyncTest]
        public Task CancelClient_StreamToken_IsCancelled()
        {
            var stream = GetStream(client);

            var streamToken = stream.GetCancellationToken();

            client.Cancel();

            Assert.IsTrue(streamToken.IsCancellationRequested);

            return Task.CompletedTask;
        }

        [AsyncTest]
        public async Task CloseClient_StreamTokenAfter_Exception()
        {
            var stream = GetStream(client);

            CancellationToken streamToken;

            await client.CloseAsync();

            Assert.Throws<InvalidOperationException>(
                () => streamToken = stream.GetCancellationToken());
        }

        [AsyncTest]
        public Task CancelClient_StreamTokenAfter_Exception()
        {
            var stream = GetStream(client);

            CancellationToken streamToken;

            client.Cancel();

            Assert.Throws<InvalidOperationException>(
                () => streamToken = stream.GetCancellationToken());

            return Task.CompletedTask;
        }

        [AsyncTest]
        public async Task CloseStream_Stream_IsCancelled()
        {
            var stream = GetStream(client);

            await stream.CloseAsync();

            Assert.IsTrue(stream.IsCancelled);
        }

        [AsyncTest]
        public Task CancelStream_Stream_IsCancelled()
        {
            var stream = GetStream(client);

            stream.Cancel();

            Assert.IsTrue(stream.IsCancelled);

            return Task.CompletedTask;
        }

        [AsyncTest]
        public async Task CloseStream_StreamToken_IsCancelled()
        {
            var stream = GetStream(client);

            var streamToken = stream.GetCancellationToken();

            await stream.CloseAsync();

            Assert.IsTrue(streamToken.IsCancellationRequested);
        }

        [AsyncTest]
        public Task CancelStream_StreamToken_IsCancelled()
        {
            var stream = GetStream(client);

            var streamToken = stream.GetCancellationToken();

            stream.Cancel();

            Assert.IsTrue(streamToken.IsCancellationRequested);

            return Task.CompletedTask;
        }

        [AsyncTest]
        public async Task CloseStream_StreamTokenAfter_Exception()
        {
            var stream = GetStream(client);

            CancellationToken streamToken;

            await stream.CloseAsync();

            Assert.Throws<InvalidOperationException>(
                () => streamToken = stream.GetCancellationToken());
        }

        [AsyncTest]
        public Task CancelStream_StreamTokenAfter_Exception()
        {
            var stream = GetStream(client);

            CancellationToken streamToken;

            stream.Cancel();

            Assert.Throws<InvalidOperationException>(
                () => streamToken = stream.GetCancellationToken());

            return Task.CompletedTask;
        }

        [AsyncTest]
        public async Task CloseStream_OtherStreamStillActive()
        {
            var stream1 = GetStream(client);
            var stream2 = GetStream(client);

            var stream1Token = stream1.GetCancellationToken();
            var stream2Token = stream2.GetCancellationToken();

            await stream1.CloseAsync();

            Assert.IsTrue(stream1Token.IsCancellationRequested);
            Assert.IsFalse(stream2Token.IsCancellationRequested);

            await stream2.CloseAsync();
        }

        [AsyncTest]
        public Task CancelStream_OtherStreamStillActive()
        {
            var stream1 = GetStream(client);
            var stream2 = GetStream(client);

            var stream1Token = stream1.GetCancellationToken();
            var stream2Token = stream2.GetCancellationToken();

            stream1.Cancel();

            Assert.IsTrue(stream1Token.IsCancellationRequested);
            Assert.IsFalse(stream2Token.IsCancellationRequested);

            stream2.Cancel();

            return Task.CompletedTask;
        }

        [AsyncTest]
        public async Task CloseStream_Idempotent()
        {
            var stream = GetStream(client);

            await stream.CloseAsync();

            await stream.CloseAsync();
        }

        [AsyncTest]
        public Task CancelStream_Idempotent()
        {
            var stream = GetStream(client);

            stream.Cancel();

            stream.Cancel();

            return Task.CompletedTask;
        }

        [AsyncTest]
        public async Task CloseThenCancelStream()
        {
            var stream = GetStream(client);

            await stream.CloseAsync();

            stream.Cancel();
        }

        [AsyncTest]
        public async Task CancelThenCloseStream()
        {
            var stream = GetStream(client);

            stream.Cancel();

            await stream.CloseAsync();
        }

        [AsyncTest]
        public async Task CloseStream_Simultaneous()
        {
            var stream = GetStream(client);

            await Task.WhenAll(stream.CloseAsync(), stream.CloseAsync());
        }
    }
}