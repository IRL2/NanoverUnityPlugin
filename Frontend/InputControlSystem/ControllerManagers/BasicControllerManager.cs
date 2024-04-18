using Nanover.Frontend.InputControlSystem.InputControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nanover.Frontend.InputControlSystem.InputArbiters;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using InputDevice = UnityEngine.XR.InputDevice;
using Nanover.Frontend.InputControlSystem.InputHandlers;
using Nanover.Frontend.InputControlSystem.Utilities;
using Nanover.Grpc.Multiplayer;
using Nanover.Grpc.Trajectory;
using Nanover.Frontend.XR;

namespace Nanover.Frontend.InputControlSystem.ControllerManagers
{

    /// <summary>
    /// This controller manager is intended to work with the most basic virtual reality systems and
    /// hardware. As such, it supports only a single static simulation, along with a pair of basic
    /// held-in-hand XR-controller devices. This manager is the first, and the most basic, concrete
    /// implementation of the abstract base class <see cref="ControllerManager"/>.
    ///
    /// This entity is tasked with management of controller devices in a manner that aligns with user
    /// handedness and bootstrapping of the input control system. This entity is always attached to
    /// the top level object of the XR-rig within the Unity scene.
    /// </summary>
    /// <remarks>
    /// Upon instantiation, this class will start watching for connected XR-controller devices. When
    /// a new controller is detected, the manager will then create a new <c>GameObject</c> and
    /// <c>BasicInputDevice</c> component, which are added to the XR-rig. It will then dynamically
    /// respond to controller dis/re/connection events to ensure smooth operation. It will also
    /// instantiate and initialise an input arbiter thereby bootstrapping the input control system.  
    /// </remarks>
    public class BasicControllerManager : ControllerManager
    {
        /// <summary>
        /// Represents the input controller associated with the user's dominant hand.
        /// </summary>
        /// <remarks>
        /// This field is assigned at runtime based on the detection of the connected controller
        /// device and the user's handedness preference. It serves as a direct link to basic controller
        /// specific information.
        /// </remarks>
        private BasicInputController dominantHandController;

        /// <summary>
        /// Represents the input controller associated with the user's dominant hand.
        /// </summary>
        /// <remarks>
        /// This field is assigned at runtime based on the detection of the connected controller
        /// device and the user's handedness preference. It serves as a direct link to basic controller
        /// specific information.
        /// </remarks>
        private BasicInputController nonDominantHandController;

        /// <summary>
        /// The <c>GameObject</c> to which the <see cref="InputController"/> component representing
        ///  the user's dominant had is attached.
        /// </summary>
        /// <remarks>
        /// This object, along with all of the relevant components, are generated at runtime when
        /// the associated XR controller device is detected.
        /// device is detected.
        /// </remarks>
        private GameObject dominantHandControllerGameObject;

        /// <summary>
        /// The <c>GameObject</c> to which the <see cref="InputController"/> component representing
        ///  the user's non-dominant had is attached.
        /// </summary>
        /// <remarks>
        /// This object, along with all of the relevant components, are generated at runtime when
        /// the associated XR controller device is detected.
        /// device is detected.
        /// </remarks>
        private GameObject nonDominantHandControllerGameObject;

        /// <summary>
        /// The <see cref="BasicInputArbiter"/> component that is responsible for the
        /// <see cref="InputHandler">InputHandlers</see>.
        /// </summary>
        /// <remarks>
        /// The input arbiter is responsible for setting up, managing, and switching between the
        /// various input handlers.
        /// </remarks>
        private BasicInputArbiter arbiter;

        /// <summary>
        /// The <see cref="BasicInputArbiter"/> component that is responsible for the
        /// <see cref="InputHandler">InputHandlers</see>.
        /// </summary>
        /// <remarks>
        /// The input arbiter is responsible for setting up, managing, and switching between the
        /// various input handlers.
        /// </remarks>
        public BasicInputArbiter Arbiter
        {
            get => arbiter;
            set => arbiter = value;
        }

        /// <summary>
        /// The <c>GameObject</c> to which the <see cref="BasicInputArbiter"/> component specified
        /// by the <c>arbiter</c> field is attached.
        /// </summary>
        /// <remarks>
        /// The arbiter is placed on a separate, dedicated object to help clean up the object hierarchy.
        /// </remarks>
        private GameObject ArbiterObject;

        /* The callback functions that are invoked whenever a controller (dis)connects. These are
         * stored to allow subscriptions to be easily de-registered when the controller manager
         * instance terminates.
         */
        /// <summary> Invoked whenever the right controller's connection status changes.</summary>
        private Action<InputAction.CallbackContext> rightControllerCallback;
        /// <summary> Invoked whenever the left controller's connection status changes.</summary>
        private Action<InputAction.CallbackContext> leftControllerCallback;

