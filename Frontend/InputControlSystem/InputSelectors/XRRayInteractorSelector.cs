using System.Collections.Generic;
using Nanover.Frontend.InputControlSystem.InputControllers;
using UnityEngine.XR.Interaction.Toolkit.UI;
using Nanover.Frontend.InputControlSystem.InputArbiters;
using UnityEngine;
using UnityEngine.Events;

// TODO: Sweeping across a UI will trigger a hover entry and exit event for each element that the ray touches. Thus the last interactor state should be check to prevent pause/resume spam requests.
// TODO: A check should be performed first to see if the input handler is happy to be placed into a paused state rather than to just force it upon it. In this case the <c>XRRayInteractor</c> should be disabled.
// TODO: Only input handlers that make use the the trigger button should be paused, all other currently active but non-conflicting input handlers should be allowed to continue existing.

namespace Nanover.Frontend.InputControlSystem.InputSelectors
{

    /// <summary>
    /// Selectively pauses input handlers when users interact with user interfaces.
    /// </summary>
    /// <remarks>
    /// This input selector is designed to detect when a user is interacting with a user interface
    /// via a <see cref="XRRayInteractor">Raycasting</see> based approach. When the user orientates
    /// an input controller device so the attached raycaster intersects a UI element the selector
    /// will assume that the user wishes to interact with the user interface. Thus the selector will
    /// instruct the arbiter to pause all currently active input handlers on the associated controller.
    /// Input handlers will be resumed one the raycaster reports that it is no longer pointing at
    /// a user interface element. 
    /// </remarks>
    public class XRRayInteractorSelector : IInputSelector
    {

        /// <summary>
        /// Holds a reference to the input arbiter that is tasked with managing the state changes
        /// of input handlers.
        /// </summary>
        /// <remarks>
        /// This reference is utilised to invoke methods for pausing and resuming input handlers
        /// based on user interaction with UI elements, ensuring seamless management of inputs.
        /// </remarks>
        private InputArbiter arbiter;

        /// <summary>
        /// Maintains a collection of basic input controller devices.
        /// </summary>
        /// <remarks>
        /// These devices are tracked for the purpose of input management and are crucial for the
        /// dynamic handling of user interactions.
        /// </remarks>
        private readonly List<BasicInputController> controllers =
            new List<BasicInputController>();

        /// <summary>
        /// Stores the callback functions to be invoked when a UI hover entered event is detected.
        /// </summary>
        /// <remarks>
        /// Callbacks in this list are called to manage the input state in response to user
        /// interactions with UI elements, facilitating the pausing of input handlers.
        /// </remarks>
        private readonly List<UnityAction<UIHoverEventArgs>> uiHoverEnteredCallbacks = 
            new List<UnityAction<UIHoverEventArgs>>();

        /// <summary>
        /// Stores the callback functions to be invoked when a UI hover exited event is detected.
        /// </summary>
        /// <remarks>
        /// Similar to <see cref="uiHoverEnteredCallbacks"/>, these callbacks are utilised to resume
        /// input handler operations once the user interaction with a UI element ceases.
        /// </remarks>
        private readonly List<UnityAction<UIHoverEventArgs>> uiHoverExitedCallbacks =
            new List<UnityAction<UIHoverEventArgs>>();

