using System;
using Narupa.Core.Async;
using Narupa.Frame;
using Narupa.Grpc.Frame;
using Narupa.Grpc.Stream;
using Narupa.Protocol.Trajectory;

namespace Narupa.Grpc.Trajectory
{
    /// <summary>
    /// Adapts <see cref="TrajectoryClient" /> into an
    /// <see cref="ITrajectorySnapshot" /> where
    /// <see cref="ITrajectorySnapshot.CurrentFrame" /> is the latest received frame.
    /// </summary>
    public class TrajectorySession : ITrajectorySnapshot, IDisposable
    {
        /// <inheritdoc cref="ITrajectorySnapshot.CurrentFrame" />
        public Narupa.Frame.Frame CurrentFrame => trajectorySnapshot.CurrentFrame;
        
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

            trajectoryClient = new TrajectoryClient(connection);
            frameStream = trajectoryClient.SubscribeLatestFrames(1f / 30f);
            frameStream.MessageReceived += Callback;
            frameStream.StartReceiving().AwaitInBackgroundIgnoreCancellation();

            void Callback(GetFrameResponse response)
            {
                var (frame, changes) = FrameConverter.ConvertFrame(response.Frame, CurrentFrame);
                trajectorySnapshot.SetCurrentFrame(frame, changes);
                CurrentFrameIndex = (int) response.FrameIndex;
            }
        }

        /// <summary>
        /// Close the current trajectory client.
        /// </summary>
        public void CloseClient()
        {
            trajectoryClient?.CloseAndCancelAllSubscriptions();
            trajectoryClient = null;
        }

        /// <inheritdoc cref="IDisposable.Dispose" />
        public void Dispose()
        {
            trajectoryClient?.Dispose();
            frameStream?.Dispose();
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
    }
}