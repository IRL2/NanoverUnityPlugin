using Nanover.Frontend.InputControlSystem.InputControllers;
using UnityEngine.Assertions;


namespace Nanover.Frontend.InputControlSystem.InputHandlers
{
    /// <summary>
    /// Abstract class that extends the functionality of the <see cref="InputHandler"/> class
    /// to accommodate handlers which support inputs from two controllers, but can still operate,
    /// in a somewhat diminished capacity, using only a single controller.
    /// </summary>
    /// <remarks>
    /// It is worth noting that the hybrid nature of these handlers results a steep leaning curve.
    /// 
    /// When the state of a hybrid input handler is explicitly set to <c>Active</c> by an external
    /// entity it should source user inputs from all bound controllers. That is to say, it should
    /// initially behave in an identical manner to its <see cref="DualInputHandler"/> super-class.
    /// However, at this point one of the two controllers may be "restricted" causing the handler
    /// to behave more akin to a standard <see cref="MonoInputHandler"/> instance.
    ///
    /// Similar to the <see cref="DualInputHandler"/> class, the controller held in the user's 
    /// dominant hand will commonly be assigned to the <c>Controller</c> field, and that in the
    /// user's non-dominant hand to the <c>Ancillary</c> field. However, in situations where the
    /// device assigned <c>Controller</c> is disabled, the contents of the <c>Controller</c> and
    /// <c>Ancillary</c> fields may be swapped; although this left to the discretion of the
    /// developer and is not required or always useful.
    ///
    /// If, for some reason, both controllers are restricted, the handler should auto-transition
    /// into a background state; i.e. <c>Disabled</c> or <c>Passive</c>. Furthermore, if the handler
    /// is operating in a diminished capacity & its state is cycled from <c>Active</c> to <c>Disabled</c>
    /// and back, then it should assume that it has sole event binding privilege to both of the
    /// controllers.
    ///
    /// A hybrid input handler may transition from a background state to a diminished <c>Active</c>
    /// state by calling the <c>Release</c> method to allow it to act upon user inputs sourced from
    /// the specified controller only.
    ///
    /// Calling <c>Release</c> once on a disabled handler will move it into an active, but restricted,
    /// state in which it may only access one of the controllers. Calling <c>Release</c> a second
    /// time will permit the handler to access the other controller, and will thus be fully active
    /// and unrestricted. Calling <c>Restrict</c> on a fully active handler will restrict the
    /// handler's access to one of the controllers. While it will remain in the <c>Active</c> state
    /// it will have restricted functionality. Calling <c>Restrict</c> a second time will remove
    /// access to the second remaining controller. Given that the handler can no longer access
    /// either controller now, it will transition into the <c>Disabled</c> state. Note that the
    /// the term "restricted state" is not a true state but rather a sub-state of <c>Active</c>.
    /// </remarks>
    public abstract class HybridInputHandler : InputHandler
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
        /// in the user's dominant hand to <c>Controller</c>, and that in the user's non-dominant hand
        /// to <c>Ancillary</c>. However, this is not guaranteed, and both <c>Controller</c> and <c>Ancillary</c>
        /// are assumed to be of equal importance by code external to the input handler.
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
        /// By default, the controller's <c>IsDominant</c> property is used to determine whether it should
        /// be assigned to the <c>Controller</c> or <c>Ancillary</c> field. An assertion is made to ensure that
        /// a controller is not bound if a controller of the same dominance is already assigned.
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

        /// <summary>
        /// Boolean array indicating which controllers this handler is actively bound to. If the
        /// handler is disabled then both value should be false. If fully active then both should
        /// be true. If the handler is restricted then one value should be true and the other false.
        /// </summary>
        protected bool[] activelyBound = { false, false };

        /// <summary>
        /// Boolean indicating whether the the handler is operating in a diminished capacity.
        /// Specifically, the handler is active but only has sole event binding privilege to
        /// only one of the two controller devices. Note that this will return false if the
        /// handler is not active.
        /// </summary>
        public bool Restricted => activelyBound[0] != activelyBound[1];

        /// <summary>
        /// Inform the hybrid input handler that it may no longer act upon user inputs sourced
        /// from a specific controller.
        /// </summary>
        /// <param name="controller">Controller whose access is being restricted</param>
        /// <remarks>
        /// It is assumed that the controller provided is one currently assigned to the handler.
        /// </remarks>
        public abstract void Restrict(InputController controller);

        /// <summary>
        /// Release the restriction placed upon the handler and allow it to once again act upon
        /// inputs sourced from the currently restricted controller.
        /// </summary>
        /// <param name="controller">Controller whose access restriction is being lifted</param>
        public abstract void Release(InputController controller);

        /// <summary>
        /// Boolean indicating if the specified controller is currently restricted.
        /// </summary>
        /// <param name="controller">Controller to be tested for restriction.</param>
        public bool IsRestricted(InputController controller) => !activelyBound[ControllerIndex(controller)];
        

    }
}
