using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Narupa.Core.Async;
using Narupa.Grpc.Tests.Trajectory;

namespace Narupa.Grpc.Tests.Multiplayer
{
    /// <summary>
    /// Generic gRPC server for testing purposes.
    /// </summary>
    internal class GrpcServer : IAsyncClosable
    {
        private Server server;

        public GrpcServer(params IBindableService[] services) : this(
            services.Select(s => s.BindService()).ToArray())
        {
        }

        public GrpcServer(params ServerServiceDefinition[] services)
        {
            server = new Server
            {
                Ports =
                {
                    new ServerPort("localhost", 0, ServerCredentials.Insecure)
                }
            };
            foreach (var service in services)
                server.Services.Add(service);
            server.Start();
        }

        public int Port => server.Ports.First().BoundPort;

        public async Task CloseAsync()
        {
            if (server == null)
                return;
            await server.KillAsync();
            server = null;
        }

        public static (GrpcServer server, GrpcConnection connection) CreateServerAndConnection(params IBindableService[] services)
        {
            var server = new GrpcServer(services);
            return (server, new GrpcConnection("localhost", server.Port));
        }
    }
}