using System;
using System.Threading;
using System.Threading.Tasks;
using Narupa.Core.Async;
using Narupa.Grpc.Stream;
using NSubstitute.ReceivedExtensions;
using UnityEngine;

namespace Narupa.Grpc.Trajectory
{
    /// <summary>
    /// A utility to start two separate tasks for an incoming stream - a background thread which
    /// is constantly polling the gRPC stream (not limited to Unity's update loop) and merging
    /// concurrent data together, and a task which runs on the main thread and invokes a callback
    /// on this data.
    /// </summary>
    public class BackgroundIncomingStreamReceiver<TResponse> where TResponse : class
    {
        public static void Start(IncomingStream<TResponse> stream, Action<TResponse> callback, Action<TResponse, TResponse> merge)
        {
            new BackgroundIncomingStreamReceiver<TResponse>(stream, callback, merge);
        }
        
        private void ReceiveOnBackgroundThread(TResponse response)
        {
            if (receivedData == null)
                receivedData = response;
            else
                Merge(receivedData, response);
        }

        private IncomingStream<TResponse> stream;
        
        private BackgroundIncomingStreamReceiver(IncomingStream<TResponse> stream, Action<TResponse> callback, Action<TResponse, TResponse> merge)
        {
            this.stream = stream;
            stream.MessageReceived += ReceiveOnBackgroundThread;
            BackgroundThreadTask().AwaitInBackgroundIgnoreCancellation();
            MainThreadTask().AwaitInBackgroundIgnoreCancellation();
            Callback = callback;
            Merge = merge;
        }

        private async Task BackgroundThreadTask()
        {
            await Task.Run(stream.StartReceiving, stream.GetCancellationToken());
        }

        private async Task MainThreadTask()
        {
            while (true)
            {
                if (stream.IsCancelled)
                    return;
                
                if (receivedData != null)
                {
                    var newReceivedData = receivedData;
                    receivedData = null;
                    Callback?.Invoke(newReceivedData);
                }

                await Task.Delay(1, stream.GetCancellationToken());
            }
        }
        
        private TResponse receivedData = null;

        private Action<TResponse> Callback;
        private Action<TResponse, TResponse> Merge;
    }
}