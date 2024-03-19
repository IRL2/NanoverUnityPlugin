using System.Collections.Generic;
using Nanover.Grpc.Trajectory;
using Nanover.Visualisation;
using UnityEngine;
using UnityEngine.InputSystem;
using static Nanover.Frontend.InputControlSystem.Utilities.InputControlUtilities;

namespace Nanover.Frontend.InputControlSystem.InputHandlers
{
    /// <summary>
    /// This abstract base class is tasked with holding all of the base functionality required to
    /// select an atom, or group thereof, from a target system.
    /// </summary>
    /// <remarks>
    /// Selection operations will become more involved over time and thus it is logical to create
    /// a base input handler class that offers all such functionality in one place.
    /// </remarks>
    public abstract class SelectorMonoInputInputHandler : MonoInputHandler, ISystemDependentInputHandler, ITrajectorySessionDependentInputHandler
    {

        /// <summary>
        /// Current state of the handler.
        /// </summary>
        /// <remarks>
        /// This will default to <c>Disabled</c> when initialised.
        /// </remarks>
        public override State State
        {
            get => state;
            set => PerformStateChange(state, value);
        }

        /// <summary>
        /// Internal boolean field used to keep track of whether controller events have been
        /// subscribed to or not.
        /// </summary>
        protected bool isSubscribed = false;


        /// <summary>
        /// Internal boolean field that signals when the input handler is engaged. The handler is
        /// considered to be "engaged" when the trigger button is depressed.
        /// </summary>
        protected bool isEngaged = false;

        /// <summary>
        /// Stores index of the atom that was closest to the controller when it was first engaged.
        /// </summary>
        /// <remarks>
        /// This will default to a value of <c>-1</c> when the handler is not engaged or in situations
        /// where no closest atom is found.
        /// </remarks>
        protected int closestAtomIndex = -1;


        /// <summary>
        /// The button which, when pressed, triggers an engagement.
        /// </summary>
        /// <remarks>
        /// The name "trigger" is used only in the most general scene here; i.e. it does not have
        /// to be the actual trigger button on the controller. This is to be hard-coded by each
        /// class implementation.
        /// </remarks>
        protected string triggerButtonName = "Trigger";


        /// <summary>
        /// Game object to which the simulation is attached.
        /// </summary>
        /// <remarks>
        /// This is required by the handler to translate the global coordinates of the controller
        /// into the local coordinates of the simulation object.
        /// </remarks>
        protected GameObject simulationGameObject;


        /// <summary>
        /// Object from which frame data can be sourced.
        /// </summary>
        /// <remarks>
        /// This will likely be replaced once a unified interface structure for geometric systems
        /// has been implemented.
        /// </remarks>
        protected SynchronisedFrameSource frameSynchroniser;

        /// <summary>
        /// Specify the system which the input handler is responsible for.
        /// </summary>
        /// <param name="systemObject">Game object representing the target system</param>
        public void SetSystem(GameObject systemObject) => simulationGameObject = systemObject;

        /// <summary>
        /// Set the required trajectory session.
        /// </summary>
        /// <param name="trajectory">The required trajectory session</param>
        public void SetTrajectorySession(TrajectorySession trajectory)
        {
            // Create the frame synchroniser component.  
            frameSynchroniser = gameObject.AddComponent<SynchronisedFrameSource>();
            frameSynchroniser.FrameSource = trajectory;
        }

        /// <summary>
        /// Returns an array storing the positions of the atoms.
        /// </summary>
        /// <returns>Array of vectors representing the positions of the atoms.</returns>
        protected Vector3[] GetPositions()
        {

            // This is mostly just a convenience function to avoid having to perform a safety check
            // each and every place that the position array is needed.
            var frame = frameSynchroniser.CurrentFrame;
            if (frame != null && frame.ParticlePositions != null)
                return frame.ParticlePositions;
            else
                return null;
        }