        /// <summary>
        /// Adds a controller to the managed collection and sets up event listeners for UI
        /// interaction events.
        /// </summary>
        /// <param name="controller">The input controller to be added.</param>
        /// <remarks>
        /// This method ensures that each controller is uniquely added and appropriately hooked into
        /// the UI interaction event system, enabling the dynamic management of input handler states
        /// based on whether or not the user is interacting with a user interface element.
        /// </remarks>
        public void AddController(BasicInputController controller)
        {
            // Don't allow the same controller to be added more than once.
            if (controllers.IndexOf(controller) != -1) return;

            // Add the controller to the controller list for tracking and bookkeeping purposes
            controllers.Add(controller);

            // Construct callback functions for the `uiHoverEntered` and `uiHoverExited` events.
            // These events will be invoked whenever the `XRRayInteractor` detects that the user
            // is pointing the ray cast interactor at a user interface element. These callback
            // functions will just redirect to the input arbiter's pause and resume methods
            // which allow input handlers to be temporarily frozen when the user is interacting
            // with a user interface.

            // Multiple user interface enter & exit events will be triggered when a user scans the
            // ray interactor over an interface. This is because the ray will enter & exit multiple
            // user interface elements. To prevent the arbiter from receiving an unrelenting torrent
            // of pause & resume requests, a catch must be added to only permit through those which
            // represent the ray no longer pointing at ANY UI element.
            UnityAction<UIHoverEventArgs> uiHoverExitedCallback = _ => ExitCallback(controller);

            // Sending unnecessary pause request has very little performance impact compared to 
            // resume request. Thus no special checking is required here.
            UnityAction<UIHoverEventArgs> uiHoverEnteredCallback = _ => arbiter.RequestPauseOfInputHandlersOnController(controller);
            

            // Add the callback functions to their respective lists so that they can be unsubscribed
            // from later on as and when needed.
            uiHoverEnteredCallbacks.Add(uiHoverEnteredCallback);
            uiHoverExitedCallbacks.Add(uiHoverExitedCallback);

            // Add a listener to the UI events. Not that this is not a traditional C# event but rather
            // a custom `Unity` event. Thus one must use `AddListener` rather than `+=`.
            controller.RayInteractor.uiHoverEntered.AddListener(uiHoverEnteredCallback);
            controller.RayInteractor.uiHoverExited.AddListener(uiHoverExitedCallback);
        }
        
        private void ExitCallback(BasicInputController controller)
        {
            // Multiple user interface enter & exit events will be triggered when a user scans the
            // ray interactor over an interface. This is because the ray will enter & exit multiple
            // user interface elements. To prevent the arbiter from receiving an unrelenting torrent
            // of pause & resume requests, a catch must be added to only permit through those which
            // represent the ray no longer pointing at ANY UI element.
            if (!controller.RayInteractor.IsOverUIGameObject())
                arbiter.RequestResumptionOfInputHandlersOnController(controller);
        }
        
        /// <summary>
        /// Removes a previously added controller and its associated event listeners from the
        /// management system.
        /// </summary>
        /// <param name="controller">The input controller to be removed.</param>
        /// <remarks>
        /// This method facilitates the clean-up of resources and subscriptions related to a
        /// controller, ensuring that input handling remains efficient and relevant to only
        /// the current set of active controllers.
        /// </remarks>
        public void RemoveController(BasicInputController controller)
        {

            // Identify the index of the controller within the controller list so that the
            // corresponding element in the callback lists can be identified and removed.
            int index = controllers.IndexOf(controller);

            // Don't attempt to remove controllers that where never added in the first place.
            if (index == -1) return;

            // Unsubscribe the callback methods from the UI events.
            controller.RayInteractor.uiHoverEntered.RemoveListener(uiHoverEnteredCallbacks[index]);
            controller.RayInteractor.uiHoverEntered.RemoveListener(uiHoverExitedCallbacks[index]);

            // Finally, remove the controller & its associated callback functions from the various lists.
            controllers.RemoveAt(index);
            uiHoverEnteredCallbacks.RemoveAt(index);
            uiHoverExitedCallbacks.RemoveAt(index);
        }

        /// <summary>
        /// Initialises the selector with a given input arbiter.
        /// </summary>
        /// <param name="arbiter">The input arbiter to be used for managing input handler state changes.</param>
        /// <remarks>
        /// This method is responsible for setting up the foundational link between the selector
        /// and the input arbiter, enabling the selector to manage input states effectively.
        /// </remarks>
        public void Initialise(InputArbiter arbiter) => this.arbiter = arbiter;

    }

}