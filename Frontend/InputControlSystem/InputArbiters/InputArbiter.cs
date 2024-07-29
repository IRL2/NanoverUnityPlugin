using System;
using Nanover.Frontend.InputControlSystem.InputControllers;
using Nanover.Frontend.InputControlSystem.InputHandlers;
using JetBrains.Annotations;
using UnityEngine;
using System.Collections.Generic;

namespace Nanover.Frontend.InputControlSystem.InputArbiters
{
    /// <summary>
    /// Input arbiters are the components of the input control system responsible for managing the
    /// task of activating and deactivating input handlers during runtime.
    /// </summary>
    /// <remarks>
    /// Input arbiters are high-level entities, based upon the singleton abstract base class
    /// <c>InputArbiter</c>, whose primary responsibility is to manage input handler state change
    /// events. These entities will idle until they receive an explicit request from another part
    /// of the code to activate a specific input handler, via the <c>RequestInputHandlerActivation</c>
    /// method. Upon receiving an activation request, the arbiter initiates a sequence of operations.
    /// It first determines whether any active handlers could potentially conflict with the requested
    /// handler; i.e. do they require access to the same controller bindings. If such conflicts are
    /// identified, the arbiter assesses whether the conflicting handler can be deactivated. Should
    /// deactivation be feasible, the arbiter proceeds to disable the conflicting handler before
    /// activating the requested one. Conversely, if the conflicting handler cannot be deactivated,
    /// the arbiter will disregard the activation request. In instances where no conflicting handlers
    /// are discovered, the arbiter straightforwardly activates the specified input handler.
    ///
    /// During the initial stages of operation, the arbiter sequentially examines each provided
    /// pair of <c>InputController</c> instances and <c>InputHandler</c> class types. Utilising the
    /// <c>IsCompatibleWithController</c> method, it determines which handlers are compatible with
    /// the supplied controllers and proceeds to instantiate them. This not only abstracts the input
    /// handler factory pattern to a centralised and dedicated location but also provides a second
    /// location in which incompatible handlers can be filtered out. Additional controller instances
    /// or input handler types may be periodically added to the arbiter during runtime as and when
    /// needed.
    ///
    /// The arbiter may then build one or more input selectors that will be used to decide under
    /// what conditions a specific handler should become active during runtime. This may be something
    /// as simple as a radial menu from which a user may select a handler, or something more complex
    /// like a contextual trigger that will activate when specific conditions are met within the
    /// virtual construct. However, other parts of the code may also instruct the arbiter to activate
    /// a particular handler via a call to its <c>RequestInputHandlerActivation</c> method.
    /// </remarks>
    public abstract class InputArbiter: MonoBehaviour
    {
        /* Developers Notes:
         * For an example concrete implementation of this abstract base class please see the
         * `BasicInputArbiter` module.
         */

        /// <summary>
        /// Adds an input controller to the arbiter's management system.
        /// </summary>
        /// <param name="controller">The input controller to be added.</param>
        /// <remarks>
        /// This method ensures that input controllers can be added into the arbiter's control system.
        /// It must account for the compatibility of the controller with existing and potential future
        /// input handlers, facilitating the dynamic addition of controllers during runtime.
        /// </remarks>
        public abstract void AddController(InputController controller);
        
        /// <summary>
        /// Registers a new input handler type within the arbiter's system.
        /// </summary>
        /// <param name="handlerType">The type of the input handler to be added.</param>
        /// <remarks>
        /// This method is tasked with adding new input handler types, allowing the arbiter system
        /// to instantiate instances of this handler when appropriate. It necessitates a consideration
        /// of the handler's compatibility with current and future controllers, promoting flexibility
        /// in input handling.
        /// </remarks>
        public abstract void AddHandler(Type handlerType);
        
