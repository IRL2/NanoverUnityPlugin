using Grpc.Core;

namespace NanoVer.Grpc.Tests
{
    /// <summary>
    /// Wrapper for a gRPC service that knows how to bind itself.
    /// </summary>
    internal interface IBindableService
    {
        ServerServiceDefinition BindService();
    }
}