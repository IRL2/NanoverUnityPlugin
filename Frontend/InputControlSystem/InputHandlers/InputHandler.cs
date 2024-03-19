using System;
using System.Collections.Generic;
using System.Linq;
using Nanover.Frontend.InputControlSystem.InputControllers;
using Nanover.Frontend.InputControlSystem.InputSelectors;
using Nanover.Frontend.InputControlSystem.Utilities;
using Nanover.Grpc.Multiplayer;
using Nanover.Grpc.Trajectory;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using static Nanover.Frontend.InputControlSystem.Utilities.InputControlUtilities;



namespace Nanover.Frontend.InputControlSystem.InputHandlers
{
    /// <summary>
    /// Abstract base class for all input handlers. Input handlers are designed to capture and map
    /// user inputs to specified behaviours. They are responsible for defining & executing specific
    /// runtime behaviours, and should encapsulate all of the code required to perform a particular
    /// function.
    /// </summary>
    /// <remarks>
    /// It is important to note that all input handlers must start in the `Disabled` state, and
    /// should only subscribe to user inputs when its state is set to `Active`. A more detailed
    /// outline of the input handler's role within the Input Control System (ICS) is provided below.
    ///
    /// The input handler component forms one of the most crucial elements of the ICS. These
    /// components are tasked with capturing and mapping user inputs to specific, pre-defined
    /// behaviours. These behaviours encompass a wide variety of possible user actions, ranging from
    /// simple navigational activities to complex gestural-based interactions with the virtual
    /// environment. Each input handler encapsulates all of the code and logic necessary to perform
    /// its designated function, thereby enabling a modular & scalable approach to input management.
    /// Specific features may then be provided to users by activating and deactivating individual
    /// input handlers based on a variety of runtime conditions.
    ///
    /// All input handlers are based on the abstract base class <c>InputHandler</c>. It is worth
    /// noting that as handlers are tasked with processing user inputs, and as controllers are
    /// responsible for the majority of explicit user input, one could understandably assign each
    /// handler to a specific controller instance (<c>InputController</c> object). However, It is
    /// more advisable to invert this structure, and instead assign controllers to the handlers.
    /// This avoids creating an unnecessary coupling between controller instances in situations
    /// where an input handler requires inputs from multiple controllers. 
    ///
    /// Following instantiation, input handlers are furnished with <c>InputController</c> instances
    /// via their <c>AddController</c> method, equipping them with the means to interact with and
    /// source information, commonly input actions, from the specified controller devices. It should
    /// be noted that just because a controller instance is bound to an input handler does not
    /// necessarily mean that the input handler is currently sourcing input from it or even will do
    /// so at any point in the future. It just means that the input handler now has the opportunity
    /// to do so if it so wishes. Furthermore, any particular <c>InputController</c> instance my be
    /// bound to multiple or no handlers at any one time. Finally, an input handler may only ever
    /// source explicit user input if it is in an <c>Active</c> state.
    /// </remarks>
    public abstract class InputHandler: MonoBehaviour
    {

        /// <summary>
        /// An array storing all associated input controller devices.
        /// </summary>
        /// <remarks>
        /// All controllers added to an input handler via its <c>BindController</c> method will be
        /// added to this array. An input handler is then free to subscribe to the input actions
        /// provided by these controllers so long as it is in the <c>Active</c> state. Controllers
        /// can be removed from this array via the <c>UnbindController</c>.
        /// </remarks>
        public InputController[] Controllers { get; protected set; }

        /// <summary>
        /// Gets or sets the current state of the input handler. The state dictates the handler's
        /// level of activity and responsiveness to input.
        /// </summary>
        /// <remarks>
        /// Commonly a non-trivial degree of logic is required to perform this transition as input
        /// actions must be un/subscribed from/to and many other tasks will need to be performed by
        /// an input handler to facilitate this transition. Note that input handlers should always
        /// start in the <c>Disabled</c> state.
        /// </remarks>
        public abstract State State { get; set; }

        /// <summary>
        /// Protected backing field for the <c>State</c> field.
        /// </summary>
        /// <remarks>
        /// The inclusion of a protected backing field is necessitated by the anticipation that
        /// derived classes may require direct manipulation of the underlying state outside the
        /// confines of the <c>State</c> property's getter/setter. This approach prevents the need
        /// for overly complex logic within these accessors, thereby simplifying the implementation
        /// in derived classes. The initial state is set to <c>Disabled</c>, aligning with the
        /// required starting state for input handlers.
        /// </remarks>
        protected State state = State.Disabled;

        /// <summary>
        /// Boolean indicating whether the input handler is currently locked. The lock condition
        /// signifies that the handler is performing an action that must not be interrupted.
        /// </summary>
        /// <remarks>
        /// This should be used sparingly, and only for the most critical of operations as it may
        /// have a profound effect on functionality elsewhere (e.g. GUIs becoming unresponsive).
        /// </remarks>
        public bool Locked { get; protected set; }

