using JetBrains.Annotations;
using Nanover.Frontend.InputControlSystem.InputControllers;
using Nanover.Frontend.InputControlSystem.InputHandlers;
using Nanover.Frontend.InputControlSystem.InputSelectors;
using Nanover.Frontend.XR;
using Nanover.Grpc.Multiplayer;
using Nanover.Grpc.Trajectory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using State = Nanover.Frontend.InputControlSystem.InputHandlers.State;


namespace Nanover.Frontend.InputControlSystem.InputArbiters
{
    /// <summary>
    /// Manages the instantiation, activation, and deactivation of input handlers within a dynamic
    /// input control system.
    ///
    /// This class oversees the coordination between various input handlers & controllers, ensuring
    /// that dependencies are fulfilled & conflicts are resolved to maintain system integrity. It
    /// supports dynamic responses to changing input needs, allowing for the flexible activation,
    /// deactivation, & toggling of input handlers based on runtime conditions. Additionally, it
    /// facilitates temporary suspension of input processing during user interface interactions,
    /// ensuring a smooth & predictable user experience.
    /// </summary>
    public class BasicInputArbiter : InputArbiter
    {
        /// <summary>
        /// List of all input handler instances being currently being managed by the arbiter.
        /// </summary>
        private readonly List<InputHandler> inputHandlers = new();

        /// <summary>
        /// List of all input handler instances being currently being managed by the arbiter.
        /// </summary>
        public IReadOnlyList<InputHandler> InputHandlers => inputHandlers.AsReadOnly();

        /// <summary>
        /// List of all input controller devices currently under the watch of the arbiter.
        /// </summary>
        private readonly List<InputController> controllers = new();

        /// <summary>
        /// List of all input controller devices currently under the watch of the arbiter.
        /// </summary>
        public IReadOnlyList<InputController> Controllers => controllers.AsReadOnly();

        /// <summary>
        /// List of all input handler class types known to the arbiter. Regardless of whether or not
        /// any instances of such classes have yet been instantiated.
        /// </summary>
        /// <remarks>
        /// This exists so that when a new controller is added, a scan can be performed to identify
        /// and instantiate any compatible input handlers. This is of particular use in situations
        /// where a handler is added before any controller.
        /// </remarks>
        private readonly List<Type> knownInputHandlerTypes = new();

        /// <summary>
        /// Bookkeeping dictionary helps to keep track of which input handler have been paused
        /// on which controllers, allowing for paused controllers to be resumed.
        /// </summary>
        private Dictionary<InputController, InputHandler> pausedHandlers =
            new Dictionary<InputController, InputHandler>();

        /// <summary>
        /// The XR-ray interactor selector responsible for disabling input handlers when the user
        /// interacts with user interfaces.
        /// </summary>
        private XRRayInteractorSelector xrRayInteractorSelector;

        /// <summary>
        /// Top level game objects used as the root node for each of the radial selection menus.
        /// </summary>
        /// <remarks>
        /// One radial menu is created for each controller which allows users, upon depression of
        /// the <c>radialMenuPromptActionName</c> button, to select which input handler should be
        /// activated or deactivated. Note that only input handlers that have been tagged with the
        /// <c>IUserSelectableInputHandler</c> interface will be shown in such menu systems.
        /// </remarks>
        private readonly List<GameObject> radialSelectorGameObjects = new();

        /// <summary>
        /// Name of the input action which, when triggered, will display the input handler radial
        /// selection menu.
        /// </summary>
        private const string radialMenuPromptActionName = "Secondary Button";

        // Caching fields for storing information that might be required when instantiating new
        // input handlers.
        private (Transform, Transform) simulationSpaceTransforms;
        private MultiplayerSession multiplayerSession;
        private TrajectorySession trajectorySession;
        private PhysicallyCalibratedSpace physicallyCalibratedSpace;
        private bool initialised = false;

        /// <summary>
        /// Upon awaking set up a new <c>XRRayInteractorSelector</c> so that input handlers can be
        /// disabled when the user interacts with the user interface. Controllers will be added to
        /// the selector as and when they are added to the arbiter itself.
        /// </summary>
        public void Awake()
        {
            xrRayInteractorSelector = new XRRayInteractorSelector();
            xrRayInteractorSelector.Initialise(this);
        }

