using SteamVRStub;
using UnityEngine;
using UnityEngine.XR;

namespace Nanover.Frontend.Controllers
{
    public class InteractionInputMode : ControllerInputMode
    {
#pragma warning disable 0649
        [SerializeField]
        private GameObject gizmo;

        [SerializeField]
        private SteamVR_ActionSet[] actionSets;

        [SerializeField]
        private int priority;
#pragma warning restore 0649

        public override int Priority => priority;

        public override void OnModeStarted()
        {
            foreach (var actionSet in actionSets)
                actionSet.Activate();
        }

        public override void OnModeEnded()
        {
            foreach (var actionSet in actionSets)
                actionSet.Deactivate();
        }

        public override void SetupController(VrController controller,
                                             InputDeviceCharacteristics inputSource)
        {
            if (controller.IsControllerActive)
                controller.InstantiateCursorGizmo(gizmo);
        }
    }
}