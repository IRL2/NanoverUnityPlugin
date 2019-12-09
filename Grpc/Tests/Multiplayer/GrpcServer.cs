using System.Threading.Tasks;
using Grpc.Core;
using Narupa.Core.Async;

namespace Narupa.Grpc.Tests.Multiplayer
{
    internal class GrpcServer : IAsyncClosable
    {
        private Server server;

        public GrpcServer(int port, params ServerServiceDefinition[] services)
        {
            server = new Server
            {
                Ports =
                {
                    new ServerPort("localhost", port, ServerCredentials.Insecure)
                }
            };
            foreach (var service in services)
                server.Services.Add(service);
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