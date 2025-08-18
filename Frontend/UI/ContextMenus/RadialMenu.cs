using Nanover.Frontend.InputControlSystem.InputControllers;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using InputDevice = UnityEngine.XR.InputDevice;
using Plane = System.Numerics.Plane;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;


// TODO: Cache path management should be abstracted to a global cache manager.
// TODO: Colours should default to global definitions as specified in a settings menu.
// TODO: Implement unit tests.
// TODO: Need to change documentation format from mark down to extensible markup language.
// TODO: This object needs to delete all sibling components it created when it is destroyed.
// TODO: This should be updated to support dynamic modification of and updating.
// TODO: Support for clockwise and counterclockwise ordering should be added.
// This code could benefit from refactoring to abstract out the core components allowing for an
// overall simplified operation.

namespace Nanover.Frontend.UI.ContextMenus
{
    /// <summary>
    /// The `RadialMenu` class provides functionality for creating an interactive, radial (circular) menu
    /// using Unity's UI system. Each sector of the menu can have its own icon and text, and one can subscribe
    /// to an event to be notified when a sector is selected.
    /// </summary>
    /// <remarks>
    /// ### Usage
    /// #### Initialisation and Configuration
    /// The setup and configuration of a `RadialMenu` involve several steps, and certain methods must be called
    /// in a specific order to ensure correct setup:
    /// 
    /// 1. **Initialisation**: Begin by invoking the `Initialise` method, where various properties related to 
    ///    input can be specified.
    ///    ```cs
    ///    Initialise(InputController controller, InputAction showRadialMenuAction, ...)
    ///    ```
    ///    Here the `controller` provides a device from which an input action map can be sourced so
    ///    that controller and orientation can be accessed. The input action (button) that is used
    ///    to display the menu cannot be inferred from context and thus must be explicitly specified
    ///    via the `showRadialMenuAction` argument. One may also control the size of the menu through
    ///    the `scale` argument along with the default text shown within it by changing `menuName`.
    /// 
    /// 2. **Configure Menu Elements**: After initialisation, specify the icons and names for each sector 
    ///    using `ConfigureMenuElements`.
    ///    ```cs
    ///    ConfigureMenuElements(string[] iconFilePaths, string[] names)
    ///    // Or use if providing sprite objects direly. 
    ///    ConfigureMenuElements(Sprite[] iconSprites, string[] names)
    ///    ```
    /// 
    /// 3. **(Optional) Additional Configuration**: Subsequent to configuring the menu elements, one may 
    ///    optionally configure additional aesthetics, behaviours, and properties of the radial menu by
    ///    calling the following methods, as per ones requirements:
    ///    - `ConfigureSectorAesthetics(...)`
    ///    - `ConfigureFocusBehaviour(...)`
    ///    - `ConfigureColours(...)`
    ///    - `ConfigureFont(...)`
    /// 
    /// 4. **Finalisation**: Finally, finalise the menu configuration by calling the `Finalise` method.
    ///    This instructs the menu system that configuration has finished and that the required setup
    ///    procedures may now be performed. It is important to note that the menu will be enabled by
    ///    default.
    ///    ```cs
    ///    Finalise()
    ///    ```
    ///    
    /// #### Event Subscription
    /// To receive notifications when a menu option is selected, subscribe to the `OptionSelectedEventHandler` 
    /// event, which provides the index of the selected sector.
    /// 
    /// #### Notes
    /// - Make sure to call the `Finalise` method after all configurations are done, and before using the radial menu.
    /// - The icons used should ideally have white as the foreground colour and transparent white as the background to 
    ///   achieve the best visual results.
    ///   
    /// Example usage might look like this:
    /// ```cs
    /// var radialMenu = new RadialMenu();
    /// RadialMenu radialMenu = someGameObject.AddComponent<RadialMenu>();;
    /// radialMenu.Initialise(controller, showRadialMenuAction);
    /// radialMenu.ConfigureMenuElements(new string[] {"path/icon1", "path/icon2"}, new string[] {"Option 1", "Option 2"});
    /// radialMenu.ConfigureColours(Color.red, Color.gray, Color.white);
    /// radialMenu.Finalise();
    /// radialMenu.OptionSelected += YourEventHandlerMethod;
    /// ```
    /// 
    /// ### Event Handling
    /// To handle an option selection, your event handler method should match the signature of the `OptionSelectedEventHandler`:
    /// ```cs
    /// void YourEventHandlerMethod(int selectedIndex)
    /// ```
    /// wherein `selectedIndex` gives the index of the selected menu sector.
    /// </remarks>
    /// <example>
    /// Example of subscribing to the event and logging the selected option index:
    /// ```cs
    /// radialMenu.OptionSelected += (selectedIndex) => {
    ///     Debug.Log($"Option selected: {selectedIndex}");
    /// };
    /// ```
    /// </example>
    public class RadialMenu : MonoBehaviour
    {

        // Developer's Notes:
        // Most of the fields have been designated private as allowing users to modify almost any of
        // these variables would at best will do nothing and at worst cause undefined behaviour. The
        // fields are configured via configuration methods.

        #region "Base public fields"
        /// <summary>
        /// Name of the menu to be displayed when no sector is focused.
        /// </summary>
        public string MenuName = "";

        /// <summary>
        /// Array of icons to be displayed. One for each sector.
        /// </summary>
        /// <remarks>
        /// Note that the foreground and background of these icons should be white and transparent white
        /// respectively, i.e. (1, 1, 1, 1) and (1, 1, 1, 0). Circular icons produce the best result.
        /// </remarks>
        private Sprite[] icons;

