using Nanover.Frontend.InputControlSystem.InputControllers;
using UnityEngine.Assertions;

namespace Nanover.Frontend.InputControlSystem.InputHandlers
{
    /// <summary>
    /// This builds upon the <see cref="InputHandler"/> class to add explicit, but not concrete,
    /// support for a single controller device, specifically an <see cref="InputController"/>
    /// instance. This forms the conceptual foundation upon which the other controller focused input
    /// handlers are based.
    /// </summary>
    /// <remarks>
    /// The <see cref="MonoInputHandler"/> class and its derived classes are designed to operate
    /// with one and only one <see cref="InputController"/> device attached. Meaning that they
    /// source inputs only form one controller at a time. This is the simplest type of input handler
    /// as it does not require much in the way of dedicated logic, hence this abstract class does
    /// not add a large amount of logic. Generally speaking, one instance of each mono-handler will
    /// be created for each controller detected, unless otherwise precluded by the
    /// <c>IsCompatibleWithController</c> method. This means that two of such handlers can be active
    /// simultaneously, working independently from one another; one on the right controller and one
    /// on the left.
    /// </remarks>
    public abstract class MonoInputHandler: InputHandler
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
        /// Binds a controller to the input handler from which user inputs may be sourced.
        /// </summary>
        /// <param name="controller">The controller to be bound to the handler.</param>
        public override void BindController(InputController controller)
        {
            Controllers ??= new InputController[1];

            // Assert that the controller is compatible before attempting to bind.
            Assert.IsTrue(IsCompatibleWithInputController(controller),
                "Failed to bind controller: specified controller is not compatible with this input handler.");

            // Assert that the controller has not already been bound.
            Assert.IsNull(Controller,
                "Failed to bind controller: binding of an additional controller is not possible as the Controller has already been assigned");
            
            // Assign the controller
            Controller = controller;
        }

    }
    
}
