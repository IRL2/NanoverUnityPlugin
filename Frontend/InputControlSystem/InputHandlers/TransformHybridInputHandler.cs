using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using Nanover.Frontend.InputControlSystem.InputControllers;
using Nanover.Frontend.InputControlSystem.Utilities;
using Nanover.Core.Math;
using Nanover.Grpc.Multiplayer;
using UnityEngine.UIElements;
using Nanover.Frontend.XR;

namespace Nanover.Frontend.InputControlSystem.InputHandlers
{

    /// <summary>
    /// The <c>TransformHybridInputHandler</c> class is designed to facilitate the manipulation of
    /// objects in a virtual construct environment using controllers. This class inherits from the
    /// <see cref="HybridInputHandler"/> and implements <see cref="ISystemDependentInputHandler"/>
    /// and <see cref="IUserSelectableInputHandler"/>. It allows users to rotate, translate, and
    /// scale objects in three-dimensional space using either one or two controllers.
    /// </summary>
    /// <remarks>
    /// The class is equipped with a series of private fields that store the state of the object,
    /// the transform of the target object to be manipulated, arrays of
    /// <see cref="IndirectTrackedPoseDriver"/> for single-controller manipulation, and a
    /// <see cref="DualIndirectTrackedPoseDriver"/> for dual-controller manipulation. Additionally,
    /// an <see cref="InputActionDecoupler"/> is used to manage the input actions from the
    /// controllers.
    ///
    /// When initialised, the class sets up the necessary drivers and configures the
    /// <see cref="InputActionDecoupler"/> to handle various input actions. It also includes methods
    /// to set the system to be manipulated, perform state changes, restrict or release controller
    /// access, and check compatibility with different input controllers.
    ///
    /// The <c>TransformHybridInputHandler</c> is pivotal in providing an intuitive and responsive
    /// user experience in VR applications, where precise control over object manipulation is
    /// essential. The smooth handling of translation, rotation, and scaling actions offers users a
    /// natural and immersive interaction with virtual objects.
    ///
    /// Usage:
    /// - The <c>Initialise</c> method must be invoked before an instance can be used.
    /// - The <c>SetSystem</c> method must be called with the GameObject that is to be transformed.
    /// - The <c>State</c> property can be set to enable or disable the handler.
    /// - Compatibility with specific controllers can be checked using <c>IsCompatibleWithInputController</c>.
    /// </remarks>

    public class TransformHybridInputHandler: HybridInputHandler, IUserSelectableInputHandler, IMultiplayerSessionDependentInputHandler, IPhysicallyCalibratedSpaceDependentInputHandler, ISimulationSpaceTransformDependentInputHandler
    {
        /// <summary>
        /// Name of the input handler, as is to be presented to the user.
        /// </summary>
        public string Name => "Transform";

        /// <summary>
        /// Icon to be displayed when visually representing the input handler.
        /// </summary>
        public Sprite Icon => Resources.Load<Sprite>("UI/Icons/InputHandlers/View");

        /// <summary>
        /// Priority of the input handler relative to others.
        /// </summary>
        /// <remarks>
        /// The transform handler is given the highest priority as it is relevant to all
        /// situations.
        /// </remarks>
        public ushort Priority => 65535;

        /// <summary>
        /// Current state of the input handler. This dictates the handler's level of activity and
        /// responsiveness to input.
        /// </summary>
        public override State State
        {
            get => state;
            set => PerformStateChange(state, value);
        }

        /// <summary>
        /// Name of the action that is to be bound two on each controller. This is hard-coded to the
        /// grip button as it is the most conceptually intuitive action for it to be bound to. 
        /// </summary>
        private const string buttonName = "Grip";

        /// <summary>
        /// A <see cref="PhysicallyCalibratedSpace">physically calibrated space</see> entity that
        /// represents the shared coordinate space which the users inhabit.
        /// </summary>
        /// <remarks>
        /// This is needed to allow for positions be converted from client-side coordinate space
        /// to an abstract virtual shared server space. Without this translation layer each client
        /// would see objects at different positions in physical space. This is only important when
        /// users occupy the same real world physical space (i.e. are in the same room).
        /// </remarks>
        private PhysicallyCalibratedSpace physicallyCalibratedSpace;

        /// <summary>
        /// Represents the transform of the target object which is to be manipulated. This transform
        /// is used as the focal point for all translation, rotation, and scaling operations performed
        /// by the input handler.
        /// </summary>
        private Transform targetTransform;

        /// <summary>
        /// The multiplayer resource representation of the target's transform.
        /// </summary>
        /// <remarks>
        /// This is used to allow manipulations of the target system's transform to be synced
        /// with the server.
        /// </remarks>
        private MultiplayerResource<Transformation> targetTransformResource;

        /// <summary>
        /// An array of <c>IndirectTrackedPoseDriver</c> instances. Each driver in this array is
        /// responsible for handling the input from a single controller, providing functionality
        /// for smoothed translation and rotation manipulation of the target object. These drivers
        /// are utilised when the object is being manipulated by a single controller at any given time.
        /// </summary>
        private IndirectTrackedPoseDriver[] monoDrivers;