        /// <summary>
        /// Name to be displayed when a sector is focused.
        /// </summary>
        private string[] names;

        /// <summary>
        /// Transform of the controller object to which this radial menu is associated.
        /// </summary>
        /// <remarks>
        /// This is required to ensure that the radial menu is always centred about the controller.
        /// </remarks>
        private Transform controllerTransform;


        /// <summary>
        /// The input action associated with the button that shows the menu when depressed and hides
        /// it when released.
        /// </summary>
        private InputAction showRadialMenuAction;

        /// <summary>
        /// The XR input device with which this menu is associate.
        /// </summary>
        /// <remarks>
        /// This `UnityEngine.XR.InputDevice` structure is used to help target the haptic response.
        /// For some reason one must exactly specify the device to which a haptic impulse is to be
        /// sent, rather than just attaching to an input action.
        /// </remarks>
        private InputDevice inputDevice;
        #endregion

        public delegate void OptionSelectedEventHandler(int selectedIndex);
        public event OptionSelectedEventHandler OptionSelected;

        #region "Visual configuration settings"
        /// <summary>
        /// Fractional size of the icons [0, 1].
        /// </summary>
        /// <remarks>
        /// icons are placed in the centre of their respective annular sectors. A size value of one
        /// will result in icons being scaled to the size of the annular sector they are placed in.
        /// </remarks>
        private float iconSize = 0.75f;

        /// <summary>
        /// Relative size of the inner cut-out [0, 1). A zero valued inner radius fraction will result
        /// in each annular sector being a circular sector. Whereas a value of one will result in
        /// infinitely thin sectors.
        /// </summary>
        private float innerRadiusFraction = 0.6f;

        /// <summary>
        /// Controls the degree of separation between each sector.
        /// </summary>
        /// <remarks>
        /// This value is specified as a fraction of the unit circle's radius. As such, the use of very
        /// small values are advised (0.01 - 0.05).
        /// </remarks>
        private float padding = 0.05f;

        /// <summary>
        /// Height and width of a full radial menu in pixels.
        /// </summary>
        /// <remarsk>
        /// Note that this has not effect on the final size of the radial menu, only is clarity. Larger
        /// values should be used for higher resolution headsets.
        /// </remarsk>
        private int resolution = 1000;

        /// <summary>
        /// Width of the convolution window used during the blurring process. Larger values produce
        /// greater smoothing effects.
        /// </summary>
        /// <remarks>
        /// Texture blurring is used to help mitigate graphical artefacts arising from aliasing effects.
        /// A blurring width of zero will cause the blurring steps to be skipped.
        /// </remarks>
        private int blurWidth = 3;

        /// <summary>
        /// Number of blurring passes to be performed. Multiple smaller passes can produce better
        /// visual effects than a single larger pass.
        /// </summary>
        private int numberOfBlurPasses = 3;

        /// <summary>
        /// Texture margin size.
        /// </summary>
        /// <remarks>
        /// A margin of `margin` pixels will be added to each side of the texture to prevent miscellaneous
        /// visual artefacts. At a minimum this should be ((m - 1)/2)*n pixels, where n is the number of
        /// blurring passes and m is the blur width.
        /// </remarks>
        private int margin = 5;

        /// <summary>
        /// Size of the font used when displaying focused option name.
        /// </summary>
        private float fontSize = 0.15f;

        /// <summary>
        /// Colour of the font used to display the focused option name.
        /// </summary>
        private Color fontColour = Color.white;

        /// <summary>
        /// Default colour of each sector sprite.
        /// </summary>
        private Color baseColour = new Color(0.878f, 0.878f, 0.878f, 1f);

        /// <summary>
        /// Colour if the icon sprites.
        /// </summary>
        private Color iconColour = new Color(0.251f, 0.251f, 0.251f, 1f);

        /// <summary>
        /// Colour of the focused sector sprite.
        /// </summary>
        private Color focusedColour = new Color(0.188f, 0.753f, 1f, 1f);

        #endregion

        #region "Dynamic behaviour settings
        /// <summary>
        /// Amplitude of haptic response sent when focusing a new sector.
        /// </summary>
        private float hapticAmplitude = 0.5f;

        /// <summary>
        /// Duration of haptic response sent when focusing a new sector.
        /// </summary>
        private float hapticDuration = 0.05f;
        #endregion

        #region "Internal static fields to be set at runtime"
        /// <summary>
        /// Sprite storing the texture of the annular sector.
        /// </summary>
        private Sprite sprite;

        /// <summary>
        /// Array storing the game objects representing each of the sectors.
        /// </summary>
        private GameObject[] AnnularSectorGameObjects;

        /// <summary>
        /// Array storing the image components associated with each of the game objects stored in the
        /// `AnnularSectorGameObjects` field.
        /// </summary>
        /// <remarks>
        /// This is used to avoid having to use `GetComponent` to interact with each sector's image.
        /// </remarks>
        private Image[] AnnularSectorImages;

        /// <summary>
        /// Audio clip to be played when a new sector is focused.
        /// </summary>
        private AudioClip focusSound;


        /// <summary>
        /// Text mesh pro entity used to display focused option name to the user.
        /// </summary>
        private TMP_Text text;

        private GameObject textObject;

        /// <summary>
        /// Number of sectors present in the radial menu.
        /// </summary>
        private int NumberOfSectors
        {
            get { return icons.Length; }
        }

        /// <summary>
        /// Canvas component of the radial menu.
        /// </summary>
        /// <remarks>
        /// This is used to provide fast access to the transform component without having to go via
        /// a costly `GetComponent` calls.
        /// </remarks>
        private Canvas canvas;

