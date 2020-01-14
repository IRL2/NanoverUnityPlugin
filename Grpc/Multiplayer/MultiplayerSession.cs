// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Narupa.Protocol.Multiplayer;
using Narupa.Network;
using UnityEngine;
using Avatar = Narupa.Protocol.Multiplayer.Avatar;
using System.Linq;
using Grpc.Core;
using Narupa.Core.Async;
using Narupa.Core.Math;
using Narupa.Grpc;
using Narupa.Grpc.Multiplayer;
using Narupa.Grpc.Stream;
using UnityEngine.Profiling;

namespace Narupa.Session
{
    /// <summary>
    /// Manages the state of a single user engaging in multiplayer. Tracks
    /// local and remote avatars and maintains a copy of the latest values in
    /// the shared key/value store.
    /// </summary>
    public sealed class MultiplayerSession : IDisposable
    {
        public const string HeadsetName = "headset";
        public const string LeftHandName = "hand.left";
        public const string RightHandName = "hand.right";
        public const string SimulationPoseKey = "scene";

        public MultiplayerSession()
        {
            SimulationPose =
                new MultiplayerResource<Transformation>(this, SimulationPoseKey, PoseFromObject,
                                                        PoseToObject);
        }

        /// <summary>
        /// The transformation of the simulation box.
        /// </summary>
        public readonly MultiplayerResource<Transformation> SimulationPose;

        /// <summary>
        /// Dictionary of player ids to their last known avatar.
        /// </summary>
        public Dictionary<string, MultiplayerAvatar> Avatars { get; }
            = new Dictionary<string, MultiplayerAvatar>();

        /// <summary>
        /// Dictionary of the currently known shared state.
        /// </summary>
        public Dictionary<string, object> SharedStateDictionary { get; } =
            new Dictionary<string, object>();

        /// <summary>
        /// Is there an open client on this session?
        /// </summary>
        public bool IsOpen => client != null;

        /// <summary>
        /// Has this session created a user?
        /// </summary>
        public bool HasPlayer => PlayerId != null;

        /// <summary>
        /// Username of the current player, if any.
        /// </summary>
        public string PlayerName { get; private set; }

        /// <summary>
        /// ID of the current player, if any.
        /// </summary>
        public string PlayerId { get; private set; }

        /// <summary>
        /// How many milliseconds to put between sending updates to our avatar.
        /// </summary>
        public int AvatarPublishInterval { get; set; } = 1000 / 30;

        /// <summary>
        /// How many milliseconds to put between sending our requested value
        /// changes.
        /// </summary>
        public int ValuePublishInterval { get; set; } = 1000 / 30;

        /// <summary>
        /// The interval at which avatar updates should be sent to this client.
        /// </summary>
        public const float AvatarUpdateInterval = 1f / 30f;

        private MultiplayerClient client;

        private OutgoingStream<Avatar, StreamEndedResponse> OutgoingAvatar { get; set; }
        private IncomingStream<Avatar> IncomingAvatars { get; set; }
        private IncomingStream<ResourceValuesUpdate> IncomingValueUpdates { get; set; }

        private Dictionary<string, MultiplayerAvatar> pendingAvatars
            = new Dictionary<string, MultiplayerAvatar>();

        private Dictionary<string, object> pendingValues
            = new Dictionary<string, object>();

        private Task avatarFlushingTask, valueFlushingTask;

        public event Action<string, object> SharedStateDictionaryKeyUpdated;

        public event Action<string> SharedStateDictionaryKeyRemoved;

        /// <summary>
        /// Connect to a Multiplayer service over the given connection. 
        /// Closes any existing client.
        /// </summary>
        public void OpenClient(GrpcConnection connection)
        {
            CloseClient();

            client = new MultiplayerClient(connection);

            if (avatarFlushingTask == null)
            {
                avatarFlushingTask = CallbackInterval(FlushAvatars, AvatarPublishInterval);
                avatarFlushingTask.AwaitInBackground();
            }

            if (valueFlushingTask == null)
            {
                valueFlushingTask = CallbackInterval(FlushValues, ValuePublishInterval);
                valueFlushingTask.AwaitInBackground();
            }
        }

