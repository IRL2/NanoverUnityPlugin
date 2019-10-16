// Copyright (c) Intangible Realities Laboratory. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Grpc.Core;
using Narupa.Core.Async;
using Narupa.Protocol.Trajectory;

namespace Narupa.Grpc.Tests.Trajectory
{
    /// <summary>
    /// Simple GRPC server with a TrajectoryService that responds to
    /// SubscribeLatestFrames
    /// with a queue of provided FrameData, with an optional delay.
    /// </summary>
    public class QueueTrajectoryServer : IAsyncClosable
    {
        private Server server;
        private readonly QueueTrajectoryService service;

        public int Delay
        {
            get => service.Delay;
            set => service.Delay = value;
        }

        public QueueTrajectoryServer(int port, params FrameData[] data)
        {
            service = new QueueTrajectoryService(data);
            server = new Server
            {
                Services =
                {
                    TrajectoryService.BindService(service)
                },
                Ports =
                {
                    new ServerPort("localhost", port, ServerCredentials.Insecure)
                }
            };
            server.Start();
        }

        public async Task CloseAsync()
        {
            if (server == null)
                return;

            await server.KillAsync();
            server = null;
        }
    }
}