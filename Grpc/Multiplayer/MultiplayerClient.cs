// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Threading;
using JetBrains.Annotations;
using Narupa.Multiplayer;
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
        GrpcClient<Multiplayer.Multiplayer.MultiplayerClient>
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
        /// Starts an <see cref="IncomingStream{TIncoming}" /> on which the server
        /// provides the latest avatar positions for each player, at the 
        /// requested time interval (in seconds).
        /// </summary>
        /// <param name="updateInterval">
        /// How many seconds the service should wait and aggregate updates 
        /// between  sending them to us.
        /// </param>
        /// <remarks>
        /// Corresponds to the SubscribePlayerAvatars gRPC call.
        /// </remarks>
        public IncomingStream<Avatar> SubscribeAvatars(float updateInterval = DefaultUpdateInterval,
                                                       string ignorePlayerId = "", 
                                                       CancellationToken externalToken = default)
        {
            var request = new SubscribePlayerAvatarsRequest
            {
                IgnorePlayerId = ignorePlayerId,
                UpdateInterval = updateInterval,
            };

            return GetIncomingStream(Client.SubscribePlayerAvatars, request, externalToken);
        }

        /// <summary>
        /// Starts an <see cref="OutgoingStream{Avatar, StreamEndedResponse}"/> 
        /// where updates to the local player's avatar can be published to the
        /// service.
        /// </summary>
        /// <remarks>
        /// Corresponds to the UpdatePlayerAvatar gRPC call.
        /// </remarks>
        public OutgoingStream<Avatar, StreamEndedResponse> PublishAvatar(
            string playerId,
            CancellationToken externalToken = default) 
        {
            return GetOutgoingStream(Client.UpdatePlayerAvatar, externalToken);
        }

        /// <summary>
        /// Starts an <see cref="IncomingStream{ResourceValuesUpdate}" /> on 
        /// which the server provides updates to the shared key/value store at 
        /// the requested time interval (in seconds).
        /// </summary>
        /// <remarks>
        /// Corresponds to the SubscribeAllResourceValues gRPC call.
        /// </remarks>
        public IncomingStream<ResourceValuesUpdate> SubscribeAllResourceValues(float updateInterval = 0,
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