        /// <summary>
        /// Initialises the input arbiter with all data necessary to instantiate an input handler.
        /// </summary>
        /// <param name="simulationSpaceTransforms">The transform objects associated with the
        /// simulation's outer and inner spaces, respectively.</param>
        /// <param name="multiplayerSession">Provide for input handlers requiring a multiplayer
        /// session, adhering to <c>IMultiplayerSessionDependentInputHandler</c>.</param>
        /// <param name="trajectorySession">Provide for input handlers requiring a trajectory
        /// session, adhering to <c>ITrajectorySessionDependentInputHandler</c>.</param>
        /// <param name="physicallyCalibratedSpace">Provide for input handlers requiring a link to
        /// the physically calibrated space, adhering to <c>IPhysicallyCalibratedSpaceDependentInputHandler</c>.</param>
        /// <remarks>
        /// This method primes the input arbiter with the information it needs to satisfy the various
        /// possible requirements that may be encountered when instantiating an input handler. Some
        /// input handlers will implement specific interfaces indicating that they require access to
        /// specific pieces of information in order to successfully operate.
        /// </remarks>
        public void Initialise(
            (Transform, Transform) simulationSpaceTransforms, MultiplayerSession multiplayerSession,
            TrajectorySession trajectorySession, PhysicallyCalibratedSpace physicallyCalibratedSpace)
        {
            // Calling the `Initialise` method more than once will result in undefined behaviour as
            // this change is not propagated through to the input handlers. This may be changed
            // later on, but for now it is blocked.
            if (initialised)
                throw new Exception("Input arbiter has already been instantiated.");

            // It is fully admitted that this is not an ideal solution to this type of problem. But
            // it will do for now.
            this.multiplayerSession = multiplayerSession;
            this.trajectorySession = trajectorySession;
            this.physicallyCalibratedSpace = physicallyCalibratedSpace;
            this.simulationSpaceTransforms = simulationSpaceTransforms;
            initialised = true;
        }

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
        public override bool RequestInputHandlerActivation(InputHandler handler,
            [CanBeNull] List<InputController> targetControllers = null)
        {
            // Note that specifying target controllers via the `targetControllers` argument is only
            // meaningful for activating hybrid input handlers on a single controller. For Mono and
            // Dual input handlers, which must activate on all bound controllers, specifying target
            // controllers is unnecessary.
            
            // If the arbiter is itself disabled then it will refuse to active any input handlers.
            if (!isActiveAndEnabled) return false;

            // The arbiter cannot safely activate an input handler that it is not in direct control of.
            if (!inputHandlers.Contains(handler))
                throw new Exception("Attempted to activate an unknown input handler.");

            // Refer the handler's stored `Controller` list to identify which controllers activation
            // should take place on if no specific controller is specified. This allows hybrid input
            // handlers to activate on one or both associated controllers. For Mono and Dual input
            // handlers, the provided targetControllers, if any, match their `Controllers` list.
            if (targetControllers == null)
                targetControllers = handler.Controllers.ToList();

            // Don't attempt to activate input handlers that are already active
            if (handler is HybridInputHandler hybrid)
            {
                // For hybrid handlers, being `Active` doesn't guarantee that it has access to the
                // requested controller.
                if ((targetControllers.TrueForAll(ctrl => !hybrid.IsRestricted(ctrl))) &&
                    (handler.State == State.Active)) return true;
            }
            else if (handler.State == State.Active) return true;

            // Ensure that any provided target controllers are valid; i.e you can only activate an
            // input handler on a controller that it is bound to.
            else if (targetControllers.Except(handler.Controllers.ToList()).Any())
                throw new Exception("Invalid attempt to activate input handler on an unbound, unrelated controller.");

            // Identify which input handlers are currently active on the target controller(s)
            List<InputHandler> activeHandlers = targetControllers.SelectMany(ActiveHandlers).ToList();

            // Retain only input handlers with bindings conflicting with the new handler's requirements.
            activeHandlers = FilterConflicts(activeHandlers, handler);

            // Activate the new input handler if no active conflicts are found on the desired controllers.
            if (activeHandlers.Count == 0)
            {
                // Hybrid input handlers must be "released" as being active does not mean that it
                // has access to the target controller;
                if (handler is HybridInputHandler i) foreach (var j in targetControllers) i.Release(j);
                // Other input handlers can be "activated"
                else handler.State = State.Active;
                return true;
            }

            // Deny activation if any active, conflicting handlers are locked and cannot be disabled.
            foreach (InputHandler activeHandler in activeHandlers)
                if (activeHandler.Locked) return false;

            // Loop over and deactivate conflicting active input handlers.
            foreach (InputHandler activeHandler in activeHandlers)
            {
                // For hybrid input handlers, restrict only the conflicting controllers instead of
                // fully disabling the handler.
                if (activeHandler is HybridInputHandler activeHybridInputHandler)
                {
                    // Determine the exact binding conflicts between the current active handler and
                    // the new handler.
                    var conflicts = new HashSet<InputAction>(
                        activeHandler.RequiredBindings().Intersect(handler.RequiredBindings()));

                    // Restrict the hybridInputHandler's access to controllers with conflicting actions.
                    targetControllers.Where(
                        controller => conflicts.Any(controller.InputActionMap.Contains)
                    ).ToList().ForEach(controller => activeHybridInputHandler.Restrict(controller));
                }
                // Whereas others are directly backgrounded 
                else activeHandler.Background();
            }

            // Activate the new input handler (release is used for hybrids).
            if (handler is HybridInputHandler hybridInputHandler
                ) targetControllers.ForEach(hybridInputHandler.Release);
            else handler.State = State.Active;


            // After activating a new handler, identify any controllers left without an active handler.
            List<InputController> danglingControllers = FindFreeControllers();

            foreach (InputController controller in danglingControllers)
            {
                // Find possible input handlers that could be placed on it
                List<InputHandler> possibleHandlers = SortByPriority(AssociatedHandlers(controller));

                // Remove conflicting duel input handlers from consideration.
                possibleHandlers = possibleHandlers
                    .Where(handler => !(handler is DualInputHandler) || !handler.RequiredBindings()
                        .Overlaps(handler.RequiredBindings())).ToList();

                // Activate the first in the list
                if (possibleHandlers.Count >= 1)
                    RequestInputHandlerActivation(possibleHandlers[0], new List<InputController> { controller });

                // Only perform this action in the first loop. The recursive call structure ensures
                // subsequent steps occur in further recursions.
                break;
            }

            // Return true to indicate that the request was successful
            return true;
        }

