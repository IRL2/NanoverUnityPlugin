using System.Linq;
using Nanover.Frontend.InputControlSystem.InputControllers;
using Nanover.Frontend.InputControlSystem.InputHandlers;
using Nanover.Frontend.InputControlSystem.InputArbiters;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Nanover.Frontend.UI.ContextMenus;



namespace Nanover.Frontend.InputControlSystem.InputSelectors
{

    /// <summary>
    /// This input selector displays a radial menu that allows users to manually pick an input handler
    /// to activate. Thus, giving users a manual degree of control over what input handlers are active.
    /// </summary>
    /// <remarks>
    /// Only input handlers that implement the the <see cref="userSelectableInputHandlers"/> interface
    /// are compatible here, all others will just be ignored. This is because the interface guarantees
    /// required information such as a human readable name, an icon, etc. Each radial selector must be
    /// associated with a controller, which allows tracking information to be sourced so that the menu
    /// can be placed in the correct location & user selection events can be detected. This class does
    /// not contain much active logic, instead it simply redirects the radial menu selection events to
    /// the input arbiter, with some additional arguments passed in.
    /// </remarks>
    public class RadialInputSelector: RadialMenu, IInputSelector
    {
        /* Developer's Notes:
         * Ideally we would want to store the arbiter and controller as a read only field so that
         * each instance could be probed to identify what & who it is associated with. This would
         * also mean that we could do away with having to store the callback function as a field.
         */

        /// <summary>
        /// Radial menu callback function.
        /// </summary>
        /// <remarks>
        /// This callback function is triggered when an element in the radial menu is selected, and
        /// will redirect to the input arbiter's <c>ActivateInputHandler</c> method. This is stored
        /// as a field to make unsubscribing easier. 
        /// </remarks>
        private OptionSelectedEventHandler optionSelectedHandler;

        /// <summary>
        /// Initialise the radial selection menu entity.
        /// </summary>
        /// <param name="inputHandlers">List of <see cref="InputHandler">InputHandlers</see> that
        ///     are to be added to the radial menu. Note that only those which implement the
        ///     <see cref="userSelectableInputHandlers"/> interface will actually be added
        ///     to the menu.</param>
        /// <param name="arbiter">The <see cref="InputArbiter"/> that should be called to activate
        ///     a handler when the user actions the radial menu.</param>
        /// <param name="controller">The controller device to which this selection menu should be
        ///     attached.</param>
        /// <param name="selectionButton">The button which, when pressed, will display the radial
        ///     selection menu.</param>
        /// <remarks>
        /// Note that only input handlers that implement the <see cref="userSelectableInputHandlers"/>
        /// interface will be added to the radial menu. All others will be ignored.
        /// </remarks>
        public void Initialise(IEnumerable<InputHandler> inputHandlers, InputArbiter arbiter, InputController controller, InputAction selectionButton)
        {
            Initialise(controller, selectionButton, menuName: "Interaction Mode");

            // Identify the input handlers that are both i) bound to the specified controller, and
            // ii) are user selectable.
            var userSelectableInputHandlers = inputHandlers.Where(
                t => t.IsBoundToController(controller) && t is IUserSelectableInputHandler).ToArray();

            // Parallel array cast into the `IUserSelectableInputHandler` type so that icons & names
            // can be extracted in the next stage. A sort is performed here to ensure that the menu
            // options have a consistent order from session to session.
            var userSelectableInterfaces = userSelectableInputHandlers.OfType<IUserSelectableInputHandler>(
                ).OrderByDescending(i => i.Priority).ToArray();

            // Recast `userSelectableInterfaces` back into `userSelectableInputHandlers` to ensure
            // array orders match up.
            userSelectableInputHandlers = userSelectableInterfaces.Cast<InputHandler>().ToArray();

            // Pass in icons and names for each menu option.
            ConfigureMenuElements(
                userSelectableInterfaces.Select(obj => obj.Icon).ToArray(),
                userSelectableInterfaces.Select(obj => obj.Name).ToArray());

            // The radial menu's option selection event is redirected here so that it calls back to
            // the arbiter's `RequestInputHandlerToggle` method. The callback function is stored within
            // a field so that it can be unsubscribed later on if deemed necessary.
            optionSelectedHandler = (selectedIndex) => arbiter.RequestInputHandlerToggle(
                userSelectableInputHandlers[selectedIndex], new List<InputController> { controller });

            OptionSelected += optionSelectedHandler;

            // Inform the radial menu that the setup process is finished.
            Finalise();
        }

        /// <summary>
        /// Ensure that event subscriptions are torn down upon instance destruction.
        /// </summary>
        public void OnDestroy()
        {
            // Unsubscribe from the radial menu's option selection event.
            if (optionSelectedHandler != null)
            {
                OptionSelected -= optionSelectedHandler;
                optionSelectedHandler = null;
            }
        }
    }
}