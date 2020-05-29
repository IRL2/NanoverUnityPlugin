using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Narupa.Core.Collections;
using Narupa.Protocol.Multiplayer;
using UnityEngine;
using static Narupa.Protocol.Multiplayer.Multiplayer;

namespace Narupa.Grpc.Tests.Multiplayer
{
    internal class MultiplayerService : MultiplayerBase, IBindableService
    {
        private ObservableDictionary<string, object> resources
            = new ObservableDictionary<string, object>();

        private Dictionary<string, string> locks = new Dictionary<string, string>();

        public IDictionary<string, object> Resources => resources;

        public IReadOnlyDictionary<string, string> Locks => locks;

        public async Task<bool> AcquireLock(string playerId, string resourceKey)
        {
            if (locks.ContainsKey(resourceKey))
                return false;

            locks[resourceKey] = playerId;
            return true;
        }

        public async Task<bool> ReleaseLock(string playerId, string resourceKey)
        {
            if (!locks.ContainsKey(resourceKey) || locks[resourceKey] != playerId)
                return false;
            locks.Remove(resourceKey);
            return true;
        }

        private async Task<bool> RemoveValue(string playerId, string resourceKey)
        {
            if (locks.ContainsKey(resourceKey) && locks[resourceKey] != playerId)
                return false;
            resources.Remove(resourceKey);
            return true;
        }

        private async Task<bool> SetValue(string playerId, string resourceKey, object value)
        {
            if (locks.ContainsKey(resourceKey) && locks[resourceKey] != playerId)
                return false;
            resources[resourceKey] = value;
            return true;
        }
        
        public bool SetValueDirect(string resourceKey, object value)
        {
            if (locks.ContainsKey(resourceKey))
                return false;
            resources[resourceKey] = value;
            return true;
        }

        public override async Task<ResourceRequestResponse> AcquireResourceLock(
            AcquireLockRequest request,
            ServerCallContext context)
        {
            var success = await AcquireLock(request.PlayerId, request.ResourceId);
            return new ResourceRequestResponse
            {
                Success = success
            };
        }

        public override async Task<ResourceRequestResponse> ReleaseResourceLock(
            ReleaseLockRequest request,
            ServerCallContext context)
        {
            var success = await ReleaseLock(request.PlayerId, request.ResourceId);
            return new ResourceRequestResponse
            {
                Success = success
            };
        }

        public override async Task<ResourceRequestResponse> RemoveResourceKey(
            RemoveResourceKeyRequest request,
            ServerCallContext context)
        {
            var success = await RemoveValue(request.PlayerId, request.ResourceId);
            return new ResourceRequestResponse
            {
                Success = success
            };
        }

        public override async Task<ResourceRequestResponse> SetResourceValue(
            SetResourceValueRequest request,
            ServerCallContext context)
        {
            var success = await SetValue(request.PlayerId, request.ResourceId,
                                   request.ResourceValue.ToObject());
            return new ResourceRequestResponse
            {
                Success = success
            };
        }

        public override async Task SubscribeAllResourceValues(
            SubscribeAllResourceValuesRequest request,
            IServerStreamWriter<ResourceValuesUpdate>
                responseStream,
            ServerCallContext context)
        {
            var update = new ResourceValuesUpdate
            {
                ResourceValueChanges = new Google.Protobuf.WellKnownTypes.Struct()
            };

            void ResourcesOnCollectionChanged(object sender,
                                                    NotifyCollectionChangedEventArgs e)
            {
                var (changes, removals) = e.AsChangesAndRemovals<string>();

                foreach (var change in changes)
                    update.ResourceValueChanges.Fields[change] = resources[change].ToProtobufValue();
                foreach (var removal in removals)
                    update.ResourceValueRemovals.Add(removal);
            }

            resources.CollectionChanged += ResourcesOnCollectionChanged;
            while (true)
            {
                await Task.Delay(10);
                if (update.ResourceValueChanges.Fields.Any() || update.ResourceValueRemovals.Any())
                {
                    var toSend = update;
                    update = new ResourceValuesUpdate
                    {
                        ResourceValueChanges = new Google.Protobuf.WellKnownTypes.Struct()
                    };
                    await responseStream.WriteAsync(toSend);
                }
            }
        }

        private int playerCount = 1;

        public override async Task<CreatePlayerResponse> CreatePlayer(CreatePlayerRequest request, ServerCallContext context)
        {
            return new CreatePlayerResponse
            {
                PlayerId = $"player{playerCount++}"
            };
        }

        public ServerServiceDefinition BindService()
        {
            return Narupa.Protocol.Multiplayer.Multiplayer.BindService(this);
        }
    }
}