        /// <summary>
        /// Size delta component of the canvas.
        /// </summary>
        /// <remarks>
        /// This is used to provide fast access to the size component without having to go via
        /// a costly `GetComponent` calls.
        /// </remarks>
        private Vector2 canvasSizeDelta;

        /// <summary>
        /// Audio source necessary to play audio clips.
        /// </summary>
        private AudioSource audioSource;
        #endregion

        #region "Internal state properties"
        /// <summary>
        /// Integer tracking which sector, if any is focused. A value of -1 indicates that no
        /// sector is currently focused. This indexes the `AnnularSectorGameObjects`,
        /// `AnnularSectorImages`, and `icons` arrays.
        /// </summary>
        private int focusedSectorIndex = -1;

        /// <summary>
        /// Indicates if the menu has been finalised.
        /// </summary>
        private bool finalised = false;

        /// <summary>
        /// Indicates if `showRadialMenuAction` has been subscribed to.
        /// </summary>
        private bool subscribed;
        #endregion

        #region "Manditory configuration methods"
        /// <summary>
        /// Initialises the radial menu's primary configuration and setup input actions.
        /// </summary>
        /// <param name="controller">An <see cref="InputController"/> instance used to source the
        ///  requisite <see cref="InputActionMap"/> and <see cref="InputDevice"/> from.</param>
        /// <param name="showRadialMenuAction">An <see cref="InputAction"/> used to determine when to
        ///  display the radial menu.</param>
        /// <param name="scale">A <see cref="float"/> that denotes the global scale of the radial menu.
        ///  Default is 0.25f.</param>
        /// <param name="menuName">A <see cref="string"/> that specifies the name of the menu to be
        ///  displayed when no sector is focused. Default is an empty string.</param>
        /// <remarks>
        /// It must be ensured that the `Initialise` method is invoked prior to any of the configuration
        ///  or finalisation methods.
        /// </remarks>
        public void Initialise(
            InputController controller, InputAction showRadialMenuAction,
            float scale = 0.25f, string menuName = "")
        {
            EnsureNotFinalised();

            // Store the show-menu action button.
            this.showRadialMenuAction = showRadialMenuAction;

            // Store the transform of the controller entity so that the menu can be positioned
            // correctly later on.
            controllerTransform = controller.transform;

            inputDevice = controller.InputDevice;

            // Assign other minor fields
            MenuName = menuName;

            // Create and configure the canvas for the radial menu
            GenerateCanvas(scale);
        }

        /// <summary>
        /// Configures options to be displayed on the radial menu.
        /// </summary>
        /// <param name="iconFilePaths">An array of <see cref="string"/> indicating the file paths for
        ///  each icon to be displayed in the radial menu sectors.</param>
        /// <param name="names">An array of <see cref="string"/> specifying the names to be displayed
        ///  when a sector is focused.</param>
        /// <remarks>
        /// The length of `iconFilePaths` and `names` must be equal to define a name and icon for each
        ///  sector of the radial menu.
        /// </remarks>
        public void ConfigureMenuElements(string[] iconFilePaths, string[] names)
        {
            Sprite[] iconsTemp = new Sprite[iconFilePaths.Length];
            for (int i = 0; i < iconFilePaths.Length; i++) iconsTemp[i] = Resources.Load<Sprite>(iconFilePaths[i]);
            ConfigureMenuElements(iconsTemp, names);
        }


        /// <summary>
        /// Configures options to be displayed on the radial menu.
        /// </summary>
        /// <param name="iconSprites">An array of <see cref="Sprite"/> entities to be used as icons
        /// in the radial menu sectors.</param>
        /// <param name="names">An array of <see cref="string"/> specifying the names to be displayed
        ///  when a sector is focused.</param>
        /// <remarks>
        /// The length of `iconSprites` and `names` must be equal.
        /// </remarks>
        public void ConfigureMenuElements(Sprite[] iconSprites, string[] names)
        {
            EnsureNotFinalised();
            Assert.IsTrue(iconSprites.Length == names.Length,
                "The number if sprites supplied must match the number of names supplied.");

            icons = new Sprite[iconSprites.Length];
            Array.Copy(iconSprites, icons, iconSprites.Length);
            this.names = names;
        }
        #endregion

        #region "Optional configuration methods"
        /// <summary>
        /// Configures the visual aesthetics of the annular sectors in the radial menu.
        /// </summary>
        /// <param name="iconSize">A <see cref="float"/> representing the fractional size of the icons.
        ///  Range: [0, 1]. Default is 0.75f.</param>
        /// <param name="innerRadiusFraction">A <see cref="float"/> defining the relative size of the
        ///  inner cut-out. Range: [0, 1). Default is 0.6f.</param>
        /// <param name="padding">A <see cref="float"/> determining the degree of separation between
        ///  each sector. Very small values are recommended. Default is 0.05f.</param>
        /// <param name="resolution">An <see cref="int"/> setting the height and width of a full radial
        ///  menu in pixels. Larger values recommended for higher resolution headsets. Default is 1000.</param>
        /// <param name="blurWidth">An <see cref="int"/> indicating the width of the convolution window
        ///  used during blurring. Larger values intensify smoothing effects. Default is 3.</param>
        /// <param name="numberOfBlurPasses">An <see cref="int"/> determining the number of blurring passes
        ///  to perform. Default is 3.</param>
        /// <param name="margin">An <see cref="int"/> defining the texture margin size to prevent visual
        ///  artefacts. Default is 5.</param>
        /// <remarks>
        /// The `ConfigureSectorAesthetics` method is utilised to set various visual parameters of the
        ///  annular sectors that constitute the radial menu.
        /// </remarks>
        public void ConfigureSectorAesthetics(
            float iconSize = 0.75f, float innerRadiusFraction = 0.6f, float padding = 0.05f,
            int resolution = 1000, int blurWidth = 3, int numberOfBlurPasses = 3, int margin = 5
        )
        {
            EnsureNotFinalised();
            this.iconSize = iconSize;
            this.innerRadiusFraction = innerRadiusFraction;
            this.padding = padding;
            this.resolution = resolution;
            this.blurWidth = blurWidth;
            this.numberOfBlurPasses = numberOfBlurPasses;
            this.margin = margin;
        }

