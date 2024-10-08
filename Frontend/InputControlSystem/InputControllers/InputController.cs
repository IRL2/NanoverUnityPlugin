using UnityEngine;
using UnityEngine.InputSystem;
using InputDevice = UnityEngine.XR.InputDevice;


namespace Nanover.Frontend.InputControlSystem.InputControllers
{
    /// <summary>
    /// Physical controllers are represented within the virtual construct by a <c><GameObject</c>
    /// with an attached component based on the abstract base class <c>ControlDevice</c>. Instances
    /// of derived classes will play the roll of "controllers" within the XR-rig.  
    /// </summary>
    /// 
    /// <example>
    /// The code block proved below provides a rough idea of how such an entity is to be instantiated.
    /// However, it is worth noting that this code is not technically valid as <c>InputController</c>
    /// is an abstract class.
    /// 
    /// <code>
    /// // Create blank game object to represent the controller
    /// someControllerObject = new GameObject();
    /// 
    /// // Add a new `Controller` component to the newly created game object
    /// someControllerComponent = someControllerObject.AddComponent<InputController>();
    ///
    /// // Locate the input device associated with the right controller
    /// var controllerInputDevices = new List<InputDevice>();
    /// var desiredCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
    /// InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, controllerInputDevices);
    /// controller.Initialise(inputActionMap, ontrollerInputDevices.FirstOrDefault(), true)
    /// </code>
    /// 
    /// </example>
    public abstract class InputController: MonoBehaviour
    {

        /// <summary>
        /// Input actions that are associated exclusively with this controller instance.
        /// </summary>
        public InputActionMap InputActionMap { get; protected set; }

        /// <summary>
        /// Input device within the XR subsystem that is associated with this instance.
        /// </summary>
        /// <remarks>
        /// This can be queried to identify device specific characteristics.
        /// </remarks>
        public InputDevice InputDevice { get; protected set; }
 

        /// <summary>
        /// Indicates whether this controller is associated with the user's dominant hand.
        /// </summary>
        public bool IsDominant { get; protected set; }


        /// <summary>
        /// This method is called once to allow controller instances to perform any required post instantiation set up.
        /// </summary>
        /// <param name="inputActionMap">Input action map to be associated with this controller.</param>
        /// <param name="device">Input device represented by this controller instance.</param>
        /// <param name="isDominant">Indicates whether controller is associated with the user's dominant hand/</param>
        public abstract void Initialise(InputActionMap inputActionMap, InputDevice device, bool isDominant);
    }
}
