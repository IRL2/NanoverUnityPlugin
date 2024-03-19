using Nanover.Frame;
using Nanover.Frontend.Controllers;
using Nanover.Frontend.InputControlSystem.InputArbiters;
using Nanover.Frontend.InputControlSystem.InputHandlers;
using UnityEngine;
using Nanover.Grpc.Multiplayer;
using Nanover.Grpc.Trajectory;


namespace Nanover.Frontend.InputControlSystem.Utilities
{


    // This component forms final piece of a dependency injection system used by input handlers
    // within the input control system. The input handler entities specify their dependencies using
    // the interfaces defined along side the `InputHandler` abstract base class. For example, an
    // input handler may require access to the `TrajectorySession`, in which case it would implement
    // the `ITrajectorySessionDependentInputHandler` interface. This signals to the input arbiter that
    // it should provide the handler with the requested information. Sources of such information are
    // flagged with the interfaces as defined below. 

    // This approach might require rethinking as it could get very messy, very quickly if it is not
    // treated with the appropriate level of respect.

    /// <summary>
    /// Indicates that an entity is a <see cref="MultiplayerSession"/> source.
    /// </summary>
    /// <remarks>
    /// This is currently used by the <see cref="ControllerManager">controller manager</see> and the
    /// <see cref="InputArbiter">input arbiter</see> to identify multiplayer session sources for input
    /// handlers that implement the <see cref="IMultiplayerSessionDependentInputHandler"/> interface. 
    /// </remarks>
    public interface IMultiplayerSessionSource
    {
        /// <summary>
        /// Multiplayer session.
        /// </summary>
        /// <remarks>
        /// This property can be called to get the current multiplayer session.
        /// </remarks>
        public MultiplayerSession Multiplayer { get; }
    }


    /// <summary>
    /// Indicates that an entity is a <see cref="TrajectorySession"/> source.
    /// </summary>
    /// <remarks>
    /// This is currently used by the <see cref="ControllerManager">controller manager</see> and the
    /// <see cref="InputArbiter">input arbiter</see> to identify trajectory session sources for input
    /// handlers that implement the <see cref="ITrajectorySessionDependentInputHandler"/> interface. 
    /// </remarks>
    public interface ITrajectorySessionSource
    {
        /// <summary>
        /// Trajectory session.
        /// </summary>
        /// <remarks>
        /// This property can be called to get the current trajectory session.
        /// </remarks>
        public TrajectorySession Trajectory { get; }
    }



}