        /// <summary>
        /// Configures the behaviour upon focusing a sector, including sound and haptic feedback.
        /// </summary>
        /// <param name="hapticAmplitude">A <see cref="float"/> representing the amplitude of haptic
        ///  response sent when focusing a new sector. Default is 0.5f.</param>
        /// <param name="hapticDuration">A <see cref="float"/> indicating the duration of the haptic
        ///  response sent upon focusing a new sector. Default is 0.05f.</param>
        /// <param name="focusSoundFile">A <see cref="string"/> indicating the file path to the sound
        ///  played upon focusing a new sector. Default is "UI/Sounds/click".</param>
        /// <remarks>
        /// When a user navigates through the radial menu, audible and haptic feedback provides
        ///  tactile confirmation upon the focusing of a new sector.
        /// </remarks>
        public void ConfigureFocusBehaviour(float hapticAmplitude = 0.5f, float hapticDuration = 0.05f, 
            string focusSoundFile = "UI/Sounds/click") => ConfigureFocusBehaviour(
            hapticAmplitude, hapticDuration, Resources.Load<AudioClip>(focusSoundFile));


        /// <summary>
        /// Configures the behaviour upon focusing a sector, including sound and haptic feedback.
        /// </summary>
        /// <param name="hapticAmplitude">A <see cref="float"/> representing the amplitude of haptic
        ///  response sent when focusing a new sector. Default is 0.5f.</param>
        /// <param name="hapticDuration">A <see cref="float"/> indicating the duration of the haptic
        ///  response sent upon focusing a new sector. Default is 0.05f.</param>
        /// <param name="focusSound">A <see cref="AudioClip"/> Audio clip to be played upon focusing a
        /// new sector. This will default to that specified by "UI/Sounds/click.mp3".</param>
        /// <remarks>
        /// When a user navigates through the radial menu, audible and haptic feedback provides
        ///  tactile confirmation upon the focusing of a new sector.
        /// </remarks>
        public void ConfigureFocusBehaviour(
            float hapticAmplitude = 0.5f, float hapticDuration = 0.05f,
            AudioClip focusSound = null)
        {
            EnsureNotFinalised();
            if (focusSound == null)
                focusSound = Resources.Load<AudioClip>("UI/Sounds/click");

            this.focusSound = focusSound;
            this.hapticAmplitude = hapticAmplitude;
            this.hapticDuration = hapticDuration;
        }



        /// <summary>
        /// Configures the colours of various visual components within the radial menu.
        /// </summary>
        /// <param name="focusedColour">A <see cref="Color"/> to apply to the focused sector sprite.
        ///  Default is a predefined blue colour.</param>
        /// <param name="baseColour">A <see cref="Color"/> to apply as the default colour of each
        ///  sector sprite. Default is a predefined grey colour.</param>
        /// <param name="iconColour">A <see cref="Color"/> to apply to the icon sprites. Default is
        ///  a predefined dark grey colour.</param>
        /// <remarks>
        /// The `ConfigureColours` method is utilised to set the colours of the sector, icon and focused-sector of the radial menu.
        /// </remarks>
        public void ConfigureColours(Color? focusedColour = null, Color? baseColour = null, Color? iconColour = null)
        {
            EnsureNotFinalised();
            this.focusedColour = focusedColour ?? new Color(0.188f, 0.753f, 1f, 1f);
            this.baseColour = baseColour ?? new Color(0.878f, 0.878f, 0.878f, 1f);
            this.iconColour = iconColour ?? new Color(0.251f, 0.251f, 0.251f, 1f);
        }

        /// <summary>
        /// Configures the font settings for text displayed within the radial menu.
        /// </summary>
        /// <param name="fontSize">A <see cref="float"/> determining the size of the font. Default
        ///  is 0.15f.</param>
        /// <param name="fontColour">An optional <see cref="Color"/> to specify the colour of the
        ///  font. Default is <see cref="Color.white"/>.</param>
        /// <remarks>
        /// The `ConfigureFont` method is used to establish visual properties of the text within the
        ///  radial menu, particularly, its size and colour.
        /// </remarks>
        public void ConfigureFont(float fontSize = 0.15f, Color? fontColour = null)
        {
            EnsureNotFinalised();
            this.fontSize = fontSize;
            this.fontColour = fontColour ?? Color.white;
        }
        #endregion

