using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace Nanover.Frontend.InputControlSystem.ControllerManagers
{
    /// <summary>
    /// The abstract base class 'ControllerManager' serves as the foundation for managing controller
    /// instances within an XR rig.
    /// </summary>
    /// <remarks>
    /// This MonoBehaviour-based entity is attached as a component to the top-level object of the XR
    /// rig. It is responsible for detecting connected devices at start-up and creating appropriate
    /// 'ControlDevice' objects. Throughout runtime, it handles events related to controller
    /// disconnection and re-connection. Moreover, this entity bootstraps the input control system
    /// as a whole by setting up the components necessary for input processing. Note that the input
    /// processing components are constructed by the individual sub-classes.
    /// </remarks>
    public abstract class ControllerManager : MonoBehaviour
    {

        // Note: The 'RightHandDominant' field will eventually be opened for public set calls, but
        // this is currently disabled pending the addition of supporting framework.

        /// <summary>
        /// The <code>GameObject</code> to which the target system is attached.
        /// </summary>
        /// <remarks>
        /// Here the term "target system" refers to the entity representing the atomistic system of
        /// interest. This is commonly a simulation type object from which connections and structural
        /// data can be sourced. This should be set within the Unity editor.
        ///  </remarks>
        [SerializeField]
        private GameObject systemObject;
        
        /// <summary>
        /// Top level input actions asset from which input action maps can be sourced.
        /// </summary>
        /// <remarks>
        /// This is primary used to get the input action maps for the controllers.
        /// </remarks>
        [SerializeField]
        private InputActionAsset inputActionAsset;

        /// <summary>
        /// Boolean indicating if the user is right-hand dominant.
        /// </summary>
        [SerializeField]
        private bool rightHandDominant = true;


        /// <summary>
        /// Top level input actions asset from which input action maps can be sourced.
        /// </summary>
        /// <remarks>
        /// This is primary used to get the input action maps for the controllers.
        /// </remarks>
        public InputActionAsset InputActionAsset => inputActionAsset;


        /// <value>
        /// The <code>GameObject</code> to which the target system is attached.
        /// </value>
        /// <remarks>
        /// Here the term "target system" refers to the entity representing the atomistic system of
        /// interest. This is commonly a simulation type object from which connections and structural
        /// data can be sourced.
        ///  </remarks>
        public GameObject SystemObject
        {
            set => systemObject = value;
            get => systemObject;
        }

        /// <value>
        /// Boolean value indicating if the user is right-hand dominant.
        /// </value>
        public bool RightHandDominant { get; protected set; }

        /// <value>
        /// Represents the handedness of the user as an 'InputDeviceCharacteristics' bitmap.
        /// </value>
        /// <value>
        /// The handedness is determined by the user's right-hand dominance status. This is an alternative
        /// representation of the <code>rightHandDominant</code> field that is commonly of more use than
        /// a simple Boolean.
        /// </value>
        public InputDeviceCharacteristics Handedness => rightHandDominant ? InputDeviceCharacteristics.Right : InputDeviceCharacteristics.Left;
    }
}