        /// <summary>
        /// Close the current Multiplayer client and dispose all streams.
        /// </summary>
        public void CloseClient()
        {
            client?.CloseAndCancelAllSubscriptions();
            client = null;

            IncomingAvatars?.Dispose();
            IncomingAvatars = null;
            OutgoingAvatar?.Dispose();
            OutgoingAvatar = null;

            PlayerName = null;
            PlayerId = null;

            Avatars.Clear();
            pendingAvatars.Clear();
        }

        /// <summary>
        /// Create a new multiplayer with the given username, subscribe avatar 
        /// and value updates, and begin publishing our avatar.
        /// </summary>
        public async Task JoinMultiplayer(string playerName)
        {
            if (HasPlayer)
                throw new InvalidOperationException($"Multiplayer already joined as {PlayerName}");

            var response = await client.CreatePlayer(playerName);

            PlayerName = playerName;
            PlayerId = response.PlayerId;

            StartAvatars();

            IncomingValueUpdates = client.SubscribeAllResourceValues();
            IncomingValueUpdates.MessageReceived += OnResourceValuesUpdateReceived;
            IncomingValueUpdates.StartReceiving().AwaitInBackgroundIgnoreCancellation();
        }

        public void StartAvatars()
        {
            try
            {
                OutgoingAvatar = client.PublishAvatar(PlayerId);
                IncomingAvatars = client.SubscribeAvatars(updateInterval: AvatarUpdateInterval,
                                                          ignorePlayerId: PlayerId);

                IncomingAvatars.MessageReceived += OnAvatarReceived;

                OutgoingAvatar.StartSending().AwaitInBackgroundIgnoreCancellation();
                IncomingAvatars.StartReceiving().AwaitInBackgroundIgnoreCancellation();
            }
            catch (RpcException e)
            {
            }
        }

        /// <summary>
        /// Set the headset and hand poses of our avatar.
        /// </summary>
        public void SetVRAvatar(Transformation? headset = null,
                                Transformation? leftHand = null,
                                Transformation? rightHand = null)
        {
            var avatar = new MultiplayerAvatar
            {
                PlayerId = PlayerId,
            };

            if (headset is Transformation headsetPose)
                avatar.Components[HeadsetName] = headsetPose;

            if (leftHand is Transformation leftHandPose)
                avatar.Components[LeftHandName] = leftHandPose;

            if (rightHand is Transformation rightHandPose)
                avatar.Components[RightHandName] = rightHandPose;

            SetVRAvatar(avatar);
        }

        /// <summary>
        /// Set the headset and hand poses of our avatar.
        /// </summary>
        public void SetVRAvatar(MultiplayerAvatar avatar)
        {
            Avatars[avatar.PlayerId] = avatar;
            pendingAvatars[avatar.PlayerId] = avatar;
        }

        /// <summary>
        /// Set our avatar to empty.
        /// </summary>
        public void SetVRAvatarEmpty()
        {
            SetVRAvatar(new MultiplayerAvatar
            {
                PlayerId = PlayerId
            });
        }

        /// <summary>
        /// Set the given key in the shared state dictionary, which will be
        /// sent to the server according in the future according to the publish 
        /// interval.
        /// </summary>
        public void SetSharedState(string key, object value)
        {
            pendingValues[key] = value.ToProtobufValue();
        }

        /// <summary>
        /// Get a key in the shared state dictionary.
        /// </summary>
        public object GetSharedState(string key)
        {
            return SharedStateDictionary.TryGetValue(key, out var value) ? value : null;
        }

        /// <summary>
        /// Attempt to gain exclusive write access to the shared value of the given key.
        /// </summary>
        public async Task<bool> LockResource(string id)
        {
            return await client.LockResource(PlayerId, id);
        }

        /// <summary>
        /// Release the lock on the given object of a given key.
        /// </summary>
        public async Task<bool> ReleaseResource(string id)
        {
            return await client.ReleaseResource(PlayerId, id);
        }

        /// <inheritdoc cref="IDisposable.Dispose" />
        public void Dispose()
        {
            SetVRAvatarEmpty();
            FlushAvatars();
            FlushValues();

            client?.Dispose();
            OutgoingAvatar?.Dispose();
            IncomingAvatars?.Dispose();
        }

