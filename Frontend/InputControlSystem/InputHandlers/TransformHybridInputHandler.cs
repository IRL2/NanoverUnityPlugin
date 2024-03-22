using System;
using System.Collections.Generic;
using Nanover.Frontend.InputControlSystem.InputControllers;
using Nanover.Frontend.InputControlSystem.Utilities;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using static Nanover.Frontend.InputControlSystem.Utilities.InputControlUtilities;

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

    public class TransformHybridInputHandler: HybridInputHandler, ISystemDependentInputHandler, IUserSelectableInputHandler
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
        /// Represents the transform of the target object which is to be manipulated. This transform
        /// is used as the focal point for all translation, rotation, and scaling operations performed
        /// by the input handler.
        /// </summary>
        private Transform targetTransform;

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
        /// Set the system to be manipulated.
        /// </summary>
        /// <param name="systemObject">The game object of the system that is to be transformed by
        /// this handler.</param>
        public void SetSystem(GameObject systemObject) => targetTransform = systemObject.transform;
        
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
                    Controllers[1].InputActionMap.FindAction("Position"));

                // Create and configure an `InputActionDecoupler` instance. This is used to perform
                // three different mutually exclusive actions dependent on whether the first, second,
                // or both buttons are being held down.
                decoupler = new InputActionDecoupler(
                    Controllers[0].InputActionMap.FindAction(buttonName),
                    Controllers[1].InputActionMap.FindAction(buttonName)
                    );

                // Set up the positive action bindings first. These will cause a given transformer
                // to become active when its associated button, or combination thereof, are depressed.
                decoupler.A.performed += monoDrivers[0].Enable;
                decoupler.B.performed += monoDrivers[1].Enable;
                decoupler.AB.performed += dualDriver.Enable;

                // Now the negative bindings; which will stop a transformer when its associated button(s)
                // are no longer depressed.
                decoupler.A.canceled += monoDrivers[0].Disable;
                decoupler.B.canceled += monoDrivers[1].Disable;
                decoupler.AB.canceled += dualDriver.Disable;
            }
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
            if (state != State.Disabled)
            {
                State = State.Disabled;
            }
        }

        void Awake()
        {
            gameObject.SetActive(false);
        }
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
    public class IndirectTrackedPoseDriver
    {

        /// <summary>
        /// The <see cref="Transform"/> entity to which this driver is attached
        /// </summary>
        private Transform transform;


        /// <summary>
        /// The input action position source.
        /// </summary>
        private InputAction positionAction;

        /// <summary>
        /// The input action position source.
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
        public void Update()
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
            
            // Account for the change in position caused by rotating the target about the controller.
            deltaPosition = (deltaRotation * (transform.position - newPosition + deltaPosition)) + newPosition - transform.position;

            // Current position and orientation of the controller are stored for the next update.
            oldOrientation = newOrientation;
            oldPosition = newPosition;

            // Apply the position and rotation transformations
            transform.position = deltaPosition + transform.position;
            transform.rotation = deltaRotation * transform.rotation;
            
        }

        /// <summary>
        /// Activate the <c>IndirectTrackedPoseDriver</c> instance.
        /// </summary>
        public void Enable()
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
        public void Disable()
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
    public class DualIndirectTrackedPoseDriver
    {
        /* Developer's Notes:
         * This code is very much a work in progress and must not be committed to the master branch
         * until it has undergone a substantial cleanup. There are things done here that are overly
         * verbose and repetitive.
         *
         * Currently this ignores rotations of the controllers which is unnatural. This should be
         * introduced after the initial cleanup has been performed.
         *
         * At some point it would be worth abstracting this into a separate module. However, this task
         * can be deferred until it is needed. Furthermore, it would likely be beneficial to allow for
         * controller orientation to be taken into account during dual input translation.
         */

        private InputAction posActionA;
        private InputAction posActionB;

        private Transform transform;

        // Previous distance source positions (used when scaling)
        private float OldDist = 0;
        // Previous difference vector (used when scaling)
        private Vector3 oldDiff = Vector3.one;

        // Previous orientation of the source is stored.
        private Quaternion oldRot = Quaternion.identity;

        // Previous position of the source is stored.
        private Vector3 oldPos = Vector3.one;

        private bool enabled = false;

        public LinearVector3Scaler Vector3Scaler = new();


        public DualIndirectTrackedPoseDriver(Transform transform, InputAction positionActionA, InputAction positionActionB)
        {
            this.transform = transform;
            posActionA = positionActionA;
            posActionB = positionActionB;
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
        public void Update()
        {
            Vector3 sourcePositionA = posActionA.ReadValue<Vector3>();
            Vector3 sourcePositionB = posActionB.ReadValue<Vector3>();

            // Difference between the two source positions is calculated.
            Vector3 difference = sourcePositionB - sourcePositionA;

            // Construct a reference vector that is orthogonal to both the up vector and the current
            // distance vector. This vector is used to calculate the new orientation. Although it is
            // possible to simply use Vector3.up ([0, 1, 0]) as a reference, this can lead to problems,
            // especially when the delta vector approaches the reference vector, making the rotational
            // behaviour ill-defined. To overcome this, the reference vector is constructed uniquely
            // for each instance, ensuring that it is never ill-posed with respect to the delta vector.
            // An alternative approach might involve using the dot product to identify when the reference
            // vector is ill-posed and then selecting a new one. While potentially more efficient than
            // generating a unique reference vector every time, this approach can lead to noticeable visual
            // artefacts. Therefore, a new "UpDirection" is generated at each step to avoid these issues
            // and ensure consistent orientation behaviour.
            Vector3 forwardDirection = difference.normalized;
            Vector3 rightDirection = Vector3.Cross(forwardDirection, Vector3.up).normalized;
            Vector3 upDirection = Vector3.Cross(rightDirection, forwardDirection).normalized;

            // Orientation of the delta vector is computed.
            Quaternion newRot = Quaternion.LookRotation(forwardDirection, upDirection);

            // Previous orientation must be is changed retroactively if a different reference vector
            // is used. Which is every step at the moment.
            if (enabled)
                oldRot = Quaternion.LookRotation(oldDiff, upDirection);


            // Midpoint between the two controllers is identified
            Vector3 newPos = (sourcePositionA + sourcePositionB) / 2;

            // Distance between the two points is calculated.
            float distance = difference.magnitude;


            if (enabled)
            {

                float deltaScale = distance / OldDist;

                // Scaling below 0.005 is blocked to prevent inversions or singularities.
                if (transform.localScale.x >= 0.005 || deltaScale > 1f)
                {

                    // Grasp point (newPos) in local coordinates of the transform object 
                    Vector3 newPosLocal = transform.InverseTransformPoint(newPos);


                    // Object scaling relative to the change in distance is performed.
                    transform.localScale *= deltaScale;

                    // Apply an offset so that the object seems to scale about the grasp point.
                    // This just stops the target object from moving around relative the grasp point
                    // as it is scaled.
                    transform.position -= (transform.TransformPoint(newPosLocal) - newPos);
                }
            }




            // The rest of this function is associated primarily with updating the orientation and
            // position. This progresses similar to IndirectTrackedPoseDriver 
            if (enabled)
            {

                newPos = Vector3Scaler.Scale(newPos, oldPos);
                Quaternion deltaRot = newRot * Quaternion.Inverse(oldRot);
                Vector3 deltaPos = newPos - oldPos;

                if (Quaternion.Angle(Quaternion.identity, deltaRot) < 1E-6F)
                {

                    transform.position += deltaPos;
                    oldPos = newPos;

                }
                else
                {
                    Vector3 vectorToObject = transform.position + deltaPos - newPos;
                    Vector3 rotatedToObject = deltaRot * vectorToObject;
                    transform.SetPositionAndRotation(newPos + rotatedToObject,
                        deltaRot * transform.rotation);
                }
            }
            else
            {
                enabled = true;
            }


            // Distance and PreviousDifference are stored for use in the next loop.
            OldDist = distance;
            oldDiff = forwardDirection;
            oldRot = newRot;
            oldPos = newPos;
        }

        public void Enable()
        {
            if (!enabled)
            {
                posActionA.performed += UpdateWrapper;
                posActionB.performed += UpdateWrapper;
            }

        }

        public void Disable()
        {
            if (enabled)
            {
                posActionA.performed -= UpdateWrapper;
                posActionB.performed -= UpdateWrapper;
                enabled = false;
                oldRot = Quaternion.identity;
                oldPos = Vector3.one;
            }

        }

        private void UpdateWrapper(InputAction.CallbackContext context) => Update();
        public void Enable(InputAction.CallbackContext context) => Enable();
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