        /// <summary>
        /// Finalises the radial menu configuration and pre-fetches necessary components to optimise runtime performance.
        /// </summary>
        /// <remarks>
        /// The `Finalise` method performs several setup and optimisation steps to prepare the radial menu for use:
        /// - Pre-fetches and caches relevant components for efficient runtime usage.
        /// - Allocates arrays to store game objects and their components for the annular sectors.
        /// - Ensures blurring width (if used) adheres to requirements.
        /// - Checks for cached sprite versions or generates new ones as required.
        /// - Constructs an audio source component for sound playback.
        /// - Loads specified focus sound files.
        /// - Generates the text box for menu and option name display.
        /// - Generates the radial components of the menu.
        /// - Subscribes to relevant input actions for menu show/hide functionality.
        /// 
        /// Once finalised, the menu is ready to be displayed and interacted with by the user.
        /// Note: Further configuration attempts post-finalisation will throw an <see cref="InvalidOperationException"/>.
        /// </remarks>
        public void Finalise()
        {
            EnsureNotFinalised();

            // As calls to the `GetComponent` method are reasonably expensive the `RectTransform` and
            // `Canvas` components are prefetched now and cached for later use.
            canvasSizeDelta = GetComponent<RectTransform>().sizeDelta;
            canvas = GetComponent<Canvas>();

            // Allocate arrays to store the annular sector `GameObject`s and their `Image` components.
            AnnularSectorGameObjects = new GameObject[NumberOfSectors];
            AnnularSectorImages = new Image[NumberOfSectors];

            // If blurring is enabled (`blurWidth` > 0) then ensure that the blurring width is set to
            // an odd number. This is so that there is a central pixel for the convolution window.
            if (blurWidth != 0 && blurWidth % 2 == 0) blurWidth++;

            // Check if a cached version of this sprite is already stored on the disk. Generating the
            // sprite from scratch is more costly than one would like, so caching can aid performance.
            sprite = GenerateAnnularSectorSpriteWithCache();

            // Construct an audio source component to allow for sound files to be played
            audioSource = gameObject.AddComponent<AudioSource>();
            
            // Load in the focus sound file
            if (focusSound == null)
            {
                focusSound = Resources.Load<AudioClip>("UI/Sounds/click");
            }

            // Generate text box to display menu and option names
            GenerateText();

            // Generate the radial components
            GenerateRadials();

            canvas.enabled = false;
            textObject.SetActive(false);

            // Subscribe to the show/hide menu button's InputAction.
            SetupSubscriptions();

            finalised = true;

            HideMenu();
        }

        private void ShowMenu(InputAction.CallbackContext context) => ShowMenu();
        private void HideMenu(InputAction.CallbackContext context) => HideMenu();

        /// <summary>
        /// Activate and display the menu.
        /// </summary>
        /// <remarks>
        /// This is called whenever the `showRadialMenuAction` button is depressed.
        /// </remarks>
        private void ShowMenu()
        {

            // When the radial menu is (re)enabled the position and orientation of the menu must be
            // updated to match the controllers current position and rotation.
            if (controllerTransform != null)
            {
                transform.position = controllerTransform.position;
                transform.rotation = controllerTransform.rotation;
            }

            // Show each of the sectors
            foreach (var sector in AnnularSectorGameObjects)
                sector.SetActive(true);

            // Show the canvas and text
            canvas.enabled = true;
            textObject.SetActive(true);

        }

        /// <summary>
        /// Hide the menu and trigger an `OptionSelected` event if an option was selected.
        /// </summary>
        /// <remarks>
        /// This is called whenever the `showRadialMenuAction` button is released.
        /// </remarks>
        private void HideMenu()
        {

            // Hide each of the sectors
            foreach (var sector in AnnularSectorGameObjects)
                sector.SetActive(false);

            // Show the canvas and text
            canvas.enabled = false;
            textObject.SetActive(false);

            // If a sector was focused when the user released the button then invoke the
            // `OptionSelected` event and clear the focus.
            if (focusedSectorIndex != -1)
            {

                if (OptionSelected != null)
                {
                    OptionSelected(focusedSectorIndex);
                }

                UnfocusSector(focusedSectorIndex);
            }
        }

        /// <summary>
        /// Subscribe the `ShowMenu` & `HideMenu` methods to the `.performed` & `.cancelled` events of
        /// the `showRadialMenuAction` button.
        /// </summary>
        private void SetupSubscriptions()
        {
            showRadialMenuAction.performed += ShowMenu;
            showRadialMenuAction.canceled += HideMenu;
            subscribed = true;
        }

        /// <summary>
        /// Clear subscriptions of the `ShowMenu` & `HideMenu` methods to the `.performed` & `.cancelled`
        /// events of the `showRadialMenuAction` button.
        /// </summary>
        private void TeardownSubscriptions()
        {
            showRadialMenuAction.performed -= ShowMenu;
            showRadialMenuAction.canceled -= HideMenu;
            subscribed = false;
        }


        public void OnEnable()
        {
            if (finalised)
            {
                // The canvas and text objects are disabled here as they should only be enabled
                // when the `ShowMenu` button is depressed.
                HideMenu();

                if (!subscribed) { SetupSubscriptions(); }
            }

        }

        public void OnDisable()
        {
            if (subscribed) { TeardownSubscriptions(); }
        }

        public void Update()
        {
            
            if (finalised && canvas.enabled)
            {

                (float r, float phi) = ControllerPolarPositionOnCanvas();


                // Check to see if the controller lies outside of the internal inner radius. If so then an annular
                // sector will need to be focused.
                if (r >= innerRadiusFraction * canvasSizeDelta[0] * 0.5)
                {
                    
                    // Identify which sector should be focused based on the polar angle of the controller.
                    int selectionIndex = FocusedSectorIndex(phi);

                    // If this sector is not already focused
                    if (selectionIndex != focusedSectorIndex)
                    {
                        
                        // Remove focusing from any already focused sector
                        if (focusedSectorIndex != -1)
                        {
                            UnfocusSector(focusedSectorIndex);
                        }

                        // Then focus it
                        FocusSector(selectionIndex);

                        // Finally, update selection focus index.
                        focusedSectorIndex = selectionIndex;
                    }

                }
                // If no sector is currently being focused then check for and clear any focusing
                // formatting and related variables.
                else if (focusedSectorIndex != -1)
                {
                    // Clear focusing
                    UnfocusSector(focusedSectorIndex);

                }
            }
        }

