// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Narupa.Core.Async;
using Narupa.Grpc.Stream;
using Narupa.Testing.Async;
using NSubstitute;
using NUnit.Framework;

namespace Narupa.Grpc.Tests.Async
{
    internal abstract class
        ClientIncomingStreamTests<TService, TClient, TMessage> : ClientStreamTests<
            TService, TClient, IncomingStream<TMessage>>
        where TService : IBindableService
        where TClient : IAsyncClosable, ICancellable
    {
        public abstract Task GetStreamTask(IncomingStream<TMessage> stream);

        public abstract void SetServerDelay(int delay);

        public abstract void SetServerMaxMessage(int count);

        [AsyncTest]
        public async Task CancelConnection_StreamTask_IsCancelled()
        {
            var stream = GetStream(client);
            var streamTask = GetStreamTask(stream);

            await Task.WhenAny(streamTask, Task.Delay(50));

            await connection.CloseAsync();

            await Task.WhenAny(streamTask, Task.Delay(200));

            Assert.AreEqual(TaskStatus.Canceled, streamTask.Status);
            Assert.IsTrue(streamTask.IsCanceled);
        }

        [AsyncTest]
        public async Task CancelConnection_StartStreamTaskAfter_Exception()
        {
            var stream = GetStream(client);

            await connection.CloseAsync();

            Assert.Throws<InvalidOperationException>(() => GetStreamTask(stream));
        }

        [AsyncTest]
        public async Task CancelClient_StreamTask_IsCancelled()
        {
            var stream = GetStream(client);
            var streamTask = GetStreamTask(stream);

            await client.CloseAsync();

            await Task.WhenAny(streamTask, Task.Delay(200));

            Assert.AreEqual(TaskStatus.Canceled, streamTask.Status);
            Assert.IsTrue(streamTask.IsCanceled);
        }

        [AsyncTest]
        public async Task CancelClient_StartStreamTaskAfter_Exception()
        {
            var stream = GetStream(client);

            await client.CloseAsync();

            Assert.Throws<InvalidOperationException>(() => GetStreamTask(stream));
        }


        [AsyncTest]
        public async Task CancelStream_StreamTask_IsCancelled()
        {
            var stream = GetStream(client);
            var streamTask = GetStreamTask(stream);

            await stream.CloseAsync();

            await Task.WhenAny(streamTask, Task.Delay(200));

            Assert.AreEqual(TaskStatus.Canceled, streamTask.Status);
            Assert.IsTrue(streamTask.IsCanceled);
        }

        [AsyncTest]
        public async Task CancelStream_StartStreamTaskAfter_Exception()
        {
            var stream = GetStream(client);

            await stream.CloseAsync();

            Assert.Throws<InvalidOperationException>(() => GetStreamTask(stream));
        }

        [AsyncTest]
        public async Task IncomingStream_ResponseWithinTimePeriod()
        {
            SetServerDelay(100);
            SetServerMaxMessage(1);

            var callback = Substitute.For<Action<TMessage>>();
            var stream = GetStream(client);
            stream.MessageReceived += callback;
            var task = GetStreamTask(stream);

            await Task.WhenAny(task, Task.Delay(50));

            callback.Received(1).Invoke(Arg.Any<TMessage>());
        }

        [AsyncTest]
        public async Task IncomingStream_AwaitResponse()
        {
            SetServerDelay(100);
            SetServerMaxMessage(1);

            var callback = Substitute.For<Action<TMessage>>();

            var stream = GetStream(client);
            stream.MessageReceived += callback;
            var task = GetStreamTask(stream);

            await task;

            callback.Received(1).Invoke(Arg.Any<TMessage>());
        }

        [AsyncTest]
        public async Task IncomingStream_WithDelay()
        {
            SetServerDelay(100);
            SetServerMaxMessage(2);

            var callback = Substitute.For<Action<TMessage>>();

            var stream = GetStream(client);
            stream.MessageReceived += callback;
            var task = GetStreamTask(stream);

            await task;

            callback.Received(2).Invoke(Arg.Any<TMessage>());
        }

        [AsyncTest]
        public async Task IncomingStream_WithDelay_Interrupted()
        {
            SetServerMaxMessage(2);
            SetServerDelay(100);

            var callback = Substitute.For<Action<TMessage>>();

            var stream = GetStream(client);
            stream.MessageReceived += callback;
            var task = GetStreamTask(stream);

            await Task.WhenAny(task, Task.Delay(300));

            await server.CloseAsync();

            callback.Received(2).Invoke(Arg.Any<TMessage>());
        }

        [AsyncTest]
        public async Task IncomingStream_CloseStream_StopSending()
        {
            SetServerMaxMessage(5);
            SetServerDelay(100);

            var callback = Substitute.For<Action<TMessage>>();

            var stream = GetStream(client);
            stream.MessageReceived += callback;
            var task = GetStreamTask(stream);

            await Task.WhenAny(task, Task.Delay(180));

            callback.Received(2).Invoke(Arg.Any<TMessage>());

            stream.Cancel();

            await Task.Delay(500);

            callback.Received(2).Invoke(Arg.Any<TMessage>());
        }
    }
}