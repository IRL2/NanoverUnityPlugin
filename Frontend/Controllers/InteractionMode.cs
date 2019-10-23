// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using UnityEngine;

namespace Narupa.Frontend.Controllers
{
    /// <summary>
    /// Define the mode of the controller as 'interaction', setting up the correct
    /// gizmo.
    /// </summary>
    /// <remarks>
    /// This is a temporary object until a more sophisticated multi-mode tool setup is
    /// completed.
    /// </remarks>
    public class InteractionMode : MonoBehaviour
    {
        [SerializeField]
        private ControllerManager controller;

        private void Awake()
        {
            controller.LeftController.ControllerReset += () =>
            {
                SetupController(controller.LeftController);
            };
            controller.RightController.ControllerReset += () =>
            {
                SetupController(controller.RightController);
            };
        }

        [SerializeField]
        private GameObject interactionGizmo;

        private void SetupController(VrController controller)
        {
            if (controller.IsControllerActive)
                controller.Cursor.SetGizmo(Instantiate(interactionGizmo,
                                                       controller.Cursor.transform));
        }
    }
}