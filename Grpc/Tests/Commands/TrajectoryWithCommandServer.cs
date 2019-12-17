// Copyright (c) Intangible Realities Laboratory. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Grpc.Core;
using Narupa.Core.Async;
using Narupa.Grpc.Tests.Commands;
using Narupa.Protocol.Command;
using Narupa.Protocol.Trajectory;

namespace Narupa.Grpc.Tests.Trajectory
{
    internal class TrajectoryWithCommandServer : IAsyncClosable
    {
        private Server server;
        private readonly QueueTrajectoryService trajectory;
        private readonly CommandServer commands;

        public CommandServer Commands => commands;
        
        public TrajectoryWithCommandServer(int port)
        {
            trajectory = new QueueTrajectoryService();
            commands = new CommandServer();
            server = new Server
            {
                Services =
                {
                    TrajectoryService.BindService(trajectory),
                    Command.BindService(commands)
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