        /// <summary>
        /// Requests the deactivation of a specified input handler, optionally targeting specific
        /// controllers.
        /// </summary>
        /// <param name="handler">The input handler to be deactivated.</param>
        /// <param name="targetControllers">Optional list of controllers from which to deactivate
        /// the handler.</param>
        /// <returns>True if the deactivation request was successful; otherwise, false.</returns>
        /// <remarks>
        /// This method is responsible for deactivating an input handler, taking into account the
        /// need to preserve system integrity. It reflects the system's capability adaptively respond
        /// to decreased input requirements or to facilitate transitions between input handlers.
        ///
        /// Note that specifying target controllers via the <c>targetControllers</c> argument is
        /// only meaningful for activating hybrid input handlers on a single controller. For most
        /// other input handlers, which must activate on all bound controllers, specifying target
        /// controllers is unnecessary.
        /// </remarks>
        public override bool RequestInputHandlerDeactivation(InputHandler handler,
            [CanBeNull] List<InputController> targetControllers = null)
        {
            // Identify the controllers associated with the specified handler
            if (targetControllers == null) targetControllers = handler.Controllers.ToList();

            // Reject the deactivation request if the controller reports as locked
            if (handler.Locked) return false;

            // Hybrid input handlers are restricted whereas other handlers are directly backgrounded
            if (handler is HybridInputHandler hybrid) targetControllers.ForEach(ctrl => hybrid.Restrict(ctrl));
            else handler.Background();
            return true;
        }

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
        public override bool RequestInputHandlerToggle(
            InputHandler handler, [CanBeNull] List<InputController> targetControllers = null)
        {
            // Refuse to change the state of a locked input handler.
            if (handler.Locked) return false;

            // If the specified handler is not active, then request its activation.
            if (handler.State != State.Active) return RequestInputHandlerActivation(
                handler, targetControllers);

            // If a hybrid handler was provided along with a target controller
            else if (handler is HybridInputHandler hybrid && targetControllers != null)
            {
                // Whether or not a handler's access to a given controller must be retried ahead of
                // time rather than ad-hoc. This is because it is possible that restriction state
                // may change between steps of the loop shown below
                var restrictions = targetControllers.Select(ctrl => hybrid.IsRestricted(ctrl)).ToList();

                // If the restriction status is the same for all controllers then a single call can
                // be used to deal with them.
                if (!(restrictions.All(b => b) || restrictions.All(b => !b)))
                {
                    if (restrictions[0]) return RequestInputHandlerActivation(handler, targetControllers);
                    else return RequestInputHandlerDeactivation(handler, targetControllers);
                }

                // If access to some controllers are restricted but other are not then controllers
                // must be dealt with on an instance by instance basis. Here we restrict controllers
                // that are currently not restricted and unrestricted those that are.
                foreach (var (controller, isRestricted) in targetControllers.Zip(restrictions, (i, j) => (i, j)))
                {
                    bool success;
                    // If the restriction status changed for some reason, then just go with it. 
                    if (isRestricted != hybrid.IsRestricted(controller))
                        success = true;

                    if (isRestricted) success = RequestInputHandlerActivation(
                        handler, new List<InputController>() { controller });
                    else success = RequestInputHandlerDeactivation(
                        handler, new List<InputController>() { controller });
                    if (!success) return false;
                }

                return true;
            }
            // In all other cases deactivate the handler
            else return RequestInputHandlerDeactivation(handler, targetControllers);
        }

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
        public override bool RequestPauseOfInputHandlersOnController(InputController controller)
        {
            // Find all handlers that are currently active on the controller that the user is using
            // to interact with the UI.
            var activeHandlers = ActiveHandlers(controller);

            // Identify which, if any, active input handler is currently bound to the trigger button
            // on the specified controller. Note, only one such handler should be bound at a time.
            InputAction triggerAction = controller.InputActionMap.FindAction("Trigger");
            var filteredHandler = activeHandlers.FirstOrDefault(
                handler => handler.RequiredBindings().Contains(triggerAction));

            // If no conflicting input handler is found, then return `true` to indicate compliance.
            if (filteredHandler == null) return true;
            // If the conflicting input handler is locked, return `false` to indicate refusal.
            else if (filteredHandler.Locked) return false;
            // Else set the conflicting handler's state to `Paused` & return `true` to indicate compliance.
            else
            {
                // Store a reference to the paused handler so that it can be resumed later on.
                pausedHandlers[controller] = filteredHandler;
                filteredHandler.State = State.Paused;
                return true;
            }
        }