        /// <summary>
        /// Requests the activation of a specified input handler, optionally targeting specific
        /// controllers.
        /// </summary>
        /// <param name="handler">The input handler to be activated.</param>
        /// <param name="targetControllers">Optional list of controllers on which to activate the
        /// handler.</param>
        /// <returns>True if the activation request was successful; otherwise, false.</returns>
        /// <remarks>
        /// This method manages the activation process of an input handler, evaluating potential
        /// conflicts with currently active handlers and ensuring compatibility with specified
        /// controllers. It embodies the system's ability to dynamically adjust to changing input
        /// needs during runtime.
        /// 
        /// Note that specifying target controllers via the <c>targetControllers</c> argument is
        /// only meaningful for activating hybrid input handlers on a single controller. For most
        /// other input handlers, which must activate on all bound controllers, specifying target
        /// controllers is unnecessary.
        /// </remarks>
        public abstract bool RequestInputHandlerActivation(
            InputHandler handler, [CanBeNull] List<InputController> targetControllers = null);

        /// <summary>
        /// Requests the deactivation of a specified input handler, optionally targeting specific
        /// controllers.
        /// </summary>
        /// <param name="handler">The input handler to be deactivated.</param>
        /// <param name="targetControllers">Optional list of controllers from which to deactivate
        /// the handler.</param>
        /// <returns>True if the deactivation request was successful; otherwise, false.</returns>
        /// <remarks>
        /// This method is responsible for deactivating an input handler, taking into account the need
        /// to preserve system integrity. It reflects the system's capability adaptively respond to
        /// decreased input requirements or to facilitate transitions between input handlers.
        ///
        /// Note that specifying target controllers via the <c>targetControllers</c> argument is
        /// only meaningful for activating hybrid input handlers on a single controller. For most
        /// other input handlers, which must activate on all bound controllers, specifying target
        /// controllers is unnecessary.
        /// </remarks>
        public abstract bool RequestInputHandlerDeactivation(
            InputHandler handler, [CanBeNull] List<InputController> targetControllers = null);

        /// <summary>
        /// Toggles the activation state of a specified input handler, optionally targeting specific
        /// controllers.
        /// </summary>
        /// <param name="handler">The input handler whose activation state is to be toggled.</param>
        /// <param name="targetControllers">Optional list of controllers on which to toggle the
        /// handler.</param>
        /// <returns>True if the toggle request was successful; otherwise, false.</returns>
        /// <remarks>
        /// This method facilitates the toggling of an input handler's activation state, enabling
        /// or disabling it as required.
        ///
        /// Note that specifying target controllers via the <c>targetControllers</c> argument is
        /// only meaningful for activating hybrid input handlers on a single controller. For most
        /// other input handlers, which must activate on all bound controllers, specifying target
        /// controllers is unnecessary.
        /// </remarks>
        public abstract bool RequestInputHandlerToggle(
            InputHandler handler, [CanBeNull] List<InputController> targetControllers = null);

        /// <summary>
        /// Requests the temporary pause of input handlers associated with a specified controller.
        /// </summary>
        /// <param name="controller">The controller for which input handlers are to be paused.</param>
        /// <returns>True if the pause request was successful; otherwise, false.</returns>
        /// <remarks>
        /// This method allows for the temporary suspension of input processing from a given controller,
        /// typically to prevent unintended interactions during critical operations or user interface
        /// interactions. This will trigger all input handlers on a given controller to enter a paused
        /// state. This is done on the condition that the handler will be resumed in short order.
        /// </remarks>
        public abstract bool RequestPauseOfInputHandlersOnController(InputController controller);

        /// <summary>
        /// Requests the resumption of previously paused input handlers on a specified controller.
        /// </summary>
        /// <param name="controller">The controller for which input handlers are to be resumed.</param>
        /// <returns>True if the resumption request was successful; otherwise, false.</returns>
        /// <remarks>
        /// This method enables the reactivation of input handlers that were previously paused, ensuring
        /// that normal input processing can resume once the need for the pause has been alleviated.
        /// </remarks>
        public abstract bool RequestResumptionOfInputHandlersOnController(InputController controller);
    }
}


