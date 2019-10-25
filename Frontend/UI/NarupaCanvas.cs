using Narupa.Frontend.Controllers;
using Narupa.Frontend.XR;
using UnityEngine;
using UnityEngine.Assertions;
using Valve.VR;

namespace Narupa.Frontend.UI
{
    /// <summary>
    /// Component required to register a Unity canvas such that the given controller can interact with it.
    /// </summary>
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