        /// <summary>
        /// Requests the resumption of previously paused input handlers on a specified controller.
        /// </summary>
        /// <param name="controller">The controller for which input handlers are to be resumed.</param>
        /// <returns>True if the resumption request was successful; otherwise, false.</returns>
        /// <remarks>
        /// This method enables the reactivation of input handlers that were previously paused, ensuring
        /// that normal input processing can resume once the need for the pause has been alleviated.
        /// </remarks>
        public override bool RequestResumptionOfInputHandlersOnController(InputController controller)
        {
            if (pausedHandlers.ContainsKey(controller))
            {
                pausedHandlers[controller].State = State.Active;
                pausedHandlers.Remove(controller);
            }
            return true;
        }

        /// <summary>
        /// Adds an input controller to the arbiter's management system.
        /// </summary>
        /// <param name="controller">The input controller to be added.</param>
        /// <remarks>
        /// This method ensures that input controllers can be added into the arbiter's control system.
        /// Upon invocation, this method will bring the specified controller under the watch of the
        /// arbiter. Compatible input handlers will be automatically instantiated as and when they
        /// are encountered.
        /// </remarks>
        public override void AddController(InputController controller)
        {
            // 0.0) PRELIMINARY SET UP

            // Prevent the addition of redundant controllers as this does nothing but cause problems.
            if (controllers.Contains(controller)) return;

            // When adding a new controller it is possible for other controllers to be affected.
            // For example, the addition of a second controller may give the first controller
            // access to a new dual or hybrid input handler.
            HashSet<InputController> affectedControllers = new HashSet<InputController>();

            // 1.0) INPUT HANDLER SET UP
            // 1.1) Single-Controller Set Up
            // Loop over all one-controller input handlers.
            foreach (var handlerType in FilterSingleInputHandlers(knownInputHandlerTypes))
            {
                // Try to initialise a new input handler with the newly supplied controller. If the
                // input handler is incompatible with the controller then this call will return null.
                InputHandler handler = TryCreateAndBindInputHandler(controller, handlerType);

                // Add the newly created input handler to the handlers lis, if applicable.
                if (handler != null) inputHandlers.Add(handler);
            }
            // 1.2) Dual- and Hybrid-Controller Set Up
            // Loop over all two-controller input handlers.
            foreach (var handlerType in FilterDoubleInputHandlers(knownInputHandlerTypes))
            {
                // Then over all controller pairs.
                foreach (var otherController in controllers)
                {
                    InputHandler handler = TryCreateAndBindInputHandler(
                        controller, otherController, handlerType);

                    if (handler != null)
                    {
                        inputHandlers.Add(handler);
                        affectedControllers.Add(otherController);
                    }
                }
            }

            // Add the controller to the list of known controllers.
            controllers.Add(controller);

            // 2.0) INPUT SELECTOR SET UP:

            // 2.1) XR-Ray Interactor Selector
            // Hook the controller into the `xrRayInteractorSelector` entity so that any input
            // handlers on it can be paused, if necessary, when the user interacts with user
            // interface elements.
            if (controller is BasicInputController basicController)
                xrRayInteractorSelector.AddController(basicController);

            // 2.2) Radial Selection Menu Selector
            BuildRadialModeSelectionMenu(controller);

            // Rebuild the radial selection menus for other affected controllers so that
            // they display any newly created options.
            foreach (var otherController in affectedControllers)
                BuildRadialModeSelectionMenu(otherController);
        }

        
        /// <summary>
        /// Registers a new input handler type within the arbiter's system.
        /// </summary>
        /// <param name="handlerType">The type of the input handler to be added.</param>
        /// <remarks>
        /// This method is tasked with adding new input handler types. Upon addition of a new input
        /// handler, the arbiter will perform a test attempt to identify compatible handlers with
        /// with the new input handler can be instantiated. If none are identified, the handler
        /// type will be retested upon the addition of a new controller.
        /// </remarks>
        public override void AddHandler(Type handlerType)
        {
            // Ensure the type specified by the `handlerType` argument is actually an `InputHandler`
            // derived class.
            if (!typeof(InputHandler).IsAssignableFrom(handlerType) || handlerType.IsAbstract)
                throw new ArgumentException(
                    "Handler type must be a non-abstract subclass of InputHandler", nameof(handlerType));

            // Prevent the addition of redundant handler types as this does nothing but cause problems.
            if (knownInputHandlerTypes.Contains(handlerType)) return;

            // Add the supplied input handler to the list of known input handler types. This is used
            // by the `AddController` when adding a new controller. 
            knownInputHandlerTypes.Add(handlerType);

            // A hash-set used to keep track of which controllers the new input handler has bound
            // to. This is used to determine which radial menus need might need to be updated to
            // account for the new input handlers.
            HashSet<InputController> affectedControllers = new HashSet<InputController>();

            // If `handlerType` is of type `MonoInputHandler`.
            if (handlerType.IsSubclassOf(typeof(MonoInputHandler)))
            {
                // Then loop over and see which controllers it can be applied to
                foreach (InputController controller in controllers)
                {
                    // If this controller-input handler pairing is valid then add it to the list
                    InputHandler handler = TryCreateAndBindInputHandler(controller, handlerType);
                    if (handler != null)
                    {
                        inputHandlers.Add(handler);
                        affectedControllers.Add(controller);
                    }

                }
            }
            // If `handlerType` is of type `DualInputHandler` or `HybridInputHandler`.
            else if (handlerType.IsSubclassOf(typeof(DualInputHandler)) || handlerType.IsSubclassOf(typeof(HybridInputHandler)))
            {
                // Loop over and identify which controller pairs the input handler can be applied too
                for (int i = 0; i < controllers.Count - 1; i++)
                {
                    for (int j = i + 1; j < controllers.Count; j++)
                    {
                        InputHandler handler = TryCreateAndBindInputHandler(controllers[i], controllers[j], handlerType);
                        if (handler != null)
                        {
                            inputHandlers.Add(handler);
                            affectedControllers.Add(controllers[i]);
                            affectedControllers.Add(controllers[j]);
                        }
                    }
                }
            }
            // Anything else is unknown and should error out.
            else
                throw new ArgumentException("Handler type is unknown", nameof(handlerType));

            // Rebuild the radial selection menus so that they display the newly created options
            foreach (var controller in affectedControllers)
                BuildRadialModeSelectionMenu(controller);
        }
        