        /// <summary>
        /// A <c>DualIndirectTrackedPoseDriver</c> instance that provides functionality for coordinated,
        /// smoothed manipulation of the target object's translation, rotation, & scaling when inputs
        /// from two controllers are used simultaneously. This driver enables more complex and nuanced
        /// control over the object, such as adjusting its size or orientation in a more intuitive
        /// manner.
        /// </summary>
        private DualIndirectTrackedPoseDriver dualDriver;

        /// <summary>
        /// An instance of <c>InputActionDecoupler</c> used to manage and decouple the input actions
        /// from the controllers. It is instrumental in controlling how and when the input action
        /// from the controllers affect the target object, allowing for a more refined & controlled
        /// manipulation process. This decoupler facilitates the switching between single and dual
        /// controller inputs, ensuring smooth transitions and consistent behaviour.
        /// </summary>
        private InputActionDecoupler decoupler;

        /// <summary>
        /// Indicates whether the input handler is currently engaged in manipulating the target object's
        /// transform. This flag is set to true when manipulation begins, reflecting that the input
        /// handler has taken control of the target object to apply transformations. It is used to
        /// manage the active manipulation state & synchronise changes with the server or other systems.
        /// </summary>
        private bool isEngaged = false;

        /// <summary>
        /// Determines whether the input handler is allowed to engage & start manipulating the
        /// target object's transform. This flag is used to prevent re-engagement of manipulation
        /// under certain conditions, such as when a lock on the multiplayer resource representing
        /// the target transform is rejected. It ensures that manipulation attempts are appropriately
        /// gated, requiring a reset (e.g., releasing and re-pressing grip buttons) before manipulation
        /// can be reattempted. This mechanism helps in avoiding unintended or conflicting transformations.
        /// </summary>
        private bool isAllowedToEngage = true;

        /// <summary>
        /// Transform of the simulation visualisation space that is to be transformed.
        /// </summary>
        /// <param name="visualisationSpaceTransform">The target transform to be manipulated.</param>
        //public void SetVisualisationSpaceTransform(Transform visualisationSpaceTransform) =>
        //    targetTransform = visualisationSpaceTransform;


        public void SetSimulationSpaceTransforms(Transform outerSimulationSpace, Transform innerSimulationSpace) =>
            targetTransform = outerSimulationSpace;

        /// <summary>
        /// Set the required multiplayer session.
        /// </summary>
        /// <param name="multiplayer">The required multiplayer session</param>
        public void SetMultiplayerSession(MultiplayerSession multiplayer)
        {
            targetTransformResource = multiplayer.SimulationPose;
        }

        public void SetPhysicallyCalibratedSpace(PhysicallyCalibratedSpace physicallyCalibratedSpace) =>
            this.physicallyCalibratedSpace = physicallyCalibratedSpace;

        private void Update()
        {

            // If this entity is engaged, then the user is actively manipulating the target system's
            // transform. Thus, such changes should be synced with the server.
            if (isEngaged)
            {
                // Cast the local-space `UnityEngine.Transform` instance into a `Transformation`.
                // Note that although this is labelled as "world space" it is technically local
                // space as far as Unity is concerned.
                var worldTransformation = Transformation.FromTransformRelativeToParent(targetTransform);
                
                // Perform a quick sanity check to ensure that the transform represents something sensible.
                ClampToSensibleValues(worldTransformation);

                // Convert from client-side space to a common user-agnostic "calibrated space".
                var calibratedTransformation =
                    physicallyCalibratedSpace.TransformPoseWorldToCalibrated(worldTransformation);

                // Attempt to push this change to the server via the associated multiplayer resource.
                // If this fails, then a `MultiplayerResource.LockRejected` event will be triggered.
                // This event would be caught and processed by `SimulationTransposeLockRejected`.
                targetTransformResource.UpdateValueWithLock(calibratedTransformation);
            }
        }