        /// <summary>
        /// A callback for executing the necessary procedures at the initiation and termination stages
        /// of an engagement. This is commonly intended to be triggered by explicit user inputs via a
        /// button press event.
        /// </summary>
        /// <param name="context">The context of the input action, which determines the phase of
        /// engagement.</param>
        /// <remarks>
        /// For the sake of flexibility, this will just redirect to the <c>Engage</c> and <c>Disengage</c>
        /// methods.
        /// </remarks>
        protected void UpdateEngagement(InputAction.CallbackContext context)
        {
            // An input action being performed indicates the start of an engagement
            if (context.phase == InputActionPhase.Performed)
                Engage();
            // The same input action being cancelled indicates disengagement.
            else if (context.phase == InputActionPhase.Canceled)
                Disengage();
        }

        /// <summary>
        /// A callback for initiating engagement. The closest atom to the controller's cursor position
        /// is identified, and the handler is marked as engaged. This method typically responds to a 
        /// specific user input, signifying the beginning of an interaction.
        /// </summary>
        /// <remarks>
        /// The closest atom index is updated, and the 'isEngaged' flag is set to true, indicating that
        /// the handler is actively engaged. This method may be overridden as required.
        /// </remarks>
        protected virtual void Engage()
        {
            // Find the closest atom to the controller's cursor position and assign the index
            // of that atom to the `closestAtomIndex` field.
            closestAtomIndex = ClosestAtomicIndex(Controller.transform.position);
            // Signal that the handler is now engaged. This is used to indicate to other parts
            // of the handler that they should now act.
            isEngaged = true;
            // Set state lock to prevent arbiters form disabling the input handler mid-interaction
            Locked = true;
        }

        /// <summary>
        /// A callback for terminating engagement. This method is invoked typically when a user input
        /// signifies the end of an interaction, leading to the disengagement of the handler.
        /// </summary>
        /// <remarks>
        /// The handler is marked as no longer engaged, and the 'closestAtomIndex' is reset, reflecting
        /// the cessation of the active interaction. This method may be overridden as required.
        /// </remarks>
        protected virtual void Disengage()
        {
            // Handler is no longer engaged.
            isEngaged = false;
            // And thus may not have a closest atom.
            closestAtomIndex = -1;
            // Release the state lock
            Locked = false;
        }


        void Update()
        {
            // If the handler is engaged and a closest atom has been identified
            if (isEngaged && closestAtomIndex != -1)
            {
                // Then do something ...
            }
        }


        /// <summary>
        /// Perform state change event.
        ///
        /// This method handles transitions between the various different states. This is intended
        /// as a minimum working example, and should therefore be overridden locally with a more
        /// appropriate method.
        /// </summary>
        /// <param name="currentState">Current state in which the handler exists.</param>
        /// <param name="newState">New state to which the handler should transition.</param>
        protected virtual void PerformStateChange(State currentState, State newState)
        {
            
            switch ((currentState, newState))
            {

                case (State.Disabled, State.Active):
                    // Subscribe to controller events if transitioning from a `Disabled` state to
                    // an `Active` state.
                    Subscribe();
                    break;
                case (State.Active, State.Disabled):
                    // Abort any active engagements and unsubscribe from controller events if
                    // transitioning from an `Active`  state to a `Disabled` state.
                    Unsubscribe();
                    if (isEngaged)
                        Disengage();
                    break;
                case (State.Paused, State.Active):
                    // Treat resuming from a pause the same as `Disabled` -> `Active`
                    PerformStateChange(State.Disabled, newState);
                    break;
                case (State.Active, State.Paused):
                    // Treat pausing the same as `Active` -> `Disabled`
                    PerformStateChange(currentState, State.Disabled);
                    break;
                default:
                    break;
            }

            // Now that all necessary state change event actions have been performed the state can
            // be safely updated.
            state = newState;
        }
        
        /// <summary>
        /// Subscribe to requisite events.
        /// </summary>
        /// <remarks>
        /// This is commonly called when the handler becomes active.
        /// </remarks>
        protected virtual void Subscribe()
        {
            // Prevent redundant subscriptions
            if (!isSubscribed)
            {
                // Subscribe the UpdateEngagement method to events trigger button events.
                InputAction action = Controller.InputActionMap.FindAction(triggerButtonName);
                action.performed += UpdateEngagement;
                action.canceled += UpdateEngagement;

                // Perform any ancillary subscriptions
                SubscribeAncillary();

                isSubscribed = true;
            }
        }