        /// <summary>
        /// Binds a controller to the input handler from which user inputs may be sourced.
        /// </summary>
        /// <param name="controller">The controller to be bound to the handler.</param>
        public abstract void BindController(InputController controller);

        /// <summary>
        /// Unbinds a controller from this input handler, preventing further input capture from
        /// the controller.
        /// </summary>
        /// <param name="controller">The controller to be unbound from this handler.</param>
        /// <remarks>
        /// This should only be used when wanting to replace the currently assigned controller with
        /// a new one. Note that setting the input handler's state to `Disabled` will instruct the
        /// handler to temporary ignore inputs provided by the controller.
        /// </remarks>
        public abstract void UnbindController(InputController controller);

        /// <summary>
        /// Returns true if the specified controller instance is bound to this input handler.
        /// </summary>
        /// <param name="controller">Controller that is to be tested for binding.</param>
        /// <returns>True of the specified controller is bound.</returns>
        /// <remarks>
        /// It should be noted that just because a controller instance is bound to an input handler
        /// does not necessarily mean that the input handler is currently sourcing input from it or
        /// even will do so at any point in the future. It just means that the input handler now has
        /// the opportunity to do so if it so wishes.
        /// </remarks>
        public bool IsBoundToController(InputController controller)
        {
            return Array.Exists(Controllers, x => x == controller);
        }

        /// <summary>
        /// Transitions the input handler to a non-active state. The state is set to 'Passive' if
        /// the handler supports background operation; otherwise, it is set to 'Disabled'.
        /// </summary>
        /// <remarks>
        /// This method is invoked when the input handler must relinquish its active role, typically
        /// when another process requires focus. A handler in a 'Passive' state may continue to
        /// perform background updates or monitor events without capturing explicit user input. If
        /// the handler does not support a 'Passive' state, it is fully deactivated by entering
        /// the 'Disabled' state, ceasing all operations and input monitoring.
        /// </remarks>
        public abstract void Background();

        /// <summary>
        /// A list of strings specifying which input actions that the handler expects to be given
        /// sole binding privilege to. This should be limited only to the input actions associated
        /// with explicit user inputs.
        /// </summary>
        /// <returns>Hash set containing the names of the specific input actions that this handler is expected to use.</returns>
        public HashSet<string> RequiredBindingNames() => RequiredBindings().Select(FullyQualifiedInputActionName).ToHashSet();

        /// <summary>
        /// A list of <c>InputAction</c> entities specifying which input actions that the handler
        /// expects to be given sole binding privilege to. This should be limited only to the input
        /// actions associated with explicit user inputs.
        /// </summary>
        /// <returns>Hash set containing the input actions that this handler is expected to use.</returns>
        public abstract HashSet<InputAction> RequiredBindings();

        /// <summary>
        /// Determines whether a given input controller is compatible with the input handler.
        /// </summary>
        /// <param name="controller">The input controller to check for compatibility.</param>
        /// <returns>True if the controller is compatible; otherwise, false.</returns>
        public abstract bool IsCompatibleWithInputController(InputController controller);
        // While this would be best served as a static method, so that checks can be made without
        // having to create an instance to test against, C# does not support this behaviour. There
        // are workarounds in .NET 6 but this is not yet supported.

        // The above workaround is not particularly elegant and should be replaced with a more stable
        // and sensible approach.

        /// <summary>
        /// Identify the index of a specified input controller within the internal `Controllers` array.
        /// </summary>
        /// <param name="controller">Controller whose index is to be returned</param>
        /// <returns>Index of controller entity within the `Controllers` array.</returns>
        protected int ControllerIndex(InputController controller)
        {
            int index = Array.IndexOf(Controllers, controller);

            // If the controller does not exist within the array then this is in error!
            Assert.AreNotEqual(-1, index,
                "Handler attempted to index an unbound controller.");

            return index;
        }

        /// <summary>
        /// This method will be called by an arbiter at the end of the setup process to inform
        /// the handler that the setup process has finished. This is used to perform any required
        /// checks and initialisation operations.
        /// </summary>
        public virtual void Initialise(){}
        

    }

