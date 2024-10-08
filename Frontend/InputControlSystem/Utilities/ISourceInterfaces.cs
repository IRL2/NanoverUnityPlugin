using Nanover.Frame;
using Nanover.Frontend.Controllers;
using Nanover.Frontend.InputControlSystem.InputArbiters;
using Nanover.Frontend.InputControlSystem.InputHandlers;
using Nanover.Frontend.XR;
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

    /// <summary>
    /// Indicates that an entity is a <see cref="PhysicallyCalibratedSpace"/> source.
    /// </summary>
    /// <remarks>
    /// This is currently used by the <see cref="ControllerManager">controller manager</see> and the
    /// <see cref="InputArbiter">input arbiter</see> to identify physically calibrated space sources
    /// for input handlers that implement the <see cref="IPhysicallyCalibratedSpaceDependentInputHandler"/>
    /// interface. 
    /// </remarks>
    public interface IPhysicallyCalibratedSpaceSource
    {
        /// <summary>
        /// Physically calibrated space.
        /// </summary>
        /// <remarks>
        /// This property can be called to get the physically calibrated space entity that is
        /// used to transform between the client-side user-specific coordinate system and the
        /// server-side user-agnostic coordinate space.
        /// </remarks>
        public PhysicallyCalibratedSpace PhysicallyCalibratedSpace { get; }
    }

    /// <summary>
    /// Indicates that an entity is able to provide a pair of <see cref="UnityEngine.Transform">
    /// transforms</see> representing the outer and inner simulation spaces. The former used to
    /// describe the location of the simulation within the virtual construct, and the latter being
    /// used to convert from right-hand simulation space into left-hand coordinated Unity space. 
    /// </summary>
    /// <remarks>
    /// This is currently used by the <see cref="ControllerManager">controller manager</see> & the
    /// <see cref="InputArbiter">input arbiter</see> to extract the outer & inner simulation spaces
    /// sot that they can be provided to input handlers that implement the interface
    /// <see cref="ISimulationSpaceTransformDependentInputHandler"/>. 
    /// </remarks>
    public interface ISimulationSpaceTransformSource
    {
        /// <summary>
        /// A pair of <see cref="UnityEngine.Transform">transforms</see> representing the simulation's
        /// "<i>outer</i>" and "<i>inner</i>" spaces.
        /// </summary>
        /// <returns>
        /// A tuple containing two <see cref="UnityEngine.Transform">Transform</see> objects:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// The first item, <c>outerSimulationSpaceTransform</c>, represents the outer simulation
        /// space transform. This defines the overall location, orientation, and scale of the
        /// simulation within the virtual space. This may be freely manipulated as needed in the
        /// manner that one would expect of an objects transform. However, care must be taken to
        /// ensure that local changes made to this transform are synced with the server using the
        /// appropriate multiplayer resource.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// The second item, <c>innerSimulationSpaceTransform</c>, represents the inner simulation
        /// space transform. Molecular simulation packages often adopt the right-hand coordinate
        /// system, in contrast to Unity's left-handed system. This discrepancy results in the
        /// rendering of molecular systems as mirror images, with R-enantiomers appearing as
        /// S-enantiomers. To address this, an inner simulation space with a scale value of
        /// [-1, 1, 1] is employed to accurately reflect the simulation. Therefore, to obtain the
        /// correct controller position within the simulation space, this transform must be used.
        /// It is crucial to note that, unlike the <c>outerSimulationSpaceTransform</c>, the inner
        /// transform should <u>never</u> be manipulated directly.
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        public (Transform, Transform) SimulationSpaceTransforms { get; }
    }

}