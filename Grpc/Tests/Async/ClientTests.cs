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
    internal abstract class ClientTests<TServer, TClient>
        where TServer : IAsyncClosable
        where TClient : IAsyncClosable, ICancellable
    {
        protected abstract TServer GetServer();
        protected abstract TClient GetClient(GrpcConnection connection);
        protected abstract GrpcConnection GetConnection();

        protected TServer server;
        protected TClient client;
        protected GrpcConnection connection;

        public virtual Task SetUp()
        {
            server = GetServer();
            connection = GetConnection();
            client = GetClient(connection);

            return Task.CompletedTask;
        }

        public virtual async Task TearDown()
        {
            await connection.CloseAsync();
            await server.CloseAsync();
        }

        [AsyncTest]
        public async Task CloseConnection_ConnectionToken_IsCancelled()
        {
            var connectionToken = connection.GetCancellationToken();

            await connection.CloseAsync();

            Assert.IsTrue(connectionToken.IsCancellationRequested);
        }

        [AsyncTest]
        public async Task CloseConnection_ConnectionTokenAfter_Exception()
        {
            await connection.CloseAsync();

            CancellationToken token;

            Assert.Throws<InvalidOperationException>(
                () => token = connection.GetCancellationToken());
        }

        [AsyncTest]
        public async Task CloseConnection_ClientToken_IsCancelled()
        {
            var clientToken = client.GetCancellationToken();

            await connection.CloseAsync();

            Assert.IsTrue(clientToken.IsCancellationRequested);
        }

        [AsyncTest]
        public async Task CloseConnection_ClientTokenAfter_Exception()
        {
            CancellationToken clientToken;

            await connection.CloseAsync();

            Assert.Throws<InvalidOperationException>(
                () => clientToken = client.GetCancellationToken());
        }


        [AsyncTest]
        public async Task CloseClient_ClientToken_IsCancelled()
        {
            var clientToken = client.GetCancellationToken();

            await client.CloseAsync();

            Assert.IsTrue(clientToken.IsCancellationRequested);
        }

        [AsyncTest]
        public Task CancelClient_ClientToken_IsCancelled()
        {
            var clientToken = client.GetCancellationToken();

            client.Cancel();

            Assert.IsTrue(clientToken.IsCancellationRequested);

            return Task.CompletedTask;
        }


        [AsyncTest]
        public async Task CloseClient_ClientTokenAfter_Exception()
        {
            CancellationToken clientToken;

            await client.CloseAsync();

            Assert.Throws<InvalidOperationException>(
                () => clientToken = client.GetCancellationToken());
        }

        [AsyncTest]
        public async Task CancelClient_ClientTokenAfter_Exception()
        {
            CancellationToken clientToken;

            await client.CloseAsync();

            Assert.Throws<InvalidOperationException>(
                () => clientToken = client.GetCancellationToken());
        }

        [AsyncTest]
        public async Task CloseClient_Idempotent()
        {
            await client.CloseAsync();

            await client.CloseAsync();
        }

        [AsyncTest]
        public Task CancelClient_Idempotent()
        {
            client.Cancel();

            client.Cancel();

            return Task.CompletedTask;
        }

        [AsyncTest]
        public async Task CloseThenCancelClient()
        {
            await client.CloseAsync();

            client.Cancel();
        }

        [AsyncTest]
        public async Task CancelThenCloseClient()
        {
            client.Cancel();

            await client.CloseAsync();
        }

        [AsyncTest]
        public async Task CloseClient_Simultaneous()
        {
            await Task.WhenAll(client.CloseAsync(), client.CloseAsync());
        }
    }
}