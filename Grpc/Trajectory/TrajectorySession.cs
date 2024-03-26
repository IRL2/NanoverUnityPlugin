using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Nanover.Core;
using Nanover.Core.Async;
using Nanover.Frame;
using Nanover.Grpc.Frame;
using Nanover.Grpc.Stream;
using Nanover.Protocol.Trajectory;
using UnityEngine;

namespace Nanover.Grpc.Trajectory
{
    /// <summary>
    /// Adapts <see cref="TrajectoryClient" /> into an
    /// <see cref="ITrajectorySnapshot" /> where
    /// <see cref="ITrajectorySnapshot.CurrentFrame" /> is the latest received frame.
    /// </summary>
    public class TrajectorySession : ITrajectorySnapshot, IDisposable
    {
        /// <inheritdoc cref="ITrajectorySnapshot.CurrentFrame" />
        public Nanover.Frame.Frame CurrentFrame => trajectorySnapshot.CurrentFrame;
        
        public int CurrentFrameIndex { get; private set; }

        /// <inheritdoc cref="ITrajectorySnapshot.FrameChanged" />
        public event FrameChanged FrameChanged;

        /// <summary>
        /// Underlying <see cref="TrajectorySnapshot" /> for tracking
        /// <see cref="CurrentFrame" />.
        /// </summary>
        private readonly TrajectorySnapshot trajectorySnapshot = new TrajectorySnapshot();

        /// <summary>
        /// Underlying TrajectoryClient for receiving new frames.
        /// </summary>
        private TrajectoryClient trajectoryClient;

        private IncomingStream<GetFrameResponse> frameStream;

        public TrajectorySession()
        {
            trajectorySnapshot.FrameChanged += (sender, args) => FrameChanged?.Invoke(sender, args);
        }

        /// <summary>
        /// Connect to a trajectory service over the given connection and
        /// listen in the background for frame changes. Closes any existing
        /// client.
        /// </summary>
        public void OpenClient(GrpcConnection connection)
        {
            CloseClient();
            trajectorySnapshot.Clear();

            trajectoryClient = new TrajectoryClient(connection);
            frameStream = trajectoryClient.SubscribeLatestFrames(1f / 30f);
            BackgroundIncomingStreamReceiver<GetFrameResponse>.Start(frameStream, ReceiveFrame, Merge);

            // Integrating frames from the buffer with the current frame
            void ReceiveFrame(GetFrameResponse response)
            {
                CurrentFrameIndex = (int) response.FrameIndex;

                var clear = response.Frame.Values.ContainsKey("_clear") 
                         || response.FrameIndex == 0;
                var nextFrame = response.Frame;
                var prevFrame = clear ? null : CurrentFrame;

                var (frame, changes) = FrameConverter.ConvertFrame(nextFrame, prevFrame);

                trajectorySnapshot.SetCurrentFrame(frame, changes);
            }

            // Aggregating frames while they wait in the buffer
            void Merge(GetFrameResponse dest, GetFrameResponse toMerge)
            {
                if (toMerge.FrameIndex == 0)
                {
                    dest.Frame = new FrameData();
                    // it's possible a later frame will be merged, erasing the
                    // 0 frame index, so record it in a special field too
                    dest.Frame.Values["_clear"] = Value.ForBool(true);
                }

                dest.FrameIndex = toMerge.FrameIndex;
                foreach (var (key, array) in toMerge.Frame.Arrays)
                    dest.Frame.Arrays[key] = array;
                foreach (var (key, value) in toMerge.Frame.Values)
                    dest.Frame.Values[key] = value;
            }
        }

        /// <summary>
        /// Close the current trajectory client.
        /// </summary>
        public void CloseClient()
        {
            trajectoryClient?.CloseAndCancelAllSubscriptions();
            trajectoryClient?.Dispose();
            trajectoryClient = null;

            frameStream?.CloseAsync();
            frameStream?.Dispose();
            frameStream = null;
        }

        /// <inheritdoc cref="IDisposable.Dispose" />
        public void Dispose()
        {
            CloseClient();
        }
        
        /// <inheritdoc cref="TrajectoryClient.CommandPlay"/>
        public void Play()
        {
            trajectoryClient?.RunCommandAsync(TrajectoryClient.CommandPlay);
        }
        
        /// <inheritdoc cref="TrajectoryClient.CommandPause"/>
        public void Pause()
        {
            trajectoryClient?.RunCommandAsync(TrajectoryClient.CommandPause);
        }
        
        /// <inheritdoc cref="TrajectoryClient.CommandReset"/>
        public void Reset()
        {
            trajectoryClient?.RunCommandAsync(TrajectoryClient.CommandReset);
        }
        
        /// <inheritdoc cref="TrajectoryClient.CommandStep"/>
        public void Step()
        {
            trajectoryClient?.RunCommandAsync(TrajectoryClient.CommandStep);
        }

        // TODO: handle the non-existence of these commands
        /// <inheritdoc cref="TrajectoryClient.CommandGetSimulationsListing"/>
        public async Task<List<string>> GetSimulationListing()
        {
            var result = await trajectoryClient?.RunCommandAsync(TrajectoryClient.CommandGetSimulationsListing);
            var listing = result["simulations"] as List<object>;
            return listing?.ConvertAll(o => o as string) ?? new List<string>();
        }

        /// <inheritdoc cref="TrajectoryClient.CommandSetSimulationIndex"/>
        public void SetSimulationIndex(int index)
        {
            trajectoryClient?.RunCommandAsync(TrajectoryClient.CommandSetSimulationIndex, new Dictionary<string, object> { { "index", index } });
        }

        public void RunCommand(string name, Dictionary<string, object> commands)
        {
            trajectoryClient?.RunCommandAsync(name, commands);
        }

        public TrajectoryClient Client => trajectoryClient;
    }
}