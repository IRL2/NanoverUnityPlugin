// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Grpc.Core;
using Narupa.Core.Async;
using Narupa.Protocol.Trajectory;

namespace Narupa.Grpc.Tests.Trajectory
{
    internal class InfiniteTrajectoryServer : IAsyncClosable
    {
        private Server server;
        private readonly InfiniteTrajectoryService service;

        public int Delay
        {
            get => service.Delay;
            set => service.Delay = value;
        }

        public int MaxMessages
        {
            set => service.MaxMessage = value;
        }

        public event Action FrameDataSent;

        public InfiniteTrajectoryServer(int port)
        {
            service = new InfiniteTrajectoryService();
            service.FrameDataSent += () => FrameDataSent?.Invoke();
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