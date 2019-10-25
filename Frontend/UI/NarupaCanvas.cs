using Narupa.Frontend.Controllers;
using Narupa.Frontend.Input;
using Narupa.Frontend.XR;
using UnityEngine;
using UnityEngine.Assertions;
using Valve.VR;

namespace Narupa.Frontend.UI
{
    /// <summary>
    /// Component required to register a Unity canvas such that the given controller
    /// can interact with it.
    /// </summary>
    /// <remarks>
    /// All canvases that would like to be interacted with by physical controllers
    /// should have a script that derives from <see cref="NarupaCanvas" />. This should
    /// provide the controller and the action which is counted as a 'click'. The
    /// <see cref="RegisterCanvas" /> method can be overriden to provide a custom
    /// <see cref="IPosedObject" /> and <see cref="IButton" /> to provide the cursor
    /// location and click button.
    /// </remarks>
    [RequireComponent(typeof(Canvas))]
    public class NarupaCanvas : MonoBehaviour
    {
        [SerializeField]
        private VrController controller;

        [SerializeField]
        private SteamVR_Action_Boolean inputAction;

        [SerializeField]
        private SteamVR_Input_Sources inputSource;

        private Canvas canvas;

        private void Awake()
        {
            Assert.IsNotNull(controller);
            canvas = GetComponent<Canvas>();
        }

        private void Start()
        {
            RegisterCanvas();
        }

        /// <summary>
        /// Register the canvas with the cursor input system.
        /// </summary>
        protected virtual void RegisterCanvas()
        {
            Assert.IsNotNull(WorldSpaceCursorInput.Instance);
            WorldSpaceCursorInput.Instance.RegisterCanvas(canvas,
                                                          controller.HeadPose,
                                                          inputAction.WrapAsButton(inputSource));
        }
    }
}