        /// <summary>
        /// Callback method to deal with situations in which the multiplayer resource lock request
        /// for the target transform is denied.
        /// </summary>
        /// <remarks>
        /// If the lock acquisition request for the multiplayer resource representing the target
        /// transform is rejected then another agent must be manipulating the system already. Thus
        /// it is forbidden to modify the value. In this case the manipulation event should be
        /// aborted and no further attempts should be made to modify the target transform. This,
        /// will lock out manipulation events until the user releases all grip buttons & presses
        /// them down again. Furthermore, the local transform should be reset the the last known
        /// good value before manipulation attempts were made.
        /// </remarks>
        private void SimulationTransposeLockRejected()
        {
            // If the lock acquisition attempt is rejected then any local changes that might have
            // been made to the target system's transform must be undone. This is because the code
            // proceeds under the assumption that the lock request will be accepted. The manipulation
            // event must then be aborted as letting the user repeatedly try to transform the system
            // only to block the attempt & resent the system serves no purpose other than to introduce
            // an necessary source of frame stutter.

            // Manually disable the indirect tracked pose driver entities and set the `isEngaged`
            // flag to `false`
            monoDrivers[0].Disable();
            monoDrivers[1].Disable();
            dualDriver.Disable();
            isEngaged = false;

            // If both grip buttons are currently held down then releasing one of them will just
            // one of the mono drivers to re-engage. This is undesirable as we would want to block
            // the manipulation attempt outright, and only attempt to re-engage when the user
            // releases both grips and presses them again. Hence a `isAllowedToEngage` must be
            // set which is released along with all grips.
            isAllowedToEngage = false;

            // Roll back the transform of the target system to the last known "good" value.
            var calibratedTransformation = targetTransformResource.Value;

            /* Developer's Notes;
             * This check is a holdover from the original code and the rationed given is as follows:
             * "This is necessary because the default value of multiplayer.SimulationPose is
             *  degenerate (0 scale) and there seems to be no way to tell if the remote value has
             *  been set yet or is default."
             */
            if (calibratedTransformation.Scale.x <= 0.001f)
                calibratedTransformation = new Transformation(Vector3.zero, Quaternion.identity, Vector3.one);

            // Convert from common user-agnostic "calibrated space" to client-side world space.
            // Note that this "world space" is not the same "world space" that Unity recognises.
            var worldTransformation = physicallyCalibratedSpace.TransformPoseCalibratedToWorld(calibratedTransformation);

            // Set the local position, orientation, and scale of the target `Transform` equal to
            // those within `worldTransformation`.
            worldTransformation.CopyToTransformRelativeToParent(targetTransform);
        }

        /// <summary>
        /// Cleans the supplied <c>transformation</c> entity to ensure its contents are sensible.
        /// </summary>
        /// <param name="transformation">The transformation entity to be cleaned.</param>
        /// <remarks>
        /// This is used to ensure the transform represents a valid and sensible state of the target
        /// system; i.e. the position is valid and not too far away from the user, etc.
        /// </remarks>
        private void ClampToSensibleValues(Transformation transformation)
        {
            Vector3 pos = transformation.Position;
            Vector3 scale = transformation.Scale;

            // Resolve situations in which the position has somehow become ill-defined.
            if (float.IsNaN(pos.x) || float.IsNaN(pos.y) || float.IsNaN(pos.z))
                transformation.Position = Vector3.zero;
            // Deal with cases where the system has moved unreasonably far away from the user
            else transformation.Position = Vector3.ClampMagnitude(pos, 100f);

            // Repeat this procedure for scale
            if (float.IsNaN(scale.x) || float.IsNaN(scale.y) || float.IsNaN(scale.z))
                transformation.Scale = Vector3.one;

            else transformation.Scale = new Vector3(
                Mathf.Clamp(scale.x, 0.001f, 1000f),
                Mathf.Clamp(scale.x, 0.001f, 1000f),
                Mathf.Clamp(scale.z, 0.001f, 1000f));
        }

        /// <summary>
        /// Tasked with processing state change requests triggered externally.
        /// </summary>
        /// <param name="currentState">State in which the handler is currently in.</param>
        /// <param name="newState">State into which the handler is to be moved.</param>
        private void PerformStateChange(State currentState, State newState)
        {
            // This is only ever triggered by external calls to set the State directly.
            switch ((currentState, newState))
            {
                case (State.Disabled, State.Active):
                    // Permit access to the controllers
                    Release(Controllers[0]);
                    Release(Controllers[1]);

                    // Ensure everything is active and enabled
                    enabled = true;
                    gameObject.SetActive(true);
                    state = newState;
                    break;

                case (State.Active, State.Disabled):
                    // Make sure any lingering subscriptions are purged
                    Restrict(Controllers[0]);
                    Restrict(Controllers[1]);
                    // Set the state
                    state = newState;
                    // Disable the entity.
                    enabled = false;
                    gameObject.SetActive(false);
                    break;
            }
        }
        
