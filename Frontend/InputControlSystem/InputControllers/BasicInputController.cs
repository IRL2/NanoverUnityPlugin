using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using InputDevice = UnityEngine.XR.InputDevice;

namespace Nanover.Frontend.InputControlSystem.InputControllers
{
    /// <summary>
    /// Concrete implementation of the <see cref="InputController"/> class that focuses on more
    /// traditional XR-controllers that are held in the user's hands.
    /// </summary>
    /// <remarks>
    /// Note that this class also introduces additional functionality by way of a tracked pose driver
    /// entity which allows for the controller game objects to track the movements of their real world
    /// analogous.
    /// </remarks>
    public class BasicInputController : InputController
    {
        /// <summary>
        /// A <c>GameObject</c> storing all of the components necessary to visually represent the controller.
        /// </summary>
        private GameObject model;

        /// <summary>
        /// The action-based controller component.
        /// </summary>
        /// <remarks>
        /// The action-based controller entity is a component provided by the Unity XR Interaction
        /// Toolkit, which supplies much of the foundational functionality for the `BasicInputController`
        /// class. It is used to assist in managing interactions with user interfaces and in updating
        /// the position and orientation of the controller model to reflect the real-world position and
        /// movement of the controller accurately.
        /// </remarks>
        private ActionBasedController actionBasedController;

        /// <summary>
        /// Ray caster interactor.
        /// </summary>
        /// <remarks>
        /// Ray caster based interactor used for interacting with intractable user interfaces at a
        /// distance. This is handled via ray casts that update the current set of valid targets
        /// for this interactor.
        /// </remarks>
        public XRRayInteractor RayInteractor { get; private set; }

        /// <summary>
        /// Line visualiser for ray caster.
        /// </summary>
        /// <remarks>
        /// This component manages the visual aspects of the line renderer and forms part of the
        /// ray caster interactor system. Specifically this is responsible for modifying things
        /// like ray's length and colour as it interacts with user interfaces.
        /// </remarks>
        private XRInteractorLineVisual interactorLineVisual;

        /// <summary>
        /// Line renderer for ray caster.
        /// </summary>
        /// <remarks>
        /// The line renderer is responsible for displaying the ray-caster that is used for
        /// interacting with user interfaces.
        /// </remarks>
        private LineRenderer lineRenderer;
        
        private bool allowUIInteractions = false;

        public bool AllowUIInteractions
        {
            get => allowUIInteractions;
            set
            {
                RayInteractor.enableUIInteraction = value;
                allowUIInteractions = value;
            }
        }
        
        /// <summary>
        /// This method is called once to allow controller instances to perform any required post instantiation set up.
        /// </summary>
        /// <param name="inputActionMap">Input action map to be associated with this controller.</param>
        /// <param name="device">Input device represented by this controller instance.</param>
        /// <param name="isDominant">Indicates whether controller is associated with the user's dominant hand/</param>
        public override void Initialise(InputActionMap inputActionMap, InputDevice device, bool isDominant)
        {
            // Assign the basic device attributes.
            InputActionMap = inputActionMap;
            InputDevice = device;
            IsDominant = isDominant;

            // Load in the relevant controller model.
            InitialiseControllerModel();

            // # Action Controller Setup

            // Bind the `ActionBasedController` component to the controller's input actions. This
            // allows for movements of the controller to be tracked and the position of the game
            // object representing it to be updated accordingly.
            actionBasedController.positionAction = new InputActionProperty(InputActionMap.FindAction("Position"));
            actionBasedController.rotationAction = new InputActionProperty(InputActionMap.FindAction("Rotation"));
            actionBasedController.trackingStateAction = new InputActionProperty(InputActionMap.FindAction("Tracking State"));
            actionBasedController.isTrackedAction = new InputActionProperty(InputActionMap.FindAction("Is Tracked"));

            // The `modelParent` field of the `ActionBasedController` entity is used to specify the
            // transform of the game object whose position and rotation is to be updated in response
            // to real-world controller movements. This is just set to the transform of the current
            // `BasicInputController` instance, as this is the object that we want to "represent"
            // the controller. Note that `modelParent` does not technically have to be the immediate
            // parent of the model, just a parent.
            actionBasedController.modelParent = transform;

            // The `ActionBasedController` entity also likes to know which game object holds the
            // actual controller model. For the time being, this will just point at the top level
            // object of the controller model rather than the lower level element of the prefab.
            // This can be changed at a later date if it is found poses a problem.
            actionBasedController.model = model.transform;

            // The trigger button will be used to interact with the user interface
            actionBasedController.uiPressAction = new InputActionProperty(InputActionMap.FindAction("Trigger"));

            AllowUIInteractions = true;
        }
        
