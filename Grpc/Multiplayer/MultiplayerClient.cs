// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Threading;
using JetBrains.Annotations;
using Narupa.Protocol.Multiplayer;
using System.Threading.Tasks;
using Narupa.Grpc;
using Narupa.Grpc.Stream;
using Value = Google.Protobuf.WellKnownTypes.Value;

namespace Narupa.Network
{
    /// <summary>
    /// Wraps a <see cref="Multiplayer.MultiplayerClient" /> and
    /// provides access to avatars and the shared key/value store on the server
    /// over a <see cref="GrpcConnection" />.
    /// </summary>
    public class MultiplayerClient :
        GrpcClient<Multiplayer.MultiplayerClient>
    {
        // Chosen as an acceptable minimum rate that should ideally be 
        // explicitly increased.
        private const float DefaultUpdateInterval = 1f / 30f;

        public MultiplayerClient([NotNull] GrpcConnection connection) : base(connection)
        {
        }

        /// <summary>
        /// Requests a new player ID from the server.
        /// </summary>
        /// <remarks>
        /// Corresponds to the CreatePlayer gRPC call.
        /// </remarks>
        public async Task<CreatePlayerResponse> CreatePlayer(string name)
        {
            var request = new CreatePlayerRequest
            {
                PlayerName = name,
            };

            return await Client.CreatePlayerAsync(request);
        }

        /// <summary>
        /// Starts an <see cref="IncomingStream{ResourceValuesUpdate}" /> on 
        /// which the server provides updates to the shared key/value store at 
        /// the requested time interval (in seconds).
        /// </summary>
        /// <remarks>
        /// Corresponds to the SubscribeAllResourceValues gRPC call.
        /// </remarks>
        public IncomingStream<ResourceValuesUpdate> SubscribeAllResourceValues(float updateInterval = DefaultUpdateInterval,
                                                                               CancellationToken externalToken = default)
        {
            var request = new SubscribeAllResourceValuesRequest
            {
                UpdateInterval = updateInterval,
            };

            return GetIncomingStream(Client.SubscribeAllResourceValues, request, externalToken);
        }

        /// <summary>
        /// Attempts a change, on behalf of the given player, to the shared
        /// key value store. This will fail if the key is locked by someone 
        /// other than the player making the attempt.
        /// </summary>
        /// <remarks>
        /// Corresponds to the SetResourceValueAsync gRPC call.
        /// </remarks>
        public async Task<bool> SetResourceValue(string playerId, string key, Value value)
        {
            var request = new SetResourceValueRequest
            {
                PlayerId = playerId,
                ResourceId = key,
                ResourceValue = value,
            };

            var response = await Client.SetResourceValueAsync(request);

            return response.Success;
        }
        
        /// <summary>
        /// Attempts to remove a key, on behalf of the given player, from the shared
        /// key value store. This will fail if the key is locked by someone 
        /// other than the player making the attempt.
        /// </summary>
        /// <remarks>
        /// Corresponds to the RemoveResourceKeyAsync gRPC call.
        /// </remarks>
        public async Task<bool> RemoveResourceKey(string playerId, string key)
        {
            var request = new RemoveResourceKeyRequest()
            {
                PlayerId = playerId,
                ResourceId = key
            };

            var response = await Client.RemoveResourceKeyAsync(request);

            return response.Success;
        }

        /// <summary>
        /// Attempts to lock, on behalf of the given player, a key in the 
        /// shared key value store. This will fail if the key is locked by 
        /// someone other than the player making the attempt.
        /// </summary>
        /// <remarks>
        /// Corresponds to the AcquireResourceLockAsync gRPC call.
        /// </remarks>
        public async Task<bool> LockResource(string playerId, string key)
        {
            var request = new AcquireLockRequest
            {
                PlayerId = playerId,
                ResourceId = key,
            };

            var response = await Client.AcquireResourceLockAsync(request, 
                                                                 null, 
                                                                 DateTime.UtcNow.AddSeconds(1));

            return response.Success;
        }

        /// <summary>
        /// Attempts, on behalf of the given player, to unlock a key in the
        /// shared key value store. This will fail if the key is not locked by
        /// that player.
        /// </summary>
        /// <remarks>
        /// Corresponds to the ReleaseResourceLockAsync gRPC call.
        /// </remarks>
        public async Task<bool> ReleaseResource(string playerId, string key)
        {
            var request = new ReleaseLockRequest
            {
                PlayerId = playerId,
                ResourceId = key,
            };

            var response = await Client.ReleaseResourceLockAsync(request);

            return response.Success;
        }
    }
}
