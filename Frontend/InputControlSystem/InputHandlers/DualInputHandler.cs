using Nanover.Frontend.InputControlSystem.InputControllers;
using UnityEngine.Assertions;

namespace Nanover.Frontend.InputControlSystem.InputHandlers
{
    /// <summary>
    /// Abstract class that extends the functionality of the <see cref="InputHandler"/> class
    /// to accommodate handlers that require inputs from two separate controllers.
    /// </summary>
    /// <remarks>
    /// This is done by adding an <c>Ancillary</c> controller field. The <c>Ancillary</c> controller
    /// works in conjunction with the controller defined in <see cref="MonoInputHandler"/>
    /// (<C>Controller</C>) to facilitate dual-input scenarios. Together, they enable complex
    /// interactions that require coordination between both hands. Note that this class does not
    /// truly inherit from its mono input sibling to avoid potential situations in which a dual
    /// handler may be misidentified as a mono handler.
    /// 
    /// Commonly, <see cref="DualInputHandler"/> instances will assign the controller held in the
    /// user's dominant hand to <c>Controller</c>, and that in the user's non-dominant hand to
    /// <c>Ancillary</c>. However, this is not enforced behaviour, and both <c>Controller</c> and
    /// <c>Ancillary</c> are assumed to be of equal importance by code external to the input handler.
    /// </remarks>
    public abstract class DualInputHandler: InputHandler
    {

        /// <summary>
        /// Controller device from which inputs may be sourced whenever the handler is active.
        /// </summary> 
        public InputController Controller
        {
            get => Controllers[0];
            protected set => Controllers[0] = value;
        }

        /// <summary>
        /// Controller device from which inputs may be sourced in addition to that assigned to the
        /// <c>Controller</c> field.
        /// </summary>
        /// <remarks>
        /// Commonly, handlers that support multiple controllers will assign the controller held
        /// in the user's dominant hand to <c>Controller</c>, and that in the user's non-dominant
        /// hand to <c>Ancillary</c>. However, this is not guaranteed, and both <c>Controller</c>
        /// and <c>Ancillary</c> are assumed to be of equal importance by code external to the input
        /// handler.
        /// </remarks>
        public InputController Ancillary
        {
            get => Controllers[1];
            protected set => Controllers[1] = value;
        }


        /// <summary>
        /// Binds a controller to the input handler from which user inputs may be sourced.
        /// </summary>
        /// <param name="controller">The controller to be bound.</param>
        /// <remarks>
        /// By default, the controller's <c>IsDominant</c> property is used to determine whether it
        /// should be assigned to the <c>Controller</c> or <c>Ancillary</c>field. An assertion is
        /// made to ensure that a controller is not bound if a controller of the same dominance is
        /// already assigned.
        /// </remarks>
        public override void BindController(InputController controller)
        {
            Controllers ??= new InputController[2];

            // Assert that the controller is compatible before attempting to bind.
            Assert.IsTrue(IsCompatibleWithInputController(controller),
                "Failed to bind controller: specified controller is not compatible with this input handler.");

            // Check the controller's dominance and bind it to the corresponding property.
            if (controller.IsDominant)
            {
                // Assert that the initial controller has not already been assigned before binding.
                Assert.IsNull(Controller,
                    "Failed to bind controller as dominant hand controller has already been assigned.");
                Controller = controller;
            }
            else
            {
                // Assert that the ancillary controller has not already been assigned before binding.
                Assert.IsNull(Ancillary,
                    "Failed to bind controller as non-dominant hand controller has already been assigned.");
                Ancillary = controller;
            }
        }
    }

}
