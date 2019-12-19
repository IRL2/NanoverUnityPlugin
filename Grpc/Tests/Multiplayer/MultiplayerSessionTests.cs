using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Narupa.Grpc.Multiplayer;
using Narupa.Session;
using Narupa.Testing.Async;
using NSubstitute;
using NUnit.Framework;
using Assert = UnityEngine.Assertions.Assert;

namespace Narupa.Grpc.Tests.Multiplayer
{
    internal class MultiplayerSessionTests
    {
        private MultiplayerServer service;
        private GrpcServer server;
        private MultiplayerSession session;
        private GrpcConnection connection;
        
        private MultiplayerSession session2;
        private GrpcConnection connection2;

        private const int DelayMilliseconds = 250;

        [SetUp]
        public void AsyncSetup()
        {
            AsyncUnitTests.RunAsyncSetUp(this);
        }

        [AsyncSetUp]
        public async Task Setup()
        {
            service = new MultiplayerServer();
            server = new GrpcServer(54321,
                                    Narupa.Multiplayer.Multiplayer.BindService(service));
            session = new MultiplayerSession();
            session2 = new MultiplayerSession();
            
            connection = new GrpcConnection("localhost", 54321);
            session.OpenClient(connection);
            await session.JoinMultiplayer("alex");
            
            connection2 = new GrpcConnection("localhost", 54321);
            session2.OpenClient(connection2);
            await session2.JoinMultiplayer("mike");
        }

        [TearDown]
        public void AsyncTearDown()
        {
            AsyncUnitTests.RunAsyncTearDown(this);
        }

        [AsyncTearDown]
        public async Task TearDown()
        {
            session.CloseClient();
            session2.CloseClient();
            await connection.CloseAsync();
            await connection2.CloseAsync();
            await server.CloseAsync();
        }

        private static IEnumerable<AsyncUnitTests.AsyncTestInfo> GetTests()
        {
            return AsyncUnitTests.FindAsyncTestsInClass<MultiplayerSessionTests>();
        }

        [Test]
        public void TestAsync([ValueSource(nameof(GetTests))] AsyncUnitTests.AsyncTestInfo test)
        {
            AsyncUnitTests.RunAsyncTest(this, test);
        }

        [AsyncTest]
        public async Task ValueChanged_ClientGetsUpdate()
        {
            service.Resources["abc"] = 1.2;
            await Task.Delay(DelayMilliseconds);
            CollectionAssert.Contains(session.SharedStateDictionary.Keys, "abc");
        }

        [AsyncTest]
        public async Task ValueChanged_ClientCallback()
        {
            var callback = Substitute.For<Action<string, object>>();
            session.SharedStateDictionaryKeyUpdated += callback;

            service.Resources["abc"] = 1.2;
            await Task.Delay(DelayMilliseconds);
            callback.Received(1).Invoke(Arg.Is("abc"), Arg.Any<object>());
        }

        [AsyncTest]
        public async Task ValueChanged_MultiplayerResource()
        {
            var value = session.GetSharedResource("abc");
            service.Resources["abc"] = 1.2;

            await Task.Delay(DelayMilliseconds);

            Assert.AreEqual(1.2, value.Value);
        }

        [AsyncTest]
        public async Task ValueChanged_MultiplayerResourceCallback()
        {
            var callback = Substitute.For<Action>();
            var value = session.GetSharedResource("abc");
            value.ValueChanged += callback;
            service.Resources["abc"] = 1.2;

            await Task.Delay(DelayMilliseconds);

            Assert.AreEqual(1.2, value.Value);

            callback.Received(1).Invoke();
        }

        [AsyncTest]
        public async Task TryLock_Success()
        {
            var value = session.GetSharedResource("abc");
            value.ObtainLock();
            Assert.AreEqual(MultiplayerResourceLockState.Pending, value.LockState);

            await Task.Delay(DelayMilliseconds);

            Assert.AreEqual(MultiplayerResourceLockState.Locked, value.LockState);
            Assert.IsTrue(service.Locks.TryGetValue("abc", out var v)
                       && v.Equals(session.PlayerId));
        }
        
        [AsyncTest]
        public async Task TryLock_SomeoneElseHasLock()
        {
            var value2 = session2.GetSharedResource("abc");
            value2.ObtainLock();
            
            await Task.Delay(DelayMilliseconds);
            
            Assert.IsTrue(service.Locks.TryGetValue("abc", out var v1)
                       && v1.Equals(session2.PlayerId));

            var value1 = session.GetSharedResource("abc");
            value1.ObtainLock();
            
            Assert.AreEqual(MultiplayerResourceLockState.Pending, value1.LockState);

            await Task.Delay(DelayMilliseconds);

            Assert.AreEqual(MultiplayerResourceLockState.Unlocked, value1.LockState);
            Assert.IsTrue(service.Locks.TryGetValue("abc", out var v2)
                       && v2.Equals(session2.PlayerId));
        }
        
        [AsyncTest]
        public async Task TryLock_SomeoneElseHadLockThenReleased()
        {
            var value2 = session2.GetSharedResource("abc");
            value2.ObtainLock();
            
            await Task.Delay(DelayMilliseconds);
            
            Assert.IsTrue(service.Locks.TryGetValue("abc", out var v1)
                       && v1.Equals(session2.PlayerId));

            var value1 = session.GetSharedResource("abc");
            value1.ObtainLock();
            
            Assert.AreEqual(MultiplayerResourceLockState.Pending, value1.LockState);

            await Task.Delay(DelayMilliseconds);

            Assert.AreEqual(MultiplayerResourceLockState.Unlocked, value1.LockState);
            Assert.IsTrue(service.Locks.TryGetValue("abc", out var v2)
                       && v2.Equals(session2.PlayerId));
            
            value2.ReleaseLock();
            
            await Task.Delay(DelayMilliseconds);
            
            CollectionAssert.IsEmpty(service.Locks);
            
            value1.ObtainLock();
            Assert.AreEqual(MultiplayerResourceLockState.Pending, value1.LockState);

            await Task.Delay(DelayMilliseconds);

            Assert.AreEqual(MultiplayerResourceLockState.Locked, value1.LockState);
            Assert.IsTrue(service.Locks.TryGetValue("abc", out var v3)
                       && v3.Equals(session.PlayerId));
        }
    }
}