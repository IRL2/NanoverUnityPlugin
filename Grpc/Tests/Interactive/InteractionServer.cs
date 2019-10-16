// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Grpc.Core;
using Narupa.Core.Async;
using Narupa.Protocol.Imd;

namespace Narupa.Grpc.Tests.Interactive
{
    internal class InteractionServer : IAsyncClosable
    {
        private Server server;
        private readonly InteractionService service;

        public InteractionService Service => service;

        public InteractionServer(int port)
        {
            service = new InteractionService();
            server = new Server
            {
                Services =
                {
                    InteractiveMolecularDynamics.BindService(service)
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
            await server.KillAsync();
            server = null;
        }
    }
}