        /// <summary>
        /// Cartesian coordinates of controller's position projected onto the plane of the menu's canvas.
        /// </summary>
        /// <returns>Cartesian position of the controller on the surface of the menu panel.</returns>
        private Vector2 ControllerPositionOnCanvas() => (Vector2)canvas.transform.InverseTransformPoint(controllerTransform.position);


        /// <summary>
        /// Radial and polar coordinates of controller's position projected onto the plain of the menu's canvas.
        /// </summary>
        /// <returns>radial and polar coordinates</returns>
        private (float r, float phi) ControllerPolarPositionOnCanvas()
        {
            float r, phi;

            Vector2 pos = ControllerPositionOnCanvas();

            r = pos.magnitude;
            phi = Mathf.Atan2(pos.y, pos.x);
            if (phi < 0)
            {
                phi += 2 * Mathf.PI;
            }

            return (r, phi);

        }

        /// <summary>
        /// Return the index of the annular sector that is currently being focused by the user.
        /// </summary>
        /// <param name="phi">Polar angle specifying the location of the controller in the canvas.</param>
        /// <returns>Index of currently focused annular sector</returns>
        private int FocusedSectorIndex(in float phi)
        {

            float halfTheta = Mathf.PI / NumberOfSectors;

            // A 90° offset is applied to account for the 1'st sector lying at the 12 o'clock position,
            // rather than the 3 o'clock position. A second offset of ½θ is needed as 0° represents the
            // *midpoint* of the 1'st sector not its start.
            float phiOffset = Mathf.Repeat(phi - Mathf.PI * 0.5f + halfTheta, 2f * Mathf.PI);

            return Mathf.FloorToInt(phiOffset / (2f * halfTheta));
        }

        /// <summary>
        /// Focus a particular sector to indicate to the user which option is currently selected.
        /// </summary>
        /// <param name="sectorIndex">Index of sector to be focused</param>
        private void FocusSector(int sectorIndex)
        {

            // The colour of the targeted sector is change to indicate that this is the sector which
            // will be selected when the menu is actioned.
            AnnularSectorImages[sectorIndex].color = focusedColour;

            // A sound and haptic impulse are used as non visual signals to telegraph to the user
            // that a new sector has been focused.
            if (focusSound != null)
            {
                audioSource.clip = focusSound;
                audioSource.Play();
            }

            // If the haptic amplitude is not zero, then send a haptic response to the target device
            if (hapticAmplitude != 0f) inputDevice.SendHapticImpulse(0, hapticAmplitude, hapticDuration);
            
            // Update the text shown in the centre to show the name of the focused option
            text.text = names[sectorIndex];

        }

        /// <summary>
        /// Remove focus formatting from a specific sector.
        /// </summary>
        /// <param name="sectorIndex">Index of sector to clear focus on</param>
        private void UnfocusSector(int sectorIndex)
        {
            AnnularSectorImages[sectorIndex].color = baseColour;
            focusedSectorIndex = -1;
            text.text = MenuName;
        }

        /// <summary>
        /// Width and height of the smallest box that would bound the annular sector assuming unit circle.
        /// </summary>
        /// <remarks>
        /// Note that this does not factor in the effects of padding on image size.
        /// </remarks>
        /// <returns>Width and height of annular sector.</returns>
        private (float width, float height) RelativeSize()
        {
            float width, height;

            // Catch for special case in which only one menu item is present and a full
            // annulus must be generated.
            if (NumberOfSectors == 1)
            {
                width = height = 2.0f;
            }
            else
            {
                float halfTheta = Mathf.PI / NumberOfSectors;
                width = 2f * Mathf.Sin(halfTheta);
                height = 1f - Mathf.Cos(halfTheta) * innerRadiusFraction;
            }

            return (width, height);
        }


        /// <summary>
        /// Create the visual components of the radial menu.
        /// </summary>
        private void GenerateRadials()
        {
            // Size and height of annular sectors
            (float width, float height) = RelativeSize();

            // Scale correction factors for the annular sector sprites to account for distortions
            // caused by the added margin.
            float ScaleFactor = margin * 2f / (float)resolution;
            float widthCorrected = (width / 2f + ScaleFactor);
            float heightCorrected = (height / 2f + ScaleFactor);


            // Location and size of the icons
            float scaledIconSize = (1f - innerRadiusFraction) * iconSize * canvasSizeDelta[1] * 0.5f;
            float iconLocation = (height - 1f + innerRadiusFraction) * canvasSizeDelta[1] / 4f;

            float theta = (2 * Mathf.PI) / NumberOfSectors;
            for (int i = 0; i < NumberOfSectors; i++)
            {

                float angle = theta * i;

                // Create the UI panel game object that will hold the annular sector and set up its
                // rectangular transform.
                GameObject sectorPanel = new GameObject($"RadialPanel_{i + 1}");
                AnnularSectorGameObjects[i] = sectorPanel;
                RectTransform sectorRect = sectorPanel.AddComponent<RectTransform>();

                // Set the size of annular sector sprite
                sectorRect.sizeDelta = new Vector2(widthCorrected, heightCorrected) * canvasSizeDelta;

                // Set the coordinates of the current sector's midpoint. An offset of 90° is applied
                // so that the 1st sector lies at the 12 o'clock position. An additional offset is
                // also required to push the sprite to the edge of the bounding box. A scaling factor
                // of 1.01 is applied to ensure pixel perfect overlap of sections when a padding value
                // of zero is used.
                sectorRect.anchoredPosition = new Vector2(
                    Mathf.Sin(angle) * (height * 1.01f - 2f),
                    Mathf.Cos(angle) * (2f - height * 1.01f)
                ) * 0.25f * canvasSizeDelta[0];

                // Rotate the sector so that if forms a ring with the others.
                sectorRect.eulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * angle);

                sectorRect.SetParent(transform, false);

                // Add in the sectors associated icon.
                Image panelImage = sectorPanel.AddComponent<Image>();
                AnnularSectorImages[i] = panelImage;
                panelImage.color = baseColour;
                panelImage.sprite = sprite;

                // Build and configure the icon for each sector
                GameObject iconPanel = new GameObject($"RadialIconPanel_{i + 1}");
                RectTransform iconRect = iconPanel.AddComponent<RectTransform>();

                iconRect.sizeDelta = new Vector2(scaledIconSize, scaledIconSize);
                iconRect.anchoredPosition = new Vector2(0f, iconLocation);
                iconRect.SetParent(sectorRect, false);

                // Rotate the icon so it is correctly orientated. 
                iconRect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Rad2Deg * -angle);