        /// <summary>
        /// If true the arbiter start in the disabled state.
        /// </summary>
        /// <remarks>
        /// This can be used to prevent the input arbiter or its handlers from having any effect
        /// when the user is in the main menu screen.
        /// </remarks>
        protected bool sleepArbiterOnStart = false;

        /// <summary>Controller manager initialisation.</summary>
        protected void Awake()
        {
            // Start watching for controller connection/disconnection events
            RegisterControllerTrackingStateSubscriptions();

            // Create arbiter & associated game object. Although the arbiter is instantiated here, the
            // initialisation is actually performed later on by the `InitialiseInputArbiter` method.


            // Create the input arbiter object and the game object to which it is attached.
            ArbiterObject = new GameObject("Arbiter") { transform = { parent = transform } };
            arbiter = ArbiterObject.AddComponent<BasicInputArbiter>();


            /* Initialise the input arbiter by providing it with all of the information required to
             * build the input handlers. It should be noted that the arbiter will only instantiate
             * input handlers as and when it is provided with input controllers and input handler
             * types.
             *
             * The code below is more than a little "hacky", this will need to be replaced with a
             * cleaner approach later on down the line. However, it is "good enough" for the moment.
             */
            MultiplayerSession multiplayer = SystemObject.GetComponent<IMultiplayerSessionSource>().Multiplayer;
            TrajectorySession trajectory = SystemObject.GetComponent<ITrajectorySessionSource>().Trajectory;
            PhysicallyCalibratedSpace physicallyCalibratedSpace =
                SystemObject.GetComponent<IPhysicallyCalibratedSpaceSource>().PhysicallyCalibratedSpace;
            (Transform, Transform) simulationSpaceTransforms =
                SystemObject.GetComponent<ISimulationSpaceTransformSource>().SimulationSpaceTransforms;

            arbiter.Initialise(simulationSpaceTransforms, multiplayer, trajectory, physicallyCalibratedSpace);
            

            // Identify all permitted handlers and add them to the input arbiter. Controllers, are
            // provided to the arbiter as and when they are connected.
            foreach (var handler in GatherPermittedInputHandlers())
                arbiter.AddHandler(handler);

            // It is sometimes desirable to disable the arbiter at startup. This prevents the input
            // handlers from trying to perform actions while the user is still in the main menu.
            if (sleepArbiterOnStart) DisableArbiter();
            else EnableArbiter();
        }

        /// <summary>Upon terminating, stop watching for controller connection/disconnection events.</summary>
        protected void OnDestroy() => DeregisterControllerTrackingStateSubscriptions();

        /// <summary>
        /// Disable the input arbiter.
        /// </summary>
        public void DisableArbiter() => Arbiter.enabled = false;
        

        /// <summary>
        /// Enable the input arbiter.
        /// </summary>
        public void EnableArbiter()
        {
            Arbiter.enabled = true;
            // When enabling the arbiter ensure that the transform input handler is the first
            // selected, as it is the most commonly useful handler.
            var handler = Arbiter.InputHandlers.OfType<TransformHybridInputHandler>().FirstOrDefault();
            if (handler != null) Arbiter.RequestInputHandlerActivation(handler);
        }

        /// <summary>
        /// Identifies all non-abstract classes that are derived from the <code>InputHandler</code> class.
        /// <see cref="InputHandler"/>
        /// </summary>
        /// <returns>A list of all concrete, accessible <see cref="InputHandler"/> derived classes.</returns>
        private Type[] GatherPermittedInputHandlers()
        {
            // TODO: Implement server negotiation to check which handlers are permitted.

            // Find all non-abstract classes that are derived from the `InputHandler` class.
            Assembly assembly = Assembly.GetExecutingAssembly();
            var derivedTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsClass &&
                               !type.IsAbstract &&
                               type.IsSubclassOf(typeof(InputHandler)))
                .ToArray();

            // Communicate with the server to identify what modes are supported, and thus what
            // input handlers are supported. The input arbiter will sort out any compatibility
            // issues between controllers and input handlers.
            // ...
            // ...
            // Server check goes here
            // ...
            // ...


            return derivedTypes;
        }

        #region "Controller instantiation and management"