        /// <summary>
        /// Subscribe to any additional events.
        /// </summary>
        /// <remarks>
        /// Sub-classes that need to subscribe to additional events not present in the <c>Subscribe</c>
        /// method can do so here. This saves having to copy over the entirety of the <c>Subscribe</c>
        /// method just to add one or two extra lines.
        /// </remarks>
        protected virtual void SubscribeAncillary(){}

        /// <summary>
        /// Unsubscribe from all currently subscribed events.
        /// </summary>
        /// <remarks>
        /// This is commonly called when the handler gets disabled.
        /// </remarks>
        protected virtual void Unsubscribe()
        {
            // Only unsubscribe if currently subscribed
            if (isSubscribed)
            {
                // Unsubscribe from the trigger button action events
                InputAction action = Controller.InputActionMap.FindAction(triggerButtonName);
                action.performed -= UpdateEngagement;
                action.canceled -= UpdateEngagement;

                // Unsubscribe from any ancillary subscriptions
                UnsubscribeAncillary();

                isSubscribed = false;
            }
        }


        /// <summary>
        /// Unsubscribe from any additional events.
        /// </summary>
        /// <remarks>
        /// Sub-classes that make use of the <c>SubscribeAncillary</c> method can use this method to
        /// unsubscribe from those additional events.
        /// </remarks>
        protected virtual void UnsubscribeAncillary(){}

        /// <summary>
        /// A list of <c>InputAction</c> entities specifying which input actions that the handler
        /// expects to be given sole binding privilege to.
        /// </summary>
        /// <returns>Hash set containing the input actions that this handler is expected to use.</returns>
        public override HashSet<InputAction> RequiredBindings()
        {
            return new HashSet<InputAction> { Controller.InputActionMap.FindAction(triggerButtonName) };
        }


        /// <summary>
        /// Return the atomic index of the closest atom to some position in world space. Commonly
        /// this is the position to the controller's cursor.
        /// </summary>
        /// <param name="position">Position, in world space, to which the closest atom should be
        /// found</param>
        /// <returns>Index of the atom that is found to be closes to the specified position. Note
        /// that a value of -1 is returned if no such atom can be identified.</returns>
        /// <remarks>
        /// Currently this performs a brut force squared distance search which is not exactly the
        /// most efficient nearest neighbour search algorithm. Such tasks will be abstracted or
        /// handed off to external dedicated libraries in the future.
        /// </remarks>
        protected int ClosestAtomicIndex(Vector3 position)
        {
            // Convert the coordinates from world space to the local space coordinates of the game
            // object representing the simulation.
            Vector3 cursorPosition = simulationGameObject.transform.InverseTransformPoint(position);

            // Fetch the frame object
            var frame = frameSynchroniser.CurrentFrame;

            Vector3[] positions = GetPositions();

            // Only attempt to identify the closest atom if there atoms to be found
            if ((positions != null) && (positions.Length != 0))
            {
                float minimumSquaredDistance = 99999f;
                int closestAtomIndexLocal = -1;
                float squaredDistance;

                // Loop over each of the atomic positions
                for (int i = 0; i < positions.Length; i++)
                {
                    // Calculate the squared distance between the controller and the atom
                    squaredDistance = Vector3.SqrMagnitude(cursorPosition - positions[i]);

                    // Check if the newly calculated distance is less than the current minimum value
                    if (squaredDistance < minimumSquaredDistance)
                    {
                        // If so, then update the minimum squared distance and closest atom index variables
                        minimumSquaredDistance = squaredDistance;
                        closestAtomIndexLocal = i;
                    }
                }

                return closestAtomIndexLocal;
            }
            else
                return -1;
        }

        /// <summary>
        /// Ensure inputs are unsubscribed from during tear-down stage.
        /// </summary>
        public void OnDestroy() => Unsubscribe();
    }
}