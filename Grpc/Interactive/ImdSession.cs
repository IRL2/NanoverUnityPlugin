// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Narupa.Core;
using Narupa.Core.Async;
using Narupa.Grpc;
using Narupa.Grpc.Interactive;
using Narupa.Grpc.Stream;
using Narupa.Protocol.Imd;
using UnityEngine;

namespace Narupa.Session
{
    /// <summary>
    /// Provides functionality to manage the sending of particle interactions
    /// over an <see cref="ImdClient"/>.
    /// </summary>
    public class ImdSession : IDisposable
    {
        public struct InteractionData
        {
            public string InteractionId { get; set; }
            public Vector3 Position { get; set; }
            public int[] ParticleIds { get; set; }
        }

        public OutgoingStreamCollection<ParticleInteraction, InteractionEndReply> 
            InteractionStreams { get; private set; }

        /// <summary>
        /// Dictionary of all currently known interactions.
        /// </summary>
        public Dictionary<string, InteractionData> Interactions { get; } 
            = new Dictionary<string, InteractionData>();

        private ImdClient client;

        private Dictionary<string, ParticleInteraction> pendingInteractions
            = new Dictionary<string, ParticleInteraction>();

        private IncomingStream<InteractionsUpdate> IncomingInteractionsUpdates { get; set; }

        private Task flushingTask;

        /// <summary>
        /// Connect to an IMD service over the given connection. Closes any 
        /// existing client.
        /// </summary>
        public void OpenClient(GrpcConnection connection)
        {
            CloseClient();

            client = new ImdClient(connection);
            InteractionStreams =
                new OutgoingStreamCollection<ParticleInteraction, InteractionEndReply>(
                    client.PublishInteractions);

            IncomingInteractionsUpdates = client.SubscribeAllInteractions(1 / 30f);
            IncomingInteractionsUpdates.MessageReceived += OnInteractionsUpdateReceived;
            IncomingInteractionsUpdates.StartReceiving().AwaitInBackgroundIgnoreCancellation();

            if (flushingTask == null)
            {
                flushingTask = FlushingLoop();
                flushingTask.AwaitInBackground();
            }
        }

        /// <summary>
        /// Close the current IMD client and dispose all streams.
        /// </summary>
        public void CloseClient()
        {
            client?.CloseAndCancelAllSubscriptions();
            client = null;

            InteractionStreams?.Dispose();
            InteractionStreams = null;
            IncomingInteractionsUpdates?.Dispose();
            IncomingInteractionsUpdates = null;
        }

        /// <summary>
        /// Set the active interaction for a particular stream id. If the 
        /// stream doesn't exist, it will be started.
        /// </summary>
        public void SetInteraction(string id, 
                                   Vector3 position,
                                   float forceScale = 100,
                                   string forceModel = "spring",
                                   IEnumerable<int> particles = null,
                                   Dictionary<string, object> properties = null)
        {
            var interaction = new ParticleInteraction
            {
                InteractionId = id,
                Particles = { particles?.Select(i => (uint) i) ?? new uint[0] },
                Position = { position.x, position.y, position.z },
                Scale = forceScale,
                Type = forceModel
            };

            if (properties != null)
            {
                foreach (var (key, value) in properties)
                    interaction.Properties.Fields[key] = value.ToProtobufValue();
            }

            SetInteraction(id, interaction);
        }

        /// <summary>
        /// Stop the active interaction for a particular stream id, if it 
        /// exists.
        /// </summary>
        public void UnsetInteraction(string id)
        {
            pendingInteractions[id] = null;
            Interactions.Remove(id);
        }

        /// <summary>
        /// Set the active interaction for a particular stream id. If the
        /// interaction to be set is null, the stream will be ended. If the
        /// stream doesn't exist, it was be started.
        /// </summary>
        public void SetInteraction(string id, ParticleInteraction interaction)
        {
            pendingInteractions[id] = interaction;
            Interactions[interaction.InteractionId] = ParticleInteractionToData(interaction);
        }

        /// <summary>
        /// Send the latest state for all interactions changed since the last
        /// flush.
        /// </summary>
        public void FlushInteractions()
        {
            foreach (var pair in pendingInteractions)
            {
                var id = pair.Key;
                var interaction = pair.Value;

                if (interaction == null)
                {
                    if (InteractionStreams.HasStream(id))
                        InteractionStreams.EndStreamAsync(id)
                                          .AwaitInBackgroundIgnoreCancellation();
                }
                else
                {
                    if (!InteractionStreams.HasStream(id))
                        InteractionStreams.StartStream(id)
                                          .AwaitInBackgroundIgnoreCancellation();

                    InteractionStreams.QueueMessageAsync(id, interaction)
                                      .AwaitInBackgroundIgnoreCancellation();
                }
            }

            pendingInteractions.Clear();
        }

        /// <inheritdoc cref="IDisposable.Dispose" />
        public void Dispose()
        {
            client?.Dispose();
            InteractionStreams?.Dispose();
        }

        private async Task FlushingLoop()
        {
            while (client != null)
            {
                FlushInteractions();

                await Task.Delay(1000 / 60);
            }

            flushingTask = null;
        }

        private void OnInteractionsUpdateReceived(InteractionsUpdate update)
        {
            foreach (var interactionId in update.Removals)
            {
                Interactions.Remove(interactionId);
            }

            foreach (var interaction in update.UpdatedInteractions)
            {
                Interactions[interaction.InteractionId] = ParticleInteractionToData(interaction);
            }
        }

        private static InteractionData ParticleInteractionToData(ParticleInteraction interaction)
        {
            var data = new InteractionData
            {
                InteractionId = interaction.InteractionId,
                Position = interaction.Position.GetVector3(),
                ParticleIds = new int[interaction.Particles.Count],
            };

            for (int i = 0; i < data.ParticleIds.Length; ++i)
                data.ParticleIds[i] = (int) interaction.Particles[i];

            return data;
        }
    }
}