        /// <summary>
        /// Registers callbacks for changes in the tracking state of both the right & left controllers.
        /// These callbacks are designed to respond to the connection, disconnection, & reconnection of
        /// controller devices.
        /// </summary>
        /// <remarks>
        /// Upon registration, two distinct actions are bound to the respective right & left controller
        /// tracking state events. These actions invoke the <see cref="TrackingStateControllerUpdate"/>
        /// method with parameters that correspond to the handedness of the controller & its associated
        /// input action map. The registration facilitates the dynamic response to tracking state
        /// alterations throughout the lifecycle of the controller devices within the virtual construct.
        /// It is crucial that these subscriptions be deregistered appropriately when the controller
        /// manager instance is disposed of to prevent memory leaks or unintended behaviour.
        /// </remarks>
        private void RegisterControllerTrackingStateSubscriptions()
        {

            // Ensure that the input action map is enabled.
            InputActionAsset.Enable();

            // Subscribe the TrackingStateControllerUpdate method to changes in the "tracking state"
            // for the right and left controllers.
            InputActionMap rightControllerInputActionMap = InputActionAsset.FindActionMap("Right Controller");
            rightControllerCallback = context => TrackingStateControllerUpdate(context, rightControllerInputActionMap, InputDeviceCharacteristics.Right);
            rightControllerInputActionMap.FindAction("Tracking State").performed += rightControllerCallback;

            InputActionMap leftControllerInputActionMap = InputActionAsset.FindActionMap("Left Controller");
            leftControllerCallback = context => TrackingStateControllerUpdate(context, leftControllerInputActionMap, InputDeviceCharacteristics.Left);
            leftControllerInputActionMap.FindAction("Tracking State").performed += leftControllerCallback;
        }

        /// <summary>
        /// Activate a controller object, creating one if necessary.
        /// </summary>
        /// <param name="controllerInputActionMap">Action map associated with the controller.</param>
        /// <param name="handedness">Handedness of the controller. This is used to differentiate
        /// the right and left controllers.</param>
        private void Bind(InputActionMap controllerInputActionMap, InputDeviceCharacteristics handedness)
        {
            // Identify and access by reference the relevant  controller.
            ref GameObject controllerObject = ref GetControllerObjectByReference(handedness);
            
            // 1. a new controller has just connected for the first time. In which case a new
            //    controller instance must be created.
            if (controllerObject == null)
            {
                // Create blank game object to represent the controller and make it a child of the controller manager
                controllerObject = new GameObject($"{handedness} Controller");
                controllerObject.transform.SetParent(transform);

                // Add a new `Controller` component to the newly created game object
                BasicInputController controller = controllerObject.AddComponent<BasicInputController>();

                // Locate the input device associated with the right/left controller
                var controllerInputDevices = new List<InputDevice>();
                var desiredCharacteristics = handedness | InputDeviceCharacteristics.Controller;
                InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, controllerInputDevices);

                // Pass the required information to the controllers initialisation function to finalise the setup process
                controller.Initialise(controllerInputActionMap, controllerInputDevices.FirstOrDefault(), handedness == Handedness);
            }

