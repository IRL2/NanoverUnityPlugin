// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System.Threading;
using JetBrains.Annotations;
using Narupa.Grpc.Stream;
using Narupa.Protocol.Imd;

namespace Narupa.Grpc.Interactive
{
    /// <summary>
    /// Wraps a
    /// <see cref="InteractiveMolecularDynamics.InteractiveMolecularDynamicsClient" />
    /// and provides the ability to publish a list of interactions to a server over a
    /// <see cref="GrpcConnection" />.
    /// </summary>
    public sealed class ImdClient :
        GrpcClient<InteractiveMolecularDynamics.InteractiveMolecularDynamicsClient>
    {
        public ImdClient([NotNull] GrpcConnection connection) : base(connection)
        {
        }

        /// <summary>
        /// Starts an <see cref="OutgoingStream{Interaction,InteractionEndReply}" /> which
        /// allows interactions to be sent to the server.
        /// </summary>
        /// <remarks>
        /// Corresponds to the PublicInteractions gRPC call.
        /// </remarks>
        public OutgoingStream<ParticleInteraction, InteractionEndReply> PublishInteractions(
            CancellationToken externalToken = default)
        {
            return GetOutgoingStream(Client.PublishInteraction,
                                     externalToken);
        }
    }
}