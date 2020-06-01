using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Narupa.Grpc.Tests
{
    /// <summary>
    /// gRPC interceptor which adds a configurable latency to messages streamed from the server.
    /// </summary>
    public class LatencySimulator : Interceptor
    {
        private int serverStreamLatency;

        private event Action ServerStreamLatencyChanged;
        
        public int ServerStreamLatency
        {
            get => serverStreamLatency;
            set
            {
                serverStreamLatency = value;
                ServerStreamLatencyChanged?.Invoke();
            }
        }
        
        
        public override Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request,
                                                                               IServerStreamWriter<TResponse> responseStream,
                                                                               ServerCallContext context,
                                                                               ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            var delayed = new DelayedStreamWriter<TResponse>(responseStream);
            ServerStreamLatencyChanged += () => delayed.Latency = serverStreamLatency;
            return base.ServerStreamingServerHandler(request, delayed, context, continuation);
        }
    }
}