        /// <summary>
        /// Attempts to create and bind a mono input handler of a specified type to a given controller.
        /// </summary>
        /// <param name="controller">The input controller with which the handler is to be
        /// associated.</param>
        /// <param name="handlerType">The type of the input handler to be created and bound.</param>
        /// <returns>An initialised and bound instance of the input handler if compatible;
        /// otherwise, null.</returns>
        /// <exception cref="ArgumentException">Thrown when the provided handler type is not a valid
        /// subclass of InputHandler or is abstract.</exception>
        /// <remarks>
        /// The specified handler type is validated to ensure it is a valid subclass of InputHandler
        /// and is not abstract. A new GameObject is created and named according to the handler type
        /// and controller's hand dominance, which is then used to instantiate the handler component.
        /// The newly created handler is checked for compatibility with the provided controller; if
        /// compatible, it is bound to the controller, dependencies are fulfilled, and the handler is
        /// initialised before being returned. If the handler is found incompatible, the GameObject
        /// is destroyed, and null is returned. The process acknowledges the inefficiency of creating
        /// and potentially discarding GameObjects but is constrained by the limitations of current
        /// C# capabilities regarding static abstract methods in interfaces.
        /// </remarks>
        [CanBeNull]
        private InputHandler TryCreateAndBindInputHandler(InputController controller, Type handlerType)
        {
            // Ensure that the input handler type provided is actually a input handler type and
            // not some other random class.
            if (!typeof(InputHandler).IsAssignableFrom(handlerType) || handlerType.IsAbstract)
                throw new ArgumentException(
                    "Handler type must be a non-abstract subclass of InputHandler", nameof(handlerType));


            // Identify if this controller is held in the dominant hand or the non-dominant hand.
            // Strictly, speaking this is unnecessarily as the arbiter is hand dominance agnostic.
            string handDominanceString = controller.IsDominant ? "Dominant-Hand" : "NonDominant-Hand";

            // Create a new game object to which the input handler component will be attached
            GameObject handlerObject = new GameObject($"{handlerType.Name}_{handDominanceString}");

            // Input handlers are made children of the arbiter by default, however they may
            // override this behaviour locally if desired.
            handlerObject.transform.parent = transform;

            // Instantiate the handler component and cast it into a MonoInputHandler type.
            InputHandler handler = (InputHandler)handlerObject.AddComponent(handlerType);

            // Check if the input handler is compatible with the controller
            if (handler.IsCompatibleWithInputController(controller))
            {
                // If so, bind the controller to the input handler.
                handler.BindController(controller);
                // Fulfil any handler specific dependencies.

                FulfilDependencies(handler);

                // Initialise the handler
                handler.Initialise();

                // Ensure that handlers are deactivated by default.
                handlerObject.SetActive(false);

                return handler;
            }
            // Otherwise delete the GameObject along with its input handler
            else
            {
                Destroy(handlerObject);
                return null;
            }

            /* Developer's Notes:
             * The absurdity of creating a GameObject/InputHandler just to have to delete it
             * again if found to be incompatible is acknowledged. However, C# understandably
             * does not support static abstract methods, & other workarounds are too complex
             * to justify implementing this early on in the development process. Thankfully,
             * the upcoming .NET 6 architecture will introduce static abstract members for
             * interfaces. Once Unity brings in support for this the above hack can be done
             * away with in favour of an interface. Assuming that Unity eventually gets around
             * to updating the game engine to the newest .NET architecture.
             * https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/6.0/static-abstract-interface-methods
             */
        }
        