        public void Awake()
        {
            // Instantiate the `ActionBasedController` component. During this process the component
            // will also create a new child object named "[ParentObjectName] Model Parent". This is
            // supposedly to hold the model prefab after creation. However, this is not needed an so
            // is deleted.
            actionBasedController = gameObject.AddComponent<ActionBasedController>();
            Destroy(transform.Find($"[{gameObject.name}] Model Parent").gameObject);
            actionBasedController.modelParent = null;

            // Set up the ray caster

            // Note that the `XRRayInteractor` will create a pair of empty child objects named
            // "[ParentObjectName] Ray Origin" and "[ParentObjectName] Attach".
            RayInteractor = gameObject.AddComponent<XRRayInteractor>();

            interactorLineVisual = gameObject.AddComponent<XRInteractorLineVisual>();
            interactorLineVisual.lineOriginTransform = transform;

            // The ray interactor should only be viable when it intersects with an element of a user
            // interface. At all other times it will remain hidden.
            Gradient gradientValid = new Gradient();
            Gradient gradientInvalid = new Gradient();

            GradientColorKey[] colourKeys = new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f) };

            gradientValid.SetKeys(colourKeys, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f) });
            gradientInvalid.SetKeys(colourKeys, new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0.0f) });

            interactorLineVisual.validColorGradient = gradientValid;
            interactorLineVisual.invalidColorGradient = gradientInvalid;

            // The `XRInteractorLineVisual` will create a new `LineRenderer` so we don't need to
            // create one ourselves here. However, it is retrieved and stored for later use. Also
            // a material must be selected to permit any form of meaningful visualisation.
            lineRenderer = gameObject.GetComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }
        
        /// <summary>
        /// Identify attached device and load in the corresponding mesh.
        /// </summary>
        /// <remarks>
        /// This ensures that the correct controller model is shown to the user, irrespective of
        /// what device they are using.
        /// </remarks>
        private void InitialiseControllerModel()
        {
            // Developers note's; in time this can become more complex, such as adding the ability to
            // setup skinned meshes or allow for material textures to be updated to reflect the
            // currently active `InputHandler`.

            // Get the sanitised device name.
            string deviceName = Helper.ControllerDeviceName(InputDevice);
            string handedness = $"{GetDeviceHandedness()}";
            
            // Load in the associated controller model prefab
            model = Resources.Load($"Controllers/Prefabs/{deviceName}_{handedness}") as GameObject;

            // If an invalid path is specified when loading resources then the `controllerModel`
            // will be null initialised. Thus the validity of the prefab must be checked before 

            // If an invalid resource path is specified then an error will be encountered when trying
            // to initialise it. Thus the validity of the prefab must now be checked.
            if (model == null)
                Debug.LogError($"Could not find controller prefab resource: {deviceName}_{handedness}");
            
            // Initialise the controller prefab then make it a child of the controller object.
            model = Instantiate(model);
            model.name = $"{handedness} Controller Model";
            model.transform.SetParent(transform);

            // DEBUG
            actionBasedController.model = model.transform;
            actionBasedController.modelParent = model.transform;
        }

        /// <summary>
        /// Returns an input device characteristic bitmap specifying the handedness of the device.
        /// </summary>
        /// <returns>Handedness of the associated device.</returns>
        public InputDeviceCharacteristics GetDeviceHandedness() => (
            InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Left) & InputDevice.characteristics;
        
    }

    /// <summary>
    /// Stores some useful helper methods.
    /// </summary>
    internal static class Helper
    {
        /// <summary>
        /// Return the sanitised/parsed controller name.
        /// </summary>
        /// <param name="device">Device whose name is to be fetched</param>
        /// <returns></returns>
        public static string ControllerDeviceName(InputDevice device)
        {
            // Query the device to get name as reported by the hardware.
            string reportedDeviceName = device.name;

            // String variable to hold the sanitised device name.
            string sanitisedDeviceName;

            // Map the reported name to a common "sanitised" name that the code can recognise.
            // Care must be taken here, as there exists a great deal of variability in the names
            // returned by calls made to the `UnityEngine.XR.InputDevice.name` method. For example,
            // the name may change based on the runtime and communication protocol used. Thus, one
            // cannot make a direct string comparison or perform filename mapping. Furthermore, name
            // based differentiation between the various Quest controller models is not currently
            // possible due to uniform name returns, i.e. "Oculus Touch Controller..." is returned
            // by all quest controllers. Consequently, the Quest2 controller model is temporarily
            // employed for such devices. Device specific identification will (hopefully) not be
            // needed for loading controller models in the future once the OpenXR
            // `XR_MSFT_controller_model` extension is standardised and more widely adopted. For
            // further information on `XR_MSFT_controller_model` extension see the OpenXR specification:
            //      registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_MSFT_controller_model

            if (reportedDeviceName.Contains("Oculus Touch Controller"))
            {
                // If the string "Oculus Touch Controller" is present in the name then default to the
                // meta quest 2 controller.
                sanitisedDeviceName = "Meta_Quest_2";
            }
            else
            {
                // If the name does not correspond to one of those specified above then use the fallback.
                // This should perhaps issue a warning that the user can see.
                Debug.LogError($"No prefab found for controller device: {reportedDeviceName} (falling back to default)");
                sanitisedDeviceName = "Fallback";
            }
            
            return sanitisedDeviceName;
        }
    }
}

// Warning: by default, `TrackedPoseDriver` entities will rename any and all input actions
// provided to them. For example an `InputAction` named "<ACTION_NAME>" will be renamed to
// "<MAP_NAME> - TPD - <ACTION_NAME>". One must therefor provide the tracked pose driver
// with a clone of the action map in order to avoid unintentional name mangling. Undoing
// this may **permanently brick** the input action map (even after the program terminates).
