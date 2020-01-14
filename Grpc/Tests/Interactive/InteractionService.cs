// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Grpc.Core;
using Narupa.Protocol.Imd;

namespace Narupa.Grpc.Tests.Interactive
{
    internal class InteractionService : 
        InteractiveMolecularDynamics.InteractiveMolecularDynamicsBase, IBindableService
    {
        public override async Task<InteractionEndReply> PublishInteraction(
            IAsyncStreamReader<ParticleInteraction> requestStream,
            ServerCallContext context)
        {
            while (await requestStream.MoveNext(context.CancellationToken))
            {
                InteractionReceived?.Invoke(requestStream.Current);
                if (ThrowException)
                    throw new InvalidOperationException();
            }

            return new InteractionEndReply();
        }

        public bool ThrowException { get; set; } = false;

        public event Action<ParticleInteraction> InteractionReceived;
        
        public ServerServiceDefinition BindService()
        {
            return InteractiveMolecularDynamics.BindService(this);
        }
    }
}