                Image iconImage = iconPanel.AddComponent<Image>();
                iconImage.color = iconColour;
                iconImage.sprite = icons[i];
            }
        }


        /// <summary>
        /// Generate the annular sector sprite procedurally.
        /// </summary>
        /// <returns>Annular sector sprite</returns>
        private Sprite GenerateAnnularSectorSprite()
        {
            float halfTheta = Mathf.PI / NumberOfSectors;

            (float width, float height) = RelativeSize();

            // Calculate the size of the sprite in pixels that is required to hold the annular sector.
            // Note that a margin is added on each side of the image to prevent i) image wrapping
            // induced colour bleed, and ii) hard edges created due to blurring (applied later) getting
            // truncated when it reaches the edge of the image.
            int pixelHeight = (2 * margin) + (int)(Mathf.Ceil(resolution * height / 2f));
            int pixelWidth = (2 * margin) + (int)(Mathf.Ceil(resolution * width / 2f));

            // Construct a texture to hold the image
            Texture2D texture = new Texture2D(pixelWidth, pixelHeight)
            {
                filterMode = FilterMode.Point
            };

            // Array to store the colour values. Image is set transparent white at the start.
            Color[] colours = Enumerable.Repeat(new Color(1f, 1f, 1f, 0f), pixelWidth * pixelHeight).ToArray();

            // Precompute constants to be use within the colour assignment loop
            float offset = padding / Mathf.Sin(halfTheta);
            float innerRadiusFractionSquared = innerRadiusFraction * innerRadiusFraction;
            float yBase = 1f - height + 2f * (-margin) / resolution;
            float xBase = -width / 2f + 2f * (-margin) / resolution;

            for (int y = 0; y < pixelHeight; y++)
            {

                // Convert from pixel coordinates to real space coordinates i.e. [-1. +1]
                float yl = yBase + 2f * y / resolution;

                float yl2 = yl * yl;

                for (int x = 0; x < pixelWidth; x++)
                {
                    float xl = xBase + 2f * x / resolution;

                    // Calculate the polar coordinates. Distance is squared to avoid having to compute
                    // the square root. The offset is applied to the y-axis to introduce the desired
                    // padding between sectors.
                    float r2 = xl * xl + yl2;
                    float psi = Mathf.Atan2(xl, yl - offset);

                    // If the pixel lies within the annular sector then it is coloured white, otherwise
                    // it is left transparent.
                    if ((-halfTheta) <= psi && psi <= halfTheta &&
                        r2 >= innerRadiusFractionSquared && r2 <= 1)
                    {
                        colours[x + (y * pixelWidth)] = Color.white;
                    }
                }
            }

            // Perform the requisite number of blurring passes to help mitigate aliasing artefacts.
            if (blurWidth != 0)
            {
                for (int i = 0; i < numberOfBlurPasses; i++)
                {
                    colours = TextureBlurrer.BlurTexture(colours, pixelWidth, pixelHeight, blurWidth);
                }
            }


            // Update the texture's colour values 
            texture.SetPixels(colours);
            texture.Apply();

            // Convert the texture into a sprite and return it.
            return Sprite.Create(texture, new Rect(0, 0, pixelWidth, pixelHeight), new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// Generate the annular sector sprite procedurally with caching.
        /// </summary>
        /// <remarks>
        /// As procedurally generating the sprite for the annular sector is a computationally demanding
        /// process it is best to cache sprites where possible. This function will test for the existence
        /// of a annular sector sprite matching the required description. If one is found then it will be
        /// loaded from the disk. If not, then a new one will be generated via `GenerateAnnularSectorSprite`,
        /// cached, and then returned.
        /// </remarks>
        /// <returns>Annular sector sprite</returns>
        private Sprite GenerateAnnularSectorSpriteWithCache()
        {
            // Compute the sprite hash
            string hash = ComputeSpriteHash();

            // Check if a cached version of this sprite is already stored on the disk. Generating the
            // sprite from scratch is more costly than one would like, so caching can aid performance.
            Sprite localSprite;
            if (SpriteFileExists(hash))
            {
                // If so load the cached sprite
                localSprite = LoadSpriteFromDisk(hash);
            }
            else
            {
                // Construct the annular sector sprite, and cache it to the disk.
                localSprite = GenerateAnnularSectorSprite();
                SaveSpriteToDisk(localSprite, hash);
            }
            return localSprite;
        }

        /// <summary>
        /// Generate and configure the text box.
        /// </summary>
        /// <remarks>
        /// This text box is located within the cut-out of the radial menu and is intended to display
        /// either the menu name or the focused option (if one is currently focused).
        /// </remarks>
        private void GenerateText()
        {
            // Create the text mesh pro entity that will be used to display the name of the focused
            // option to the user.
            textObject = new GameObject("RadialMenuInfoText");

            textObject.transform.SetParent(transform);
            text = textObject.AddComponent<TextMeshPro>();

            RectTransform textRect = textObject.GetComponent<RectTransform>();

            textRect.localRotation = Quaternion.identity;
            textRect.localPosition = Vector3.zero;
            textRect.localScale = Vector3.one;
            textRect.sizeDelta = canvasSizeDelta * innerRadiusFraction;

            text.fontSize = fontSize;
            text.text = MenuName;
            text.alignment = TextAlignmentOptions.Center;
            text.color = fontColour;

        }

        /// <summary>
        /// Generate the canvas within which all sub-frames will be placed.
        /// </summary>
        /// <param name="scale">Size of the canvas (length and height)</param>
        private void GenerateCanvas(float scale)
        {
            // Add and setup the user interface canvas component 
            canvas = gameObject.AddComponent<Canvas>();
            gameObject.AddComponent<CanvasScaler>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;

            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.localScale = Vector3.one;

            // Set the size, position and orientation of the radial menu's canvas. The position and
            // orientation are dictated by the controller.
            rectTransform.sizeDelta = new Vector2(scale, scale);

            rectTransform.position = controllerTransform.position;
            rectTransform.rotation = controllerTransform.rotation;
        }

        /// <summary>
        /// Compute a hash to represent the annular sector.
        /// </summary>
        /// <returns>The hash value as a string</returns>
        /// <remarks>
        /// The hash name of an annular sector is dictated by the fields that define it.
        /// </remarks>
        private string ComputeSpriteHash()
        {
            string compositeKey = $"{NumberOfSectors}_{innerRadiusFraction}_{padding}_{resolution}_{blurWidth}_{numberOfBlurPasses}";
            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.Default.GetBytes(compositeKey));
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Checks if a cached sprite exists based on its hash-name.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns>Boolean indicating if a cached sprite file exists.</returns>
        private bool SpriteFileExists(string hash)
        {
            if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "SpritesCache")))
            {
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "SpritesCache"));
            }
            string path = Path.Combine(Application.persistentDataPath, "SpritesCache", hash + ".png");

            return File.Exists(path);
        }

        /// <summary>
        /// Load cached sprite file from disk.
        /// </summary>
        /// <param name="hash">Hash name of the cached sprite file.</param>
        /// <returns>Annular sector sprite</returns>
        /// <remarks>
        /// The name of the sprite file is set to the hash of the fields that define it.
        /// </remarks>
        private Sprite LoadSpriteFromDisk(string hash)
        {
            string path = Path.Combine(Application.persistentDataPath, "SpritesCache", hash + ".png");
            if (File.Exists(path))
            {
                byte[] bytes = File.ReadAllBytes(path);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(bytes);
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
            return null;
        }


        /// <summary>
        /// Save a cache of the sprite to the disk for use later on.
        /// </summary>
        /// <param name="sprite">Sprite object to be cached</param>
        /// <param name="hash">Hash name of the file, excluding extension.</param>
        private void SaveSpriteToDisk(Sprite sprite, string hash)
        {
            Texture2D texture = sprite.texture;
            byte[] bytes = texture.EncodeToPNG();
            string path = Path.Combine(Application.persistentDataPath, "SpritesCache", hash + ".png");
            File.WriteAllBytes(path, bytes);
        }

        /// <summary>
        /// Safety function that is used to ensure that users do not try to reconfigure settings after finalisation.
        /// </summary>
        private void EnsureNotFinalised()
        {
            if (finalised)
            {
                Debug.LogError("Cannot modify a finalised `RadialMenu` instance.");
            }
        }


        // While this method for blurring sprites leaves much to be desired it performs well enough.
        // It would be a waste of time to implement anything better unless there is an explicit need
        // for it elsewhere. Documentation and comments have also been omitted as this code should
        // only ever need to be touched by other developers if being completely replaced.
        private static class TextureBlurrer
        {
            public static Color[] BlurTexture(Color[] sourceColours, int width, int height, int windowSize = 3)
            {
                if (windowSize % 2 == 0) windowSize++;

                int offset = windowSize / 2;
                Color[] intermediateColours = BlurDirection(sourceColours, width, height, offset, true);
                Color[] finalColours = BlurDirection(intermediateColours, width, height, offset, false);

                return finalColours;
            }

            private static Color[] BlurDirection(Color[] sourceColours, int width, int height, int offset, bool isHorizontal)
            {
                Color[] blurredColours = new Color[width * height];

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        blurredColours[x + y * width] = ComputeAverageColour(sourceColours, x, y, width, height, offset, isHorizontal);
                    }
                }

                return blurredColours;
            }

            private static Color ComputeAverageColour(Color[] sourceColours, int x, int y, int width, int height, int offset, bool isHorizontal)
            {
                Color avgColour = Color.clear;
                int validNeighborCount = 0;

                for (int k = -offset; k <= offset; k++)
                {
                    int neighborX = x + (isHorizontal ? k : 0);
                    int neighborY = y + (isHorizontal ? 0 : k);

                    if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height)
                    {
                        avgColour += sourceColours[neighborX + neighborY * width];
                        validNeighborCount++;
                    }
                }

                return avgColour / validNeighborCount;
            }
        }

    }
}