        /// <summary>
        /// Attempts to create and bind a dual or hybrid input handler of a specified type to a given controller.
        /// </summary>
        /// <param name="controller">The first input controller with which the handler is to be
        /// associated.</param>
        /// <param name="otherController">The second input controller with which the handler is to be
        /// associated.</param>
        /// <param name="handlerType">The type of the input handler to be created and bound.</param>
        /// <returns>An initialised and bound instance of the input handler if compatible; otherwise,
        /// null.</returns>
        /// <exception cref="ArgumentException">Thrown when the provided handler type is not a valid
        /// subclass of InputHandler or is abstract.</exception>
        [CanBeNull]
        private InputHandler TryCreateAndBindInputHandler(InputController controller, InputController otherController, Type handlerType)
        {
            // This function is just a repeat of the above `TryCreateAndBindInputHandler` method
            // but for input handlers that require two controllers to operate. Comments are note
            // supplied here as they would effectively be identical to those in the sibling function.

            if (!typeof(InputHandler).IsAssignableFrom(handlerType) || handlerType.IsAbstract)
                throw new ArgumentException("Handler type must be a non-abstract subclass of InputHandler", nameof(handlerType));

            GameObject handlerObject = new GameObject($"{handlerType.Name}");
            handlerObject.transform.parent = transform;
            InputHandler handler = (InputHandler)handlerObject.AddComponent(handlerType);

            if (handler.IsCompatibleWithInputController(controller) && handler.IsCompatibleWithInputController(otherController))
            {
                handler.BindController(controller);
                handler.BindController(otherController);
                FulfilDependencies(handler);
                handler.Initialise();
                handlerObject.SetActive(false);
                return handler;
            }
            else
            {
                Destroy(handlerObject);
                return null;
            }
        }

        /// <summary>
        /// Build, or rebuild, the input handler radial selection menu.
        /// </summary>
        /// <param name="controller">The controller whose radial selection menu that is to be
        /// rebuilt.</param>
        /// <remarks>
        /// Controllers are to be equipped with a radial menu, enabling users to manually activate
        /// or deactivate user-selectable input handlers as desired. The initial invocation of this
        /// method on a controller shall construct the radial menu. Any further invocations are to
        /// trigger a reconstruction of the menu, allowing it to be updated.
        /// </remarks>
        private void BuildRadialModeSelectionMenu(InputController controller)
        {
            
            // In the basic input arbiter class, a radial selection menu is created for each controller,
            // corresponding to the lifecycle of its associated controller. Thus, the n-th game object
            // in the `radialSelectorGameObjects` list is always associated with the n-th controller
            // in the `Controllers` list, ensuring alignment throughout their creation & destruction.
            int index = controllers.IndexOf(controller);


            // If the radial menu exists, it is destroyed and rebuilt for updates. This approach, not
            // adhering to best practices of avoiding object destruction for updates, is temporary
            // until the radial menu system is enhanced to allow dynamic updates.

            var radialObject = new GameObject(
                $"Radial Mode Selection Menu ({(controller.IsDominant ? "Dominant-Hand" : "NonDominant-Hand")})");

            // Make the selection menu a child of the arbiter.
            radialObject.transform.parent = transform;

            // Add a new radial menu holder to `radialSelectorGameObjects` if none exists at the specified index.
            if (radialSelectorGameObjects.Count < index + 1)
                // Then add the new object to the list
                radialSelectorGameObjects.Add(radialObject);
            // Replaces a null entry in the list, indicating a previously deleted holder.
            else if (radialSelectorGameObjects[index] == null)
                // Update the contents of the list
                radialSelectorGameObjects[index] = radialObject;
            // Replaces an existing holder by first destroying the current and then setting the new one.
            else
            {
                Destroy(radialSelectorGameObjects[index]);
                radialSelectorGameObjects[index] = radialObject;
            }

            // Locate the game object to which the radial menu selector component should be attached.
            GameObject modeSelectionMenuObject = radialSelectorGameObjects[controllers.IndexOf(controller)];
            
            // Build the radial mode selection menu. 
            var radialMenu = modeSelectionMenuObject.AddComponent<RadialInputSelector>();

            // Perform the general setup and initialisation process. Note that this will register
            // a call back to the `ActivateInputProcessor` when the user selects and option.
            radialMenu.Initialise(inputHandlers, this, controller,
                controller.InputActionMap.FindAction(radialMenuPromptActionName));

            // Ensure the that the radial menu is enabled/disabled if the arbiter is
            radialMenu.enabled = enabled;

        }

