namespace Narupa.Grpc.Multiplayer
{
    public enum ResourceLockState
    {
        /// <summary>
        /// The state of the resource is not known, and shouldn't be altered.
        /// </summary>
        Unknown,
        /// <summary>
        /// A request to obtain a lock has been sent, and we are awaiting the result.
        /// </summary>
        Pending,
        /// <summary>
        /// The lock has been accepted and we have a lock on the object.
        /// </summary>
        Accepted,
        /// <summary>
        /// The lock has been rejected.
        /// </summary>
        Rejected
    }
}