            // 2. or a disconnected, but already registered, controller has just been reconnected.
            //    Thus the controller should be re-enabled.
            else
                controllerObject.SetActive(true);
            
        }

        /// <summary>
        /// Deactivate a controller object.
        /// </summary>
        /// <param name="controllerInputActionMap">Action map associated with the controller.</param>
        /// <param name="handedness">Handedness of the controller. This is used to differentiate
        /// the right and left controllers.</param>
        private void Unbind(InputActionMap controllerInputActionMap, InputDeviceCharacteristics handedness)
        {
            ref GameObject controllerObject = ref GetControllerObjectByReference(handedness);
            if (controllerObject != null)
            {
                controllerObject.SetActive(false);
            }
        }

        /// <summary>
        /// Deregisters the callbacks previously assigned to the tracking changes of the right
        /// & left controllers, ensuring that they no longer respond to tracking state events.
        /// </summary>
        /// <remarks>
        /// This method is responsible for the removal of the registered callbacks from the tracking
        /// state events for both controllers. Such an operation is essential during the teardown
        /// process of the controller manager to prevent potential memory leaks & to ensure that
        /// the callbacks do not persist beyond their intended scope. Deregistration is typically
        /// invoked when the controller manager is being destroyed or when the tracking subscriptions
        /// are no longer required, thus maintaining the integrity of the system's event handling.
        /// </remarks>
        private void DeregisterControllerTrackingStateSubscriptions()
        {
            InputActionAsset.FindActionMap("Right Controller").FindAction("Is Tracked").performed -= rightControllerCallback;
            InputActionAsset.FindActionMap("Right Controller").FindAction("Is Tracked").canceled -= rightControllerCallback;
            rightControllerCallback = null;

            InputActionAsset.FindActionMap("Left Controller").FindAction("Is Tracked").performed -= leftControllerCallback;
            InputActionAsset.FindActionMap("Left Controller").FindAction("Is Tracked").canceled -= leftControllerCallback;
            leftControllerCallback = null;
        }


        /// <summary>
        /// Retrieves a reference to the controller based on the specified handedness.
        /// </summary>
        /// <param name="handedness">The InputDeviceCharacteristics specifying the handedness of the controller (e.g., Right or Left).</param>
        /// <returns>A reference to the <see cref="BasicInputController"/> that represents either the dominant or non-dominant controller,
        /// corresponding to the handedness.</returns>
        /// <remarks>
        /// This allows the controller to be referenced in a dominance agnostic manner.
        /// </remarks>
        private ref BasicInputController GetControllerReference(InputDeviceCharacteristics handedness) => ref (
            (handedness == Handedness) ? ref dominantHandController : ref nonDominantHandController);

        /// <summary>
        /// Retrieves a reference to the controller GameObject based on the specified handedness.
        /// </summary>
        /// <param name="handedness">The InputDeviceCharacteristics specifying the handedness of the controller (e.g., Right or Left).</param>
        /// <returns>A reference to the GameObject that represents either the dominant or non-dominant controller, corresponding to the handedness.</returns>
        /// <remarks>
        /// This allows the controller objects to be referenced in a dominance agnostic manner.
        /// </remarks>
        private ref GameObject GetControllerObjectByReference(InputDeviceCharacteristics handedness) => ref (
            (handedness == Handedness) ? ref dominantHandControllerGameObject : ref nonDominantHandControllerGameObject);


        /// <summary>
        /// Invoked in response to a change in the connection status of a controller. It manages the
        /// lifecycle of controller objects, handling their creation upon connection and deactivation
        /// upon loss of connection.
        /// </summary>
        /// <param name="context">The context of the input action callback, providing the event
        ///     data.</param>
        /// <param name="controllerInputActionMap">The input action map associated with the controller
        ///     whose state has changed.</param>
        /// <param name="handedness">The handedness characteristic of the controller, indicating
        ///     whether it is the left or right controller.</param>
        private void TrackingStateControllerUpdate(
            InputAction.CallbackContext context, InputActionMap controllerInputActionMap, InputDeviceCharacteristics handedness)
        {

            // Identify if the primary or secondary controller field is to be removed.
            ref GameObject controllerObject = ref GetControllerObjectByReference(handedness);
            ref BasicInputController controller = ref GetControllerReference(handedness);

            // A tracking state value of zero indicates that no tracking data is provided & therefor
            // no controller is connected. If the tracking state value changes to anything other
            // than zero then either a controller has just connected or is has started offering more
            // or less data than it did previously.
            if (context.ReadValue<int>() != (int)InputTrackingState.None)
            {
                // 1. a new controller has just connected for the first time. In which case a new
                //    controller instance must be created.
                if (controllerObject == null)
                {
                    // Create blank game object to represent the controller and make it a child of the controller manager
                    controllerObject = new GameObject($"{handedness} Controller");
                    controllerObject.transform.SetParent(transform);

                    // Add a new `Controller` component to the newly created game object
                    controller = controllerObject.AddComponent<BasicInputController>();

                    // Locate the input device associated with the right/left controller
                    var controllerInputDevices = new List<InputDevice>();
                    var desiredCharacteristics = handedness | InputDeviceCharacteristics.Controller;
                    InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, controllerInputDevices);

                    // Pass the required information to the controllers initialisation function to finalise the setup process
                    controller.Initialise(controllerInputActionMap, controllerInputDevices.FirstOrDefault(), handedness == Handedness);

                    // Now that the controller has been initialised it can be passed to the input arbiter
                    Arbiter.AddController(controller);
                }

                // 2. or a disconnected, but already registered, controller has just been reconnected.
                //    Thus the controller should be re-enabled.
                else if (!controllerObject.activeSelf)
                    controllerObject.SetActive(true);
            }

            /* Developer's Notes:
             * The ability to disable controllers has been temporarily removed as it is more than a
             * little overzealous. Prior to the Unity `ActionBasedController` component being added
             * this worked fine. A controller would only be seen as being disconnected when it had
             * its battery removed. Now, complications with the Unity XR Toolkit mean that a controller
             * is seen as "disconnected" whenever it looses line of site with the headset. Work must
             * be undertaken to implement a more rugged and reliable means of detecting true controller
             * disconnection events.
             */

            // Note that the following code is from when the `isTracked` features was used. However,
            // this was found to be unreliable in practice. If this is to be re-implemented then it
            // will need to be updated to accommodate this.
            //// Alternatively, if the "button" is "released" then Unity is signalling that the
            //// controller has just unregistered. 
            //else if (context.phase == InputActionPhase.Canceled)
            //{
            //    if (controllerObject != null)
            //        // Thus the controller should be disabled (but only if it exists). Note that
            //        // the associated input actions will still be accessible and the input
            //        // _arbiter will be unaffected. However, this may be subject to change.
            //        controllerObject.SetActive(false);

            //    // Prevent other parts of Unity from disabling the action map. If this is not done then
            //    // the action map will be disabled by Unity and so the reconnection events will never
            //    // trigger.
            //    controllerInputActionMap.Enable();
            //}

        }
        
        #endregion

    }


}