        /// <summary>
        /// Check for and satisfy any dependencies specified by an input handler.
        /// </summary>
        /// <param name="handler">Handler whose dependencies are to be satisfied.</param>
        private void FulfilDependencies(InputHandler handler)
        {
            // Check for and satisfy any dependencies specified by an input handler. Each input
            // handler may require access to different data sources. These requirements are made
            // known through the use of an appropriate interface. It is recognised that this
            // approach is very poorly written. This should really be abstracted and generalised
            // into a custom entity. Furthermore, source objects should not be stored within the
            // arbiter class as this just makes things dirty. 

            if (handler is ISimulationSpaceTransformDependentInputHandler simulationSpaceTransformDependentHandler)
                simulationSpaceTransformDependentHandler.SetSimulationSpaceTransforms(
                    simulationSpaceTransforms.Item1, simulationSpaceTransforms.Item2);

            if (handler is IMultiplayerSessionDependentInputHandler multiplayerSessionDependentHandler)
                multiplayerSessionDependentHandler.SetMultiplayerSession(multiplayerSession);

            if (handler is ITrajectorySessionDependentInputHandler trajectorySessionDependentHandler)
                trajectorySessionDependentHandler.SetTrajectorySession(trajectorySession);

            if (handler is IPhysicallyCalibratedSpaceDependentInputHandler physicallyCalibratedSpaceDependentHandler)
                physicallyCalibratedSpaceDependentHandler.SetPhysicallyCalibratedSpace(physicallyCalibratedSpace);
                
        }


        /// <summary>
        /// Determine all registered controllers currently lacking active input handlers.
        /// </summary>
        /// <returns>List of input controllers lacking active input handlers.</returns>
        private List<InputController> FindFreeControllers() =>
            controllers.Where(controller => ActiveHandlers(controller).Count == 0).ToList();
        
        /// <summary>
        /// Identify input handlers whose binding requirements conflict with those of a specified
        /// target input handler. This filters retains only those with binding requirements that
        /// directly clash with the target handler's requirements.
        /// </summary>
        /// <param name="candidateHandlers">Input handlers to be assessed for binding
        /// conflicts</param>
        /// <param name="targetHandler">The input handler against whose binding requirements the
        /// assessment is to be conducted</param>
        /// <returns>A list of input handlers with conflicting binding requirements relative to
        /// the target handler</returns>
        private List<InputHandler> FilterConflicts(List<InputHandler> candidateHandlers, InputHandler targetHandler)
        {
            var targetHandlerBindings = targetHandler.RequiredBindingNames();

            List<InputHandler> conflictingHandlers = new List<InputHandler>();

            foreach (InputHandler handler in candidateHandlers)
                if (targetHandlerBindings.Overlaps(handler.RequiredBindingNames()) && !ReferenceEquals(handler, targetHandler))
                    conflictingHandlers.Add(handler);

            return conflictingHandlers;
        }

        /// <summary>
        /// Return a list of all currently active input handlers that are sourcing inputs from a
        /// specific controller.
        /// </summary>
        /// <param name="controller">Only input handlers actively sourcing explicit user inputs
        /// form this controller will be returned.</param>
        /// <returns>List of input handlers actively sourcing inputs from the specified controller.</returns>
        private List<InputHandler> ActiveHandlers(InputController controller)
        {
            // Find all currently active handlers that are bound to the specified controller. An
            // additional check is required for hybrid input handlers as they may have their
            // access to the controller restricted, and thus do not count.
            return inputHandlers
                .Where(handler => handler.State == State.Active && handler.IsBoundToController(controller))
                .Where(handler => !(handler is HybridInputHandler hybridHandler) || !hybridHandler.IsRestricted(controller))
                .ToList();
        }
        