        /// <summary>
        /// Initialise the <c>TransformHybridInputHandler</c> instance ready for use.
        /// </summary>
        /// <remarks>This builds and sets up all of the required mechanisms.</remarks>
        public override void Initialise()
        {
            // Don't allow the initialisation operation to be performed more than once
            if (monoDrivers == null)
            {

                // Assign and initialise all of the `IndirectTrackedPoseDriver` instances
                monoDrivers = new IndirectTrackedPoseDriver[2];
                for (int i = 0; i < Controllers.Length; i++)
                {
                    monoDrivers[i] = new IndirectTrackedPoseDriver(
                        targetTransform,
                        Controllers[i].InputActionMap.FindAction("Position"),
                        Controllers[i].InputActionMap.FindAction("Rotation"));
                }

                // Repeat the process for the `DualIndirectTrackedPoseDriver` instance
                dualDriver = new DualIndirectTrackedPoseDriver(
                    targetTransform,
                    Controllers[0].InputActionMap.FindAction("Position"),
                    Controllers[1].InputActionMap.FindAction("Position"),
                    Controllers[0].InputActionMap.FindAction("Rotation"),
                    Controllers[1].InputActionMap.FindAction("Rotation"));

                // Create and configure an `InputActionDecoupler` instance. This is used to perform
                // three different mutually exclusive actions dependent on whether the first, second,
                // or both buttons are being held down.
                decoupler = new InputActionDecoupler(
                    Controllers[0].InputActionMap.FindAction(buttonName),
                    Controllers[1].InputActionMap.FindAction(buttonName)
                    );

                // Set up the positive action bindings first. These will cause a given transformer
                // to become active when its associated button, or combination thereof, are depressed.
                decoupler.A.performed += () => GatekeptEnableCallback(monoDrivers[0]);
                decoupler.B.performed += () => GatekeptEnableCallback(monoDrivers[1]);
                decoupler.AB.performed += () => GatekeptEnableCallback(dualDriver);

                // Now the negative bindings; which will stop a transformer when its associated button(s)
                // are no longer depressed.
                decoupler.A.canceled += monoDrivers[0].Disable;
                decoupler.B.canceled += monoDrivers[1].Disable;
                decoupler.AB.canceled += dualDriver.Disable;

                // Attempt to acquire an exclusive lock on the multiplayer resource representing the
                // system's transform when one of the two grip button is pressed. This lock prevents
                // race conditions by ensuring that only one agent can manipulate the shared resource
                // at a time. The lock is then released when the last grip button is released. Note
                // that because lock acquisition attempts are made asynchronously, the system should
                // proceed under the assumption that the lock request will granted. If the lock is
                // rejected then the `SimulationTransposeLockRejected` method will be invoked to
                // handle such an occurrence.
                decoupler.None.canceled += targetTransformResource.ObtainLock;
                decoupler.None.performed += ReleaseLockWithDelay;

                // Ensure the `isAllowedToEngage` flag is always cleared whenever all of the grip
                // buttons are released
                decoupler.None.performed += () => isAllowedToEngage = true;

                // This entity is considered to be `engaged` when at least one of the grip buttons
                // is depressed. However, if the lock request on the target system's transform
                // resource is rejected then this may change.
                decoupler.None.canceled += () => isEngaged = true;
                decoupler.None.performed += () => isEngaged = false;
            }
        }

        /// <summary>
        /// A gatekeeper is needed to prevent activating the tracking drivers when the
        /// resource lock has been rejected; meaning that tracking drivers are not
        /// allowed to be re-enabled until all buttons are released and pressed again. 
        /// </summary>
        /// <param name="driver">The indirect tracked pose driver that is to be conditionally
        /// enabled</param>
        private void GatekeptEnableCallback(IndirectTrackedPoseDriverType driver)
        {
            if (isAllowedToEngage) driver.Enable();
        }

        /// <summary>
        /// This wraps around the multiplayer resource lock release method to add a small delay
        /// of 100 ms before actually releasing the lock.
        /// </summary>
        /// <remarks>
        /// This is done to give the resource time to fully flush its cache before releasing its
        /// lock. If this is not done then other parts of the code might assume that the change
        /// was made remotely, as it was made with no locally active lock.
        /// </remarks>
        private async void ReleaseLockWithDelay()
        {
            await Task.Delay(100);
            targetTransformResource.ReleaseLock();
        }

        /// <summary>
        /// Inform the hybrid input handler that it may no longer act upon user inputs sourced
        /// from a specific controller.
        /// </summary>
        /// <param name="controller">Controller whose access is being restricted</param>
        /// <remarks>
        /// It is assume that the controller provided is one currently assigned to the handler.
        /// </remarks>
        public override void Restrict(InputController controller)
        {
            activelyBound[ControllerIndex(controller)] = false;
            decoupler.RestrictIndex(ControllerIndex(controller));

            // If all controllers are found to be unbound then transition to the disabled state
            if (activelyBound.All(x => !x) && (State == State.Active))
            {
                state = State.Disabled;
                gameObject.SetActive(false);
                enabled = false;
            }
        }

        /// <summary>
        /// Release the restriction placed upon the handler and allow it to once again act upon
        /// inputs sourced from the currently restricted controller.
        /// </summary>
        /// <param name="controller">Controller whose access restriction is being lifted</param>
        public override void Release(InputController controller)
        {
            decoupler.PermitIndex(ControllerIndex(controller));
            activelyBound[ControllerIndex(controller)] = true;

            // If the handler was "disabled" prior to this activation then set its state to active
            if (State != State.Active)
            {
                state = State.Active;
                gameObject.SetActive(true);
                enabled = true;
            }
        }
        
        public override void Background() => State = State.Disabled;

        /// <summary>
        /// Determines whether a given input controller is compatible with the input handler.
        /// </summary>
        /// <param name="controller">The input controller to check for compatibility.</param>
        /// <returns>True if the controller is compatible; otherwise, false.</returns>
        /// <remarks>`TransformHybridInputHandler` instances are compatible with all `BasicInputController`
        /// type classes.</remarks>
        public override bool IsCompatibleWithInputController(InputController controller) => controller is BasicInputController;

        /// <summary>
        /// Unbinds a controller from this input handler, preventing further input capture from
        /// the controller.
        /// </summary>
        /// <param name="controller">The controller to be unbound from this handler.</param>
        /// <remarks>
        /// This should only be used when wanting to replace the currently assigned controller with
        /// a new one. Setting the input handler's state to `Disabled` will instruct the handler
        /// to temporary ignore inputs provided by the controller.
        /// </remarks>
        public override void UnbindController(InputController controller) => throw new System.NotImplementedException();