    /// <summary>
    /// Enumerates the possible states of an input handler. Each state represents a different
    /// level of interaction and responsiveness to the system's input.
    /// </summary>
    /// <remarks>
    /// Valid state transitions:
    /// <code>
    /// +--------------------------+-------------------------------------------+
    /// |                          |               Target State                |
    /// |                          +----------+----------+----------+----------+
    /// |                          | Disabled | Passive  |  Paused  |  Active  |
    /// +---------------+----------+----------+----------+----------+----------+
    /// |               | Disabled | 0        | 0        | 0        | 1        |
    /// |               +----------+----------+----------+----------+----------+
    /// |               | Passive  | 1        | 0        | 0        | 1        |
    /// | Current State +----------+----------+----------+----------+----------+
    /// |               | Paused   | 0        | 0        | 0        | 1        |
    /// |               +----------+----------+----------+----------+----------+
    /// |               | Active   | 1        | 1        | 1        | 0        |
    /// +---------------+----------+----------+----------+----------+----------+
    /// </code>
    /// When in an <c>Active</c> state, an input handler may chose to lock itself by setting its
    /// <c>Locked</c> field to <c>true</c>. When locked in this manner, the handler will not
    /// respond to state change request. Locking is only valid for active state handlers and is
    /// used to indicate that the handler is currently performing a critical task that cannot be
    /// interrupted. However, this should be used sparingly.
    /// </remarks>
    public enum State
    {
        /// <summary>
        /// Represents a state where the input handler is inactive and not responding to input.
        /// </summary>
        Disabled,

        /// <summary>
        /// Represents a state where the input handler is active but operating in the background
        /// and not actively capturing explicit user input.
        /// </summary>
        Passive,

        /// <summary>
        /// Represents a state where the input handler is temporarily inactive, often due to system
        /// interruptions or higher priority processes such as a GUI interaction taking priority.
        /// </summary>
        Paused,

        /// <summary>
        /// Represents a state where the input handler is fully active and responsive to user input.
        /// </summary>
        Active,
    }


    /// <summary>
    /// Marks input handlers as "user selectable".
    /// </summary>
    /// <remarks>
    /// This interface marks input handlers that implement it as user selectable. Meaning that users
    /// are able to manually activate and deactivate such handlers, commonly by means of a graphical
    /// user interface. Such an interface ensures that handlers offer up both an icon and a human
    /// readable name which can be displayed in a menu. This interface is used by other components of
    /// the input control system like the <see cref="RadialInputSelector">radial input selection menu
    /// </see>.
    /// </remarks>
    public interface IUserSelectableInputHandler
    {
        /// <summary>
        /// The name given to the input handler, this is intended to uniquely identify the
        /// handler to the user at runtime. This is commonly set at the class level.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// A unique icon that may be displayed to the user to visually represent the input
        /// handler. This is commonly defined at the class level and set once at runtime.
        /// </summary>
        public Sprite Icon { get; }

        /// <summary>
        /// A ushort specifying the priority of the input handler. A higher number corresponds to a
        /// greater priority. This is used to help control the order in which elements are shown in
        /// in selection menus, to identify which handler should be activated on startup, etc.
        /// </summary>
        public ushort Priority { get; }
    }


    /// <summary>
    /// Used to indicate that an input handler operates upon the system game object. In this context
    /// "system" means the game object associated with the atomic system.
    /// </summary>
    /// <remarks>
    /// For example the input handler responsible for allowing users to rotate and translate the atomic
    /// system must know what game object it should be rotating.
    /// </remarks>
    public interface ISystemDependentInputHandler
    {
        /// <summary>
        /// Specify the system which the input handler is responsible for.
        /// </summary>
        /// <param name="systemObject">Game object representing the target system</param>
        public void SetSystem(GameObject systemObject);
    }


    /// <summary>
    /// Used to designate input handlers that require a <see cref="MultiplayerSession">multiplayer session</see> to operate.
    /// </summary>
    public interface IMultiplayerSessionDependentInputHandler
    {
        /// <summary>
        /// Set the required multiplayer session.
        /// </summary>
        /// <param name="multiplayer">The required multiplayer session</param>
        public void SetMultiplayerSession(MultiplayerSession multiplayer);
    }


    /// <summary>
    /// Used to signify input handlers that require a <see cref="TrajectorySession">trajectory session</see> to operate.
    /// </summary>
    public interface ITrajectorySessionDependentInputHandler
    {
        /// <summary>
        /// Set the required trajectory session.
        /// </summary>
        /// <param name="trajectory">The required trajectory session</param>
        public void SetTrajectorySession(TrajectorySession trajectory);
    }

    // Developer's notes, this can be enabled once Unity offers support for the .NET 6 framework.
    // This will allow for InputHandler classes to be tested for compatibility against different
    // InputController instances without needing to instantiate a handler entity.
    //public interface IPickyInputHandler
    //{
    //    /// <summary>
    //    /// Determines whether a given input controller is compatible with the input handler.
    //    /// </summary>
    //    /// <param name="controller">The input controller to check for compatibility.</param>
    //    /// <returns>True if the controller is compatible; otherwise, false.</returns>
    //    public abstract static bool IsCompatibleWithInputController(InputController controller);
    //}

}