        private void OnAvatarReceived(Avatar avatar)
        {
            // ignore remote ideas about our own value
            if (avatar.PlayerId == PlayerId)
                return;

            Avatars[avatar.PlayerId] = ProtoAvatarToClientAvatar(avatar);
        }

        private void OnResourceValuesUpdateReceived(ResourceValuesUpdate update)
        {
            if (update.ResourceValueChanges != null)
            {
                foreach (var pair in update.ResourceValueChanges.Fields)
                {
                    var value = pair.Value.ToObject();
                    SharedStateDictionary[pair.Key] = value;
                    SharedStateDictionaryKeyUpdated?.Invoke(pair.Key, value);
                }
            }

            if (update.ResourceValueRemovals != null)
            {
                foreach (var key in update.ResourceValueRemovals)
                {
                    SharedStateDictionaryKeyRemoved?.Invoke(key);
                }
            }
        }

        private void FlushAvatars()
        {
            if (!IsOpen || !HasPlayer)
                return;

            foreach (var avatar in pendingAvatars.Values)
            {
                OutgoingAvatar.QueueMessageAsync(ClientAvatarToProtoAvatar(avatar))
                              .AwaitInBackgroundIgnoreCancellation();
            }

            pendingAvatars.Clear();
        }

        private void FlushValues()
        {
            if (!IsOpen || !HasPlayer)
                return;

            foreach (var pair in pendingValues)
            {
                client.SetResourceValue(PlayerId, pair.Key, pair.Value.ToProtobufValue())
                      .AwaitInBackgroundIgnoreCancellation();
            }

            pendingValues.Clear();
        }

        private static async Task CallbackInterval(Action callback, int interval)
        {
            while (true)
            {
                callback();
                await Task.Delay(interval);
            }
        }

        private static Avatar ClientAvatarToProtoAvatar(MultiplayerAvatar clientAvatar)
        {
            var components = clientAvatar
                             .Components
                             .Select(pair => ComponentFromPose(pair.Key, pair.Value));

            var protoAvatar = new Avatar
            {
                PlayerId = clientAvatar.PlayerId,
                Components =
                {
                    components
                },
            };

            return protoAvatar;

            AvatarComponent ComponentFromPose(string name, Transformation? pose)
            {
                var component = new AvatarComponent
                {
                    Name = name
                };

                if (pose is Transformation validPose)
                {
                    component.Position.PutValues(validPose.Position);
                    component.Rotation.PutValues(validPose.Rotation);
                }

                return component;
            }
        }

        private static MultiplayerAvatar ProtoAvatarToClientAvatar(Avatar protoAvatar)
        {
            var clientAvatar = new MultiplayerAvatar
            {
                PlayerId = protoAvatar.PlayerId,
            };

            foreach (var component in protoAvatar.Components)
            {
                clientAvatar.Components[component.Name] = PoseFromComponent(component);
            }

            return clientAvatar;

            Transformation? PoseFromComponent(AvatarComponent component)
            {
                if (component.Position.Count != 3 || component.Rotation.Count != 4)
                    return null;

                return new Transformation(component.Position.GetVector3(),
                                          component.Rotation.GetQuaternion(),
                                          Vector3.one);
            }
        }

        private static object PoseToObject(Transformation pose)
        {
            var data = new object[]
            {
                pose.Position.x, pose.Position.y, pose.Position.z, pose.Rotation.x, pose.Rotation.y,
                pose.Rotation.z, pose.Rotation.w, pose.Scale.x, pose.Scale.y, pose.Scale.z,
            };

            return data;
        }

        private static Transformation PoseFromObject(object @object)
        {
            if (@object is List<object> list)
            {
                var values = list.Select(value => Convert.ToSingle(value)).ToList();
                var position = new Vector3(values[0], values[1], values[2]);
                var rotation = new Quaternion(values[3], values[4], values[5], values[6]);
                var scale = new Vector3(values[7], values[8], values[9]);

                return new Transformation(position, rotation, scale);
            }

            throw new ArgumentOutOfRangeException();
        }

        public MultiplayerResource<object> GetSharedResource(string key)
        {
            return new MultiplayerResource<object>(this, key);
        }
    }

    public sealed class MultiplayerAvatar
    {
        public string PlayerId { get; set; }

        public Dictionary<string, Transformation?> Components { get; }
            = new Dictionary<string, Transformation?>();
    }
}