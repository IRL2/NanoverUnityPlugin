// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Narupa.Multiplayer;
using Narupa.Network;
using UnityEngine;

using Avatar = Narupa.Multiplayer.Avatar;
using System.Linq;
using Narupa.Core.Async;
using Narupa.Core.Math;
using Narupa.Grpc;
using Narupa.Grpc.Stream;

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

        /// <summary>
        /// Dictionary of player ids to their last known avatar.
        /// </summary>
        public Dictionary<string, MultiplayerAvatar> Avatars { get; } 
            = new Dictionary<string, MultiplayerAvatar>();

        /// <summary>
        /// Dictionary of the currently known shared state.
        /// </summary>
        public Dictionary<string, object> SharedStateDictionary { get; } = new Dictionary<string, object>();

        /// <summary>
        /// Currently known pose of the simulation space.
        /// </summary>
        public Transformation SimulationPose { get; private set; } = Transformation.Identity;

        /// <summary>
        /// Is there an open client on this session?
        /// </summary>
        public bool IsOpen => client != null;
        
        /// <summary>
        /// Has this session created a user?
        /// </summary>
        public bool HasPlayer => PlayerId != null;

        /// <summary>
        /// Do we know we have a lock on the simulation pose key in the shared
        /// state dictionary?
        /// </summary>
        public bool HasSimulationPoseLock { get; private set; }

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

        private MultiplayerClient client;

        private OutgoingStream<Avatar, StreamEndedResponse> OutgoingAvatar { get; set; }
        private IncomingStream<Avatar> IncomingAvatars { get; set; }
        private IncomingStream<ResourceValuesUpdate> IncomingValueUpdates { get; set; }

        private Dictionary<string, MultiplayerAvatar> pendingAvatars
            = new Dictionary<string, MultiplayerAvatar>();
        private Dictionary<string, object> pendingValues
            = new Dictionary<string, object>();

        private Task avatarFlushingTask, valueFlushingTask;

        public event Action<string, object> SharedStateDictionaryKeyChanged;

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

            if (valueFlushingTask ==  null)
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
            HasSimulationPoseLock = false;

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

            OutgoingAvatar = client.PublishAvatar(PlayerId);
            IncomingAvatars = client.SubscribeAvatars(updateInterval: 1f / 30f,
                                                      ignorePlayerId: PlayerId);

            IncomingAvatars.MessageReceived += OnAvatarReceived;

            OutgoingAvatar.StartSending().AwaitInBackgroundIgnoreCancellation();
            IncomingAvatars.StartReceiving().AwaitInBackgroundIgnoreCancellation();

            IncomingValueUpdates = client.SubscribeAllResourceValues();
            IncomingValueUpdates.MessageReceived += OnResourceValuesUpdateReceived;
            IncomingValueUpdates.StartReceiving().AwaitInBackgroundIgnoreCancellation();
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
            SetVRAvatar(new MultiplayerAvatar { PlayerId = PlayerId });
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
        /// Attempt to gain exclusive write access to the shared "scene" value, 
        /// which controls the pose of the simulation scene.
        /// </summary>
        public async Task<bool> LockSimulationPose()
        {
            bool success = await client.LockResource(PlayerId, SimulationPoseKey);

            HasSimulationPoseLock = success;

            return success;
        }

        /// <summary>
        /// Attempt to release our lock on the shared "scene" value.
        /// </summary>
        public async Task<bool> ReleaseSimulationPose()
        {
            bool success = await client.ReleaseResource(PlayerId, SimulationPoseKey);

            HasSimulationPoseLock = false;

            return success;
        }

        /// <summary>
        /// Set the simulation pose to a given value.
        /// </summary>
        public void SetSimulationPose(Transformation pose)
        {
            if (!HasSimulationPoseLock)
                throw new InvalidOperationException(
                    "You must lock the simulation pose before setting it!");

            SetSharedState(SimulationPoseKey, PoseToObject(pose));
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
            foreach (var pair in update.ResourceValueChanges)
            {
                var value = pair.Value.ToObject();
                SharedStateDictionary[pair.Key] = value;
                SharedStateDictionaryKeyChanged?.Invoke(pair.Key, value);
            }

            if (update.ResourceValueChanges.ContainsKey(SimulationPoseKey))
            {
                SimulationPose = PoseFromObject(SharedStateDictionary[SimulationPoseKey]);
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
                Component = { components },
            };

            return protoAvatar;

            AvatarComponent ComponentFromPose(string name, Transformation? pose)
            {
                var component = new AvatarComponent { Name = name };

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

            foreach (var component in protoAvatar.Component)
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
                pose.Position.x, pose.Position.y, pose.Position.z,
                pose.Rotation.x, pose.Rotation.y, pose.Rotation.z, pose.Rotation.w,
                pose.Scale.x, pose.Scale.y, pose.Scale.z,
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
    }

    public sealed class MultiplayerAvatar
    {
        public string PlayerId { get; set; }
        public Dictionary<string, Transformation?> Components { get; } 
            = new Dictionary<string, Transformation?>();
    }
}