        /// <summary>
        /// A list of <c>InputAction</c> entities specifying which input actions that the handler
        /// expects to be given sole binding privilege to.
        /// </summary>
        /// <returns>Hash set containing the input actions that this handler is expected to use.</returns>
        public override HashSet<InputAction> RequiredBindings()
        {
            // This input handler only binds to one button on each controller.
            return new HashSet<InputAction>
            {
                Controllers[0].InputActionMap.FindAction(buttonName),
                Controllers[1].InputActionMap.FindAction(buttonName)
            };
        }

        public void OnDisable()
        {
            // Conditional is needed here to prevent a recursion
            if (state != State.Disabled) State = State.Disabled;
            
        }

        void Awake()
        {
            gameObject.SetActive(false);
        }
    }

    public abstract class IndirectTrackedPoseDriverType
    {
        public abstract void Enable();
        public abstract void Disable();
        public abstract void Update();
    }




    /// <summary>
    /// A class that provides functionality for smoothed translation and rotation manipulation of a
    /// target object.
    /// </summary>
    /// <remarks>
    /// This class is used to rotate and translate objects with inputs from a controller, applying
    /// smoothing to the inputs to prevent small unintentional movements. Translational and angular
    /// movements are scaled linearly up to specified cut-off limits, followed by constant scaling.
    /// </remarks>
    public class IndirectTrackedPoseDriver: IndirectTrackedPoseDriverType
    {

        /// <summary>
        /// The <see cref="Transform"/> entity to which this driver is attached
        /// </summary>
        private Transform transform;

        /// <summary>
        /// The input action for the position source.
        /// </summary>
        private InputAction positionAction;

        /// <summary>
        /// The input action for the rotation source.
        /// </summary>
        private InputAction rotationAction;

        /// <summary>
        /// Position from the previous step.
        /// </summary>
        private Vector3 oldPosition = Vector3.one;

        /// <summary>
        /// Orientation from the previous step.
        /// </summary>
        private Quaternion oldOrientation = Quaternion.identity;

        /// <summary>
        /// Linear mixer to smooth out translational movements.
        /// </summary>
        public LinearVector3Scaler Vector3Scalar = new();

        /// <summary>
        /// Linear mixer to smooth out rotational movements.
        /// </summary>
        public LinearQuaternionScaler QuaternionScalar = new();

        /// <summary>
        /// Tracks current activity status used as a subscription tracker.
        /// </summary>
        private bool enabled = false;

        public IndirectTrackedPoseDriver(Transform transform, InputAction positionAction, InputAction rotationAction)
        {
            this.transform = transform;
            this.positionAction = positionAction;
            this.rotationAction = rotationAction;
        }

        /// <summary>
        /// Updates the target object's position and orientation based on the source's rotation and
        /// position.
        /// </summary>
        /// <remarks>
        /// This method applies scaled translation and rotation to the target object, smoothing out
        /// smaller movements and scaling larger movements differently. Rotations corresponding to
        /// small angular changes are ignored to prevent singularities.
        /// </remarks>
        public override void Update()
        {
            // Retrieve the position/orientation of the controller; using a mixer to smooth out its movement.
            Vector3 newPosition = Vector3Scalar.Scale(
                positionAction.ReadValue<Vector3>(), oldPosition);
            Quaternion newOrientation = QuaternionScalar.Scale(
                rotationAction.ReadValue<Quaternion>(), oldOrientation);

            // Work out the change in the controller's position and orientation since the last call
            // to this method was made. These variables will then be modified by the body of this
            // function so that they eventually represent expected changes in the target objects
            // position and orientation.
            Vector3 deltaPosition = newPosition - oldPosition;
            Quaternion deltaRotation = newOrientation * Quaternion.Inverse(oldOrientation);

            // Rotations corresponding to small angular changes must be ignored to prevent the emergence
            // of singularities.
            if (Quaternion.Angle(Quaternion.identity, deltaRotation) < 1E-6F)
            {
                // then the rotational change must be ignored to prevent the emergence of singularities.
                deltaRotation = Quaternion.identity;  // <- pretend rotation is zero
                newOrientation = oldOrientation; // <- retcon the "new orientation" to avoid error accumulation
            }

            // Current position and orientation of the controller are stored for the next update.
            oldOrientation = newOrientation;
            oldPosition = newPosition;

            // Apply the change in position 
            transform.position = deltaPosition + transform.position;

            // Rotate the target about the controller
            deltaRotation.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);
            transform.RotateAround(newPosition, rotationAxis, angleInDegrees);
        }

        /// <summary>
        /// Activate the <c>IndirectTrackedPoseDriver</c> instance.
        /// </summary>
        public override void Enable()
        {
            // Set up a subscription to invoke the `Update` method every time the controller's
            // position is changed. Note that an explicit subscription to controller rotation is
            // not necessary as there will be enough jitter in controller rotation to trigger a
            // position update event.
            if (!enabled) positionAction.performed += UpdateWrapper;

            enabled = true;
            oldOrientation = rotationAction.ReadValue<Quaternion>();
            oldPosition = positionAction.ReadValue<Vector3>();
        }