        /// <summary>
        /// Return a list of all input handlers that are bound to a specific controller.
        /// </summary>
        /// <param name="controller">Only input handlers bound to this controller will be returned.</param>
        /// <returns>List of input handlers bound to the specified controller.</returns>
        private List<InputHandler> AssociatedHandlers(InputController controller) =>
            inputHandlers.Where(handler => handler.IsBoundToController(controller)).ToList();
        
        /// <summary>
        /// Return a filtered list of input handler types containing only single-controller input
        /// handler, i.e. those deriving from the <c>MonoInputHandler</c> class.
        /// </summary>
        /// <param name="handlers">List of input handler types to filter.</param>
        /// <returns>Filtered list containing only single-controller input handler types.</returns>
        /// <remarks>
        /// This ancillary method exists to help reduce the verbosity of other methods.
        /// </remarks>
        private static List<Type> FilterSingleInputHandlers(List<Type> handlers) => handlers.Where(
            t => t.IsSubclassOf(typeof(MonoInputHandler)) && !t.IsAbstract).ToList();

        /// <summary>
        /// Return a filtered list of input handler types containing only input handlers that
        /// required two controllers, i.e. those deriving from either the <c>DualInputHandler</c>
        /// or <c>HybridInputHandler</c> classes.
        /// </summary>
        /// <param name="handlers">List of input handler types to filter.</param>
        /// <returns>Filtered list containing only input handler types requiring two controllers.</returns>
        /// <remarks>
        /// This ancillary method exists to help reduce the verbosity of other methods.
        /// </remarks>
        private static List<Type> FilterDoubleInputHandlers(List<Type> handlers) => handlers.Where(
            t => (t.IsSubclassOf(typeof(DualInputHandler)) ||
                  t.IsSubclassOf(typeof(HybridInputHandler))) && !t.IsAbstract).ToList();

        /// <summary>
        /// Sorts a list of input handlers by priority, where available.
        /// </summary>
        /// <param name="handlers">Handlers to be sorted.</param>
        /// <returns>Sorted list of handlers.</returns>
        private static List<InputHandler> SortByPriority(List<InputHandler> handlers)
        {
            // Activating a new input handler will, in many situations, cause the currently active
            // input handler to be deactivated. This sometimes causes one of the other controllers
            // to be left without an active input handler on it, which is undesirable. Therefore,
            // an attempt will be made to identify a suitable handler to activate for it so that
            // it is not left doing nothing. Once a list of possible candidate handlers has been
            // created, it will be passed through this method to try & introduce some consistency
            // as to which handler is activated, rather than "whatever happens to be first in the
            // list". The actual order is not that important as this method is not intended to be
            // used all that much.

            // Identify the input handlers that implement the `IUserSelectableInputHandler` interface
            // as these will process a quantifiable priority value.
            var A = handlers.OfType<IUserSelectableInputHandler>().OrderByDescending(
                handler => handler.Priority).ThenBy(i => i.GetType().Name).Cast<InputHandler>().ToList();

            // Other input handlers are haphazardly sorted to provide some semblance of consistency
            var B = handlers.Except(A).OrderBy(i => i.GetType().Name).ThenBy(
                i => i.Controllers[0].IsDominant).ThenBy(i => i.Controllers[0].GetType().Name).ToList();

            // Combine and return the two lists.
            return A.Concat(B).ToList();
        }

        /// <summary>
        /// This is invoked whenever the input arbiter is disabled.
        /// </summary>
        /// <remarks>
        /// This ensure that the input handlers are explicitly disabled & do not continue to function
        /// in the background.
        /// </remarks>
        void OnDisable()
        {
            // Ensure that all input handlers are disabled when the arbiter is. Note that this will
            // not respect the handler's `Lock` flag.
            foreach (var handler in inputHandlers) if (handler.State != State.Disabled) handler.State = State.Disabled;

            // Disable the radial selection menus as they will not be needed when the input arbiter
            // is inactive.
            foreach (var menuObject in radialSelectorGameObjects) menuObject.GetComponent<RadialInputSelector>().enabled = false;
        }

        /// <summary>
        /// This is invoked whenever the input arbiter is re-enabled.
        /// </summary>
        /// <remarks>
        /// When the input arbiter is re-enabled the radial selection menus associated with it should
        /// also become active.
        /// </remarks>
        void OnEnable()
        {
            foreach (var menuObject in radialSelectorGameObjects) menuObject.GetComponent<RadialInputSelector>().enabled = true;
        }
    }
}