// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Narupa.Core.Async;
using Narupa.Grpc.Stream;
using Narupa.Testing.Async;
using NSubstitute;

namespace Narupa.Grpc.Tests.Async
{
    internal abstract class ClientOutgoingStreamTests<TServer, TClient, TMessage, TReply> :
        ClientStreamTests<
            TServer, TClient, OutgoingStream<TMessage, TReply>>
        where TServer : IAsyncClosable
        where TClient : IAsyncClosable, ICancellable
        where TMessage : new()
    {
        public override async Task SetUp()
        {
            await base.SetUp();
            serverCallback = Substitute.For<Action<TMessage>>();
            AddServerCallback(serverCallback);
        }

        public abstract void AddServerCallback(Action<TMessage> callback);

        private Action<TMessage> serverCallback;

        [AsyncTest]
        public async Task TwoSendRequests_Sequential()
        {
            var stream = GetStream(client);
            var streamTask = GetStreamTask(stream);

            async Task Sequential()
            {
                await stream.QueueMessageAsync(new TMessage());
                await stream.QueueMessageAsync(new TMessage());
                await Task.Delay(50);
            }

            await Task.WhenAny(streamTask,
                               Sequential());

            serverCallback.ReceivedWithAnyArgs(2).Invoke(default);
        }

        private Task GetStreamTask(OutgoingStream<TMessage, TReply> stream)
        {
            return stream.StartSending();
        }

        [AsyncTest]
        public async Task TwoSendRequests_Concurrent()
        {
            var stream = GetStream(client);
            var streamTask = GetStreamTask(stream);

            async Task Concurrent()
            {
                await Task.WhenAll(stream.QueueMessageAsync(new TMessage()),
                                   stream.QueueMessageAsync(new TMessage()));
                await Task.Delay(50);
            }

            await Task.WhenAny(streamTask,
                               Concurrent());

            serverCallback.ReceivedWithAnyArgs(2).Invoke(default);
        }
    }
}