        /// <summary>
        /// Deactivate the <c>IndirectTrackedPoseDriver</c> instance.
        /// </summary>
        public override void Disable()
        {
            if (enabled) positionAction.performed -= UpdateWrapper;
            enabled = false;
        }

        /// <summary>
        /// A wrapper that redirects input action events to the <c>Update</c> method.
        /// </summary>
        /// <param name="context">The callback context from the input action event.</param>
        private void UpdateWrapper(InputAction.CallbackContext context) => Update();

        /// <summary>
        /// Wrapper for the <c>Enable</c> method that allow input action button press events to
        /// enable the driver.
        /// </summary>
        /// <param name="context">Callback context from the input action button event.</param>
        public void Enable(InputAction.CallbackContext context) => Enable();

        /// <summary>
        /// Wrapper for the <c>Enable</c> method that allow input action button release events to
        /// enable the driver.
        /// </summary>
        /// <param name="context">Callback context from the input action button event.</param>
        public void Disable(InputAction.CallbackContext context) => Disable();
    }



    /// <summary>
    /// A class that provides functionality for smoothed translation, rotation, and scaling based
    /// manipulation of a target object.
    /// </summary>
    /// <remarks>
    /// This class is used to rotate and translate objects with inputs from a controller, applying
    /// smoothing to the inputs to prevent small unintentional movements. Translational and angular
    /// movements are scaled linearly up to specified cut-off limits, followed by constant scaling.
    /// </remarks>
    public class DualIndirectTrackedPoseDriver: IndirectTrackedPoseDriverType
    {

        /// <summary>
        /// The <see cref="Transform"/> entity to which this driver is attached
        /// </summary>
        private Transform transform;

        /// <summary>
        /// The input action position source for the first controller.
        /// </summary>
        private InputAction positionActionA;

        /// <summary>
        /// The input action position source for the second controller.
        /// </summary>
        private InputAction positionActionB;

        /// <summary>
        /// The input action rotation source for the first controller.
        /// </summary>
        private InputAction rotationActionA;

        /// <summary>
        /// The input action rotation source for the first controller.
        /// </summary>
        private InputAction rotationActionB;

        /// <summary>
        /// Midpoint between the two position sources from the previous step.
        /// </summary>
        private Vector3 oldMidpoint = Vector3.one;

        /// <summary>
        /// Distance vector between the two position sources from the previous step.
        /// </summary>
        private Vector3 oldDistanceVector;

        /// <summary>
        /// Value of the first orientation source from the previous step.
        /// </summary>
        private Quaternion oldOrientationA = Quaternion.identity;

        /// <summary>
        /// Value of the first orientation source from the previous step.
        /// </summary>
        private Quaternion oldOrientationB = Quaternion.identity;

        /// <summary>
        /// Tracks current activity status used as a subscription tracker.
        /// </summary>
        private bool enabled = false;

        /// <summary>
        /// Linear mixer to smooth out translational movements.
        /// </summary>
        public LinearVector3Scaler Vector3Scaler = new();

        /// <summary>
        /// Linear mixer to smooth out rotational movements for the first orientation source.
        /// </summary>
        public LinearQuaternionScaler QuaternionScalarA = new();

        /// <summary>
        /// Linear mixer to smooth out rotational movements for the first orientation source.
        /// </summary>
        public LinearQuaternionScaler QuaternionScalarB = new();


        public DualIndirectTrackedPoseDriver(Transform transform, InputAction positionActionA, InputAction positionActionB, InputAction rotationActionA, InputAction rotationActionB)
        {
            this.transform = transform;
            this.positionActionA = positionActionA;
            this.positionActionB = positionActionB;
            this.rotationActionA = rotationActionA;
            this.rotationActionB = rotationActionB;

        }


        /// <summary>
        /// Updates the target object's position, orientation, and scale based on the positions of
        /// two source points.
        /// </summary>
        /// <remarks>
        /// A unique reference vector, orthogonal to the up vector and the current distance vector
        /// between source points, is constructed to calculate the new orientation. The distance
        /// between the two source points is measured, and the object is scaled relative to the
        /// change in distance since the last update. The midpoint between the two source positions
        /// is identified, and the position and orientation are updated with rotational scaling disabled.
        /// The distance and previous difference between source points are stored for use in subsequent
        /// updates.
        /// </remarks>
        public override void Update()
        {

            // Retrieve the positions of the controllers. Note that the controller positions are not
            // smoothed directly, but rather the midpoint between them is.
            Vector3 sourcePositionA = positionActionA.ReadValue<Vector3>();
            Vector3 sourcePositionB = positionActionB.ReadValue<Vector3>();

            // Difference between the two source positions is calculated
            Vector3 difference = sourcePositionB - sourcePositionA;

            // Axis along which secondary rotation should take place (performed later)
            Vector3 axis = difference.normalized;

            // Midpoint between the two controllers is identified
            Vector3 midpoint = (sourcePositionA + sourcePositionB) / 2;

            // Smooth out the motion of the midpoint
            midpoint = Vector3Scaler.Scale(midpoint, oldMidpoint);

            // Update the transform's position to match the relative movement of the controllers
            transform.position += midpoint - oldMidpoint;

            // Identify scale by which the distance between the two controllers has changed
            float deltaScale = difference.magnitude / oldDistanceVector.magnitude;

            // Scaling below 0.005 is blocked to prevent inversions or singularities.
            if (Mathf.Abs(transform.localScale.x) >= 0.005 || deltaScale > 1f)
                ScaleAboutPoint(transform, deltaScale, midpoint);

            // Perform the primary rotation which ensures that the orientation of the target transform
            // relative to the always remains constant; i.e. this the rotation applied when the two
            // controllers are moved in a "steering wheel" like gesture.
            Quaternion deltaRotation = Quaternion.FromToRotation(oldDistanceVector.normalized, difference.normalized);

            // Very small rotation events are skipped over to prevent introducing noise.
            if (Quaternion.Angle(Quaternion.identity, deltaRotation) > 1E-6F)
            {
                deltaRotation.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);
                transform.RotateAround(midpoint, rotationAxis, angleInDegrees);
            }

            // Apply the secondary rotation event which is responsible for rotations about the axis
            // formed between the two controllers. This is the rotation applied when the orientation
            // of the controllers are changed.

            // Get the new controller orientations and apply scaling to smooth out movements and shake
            Quaternion orientationA = QuaternionScalarA.Scale(
                rotationActionA.ReadValue<Quaternion>(), oldOrientationA);
            Quaternion orientationB = QuaternionScalarB.Scale(
                rotationActionB.ReadValue<Quaternion>(), oldOrientationB);

            // Work out the average change in orientation of the two controllers.
            Quaternion relativeRotation = Quaternion.Slerp(
                Quaternion.Inverse(oldOrientationA * Quaternion.Inverse(orientationA)),
                Quaternion.Inverse(oldOrientationB * Quaternion.Inverse(orientationB)),
                0.5f);

            // Perform a swing twist decomposition on the rotational delta value to extract the
            // component representing rotation about the target axis. 
            var p = Vector3.Dot(new Vector3(relativeRotation.x, relativeRotation.y, relativeRotation.z), axis) * axis;
            var twist = new Quaternion(p.x, p.y, p.z, relativeRotation.w).normalized;

            // Only apply the rotation if the signal to noise ratio is high enough
            if (Quaternion.Angle(Quaternion.identity, twist) > 1E-6F)
            {
                twist.ToAngleAxis(out float angle, out Vector3 rotationAxis);
                transform.RotateAround(midpoint, rotationAxis, angle);

                // The old rotations are only updated when and if the rotation is applied. This means
                // that lots of small rotation events may accumulate over time to improve the signal
                // to noise ratio.
                oldOrientationA = orientationA;
                oldOrientationB = orientationB;
            }

            // Distance and PreviousDifference are stored for use in the next loop.
            oldDistanceVector = difference;
            oldMidpoint = midpoint;
        }


        /// <summary>
        /// Scale a transform by a specified degree about a given position.
        /// </summary>
        /// <param name="transform">The transform to be scaled.</param>
        /// <param name="scale">The fractional degree to which it is to be scaled.</param>
        /// <param name="origin">The location in world space about which the scaling is to take place.</param>
        /// <remarks>
        /// This allows an object to be scaled but have its position relative to some arbitrary location
        /// remain unchanged; as opposed to the origin of the object being fixed.
        /// </remarks>
        private static void ScaleAboutPoint(Transform transform, float scale, Vector3 origin)
        {
            // Transform the origin point from global to local space.
            Vector3 newPosLocal = transform.InverseTransformPoint(origin);

            // Object scaling relative to the change in distance is performed.
            transform.localScale *= scale;

            // Apply an offset so that the object seems to scale about the grasp point.
            // This just stops the target object from moving around relative the grasp point
            // as it is scaled.
            transform.position -= (transform.TransformPoint(newPosLocal) - origin);
        }


        /// <summary>
        /// Activate the <c>DualIndirectTrackedPoseDriver</c> instance.
        /// </summary>
        public override void Enable()
        {
            if (!enabled)
            {
                // Set up event subscriptions
                positionActionA.performed += UpdateWrapper;
                positionActionB.performed += UpdateWrapper;
                rotationActionA.performed += UpdateWrapper;
                rotationActionB.performed += UpdateWrapper;

                // Pre initialise "old values" this means that no special treatment is needed in the
                // `Update` method during the first call.
                oldOrientationA = rotationActionA.ReadValue<Quaternion>();
                oldOrientationB = rotationActionB.ReadValue<Quaternion>();
                oldMidpoint = (positionActionA.ReadValue<Vector3>() + positionActionB.ReadValue<Vector3>()) / 2;
                oldDistanceVector = positionActionB.ReadValue<Vector3>() - positionActionA.ReadValue<Vector3>();
                enabled = true;
            }

        }

        /// <summary>
        /// Deactivate the <c>DualIndirectTrackedPoseDriver</c> instance.
        /// </summary>
        public override void Disable()
        {
            if (enabled)
            {
                // tear down event subscriptions
                positionActionA.performed -= UpdateWrapper;
                positionActionB.performed -= UpdateWrapper;
                rotationActionA.performed -= UpdateWrapper;
                rotationActionB.performed -= UpdateWrapper;
                enabled = false;

            }

        }

        /// <summary>
        /// A wrapper that redirects input action events to the <c>Update</c> method.
        /// </summary>
        /// <param name="context">The callback context from the input action event.</param>
        private void UpdateWrapper(InputAction.CallbackContext context) => Update();

        /// <summary>
        /// Wrapper for the <c>Enable</c> method that allow input action button press events to
        /// enable the driver.
        /// </summary>
        /// <param name="context">Callback context from the input action button event.</param>
        public void Enable(InputAction.CallbackContext context) => Enable();

        /// <summary>
        /// Wrapper for the <c>Enable</c> method that allow input action button release events to
        /// enable the driver.
        /// </summary>
        /// <param name="context">Callback context from the input action button event.</param>
        public void Disable(InputAction.CallbackContext context) => Disable();

    }
    

    /// <summary>
    /// Simple linear mixer for smoothing out translational movements.
    /// </summary>
    public class LinearVector3Scaler
    {
        /// <summary>
        /// Upper scaling limit for translational movement.
        /// </summary>
        /// <value>Represents the maximum scaling factor applied to translational movement, with a default value of 0.5.</value>
        public float UpperScalingLimit = 0.5F;

        /// <summary>
        /// Lower scaling limit for translational movement.
        /// </summary>
        /// <value>Represents the minimum scaling factor applied to translational movement, with a default value of 0.01.</value>
        public float LowerScalingLimit = 0.01F;

        /// <summary>
        /// Cut-off distance for translational movement scaling.
        /// </summary>
        /// <value>Represents the distance beyond which the upper scaling limit is applied, with a default value of 0.005.</value>
        public float cutoff = 0.005F;

        /*
         *   cutoff: 0.005
         *     ↓
         *     ______ ← UpperScalingLimit: 0.5
         *    ╱
         *   ╱
         *  ╱
         * ╱
         *╱           ← LowerScalingLimit: 0.01
         */

        /// <summary>
        /// Scale the position.
        /// </summary>
        /// <param name="newPosition">New target position</param>
        /// <param name="oldPosition">Old reference position</param>
        /// <returns>Smoothed position, which lies somewhere between the new and old position.</returns>
        public Vector3 Scale(Vector3 newPosition, Vector3 oldPosition)
        {

            float distance = (oldPosition - newPosition).magnitude;
            float scaleFactor;

            if (distance <= cutoff)
            {
                // Scaling factor is linearly interpolated for distances less than cutoff.
                scaleFactor = Mathf.Lerp(
                    LowerScalingLimit, UpperScalingLimit,
                    distance / cutoff);
            }
            else
            {
                // Scaling factor is set to UpperScalingLimit for distances greater than cutoff.
                scaleFactor = UpperScalingLimit;
            }

            return Vector3.Lerp(oldPosition, newPosition, scaleFactor);
        }
    }


    /// <summary>
    /// Simple linear mixer for smoothing out rotational movements.
    /// </summary>
    public class LinearQuaternionScaler
    {
        /// <summary>
        /// Upper scaling limit for angular movement.
        /// </summary>
        /// <value>Represents the maximum scaling factor applied to angular movement, with a default value of 0.4.</value>
        public float UpperScalingLimit = 0.4F;

        /// <summary>
        /// Lower scaling limit for angular movement.
        /// </summary>
        /// <value>Represents the minimum scaling factor applied to angular movement, with a default value of 0.01.</value>
        public float LowerScalingLimit = 0.01F;

        /// <summary>
        /// Cut-off angle for angular movement scaling.
        /// </summary>
        /// <value>Represents the angle in degrees beyond which the upper scaling limit is applied, with a default value of 5.0.</value>
        public float Cutoff = 5.0F;

        /// <summary>
        /// Scale the rotation.
        /// </summary>
        /// <param name="newRotation">New target orientation</param>
        /// <param name="oldRotation">Old reference orientation</param>
        /// <returns>Smoothed orientation, which lies somewhere between the new and old orientations.</returns>
        public Quaternion Scale(Quaternion newRotation, Quaternion oldRotation)
        {
            float angle = Quaternion.Angle(oldRotation, newRotation);
            float scaleFactor;

            if (angle <= Cutoff)
            {
                // Scaling factor is linearly interpolated for angles less than Cutoff.
                scaleFactor = Mathf.Lerp(
                    LowerScalingLimit, UpperScalingLimit,
                    angle / Cutoff);
            }
            else
            {
                // Scaling factor is set to UpperScalingLimit for angles greater than Cutoff.
                scaleFactor = UpperScalingLimit;
            }

            return Quaternion.Slerp(oldRotation, newRotation, scaleFactor);
        }

    }



}



